// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP client implementation. See RFC 7230 through 7235.
//
// This is the low-level Transport implementation of RoundTripper.
// The high-level interface is in client.go.
namespace go.net;

using bufio = bufio_package;
using gzip = compress.gzip_package;
using list = container.list_package;
using context = context_package;
using tls = crypto.tls_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using log = log_package;
using maps = maps_package;
using net = net_package;
using httptrace = go.net.http.httptrace_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using httpproxy = vendor.golang.org.x.net.http.httpproxy_package;
using compress;
using container;
using crypto;
using go.@internal;
using go.net;
using go.net.http;
using go.net.http.@internal;
using go.sync;
using vendor.golang.org.x.net.http;
using ꓸꓸꓸany = Span<any>;

partial class http_package {

// DefaultTransport is the default implementation of [Transport] and is
// used by [DefaultClient]. It establishes network connections as needed
// and caches them for reuse by subsequent calls. It uses HTTP proxies
// as directed by the environment variables HTTP_PROXY, HTTPS_PROXY
// and NO_PROXY (or the lowercase versions thereof).
public static RoundTripper DefaultTransport;
internal static void initᴛDefaultTransport() { DefaultTransport = new TransportжRoundTripper(Ꮡ(new Transport(
    Proxy: ProxyFromEnvironment,
    DialContext: defaultTransportDialContext(Ꮡ(new net.Dialer(
        Timeout: (time.Duration)(30000000000L),
        KeepAlive: (time.Duration)(30000000000L)
    ))),
    ForceAttemptHTTP2: true,
    MaxIdleConns: 100,
    IdleConnTimeout: (time.Duration)(90000000000L),
    TLSHandshakeTimeout: (time.Duration)(10000000000L),
    ExpectContinueTimeout: 1 * time.ΔSecond
))); }

// DefaultMaxIdleConnsPerHost is the default value of [Transport]'s
// MaxIdleConnsPerHost.
public static UntypedInt DefaultMaxIdleConnsPerHost => 2;

// Transport is an implementation of [RoundTripper] that supports HTTP,
// HTTPS, and HTTP proxies (for either HTTP or HTTPS with CONNECT).
//
// By default, Transport caches connections for future re-use.
// This may leave many open connections when accessing many hosts.
// This behavior can be managed using [Transport.CloseIdleConnections] method
// and the [Transport.MaxIdleConnsPerHost] and [Transport.DisableKeepAlives] fields.
//
// Transports should be reused instead of created as needed.
// Transports are safe for concurrent use by multiple goroutines.
//
// A Transport is a low-level primitive for making HTTP and HTTPS requests.
// For high-level functionality, such as cookies and redirects, see [Client].
//
// Transport uses HTTP/1.1 for HTTP URLs and either HTTP/1.1 or HTTP/2
// for HTTPS URLs, depending on whether the server supports HTTP/2,
// and how the Transport is configured. The [DefaultTransport] supports HTTP/2.
// To explicitly enable HTTP/2 on a transport, set [Transport.Protocols].
//
// Responses with status codes in the 1xx range are either handled
// automatically (100 expect-continue) or ignored. The one
// exception is HTTP status code 101 (Switching Protocols), which is
// considered a terminal status and returned by [Transport.RoundTrip]. To see the
// ignored 1xx responses, use the httptrace trace package's
// ClientTrace.Got1xxResponse.
//
// Transport only retries a request upon encountering a network error
// if the connection has been already been used successfully and if the
// request is idempotent and either has no body or has its [Request.GetBody]
// defined. HTTP requests are considered idempotent if they have HTTP methods
// GET, HEAD, OPTIONS, or TRACE; or if their [Header] map contains an
// "Idempotency-Key" or "X-Idempotency-Key" entry. If the idempotency key
// value is a zero-length slice, the request is treated as idempotent but the
// header is not sent on the wire.
[GoType] partial struct Transport {
    internal sync.Mutex idleMu;
    internal bool closeIdle;                                // user has requested to close all idle conns
    internal map<connectMethodKey, slice<ж<persistConn>>> idleConn; // most recently used at end
    internal map<connectMethodKey, wantConnQueue> idleConnWait; // waiting getConns
    internal connLRU idleLRU;
    internal sync.Mutex reqMu;
    internal map<ж<Request>, Action<error>> reqCanceler;
    internal sync.Mutex altMu;   // guards changing altProto only
    internal atomic.Value altProto; // of nil or map[string]RoundTripper, key is URI scheme
    internal sync.Mutex connsPerHostMu;
    internal map<connectMethodKey, nint> connsPerHost;
    internal map<connectMethodKey, wantConnQueue> connsPerHostWait; // waiting getConns
    internal wantConnQueue dialsInProgress;
    // Proxy specifies a function to return a proxy for a given
    // Request. If the function returns a non-nil error, the
    // request is aborted with the provided error.
    //
    // The proxy type is determined by the URL scheme. "http",
    // "https", "socks5", and "socks5h" are supported. If the scheme is empty,
    // "http" is assumed.
    // "socks5" is treated the same as "socks5h".
    //
    // If the proxy URL contains a userinfo subcomponent,
    // the proxy request will pass the username and password
    // in a Proxy-Authorization header.
    //
    // If Proxy is nil or returns a nil *URL, no proxy is used.
    public Func<ж<Request>, (ж<url.URL>, error)> Proxy;
    // OnProxyConnectResponse is called when the Transport gets an HTTP response from
    // a proxy for a CONNECT request. It's called before the check for a 200 OK response.
    // If it returns an error, the request fails with that error.
    public Func<context.Context, ж<url.URL>, ж<Request>, ж<Response>, error> OnProxyConnectResponse;
    // DialContext specifies the dial function for creating unencrypted TCP connections.
    // If DialContext is nil (and the deprecated Dial below is also nil),
    // then the transport dials using package net.
    //
    // DialContext runs concurrently with calls to RoundTrip.
    // A RoundTrip call that initiates a dial may end up using
    // a connection dialed previously when the earlier connection
    // becomes idle before the later DialContext completes.
    public Func<context.Context, @string, @string, (net.Conn, error)> DialContext;
    // Dial specifies the dial function for creating unencrypted TCP connections.
    //
    // Dial runs concurrently with calls to RoundTrip.
    // A RoundTrip call that initiates a dial may end up using
    // a connection dialed previously when the earlier connection
    // becomes idle before the later Dial completes.
    //
    // Deprecated: Use DialContext instead, which allows the transport
    // to cancel dials as soon as they are no longer needed.
    // If both are set, DialContext takes priority.
    public Func<@string, @string, (net.Conn, error)> Dial;
    // DialTLSContext specifies an optional dial function for creating
    // TLS connections for non-proxied HTTPS requests.
    //
    // If DialTLSContext is nil (and the deprecated DialTLS below is also nil),
    // DialContext and TLSClientConfig are used.
    //
    // If DialTLSContext is set, the Dial and DialContext hooks are not used for HTTPS
    // requests and the TLSClientConfig and TLSHandshakeTimeout
    // are ignored. The returned net.Conn is assumed to already be
    // past the TLS handshake.
    public Func<context.Context, @string, @string, (net.Conn, error)> DialTLSContext;
    // DialTLS specifies an optional dial function for creating
    // TLS connections for non-proxied HTTPS requests.
    //
    // Deprecated: Use DialTLSContext instead, which allows the transport
    // to cancel dials as soon as they are no longer needed.
    // If both are set, DialTLSContext takes priority.
    public Func<@string, @string, (net.Conn, error)> DialTLS;
    // TLSClientConfig specifies the TLS configuration to use with
    // tls.Client.
    // If nil, the default configuration is used.
    // If non-nil, HTTP/2 support may not be enabled by default.
    public ж<tls.Config> TLSClientConfig;
    // TLSHandshakeTimeout specifies the maximum amount of time to
    // wait for a TLS handshake. Zero means no timeout.
    public time.Duration TLSHandshakeTimeout;
    // DisableKeepAlives, if true, disables HTTP keep-alives and
    // will only use the connection to the server for a single
    // HTTP request.
    //
    // This is unrelated to the similarly named TCP keep-alives.
    public bool DisableKeepAlives;
    // DisableCompression, if true, prevents the Transport from
    // requesting compression with an "Accept-Encoding: gzip"
    // request header when the Request contains no existing
    // Accept-Encoding value. If the Transport requests gzip on
    // its own and gets a gzipped response, it's transparently
    // decoded in the Response.Body. However, if the user
    // explicitly requested gzip it is not automatically
    // uncompressed.
    public bool DisableCompression;
    // MaxIdleConns controls the maximum number of idle (keep-alive)
    // connections across all hosts. Zero means no limit.
    public nint MaxIdleConns;
    // MaxIdleConnsPerHost, if non-zero, controls the maximum idle
    // (keep-alive) connections to keep per-host. If zero,
    // DefaultMaxIdleConnsPerHost is used.
    public nint MaxIdleConnsPerHost;
    // MaxConnsPerHost optionally limits the total number of
    // connections per host, including connections in the dialing,
    // active, and idle states. On limit violation, dials will block.
    //
    // Zero means no limit.
    public nint MaxConnsPerHost;
    // IdleConnTimeout is the maximum amount of time an idle
    // (keep-alive) connection will remain idle before closing
    // itself.
    // Zero means no limit.
    public time.Duration IdleConnTimeout;
    // ResponseHeaderTimeout, if non-zero, specifies the amount of
    // time to wait for a server's response headers after fully
    // writing the request (including its body, if any). This
    // time does not include the time to read the response body.
    public time.Duration ResponseHeaderTimeout;
    // ExpectContinueTimeout, if non-zero, specifies the amount of
    // time to wait for a server's first response headers after fully
    // writing the request headers if the request has an
    // "Expect: 100-continue" header. Zero means no timeout and
    // causes the body to be sent immediately, without
    // waiting for the server to approve.
    // This time does not include the time to send the request header.
    public time.Duration ExpectContinueTimeout;
    // TLSNextProto specifies how the Transport switches to an
    // alternate protocol (such as HTTP/2) after a TLS ALPN
    // protocol negotiation. If Transport dials a TLS connection
    // with a non-empty protocol name and TLSNextProto contains a
    // map entry for that key (such as "h2"), then the func is
    // called with the request's authority (such as "example.com"
    // or "example.com:1234") and the TLS connection. The function
    // must return a RoundTripper that then handles the request.
    // If TLSNextProto is not nil, HTTP/2 support is not enabled
    // automatically.
    public map<@string, Func<@string, ж<tls.Conn>, RoundTripper>> TLSNextProto;
    // ProxyConnectHeader optionally specifies headers to send to
    // proxies during CONNECT requests.
    // To set the header dynamically, see GetProxyConnectHeader.
    public ΔHeader ProxyConnectHeader;
    // GetProxyConnectHeader optionally specifies a func to return
    // headers to send to proxyURL during a CONNECT request to the
    // ip:port target.
    // If it returns an error, the Transport's RoundTrip fails with
    // that error. It can return (nil, nil) to not add headers.
    // If GetProxyConnectHeader is non-nil, ProxyConnectHeader is
    // ignored.
    public Func<context.Context, ж<url.URL>, @string, (ΔHeader, error)> GetProxyConnectHeader;
    // MaxResponseHeaderBytes specifies a limit on how many
    // response bytes are allowed in the server's response
    // header.
    //
    // Zero means to use a default limit.
    public int64 MaxResponseHeaderBytes;
    // WriteBufferSize specifies the size of the write buffer used
    // when writing to the transport.
    // If zero, a default (currently 4KB) is used.
    public nint WriteBufferSize;
    // ReadBufferSize specifies the size of the read buffer used
    // when reading from the transport.
    // If zero, a default (currently 4KB) is used.
    public nint ReadBufferSize;
    // nextProtoOnce guards initialization of TLSNextProto and
    // h2transport (via onceSetNextProtoDefaults)
    internal sync.Once nextProtoOnce;
    internal h2Transport h2transport; // non-nil if http2 wired up
    internal bool tlsNextProtoWasNil;        // whether TLSNextProto was nil when the Once fired
    // ForceAttemptHTTP2 controls whether HTTP/2 is enabled when a non-zero
    // Dial, DialTLS, or DialContext func or TLSClientConfig is provided.
    // By default, use of any those fields conservatively disables HTTP/2.
    // To use a custom dialer or TLS config and still attempt HTTP/2
    // upgrades, set this to true.
    public bool ForceAttemptHTTP2;
    // HTTP2 configures HTTP/2 connections.
    //
    // This field does not yet have any effect.
    // See https://go.dev/issue/67813.
    public ж<HTTP2Config> HTTP2;
    // Protocols is the set of protocols supported by the transport.
    //
    // If Protocols includes UnencryptedHTTP2 and does not include HTTP1,
    // the transport will use unencrypted HTTP/2 for requests for http:// URLs.
    //
    // If Protocols is nil, the default is usually HTTP/1 only.
    // If ForceAttemptHTTP2 is true, or if TLSNextProto contains an "h2" entry,
    // the default is HTTP/1 and HTTP/2.
    public ж<Protocols> Protocols;
}

[GoRecv] internal static nint writeBufferSize(this ref Transport t) {
    if (t.WriteBufferSize > 0) {
        return t.WriteBufferSize;
    }
    return (4 << (int)(10));
}

[GoRecv] internal static nint readBufferSize(this ref Transport t) {
    if (t.ReadBufferSize > 0) {
        return t.ReadBufferSize;
    }
    return (4 << (int)(10));
}

// Clone returns a deep copy of t's exported fields.
public static ж<Transport> Clone(this ж<Transport> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.of(Transport.ᏑnextProtoOnce).Do(Ꮡt.onceSetNextProtoDefaults);
    var t2 = Ꮡ(new Transport(
        Proxy: t.Proxy,
        OnProxyConnectResponse: t.OnProxyConnectResponse,
        DialContext: t.DialContext,
        Dial: t.Dial,
        DialTLS: t.DialTLS,
        DialTLSContext: t.DialTLSContext,
        TLSHandshakeTimeout: t.TLSHandshakeTimeout,
        DisableKeepAlives: t.DisableKeepAlives,
        DisableCompression: t.DisableCompression,
        MaxIdleConns: t.MaxIdleConns,
        MaxIdleConnsPerHost: t.MaxIdleConnsPerHost,
        MaxConnsPerHost: t.MaxConnsPerHost,
        IdleConnTimeout: t.IdleConnTimeout,
        ResponseHeaderTimeout: t.ResponseHeaderTimeout,
        ExpectContinueTimeout: t.ExpectContinueTimeout,
        ProxyConnectHeader: t.ProxyConnectHeader.Clone(),
        GetProxyConnectHeader: t.GetProxyConnectHeader,
        MaxResponseHeaderBytes: t.MaxResponseHeaderBytes,
        ForceAttemptHTTP2: t.ForceAttemptHTTP2,
        WriteBufferSize: t.WriteBufferSize,
        ReadBufferSize: t.ReadBufferSize
    ));
    if (t.TLSClientConfig != nil) {
        t2.Value.TLSClientConfig = t.TLSClientConfig.Clone();
    }
    if (t.HTTP2 != nil) {
        t2.Value.HTTP2 = Ꮡ(new HTTP2Config(nil));
        (~t2).HTTP2.Value = t.HTTP2.Value;
    }
    if (t.Protocols != nil) {
        t2.Value.Protocols = Ꮡ(new Protocols(nil));
        (~t2).Protocols.Value = t.Protocols.Value;
    }
    if (!t.tlsNextProtoWasNil) {
        var npm = maps.Clone<map<@string, Func<@string, ж<tls.Conn>, RoundTripper>>, @string, Func<@string, ж<tls.Conn>, RoundTripper>>(t.TLSNextProto);
        if (npm == default!) {
            npm = new map<@string, Func<@string, ж<tls.Conn>, RoundTripper>>();
        }
        t2.Value.TLSNextProto = npm;
    }
    return t2;
}

// h2Transport is the interface we expect to be able to call from
// net/http against an *http2.Transport that's either bundled into
// h2_bundle.go or supplied by the user via x/net/http2.
//
// We name it with the "h2" prefix to stay out of the "http2" prefix
// namespace used by x/tools/cmd/bundle for h2_bundle.go.
[GoType] partial interface h2Transport {
    void CloseIdleConnections();
}

[GoRecv] internal static bool hasCustomTLSDialer(this ref Transport t) {
    return t.DialTLS != default! || t.DialTLSContext != default!;
}

internal static ж<godebug.Setting> http2client = godebug.New("http2client"u8);

// onceSetNextProtoDefaults initializes TLSNextProto.
// It must be called via t.nextProtoOnce.Do.
internal static void onceSetNextProtoDefaults(this ж<Transport> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.tlsNextProtoWasNil = (t.TLSNextProto == default!);
    if (http2client.Value() == "0"u8) {
        http2client.IncNonDefault();
        return;
    }
    // If they've already configured http2 with
    // golang.org/x/net/http2 instead of the bundled copy, try to
    // get at its http2.Transport value (via the "https"
    // altproto map) so we can call CloseIdleConnections on it if
    // requested. (Issue 22891)
    var (altProto, _) = Ꮡt.of(Transport.ᏑaltProto).Load()._<map<@string, RoundTripper>>(ᐧ);
    {
        var rv = reflect.ValueOf(altProto[httpsˢ]); if (rv.IsValid() && rv.Type().Kind() == reflect.Struct && rv.Type().NumField() == 1) {
            {
                var v = rv.Field(0); if (v.CanInterface()) {
                    {
                        var (h2i, ok) = v.Interface()._<h2Transport>(ᐧ); if (ok) {
                            t.h2transport = h2i;
                            return;
                        }
                    }
                }
            }
        }
    }
    {
        var (_, ok) = t.TLSNextProto["h2"u8, ꟷ]; if (ok) {
            // There's an existing HTTP/2 implementation installed.
            return;
        }
    }
    var protocols = t.protocols();
    if (!protocols.HTTP2() && !protocols.UnencryptedHTTP2()) {
        return;
    }
    if (omitBundledHTTP2) {
        return;
    }
    var (t2, err) = http2configureTransports(Ꮡt);
    if (err != default!) {
        log.Printf("Error enabling Transport HTTP/2 support: %v"u8, err);
        return;
    }
    t.h2transport = new http2Transportжh2Transport(t2);
    // Auto-configure the http2.Transport's MaxHeaderListSize from
    // the http.Transport's MaxResponseHeaderBytes. They don't
    // exactly mean the same thing, but they're close.
    //
    // TODO: also add this to x/net/http2.Configure Transport, behind
    // a +build go1.7 build tag:
    {
        var limit1 = t.MaxResponseHeaderBytes; if (limit1 != 0 && (~t2).MaxHeaderListSize == 0) {
            UntypedInt h2max = /* 1<<32 - 1 */ 4294967295;
            if (limit1 >= h2max){
                t2.Value.MaxHeaderListSize = h2max;
            } else {
                t2.Value.MaxHeaderListSize = (uint32)limit1;
            }
        }
    }
    // Server.ServeTLS clones the tls.Config before modifying it.
    // Transport doesn't. We may want to make the two consistent some day.
    //
    // http2configureTransport will have already set NextProtos, but adjust it again
    // here to remove HTTP/1.1 if the user has disabled it.
    t.TLSClientConfig.Value.NextProtos = adjustNextProtos((~t.TLSClientConfig).NextProtos, protocols);
}

[GoRecv] internal static Protocols protocols(this ref Transport t) {
    if (t.Protocols != nil) {
        return t.Protocols.Value; // user-configured set
    }
    Protocols p = default!;
    p.SetHTTP1(true); // default always includes HTTP/1
    switch (ᐧ) {
    case {} when t.TLSNextProto != default!: {
        if (t.TLSNextProto["h2"u8] != default!) {
            // Setting TLSNextProto to an empty map is a documented way
            // to disable HTTP/2 on a Transport.
            p.SetHTTP2(true);
        }
        break;
    }
    case {} when !t.ForceAttemptHTTP2 && (t.TLSClientConfig != nil || t.Dial != default! || t.DialContext != default! || t.hasCustomTLSDialer()): {
        break;
    }
    case {} when http2client.Value() == "0"u8: {
        break;
    }
    default: {
        p.SetHTTP2(true);
        break;
    }}

    // Be conservative and don't automatically enable
    // http2 if they've specified a custom TLS config or
    // custom dialers. Let them opt-in themselves via
    // Transport.Protocols.SetHTTP2(true) so we don't surprise them
    // by modifying their tls.Config. Issue 14275.
    // However, if ForceAttemptHTTP2 is true, it overrides the above checks.
    return p;
}

// ProxyFromEnvironment returns the URL of the proxy to use for a
// given request, as indicated by the environment variables
// HTTP_PROXY, HTTPS_PROXY and NO_PROXY (or the lowercase versions
// thereof). Requests use the proxy from the environment variable
// matching their scheme, unless excluded by NO_PROXY.
//
// The environment values may be either a complete URL or a
// "host[:port]", in which case the "http" scheme is assumed.
// An error is returned if the value is a different form.
//
// A nil URL and nil error are returned if no proxy is defined in the
// environment, or a proxy should not be used for the given request,
// as defined by NO_PROXY.
//
// As a special case, if req.URL.Host is "localhost" (with or without
// a port number), then a nil URL and nil error will be returned.
public static (ж<url.URL>, error) ProxyFromEnvironment(ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    return envProxyFunc()(req.URL);
}

// ProxyURL returns a proxy function (for use in a [Transport])
// that always returns the same URL.
public static Func<ж<Request>, (ж<url.URL>, error)> ProxyURL(ж<url.URL> ᏑfixedURL) {
    return (ж<Request> _) => (ᏑfixedURL, default!);
}

// transportRequest is a wrapper around a *Request that adds
// optional extra headers to write and stores any error to return
// from roundTrip.
[GoType] partial struct transportRequest {
    public partial ref ж<Request> Request { get; }                     // original request, not to be mutated
    internal ΔHeader extra;                 // extra headers to write, or nil
    internal ж<httptrace.ClientTrace> trace; // optional
    internal context.Context ctx; // canceled when we are done with the request
    internal Action<error> cancel;
    internal sync.Mutex mu; // guards err
    internal error err;      // first setError value for mapRoundTripError to consider
}

[GoRecv] internal static ΔHeader extraHeaders(this ref transportRequest tr) {
    if (tr.extra == default!) {
        tr.extra = new ΔHeader(0);
    }
    return tr.extra;
}

internal static void setError(this ж<transportRequest> Ꮡtr, error err) {
    ref var tr = ref Ꮡtr.DerefOrNull();

    tr.mu.Lock();
    if (tr.err == default!) {
        tr.err = err;
    }
    tr.mu.Unlock();
}

// useRegisteredProtocol reports whether an alternate protocol (as registered
// with Transport.RegisterProtocol) should be respected for this request.
[GoRecv] internal static bool useRegisteredProtocol(this ref Transport t, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    if ((~req.URL).Scheme == "https"u8 && req.requiresHTTP1()) {
        // If this request requires HTTP/1, don't use the
        // "https" alternate protocol, which is used by the
        // HTTP/2 code to take over requests if there's an
        // existing cached HTTP/2 connection.
        return false;
    }
    return true;
}

// alternateRoundTripper returns the alternate RoundTripper to use
// for this request if the Request's URL scheme requires one,
// or nil for the normal case of using the Transport.
internal static RoundTripper alternateRoundTripper(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var req = ref Ꮡreq.DerefOrNull();

    if (!t.useRegisteredProtocol(Ꮡreq)) {
        return default!;
    }
    var (altProto, _) = Ꮡt.of(Transport.ᏑaltProto).Load()._<map<@string, RoundTripper>>(ᐧ);
    return altProto[(~req.URL).Scheme];
}

internal static @string validateHeaders(ΔHeader hdrs) {
    foreach (var (k, vv) in hdrs) {
        if (!httpguts.ValidHeaderFieldName(k)) {
            return fmt.Sprintf("field name %q"u8, k);
        }
        foreach (var (_, v) in vv) {
            if (!httpguts.ValidHeaderFieldValue(v)) {
                // Don't include the value in the error,
                // because it may be sensitive.
                return fmt.Sprintf("field value for %q"u8, k);
            }
        }
    }
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpNilRequestHeaderˢ = "http: nil Request.Header"u8;
internal static readonly @string unsupportedProtocolˢ = "unsupported protocol scheme"u8;
internal static readonly @string httpNoHostInRequestUrlˢ = "http: no Host in request URL"u8;

// roundTrip implements a RoundTripper over HTTP.
internal static (ж<Response>, error err) roundTrip(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) {
    ж<Response> _ᴛ1 = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var req = ref Ꮡreq.DerefOrNull();

        Ꮡt.of(Transport.ᏑnextProtoOnce).Do(Ꮡt.onceSetNextProtoDefaults);
        var ctx = req.Context();
        var trace = httptrace.ContextClientTrace(ctx);
        if (req.URL == nil) {
            req.closeBody();
            (_ᴛ1, err) = (default!, errors.New(httpNilRequestUrlˢ)); goto ᒐdone;
        }
        if (req.Header == default!) {
            req.closeBody();
            (_ᴛ1, err) = (default!, errors.New(httpNilRequestHeaderˢ)); goto ᒐdone;
        }
        @string scheme = req.URL.Value.Scheme;
        var isHTTP = scheme == "http"u8 || scheme == "https"u8;
        if (isHTTP) {
            // Validate the outgoing headers.
            {
                @string errΔ1 = validateHeaders(req.Header); if (errΔ1 != ""u8) {
                    req.closeBody();
                    (_ᴛ1, err) = (default!, fmt.Errorf("net/http: invalid header %s"u8, errΔ1)); goto ᒐdone;
                }
            }
            // Validate the outgoing trailers too.
            {
                @string errΔ2 = validateHeaders(req.Trailer); if (errΔ2 != ""u8) {
                    req.closeBody();
                    (_ᴛ1, err) = (default!, fmt.Errorf("net/http: invalid trailer %s"u8, errΔ2)); goto ᒐdone;
                }
            }
        }
        var origReq = Ꮡreq;
        Ꮡreq = setupRewindBody(Ꮡreq); req = ref Ꮡreq.DerefOrNull();
        {
            var altRT = Ꮡt.alternateRoundTripper(Ꮡreq); if (altRT != default!) {
                {
                    var (resp, errΔ3) = altRT.RoundTrip(Ꮡreq); if (!AreEqual(errΔ3, ErrSkipAltProtocol)) {
                        (_ᴛ1, err) = (resp, errΔ3); goto ᒐdone;
                    }
                }
                error errΔ4 = default!;
                (Ꮡreq, errΔ4) = rewindBody(Ꮡreq); req = ref Ꮡreq.DerefOrNull();
                if (errΔ4 != default!) {
                    (_ᴛ1, err) = (default!, errΔ4); goto ᒐdone;
                }
            }
        }
        if (!isHTTP) {
            req.closeBody();
            (_ᴛ1, err) = (default!, badStringError(unsupportedProtocolˢ, scheme)); goto ᒐdone;
        }
        if (req.Method != ""u8 && !validMethod(req.Method)) {
            req.closeBody();
            (_ᴛ1, err) = (default!, fmt.Errorf("net/http: invalid method %q"u8, req.Method)); goto ᒐdone;
        }
        if ((~req.URL).Host == ""u8) {
            req.closeBody();
            (_ᴛ1, err) = (default!, errors.New(httpNoHostInRequestUrlˢ)); goto ᒐdone;
        }
        // Transport request context.
        //
        // If RoundTrip returns an error, it cancels this context before returning.
        //
        // If RoundTrip returns no error:
        //   - For an HTTP/1 request, persistConn.readLoop cancels this context
        //     after reading the request body.
        //   - For an HTTP/2 request, RoundTrip cancels this context after the HTTP/2
        //     RoundTripper returns.
        (ctx, var cancel) = context_package.WithCancelCause(req.Context());
        // Convert Request.Cancel into context cancelation.
        if ((~origReq).Cancel != default!) {
            goǃ((ᴛ1, ᴛ2, ᴛ3) => awaitLegacyCancel(ᴛ1, ᴛ2, ref ᴛ3.DerefOrNull()), ctx, cancel, origReq);
        }
        // Convert Transport.CancelRequest into context cancelation.
        //
        // This is lamentably expensive. CancelRequest has been deprecated for a long time
        // and doesn't work on HTTP/2 requests. Perhaps we should drop support for it entirely.
        cancel = Ꮡt.prepareTransportCancel(origReq, cancel);
        var cancelʗ1 = cancel;
        defer(() => {
            if (err != default!) {
                cancelʗ1(err);
            }
        }, ref ᒐ);
        while (ᐧ) {
            var selᴛ93 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ93, ꓸꓸꓸ))) {
            case 0 when selᴛ93.ꟷᐳ(out _): {
                req.closeBody();
                (_ᴛ1, err) = (default!, context_package.Cause(ctx)); goto ᒐdone;
            }
            default: {
                break;
            }}
            // treq gets modified by roundTrip, so we need to recreate for each retry.
            var treq = Ꮡ(new transportRequest(Request: Ꮡreq, trace: trace, ctx: ctx, cancel: cancel));
            var (cm, errΔ5) = t.connectMethodForRequest(treq);
            if (errΔ5 != default!) {
                req.closeBody();
                (_ᴛ1, err) = (default!, errΔ5); goto ᒐdone;
            }
            // Get the cached or newly-created connection to either the
            // host (for http or https), the http proxy, or the http proxy
            // pre-CONNECTed to https server. In any case, we'll be ready
            // to send it requests.
            (var pconn, errΔ5) = Ꮡt.getConn(treq, cm);
            if (errΔ5 != default!) {
                req.closeBody();
                (_ᴛ1, err) = (default!, errΔ5); goto ᒐdone;
            }
            ж<Response> resp = default!;
            if ((~pconn).alt != default!){
                // HTTP/2 path.
                (resp, errΔ5) = (~pconn).alt.RoundTrip(Ꮡreq);
            } else {
                (resp, errΔ5) = pconn.roundTrip(treq);
            }
            if (errΔ5 == default!) {
                if ((~pconn).alt != default!) {
                    // HTTP/2 requests are not cancelable with CancelRequest,
                    // so we have no further need for the request context.
                    //
                    // On the HTTP/1 path, roundTrip takes responsibility for
                    // canceling the context after the response body is read.
                    cancel(errRequestDone);
                }
                resp.Value.Request = origReq;
                (_ᴛ1, err) = (resp, default!); goto ᒐdone;
            }
            // Failed. Clean up and determine whether to retry.
            if (http2isNoCachedConnError(errΔ5)){
                if (Ꮡt.removeIdleConn(pconn)) {
                    Ꮡt.decConnsPerHost((~pconn).cacheKey);
                }
            } else 
            if (!pconn.shouldRetryRequest(Ꮡreq, errΔ5)) {
                // Issue 16465: return underlying net.Conn.Read error from peek,
                // as we've historically done.
                {
                    var (e, ok) = errΔ5._<nothingWrittenError>(ᐧ); if (ok) {
                        errΔ5 = e.error;
                    }
                }
                {
                    var (e, ok) = errΔ5._<transportReadFromServerError>(ᐧ); if (ok) {
                        errΔ5 = e.err;
                    }
                }
                {
                    var (b, ok) = req.Body._<ж<readTrackingBody>>(ᐧ); if (ok && !(~b).didClose) {
                        // Issue 49621: Close the request body if pconn.roundTrip
                        // didn't do so already. This can happen if the pconn
                        // write loop exits without reading the write request.
                        req.closeBody();
                    }
                }
                (_ᴛ1, err) = (default!, errΔ5); goto ᒐdone;
            }
            testHookRoundTripRetried();
            // Rewind the body if we're able to.
            (Ꮡreq, errΔ5) = rewindBody(Ꮡreq); req = ref Ꮡreq.DerefOrNull();
            if (errΔ5 != default!) {
                (_ᴛ1, err) = (default!, errΔ5); goto ᒐdone;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, err);
}

