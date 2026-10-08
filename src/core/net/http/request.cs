// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP Request reading and parsing.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using maps = maps_package;
using mime = mime_package;
using multipart = go.mime.multipart_package;
using httptrace = go.net.http.httptrace_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using urlpkg = go.net.url_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using idna = vendor.golang.org.x.net.idna_package;
using crypto;
using encoding;
using go.mime;
using go.net;
using go.net.http;
using go.net.http.@internal;
using vendor.golang.org.x.net;
using vendor.golang.org.x.net.http;

partial class http_package {

internal static UntypedInt defaultMaxMemory => /* 32 << 20 */ 33554432; // 32 MB

// ErrMissingFile is returned by FormFile when the provided file field name
// is either not present in the request or not a file field.
public static error ErrMissingFile = errors.New("http: no such file"u8);

// ProtocolError represents an HTTP protocol error.
//
// Deprecated: Not all errors in the http package related to protocol errors
// are of type ProtocolError.
partial struct ProtocolError {
    public @string ErrorString;
}

public static @string Error(this ref ProtocolError pe) {
    return pe.ErrorString;
}

// Is lets http.ErrNotSupported match errors.ErrUnsupported.
public static bool Is(this ж<ProtocolError> Ꮡpe, error err) {
    return Ꮡpe == ErrNotSupported && AreEqual(err, errors.ErrUnsupported);
}

public static ж<ProtocolError> ErrNotSupported = Ꮡ(new ProtocolError("feature not supported"u8));
public static ж<ProtocolError> ErrUnexpectedTrailer = Ꮡ(new ProtocolError("trailer header without chunked transfer encoding"u8));
public static ж<ProtocolError> ErrMissingBoundary = Ꮡ(new ProtocolError("no multipart boundary param in Content-Type"u8));
public static ж<ProtocolError> ErrNotMultipart = Ꮡ(new ProtocolError("request Content-Type isn't multipart/form-data"u8));
public static ж<ProtocolError> ErrHeaderTooLong = Ꮡ(new ProtocolError("header too long"u8));
public static ж<ProtocolError> ErrShortBody = Ꮡ(new ProtocolError("entity body too short"u8));
public static ж<ProtocolError> ErrMissingContentLength = Ꮡ(new ProtocolError("missing ContentLength in HEAD response"u8));

internal static error badStringError(@string what, @string val) {
    return fmt.Errorf("%s %q"u8, what, val);
}

// not in Header map anyway
// Headers that Request.Write handles itself and should be skipped.
internal static map<@string, bool> reqWriteExcludeHeader = new map<@string, bool>{
    ["Host"u8] = true,
    ["User-Agent"u8] = true,
    ["Content-Length"u8] = true,
    ["Transfer-Encoding"u8] = true,
    ["Trailer"u8] = true
};

// A Request represents an HTTP request received by a server
// or to be sent by a client.
//
// The field semantics differ slightly between client and server
// usage. In addition to the notes on the fields below, see the
// documentation for [Request.Write] and [RoundTripper].
partial struct Request {
    // Method specifies the HTTP method (GET, POST, PUT, etc.).
    // For client requests, an empty string means GET.
    public @string Method;
    // URL specifies either the URI being requested (for server
    // requests) or the URL to access (for client requests).
    //
    // For server requests, the URL is parsed from the URI
    // supplied on the Request-Line as stored in RequestURI.  For
    // most requests, fields other than Path and RawQuery will be
    // empty. (See RFC 7230, Section 5.3)
    //
    // For client requests, the URL's Host specifies the server to
    // connect to, while the Request's Host field optionally
    // specifies the Host header value to send in the HTTP
    // request.
    public ж<urlpkg.URL> URL;
    // The protocol version for incoming server requests.
    //
    // For client requests, these fields are ignored. The HTTP
    // client code always uses either HTTP/1.1 or HTTP/2.
    // See the docs on Transport for details.
    public @string Proto; // "HTTP/1.0"
    public nint ProtoMajor;   // 1
    public nint ProtoMinor;   // 0
    // Header contains the request header fields either received
    // by the server or to be sent by the client.
    //
    // If a server received a request with header lines,
    //
    //	Host: example.com
    //	accept-encoding: gzip, deflate
    //	Accept-Language: en-us
    //	fOO: Bar
    //	foo: two
    //
    // then
    //
    //	Header = map[string][]string{
    //		"Accept-Encoding": {"gzip, deflate"},
    //		"Accept-Language": {"en-us"},
    //		"Foo": {"Bar", "two"},
    //	}
    //
    // For incoming requests, the Host header is promoted to the
    // Request.Host field and removed from the Header map.
    //
    // HTTP defines that header names are case-insensitive. The
    // request parser implements this by using CanonicalHeaderKey,
    // making the first character and any characters following a
    // hyphen uppercase and the rest lowercase.
    //
    // For client requests, certain headers such as Content-Length
    // and Connection are automatically written when needed and
    // values in Header may be ignored. See the documentation
    // for the Request.Write method.
    public ΔHeader Header;
    // Body is the request's body.
    //
    // For client requests, a nil body means the request has no
    // body, such as a GET request. The HTTP Client's Transport
    // is responsible for calling the Close method.
    //
    // For server requests, the Request Body is always non-nil
    // but will return EOF immediately when no body is present.
    // The Server will close the request body. The ServeHTTP
    // Handler does not need to.
    //
    // Body must allow Read to be called concurrently with Close.
    // In particular, calling Close should unblock a Read waiting
    // for input.
    public io.ReadCloser Body;
    // GetBody defines an optional func to return a new copy of
    // Body. It is used for client requests when a redirect requires
    // reading the body more than once. Use of GetBody still
    // requires setting Body.
    //
    // For server requests, it is unused.
    public Func<(io.ReadCloser, error)> GetBody;
    // ContentLength records the length of the associated content.
    // The value -1 indicates that the length is unknown.
    // Values >= 0 indicate that the given number of bytes may
    // be read from Body.
    //
    // For client requests, a value of 0 with a non-nil Body is
    // also treated as unknown.
    public int64 ContentLength;
    // TransferEncoding lists the transfer encodings from outermost to
    // innermost. An empty list denotes the "identity" encoding.
    // TransferEncoding can usually be ignored; chunked encoding is
    // automatically added and removed as necessary when sending and
    // receiving requests.
    public slice<@string> TransferEncoding;
    // Close indicates whether to close the connection after
    // replying to this request (for servers) or after sending this
    // request and reading its response (for clients).
    //
    // For server requests, the HTTP server handles this automatically
    // and this field is not needed by Handlers.
    //
    // For client requests, setting this field prevents re-use of
    // TCP connections between requests to the same hosts, as if
    // Transport.DisableKeepAlives were set.
    public bool Close;
    // For server requests, Host specifies the host on which the
    // URL is sought. For HTTP/1 (per RFC 7230, section 5.4), this
    // is either the value of the "Host" header or the host name
    // given in the URL itself. For HTTP/2, it is the value of the
    // ":authority" pseudo-header field.
    // It may be of the form "host:port". For international domain
    // names, Host may be in Punycode or Unicode form. Use
    // golang.org/x/net/idna to convert it to either format if
    // needed.
    // To prevent DNS rebinding attacks, server Handlers should
    // validate that the Host header has a value for which the
    // Handler considers itself authoritative. The included
    // ServeMux supports patterns registered to particular host
    // names and thus protects its registered Handlers.
    //
    // For client requests, Host optionally overrides the Host
    // header to send. If empty, the Request.Write method uses
    // the value of URL.Host. Host may contain an international
    // domain name.
    public @string Host;
    // Form contains the parsed form data, including both the URL
    // field's query parameters and the PATCH, POST, or PUT form data.
    // This field is only available after ParseForm is called.
    // The HTTP client ignores Form and uses Body instead.
    public urlpkg.Values Form;
    // PostForm contains the parsed form data from PATCH, POST
    // or PUT body parameters.
    //
    // This field is only available after ParseForm is called.
    // The HTTP client ignores PostForm and uses Body instead.
    public urlpkg.Values PostForm;
    // MultipartForm is the parsed multipart form, including file uploads.
    // This field is only available after ParseMultipartForm is called.
    // The HTTP client ignores MultipartForm and uses Body instead.
    public ж<multipart.Form> MultipartForm;
    // Trailer specifies additional headers that are sent after the request
    // body.
    //
    // For server requests, the Trailer map initially contains only the
    // trailer keys, with nil values. (The client declares which trailers it
    // will later send.)  While the handler is reading from Body, it must
    // not reference Trailer. After reading from Body returns EOF, Trailer
    // can be read again and will contain non-nil values, if they were sent
    // by the client.
    //
    // For client requests, Trailer must be initialized to a map containing
    // the trailer keys to later send. The values may be nil or their final
    // values. The ContentLength must be 0 or -1, to send a chunked request.
    // After the HTTP request is sent the map values can be updated while
    // the request body is read. Once the body returns EOF, the caller must
    // not mutate Trailer.
    //
    // Few HTTP clients, servers, or proxies support HTTP trailers.
    public ΔHeader Trailer;
    // RemoteAddr allows HTTP servers and other software to record
    // the network address that sent the request, usually for
    // logging. This field is not filled in by ReadRequest and
    // has no defined format. The HTTP server in this package
    // sets RemoteAddr to an "IP:port" address before invoking a
    // handler.
    // This field is ignored by the HTTP client.
    public @string RemoteAddr;
    // RequestURI is the unmodified request-target of the
    // Request-Line (RFC 7230, Section 3.1.1) as sent by the client
    // to a server. Usually the URL field should be used instead.
    // It is an error to set this field in an HTTP client request.
    public @string RequestURI;
    // TLS allows HTTP servers and other software to record
    // information about the TLS connection on which the request
    // was received. This field is not filled in by ReadRequest.
    // The HTTP server in this package sets the field for
    // TLS-enabled connections before invoking a handler;
    // otherwise it leaves the field nil.
    // This field is ignored by the HTTP client.
    public ж<tlsꓸConnectionState> TLS;
    // Cancel is an optional channel whose closure indicates that the client
    // request should be regarded as canceled. Not all implementations of
    // RoundTripper may support Cancel.
    //
    // For server requests, this field is not applicable.
    //
    // Deprecated: Set the Request's context with NewRequestWithContext
    // instead. If a Request's Cancel field and context are both
    // set, it is undefined whether Cancel is respected.
    public /*<-*/channel<EmptyStruct> Cancel = /*<-*/channel<EmptyStruct>.RecvOnly;
    // Response is the redirect response which caused this request
    // to be created. This field is only populated during client
    // redirects.
    public ж<Response> Response;
    // Pattern is the [ServeMux] pattern that matched the request.
    // It is empty if the request was not matched against a pattern.
    public @string Pattern;
    // ctx is either the client or server context. It should only
    // be modified via copying the whole Request using Clone or WithContext.
    // It is unexported to prevent people from using Context wrong
    // and mutating the contexts held by callers of the same request.
    internal context.Context ctx;
    // The following fields are for requests matched by ServeMux.
    internal ж<pattern> pat;       // the pattern that matched
    internal slice<@string> matches;    // values for the matching wildcards in pat
    internal map<@string, @string> otherValues; // for calls to SetPathValue that don't match a wildcard
}

// Context returns the request's context. To change the context, use
// [Request.Clone] or [Request.WithContext].
//
// The returned context is always non-nil; it defaults to the
// background context.
//
// For outgoing client requests, the context controls cancellation.
//
// For incoming server requests, the context is canceled when the
// client's connection closes, the request is canceled (with HTTP/2),
// or when the ServeHTTP method returns.
public static context.Context Context(this ref Request r) {
    if (r.ctx != default!) {
        return r.ctx;
    }
    return context_package.Background();
}

// WithContext returns a shallow copy of r with its context changed
// to ctx. The provided ctx must be non-nil.
//
// For outgoing client request, the context controls the entire
// lifetime of a request and its response: obtaining a connection,
// sending the request, and reading the response headers and body.
//
// To create a new request with a context, use [NewRequestWithContext].
// To make a deep copy of a request with a new context, use [Request.Clone].
public static ж<Request> WithContext(this ref Request r, context.Context ctx) {
    if (ctx == default!) {
        throw panic("nil context");
    }
    var r2 = @new<Request>();
    r2.Value = r;
    r2.Value.ctx = ctx;
    return r2;
}

// Clone returns a deep copy of r with its context changed to ctx.
// The provided ctx must be non-nil.
//
// Clone only makes a shallow copy of the Body field.
//
// For an outgoing client request, the context controls the entire
// lifetime of a request and its response: obtaining a connection,
// sending the request, and reading the response headers and body.
public static ж<Request> Clone(this ref Request r, context.Context ctx) {
    if (ctx == default!) {
        throw panic("nil context");
    }
    var r2 = @new<Request>();
    r2.Value = r;
    r2.Value.ctx = ctx;
    r2.Value.URL = cloneURL(r.URL);
    r2.Value.Header = r.Header.Clone();
    r2.Value.Trailer = r.Trailer.Clone();
    {
        var s = r.TransferEncoding; if (s != default!) {
            var s2 = new slice<@string>(builtin.len(s));
            copy(s2, s);
            r2.Value.TransferEncoding = s2;
        }
    }
    r2.Value.Form = cloneURLValues(r.Form);
    r2.Value.PostForm = cloneURLValues(r.PostForm);
    r2.Value.MultipartForm = cloneMultipartForm(r.MultipartForm);
    // Copy matches and otherValues. See issue 61410.
    {
        var s = r.matches; if (s != default!) {
            var s2 = new slice<@string>(builtin.len(s));
            copy(s2, s);
            r2.Value.matches = s2;
        }
    }
    r2.Value.otherValues = maps.Clone<map<@string, @string>, @string, @string>(r.otherValues);
    return r2;
}

// ProtoAtLeast reports whether the HTTP protocol used
// in the request is at least major.minor.
public static bool ProtoAtLeast(this ref Request r, nint major, nint minor) {
    return r.ProtoMajor > major || r.ProtoMajor == major && r.ProtoMinor >= minor;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userAgentˢ2 = "User-Agent"u8;

// UserAgent returns the client's User-Agent, if sent in the request.
public static @string UserAgent(this ref Request r) {
    return r.Header.Get(userAgentˢ2);
}

// Cookies parses and returns the HTTP cookies sent with the request.
public static slice<ж<ΔCookie>> Cookies(this ref Request r) {
    return readCookies(r.Header, ""u8);
}

// CookiesNamed parses and returns the named HTTP cookies sent with the request
// or an empty slice if none matched.
public static slice<ж<ΔCookie>> CookiesNamed(this ref Request r, @string name) {
    if (name == ""u8) {
        return new ж<ΔCookie>[]{}.slice();
    }
    return readCookies(r.Header, name);
}

// ErrNoCookie is returned by Request's Cookie method when a cookie is not found.
public static error ErrNoCookie = errors.New("http: named cookie not present"u8);

// Cookie returns the named cookie provided in the request or
// [ErrNoCookie] if not found.
// If multiple cookies match the given name, only one cookie will
// be returned.
public static (ж<ΔCookie>, error) Cookie(this ref Request r, @string name) {
    if (name == ""u8) {
        return (default!, ErrNoCookie);
    }
    foreach (var (_, c) in readCookies(r.Header, name)) {
        return (c, default!);
    }
    return (default!, ErrNoCookie);
}

// AddCookie adds a cookie to the request. Per RFC 6265 section 5.4,
// AddCookie does not attach more than one [Cookie] header field. That
// means all cookies, if any, are written into the same line,
// separated by semicolon.
// AddCookie only sanitizes c's name and value, and does not sanitize
// a Cookie header already present in the request.
public static void AddCookie(this ref Request r, ж<ΔCookie> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    @string s = fmt.Sprintf("%s=%s"u8, sanitizeCookieName(c.Name), sanitizeCookieValue(c.Value, c.Quoted));
    {
        @string cΔ1 = r.Header.Get(cookieˢ); if (cΔ1 != ""u8){
            r.Header.Set(cookieˢ, cΔ1 + "; "u8 + s);
        } else {
            r.Header.Set(cookieˢ, s);
        }
    }
}

// Referer returns the referring URL, if sent in the request.
//
// Referer is misspelled as in the request itself, a mistake from the
// earliest days of HTTP.  This value can also be fetched from the
// [Header] map as Header["Referer"]; the benefit of making it available
// as a method is that the compiler can diagnose programs that use the
// alternate (correct English) spelling req.Referrer() but cannot
// diagnose programs that use Header["Referrer"].
public static @string Referer(this ref Request r) {
    return r.Header.Get(refererˢ);
}

// multipartByReader is a sentinel value.
// Its presence in Request.MultipartForm indicates that parsing of the request
// body has been handed off to a MultipartReader instead of ParseMultipartForm.
internal static ж<multipart.Form> multipartByReader = Ꮡ(new multipart.Form(
    Value: new map<@string, slice<@string>>(),
    File: new map<@string, slice<ж<multipart.FileHeader>>>()
));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpMultipartReaderˢ = "http: MultipartReader called twice"u8;
internal static readonly @string httpMultipartHandledByˢ = "http: multipart handled by ParseMultipartForm"u8;

// MultipartReader returns a MIME multipart reader if this is a
// multipart/form-data or a multipart/mixed POST request, else returns nil and an error.
// Use this function instead of [Request.ParseMultipartForm] to
// process the request body as a stream.
public static (ж<multipart.Reader>, error) MultipartReader(this ref Request r) {
    if (r.MultipartForm == multipartByReader) {
        return (default!, errors.New(httpMultipartReaderˢ));
    }
    if (r.MultipartForm != nil) {
        return (default!, errors.New(httpMultipartHandledByˢ));
    }
    r.MultipartForm = multipartByReader;
    return r.multipartReader(true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingFormBodyˢ = "missing form body"u8;
internal static readonly @string boundaryˢ = "boundary"u8;

internal static (ж<multipart.Reader>, error) multipartReader(this ref Request r, bool allowMixed) {
    @string v = r.Header.Get(contentTypeˢ);
    if (v == ""u8) {
        return (default!, new ProtocolErrorжerror(ErrNotMultipart));
    }
    if (r.Body == default!) {
        return (default!, errors.New(missingFormBodyˢ));
    }
    var (d, @params, err) = mime.ParseMediaType(v);
    if (err != default! || !(d == "multipart/form-data"u8 || allowMixed && d == "multipart/mixed"u8)) {
        return (default!, new ProtocolErrorжerror(ErrNotMultipart));
    }
    var (boundary, ok) = @params[boundaryˢ, ꟷ];
    if (!ok) {
        return (default!, new ProtocolErrorжerror(ErrMissingBoundary));
    }
    return (multipart.NewReader(r.Body, boundary), default!);
}

// isH2Upgrade reports whether r represents the http2 "client preface"
// magic string.
internal static bool isH2Upgrade(this ref Request r) {
    return r.Method == "PRI"u8 && builtin.len(r.Header) == 0 && (~r.URL).Path == "*"u8 && r.Proto == "HTTP/2.0"u8;
}

// Return value if nonempty, def otherwise.
internal static @string valueOrDefault(@string value, @string def) {
    if (value != ""u8) {
        return value;
    }
    return def;
}

// NOTE: This is not intended to reflect the actual Go version being used.
// It was changed at the time of Go 1.1 release because the former User-Agent
// had ended up blocked by some intrusion detection systems.
// See https://codereview.appspot.com/7532043.
internal static readonly @string defaultUserAgent = "Go-http-client/1.1"u8;

// Write writes an HTTP/1.1 request, which is the header and body, in wire format.
// This method consults the following fields of the request:
//
//	Host
//	URL
//	Method (defaults to "GET")
//	Header
//	ContentLength
//	TransferEncoding
//	Body
//
// If Body is present, Content-Length is <= 0 and [Request.TransferEncoding]
// hasn't been set to "identity", Write adds "Transfer-Encoding:
// chunked" to the header. Body is closed after it is sent.
public static error Write(this ж<Request> Ꮡr, io.Writer w) {
    return Ꮡr.write(w, false, default!, default!);
}

// WriteProxy is like [Request.Write] but writes the request in the form
// expected by an HTTP proxy. In particular, [Request.WriteProxy] writes the
// initial Request-URI line of the request with an absolute URI, per
// section 5.3 of RFC 7230, including the scheme and host.
// In either case, WriteProxy also writes a Host header, using
// either r.Host or r.URL.Host.
public static error WriteProxy(this ж<Request> Ꮡr, io.Writer w) {
    return Ꮡr.write(w, true, default!, default!);
}

// errMissingHost is returned by Write when there is no Host or URL present in
// the Request.
internal static error errMissingHost = errors.New("http: Request.Write on Request with no Host or URL set"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpInvalidHostHeaderˢ = "http: invalid Host header"u8;
internal static readonly @string netHttpCanTWriteControlˢ = "net/http: can't write control character in Request.URL"u8;

// extraHeaders may be nil
// waitForContinue may be nil
// always closes body
internal static error /*err*/ write(this ж<Request> Ꮡr, io.Writer w, bool usingProxy, ΔHeader extraHeaders, Func<bool> waitForContinue) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        var trace = httptrace.ContextClientTrace(r.Context());
        if (trace != nil && (~trace).WroteRequest != default!) {
            var traceʗ1 = trace;
            defer(() => {
                (~traceʗ1).WroteRequest(new httptrace.WroteRequestInfo(
                    Err: Ꮡerr.ValueSlot
                ));
            }, ref ᒐ);
        }
        var closed = false;
        defer(() => {
            if (closed) {
                return;
            }
            {
                var closeErr = Ꮡr.Value.closeBody(); if (closeErr != default! && Ꮡerr.ValueSlot == default!) {
                    Ꮡerr.ValueSlot = closeErr;
                }
            }
        }, ref ᒐ);
        // Find the target host. Prefer the Host: header, but if that
        // is not given, use the host from the request URL.
        //
        // Clean the host, in case it arrives with unexpected stuff in it.
        @string host = r.Host;
        if (host == ""u8) {
            if (r.URL == nil) {
                err = errMissingHost; goto ᒐdone;
            }
            host = r.URL.Value.Host;
        }
        (host, err) = httpguts.PunycodeHostPort(host);
        if (err != default!) {
            goto ᒐdone;
        }
        // Validate that the Host header is a valid header in general,
        // but don't validate the host itself. This is sufficient to avoid
        // header or request smuggling via the Host field.
        // The server can (and will, if it's a net/http server) reject
        // the request if it doesn't consider the host valid.
        if (!httpguts.ValidHostHeader(host)) {
            // Historically, we would truncate the Host header after '/' or ' '.
            // Some users have relied on this truncation to convert a network
            // address such as Unix domain socket path into a valid, ignored
            // Host header (see https://go.dev/issue/61431).
            //
            // We don't preserve the truncation, because sending an altered
            // header field opens a smuggling vector. Instead, zero out the
            // Host header entirely if it isn't valid. (An empty Host is valid;
            // see RFC 9112 Section 3.2.)
            //
            // Return an error if we're sending to a proxy, since the proxy
            // probably can't do anything useful with an empty Host header.
            if (!usingProxy){
                host = ""u8;
            } else {
                err = errors.New(httpInvalidHostHeaderˢ); goto ᒐdone;
            }
        }
        // According to RFC 6874, an HTTP client, proxy, or other
        // intermediary must remove any IPv6 zone identifier attached
        // to an outgoing URI.
        host = removeZone(host);
        @string ruri = r.URL.RequestURI();
        if (usingProxy && (~r.URL).Scheme != ""u8 && (~r.URL).Opaque == ""u8){
            ruri = (~r.URL).Scheme + "://"u8 + host + ruri;
        } else 
        if (r.Method == "CONNECT"u8 && (~r.URL).Path == ""u8) {
            // CONNECT requests normally give just the host and port, not a full URL.
            ruri = host;
            if ((~r.URL).Opaque != ""u8) {
                ruri = r.URL.Value.Opaque;
            }
        }
        if (stringContainsCTLByte(ruri)) {
            err = errors.New(netHttpCanTWriteControlˢ); goto ᒐdone;
        }
        // TODO: validate r.Method too? At least it's less likely to
        // come from an attacker (more likely to be a constant in
        // code).
        // Wrap the writer in a bufio Writer if it's not already buffered.
        // Don't always call NewWriter, as that forces a bytes.Buffer
        // and other small bufio Writers to have a minimum 4k buffer
        // size.
        ж<bufio.Writer> bw = default!;
        {
            var (_, ok) = w._<io.ByteWriter>(ᐧ); if (!ok) {
                bw = bufio.NewWriter(w);
                w = new bufio_WriterжWriter(bw);
            }
        }
        (_, err) = fmt.Fprintf(w, "%s %s HTTP/1.1\r\n"u8, valueOrDefault(r.Method, getˢ), ruri);
        if (err != default!) {
            goto ᒐdone;
        }
        // Header lines
        (_, err) = fmt.Fprintf(w, "Host: %s\r\n"u8, host);
        if (err != default!) {
            goto ᒐdone;
        }
        if (trace != nil && (~trace).WroteHeaderField != default!) {
            (~trace).WroteHeaderField(hostˢ, new @string[]{host}.slice());
        }
        // Use the defaultUserAgent unless the Header contains one, which
        // may be blank to not send the header.
        @string userAgent = defaultUserAgent;
        if (r.Header.has(userAgentˢ2)) {
            userAgent = r.Header.Get(userAgentˢ2);
        }
        if (userAgent != ""u8) {
            userAgent = headerNewlineToSpace.Replace(userAgent);
            userAgent = textproto.TrimString(userAgent);
            (_, err) = fmt.Fprintf(w, "User-Agent: %s\r\n"u8, userAgent);
            if (err != default!) {
                goto ᒐdone;
            }
            if (trace != nil && (~trace).WroteHeaderField != default!) {
                (~trace).WroteHeaderField(userAgentˢ2, new @string[]{userAgent}.slice());
            }
        }
        // Process Body,ContentLength,Close,Trailer
        (var tw, err) = newTransferWriter(Ꮡr.OrTypedNil());
        if (err != default!) {
            goto ᒐdone;
        }
        err = tw.writeHeader(w, trace);
        if (err != default!) {
            goto ᒐdone;
        }
        err = r.Header.writeSubset(w, reqWriteExcludeHeader, trace);
        if (err != default!) {
            goto ᒐdone;
        }
        if (extraHeaders != default!) {
            err = extraHeaders.write(w, trace);
            if (err != default!) {
                goto ᒐdone;
            }
        }
        (_, err) = io.WriteString(w, "\r\n"u8);
        if (err != default!) {
            goto ᒐdone;
        }
        if (trace != nil && (~trace).WroteHeaders != default!) {
            (~trace).WroteHeaders();
        }
        // Flush and wait for 100-continue if expected.
        if (waitForContinue != default!) {
            {
                var (bwΔ1, ok) = w._<ж<bufio.Writer>>(ᐧ); if (ok) {
                    err = bwΔ1.Flush();
                    if (err != default!) {
                        goto ᒐdone;
                    }
                }
            }
            if (trace != nil && (~trace).Wait100Continue != default!) {
                (~trace).Wait100Continue();
            }
            if (!waitForContinue()) {
                closed = true;
                r.closeBody();
                err = default!; goto ᒐdone;
            }
        }
        {
            var (bwΔ2, ok) = w._<ж<bufio.Writer>>(ᐧ); if (ok && (~tw).FlushHeaders) {
                {
                    var errΔ1 = bwΔ2.Flush(); if (errΔ1 != default!) {
                        err = errΔ1; goto ᒐdone;
                    }
                }
            }
        }
        // Write body and trailer
        closed = true;
        err = tw.writeBody(w);
        if (err != default!) {
            if (AreEqual((~tw).bodyReadError, err)) {
                err = new requestBodyReadError(err);
            }
            goto ᒐdone;
        }
        if (bw != nil) {
            err = bw.Flush(); goto ᒐdone;
        }
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

// requestBodyReadError wraps an error from (*Request).write to indicate
// that the error came from a Read call on the Request.Body.
// This error type should not escape the net/http package to users.
partial struct requestBodyReadError {
    /*embed*/ internal error error;
}

internal static (@string, error) idnaASCII(@string v) {
    // TODO: Consider removing this check after verifying performance is okay.
    // Right now punycode verification, length checks, context checks, and the
    // permissible character tests are all omitted. It also prevents the ToASCII
    // call from salvaging an invalid IDN, when possible. As a result it may be
    // possible to have two IDNs that appear identical to the user where the
    // ASCII-only version causes an error downstream whereas the non-ASCII
    // version does not.
    // Note that for correct ASCII IDNs ToASCII will only do considerably more
    // work, but it will not cause an allocation.
    if (ascii.Is(v)) {
        return (v, default!);
    }
    return idna.Lookup.ToASCII(v);
}

// removeZone removes IPv6 zone identifier from host.
// E.g., "[fe80::1%en0]:8080" to "[fe80::1]:8080"
internal static @string removeZone(@string host) {
    if (!strings.HasPrefix(host, "["u8)) {
        return host;
    }
    nint i = strings.LastIndex(host, "]"u8);
    if (i < 0) {
        return host;
    }
    nint j = strings.LastIndex(host.slice(0, i), "%"u8);
    if (j < 0) {
        return host;
    }
    return host.slice(0, j) + host.slice(i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpˢ2 = "HTTP/"u8;

// ParseHTTPVersion parses an HTTP version string according to RFC 7230, section 2.6.
// "HTTP/1.0" returns (1, 0, true). Note that strings without
// a minor version, such as "HTTP/2", are not valid.
public static (nint major, nint minor, bool ok) ParseHTTPVersion(@string vers) {
    var exprᴛ1 = vers;
    if (exprᴛ1 == "HTTP/1.1"u8) {
        return (1, 1, true);
    }
    if (exprᴛ1 == "HTTP/1.0"u8) {
        return (1, 0, true);
    }

    if (!strings.HasPrefix(vers, httpˢ2)) {
        return (0, 0, false);
    }
    if (builtin.len(vers) != builtin.len("HTTP/X.Y")) {
        return (0, 0, false);
    }
    if (vers[6] != (rune)'.') {
        return (0, 0, false);
    }
    var (maj, err) = strconv.ParseUint(vers[5..6], 10, 0);
    if (err != default!) {
        return (0, 0, false);
    }
    (var min, err) = strconv.ParseUint(vers[7..8], 10, 0);
    if (err != default!) {
        return (0, 0, false);
    }
    return ((nint)maj, (nint)min, true);
}

internal static bool validMethod(@string method) {
    /*
	     Method         = "OPTIONS"                ; Section 9.2
	                    | "GET"                    ; Section 9.3
	                    | "HEAD"                   ; Section 9.4
	                    | "POST"                   ; Section 9.5
	                    | "PUT"                    ; Section 9.6
	                    | "DELETE"                 ; Section 9.7
	                    | "TRACE"                  ; Section 9.8
	                    | "CONNECT"                ; Section 9.9
	                    | extension-method
	   extension-method = token
	     token          = 1*<any CHAR except CTLs or separators>
	*/
    return builtin.len(method) > 0 && strings.IndexFunc(method, isNotToken) == -1;
}

// NewRequest wraps [NewRequestWithContext] using [context.Background].
public static (ж<Request>, error) NewRequest(@string method, @string url, io.Reader body) {
    return NewRequestWithContext(context_package.Background(), method, url, body);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netHttpNilContextˢ = "net/http: nil Context"u8;

// NewRequestWithContext returns a new [Request] given a method, URL, and
// optional body.
//
// If the provided body is also an [io.Closer], the returned
// [Request.Body] is set to body and will be closed (possibly
// asynchronously) by the Client methods Do, Post, and PostForm,
// and [Transport.RoundTrip].
//
// NewRequestWithContext returns a Request suitable for use with
// [Client.Do] or [Transport.RoundTrip]. To create a request for use with
// testing a Server Handler, either use the [net/http/httptest.NewRequest] function,
// use [ReadRequest], or manually update the Request fields.
// For an outgoing client request, the context
// controls the entire lifetime of a request and its response:
// obtaining a connection, sending the request, and reading the
// response headers and body. See the Request type's documentation for
// the difference between inbound and outbound request fields.
//
// If body is of type [*bytes.Buffer], [*bytes.Reader], or
// [*strings.Reader], the returned request's ContentLength is set to its
// exact value (instead of -1), GetBody is populated (so 307 and 308
// redirects can replay the body), and Body is set to [NoBody] if the
// ContentLength is 0.
public static (ж<Request>, error) NewRequestWithContext(context.Context ctx, @string method, @string url, io.Reader body) {
    if (method == ""u8) {
        // We document that "" means "GET" for Request.Method, and people have
        // relied on that from NewRequest, so keep that working.
        // We still enforce validMethod for non-empty methods.
        method = getˢ;
    }
    if (!validMethod(method)) {
        return (default!, fmt.Errorf("net/http: invalid method %q"u8, method));
    }
    if (ctx == default!) {
        return (default!, errors.New(netHttpNilContextˢ));
    }
    var (u, err) = urlpkg.Parse(url);
    if (err != default!) {
        return (default!, err);
    }
    var (rc, ok) = body._<io.ReadCloser>(ᐧ);
    if (!ok && body != default!) {
        rc = io.NopCloser(body);
    }
    // The host's colon:port should be normalized. See Issue 14836.
    u.Value.Host = removeEmptyPort((~u).Host);
    var req = Ꮡ(new Request(
        ctx: ctx,
        Method: method,
        URL: u,
        Proto: "HTTP/1.1"u8,
        ProtoMajor: 1,
        ProtoMinor: 1,
        Header: new ΔHeader(0),
        Body: rc,
        Host: (~u).Host
    ));
    if (body != default!) {
        switch (body.type()) {
        case ж<bytes.Buffer> v: {
            req.Value.ContentLength = (int64)v.Len();
            var buf = v.Bytes();
            var bufʗ1 = buf;
            req.Value.GetBody = (io.ReadCloser, error) () => {
                var r = bytes.NewReader(bufʗ1);
                return (io.NopCloser(new bytes_ReaderжReader(r)), default!);
            };
            break;
        }
        case ж<bytes.Reader> v: {
            req.Value.ContentLength = (int64)v.Len();
            ref var snapshot = ref heap<bytes.Reader>(out var Ꮡsnapshot);
            snapshot = v.Value;
            var snapshotʗ1 = snapshot;
            req.Value.GetBody = (io.ReadCloser, error) () => {
                ref var r = ref heap<bytes.Reader>(out var Ꮡr);
                r = snapshotʗ1;
                return (io.NopCloser(new bytes_ReaderжReader(Ꮡr)), default!);
            };
            break;
        }
        case ж<strings.Reader> v: {
            req.Value.ContentLength = (int64)v.Len();
            ref var snapshot = ref heap<strings.Reader>(out var Ꮡsnapshot);
            snapshot = v.Value;
            var snapshotʗ2 = snapshot;
            req.Value.GetBody = (io.ReadCloser, error) () => {
                ref var r = ref heap<strings.Reader>(out var Ꮡr);
                r = snapshotʗ2;
                return (io.NopCloser(new strings_ReaderжReader(Ꮡr)), default!);
            };
            break;
        }
        default: {
            var v = body;
            break;
        }}
        // This is where we'd set it to -1 (at least
        // if body != NoBody) to mean unknown, but
        // that broke people during the Go 1.8 testing
        // period. People depend on it being 0 I
        // guess. Maybe retry later. See Issue 18117.
        // For client requests, Request.ContentLength of 0
        // means either actually 0, or unknown. The only way
        // to explicitly say that the ContentLength is zero is
        // to set the Body to nil. But turns out too much code
        // depends on NewRequest returning a non-nil Body,
        // so we use a well-known ReadCloser variable instead
        // and have the http package also treat that sentinel
        // variable to mean explicitly zero.
        if ((~req).GetBody != default! && (~req).ContentLength == 0) {
            req.Value.Body = NoBody;
            req.Value.GetBody = (io.ReadCloser, error) () => (NoBody, default!);
        }
    }
    return (req, default!);
}

// BasicAuth returns the username and password provided in the request's
// Authorization header, if the request uses HTTP Basic Authentication.
// See RFC 2617, Section 2.
public static (@string username, @string password, bool ok) BasicAuth(this ref Request r) {
    @string auth = r.Header.Get(authorizationˢ);
    if (auth == ""u8) {
        return ("", "", false);
    }
    return parseBasicAuth(auth);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ = "Basic "u8;

// parseBasicAuth parses an HTTP Basic Authentication string.
// "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==" returns ("Aladdin", "open sesame", true).
//
// parseBasicAuth should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname parseBasicAuth
internal static (@string username, @string password, bool ok) parseBasicAuth(@string auth) {
    @string username = default!;
    @string password = default!;
    bool ok = default!;

    @string prefix = prefixᶜ;
    // Case insensitive prefix match. See Issue 22736.
    if (builtin.len(auth) < builtin.len(prefix) || !ascii.EqualFold(auth[..(int)(builtin.len(prefix))], prefix)) {
        return ("", "", false);
    }
    var (c, err) = base64.StdEncoding.DecodeString(auth[(int)(builtin.len(prefix))..]);
    if (err != default!) {
        return ("", "", false);
    }
    @string cs = ((@string)c);
    (username, password, ok) = strings.Cut(cs, ":"u8);
    if (!ok) {
        return ("", "", false);
    }
    return (username, password, true);
}

// SetBasicAuth sets the request's Authorization header to use HTTP
// Basic Authentication with the provided username and password.
//
// With HTTP Basic Authentication the provided username and password
// are not encrypted. It should generally only be used in an HTTPS
// request.
//
// The username may not contain a colon. Some protocols may impose
// additional requirements on pre-escaping the username and
// password. For instance, when used with OAuth2, both arguments must
// be URL encoded first with [url.QueryEscape].
public static void SetBasicAuth(this ref Request r, @string username, @string password) {
    r.Header.Set(authorizationˢ, "Basic "u8 + basicAuth(username, password));
}

// parseRequestLine parses "GET /foo HTTP/1.1" into its three parts.
internal static (@string method, @string requestURI, @string proto, bool ok) parseRequestLine(@string line) {
    @string method = default!;
    @string requestURI = default!;
    @string proto = default!;

    (method, var rest, var ok1) = strings.Cut(line, " "u8);
    (requestURI, proto, var ok2) = strings.Cut(rest, " "u8);
    if (!ok1 || !ok2) {
        return ("", "", "", false);
    }
    return (method, requestURI, proto, true);
}

internal static ж<sync.Pool> ᏑtextprotoReaderPool = new StandardBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool textprotoReaderPool => ref ᏑtextprotoReaderPool.Value;

internal static ж<textproto.Reader> newTextprotoReader(ж<bufio.Reader> Ꮡbr) {
    {
        var v = ᏑtextprotoReaderPool.Get(); if (v != default!) {
            var tr = v._<ж<textproto.Reader>>();
            tr.Value.R = Ꮡbr;
            return tr;
        }
    }
    return textproto.NewReader(Ꮡbr);
}

internal static void putTextprotoReader(ж<textproto.Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.R = default!;
    ᏑtextprotoReaderPool.Put(Ꮡr.OrTypedNil());
}

// ReadRequest reads and parses an incoming request from b.
//
// ReadRequest is a low-level function and should only be used for
// specialized applications; most code should use the [Server] to read
// requests and handle them via the [Handler] interface. ReadRequest
// only supports HTTP/1.x requests. For HTTP/2, use golang.org/x/net/http2.
public static (ж<Request>, error) ReadRequest(ж<bufio.Reader> Ꮡb) {
    var (req, err) = readRequest(Ꮡb);
    if (err != default!) {
        return (default!, err);
    }
    delete((~req).Header, "Host"u8);
    return (req, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string malformedHttpRequestˢ = "malformed HTTP request"u8;
internal static readonly @string invalidMethodˢ = "invalid method"u8;
internal static readonly @string malformedHttpVersionˢ = "malformed HTTP version"u8;

// readRequest should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing
//   - github.com/v2fly/v2ray-core/v4
//   - github.com/v2fly/v2ray-core/v5
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname readRequest
internal static (ж<Request> req, error err) readRequest(ж<bufio.Reader> Ꮡb) {
    ж<Request> req = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        var tp = newTextprotoReader(Ꮡb);
        defer(putTextprotoReader, tp, ref ᒐ);
        req = @new<Request>();
        // First line: GET /index.html HTTP/1.0
        @string s = default!;
        {
            (s, err) = tp.ReadLine(); if (err != default!) {
                (req, err) = (default!, err); goto ᒐdone;
            }
        }
        defer(() => {
            if (AreEqual(Ꮡerr.ValueSlot, io.EOF)) {
                Ꮡerr.ValueSlot = io.ErrUnexpectedEOF;
            }
        }, ref ᒐ);
        bool ok = default!;
        (req.Value.Method, req.Value.RequestURI, req.Value.Proto, ok) = parseRequestLine(s);
        if (!ok) {
            (req, err) = (default!, badStringError(malformedHttpRequestˢ, s)); goto ᒐdone;
        }
        if (!validMethod((~req).Method)) {
            (req, err) = (default!, badStringError(invalidMethodˢ, (~req).Method)); goto ᒐdone;
        }
        @string rawurl = req.Value.RequestURI;
        {
            (req.Value.ProtoMajor, req.Value.ProtoMinor, ok) = ParseHTTPVersion((~req).Proto); if (!ok) {
                (req, err) = (default!, badStringError(malformedHttpVersionˢ, (~req).Proto)); goto ᒐdone;
            }
        }
        // CONNECT requests are used two different ways, and neither uses a full URL:
        // The standard use is to tunnel HTTPS through an HTTP proxy.
        // It looks like "CONNECT www.google.com:443 HTTP/1.1", and the parameter is
        // just the authority section of a URL. This information should go in req.URL.Host.
        //
        // The net/rpc package also uses CONNECT, but there the parameter is a path
        // that starts with a slash. It can be parsed with the regular URL parser,
        // and the path will end up in req.URL.Path, where it needs to be in order for
        // RPC to work.
        var justAuthority = (~req).Method == "CONNECT"u8 && !strings.HasPrefix(rawurl, "/"u8);
        if (justAuthority) {
            rawurl = "http://"u8 + rawurl;
        }
        {
            (req.Value.URL, err) = url.ParseRequestURI(rawurl); if (err != default!) {
                (req, err) = (default!, err); goto ᒐdone;
            }
        }
        if (justAuthority) {
            // Strip the bogus "http://" back off.
            req.Value.URL.Value.Scheme = ""u8;
        }
        // Subsequent lines: Key: value.
        (var mimeHeader, err) = tp.ReadMIMEHeader();
        if (err != default!) {
            (req, err) = (default!, err); goto ᒐdone;
        }
        req.Value.Header = ((ΔHeader)(map<@string, slice<@string>>)mimeHeader);
        if (builtin.len((~req).Header[hostˢ]) > 1) {
            (req, err) = (default!, fmt.Errorf("too many Host headers"u8)); goto ᒐdone;
        }
        // RFC 7230, section 5.3: Must treat
        //	GET /index.html HTTP/1.1
        //	Host: www.google.com
        // and
        //	GET http://www.google.com/index.html HTTP/1.1
        //	Host: doesntmatter
        // the same. In the second case, any Host line is ignored.
        req.Value.Host = req.Value.URL.Value.Host;
        if ((~req).Host == ""u8) {
            req.Value.Host = (~req).Header.get(hostˢ);
        }
        fixPragmaCacheControl((~req).Header);
        req.Value.Close = shouldClose((~req).ProtoMajor, (~req).ProtoMinor, (~req).Header, false);
        err = readTransfer(req.OrTypedNil(), Ꮡb);
        if (err != default!) {
            (req, err) = (default!, err); goto ᒐdone;
        }
        if (req.isH2Upgrade()) {
            // Because it's neither chunked, nor declared:
            req.Value.ContentLength = -1;
            // We want to give handlers a chance to hijack the
            // connection, but we need to prevent the Server from
            // dealing with the connection further if it's not
            // hijacked. Set Close to ensure that:
            req.Value.Close = true;
        }
        (req, err) = (req, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (req, Ꮡerr.ValueSlot);
}

// MaxBytesReader is similar to [io.LimitReader] but is intended for
// limiting the size of incoming request bodies. In contrast to
// io.LimitReader, MaxBytesReader's result is a ReadCloser, returns a
// non-nil error of type [*MaxBytesError] for a Read beyond the limit,
// and closes the underlying reader when its Close method is called.
//
// MaxBytesReader prevents clients from accidentally or maliciously
// sending a large request and wasting server resources. If possible,
// it tells the [ResponseWriter] to close the connection after the limit
// has been reached.
public static io.ReadCloser MaxBytesReader(ResponseWriter w, io.ReadCloser r, int64 n) {
    if (n < 0) {
        // Treat negative limits as equivalent to 0.
        n = 0;
    }
    return new maxBytesReaderжReadCloser(Ꮡ(new maxBytesReader(w: w, r: r, i: n, n: n)));
}

// MaxBytesError is returned by [MaxBytesReader] when its read limit is exceeded.
partial struct MaxBytesError {
    public int64 Limit;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpRequestBodyTooLargeˢ = "http: request body too large"u8;

public static @string Error(this ref MaxBytesError e) {
    // Due to Hyrum's law, this text cannot be changed.
    return httpRequestBodyTooLargeˢ;
}

partial struct maxBytesReader {
    internal ResponseWriter w;
    internal io.ReadCloser r; // underlying reader
    internal int64 i;         // max bytes initially, for MaxBytesError
    internal int64 n;         // max bytes remaining
    internal error err;         // sticky error
}

// The server code and client code both use
// maxBytesReader. This "requestTooLarge" check is
// only used by the server code. To prevent binaries
// which only using the HTTP Client code (such as
// cmd/go) from also linking in the HTTP server, don't
// use a static type assertion to the server
// "*response" type. Check this interface instead:
internal partial interface Read_requestTooLarger /*dyn*/ {
    void requestTooLarge();
}

internal static (nint n, error err) Read(this ref maxBytesReader l, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (l.err != default!) {
        return (0, l.err);
    }
    if (builtin.len(p) == 0) {
        return (0, default!);
    }
    // If they asked for a 32KB byte read but only 5 bytes are
    // remaining, no need to read 32KB. 6 bytes will answer the
    // question of the whether we hit the limit or go past it.
    // 0 < len(p) < 2^63
    if ((int64)builtin.len(p) - 1 > l.n) {
        p = p.slice(0, (nint)(l.n + 1));
    }
    (n, err) = l.r.Read(p);
    if ((int64)n <= l.n) {
        l.n -= (int64)n;
        l.err = err;
        return (n, err);
    }
    n = (nint)l.n;
    l.n = 0;
    {
        var (res, ok) = l.w._<Read_requestTooLarger>(ᐧ); if (ok) {
            res.requestTooLarge();
        }
    }
    l.err = new MaxBytesErrorжerror(Ꮡ(new MaxBytesError(l.i)));
    return (n, l.err);
}

internal static error Close(this ref maxBytesReader l) {
    return l.r.Close();
}

internal static void copyValues(urlpkg.Values dst, urlpkg.Values src) {
    foreach (var (k, vs) in src) {
        dst[k] = appendꓸꓸꓸ(dst[k], vs);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string applicationOctetStreamˢ = "application/octet-stream"u8;
internal static readonly @string httpPostTooLargeˢ = "http: POST too large"u8;

internal static (urlpkg.Values vs, error err) parsePostForm(ref Request r) {
    urlpkg.Values vs = default!;
    error err = default!;

    if (r.Body == default!) {
        err = errors.New(missingFormBodyˢ);
        return (vs, err);
    }
    @string ct = r.Header.Get(contentTypeˢ);
    // RFC 7231, section 3.1.1.5 - empty type
    //   MAY be treated as application/octet-stream
    if (ct == ""u8) {
        ct = applicationOctetStreamˢ;
    }
    (ct, _, err) = mime.ParseMediaType(ct);
    switch (ᐧ) {
    case {} when ct == "application/x-www-form-urlencoded"u8: {
        io.Reader reader = r.Body;
        var maxFormSize = (int64)(9223372036854775807L);
        {
            var (_, ok) = r.Body._<ж<maxBytesReader>>(ᐧ); if (!ok) {
                maxFormSize = (int64)(((int64)10 << (int)(20))); // 10 MB is a lot of text.
                reader = io.LimitReader(r.Body, maxFormSize + 1);
            }
        }
        var (b, e) = io.ReadAll(reader);
        if (e != default!) {
            if (err == default!) {
                err = e;
            }
            break;
        }
        if ((int64)builtin.len(b) > maxFormSize) {
            err = errors.New(httpPostTooLargeˢ);
            return (vs, err);
        }
        (vs, e) = url.ParseQuery(((@string)b));
        if (err == default!) {
            err = e;
        }
        break;
    }
    case {} when ct == "multipart/form-data"u8: {
        break;
    }}

    // handled by ParseMultipartForm (which is calling us, or should be)
    // TODO(bradfitz): there are too many possible
    // orders to call too many functions here.
    // Clean this up and write more tests.
    // request_test.go contains the start of this,
    // in TestParseMultipartFormOrder and others.
    return (vs, err);
}

// ParseForm populates r.Form and r.PostForm.
//
// For all requests, ParseForm parses the raw query from the URL and updates
// r.Form.
//
// For POST, PUT, and PATCH requests, it also reads the request body, parses it
// as a form and puts the results into both r.PostForm and r.Form. Request body
// parameters take precedence over URL query string values in r.Form.
//
// If the request Body's size has not already been limited by [MaxBytesReader],
// the size is capped at 10MB.
//
// For other HTTP methods, or when the Content-Type is not
// application/x-www-form-urlencoded, the request Body is not read, and
// r.PostForm is initialized to a non-nil, empty value.
//
// [Request.ParseMultipartForm] calls ParseForm automatically.
// ParseForm is idempotent.
public static error ParseForm(this ж<Request> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    error err = default!;
    if (r.PostForm == default!) {
        if (r.Method == "POST"u8 || r.Method == "PUT"u8 || r.Method == "PATCH"u8) {
            (r.PostForm, err) = parsePostForm(ref (Ꮡr).DerefOrNull());
        }
        if (r.PostForm == default!) {
            r.PostForm = new urlpkg.Values(0);
        }
    }
    if (r.Form == default!) {
        if (builtin.len(r.PostForm) > 0) {
            r.Form = new urlpkg.Values(0);
            copyValues(r.Form, r.PostForm);
        }
        urlpkg.Values newValues = default!;
        if (r.URL != nil) {
            error e = default!;
            (newValues, e) = url.ParseQuery((~r.URL).RawQuery);
            if (err == default!) {
                err = e;
            }
        }
        if (newValues == default!) {
            newValues = new urlpkg.Values(0);
        }
        if (r.Form == default!){
            r.Form = newValues;
        } else {
            copyValues(r.Form, newValues);
        }
    }
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpMultipartHandledByˢ2 = "http: multipart handled by MultipartReader"u8;

// ParseMultipartForm parses a request body as multipart/form-data.
// The whole request body is parsed and up to a total of maxMemory bytes of
// its file parts are stored in memory, with the remainder stored on
// disk in temporary files.
// ParseMultipartForm calls [Request.ParseForm] if necessary.
// If ParseForm returns an error, ParseMultipartForm returns it but also
// continues parsing the request body.
// After one call to ParseMultipartForm, subsequent calls have no effect.
public static error ParseMultipartForm(this ж<Request> Ꮡr, int64 maxMemory) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.MultipartForm == multipartByReader) {
        return errors.New(httpMultipartHandledByˢ2);
    }
    error parseFormErr = default!;
    if (r.Form == default!) {
        // Let errors in ParseForm fall through, and just
        // return it at the end.
        parseFormErr = Ꮡr.ParseForm();
    }
    if (r.MultipartForm != nil) {
        return default!;
    }
    var (mr, err) = r.multipartReader(false);
    if (err != default!) {
        return err;
    }
    (var f, err) = mr.ReadForm(maxMemory);
    if (err != default!) {
        return err;
    }
    if (r.PostForm == default!) {
        r.PostForm = new urlpkg.Values(0);
    }
    foreach (var (k, v) in (~f).Value) {
        r.Form[k] = appendꓸꓸꓸ(r.Form[k], v);
        // r.PostForm should also be populated. See Issue 9305.
        r.PostForm[k] = appendꓸꓸꓸ(r.PostForm[k], v);
    }
    r.MultipartForm = f;
    return parseFormErr;
}

// FormValue returns the first value for the named component of the query.
// The precedence order:
//  1. application/x-www-form-urlencoded form body (POST, PUT, PATCH only)
//  2. query parameters (always)
//  3. multipart/form-data form body (always)
//
// FormValue calls [Request.ParseMultipartForm] and [Request.ParseForm]
// if necessary and ignores any errors returned by these functions.
// If key is not present, FormValue returns the empty string.
// To access multiple values of the same key, call ParseForm and
// then inspect [Request.Form] directly.
public static @string FormValue(this ж<Request> Ꮡr, @string key) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.Form == default!) {
        Ꮡr.ParseMultipartForm(defaultMaxMemory);
    }
    {
        var vs = r.Form[key]; if (builtin.len(vs) > 0) {
            return vs[0];
        }
    }
    return ""u8;
}

// PostFormValue returns the first value for the named component of the POST,
// PUT, or PATCH request body. URL query parameters are ignored.
// PostFormValue calls [Request.ParseMultipartForm] and [Request.ParseForm] if necessary and ignores
// any errors returned by these functions.
// If key is not present, PostFormValue returns the empty string.
public static @string PostFormValue(this ж<Request> Ꮡr, @string key) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.PostForm == default!) {
        Ꮡr.ParseMultipartForm(defaultMaxMemory);
    }
    {
        var vs = r.PostForm[key]; if (builtin.len(vs) > 0) {
            return vs[0];
        }
    }
    return ""u8;
}

// FormFile returns the first file for the provided form key.
// FormFile calls [Request.ParseMultipartForm] and [Request.ParseForm] if necessary.
public static (multipart.File, ж<multipart.FileHeader>, error) FormFile(this ж<Request> Ꮡr, @string key) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.MultipartForm == multipartByReader) {
        return (default!, default!, errors.New(httpMultipartHandledByˢ2));
    }
    if (r.MultipartForm == nil) {
        var err = Ꮡr.ParseMultipartForm(defaultMaxMemory);
        if (err != default!) {
            return (default!, default!, err);
        }
    }
    if (r.MultipartForm != nil && (~r.MultipartForm).File != default!) {
        {
            var fhs = (~r.MultipartForm).File[key]; if (builtin.len(fhs) > 0) {
                var (f, err) = fhs[0].Open();
                return (f, fhs[0], err);
            }
        }
    }
    return (default!, default!, ErrMissingFile);
}

// PathValue returns the value for the named path wildcard in the [ServeMux] pattern
// that matched the request.
// It returns the empty string if the request was not matched against a pattern
// or there is no such wildcard in the pattern.
public static @string PathValue(this ref Request r, @string name) {
    {
        nint i = r.patIndex(name); if (i >= 0) {
            return r.matches[i];
        }
    }
    return r.otherValues[name];
}

// SetPathValue sets name to value, so that subsequent calls to r.PathValue(name)
// return value.
public static void SetPathValue(this ref Request r, @string name, @string value) {
    {
        nint i = r.patIndex(name); if (i >= 0){
            r.matches[i] = value;
        } else {
            if (r.otherValues == default!) {
                r.otherValues = new map<@string, @string>{};
            }
            r.otherValues[name] = value;
        }
    }
}

// patIndex returns the index of name in the list of named wildcards of the
// request's pattern, or -1 if there is no such name.
internal static nint patIndex(this ref Request r, @string name) {
    // The linear search seems expensive compared to a map, but just creating the map
    // takes a lot of time, and most patterns will just have a couple of wildcards.
    if (r.pat == nil) {
        return -1;
    }
    nint i = 0;
    foreach (var (_, seg) in (~r.pat).segments) {
        if (seg.wild && seg.s != ""u8) {
            if (name == seg.s) {
                return i;
            }
            i++;
        }
    }
    return -1;
}

internal static bool expectsContinue(this ref Request r) {
    return hasToken(r.Header.get(expectˢ), continueˢ);
}

internal static bool wantsHttp10KeepAlive(this ref Request r) {
    if (r.ProtoMajor != 1 || r.ProtoMinor != 0) {
        return false;
    }
    return hasToken(r.Header.get(connectionˢ), keepAliveˢ);
}

internal static bool wantsClose(this ref Request r) {
    if (r.Close) {
        return true;
    }
    return hasToken(r.Header.get(connectionˢ), closeˢ);
}

internal static error closeBody(this ref Request r) {
    if (r.Body == default!) {
        return default!;
    }
    return r.Body.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string idempotencyKeyˢ = "Idempotency-Key"u8;
internal static readonly @string xIdempotencyKeyˢ = "X-Idempotency-Key"u8;

internal static bool isReplayable(this ref Request r) {
    if (r.Body == default! || AreEqual(r.Body, NoBody) || r.GetBody != default!) {
        var exprᴛ1 = valueOrDefault(r.Method, getˢ);
        if (exprᴛ1 == "GET"u8 || exprᴛ1 == "HEAD"u8 || exprᴛ1 == "OPTIONS"u8 || exprᴛ1 == "TRACE"u8) {
            return true;
        }

        // The Idempotency-Key, while non-standard, is widely used to
        // mean a POST or other request is idempotent. See
        // https://golang.org/issue/19943#issuecomment-421092421
        if (r.Header.has(idempotencyKeyˢ) || r.Header.has(xIdempotencyKeyˢ)) {
            return true;
        }
    }
    return false;
}

// outgoingLength reports the Content-Length of this outgoing (Client) request.
// It maps 0 into -1 (unknown) when the Body is non-nil.
internal static int64 outgoingLength(this ref Request r) {
    if (r.Body == default! || AreEqual(r.Body, NoBody)) {
        return 0;
    }
    if (r.ContentLength != 0) {
        return r.ContentLength;
    }
    return -1;
}

// requestMethodUsuallyLacksBody reports whether the given request
// method is one that typically does not involve a request body.
// This is used by the Transport (via
// transferWriter.shouldSendChunkedRequestBody) to determine whether
// we try to test-read a byte from a non-nil Request.Body when
// Request.outgoingLength() returns -1. See the comments in
// shouldSendChunkedRequestBody.
internal static bool requestMethodUsuallyLacksBody(@string method) {
    var exprᴛ1 = method;
    if (exprᴛ1 == "GET"u8 || exprᴛ1 == "HEAD"u8 || exprᴛ1 == "DELETE"u8 || exprᴛ1 == "OPTIONS"u8 || exprᴛ1 == "PROPFIND"u8 || exprᴛ1 == "SEARCH"u8) {
        return true;
    }

    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string websocketˢ = "websocket"u8;

// requiresHTTP1 reports whether this request requires being sent on
// an HTTP/1 connection.
internal static bool requiresHTTP1(this ref Request r) {
    return hasToken(r.Header.Get(connectionˢ), upgradeˢ2) && ascii.EqualFold(r.Header.Get(upgradeˢ), websocketˢ);
}

} // end http_package