internal static void awaitLegacyCancel(context.Context ctx, Action<error> cancel, ref Request req) {
    var selᴛ94 = req.Cancel;
    var selᴛ95 = ctx.Done();
    switch (select(ᐸꟷ(selᴛ94, ꓸꓸꓸ), ᐸꟷ(selᴛ95, ꓸꓸꓸ))) {
    case 0 when selᴛ94.ꟷᐳ(out _): {
        cancel(errRequestCanceled);
        break;
    }
    case 1 when selᴛ95.ꟷᐳ(out _): {
        break;
    }}
}

internal static error errCannotRewind = errors.New("net/http: cannot rewind body after connection loss"u8);

[GoType] partial struct readTrackingBody {
    public io_package.ReadCloser ReadCloser;
    internal bool didRead;
    internal bool didClose;
}

[GoRecv] internal static (nint, error) Read(this ref readTrackingBody r, slice<byte> data) {
    r.didRead = true;
    return r.ReadCloser.Read(data);
}

[GoRecv] internal static error Close(this ref readTrackingBody r) {
    r.didClose = true;
    return r.ReadCloser.Close();
}

// setupRewindBody returns a new request with a custom body wrapper
// that can report whether the body needs rewinding.
// This lets rewindBody avoid an error result when the request
// does not have GetBody but the body hasn't been read at all yet.
internal static ж<Request> setupRewindBody(ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    if (req.Body == default! || AreEqual(req.Body, NoBody)) {
        return Ꮡreq;
    }
    ref var newReq = ref heap<Request>(out var ᏑnewReq);
    newReq = req;
    newReq.Body = new readTrackingBodyжReadCloser(Ꮡ(new readTrackingBody(ReadCloser: req.Body)));
    return ᏑnewReq;
}

// rewindBody returns a new request with the body rewound.
// It returns req unmodified if the body does not need rewinding.
// rewindBody takes care of closing req.Body when appropriate
// (in all cases except when rewindBody returns req unmodified).
internal static (ж<Request> rewound, error err) rewindBody(ж<Request> Ꮡreq) {
    error err = default!;

    ref var req = ref Ꮡreq.DerefOrNull();
    if (req.Body == default! || AreEqual(req.Body, NoBody) || (!(~req.Body._<ж<readTrackingBody>>()).didRead && !(~req.Body._<ж<readTrackingBody>>()).didClose)) {
        return (Ꮡreq, default!); // nothing to rewind
    }
    if (!(~req.Body._<ж<readTrackingBody>>()).didClose) {
        req.closeBody();
    }
    if (req.GetBody == default!) {
        return (default!, errCannotRewind);
    }
    (var body, err) = req.GetBody();
    if (err != default!) {
        return (default!, err);
    }
    ref var newReq = ref heap<Request>(out var ᏑnewReq);
    newReq = req;
    newReq.Body = new readTrackingBodyжReadCloser(Ꮡ(new readTrackingBody(ReadCloser: body)));
    return (ᏑnewReq, default!);
}

// shouldRetryRequest reports whether we should retry sending a failed
// HTTP request on a new connection. The non-nil input error is the
// error from roundTrip.
internal static bool shouldRetryRequest(this ж<persistConn> Ꮡpc, ж<Request> Ꮡreq, error err) {
    ref var req = ref Ꮡreq.DerefOrNull();

    if (http2isNoCachedConnError(err)) {
        // Issue 16582: if the user started a bunch of
        // requests at once, they can all pick the same conn
        // and violate the server's max concurrent streams.
        // Instead, match the HTTP/1 behavior for now and dial
        // again to get a new TCP connection, rather than failing
        // this request.
        return true;
    }
    if (AreEqual(err, errMissingHost)) {
        // User error.
        return false;
    }
    if (!Ꮡpc.isReused()) {
        // This was a fresh connection. There's no reason the server
        // should've hung up on us.
        //
        // Also, if we retried now, we could loop forever
        // creating new connections and retrying if the server
        // is just hanging up on us because it doesn't like
        // our request (as opposed to sending an error).
        return false;
    }
    {
        var (_, ok) = err._<nothingWrittenError>(ᐧ); if (ok) {
            // We never wrote anything, so it's safe to retry, if there's no body or we
            // can "rewind" the body with GetBody.
            return req.outgoingLength() == 0 || req.GetBody != default!;
        }
    }
    if (!req.isReplayable()) {
        // Don't retry non-idempotent requests.
        return false;
    }
    {
        var (_, ok) = err._<transportReadFromServerError>(ᐧ); if (ok) {
            // We got some non-EOF net.Conn.Read failure reading
            // the 1st response byte from the server.
            return true;
        }
    }
    if (AreEqual(err, errServerClosedIdle)) {
        // The server replied with io.EOF while we were trying to
        // read the response. Probably an unfortunately keep-alive
        // timeout, just as the client was writing a request.
        return true;
    }
    return false; // conservatively
}

// ErrSkipAltProtocol is a sentinel error value defined by Transport.RegisterProtocol.
public static error ErrSkipAltProtocol = errors.New("net/http: skip alternate protocol"u8);

// RegisterProtocol registers a new protocol with scheme.
// The [Transport] will pass requests using the given scheme to rt.
// It is rt's responsibility to simulate HTTP request semantics.
//
// RegisterProtocol can be used by other packages to provide
// implementations of protocol schemes like "ftp" or "file".
//
// If rt.RoundTrip returns [ErrSkipAltProtocol], the Transport will
// handle the [Transport.RoundTrip] itself for that one request, as if the
// protocol were not registered.
public static void RegisterProtocol(this ж<Transport> Ꮡt, @string scheme, RoundTripper rt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.altMu.Lock();
        ᒐd1 = true;
        var (oldMap, _) = Ꮡt.of(Transport.ᏑaltProto).Load()._<map<@string, RoundTripper>>(ᐧ);
        {
            var (_, exists) = oldMap[scheme, ꟷ]; if (exists) {
                throw panic("protocol " + scheme + " already registered");
            }
        }
        var newMap = maps.Clone<map<@string, RoundTripper>, @string, RoundTripper>(oldMap);
        if (newMap == default!) {
            newMap = new map<@string, RoundTripper>();
        }
        newMap[scheme] = rt;
        Ꮡt.of(Transport.ᏑaltProto).Store(newMap);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().altMu.Unlock(); ᒐ.Run(); }
}

// CloseIdleConnections closes any connections which were previously
// connected from previous requests but are now sitting idle in
// a "keep-alive" state. It does not interrupt any connections currently
// in use.
public static void CloseIdleConnections(this ж<Transport> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.of(Transport.ᏑnextProtoOnce).Do(Ꮡt.onceSetNextProtoDefaults);
    t.idleMu.Lock();
    var m = t.idleConn;
    t.idleConn = default!;
    t.closeIdle = true; // close newly idle connections
    t.idleLRU = new connLRU(nil);
    t.idleMu.Unlock();
    foreach (var (_, conns) in m) {
        foreach (var (_, pconn) in conns) {
            pconn.close(errCloseIdleConns);
        }
    }
    t.connsPerHostMu.Lock();
    t.dialsInProgress.all((ж<wantConn> w) => {
        if ((~w).cancelCtx != default! && !w.waiting()) {
            (~w).cancelCtx();
        }
    });
    t.connsPerHostMu.Unlock();
    {
        var t2 = t.h2transport; if (t2 != default!) {
            t2.CloseIdleConnections();
        }
    }
}

// prepareTransportCancel sets up state to convert Transport.CancelRequest into context cancelation.
internal static Action<error> prepareTransportCancel(this ж<Transport> Ꮡt, ж<Request> Ꮡreq, Action<error> origCancel) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Historically, RoundTrip has not modified the Request in any way.
    // We could avoid the need to keep a map of all in-flight requests by adding
    // a field to the Request containing its cancel func, and setting that field
    // while the request is in-flight. Callers aren't supposed to reuse a Request
    // until after the response body is closed, so this wouldn't violate any
    // concurrency guarantees.
    var cancel = (error err) => {
        origCancel(err);
        Ꮡt.Value.reqMu.Lock();
        delete(Ꮡt.Value.reqCanceler, Ꮡreq);
        Ꮡt.Value.reqMu.Unlock();
    };
    t.reqMu.Lock();
    if (t.reqCanceler == default!) {
        t.reqCanceler = new map<ж<Request>, Action<error>>();
    }
    t.reqCanceler[Ꮡreq] = cancel;
    t.reqMu.Unlock();
    return cancel;
}

// CancelRequest cancels an in-flight request by closing its connection.
// CancelRequest should only be called after [Transport.RoundTrip] has returned.
//
// Deprecated: Use [Request.WithContext] to create a request with a
// cancelable context instead. CancelRequest cannot cancel HTTP/2
// requests. This may become a no-op in a future release of Go.
public static void CancelRequest(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.reqMu.Lock();
    var cancel = t.reqCanceler[Ꮡreq];
    t.reqMu.Unlock();
    if (cancel != default!) {
        cancel(errRequestCanceled);
    }
}

//
// Private implementation past this point.
//
internal static ж<sync.Once> ᏑenvProxyOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once envProxyOnce => ref ᏑenvProxyOnce.Value;
internal static Func<ж<url.URL>, (ж<url.URL>, error)> envProxyFuncValue;

// envProxyFunc returns a function that reads the
// environment variable to determine the proxy address.
internal static Func<ж<url.URL>, (ж<url.URL>, error)> envProxyFunc() {
    ᏑenvProxyOnce.Do(() => {
        envProxyFuncValue = httpproxy.FromEnvironment().ProxyFunc();
    });
    return envProxyFuncValue;
}

// resetProxyConfig is used by tests.
internal static void resetProxyConfig() {
    envProxyOnce = new sync.Once(nil);
    envProxyFuncValue = default!;
}

[GoRecv] internal static (connectMethod cm, error err) connectMethodForRequest(this ref Transport t, ж<transportRequest> Ꮡtreq) {
    connectMethod cm = default!;
    error err = default!;

    ref var treq = ref Ꮡtreq.DerefOrNull();
    cm.targetScheme = treq.URL.Value.Scheme;
    cm.targetAddr = canonicalAddr(treq.URL);
    if (t.Proxy != default!) {
        (cm.proxyURL, err) = t.Proxy(treq.Request);
    }
    cm.onlyH1 = treq.requiresHTTP1();
    return (cm, err);
}

// proxyAuth returns the Proxy-Authorization header to set
// on requests, if applicable.
[GoRecv] internal static @string proxyAuth(this ref connectMethod cm) {
    if (cm.proxyURL == nil) {
        return ""u8;
    }
    {
        var u = cm.proxyURL.Value.User; if (u != nil) {
            @string username = u.Username();
            var (password, _) = u.Password();
            return "Basic "u8 + basicAuth(username, password);
        }
    }
    return ""u8;
}

// error values for debugging and testing, not seen by users.
internal static error errKeepAlivesDisabled = errors.New("http: putIdleConn: keep alives disabled"u8);

internal static error errConnBroken = errors.New("http: putIdleConn: connection is in bad state"u8);

internal static error errCloseIdle = errors.New("http: putIdleConn: CloseIdleConnections was called"u8);

internal static error errTooManyIdle = errors.New("http: putIdleConn: too many idle connections"u8);

internal static error errTooManyIdleHost = errors.New("http: putIdleConn: too many idle connections for host"u8);

internal static error errCloseIdleConns = errors.New("http: CloseIdleConnections called"u8);

internal static error errReadLoopExiting = errors.New("http: persistConn.readLoop exiting"u8);

internal static error errIdleConnTimeout = errors.New("http: idle connection timeout"u8);

internal static error errServerClosedIdle = errors.New("http: server closed idle connection"u8);

// transportReadFromServerError is used by Transport.readLoop when the
// 1 byte peek read fails and we're actually anticipating a response.
// Usually this is just due to the inherent keep-alive shut down race,
// where the server closed the connection at the same time the client
// wrote. The underlying err field is usually io.EOF or some
// ECONNRESET sort of thing which varies by platform. But it might be
// the user's custom net.Conn.Read error too, so we carry it along for
// them to return from Transport.RoundTrip.
[GoType] partial struct transportReadFromServerError {
    internal error err;
}

internal static error Unwrap(this transportReadFromServerError e) {
    return e.err;
}

internal static @string Error(this transportReadFromServerError e) {
    return fmt.Sprintf("net/http: Transport failed to read from server: %v"u8, e.err);
}

internal static void putOrCloseIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) {
    {
        var err = Ꮡt.tryPutIdleConn(Ꮡpconn); if (err != default!) {
            Ꮡpconn.close(err);
        }
    }
}

[GoRecv] internal static nint maxIdleConnsPerHost(this ref Transport t) {
    {
        nint v = t.MaxIdleConnsPerHost; if (v != 0) {
            return v;
        }
    }
    return DefaultMaxIdleConnsPerHost;
}

// tryPutIdleConn adds pconn to the list of idle persistent connections awaiting
// a new request.
// If pconn is no longer needed or not in a good state, tryPutIdleConn returns
// an error explaining why it wasn't registered.
// tryPutIdleConn does not close pconn. Use putOrCloseIdleConn instead for that.
internal static error tryPutIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var pconn = ref Ꮡpconn.DerefOrNull();

        if (t.DisableKeepAlives || t.MaxIdleConnsPerHost < 0) {
            return errKeepAlivesDisabled;
        }
        if (Ꮡpconn.isBroken()) {
            return errConnBroken;
        }
        Ꮡpconn.markReused();
        t.idleMu.Lock();
        ᒐd1 = true;
        // HTTP/2 (pconn.alt != nil) connections do not come out of the idle list,
        // because multiple goroutines can use them simultaneously.
        // If this is an HTTP/2 connection being “returned,” we're done.
        if (pconn.alt != default! && t.idleLRU.m[Ꮡpconn] != nil) {
            return default!;
        }
        // Deliver pconn to goroutine waiting for idle connection, if any.
        // (They may be actively dialing, but this conn is ready first.
        // Chrome calls this socket late binding.
        // See https://www.chromium.org/developers/design-documents/network-stack#TOC-Connection-Management.)
        var key = pconn.cacheKey;
        {
            var (q, ok) = t.idleConnWait[key, ꟷ]; if (ok) {
                var done = false;
                if (pconn.alt == default!){
                    // HTTP/1.
                    // Loop over the waiting list until we find a w that isn't done already, and hand it pconn.
                    while (q.len() > 0) {
                        var w = q.popFront();
                        if (w.tryDeliver(Ꮡpconn, default!, new time.Time(nil))) {
                            done = true;
                            break;
                        }
                    }
                } else {
                    // HTTP/2.
                    // Can hand the same pconn to everyone in the waiting list,
                    // and we still won't be done: we want to put it in the idle
                    // list unconditionally, for any future clients too.
                    while (q.len() > 0) {
                        var w = q.popFront();
                        w.tryDeliver(Ꮡpconn, default!, new time.Time(nil));
                    }
                }
                if (q.len() == 0){
                    delete(t.idleConnWait, key);
                } else {
                    t.idleConnWait[key] = q;
                }
                if (done) {
                    return default!;
                }
            }
        }
        if (t.closeIdle) {
            return errCloseIdle;
        }
        if (t.idleConn == default!) {
            t.idleConn = new map<connectMethodKey, slice<ж<persistConn>>>();
        }
        var idles = t.idleConn[key];
        if (builtin.len(idles) >= t.maxIdleConnsPerHost()) {
            return errTooManyIdleHost;
        }
        foreach (var (_, exist) in idles) {
            if (exist == Ꮡpconn) {
                log.Fatalf("dup idle pconn %p in freelist"u8, Ꮡpconn.OrTypedNil());
            }
        }
        t.idleConn[key] = append(idles, Ꮡpconn);
        t.idleLRU.add(Ꮡpconn);
        if (t.MaxIdleConns != 0 && t.idleLRU.len() > t.MaxIdleConns) {
            var oldest = t.idleLRU.removeOldest();
            oldest.close(errTooManyIdle);
            t.removeIdleConnLocked(oldest);
        }
        // Set idle timer, but only for HTTP/1 (pconn.alt == nil).
        // The HTTP/2 implementation manages the idle timer itself
        // (see idleConnTimeout in h2_bundle.go).
        if (t.IdleConnTimeout > 0 && pconn.alt == default!) {
            if (pconn.idleTimer != nil){
                pconn.idleTimer.Reset(t.IdleConnTimeout);
            } else {
                pconn.idleTimer = time.AfterFunc(t.IdleConnTimeout, Ꮡpconn.closeConnIfStillIdle);
            }
        }
        pconn.idleAt = time.Now();
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

// queueForIdleConn queues w to receive the next idle connection for w.cm.
// As an optimization hint to the caller, queueForIdleConn reports whether
// it successfully delivered an already-idle connection.
internal static bool /*delivered*/ queueForIdleConn(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) {
    bool delivered = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var w = ref Ꮡw.DerefOrNull();

        if (t.DisableKeepAlives) {
            delivered = false; goto ᒐdone;
        }
        t.idleMu.Lock();
        defer(Ꮡt.of(Transport.ᏑidleMu).Unlock, ref ᒐ);
        // Stop closing connections that become idle - we might want one.
        // (That is, undo the effect of t.CloseIdleConnections.)
        t.closeIdle = false;
        if (Ꮡw == nil) {
            // Happens in test hook.
            delivered = false; goto ᒐdone;
        }
        // If IdleConnTimeout is set, calculate the oldest
        // persistConn.idleAt time we're willing to use a cached idle
        // conn.
        time.Time oldTime = default!;
        if (t.IdleConnTimeout > 0) {
            oldTime = time.Now().Add(-t.IdleConnTimeout);
        }
        // Look for most recently-used idle connection.
        {
            var (list, ok) = t.idleConn[w.key, ꟷ]; if (ok) {
                var stop = false;
                var deliveredΔ1 = false;
                while (builtin.len(list) > 0 && !stop) {
                    var pconn = list[builtin.len(list) - 1];
                    // See whether this connection has been idle too long, considering
                    // only the wall time (the Round(0)), in case this is a laptop or VM
                    // coming out of suspend with previously cached idle connections.
                    var tooOld = !oldTime.IsZero() && (~pconn).idleAt.Round(0).Before(oldTime);
                    if (tooOld) {
                        // Async cleanup. Launch in its own goroutine (as if a
                        // time.AfterFunc called it); it acquires idleMu, which we're
                        // holding, and does a synchronous net.Conn.Close.
                        var pconnʗ1 = pconn;
                        goǃ(pconnʗ1.closeConnIfStillIdle);
                    }
                    if (pconn.isBroken() || tooOld) {
                        // If either persistConn.readLoop has marked the connection
                        // broken, but Transport.removeIdleConn has not yet removed it
                        // from the idle list, or if this persistConn is too old (it was
                        // idle too long), then ignore it and look for another. In both
                        // cases it's already in the process of being closed.
                        list = list.slice(0, builtin.len(list) - 1);
                        continue;
                    }
                    deliveredΔ1 = Ꮡw.tryDeliver(pconn, default!, (~pconn).idleAt);
                    if (deliveredΔ1) {
                        if ((~pconn).alt != default!){
                        } else {
                            // HTTP/2: multiple clients can share pconn.
                            // Leave it in the list.
                            // HTTP/1: only one client can use pconn.
                            // Remove it from the list.
                            t.idleLRU.remove(pconn);
                            list = list.slice(0, builtin.len(list) - 1);
                        }
                    }
                    stop = true;
                }
                if (builtin.len(list) > 0){
                    t.idleConn[w.key] = list;
                } else {
                    delete(t.idleConn, w.key);
                }
                if (stop) {
                    delivered = deliveredΔ1; goto ᒐdone;
                }
            }
        }
        // Register to receive next connection that becomes idle.
        if (t.idleConnWait == default!) {
            t.idleConnWait = new map<connectMethodKey, wantConnQueue>();
        }
        var q = t.idleConnWait[w.key];
        q.cleanFrontNotWaiting();
        q.pushBack(Ꮡw);
        t.idleConnWait[w.key] = q;
        delivered = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return delivered;
}

// removeIdleConn marks pconn as dead.
internal static bool removeIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.idleMu.Lock();
        ᒐd1 = true;
        return t.removeIdleConnLocked(Ꮡpconn);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

// t.idleMu must be held.
[GoRecv] internal static bool removeIdleConnLocked(this ref Transport t, ж<persistConn> Ꮡpconn) {
    ref var pconn = ref Ꮡpconn.DerefOrNull();

    if (pconn.idleTimer != nil) {
        pconn.idleTimer.Stop();
    }
    t.idleLRU.remove(Ꮡpconn);
    var key = pconn.cacheKey;
    var pconns = t.idleConn[key];
    bool removed = default!;
    switch (builtin.len(pconns)) {
    case 0: {
        break;
    }
    case 1: {
        if (pconns[0] == Ꮡpconn) {
            // Nothing
            delete(t.idleConn, key);
            removed = true;
        }
        break;
    }
    default: {
        foreach (var (i, v) in pconns) {
            if (v != Ꮡpconn) {
                continue;
            }
            // Slide down, keeping most recently-used
            // conns at the end.
            copy(pconns.slice(i), pconns.slice(i + 1));
            t.idleConn[key] = pconns.slice(0, builtin.len(pconns) - 1);
            removed = true;
            break;
        }
        break;
    }}

    return removed;
}

internal static ж<net.Dialer> ᏑzeroDialer = new StandardBox<net.Dialer>(default(net.Dialer));
internal static ref net.Dialer zeroDialer => ref ᏑzeroDialer.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netHttpTransportˢ = "net/http: Transport.DialContext hook returned (nil, nil)"u8;
internal static readonly @string netHttpTransportDialHookˢ = "net/http: Transport.Dial hook returned (nil, nil)"u8;

[GoRecv] internal static (net.Conn, error) dial(this ref Transport t, context.Context ctx, @string network, @string addr) {
    if (t.DialContext != default!) {
        var (c, err) = t.DialContext(ctx, network, addr);
        if (c == default! && err == default!) {
            err = errors.New(netHttpTransportˢ);
        }
        return (c, err);
    }
    if (t.Dial != default!) {
        var (c, err) = t.Dial(network, addr);
        if (c == default! && err == default!) {
            err = errors.New(netHttpTransportDialHookˢ);
        }
        return (c, err);
    }
    return ᏑzeroDialer.DialContext(ctx, network, addr);
}

// A wantConn records state about a wanted connection
// (that is, an active call to getConn).
// The conn may be gotten by dialing or by finding an idle connection,
// or a cancellation may make the conn no longer wanted.
// These three options are racing against each other and use
// wantConn to coordinate and agree about the winning outcome.
[GoType] partial struct wantConn {
    internal connectMethod cm;
    internal connectMethodKey key; // cm.key()
    // hooks for testing to know when dials are done
    // beforeDial is called in the getConn goroutine when the dial is queued.
    // afterDial is called when the dial is completed or canceled.
    internal Action beforeDial;
    internal Action afterDial;
    internal sync.Mutex mu;      // protects ctx, done and sending of the result
    internal context.Context ctx; // context for dial, cleared after delivered or canceled
    internal Action cancelCtx;
    internal bool done;             // true after delivered or canceled
    internal channel<connOrError> result; // channel to deliver connection or error
}

[GoType] partial struct connOrError {
    internal ж<persistConn> pc;
    internal error err;
    internal time.Time idleAt;
}

// waiting reports whether w is still waiting for an answer (connection or error).
internal static bool waiting(this ж<wantConn> Ꮡw) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        ᒐd1 = true;
        return !w.done;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡw.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// getCtxForDial returns context for dial or nil if connection was delivered or canceled.
internal static context.Context getCtxForDial(this ж<wantConn> Ꮡw) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        ᒐd1 = true;
        return w.ctx;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡw.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// tryDeliver attempts to deliver pc, err to w and reports whether it succeeded.
internal static bool tryDeliver(this ж<wantConn> Ꮡw, ж<persistConn> Ꮡpc, error err, time.Time idleAt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        ᒐd1 = true;
        if (w.done) {
            return false;
        }
        if ((Ꮡpc == nil) == (err == default!)) {
            throw panic("net/http: internal error: misuse of tryDeliver");
        }
        w.ctx = default!;
        w.done = true;
        w.result.ᐸꟷ(new connOrError(pc: Ꮡpc, err: err, idleAt: idleAt));
        builtin.close(w.result);
        return true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡw.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// cancel marks w as no longer wanting a result (for example, due to cancellation).
// If a connection has been delivered already, cancel returns it with t.putOrCloseIdleConn.
internal static void cancel(this ж<wantConn> Ꮡw, ж<Transport> Ꮡt, error err) {
    ref var w = ref Ꮡw.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    w.mu.Lock();
    ж<persistConn> pc = default!;
    if (w.done){
        {
            var (r, ok) = ᐸꟷ(w.result, ꟷ); if (ok) {
                pc = r.pc;
            }
        }
    } else {
        builtin.close(w.result);
    }
    w.ctx = default!;
    w.done = true;
    w.mu.Unlock();
    // HTTP/2 connections (pc.alt != nil) aren't removed from the idle pool on use,
    // and should not be added back here. If the pconn isn't in the idle pool,
    // it's because we removed it due to an error.
    if (pc != nil && (~pc).alt == default!) {
        Ꮡt.putOrCloseIdleConn(pc);
    }
}

// A wantConnQueue is a queue of wantConns.
[GoType] partial struct wantConnQueue {
    // This is a queue, not a deque.
    // It is split into two stages - head[headPos:] and tail.
    // popFront is trivial (headPos++) on the first stage, and
    // pushBack is trivial (append) on the second stage.
    // If the first stage is empty, popFront can swap the
    // first and second stages to remedy the situation.
    //
    // This two-stage split is analogous to the use of two lists
    // in Okasaki's purely functional queue but without the
    // overhead of reversing the list when swapping stages.
    internal slice<ж<wantConn>> head;
    internal nint headPos;
    internal slice<ж<wantConn>> tail;
}

// len returns the number of items in the queue.
[GoRecv] internal static nint len(this ref wantConnQueue q) {
    return builtin.len(q.head) - q.headPos + builtin.len(q.tail);
}

// pushBack adds w to the back of the queue.
[GoRecv] internal static void pushBack(this ref wantConnQueue q, ж<wantConn> Ꮡw) {
    q.tail = append(q.tail, Ꮡw);
}

// popFront removes and returns the wantConn at the front of the queue.
[GoRecv] internal static ж<wantConn> popFront(this ref wantConnQueue q) {
    if (q.headPos >= builtin.len(q.head)) {
        if (builtin.len(q.tail) == 0) {
            return default!;
        }
        // Pick up tail as new head, clear tail.
        (q.head, q.headPos, q.tail) = (q.tail, 0, q.head[..0]);
    }
    var w = q.head[q.headPos];
    q.head[q.headPos] = default!;
    q.headPos++;
    return w;
}

// peekFront returns the wantConn at the front of the queue without removing it.
[GoRecv] internal static ж<wantConn> peekFront(this ref wantConnQueue q) {
    if (q.headPos < builtin.len(q.head)) {
        return q.head[q.headPos];
    }
    if (builtin.len(q.tail) > 0) {
        return q.tail[0];
    }
    return default!;
}

// cleanFrontNotWaiting pops any wantConns that are no longer waiting from the head of the
// queue, reporting whether any were popped.
[GoRecv] internal static bool /*cleaned*/ cleanFrontNotWaiting(this ref wantConnQueue q) {
    bool cleaned = default!;

    while (ᐧ) {
        var w = q.peekFront();
        if (w == nil || w.waiting()) {
            return cleaned;
        }
        q.popFront();
        cleaned = true;
    }
}

// cleanFrontCanceled pops any wantConns with canceled dials from the head of the queue.
[GoRecv] internal static void cleanFrontCanceled(this ref wantConnQueue q) {
    while (ᐧ) {
        var w = q.peekFront();
        if (w == nil || (~w).cancelCtx != default!) {
            return;
        }
        q.popFront();
    }
}

// all iterates over all wantConns in the queue.
// The caller must not modify the queue while iterating.
[GoRecv] internal static void all(this ref wantConnQueue q, Action<ж<wantConn>> f) {
    foreach (var (_, w) in q.head.slice(q.headPos)) {
        f(w);
    }
    foreach (var (_, w) in q.tail) {
        f(w);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netHttpTransportDialTLSˢ = "net/http: Transport.DialTLS or DialTLSContext returned (nil, nil)"u8;

[GoRecv] internal static (net.Conn conn, error err) customDialTLS(this ref Transport t, context.Context ctx, @string network, @string addr) {
    net.Conn conn = default!;
    error err = default!;

    if (t.DialTLSContext != default!){
        (conn, err) = t.DialTLSContext(ctx, network, addr);
    } else {
        (conn, err) = t.DialTLS(network, addr);
    }
    if (conn == default! && err == default!) {
        err = errors.New(netHttpTransportDialTLSˢ);
    }
    return (conn, err);
}

// getConn dials and creates a new persistConn to the target as
// specified in the connectMethod. This includes doing a proxy CONNECT
// and/or setting up TLS.  If this doesn't return an error, the persistConn
// is ready to write requests to.
internal static (ж<persistConn>, error err) getConn(this ж<Transport> Ꮡt, ж<transportRequest> Ꮡtreq, connectMethod cm) {
    ж<persistConn> _ᴛ1 = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var treq = ref Ꮡtreq.DerefOrNull();

        var req = treq.Request;
        var trace = treq.trace;
        var ctx = req.Context();
        if (trace != nil && (~trace).GetConn != default!) {
            (~trace).GetConn(cm.addr());
        }
        // Detach from the request context's cancellation signal.
        // The dial should proceed even if the request is canceled,
        // because a future request may be able to make use of the connection.
        //
        // We retain the request context's values.
        var (dialCtx, dialCancel) = context_package.WithCancel(context_package.WithoutCancel(ctx));
        var w = Ꮡ(new wantConn(
            cm: cm,
            key: cm.key(),
            ctx: dialCtx,
            cancelCtx: dialCancel,
            result: new channel<connOrError>(1),
            beforeDial: testHookPrePendingDial,
            afterDial: testHookPostPendingDial
        ));
        var wʗ1 = w;
        defer(() => {
            if (err != default!) {
                wʗ1.cancel(Ꮡt, err);
            }
        }, ref ᒐ);
        // Queue for idle connection.
        {
            var delivered = Ꮡt.queueForIdleConn(w); if (!delivered) {
                Ꮡt.queueForDial(w);
            }
        }
        // Wait for completion or cancellation.
        var selᴛ96 = (~w).result;
        var selᴛ97 = treq.ctx.Done();
        switch (select(ᐸꟷ(selᴛ96, ꓸꓸꓸ), ᐸꟷ(selᴛ97, ꓸꓸꓸ))) {
        case 0 when selᴛ96.ꟷᐳ(out var r): {
            if (r.pc != nil && (~r.pc).alt == default! && trace != nil && (~trace).GotConn != default!) {
                // Trace success but only for HTTP/1.
                // HTTP/2 calls trace.GotConn itself.
                var info = new httptrace.GotConnInfo(
                    Conn: (~r.pc).conn,
                    Reused: r.pc.isReused()
                );
                if (!r.idleAt.IsZero()) {
                    info.WasIdle = true;
                    info.IdleTime = time.Since(r.idleAt);
                }
                (~trace).GotConn(info);
            }
            if (r.err != default!) {
                // If the request has been canceled, that's probably
                // what caused r.err; if so, prefer to return the
                // cancellation error (see golang.org/issue/16049).
                var selᴛ98 = treq.ctx.Done();
                switch (trySelect(ᐸꟷ(selᴛ98, ꓸꓸꓸ))) {
                case 0 when selᴛ98.ꟷᐳ(out _): {
                    var errΔ1 = context_package.Cause(treq.ctx);
                    if (AreEqual(errΔ1, errRequestCanceled)) {
                        errΔ1 = errRequestCanceledConn;
                    }
                    (_ᴛ1, err) = (default!, errΔ1); goto ᒐdone;
                }
                default: {
                    break;
                }}
            }
            (_ᴛ1, err) = (r.pc, r.err); goto ᒐdone;
        }
        case 1 when selᴛ97.ꟷᐳ(out _): {
            var errΔ2 = context_package.Cause(treq.ctx);
            if (AreEqual(errΔ2, errRequestCanceled)) {
                // return below
                errΔ2 = errRequestCanceledConn;
            }
            (_ᴛ1, err) = (default!, errΔ2); goto ᒐdone;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, err);
}

// queueForDial queues w to wait for permission to begin dialing.
// Once w receives permission to dial, it will do so in a separate goroutine.
internal static void queueForDial(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var w = ref Ꮡw.DerefOrNull();

        w.beforeDial();
        t.connsPerHostMu.Lock();
        ᒐd1 = true;
        if (t.MaxConnsPerHost <= 0) {
            Ꮡt.startDialConnForLocked(Ꮡw);
            return;
        }
        {
            nint n = t.connsPerHost[w.key]; if (n < t.MaxConnsPerHost) {
                if (t.connsPerHost == default!) {
                    t.connsPerHost = new map<connectMethodKey, nint>();
                }
                t.connsPerHost[w.key] = n + 1;
                Ꮡt.startDialConnForLocked(Ꮡw);
                return;
            }
        }
        if (t.connsPerHostWait == default!) {
            t.connsPerHostWait = new map<connectMethodKey, wantConnQueue>();
        }
        var q = t.connsPerHostWait[w.key];
        q.cleanFrontNotWaiting();
        q.pushBack(Ꮡw);
        t.connsPerHostWait[w.key] = q;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().connsPerHostMu.Unlock(); ᒐ.Run(); }
}

// startDialConnFor calls dialConn in a new goroutine.
// t.connsPerHostMu must be held.
internal static void startDialConnForLocked(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.dialsInProgress.cleanFrontCanceled();
    t.dialsInProgress.pushBack(Ꮡw);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            Ꮡt.dialConnFor(Ꮡw);
            Ꮡt.Value.connsPerHostMu.Lock();
            defer(Ꮡt.of(Transport.ᏑconnsPerHostMu).Unlock, ref ᒐ);
            Ꮡw.Value.cancelCtx = default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// dialConnFor dials on behalf of w and delivers the result to w.
// dialConnFor has received permission to dial w.cm and is counted in t.connCount[w.cm.key()].
// If the dial is canceled or unsuccessful, dialConnFor decrements t.connCount[w.cm.key()].
internal static void dialConnFor(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) {
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        defer(Ꮡw.Value.afterDial, ref ᒐ);
        var ctx = Ꮡw.getCtxForDial();
        if (ctx == default!) {
            Ꮡt.decConnsPerHost(w.key);
            return;
        }
        var (pc, err) = Ꮡt.dialConn(ctx, w.cm);
        var delivered = Ꮡw.tryDeliver(pc, err, new time.Time(nil));
        if (err == default! && (!delivered || (~pc).alt != default!)) {
            // pconn was not passed to w,
            // or it is HTTP/2 and can be shared.
            // Add to the idle connection pool.
            Ꮡt.putOrCloseIdleConn(pc);
        }
        if (err != default!) {
            Ꮡt.decConnsPerHost(w.key);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// decConnsPerHost decrements the per-host connection count for key,
// which may in turn give a different waiting goroutine permission to dial.
internal static void decConnsPerHost(this ж<Transport> Ꮡt, connectMethodKey key) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (t.MaxConnsPerHost <= 0) {
            return;
        }
        t.connsPerHostMu.Lock();
        ᒐd1 = true;
        nint n = t.connsPerHost[key];
        if (n == 0) {
            // Shouldn't happen, but if it does, the counting is buggy and could
            // easily lead to a silent deadlock, so report the problem loudly.
            throw panic("net/http: internal error: connCount underflow");
        }
        // Can we hand this count to a goroutine still waiting to dial?
        // (Some goroutines on the wait list may have timed out or
        // gotten a connection another way. If they're all gone,
        // we don't want to kick off any spurious dial operations.)
        {
            var q = t.connsPerHostWait[key]; if (q.len() > 0) {
                var done = false;
                while (q.len() > 0) {
                    var w = q.popFront();
                    if (w.waiting()) {
                        Ꮡt.startDialConnForLocked(w);
                        done = true;
                        break;
                    }
                }
                if (q.len() == 0){
                    delete(t.connsPerHostWait, key);
                } else {
                    // q is a value (like a slice), so we have to store
                    // the updated q back into the map.
                    t.connsPerHostWait[key] = q;
                }
                if (done) {
                    return;
                }
            }
        }
        // Otherwise, decrement the recorded count.
        {
            n--; if (n == 0){
                delete(t.connsPerHost, key);
            } else {
                t.connsPerHost[key] = n;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().connsPerHostMu.Unlock(); ᒐ.Run(); }
}

// Add TLS to a persistent connection, i.e. negotiate a TLS session. If pconn is already a TLS
// tunnel, this function establishes a nested TLS session inside the encrypted channel.
// The remote endpoint's name may be overridden by TLSClientConfig.ServerName.
[GoRecv] internal static error addTLS(this ref persistConn pconn, context.Context ctx, @string name, ж<httptrace.ClientTrace> Ꮡtrace) {
    ref var trace = ref Ꮡtrace.DerefOrNull();

    // Initiate TLS and check remote host name against certificate.
    var cfg = cloneTLSConfig((~pconn.t).TLSClientConfig);
    if ((~cfg).ServerName == ""u8) {
        cfg.Value.ServerName = name;
    }
    if (pconn.cacheKey.onlyH1) {
        cfg.Value.NextProtos = default!;
    }
    var plainConn = pconn.conn;
    var tlsConn = tls.Client(plainConn, cfg);
    var errc = new channel<error>(2);
    ж<time.Timer> timer = default!;                   // for canceling TLS handshake
    {
        var d = pconn.t.Value.TLSHandshakeTimeout; if (d != 0) {
            var errcʗ1 = errc;
            timer = time.AfterFunc(d, () => {
                errcʗ1.ᐸꟷ(new tlsHandshakeTimeoutError(nil));
            });
        }
    }
    var errcʗ2 = errc;
    var timerʗ1 = timer;
    var tlsConnʗ1 = tlsConn;
    goǃ(() => {
        if (Ꮡtrace != nil && Ꮡtrace.Value.TLSHandshakeStart != default!) {
            Ꮡtrace.Value.TLSHandshakeStart();
        }
        var err = tlsConnʗ1.HandshakeContext(ctx);
        if (timerʗ1 != nil) {
            timerʗ1.Stop();
        }
        errcʗ2.ᐸꟷ(err);
    });
    {
        var err = ᐸꟷ(errc); if (err != default!) {
            plainConn.Close();
            if (AreEqual(err, (new tlsHandshakeTimeoutError(nil)))) {
                // Now that we have closed the connection,
                // wait for the call to HandshakeContext to return.
                ᐸꟷ(errc);
            }
            if (Ꮡtrace != nil && trace.TLSHandshakeDone != default!) {
                trace.TLSHandshakeDone(new tlsꓸConnectionState(nil), err);
            }
            return err;
        }
    }
    ref var cs = ref heap<tlsꓸConnectionState>(out var Ꮡcs);
    cs = tlsConn.ConnectionState();
    if (Ꮡtrace != nil && trace.TLSHandshakeDone != default!) {
        trace.TLSHandshakeDone(cs, default!);
    }
    pconn.tlsState = Ꮡcs;
    pconn.conn = new tls.ConnжConn(tlsConn);
    return default!;
}

[GoType] partial interface erringRoundTripper {
    error RoundTripErr();
}

internal static Func<context.Context, time.Duration, (context.Context, Action)> testHookProxyConnectTimeout = context_package.WithTimeout;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string proxyAuthorizationˢ = "Proxy-Authorization"u8;
internal static readonly @string unknownStatusCodeˢ = "unknown status code"u8;
internal static readonly @string httpTransportDoesNotˢ = "http: Transport does not support unencrypted HTTP/2"u8;

internal static (ж<persistConn> pconn, error err) dialConn(this ж<Transport> Ꮡt, context.Context ctx, connectMethod cm) {
    ж<persistConn> pconn = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        pconn = Ꮡ(new persistConn(
            t: Ꮡt,
            cacheKey: cm.key(),
            reqch: new channel<requestAndChan>(1),
            writech: new channel<ΔwriteRequest>(1),
            closech: new channel<EmptyStruct>(0),
            writeErrCh: new channel<error>(1),
            writeLoopDone: new channel<EmptyStruct>(0)
        ));
        var trace = httptrace.ContextClientTrace(ctx);
        error wrapErr(error errΔ1) {
            if (cm.proxyURL != nil) {
                // Return a typed error, per Issue 16997
                return new net.OpErrorжerror(Ꮡ(new net.OpError(Op: "proxyconnect"u8, Net: "tcp"u8, Err: errΔ1)));
            }
            return errΔ1;
        }
        if (cm.scheme() == "https"u8 && t.hasCustomTLSDialer()){
            error errΔ2 = default!;
            (pconn.Value.conn, errΔ2) = t.customDialTLS(ctx, tcpˢ, cm.addr());
            if (errΔ2 != default!) {
                (pconn, err) = (default!, wrapErr(errΔ2)); goto ᒐdone;
            }
            {
                var (tc, ok) = (~pconn).conn._<ж<tls.Conn>>(ᐧ); if (ok) {
                    // Handshake here, in case DialTLS didn't. TLSNextProto below
                    // depends on it for knowing the connection state.
                    if (trace != nil && (~trace).TLSHandshakeStart != default!) {
                        (~trace).TLSHandshakeStart();
                    }
                    {
                        var errΔ3 = tc.HandshakeContext(ctx); if (errΔ3 != default!) {
                            goǃ(() => (~pconn).conn.Close());
                            if (trace != nil && (~trace).TLSHandshakeDone != default!) {
                                (~trace).TLSHandshakeDone(new tlsꓸConnectionState(nil), errΔ3);
                            }
                            (pconn, err) = (default!, errΔ3); goto ᒐdone;
                        }
                    }
                    ref var cs = ref heap<tlsꓸConnectionState>(out var Ꮡcs);
                    cs = tc.ConnectionState();
                    if (trace != nil && (~trace).TLSHandshakeDone != default!) {
                        (~trace).TLSHandshakeDone(cs, default!);
                    }
                    pconn.Value.tlsState = Ꮡcs;
                }
            }
        } else {
            var (conn, errΔ4) = t.dial(ctx, tcpˢ, cm.addr());
            if (errΔ4 != default!) {
                (pconn, err) = (default!, wrapErr(errΔ4)); goto ᒐdone;
            }
            pconn.Value.conn = conn;
            if (cm.scheme() == "https"u8) {
                @string firstTLSHost = default!;
                {
                    (firstTLSHost, _, errΔ4) = net.SplitHostPort(cm.addr()); if (errΔ4 != default!) {
                        (pconn, err) = (default!, wrapErr(errΔ4)); goto ᒐdone;
                    }
                }
                {
                    errΔ4 = pconn.addTLS(ctx, firstTLSHost, trace); if (errΔ4 != default!) {
                        (pconn, err) = (default!, wrapErr(errΔ4)); goto ᒐdone;
                    }
                }
            }
        }
        // Proxy setup.
        switch (ᐧ) {
        case {} when cm.proxyURL == nil: {
            break;
        }
        case {} when (~cm.proxyURL).Scheme == "socks5"u8 || (~cm.proxyURL).Scheme == "socks5h"u8: {
            var conn = pconn.Value.conn;
            var d = socksNewDialer(tcpˢ, // Do nothing. Not using a proxy.
 conn.RemoteAddr().String());
            {
                var u = cm.proxyURL.Value.User; if (u != nil) {
                    var auth = Ꮡ(new socksUsernamePassword(
                        Username: u.Username()
                    ));
                    (auth.Value.Password, _) = u.Password();
                    d.Value.AuthMethods = new socksAuthMethod[]{
                        socksAuthMethodNotRequired,
                        socksAuthMethodUsernamePassword
                    }.slice();
                    var authʗ1 = auth;
                                        d.Value.Authenticate = (context.Context p1, io.ReadWriter p2, socksAuthMethod p3) => authʗ1.Authenticate(p1, p2, p3);
                }
            }
            {
                var (_, errΔ8) = d.DialWithConn(ctx, conn, tcpˢ, cm.targetAddr); if (errΔ8 != default!) {
                    conn.Close();
                    (pconn, err) = (default!, errΔ8); goto ᒐdone;
                }
            }
            break;
        }
        case {} when cm.targetScheme == "http"u8: {
            pconn.Value.isProxy = true;
            {
                @string pa = cm.proxyAuth(); if (pa != ""u8) {
                    pconn.Value.mutateHeaderFunc = (ΔHeader h) => {
                        h.Set(proxyAuthorizationˢ, pa);
                    };
                }
            }
            break;
        }
        case {} when cm.targetScheme == "https"u8: {
            var conn = pconn.Value.conn;
            ΔHeader hdr = default!;
            if (t.GetProxyConnectHeader != default!){
                error errΔ9 = default!;
                (hdr, errΔ9) = t.GetProxyConnectHeader(ctx, cm.proxyURL, cm.targetAddr);
                if (errΔ9 != default!) {
                    conn.Close();
                    (pconn, err) = (default!, errΔ9); goto ᒐdone;
                }
            } else {
                hdr = t.ProxyConnectHeader;
            }
            if (hdr == default!) {
                hdr = new ΔHeader(0);
            }
            {
                @string pa = cm.proxyAuth(); if (pa != ""u8) {
                    hdr = hdr.Clone();
                    hdr.Set(proxyAuthorizationˢ, pa);
                }
            }
            var connectReq = Ꮡ(new Request(
                Method: "CONNECT"u8,
                URL: Ꮡ(new url.URL(Opaque: cm.targetAddr)),
                Host: cm.targetAddr,
                Header: hdr
            ));
            var (connectCtx, cancel) = testHookProxyConnectTimeout(ctx, // Set a (long) timeout here to make sure we don't block forever
 // and leak a goroutine if the connection stops replying after
 // the TCP connect.
 (time.Duration)(60000000000L));
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);
            var didReadResponse = new channel<EmptyStruct>(0); // closed after CONNECT write+read is done or fails
            ref var resp = ref heap<ж<Response>>(out var Ꮡresp);
            ref var errΔ10 = ref heap<error>(out var ᏑerrΔ10);      // write or read error
            var connʗ1 = conn;
            var connectReqʗ1 = connectReq;
            var didReadResponseʗ1 = didReadResponse;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    // Write the CONNECT request & read the response.
                    defer(ᴛ1 => builtin.close(ᴛ1), didReadResponseʗ1, ref ᒐ);
                    ᏑerrΔ10.ValueSlot = connectReqʗ1.Write(new net_ConnᴠWriter(connʗ1));
                    if (ᏑerrΔ10.ValueSlot != default!) {
                        return;
                    }
                    // Okay to use and discard buffered reader here, because
                    // TLS server will not speak until spoken to.
                    var br = bufio.NewReader(new net_ConnᴠReader(connʗ1));
                    (Ꮡresp.ValueSlot, ᏑerrΔ10.ValueSlot) = ReadResponse(br, connectReqʗ1);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            var selᴛ99 = connectCtx.Done();
            var selᴛ100 = didReadResponse;
            switch (select(ᐸꟷ(selᴛ99, ꓸꓸꓸ), ᐸꟷ(selᴛ100, ꓸꓸꓸ))) {
            case 0 when selᴛ99.ꟷᐳ(out _): {
                conn.Close();
                ᐸꟷ(didReadResponse);
                (pconn, err) = (default!, connectCtx.Err()); goto ᒐdone;
            }
            case 1 when selᴛ100.ꟷᐳ(out _): {
                break;
            }}
            if (errΔ10 != default!) {
                // resp or err now set
                conn.Close();
                (pconn, err) = (default!, errΔ10); goto ᒐdone;
            }
            if (t.OnProxyConnectResponse != default!) {
                errΔ10 = t.OnProxyConnectResponse(ctx, cm.proxyURL, connectReq, resp);
                if (errΔ10 != default!) {
                    conn.Close();
                    (pconn, err) = (default!, errΔ10); goto ᒐdone;
                }
            }
            if ((~resp).StatusCode != 200) {
                var (_, text, ok) = strings.Cut((~resp).Status, " "u8);
                conn.Close();
                if (!ok) {
                    (pconn, err) = (default!, errors.New(unknownStatusCodeˢ)); goto ᒐdone;
                }
                (pconn, err) = (default!, errors.New(text)); goto ᒐdone;
            }
            break;
        }}

        if (cm.proxyURL != nil && cm.targetScheme == "https"u8) {
            {
                var errΔ11 = pconn.addTLS(ctx, cm.tlsHost(), trace); if (errΔ11 != default!) {
                    (pconn, err) = (default!, errΔ11); goto ᒐdone;
                }
            }
        }
        // Possible unencrypted HTTP/2 with prior knowledge.
        var unencryptedHTTP2 = (~pconn).tlsState == nil && t.Protocols != nil && (~t.Protocols).UnencryptedHTTP2() && !(~t.Protocols).HTTP1();
        if (unencryptedHTTP2) {
            var (next, ok) = t.TLSNextProto[nextProtoUnencryptedHTTP2, ꟷ];
            if (!ok) {
                (pconn, err) = (default!, errors.New(httpTransportDoesNotˢ)); goto ᒐdone;
            }
            var alt = next(cm.targetAddr, unencryptedTLSConn((~pconn).conn));
            {
                var (e, okΔ1) = alt._<erringRoundTripper>(ᐧ); if (okΔ1) {
                    // pconn.conn was closed by next (http2configureTransports.upgradeFn).
                    (pconn, err) = (default!, e.RoundTripErr()); goto ᒐdone;
                }
            }
            (pconn, err) = (Ꮡ(new persistConn(t: Ꮡt, cacheKey: (~pconn).cacheKey, alt: alt)), default!); goto ᒐdone;
        }
        {
            var s = pconn.Value.tlsState; if (s != nil && (~s).NegotiatedProtocolIsMutual && (~s).NegotiatedProtocol != ""u8) {
                {
                    var (next, ok) = t.TLSNextProto[(~s).NegotiatedProtocol, ꟷ]; if (ok) {
                        var alt = next(cm.targetAddr, (~pconn).conn._<ж<tls.Conn>>());
                        {
                            var (e, okΔ1) = alt._<erringRoundTripper>(ᐧ); if (okΔ1) {
                                // pconn.conn was closed by next (http2configureTransports.upgradeFn).
                                (pconn, err) = (default!, e.RoundTripErr()); goto ᒐdone;
                            }
                        }
                        (pconn, err) = (Ꮡ(new persistConn(t: Ꮡt, cacheKey: (~pconn).cacheKey, alt: alt)), default!); goto ᒐdone;
                    }
                }
            }
        }
        pconn.Value.br = bufio.NewReaderSize(new persistConnжReader(pconn), t.readBufferSize());
        pconn.Value.bw = bufio.NewWriterSize(new persistConnWriter(pconn), t.writeBufferSize());
        goǃ(pconn.readLoop);
        goǃ(pconn.writeLoop);
        (pconn, err) = (pconn, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (pconn, err);
}

// persistConnWriter is the io.Writer written to by pc.bw.
// It accumulates the number of bytes written to the underlying conn,
// so the retry logic can determine whether any bytes made it across
// the wire.
// This is exactly 1 pointer field wide so it can go into an interface
// without allocation.
[GoType] partial struct persistConnWriter {
    internal ж<persistConn> pc;
}

internal static (nint n, error err) Write(this persistConnWriter w, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = (~w.pc).conn.Write(p);
    w.pc.Value.nwrite += (int64)n;
    return (n, err);
}

// ReadFrom exposes persistConnWriter's underlying Conn to io.Copy and if
// the Conn implements io.ReaderFrom, it can take advantage of optimizations
// such as sendfile.
internal static (int64 n, error err) ReadFrom(this persistConnWriter w, io.Reader r) {
    int64 n = default!;
    error err = default!;

    (n, err) = io.Copy(new net_ConnᴠWriter((~w.pc).conn), r);
    w.pc.Value.nwrite += n;
    return (n, err);
}

internal static io.ReaderFrom _ᴛ12ʗ = new persistConnWriterжReaderFrom(((ж<persistConnWriter>)nil));

// connectMethod is the map key (in its String form) for keeping persistent
// TCP connections alive for subsequent HTTP requests.
//
// A connect method may be of the following types:
//
//	connectMethod.key().String()      Description
//	------------------------------    -------------------------
//	|http|foo.com                     http directly to server, no proxy
//	|https|foo.com                    https directly to server, no proxy
//	|https,h1|foo.com                 https directly to server w/o HTTP/2, no proxy
//	http://proxy.com|https|foo.com    http to proxy, then CONNECT to foo.com
//	http://proxy.com|http             http to proxy, http to anywhere after that
//	socks5://proxy.com|http|foo.com   socks5 to proxy, then http to foo.com
//	socks5://proxy.com|https|foo.com  socks5 to proxy, then https to foo.com
//	https://proxy.com|https|foo.com   https to proxy, then CONNECT to foo.com
//	https://proxy.com|http            https to proxy, http to anywhere after that
[GoType] partial struct connectMethod {
    internal incomparable _;
    internal ж<url.URL> proxyURL; // nil for no proxy, else full proxy URL
    internal @string targetScheme;  // "http" or "https"
    // If proxyURL specifies an http or https proxy, and targetScheme is http (not https),
    // then targetAddr is not included in the connect method key, because the socket can
    // be reused for different targetAddr values.
    internal @string targetAddr;
    internal bool onlyH1; // whether to disable HTTP/2 and force HTTP/1
}

[GoRecv] internal static connectMethodKey key(this ref connectMethod cm) {
    @string proxyStr = ""u8;
    @string targetAddr = cm.targetAddr;
    if (cm.proxyURL != nil) {
        proxyStr = cm.proxyURL.String();
        if (((~cm.proxyURL).Scheme == "http"u8 || (~cm.proxyURL).Scheme == "https"u8) && cm.targetScheme == "http"u8) {
            targetAddr = ""u8;
        }
    }
    return new connectMethodKey(
        proxy: proxyStr,
        scheme: cm.targetScheme,
        addr: targetAddr,
        onlyH1: cm.onlyH1
    );
}

// scheme returns the first hop scheme: http, https, or socks5
[GoRecv] internal static @string scheme(this ref connectMethod cm) {
    if (cm.proxyURL != nil) {
        return (~cm.proxyURL).Scheme;
    }
    return cm.targetScheme;
}

// addr returns the first hop "host:port" to which we need to TCP connect.
[GoRecv] internal static @string addr(this ref connectMethod cm) {
    if (cm.proxyURL != nil) {
        return canonicalAddr(cm.proxyURL);
    }
    return cm.targetAddr;
}

// tlsHost returns the host name to match against the peer's
// TLS certificate.
[GoRecv] internal static @string tlsHost(this ref connectMethod cm) {
    @string h = cm.targetAddr;
    if (hasPort(h)) {
        h = h.slice(0, strings.LastIndex(h, ":"u8));
    }
    return h;
}

// connectMethodKey is the map key version of connectMethod, with a
// stringified proxy URL (or the empty string) instead of a pointer to
// a URL.
[GoType] partial struct connectMethodKey {
    internal @string proxy, scheme, addr;
    internal bool onlyH1;
}

internal static @string String(this connectMethodKey k) {
    // Only used by tests.
    @string h1 = default!;
    if (k.onlyH1) {
        h1 = ",h1"u8;
    }
    return fmt.Sprintf("%s|%s%s|%s"u8, k.proxy, k.scheme, h1, k.addr);
}

// persistConn wraps a connection, usually a persistent one
// (but may be used for non-keep-alive requests as well)
[GoType] partial struct persistConn {
    // alt optionally specifies the TLS NextProto RoundTripper.
    // This is used for HTTP/2 today and future protocols later.
    // If it's non-nil, the rest of the fields are unused.
    internal RoundTripper alt;
    internal ж<Transport> t;
    internal connectMethodKey cacheKey;
    internal net.Conn conn;
    internal ж<tlsꓸConnectionState> tlsState;
    internal ж<bufio.Reader> br;    // from conn
    internal ж<bufio.Writer> bw;    // to conn
    internal int64 nwrite;               // bytes written
    internal channel<requestAndChan> reqch; // written by roundTrip; read by readLoop
    internal channel<ΔwriteRequest> writech; // written by roundTrip; read by writeLoop
    internal channel<EmptyStruct> closech; // closed when conn closed
    internal bool isProxy;
    internal bool sawEOF;  // whether we've seen EOF from conn; owned by readLoop
    internal int64 readLimit; // bytes allowed to be read; owned by readLoop
    // writeErrCh passes the request write error (usually nil)
    // from the writeLoop goroutine to the readLoop which passes
    // it off to the res.Body reader, which then uses it to decide
    // whether or not a connection can be reused. Issue 7569.
    internal channel<error> writeErrCh;
    internal channel<EmptyStruct> writeLoopDone; // closed when write loop ends
    // Both guarded by Transport.idleMu:
    internal time.Time idleAt;   // time it last become idle
    internal ж<time.Timer> idleTimer; // holding an AfterFunc to close it
    internal sync.Mutex mu; // guards following fields
    internal nint numExpectedResponses;
    internal error closed; // set non-nil when conn is closed, before closech is closed
    internal error canceledErr; // set non-nil if conn is canceled
    internal bool broken;  // an error has happened on this connection; marked broken so it's not reused.
    internal bool reused;  // whether conn has had successful request/response and is being reused.
    // mutateHeaderFunc is an optional func to modify extra
    // headers on each outbound request before it's written. (the
    // original Request given to RoundTrip is not modified)
    internal Action<ΔHeader> mutateHeaderFunc;
}

[GoRecv] internal static int64 maxHeaderResponseSize(this ref persistConn pc) {
    {
        var v = pc.t.Value.MaxResponseHeaderBytes; if (v != 0) {
            return v;
        }
    }
    return ((int64)10 << (int)(20)); // conservative default; same as http2
}

[GoRecv] internal static (nint n, error err) Read(this ref persistConn pc, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (pc.readLimit <= 0) {
        return (0, fmt.Errorf("read limit of %d bytes exhausted"u8, pc.maxHeaderResponseSize()));
    }
    if ((int64)builtin.len(p) > pc.readLimit) {
        p = p.slice(0, (nint)(pc.readLimit));
    }
    (n, err) = pc.conn.Read(p);
    if (AreEqual(err, io.EOF)) {
        pc.sawEOF = true;
    }
    pc.readLimit -= (int64)n;
    return (n, err);
}

// isBroken reports whether this connection is in a known broken state.
internal static bool isBroken(this ж<persistConn> Ꮡpc) {
    ref var pc = ref Ꮡpc.DerefOrNull();

    pc.mu.Lock();
    var b = pc.closed != default!;
    pc.mu.Unlock();
    return b;
}

// canceled returns non-nil if the connection was closed due to
// CancelRequest or due to context cancellation.
internal static error canceled(this ж<persistConn> Ꮡpc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        pc.mu.Lock();
        ᒐd1 = true;
        return pc.canceledErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡpc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// isReused reports whether this connection has been used before.
internal static bool isReused(this ж<persistConn> Ꮡpc) {
    ref var pc = ref Ꮡpc.DerefOrNull();

    pc.mu.Lock();
    var r = pc.reused;
    pc.mu.Unlock();
    return r;
}

internal static void cancelRequest(this ж<persistConn> Ꮡpc, error err) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        pc.mu.Lock();
        ᒐd1 = true;
        pc.canceledErr = err;
        pc.closeLocked(errRequestCanceled);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡpc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// closeConnIfStillIdle closes the connection if it's still sitting idle.
// This is what's called by the persistConn's idleTimer, and is run in its
// own goroutine.
internal static void closeConnIfStillIdle(this ж<persistConn> Ꮡpc) {
    GoFrame ᒐ = default;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        var t = pc.t;
        t.of(Transport.ᏑidleMu).Lock();
        var tʗ1 = t;
        defer(tʗ1.of(Transport.ᏑidleMu).Unlock, ref ᒐ);
        {
            var (_, ok) = (~t).idleLRU.m[Ꮡpc, ꟷ]; if (!ok) {
                // Not idle.
                return;
            }
        }
        t.removeIdleConnLocked(Ꮡpc);
        Ꮡpc.close(errIdleConnTimeout);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// mapRoundTripError returns the appropriate error value for
// persistConn.roundTrip.
//
// The provided err is the first error that (*persistConn).roundTrip
// happened to receive from its select statement.
//
// The startBytesWritten value should be the value of pc.nwrite before the roundTrip
// started writing the request.
internal static error mapRoundTripError(this ж<persistConn> Ꮡpc, ж<transportRequest> Ꮡreq, int64 startBytesWritten, error err) {
    ref var pc = ref Ꮡpc.DerefOrNull();
    ref var req = ref Ꮡreq.DerefOrNull();

    if (err == default!) {
        return default!;
    }
    // Wait for the writeLoop goroutine to terminate to avoid data
    // races on callers who mutate the request on failure.
    //
    // When resc in pc.roundTrip and hence rc.ch receives a responseAndError
    // with a non-nil error it implies that the persistConn is either closed
    // or closing. Waiting on pc.writeLoopDone is hence safe as all callers
    // close closech which in turn ensures writeLoop returns.
    ᐸꟷ(pc.writeLoopDone);
    // If the request was canceled, that's better than network
    // failures that were likely the result of tearing down the
    // connection.
    {
        var cerr = Ꮡpc.canceled(); if (cerr != default!) {
            return cerr;
        }
    }
    // See if an error was set explicitly.
    req.mu.Lock();
    var reqErr = req.err;
    req.mu.Unlock();
    if (reqErr != default!) {
        return reqErr;
    }
    if (AreEqual(err, errServerClosedIdle)) {
        // Don't decorate
        return err;
    }
    {
        var (_, ok) = err._<transportReadFromServerError>(ᐧ); if (ok) {
            if (pc.nwrite == startBytesWritten) {
                return new nothingWrittenError(err);
            }
            // Don't decorate
            return err;
        }
    }
    if (Ꮡpc.isBroken()) {
        if (pc.nwrite == startBytesWritten) {
            return new nothingWrittenError(err);
        }
        return fmt.Errorf("net/http: HTTP/1.x transport connection broken: %w"u8, err);
    }
    return err;
}

// errCallerOwnsConn is an internal sentinel error used when we hand
// off a writable response.Body to the caller. We use this to prevent
// closing a net.Conn that is now owned by the caller.
internal static error errCallerOwnsConn = errors.New("read loop ending; caller owns writable underlying conn"u8);

internal static void readLoop(this ж<persistConn> Ꮡpc) {
    GoFrame ᒐ = default;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        ref var closeErr = ref heap<error>(out var ᏑcloseErr);
        closeErr = errReadLoopExiting; // default value, if not changed below
        defer(() => {
            Ꮡpc.close(ᏑcloseErr.ValueSlot);
            Ꮡpc.Value.t.removeIdleConn(Ꮡpc);
        }, ref ᒐ);
        bool tryPutIdleConn(ж<transportRequest> treq) {
            var trace = treq.Value.trace;
            {
                var err = Ꮡpc.Value.t.tryPutIdleConn(Ꮡpc); if (err != default!) {
                    ᏑcloseErr.ValueSlot = err;
                    if (trace != nil && (~trace).PutIdleConn != default! && !AreEqual(err, errKeepAlivesDisabled)) {
                        (~trace).PutIdleConn(err);
                    }
                    return false;
                }
            }
            if (trace != nil && (~trace).PutIdleConn != default!) {
                (~trace).PutIdleConn(default!);
            }
            return true;
        }
        // eofc is used to block caller goroutines reading from Response.Body
        // at EOF until this goroutines has (potentially) added the connection
        // back to the idle pool.
        var eofc = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), eofc, ref ᒐ); // unblock reader on errors
        // Read this once, before loop starts. (to avoid races in tests)
        testHookMu.Lock();
        var testHookReadLoopBeforeNextReadΔ1 = http_package.testHookReadLoopBeforeNextRead;
        testHookMu.Unlock();
        var alive = true;
        while (alive) {
            pc.readLimit = pc.maxHeaderResponseSize();
            var (_, err) = pc.br.Peek(1);
            pc.mu.Lock();
            if (pc.numExpectedResponses == 0) {
                pc.readLoopPeekFailLocked(err);
                pc.mu.Unlock();
                return;
            }
            pc.mu.Unlock();
            var rc = ᐸꟷ(pc.reqch);
            var trace = rc.treq.Value.trace;
            ж<Response> resp = default!;
            if (err == default!){
                (resp, err) = pc.readResponse(rc, trace);
            } else {
                err = new transportReadFromServerError(err);
                closeErr = err;
            }
            if (err != default!) {
                if (pc.readLimit <= 0) {
                    err = fmt.Errorf("net/http: server response headers exceeded %d bytes; aborted"u8, pc.maxHeaderResponseSize());
                }
                var selᴛ101 = rc.ch.ᐸꟷ(new responseAndError(err: err), ꓸꓸꓸ);
                var selᴛ102 = rc.callerGone;
                switch (select(selᴛ101, ᐸꟷ(selᴛ102, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1 when selᴛ102.ꟷᐳ(out _): {
                    return;
                }}
                return;
            }
            pc.readLimit = maxInt64; // effectively no limit for response bodies
            pc.mu.Lock();
            pc.numExpectedResponses--;
            pc.mu.Unlock();
            var bodyWritable = resp.bodyIsWritable();
            var hasBody = (~(~rc.treq).Request).Method != "HEAD"u8 && (~resp).ContentLength != 0;
            if ((~resp).Close || (~(~rc.treq).Request).Close || (~resp).StatusCode <= 199 || bodyWritable) {
                // Don't do keep-alive on error if either party requested a close
                // or we get an unexpected informational (1xx) response.
                // StatusCode 100 is already handled above.
                alive = false;
            }
            if (!hasBody || bodyWritable) {
                // Put the idle conn back into the pool before we send the response
                // so if they process it quickly and make another request, they'll
                // get this same conn. But we use the unbuffered channel 'rc'
                // to guarantee that persistConn.roundTrip got out of its select
                // potentially waiting for this persistConn to close.
                alive = alive && !pc.sawEOF && Ꮡpc.wroteRequest() && tryPutIdleConn(rc.treq);
                if (bodyWritable) {
                    closeErr = errCallerOwnsConn;
                }
                var selᴛ103 = rc.ch.ᐸꟷ(new responseAndError(res: resp), ꓸꓸꓸ);
                var selᴛ104 = rc.callerGone;
                switch (select(selᴛ103, ᐸꟷ(selᴛ104, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1 when selᴛ104.ꟷᐳ(out _): {
                    return;
                }}
                (~rc.treq).cancel(errRequestDone);
                // Now that they've read from the unbuffered channel, they're safely
                // out of the select that also waits on this goroutine to die, so
                // we're allowed to exit now if needed (if alive is false)
                testHookReadLoopBeforeNextReadΔ1();
                continue;
            }
            var waitForBodyRead = new channel<bool>(2);
                var eofcʗ1 = eofc;
                var waitForBodyReadʗ1 = waitForBodyRead;

                var eofcʗ2 = eofc;
                var waitForBodyReadʗ2 = waitForBodyRead;
            var body = Ꮡ(new bodyEOFSignal(
                body: (~resp).Body,
                earlyCloseFn: () => {
                    waitForBodyReadʗ1.ᐸꟷ(false);
                    ᐸꟷ(eofcʗ1); // will be closed by deferred call at the end of the function
                    return default!;
                },
                fn: (error errΔ1) => {
                    var isEOF = AreEqual(errΔ1, io.EOF);
                    waitForBodyReadʗ2.ᐸꟷ(isEOF);
                    if (isEOF){
                        ᐸꟷ(eofcʗ2); // see comment above eofc declaration
                    } else 
                    if (errΔ1 != default!) {
                        {
                            var cerr = Ꮡpc.canceled(); if (cerr != default!) {
                                return cerr;
                            }
                        }
                    }
                    return errΔ1;
                }
            ));
            resp.Value.Body = new bodyEOFSignalжReadCloser(body);
            if (rc.addedGzip && ascii.EqualFold((~resp).Header.Get(contentEncodingˢ), gzipˢ)) {
                resp.Value.Body = new gzipReaderжReadCloser(Ꮡ(new gzipReader(body: body)));
                (~resp).Header.Del(contentEncodingˢ);
                (~resp).Header.Del(contentLengthˢ);
                resp.Value.ContentLength = -1;
                resp.Value.Uncompressed = true;
            }
            var selᴛ105 = rc.ch.ᐸꟷ(new responseAndError(res: resp), ꓸꓸꓸ);
            var selᴛ106 = rc.callerGone;
            switch (select(selᴛ105, ᐸꟷ(selᴛ106, ꓸꓸꓸ))) {
            case 0: {
                break;
            }
            case 1 when selᴛ106.ꟷᐳ(out _): {
                return;
            }}
            // Before looping back to the top of this function and peeking on
            // the bufio.Reader, wait for the caller goroutine to finish
            // reading the response body. (or for cancellation or death)
            var selᴛ107 = waitForBodyRead;
            var selᴛ108 = (~rc.treq).ctx.Done();
            var selᴛ109 = pc.closech;
            switch (select(ᐸꟷ(selᴛ107, ꓸꓸꓸ), ᐸꟷ(selᴛ108, ꓸꓸꓸ), ᐸꟷ(selᴛ109, ꓸꓸꓸ))) {
            case 0 when selᴛ107.ꟷᐳ(out var bodyEOF): {
                alive = alive && bodyEOF && !pc.sawEOF && Ꮡpc.wroteRequest() && tryPutIdleConn(rc.treq);
                if (bodyEOF) {
                    eofc.ᐸꟷ(new EmptyStruct());
                }
                break;
            }
            case 1 when selᴛ108.ꟷᐳ(out _): {
                alive = false;
                Ꮡpc.cancelRequest(context_package.Cause((~rc.treq).ctx));
                break;
            }
            case 2 when selᴛ109.ꟷᐳ(out _): {
                alive = false;
                break;
            }}
            (~rc.treq).cancel(errRequestDone);
            testHookReadLoopBeforeNextReadΔ1();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoRecv] internal static void readLoopPeekFailLocked(this ref persistConn pc, error peekErr) {
    if (pc.closed != default!) {
        return;
    }
    {
        nint n = pc.br.Buffered(); if (n > 0) {
            var (buf, _) = pc.br.Peek(n);
            if (is408Message(buf)){
                pc.closeLocked(errServerClosedIdle);
                return;
            } else {
                log.Printf("Unsolicited response received on idle HTTP channel starting with %q; err=%v"u8, buf, peekErr);
            }
        }
    }
    if (AreEqual(peekErr, io.EOF)){
        // common case.
        pc.closeLocked(errServerClosedIdle);
    } else {
        pc.closeLocked(fmt.Errorf("readLoopPeekFailLocked: %w"u8, peekErr));
    }
}

// is408Message reports whether buf has the prefix of an
// HTTP 408 Request Timeout response.
// See golang.org/issue/32310.
internal static bool is408Message(slice<byte> buf) {
    if (builtin.len(buf) < builtin.len("HTTP/1.x 408")) {
        return false;
    }
    if (((sstring)(buf[..7])) != "HTTP/1."u8) {
        return false;
    }
    return ((sstring)(buf[8..12])) == " 408"u8;
}

// readResponse reads an HTTP response (or two, in the case of "Expect:
// 100-continue") from the server. It returns the final non-100 one.
// trace is optional.
[GoRecv] internal static (ж<Response> resp, error err) readResponse(this ref persistConn pc, requestAndChan rc, ж<httptrace.ClientTrace> Ꮡtrace) {
    ж<Response> resp = default!;
    error err = default!;

    ref var trace = ref Ꮡtrace.DerefOrNull();
    if (Ꮡtrace != nil && trace.GotFirstResponseByte != default!) {
        {
            var (peek, errΔ1) = pc.br.Peek(1); if (errΔ1 == default! && builtin.len(peek) == 1) {
                trace.GotFirstResponseByte();
            }
        }
    }
    var continueCh = rc.continueCh;
    while (ᐧ) {
        (resp, err) = ReadResponse(pc.br, (~rc.treq).Request);
        if (err != default!) {
            return (resp, err);
        }
        nint resCode = resp.Value.StatusCode;
        if (continueCh != default! && resCode == StatusContinue) {
            if (Ꮡtrace != nil && trace.Got100Continue != default!) {
                trace.Got100Continue();
            }
            continueCh.ᐸꟷ(new EmptyStruct());
            continueCh = default!;
        }
        var is1xx = 100 <= resCode && resCode <= 199;
        // treat 101 as a terminal status, see issue 26161
        var is1xxNonTerminal = is1xx && resCode != StatusSwitchingProtocols;
        if (is1xxNonTerminal) {
            if (Ꮡtrace != nil && trace.Got1xxResponse != default!) {
                {
                    var errΔ2 = trace.Got1xxResponse(resCode, ((textproto.MIMEHeader)(map<@string, slice<@string>>)(~resp).Header)); if (errΔ2 != default!) {
                        return (default!, errΔ2);
                    }
                }
                // If the 1xx response was delivered to the user,
                // then they're responsible for limiting the number of
                // responses. Reset the header limit.
                //
                // If the user didn't examine the 1xx response, then we
                // limit the size of all headers (including both 1xx
                // and the final response) to maxHeaderResponseSize.
                pc.readLimit = pc.maxHeaderResponseSize(); // reset the limit
            }
            continue;
        }
        break;
    }
    if (resp.isProtocolSwitch()) {
        resp.Value.Body = new io_ReadWriteCloserᴠReadCloser(newReadWriteCloserBody(pc.br, new net_ConnᴠReadWriteCloser(pc.conn)));
    }
    if (continueCh != default!) {
        // We send an "Expect: 100-continue" header, but the server
        // responded with a terminal status and no 100 Continue.
        //
        // If we're going to keep using the connection, we need to send the request body.
        // Tell writeLoop to skip sending the body if we're going to close the connection,
        // or to send it otherwise.
        //
        // The case where we receive a 101 Switching Protocols response is a bit
        // ambiguous, since we don't know what protocol we're switching to.
        // Conceivably, it's one that doesn't need us to send the body.
        // Given that we'll send the body if ExpectContinueTimeout expires,
        // be consistent and always send it if we aren't closing the connection.
        if ((~resp).Close || (~(~rc.treq).Request).Close){
            builtin.close(continueCh); // don't send the body; the connection will close
        } else {
            continueCh.ᐸꟷ(new EmptyStruct()); // send the body
        }
    }
    resp.Value.TLS = pc.tlsState;
    return (resp, err);
}

// waitForContinue returns the function to block until
// any response, timeout or connection close. After any of them,
// the function returns a bool which indicates if the body should be sent.
internal static Func<bool> waitForContinue(this ж<persistConn> Ꮡpc, /*<-*/channel<EmptyStruct> continueCh) {
    if (continueCh == default!) {
        return default!;
    }
    var continueChʗ1 = continueCh;
    return () => {
        GoFrame ᒐ = default;
        try {
            var timer = time.NewTimer((~Ꮡpc.Value.t).ExpectContinueTimeout);
            var timerʗ1 = timer;
            defer(() => timerʗ1.Stop(), ref ᒐ);
            var selᴛ110 = continueChʗ1;
            var selᴛ111 = (~timer).C;
            var selᴛ112 = Ꮡpc.Value.closech;
            switch (select(ᐸꟷ(selᴛ110, ꓸꓸꓸ), ᐸꟷ(selᴛ111, ꓸꓸꓸ), ᐸꟷ(selᴛ112, ꓸꓸꓸ))) {
            case 0 when selᴛ110.ꟷᐳ(out var _, out var ok): {
                return ok;
            }
            case 1 when selᴛ111.ꟷᐳ(out _): {
                return true;
            }
            case 2 when selᴛ112.ꟷᐳ(out _): {
                return false;
            }}
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    };
}

internal static io.ReadWriteCloser newReadWriteCloserBody(ж<bufio.Reader> Ꮡbr, io.ReadWriteCloser rwc) {
    ref var br = ref Ꮡbr.DerefOrNull();

    var body = Ꮡ(new readWriteCloserBody(ReadWriteCloser: rwc));
    if (br.Buffered() != 0) {
        body.Value.br = Ꮡbr;
    }
    return new readWriteCloserBodyжReadWriteCloser(body);
}

// readWriteCloserBody is the Response.Body type used when we want to
// give users write access to the Body through the underlying
// connection (TCP, unless using custom dialers). This is then
// the concrete type for a Response.Body on the 101 Switching
// Protocols response, as used by WebSockets, h2c, etc.
[GoType] partial struct readWriteCloserBody {
    internal incomparable _;
    internal ж<bufio.Reader> br; // used until empty
    public io_package.ReadWriteCloser ReadWriteCloser;
}

// Go method set entry for the promoted 'ReadWriteCloser.Close()' - provided ONLY by the embedded
// interface field in *readWriteCloserBody's method set; see the pointer-only satisfaction record.
internal static error Close(this readWriteCloserBody recvᴛ) => recvᴛ.ReadWriteCloser.Close();

// Go method set entry for the promoted 'ReadWriteCloser.Write()' - provided ONLY by the embedded
// interface field in *readWriteCloserBody's method set; see the pointer-only satisfaction record.
internal static (nint, error) Write(this readWriteCloserBody recvᴛ, slice<byte> p) => recvᴛ.ReadWriteCloser.Write(p);

[GoRecv] internal static (nint n, error err) Read(this ref readWriteCloserBody b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (b.br != nil) {
        {
            nint nΔ1 = b.br.Buffered(); if (builtin.len(p) > nΔ1) {
                p = p.slice(0, nΔ1);
            }
        }
        (n, err) = b.br.Read(p);
        if (b.br.Buffered() == 0) {
            b.br = default!;
        }
        return (n, err);
    }
    return b.ReadWriteCloser.Read(p);
}

// nothingWrittenError wraps a write errors which ended up writing zero bytes.
[GoType] partial struct nothingWrittenError {
    internal error error;
}

internal static error Unwrap(this nothingWrittenError nwe) {
    return nwe.error;
}

internal static void writeLoop(this ж<persistConn> Ꮡpc) {
    GoFrame ᒐ = default;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        defer(ᴛ1 => builtin.close(ᴛ1), Ꮡpc.Value.writeLoopDone, ref ᒐ);
        while (ᐧ) {
            var selᴛ113 = pc.writech;
            var selᴛ114 = pc.closech;
            switch (select(ᐸꟷ(selᴛ113, ꓸꓸꓸ), ᐸꟷ(selᴛ114, ꓸꓸꓸ))) {
            case 0 when selᴛ113.ꟷᐳ(out var wr): {
                var startBytesWritten = pc.nwrite;
                var err = (~wr.req).Request.write(new bufio_WriterжWriter(pc.bw), pc.isProxy, (~wr.req).extra, Ꮡpc.waitForContinue(wr.continueCh));
                {
                    var (bre, ok) = err._<requestBodyReadError>(ᐧ); if (ok) {
                        err = bre.error;
                        // Errors reading from the user's
                        // Request.Body are high priority.
                        // Set it here before sending on the
                        // channels below or calling
                        // pc.close() which tears down
                        // connections and causes other
                        // errors.
                        wr.req.setError(err);
                    }
                }
                if (err == default!) {
                    err = pc.bw.Flush();
                }
                if (err != default!) {
                    if (pc.nwrite == startBytesWritten) {
                        err = new nothingWrittenError(err);
                    }
                }
                pc.writeErrCh.ᐸꟷ(err); // to the body reader, which might recycle us
                wr.ch.ᐸꟷ(err); // to the roundTrip function
                if (err != default!) {
                    Ꮡpc.close(err);
                    return;
                }
                break;
            }
            case 1 when selᴛ114.ꟷᐳ(out _): {
                return;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// maxWriteWaitBeforeConnReuse is how long the a Transport RoundTrip
// will wait to see the Request's Body.Write result after getting a
// response from the server. See comments in (*persistConn).wroteRequest.
//
// In tests, we set this to a large value to avoid flakiness from inconsistent
// recycling of connections.
internal static ж<time.Duration> ᏑmaxWriteWaitBeforeConnReuse = new StandardBox<time.Duration>(50 * time.Millisecond);
internal static ref time.Duration maxWriteWaitBeforeConnReuse => ref ᏑmaxWriteWaitBeforeConnReuse.Value;

// wroteRequest is a check before recycling a connection that the previous write
// (from writeLoop above) happened and was successful.
internal static bool wroteRequest(this ж<persistConn> Ꮡpc) {
    GoFrame ᒐ = default;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        var selᴛ115 = pc.writeErrCh;
        switch (trySelect(ᐸꟷ(selᴛ115, ꓸꓸꓸ))) {
        case 0 when selᴛ115.ꟷᐳ(out var err): {
            return err == default!;
        }
        default: {
            var t = time.NewTimer(maxWriteWaitBeforeConnReuse);
            var tʗ1 = t;
            defer(() => tʗ1.Stop(), ref ᒐ);
            var selᴛ116 = pc.writeErrCh;
            var selᴛ117 = (~t).C;
            switch (select(ᐸꟷ(selᴛ116, ꓸꓸꓸ), ᐸꟷ(selᴛ117, ꓸꓸꓸ))) {
            case 0 when selᴛ116.ꟷᐳ(out var errΔ1): {
                return errΔ1 == default!;
            }
            case 1 when selᴛ117.ꟷᐳ(out _): {
                return false;
            }}
            return default!;
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Common case: the write happened well before the response, so
// avoid creating a timer.
// Rare case: the request was written in writeLoop above but
// before it could send to pc.writeErrCh, the reader read it
// all, processed it, and called us here. In this case, give the
// write goroutine a bit of time to finish its send.
//
// Less rare case: We also get here in the legitimate case of
// Issue 7569, where the writer is still writing (or stalled),
// but the server has already replied. In this case, we don't
// want to wait too long, and we want to return false so this
// connection isn't re-used.

// responseAndError is how the goroutine reading from an HTTP/1 server
// communicates with the goroutine doing the RoundTrip.
[GoType] partial struct responseAndError {
    internal incomparable _;
    internal ж<Response> res; // else use this response (see res method)
    internal error err;
}

[GoType] partial struct requestAndChan {
    internal incomparable _;
    internal ж<transportRequest> treq;
    internal channel<responseAndError> ch; // unbuffered; always send in select on callerGone
    // whether the Transport (as opposed to the user client code)
    // added the Accept-Encoding gzip header. If the Transport
    // set it, only then do we transparently decode the gzip.
    internal bool addedGzip;
    // Optional blocking chan for Expect: 100-continue (for send).
    // If the request has an "Expect: 100-continue" header and
    // the server responds 100 Continue, readLoop send a value
    // to writeLoop via this chan.
    internal channel/*<-*/<EmptyStruct> continueCh = channel/*<-*/<EmptyStruct>.SendOnly;
    internal /*<-*/channel<EmptyStruct> callerGone = /*<-*/channel<EmptyStruct>.RecvOnly; // closed when roundTrip caller has returned
}

// A writeRequest is sent by the caller's goroutine to the
// writeLoop's goroutine to write a request while the read loop
// concurrently waits on both the write response and the server's
// reply.
[GoType] partial struct ΔwriteRequest {
    internal ж<transportRequest> req;
    internal channel/*<-*/<error> ch = channel/*<-*/<error>.SendOnly;
    // Optional blocking chan for Expect: 100-continue (for receive).
    // If not nil, writeLoop blocks sending request body until
    // it receives from this chan.
    internal /*<-*/channel<EmptyStruct> continueCh = /*<-*/channel<EmptyStruct>.RecvOnly;
}

// httpTimeoutError represents a timeout.
// It implements net.Error and wraps context.DeadlineExceeded.
[GoType] partial struct timeoutError {
    internal @string err;
}

[GoRecv] internal static @string Error(this ref timeoutError e) {
    return e.err;
}

[GoRecv] internal static bool Timeout(this ref timeoutError e) {
    return true;
}

[GoRecv] internal static bool Temporary(this ref timeoutError e) {
    return true;
}

[GoRecv] internal static bool Is(this ref timeoutError e, error err) {
    return AreEqual(err, context_package.DeadlineExceeded);
}

internal static error errTimeout = new timeoutErrorжerror(Ꮡ(new timeoutError("net/http: timeout awaiting response headers"u8)));

// errRequestCanceled is set to be identical to the one from h2 to facilitate
// testing.
internal static error errRequestCanceled;
internal static void initᴛerrRequestCanceled() { errRequestCanceled = http2errRequestCanceled; }

internal static error errRequestCanceledConn = errors.New("net/http: request canceled while waiting for connection"u8); // TODO: unify?

// errRequestDone is used to cancel the round trip Context after a request is successfully done.
// It should not be seen by the user.
internal static error errRequestDone = errors.New("net/http: request completed"u8);

internal static void nop() {
}

// testHooks. Always non-nil.
internal static ж<Action> ᏑtestHookEnterRoundTrip = new StandardBox<Action>(nop);
internal static ref Action testHookEnterRoundTrip => ref ᏑtestHookEnterRoundTrip.ValueSlot;

internal static Action testHookWaitResLoop = nop;

internal static ж<Action> ᏑtestHookRoundTripRetried = new StandardBox<Action>(nop);
internal static ref Action testHookRoundTripRetried => ref ᏑtestHookRoundTripRetried.ValueSlot;

internal static Action testHookPrePendingDial = nop;

internal static Action testHookPostPendingDial = nop;

internal static sync.Locker testHookMu = new fakeLocker(nil); // guards following

internal static Action testHookReadLoopBeforeNextRead = nop;

internal static (ж<Response> resp, error err) roundTrip(this ж<persistConn> Ꮡpc, ж<transportRequest> Ꮡreq) {
    ж<Response> resp = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();
        ref var req = ref Ꮡreq.DerefOrNull();

        testHookEnterRoundTrip();
        pc.mu.Lock();
        pc.numExpectedResponses++;
        var headerFn = pc.mutateHeaderFunc;
        pc.mu.Unlock();
        if (headerFn != default!) {
            headerFn(req.extraHeaders());
        }
        // Ask for a compressed version if the caller didn't set their
        // own value for Accept-Encoding. We only attempt to
        // uncompress the gzip stream if we were the layer that
        // requested it.
        var requestedGzip = false;
        if (!(~pc.t).DisableCompression && req.Header.Get(acceptEncodingˢ) == ""u8 && req.Header.Get(rangeˢ) == ""u8 && req.Method != "HEAD"u8) {
            // Request gzip only, not deflate. Deflate is ambiguous and
            // not as universally supported anyway.
            // See: https://zlib.net/zlib_faq.html#faq39
            //
            // Note that we don't request this for HEAD requests,
            // due to a bug in nginx:
            //   https://trac.nginx.org/nginx/ticket/358
            //   https://golang.org/issue/5522
            //
            // We don't request gzip if the request is for a range, since
            // auto-decoding a portion of a gzipped document will just fail
            // anyway. See https://golang.org/issue/8923
            requestedGzip = true;
            req.extraHeaders().Set(acceptEncodingˢ, gzipˢ);
        }
        channel<EmptyStruct> continueCh = default!;
        if (req.ProtoAtLeast(1, 1) && req.Body != default! && req.expectsContinue()) {
            continueCh = new channel<EmptyStruct>(1);
        }
        if ((~pc.t).DisableKeepAlives && !req.wantsClose() && !isProtocolSwitchHeader(req.Header)) {
            req.extraHeaders().Set(connectionˢ, closeˢ);
        }
        var gone = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), gone, ref ᒐ);
        const bool debugRoundTrip = false;
        // Write the request concurrently with waiting for a response,
        // in case the server decides to reply before reading our full
        // request body.
        var startBytesWritten = pc.nwrite;
        var writeErrCh = new channel<error>(1);
        pc.writech.ᐸꟷ(new ΔwriteRequest(Ꮡreq, writeErrCh, continueCh));
        var resc = new channel<responseAndError>(0);
        pc.reqch.ᐸꟷ(new requestAndChan(
            treq: Ꮡreq,
            ch: resc,
            addedGzip: requestedGzip,
            continueCh: continueCh,
            callerGone: gone
        ));
        (ж<Response>, error) handleResponse(responseAndError re) {
            if ((re.res == nil) == (re.err == default!)) {
                throw panic(fmt.Sprintf("internal error: exactly one of res or err should be set; nil=%v"u8, re.res == nil));
            }
            if (debugRoundTrip) {
                Ꮡreq.Value.logf("resc recv: %p, %T/%#v"u8, re.res.OrTypedNil(), re.err, re.err);
            }
            if (re.err != default!) {
                return (default!, Ꮡpc.mapRoundTripError(Ꮡreq, startBytesWritten, re.err));
            }
            return (re.res, default!);
        }
        /*<-*/channel<time.Time> respHeaderTimer = /*<-*/channel<time.Time>.RecvOnly;
        var ctxDoneChan = req.ctx.Done();
        var pcClosed = pc.closech;
        while (ᐧ) {
            testHookWaitResLoop();
            var selᴛ118 = writeErrCh;
            var selᴛ119 = pcClosed;
            var selᴛ120 = respHeaderTimer;
            var selᴛ121 = resc;
            var selᴛ122 = ctxDoneChan;
            switch (select(ᐸꟷ(selᴛ118, ꓸꓸꓸ), ᐸꟷ(selᴛ119, ꓸꓸꓸ), ᐸꟷ(selᴛ120, ꓸꓸꓸ), ᐸꟷ(selᴛ121, ꓸꓸꓸ), ᐸꟷ(selᴛ122, ꓸꓸꓸ))) {
            case 0 when selᴛ118.ꟷᐳ(out var errΔ1): {
                if (debugRoundTrip) {
                    req.logf("writeErrCh recv: %T/%#v"u8, errΔ1, errΔ1);
                }
                if (errΔ1 != default!) {
                    Ꮡpc.close(fmt.Errorf("write error: %w"u8, errΔ1));
                    (resp, err) = (default!, Ꮡpc.mapRoundTripError(Ꮡreq, startBytesWritten, errΔ1)); goto ᒐdone;
                }
                {
                    var d = pc.t.Value.ResponseHeaderTimeout; if (d > 0) {
                        if (debugRoundTrip) {
                            req.logf("starting timer for %v"u8, d);
                        }
                        var timer = time.NewTimer(d);
                        var timerʗ1 = timer;
                        defer(() => timerʗ1.Stop(), ref ᒐ); // prevent leaks
                        respHeaderTimer = timer.Value.C;
                    }
                }
                break;
            }
            case 1 when selᴛ119.ꟷᐳ(out _): {
                var selᴛ123 = resc;
                switch (trySelect(ᐸꟷ(selᴛ123, ꓸꓸꓸ))) {
                case 0 when selᴛ123.ꟷᐳ(out var re): {
                    (resp, err) = handleResponse(re); goto ᒐdone;
                }
                default: {
                    break;
                }}
                if (debugRoundTrip) {
                    // The pconn closing raced with the response to the request,
                    // probably after the server wrote a response and immediately
                    // closed the connection. Use the response.
                    req.logf("closech recv: %T %#v"u8, pc.closed, pc.closed);
                }
                (resp, err) = (default!, Ꮡpc.mapRoundTripError(Ꮡreq, startBytesWritten, pc.closed)); goto ᒐdone;
            }
            case 2 when selᴛ120.ꟷᐳ(out _): {
                if (debugRoundTrip) {
                    req.logf("timeout waiting for response headers."u8);
                }
                Ꮡpc.close(errTimeout);
                (resp, err) = (default!, errTimeout); goto ᒐdone;
            }
            case 3 when selᴛ121.ꟷᐳ(out var re): {
                (resp, err) = handleResponse(re); goto ᒐdone;
            }
            case 4 when selᴛ122.ꟷᐳ(out _): {
                var selᴛ124 = resc;
                switch (trySelect(ᐸꟷ(selᴛ124, ꓸꓸꓸ))) {
                case 0 when selᴛ124.ꟷᐳ(out var reΔ1): {
                    (resp, err) = handleResponse(reΔ1); goto ᒐdone;
                }
                default: {
                    break;
                }}
                Ꮡpc.cancelRequest(context_package.Cause(req.ctx));
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (resp, err);
}

// readLoop is responsible for canceling req.ctx after
// it reads the response body. Check for a response racing
// the context close, and use the response if available.

// tLogKey is a context WithValue key for test debugging contexts containing
// a t.Logf func. See export_test.go's Request.WithT method.
[GoType] partial struct tLogKey {
}

[GoRecv] internal static void logf(this ref transportRequest tr, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    {
        var (logf, ok) = tr.Request.Context().Value(new tLogKey(nil))._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok) {
            logf(time.Now().Format(time.RFC3339Nano) + ": "u8 + format, args.ꓸꓸꓸ);
        }
    }
}

// markReused marks this connection as having been successfully used for a
// request and response.
internal static void markReused(this ж<persistConn> Ꮡpc) {
    ref var pc = ref Ꮡpc.DerefOrNull();

    pc.mu.Lock();
    pc.reused = true;
    pc.mu.Unlock();
}

// close closes the underlying TCP connection and closes
// the pc.closech channel.
//
// The provided err is only for testing and debugging; in normal
// circumstances it should never be seen by users.
internal static void close(this ж<persistConn> Ꮡpc, error err) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var pc = ref Ꮡpc.DerefOrNull();

        pc.mu.Lock();
        ᒐd1 = true;
        pc.closeLocked(err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡpc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static void closeLocked(this ref persistConn pc, error err) {
    if (err == default!) {
        throw panic("nil error");
    }
    pc.broken = true;
    if (pc.closed == default!) {
        pc.closed = err;
        pc.t.decConnsPerHost(pc.cacheKey);
        // Close HTTP/1 (pc.alt == nil) connection.
        // HTTP/2 closes its connection itself.
        if (pc.alt == default!) {
            if (!AreEqual(err, errCallerOwnsConn)) {
                pc.conn.Close();
            }
            builtin.close(pc.closech);
        }
    }
    pc.mutateHeaderFunc = default!;
}

internal static map<@string, @string> portMap = new map<@string, @string>{
    ["http"u8] = "80"u8,
    ["https"u8] = "443"u8,
    ["socks5"u8] = "1080"u8,
    ["socks5h"u8] = "1080"u8
};

internal static @string idnaASCIIFromURL(ж<url.URL> Ꮡurl) {
    ref var urlΔ1 = ref Ꮡurl.DerefOrNull();

    @string addr = urlΔ1.Hostname();
    {
        var (v, err) = idnaASCII(addr); if (err == default!) {
            addr = v;
        }
    }
    return addr;
}

// canonicalAddr returns url.Host but always with a ":port" suffix.
internal static @string canonicalAddr(ж<url.URL> Ꮡurl) {
    ref var urlΔ1 = ref Ꮡurl.DerefOrNull();

    @string port = urlΔ1.Port();
    if (port == ""u8) {
        port = portMap[urlΔ1.Scheme];
    }
    return net.JoinHostPort(idnaASCIIFromURL(Ꮡurl), port);
}

// bodyEOFSignal is used by the HTTP/1 transport when reading response
// bodies to make sure we see the end of a response body before
// proceeding and reading on the connection again.
//
// It wraps a ReadCloser but runs fn (if non-nil) at most
// once, right before its final (error-producing) Read or Close call
// returns. fn should return the new error to return from Read or Close.
//
// If earlyCloseFn is non-nil and Close is called before io.EOF is
// seen, earlyCloseFn is called instead of fn, and its return value is
// the return value from Close.
[GoType] partial struct bodyEOFSignal {
    internal io.ReadCloser body;
    internal sync.Mutex mu;        // guards following 4 fields
    internal bool closed;              // whether Close has been called
    internal error rerr;             // sticky Read error
    internal Func<error, error> fn; // err will be nil on Read io.EOF
    internal Func<error> earlyCloseFn;       // optional alt Close func used if io.EOF not seen
}

internal static error errReadOnClosedResBody = errors.New("http: read on closed response body"u8);

internal static (nint n, error err) Read(this ж<bodyEOFSignal> Ꮡes, slice<byte> p) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var es = ref Ꮡes.DerefOrNull();

        es.mu.Lock();
        var (closed, rerr) = (es.closed, es.rerr);
        es.mu.Unlock();
        if (closed) {
            (n, err) = (0, errReadOnClosedResBody); goto ᒐdone;
        }
        if (rerr != default!) {
            (n, err) = (0, rerr); goto ᒐdone;
        }
        (n, err) = es.body.Read(p);
        if (err != default!) {
            es.mu.Lock();
            defer(Ꮡes.of(bodyEOFSignal.Ꮡmu).Unlock, ref ᒐ);
            if (es.rerr == default!) {
                es.rerr = err;
            }
            err = es.condfn(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static error Close(this ж<bodyEOFSignal> Ꮡes) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var es = ref Ꮡes.DerefOrNull();

        es.mu.Lock();
        ᒐd1 = true;
        if (es.closed) {
            return default!;
        }
        es.closed = true;
        if (es.earlyCloseFn != default! && !AreEqual(es.rerr, io.EOF)) {
            return es.earlyCloseFn();
        }
        var err = es.body.Close();
        return es.condfn(err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡes.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// caller must hold es.mu.
[GoRecv] internal static error condfn(this ref bodyEOFSignal es, error err) {
    if (es.fn == default!) {
        return err;
    }
    err = es.fn(err);
    es.fn = default!;
    return err;
}

// gzipReader wraps a response body so it can lazily
// call gzip.NewReader on the first call to Read
[GoType] partial struct gzipReader {
    internal incomparable _;
    internal ж<bodyEOFSignal> body; // underlying HTTP/1 response body framing
    internal ж<gzip.Reader> zr; // lazily-initialized gzip reader
    internal error zerr;          // any error from gzip.NewReader; sticky
}

[GoRecv] internal static (nint n, error err) Read(this ref gzipReader gz, slice<byte> p) {
    error err = default!;

    if (gz.zr == nil) {
        if (gz.zerr == default!) {
            (gz.zr, gz.zerr) = gzip.NewReader(new bodyEOFSignalжReader(gz.body));
        }
        if (gz.zerr != default!) {
            return (0, gz.zerr);
        }
    }
    gz.body.of(bodyEOFSignal.Ꮡmu).Lock();
    if ((~gz.body).closed) {
        err = errReadOnClosedResBody;
    }
    gz.body.of(bodyEOFSignal.Ꮡmu).Unlock();
    if (err != default!) {
        return (0, err);
    }
    return gz.zr.Read(p);
}

[GoRecv] internal static error Close(this ref gzipReader gz) {
    return gz.body.Close();
}

[GoType] partial struct tlsHandshakeTimeoutError {
}

internal static bool Timeout(this tlsHandshakeTimeoutError _) {
    return true;
}

internal static bool Temporary(this tlsHandshakeTimeoutError _) {
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netHttpTlsHandshakeˢ = "net/http: TLS handshake timeout"u8;

internal static @string Error(this tlsHandshakeTimeoutError _) {
    return netHttpTlsHandshakeˢ;
}

// fakeLocker is a sync.Locker which does nothing. It's used to guard
// test-only fields when not under test, to avoid runtime atomic
// overhead.
[GoType] partial struct fakeLocker {
}

internal static void Lock(this fakeLocker _) {
}

internal static void Unlock(this fakeLocker _) {
}

// cloneTLSConfig returns a shallow clone of cfg, or a new zero tls.Config if
// cfg is nil. This is safe to call even if cfg is in active use by a TLS
// client or server.
//
// cloneTLSConfig should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneTLSConfig
internal static ж<tls.Config> cloneTLSConfig(ж<tls.Config> Ꮡcfg) {
    if (Ꮡcfg == nil) {
        return Ꮡ(new tls.Config(nil));
    }
    return Ꮡcfg.Clone();
}

[GoType] partial struct connLRU {
    internal ж<list.List> ll; // list.Element.Value type is of *persistConn
    internal map<ж<persistConn>, ж<list.Element>> m;
}

// add adds pc to the head of the linked list.
[GoRecv] internal static void add(this ref connLRU cl, ж<persistConn> Ꮡpc) {
    if (cl.ll == nil) {
        cl.ll = list.New();
        cl.m = new map<ж<persistConn>, ж<list.Element>>();
    }
    var ele = cl.ll.PushFront(Ꮡpc.OrTypedNil());
    {
        var (_, ok) = cl.m[Ꮡpc, ꟷ]; if (ok) {
            throw panic("persistConn was already in LRU");
        }
    }
    cl.m[Ꮡpc] = ele;
}

[GoRecv] internal static ж<persistConn> removeOldest(this ref connLRU cl) {
    var ele = cl.ll.Back();
    var pc = (~ele).Value._<ж<persistConn>>();
    cl.ll.Remove(ele);
    delete(cl.m, pc);
    return pc;
}

// remove removes pc from cl.
[GoRecv] internal static void remove(this ref connLRU cl, ж<persistConn> Ꮡpc) {
    {
        var (ele, ok) = cl.m[Ꮡpc, ꟷ]; if (ok) {
            cl.ll.Remove(ele);
            delete(cl.m, Ꮡpc);
        }
    }
}

// len returns the number of items in the cache.
[GoRecv] internal static nint len(this ref connLRU cl) {
    return builtin.len(cl.m);
}

} // end http_package
