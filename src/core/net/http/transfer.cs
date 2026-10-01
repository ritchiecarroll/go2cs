// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using maps = maps_package;
using httptrace = go.net.http.httptrace_package;
using @internal = go.net.http.internal_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using System.Runtime.CompilerServices;
using go.@internal;
using go.net;
using go.net.http;
using go.net.http.@internal;
using vendor.golang.org.x.net.http;

partial class http_package {

// ErrLineTooLong is returned when reading request or response bodies
// with malformed chunked encoding.
public static error ErrLineTooLong = @internal.ErrLineTooLong;

[GoType] partial struct errorReader {
    internal error err;
}

internal static (nint n, error err) Read(this errorReader r, slice<byte> p) {
    return (0, r.err);
}

[GoType] partial struct byteReader {
    internal byte b;
    internal bool done;
}

[GoRecv] internal static (nint n, error err) Read(this ref byteReader br, slice<byte> p) {
    if (br.done) {
        return (0, io.EOF);
    }
    if (builtin.len(p) == 0) {
        return (0, default!);
    }
    br.done = true;
    p[0] = br.b;
    return (1, io.EOF);
}

// transferWriter inspects the fields of a user-supplied Request or Response,
// sanitizes them without changing the user object and provides methods for
// writing the respective header, body and trailer in wire format.
[GoType] partial struct transferWriter {
    public @string Method;
    public io.Reader Body;
    public io.Closer BodyCloser;
    public bool ResponseToHEAD;
    public int64 ContentLength; // -1 means unknown, 0 means exactly none
    public bool Close;
    public slice<@string> TransferEncoding;
    public ΔHeader Header;
    public ΔHeader Trailer;
    public bool IsResponse;
    internal error bodyReadError; // any non-EOF error from reading Body
    public bool FlushHeaders;            // flush headers to network before body
    public channel<readResult> ByteReadCh; // non-nil if probeRequestBody called
}

internal static (ж<transferWriter> t, error err) newTransferWriter(any r) {
    ж<transferWriter> t = default!;

    t = Ꮡ(new transferWriter(nil));
    // Extract relevant fields
    var atLeastHTTP11 = false;
    switch (r.type()) {
    case ж<Request> rr: {
        if ((~rr).ContentLength != 0 && (~rr).Body == default!) {
            return (default!, fmt.Errorf("http: Request.ContentLength=%d with nil Body"u8, (~rr).ContentLength));
        }
        t.Value.Method = valueOrDefault((~rr).Method, getˢ);
        t.Value.Close = rr.Value.Close;
        t.Value.TransferEncoding = rr.Value.TransferEncoding;
        t.Value.Header = rr.Value.Header;
        t.Value.Trailer = rr.Value.Trailer;
        t.Value.Body = rr.Value.Body;
        t.Value.BodyCloser = rr.Value.Body;
        t.Value.ContentLength = rr.outgoingLength();
        if ((~t).ContentLength < 0 && builtin.len((~t).TransferEncoding) == 0 && t.shouldSendChunkedRequestBody()) {
            t.Value.TransferEncoding = new @string[]{"chunked"u8}.slice();
        }
        if ((~t).ContentLength != 0 && !isKnownInMemoryReader((~t).Body)) {
            // If there's a body, conservatively flush the headers
            // to any bufio.Writer we're writing to, just in case
            // the server needs the headers early, before we copy
            // the body and possibly block. We make an exception
            // for the common standard library in-memory types,
            // though, to avoid unnecessary TCP packets on the
            // wire. (Issue 22088.)
            t.Value.FlushHeaders = true;
        }
        atLeastHTTP11 = true; // Transport requests are always 1.1 or 2.0
        break;
    }
    case ж<Response> rr: {
        t.Value.IsResponse = true;
        if ((~rr).Request != nil) {
            t.Value.Method = rr.Value.Request.Value.Method;
        }
        t.Value.Body = rr.Value.Body;
        t.Value.BodyCloser = rr.Value.Body;
        t.Value.ContentLength = rr.Value.ContentLength;
        t.Value.Close = rr.Value.Close;
        t.Value.TransferEncoding = rr.Value.TransferEncoding;
        t.Value.Header = rr.Value.Header;
        t.Value.Trailer = rr.Value.Trailer;
        atLeastHTTP11 = rr.ProtoAtLeast(1, 1);
        t.Value.ResponseToHEAD = noResponseBodyExpected((~t).Method);
        break;
    }}
    // Sanitize Body,ContentLength,TransferEncoding
    if ((~t).ResponseToHEAD){
        t.Value.Body = default!;
        if (chunked((~t).TransferEncoding)) {
            t.Value.ContentLength = -1;
        }
    } else {
        if (!atLeastHTTP11 || (~t).Body == default!) {
            t.Value.TransferEncoding = default!;
        }
        if (chunked((~t).TransferEncoding)){
            t.Value.ContentLength = -1;
        } else 
        if ((~t).Body == default!) {
            // no chunking, no body
            t.Value.ContentLength = 0;
        }
    }
    // Sanitize Trailer
    if (!chunked((~t).TransferEncoding)) {
        t.Value.Trailer = default!;
    }
    return (t, default!);
}

// shouldSendChunkedRequestBody reports whether we should try to send a
// chunked request body to the server. In particular, the case we really
// want to prevent is sending a GET or other typically-bodyless request to a
// server with a chunked body when the body has zero bytes, since GETs with
// bodies (while acceptable according to specs), even zero-byte chunked
// bodies, are approximately never seen in the wild and confuse most
// servers. See Issue 18257, as one example.
//
// The only reason we'd send such a request is if the user set the Body to a
// non-nil value (say, io.NopCloser(bytes.NewReader(nil))) and didn't
// set ContentLength, or NewRequest set it to -1 (unknown), so then we assume
// there's bytes to send.
//
// This code tries to read a byte from the Request.Body in such cases to see
// whether the body actually has content (super rare) or is actually just
// a non-nil content-less ReadCloser (the more common case). In that more
// common case, we act as if their Body were nil instead, and don't send
// a body.
internal static bool shouldSendChunkedRequestBody(this ж<transferWriter> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Note that t.ContentLength is the corrected content length
    // from rr.outgoingLength, so 0 actually means zero, not unknown.
    if (t.ContentLength >= 0 || t.Body == default!) {
        // redundant checks; caller did them
        return false;
    }
    if (t.Method == "CONNECT"u8) {
        return false;
    }
    if (requestMethodUsuallyLacksBody(t.Method)) {
        // Only probe the Request.Body for GET/HEAD/DELETE/etc
        // requests, because it's only those types of requests
        // that confuse servers.
        Ꮡt.probeRequestBody(); // adjusts t.Body, t.ContentLength
        return t.Body != default!;
    }
    // For all other request types (PUT, POST, PATCH, or anything
    // made-up we've never heard of), assume it's normal and the server
    // can deal with a chunked request body. Maybe we'll adjust this
    // later.
    return true;
}

// probeRequestBody reads a byte from t.Body to see whether it's empty
// (returns io.EOF right away).
//
// But because we've had problems with this blocking users in the past
// (issue 17480) when the body is a pipe (perhaps waiting on the response
// headers before the pipe is fed data), we need to be careful and bound how
// long we wait for it. This delay will only affect users if all the following
// are true:
//   - the request body blocks
//   - the content length is not set (or set to -1)
//   - the method doesn't usually have a body (GET, HEAD, DELETE, ...)
//   - there is no transfer-encoding=chunked already set.
//
// In other words, this delay will not normally affect anybody, and there
// are workarounds if it does.
[MethodImpl(MethodImplOptions.NoInlining)] internal static void probeRequestBody(this ж<transferWriter> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.ByteReadCh = new channel<readResult>(1);
    goǃ((io.Reader body) => {
        ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
        ref var rres = ref heap(new readResult(), out var Ꮡrres);
        (rres.n, rres.err) = body.Read(buf[..]);
        if (rres.n == 1) {
            rres.b = buf[0];
        }
        Ꮡt.Value.ByteReadCh.ᐸꟷ(rres);
        builtin.close(Ꮡt.Value.ByteReadCh);
    }, Ꮡt.Value.Body);
    var timer = time.NewTimer(200 * time.Millisecond);
    var selᴛ91 = t.ByteReadCh;
    var selᴛ92 = (~timer).C;
    switch (select(ᐸꟷ(selᴛ91, ꓸꓸꓸ), ᐸꟷ(selᴛ92, ꓸꓸꓸ))) {
    case 0 when selᴛ91.ꟷᐳ(out var rres): {
        timer.Stop();
        if (rres.n == 0 && AreEqual(rres.err, io.EOF)){
            // It was empty.
            t.Body = default!;
            t.ContentLength = 0;
        } else 
        if (rres.n == 1){
            if (rres.err != default!){
                t.Body = io.MultiReader(new byteReaderжReader(Ꮡ(new byteReader(b: rres.b))), new errorReader(rres.err));
            } else {
                t.Body = io.MultiReader(new byteReaderжReader(Ꮡ(new byteReader(b: rres.b))), t.Body);
            }
        } else 
        if (rres.err != default!) {
            t.Body = new errorReader(rres.err);
        }
        break;
    }
    case 1 when selᴛ92.ꟷᐳ(out _): {
        t.Body = io.MultiReader(new finishAsyncByteRead( // Too slow. Don't wait. Read it later, and keep
 // assuming that this is ContentLength == -1
 // (unknown), which means we'll send a
 // "Transfer-Encoding: chunked" header.
Ꮡt), t.Body);
        t.FlushHeaders = true;
        break;
    }}
}

// Request that Request.Write flush the headers to the
// network before writing the body, since our body may not
// become readable until it's seen the response headers.
internal static bool noResponseBodyExpected(@string requestMethod) {
    return requestMethod == "HEAD"u8;
}

[GoRecv] internal static bool shouldSendContentLength(this ref transferWriter t) {
    if (chunked(t.TransferEncoding)) {
        return false;
    }
    if (t.ContentLength > 0) {
        return true;
    }
    if (t.ContentLength < 0) {
        return false;
    }
    // Many servers expect a Content-Length for these methods
    if (t.Method == "POST"u8 || t.Method == "PUT"u8 || t.Method == "PATCH"u8) {
        return true;
    }
    if (t.ContentLength == 0 && isIdentity(t.TransferEncoding)) {
        if (t.Method == "GET"u8 || t.Method == "HEAD"u8) {
            return false;
        }
        return true;
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connectionCloseˢ = "Connection: close\r\n"u8;
internal static readonly @string contentLengthˢ3 = "Content-Length: "u8;
internal static readonly @string transferEncodingChunkedˢ = "Transfer-Encoding: chunked\r\n"u8;
internal static readonly @string invalidTrailerKeyˢ = "invalid Trailer key"u8;

[GoRecv] internal static error writeHeader(this ref transferWriter t, io.Writer w, ж<httptrace.ClientTrace> Ꮡtrace) {
    ref var trace = ref Ꮡtrace.DerefOrNull();

    if (t.Close && !hasToken(t.Header.get(connectionˢ), closeˢ)) {
        {
            var (_, err) = io.WriteString(w, connectionCloseˢ); if (err != default!) {
                return err;
            }
        }
        if (Ꮡtrace != nil && trace.WroteHeaderField != default!) {
            trace.WroteHeaderField(connectionˢ, new @string[]{"close"u8}.slice());
        }
    }
    // Write Content-Length and/or Transfer-Encoding whose values are a
    // function of the sanitized field triple (Body, ContentLength,
    // TransferEncoding)
    if (t.shouldSendContentLength()){
        {
            var (_, err) = io.WriteString(w, contentLengthˢ3); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = io.WriteString(w, strconv.FormatInt(t.ContentLength, 10) + "\r\n"u8); if (err != default!) {
                return err;
            }
        }
        if (Ꮡtrace != nil && trace.WroteHeaderField != default!) {
            trace.WroteHeaderField(contentLengthˢ, new @string[]{strconv.FormatInt(t.ContentLength, 10)}.slice());
        }
    } else 
    if (chunked(t.TransferEncoding)) {
        {
            var (_, err) = io.WriteString(w, transferEncodingChunkedˢ); if (err != default!) {
                return err;
            }
        }
        if (Ꮡtrace != nil && trace.WroteHeaderField != default!) {
            trace.WroteHeaderField(transferEncodingˢ, new @string[]{"chunked"u8}.slice());
        }
    }
    // Write Trailer header
    if (t.Trailer != default!) {
        var keys = new slice<@string>(0, builtin.len(t.Trailer));
        foreach (var (kᴛ1, _) in t.Trailer) {
            var k = kᴛ1;

            k = CanonicalHeaderKey(k);
            var exprᴛ1 = k;
            if (exprᴛ1 == "Transfer-Encoding"u8 || exprᴛ1 == "Trailer"u8 || exprᴛ1 == "Content-Length"u8) {
                return badStringError(invalidTrailerKeyˢ, k);
            }

            keys = append(keys, k);
        }
        if (builtin.len(keys) > 0) {
            slices.Sort<slice<@string>, @string>(keys);
            // TODO: could do better allocation-wise here, but trailers are rare,
            // so being lazy for now.
            {
                var (_, err) = io.WriteString(w, "Trailer: "u8 + strings.Join(keys, ","u8) + "\r\n"u8); if (err != default!) {
                    return err;
                }
            }
            if (Ꮡtrace != nil && trace.WroteHeaderField != default!) {
                trace.WroteHeaderField(trailerˢ, keys);
            }
        }
    }
    return default!;
}

// always closes t.BodyCloser
internal static error /*err*/ writeBody(this ж<transferWriter> Ꮡt, io.Writer w) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        int64 ncopy = default!;
        var closed = false;
        defer(() => {
            if (closed || Ꮡt.Value.BodyCloser == default!) {
                return;
            }
            {
                var closeErr = Ꮡt.Value.BodyCloser.Close(); if (closeErr != default! && err == default!) {
                    err = closeErr;
                }
            }
        }, ref ᒐ);
        // Write body. We "unwrap" the body first if it was wrapped in a
        // nopCloser or readTrackingBody. This is to ensure that we can take advantage of
        // OS-level optimizations in the event that the body is an
        // *os.File.
        if (!t.ResponseToHEAD && t.Body != default!) {
            io.Reader body = t.unwrapBody();
            if (chunked(t.TransferEncoding)){
                {
                    var (bw, ok) = w._<ж<bufio.Writer>>(ᐧ); if (ok && !t.IsResponse) {
                        w = new internal_FlushAfterChunkWriterжWriter(Ꮡ(new @internal.FlushAfterChunkWriter(Writer: bw)));
                    }
                }
                var cw = @internal.NewChunkedWriter(w);
                (_, err) = Ꮡt.doBodyCopy(cw, body);
                if (err == default!) {
                    err = cw.Close();
                }
            } else 
            if (t.ContentLength == -1){
                var dst = w;
                if (t.Method == "CONNECT"u8) {
                    dst = new bufioFlushWriter(dst);
                }
                (ncopy, err) = Ꮡt.doBodyCopy(dst, body);
            } else {
                (ncopy, err) = Ꮡt.doBodyCopy(w, io.LimitReader(body, t.ContentLength));
                if (err != default!) {
                    goto ᒐdone;
                }
                int64 nextra = default!;
                (nextra, err) = Ꮡt.doBodyCopy(io.Discard, body);
                ncopy += nextra;
            }
            if (err != default!) {
                goto ᒐdone;
            }
        }
        if (t.BodyCloser != default!) {
            closed = true;
            {
                var errΔ1 = t.BodyCloser.Close(); if (errΔ1 != default!) {
                    err = errΔ1; goto ᒐdone;
                }
            }
        }
        if (!t.ResponseToHEAD && t.ContentLength != -1 && t.ContentLength != ncopy) {
            err = fmt.Errorf("http: ContentLength=%d with Body length %d"u8,
                t.ContentLength, ncopy); goto ᒐdone;
        }
        if (!t.ResponseToHEAD && chunked(t.TransferEncoding)) {
            // Write Trailer header
            if (t.Trailer != default!) {
                {
                    var errΔ2 = t.Trailer.Write(w); if (errΔ2 != default!) {
                        err = errΔ2; goto ᒐdone;
                    }
                }
            }
            // Last chunk, empty trailer
            (_, err) = io.WriteString(w, "\r\n"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// doBodyCopy wraps a copy operation, with any resulting error also
// being saved in bodyReadError.
//
// This function is only intended for use in writeBody.
internal static (int64 n, error err) doBodyCopy(this ж<transferWriter> Ꮡt, io.Writer dst, io.Reader src) {
    int64 n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var buf = getCopyBuf();
        defer(putCopyBuf, buf, ref ᒐ);
        (n, err) = io.CopyBuffer(dst, src, buf);
        if (err != default! && !AreEqual(err, io.EOF)) {
            t.bodyReadError = err;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (n, err);
}

// unwrapBody unwraps the body's inner reader if it's a
// nopCloser. This is to ensure that body writes sourced from local
// files (*os.File types) are properly optimized.
//
// This function is only intended for use in writeBody.
[GoRecv] internal static io.Reader unwrapBody(this ref transferWriter t) {
    {
        var (r, ok) = unwrapNopCloser(t.Body); if (ok) {
            return r;
        }
    }
    {
        var (r, ok) = t.Body._<ж<readTrackingBody>>(ᐧ); if (ok) {
            r.Value.didRead = true;
            return (~r).ReadCloser;
        }
    }
    return t.Body;
}

[GoType] partial struct transferReader {
    // Input
    public ΔHeader Header;
    public nint StatusCode;
    public @string RequestMethod;
    public nint ProtoMajor;
    public nint ProtoMinor;
    // Output
    public io.ReadCloser Body;
    public int64 ContentLength;
    public bool Chunked;
    public bool Close;
    public ΔHeader Trailer;
}

[GoRecv] internal static bool protoAtLeast(this ref transferReader t, nint m, nint n) {
    return t.ProtoMajor > m || (t.ProtoMajor == m && t.ProtoMinor >= n);
}

// bodyAllowedForStatus reports whether a given response status code
// permits a body. See RFC 7230, section 3.3.
internal static bool bodyAllowedForStatus(nint status) {
    switch (ᐧ) {
    case {} when status >= 100 && status <= 199: {
        return false;
    }
    case {} when status is 204: {
        return false;
    }
    case {} when status is 304: {
        return false;
    }}

    return true;
}

internal static slice<@string> suppressedHeaders304 = new @string[]{"Content-Type"u8, "Content-Length"u8, "Transfer-Encoding"u8}.slice();
internal static slice<@string> suppressedHeadersNoBody = new @string[]{"Content-Length"u8, "Transfer-Encoding"u8}.slice();
internal static map<@string, bool> excludedHeadersNoBody = new map<@string, bool>{["Content-Length"u8] = true, ["Transfer-Encoding"u8] = true};

internal static slice<@string> suppressedHeaders(nint status) {
    switch (ᐧ) {
    case {} when status is 304: {
        return suppressedHeaders304;
    }
    case {} when !bodyAllowedForStatus(status): {
        return suppressedHeadersNoBody;
    }}

    // RFC 7232 section 4.1
    return default!;
}

// msg is *Request or *Response.
internal static error /*err*/ readTransfer(any msg, ж<bufio.Reader> Ꮡr) {
    error err = default!;

    var t = Ꮡ(new transferReader(RequestMethod: "GET"u8));
    // Unify input
    var isResponse = false;
    switch (msg.type()) {
    case ж<Response> rr: {
        t.Value.Header = rr.Value.Header;
        t.Value.StatusCode = rr.Value.StatusCode;
        t.Value.ProtoMajor = rr.Value.ProtoMajor;
        t.Value.ProtoMinor = rr.Value.ProtoMinor;
        t.Value.Close = shouldClose((~t).ProtoMajor, (~t).ProtoMinor, (~t).Header, true);
        isResponse = true;
        if ((~rr).Request != nil) {
            t.Value.RequestMethod = rr.Value.Request.Value.Method;
        }
        break;
    }
    case ж<Request> rr: {
        t.Value.Header = rr.Value.Header;
        t.Value.RequestMethod = rr.Value.Method;
        t.Value.ProtoMajor = rr.Value.ProtoMajor;
        t.Value.ProtoMinor = rr.Value.ProtoMinor;
        t.Value.StatusCode = 200;
        t.Value.Close = rr.Value.Close;
        break;
    }
    default: {
        var rr = msg;
        throw panic("unexpected type");
        break;
    }}
    // Transfer semantics for Requests are exactly like those for
    // Responses with status code 200, responding to a GET method
    // Default to HTTP/1.1
    if ((~t).ProtoMajor == 0 && (~t).ProtoMinor == 0) {
        (t.Value.ProtoMajor, t.Value.ProtoMinor) = (1, 1);
    }
    // Transfer-Encoding: chunked, and overriding Content-Length.
    {
        var errΔ1 = t.parseTransferEncoding(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    (var realLength, err) = fixLength(isResponse, (~t).StatusCode, (~t).RequestMethod, (~t).Header, (~t).Chunked);
    if (err != default!) {
        return err;
    }
    if (isResponse && (~t).RequestMethod == "HEAD"u8){
        {
            var (n, errΔ2) = parseContentLength((~t).Header[contentLengthˢ]); if (errΔ2 != default!){
                return errΔ2;
            } else {
                t.Value.ContentLength = n;
            }
        }
    } else {
        t.Value.ContentLength = realLength;
    }
    // Trailer
    (t.Value.Trailer, err) = fixTrailer((~t).Header, (~t).Chunked);
    if (err != default!) {
        return err;
    }
    // If there is no Content-Length or chunked Transfer-Encoding on a *Response
    // and the status is not 1xx, 204 or 304, then the body is unbounded.
    // See RFC 7230, section 3.3.
    switch (msg.type()) {
    case ж<Response>: {
        if (realLength == -1 && !(~t).Chunked && bodyAllowedForStatus((~t).StatusCode)) {
            // Unbounded body.
            t.Value.Close = true;
        }
        break;
    }}

    // Prepare body reader. ContentLength < 0 means chunked encoding
    // or close connection when finished, since multipart is not supported yet
    switch (ᐧ) {
    case {} when (~t).Chunked: {
        if (isResponse && (noResponseBodyExpected((~t).RequestMethod) || !bodyAllowedForStatus((~t).StatusCode))){
            t.Value.Body = NoBody;
        } else {
            t.Value.Body = new bodyжReadCloser(Ꮡ(new body(src: @internal.NewChunkedReader(new bufio_ReaderжReader(Ꮡr)), hdr: msg, r: Ꮡr, closing: (~t).Close)));
        }
        break;
    }
    case {} when realLength is 0: {
        t.Value.Body = NoBody;
        break;
    }
    case {} when realLength is > 0: {
        t.Value.Body = new bodyжReadCloser(Ꮡ(new body(src: io.LimitReader(new bufio_ReaderжReader(Ꮡr), realLength), closing: (~t).Close)));
        break;
    }
    default: {
        if ((~t).Close){
            // realLength < 0, i.e. "Content-Length" not mentioned in header
            // Close semantics (i.e. HTTP/1.0)
            t.Value.Body = new bodyжReadCloser(Ꮡ(new body(src: new bufio_ReaderжReader(Ꮡr), closing: (~t).Close)));
        } else {
            // Persistent connection (i.e. HTTP/1.1)
            t.Value.Body = NoBody;
        }
        break;
    }}

    // Unify output
    switch (msg.type()) {
    case ж<Request> rr: {
        rr.Value.Body = t.Value.Body;
        rr.Value.ContentLength = t.Value.ContentLength;
        if ((~t).Chunked) {
            rr.Value.TransferEncoding = new @string[]{"chunked"u8}.slice();
        }
        rr.Value.Close = t.Value.Close;
        rr.Value.Trailer = t.Value.Trailer;
        break;
    }
    case ж<Response> rr: {
        rr.Value.Body = t.Value.Body;
        rr.Value.ContentLength = t.Value.ContentLength;
        if ((~t).Chunked) {
            rr.Value.TransferEncoding = new @string[]{"chunked"u8}.slice();
        }
        rr.Value.Close = t.Value.Close;
        rr.Value.Trailer = t.Value.Trailer;
        break;
    }}
    return default!;
}

// Checks whether chunked is part of the encodings stack.
internal static bool chunked(slice<@string> te) {
    return builtin.len(te) > 0 && te[0] == "chunked";
}

// Checks whether the encoding is explicitly "identity".
internal static bool isIdentity(slice<@string> te) {
    return builtin.len(te) == 1 && te[0] == "identity";
}

// unsupportedTEError reports unsupported transfer-encodings.
[GoType] partial struct unsupportedTEError {
    internal @string err;
}

[GoRecv] internal static @string Error(this ref unsupportedTEError uste) {
    return uste.err;
}

// isUnsupportedTEError checks if the error is of type
// unsupportedTEError. It is usually invoked with a non-nil err.
internal static bool isUnsupportedTEError(error err) {
    var (_, ok) = err._<ж<unsupportedTEError>>(ᐧ);
    return ok;
}

// parseTransferEncoding sets t.Chunked based on the Transfer-Encoding header.
[GoRecv] internal static error parseTransferEncoding(this ref transferReader t) {
    var (raw, present) = t.Header[transferEncodingˢ, ꟷ];
    if (!present) {
        return default!;
    }
    delete(t.Header, "Transfer-Encoding"u8);
    // Issue 12785; ignore Transfer-Encoding on HTTP/1.0 requests.
    if (!t.protoAtLeast(1, 1)) {
        return default!;
    }
    // Like nginx, we only support a single Transfer-Encoding header field, and
    // only if set to "chunked". This is one of the most security sensitive
    // surfaces in HTTP/1.1 due to the risk of request smuggling, so we keep it
    // strict and simple.
    if (builtin.len(raw) != 1) {
        return new unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(fmt.Sprintf("too many transfer encodings: %q"u8, raw))));
    }
    if (!ascii.EqualFold(raw[0], chunkedˢ)) {
        return new unsupportedTEErrorжerror(Ꮡ(new unsupportedTEError(fmt.Sprintf("unsupported transfer encoding: %q"u8, raw[0]))));
    }
    t.Chunked = true;
    return default!;
}

// Determine the expected body length, using RFC 7230 Section 3.3. This
// function is not a method, because ultimately it should be shared by
// ReadResponse and ReadRequest.
internal static (int64 n, error err) fixLength(bool isResponse, nint status, @string requestMethod, ΔHeader header, bool chunked) {
    int64 n = default!;
    error err = default!;

    var isRequest = !isResponse;
    var contentLens = header[contentLengthˢ];
    // Hardening against HTTP request smuggling
    if (builtin.len(contentLens) > 1) {
        // Per RFC 7230 Section 3.3.2, prevent multiple
        // Content-Length headers if they differ in value.
        // If there are dups of the value, remove the dups.
        // See Issue 16490.
        @string first = textproto.TrimString(contentLens[0]);
        foreach (var (_, ct) in contentLens[1..]) {
            if (first != textproto.TrimString(ct)) {
                return (0, fmt.Errorf("http: message cannot contain multiple Content-Length headers; got %q"u8, contentLens));
            }
        }
        // deduplicate Content-Length
        header.Del(contentLengthˢ);
        header.Add(contentLengthˢ, first);
        contentLens = header[contentLengthˢ];
    }
    // Reject requests with invalid Content-Length headers.
    if (builtin.len(contentLens) > 0) {
        (n, err) = parseContentLength(contentLens);
        if (err != default!) {
            return (-1, err);
        }
    }
    // Logic based on response type or status
    if (isResponse && noResponseBodyExpected(requestMethod)) {
        return (0, default!);
    }
    if (status / 100 == 1) {
        return (0, default!);
    }
    switch (status) {
    case 204 or 304: {
        return (0, default!);
    }}

    // According to RFC 9112, "If a message is received with both a
    // Transfer-Encoding and a Content-Length header field, the Transfer-Encoding
    // overrides the Content-Length. Such a message might indicate an attempt to
    // perform request smuggling (Section 11.2) or response splitting (Section 11.1)
    // and ought to be handled as an error. An intermediary that chooses to forward
    // the message MUST first remove the received Content-Length field and process
    // the Transfer-Encoding (as described below) prior to forwarding the message downstream."
    //
    // Chunked-encoding requests with either valid Content-Length
    // headers or no Content-Length headers are accepted after removing
    // the Content-Length field from header.
    //
    // Logic based on Transfer-Encoding
    if (chunked) {
        header.Del(contentLengthˢ);
        return (-1, default!);
    }
    // Logic based on Content-Length
    if (builtin.len(contentLens) > 0) {
        return (n, default!);
    }
    header.Del(contentLengthˢ);
    if (isRequest) {
        // RFC 7230 neither explicitly permits nor forbids an
        // entity-body on a GET request so we permit one if
        // declared, but we default to 0 here (not -1 below)
        // if there's no mention of a body.
        // Likewise, all other request methods are assumed to have
        // no body if neither Transfer-Encoding chunked nor a
        // Content-Length are set.
        return (0, default!);
    }
    // Body-EOF logic based on other methods (like closing, or chunked coding)
    return (-1, default!);
}

// Determine whether to hang up after sending a request and body, or
// receiving a response and body
// 'header' is the request headers.
internal static bool shouldClose(nint major, nint minor, ΔHeader header, bool removeCloseHeader) {
    if (major < 1) {
        return true;
    }
    var conv = header[connectionˢ];
    var hasClose = httpguts.HeaderValuesContainsToken(conv, closeˢ);
    if (major == 1 && minor == 0) {
        return hasClose || !httpguts.HeaderValuesContainsToken(conv, keepAliveˢ);
    }
    if (hasClose && removeCloseHeader) {
        header.Del(connectionˢ);
    }
    return hasClose;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTrailerKeyˢ = "bad trailer key"u8;

// Parse the trailer header.
internal static (ΔHeader, error) fixTrailer(ΔHeader header, bool chunked) {
    var (vv, ok) = header[trailerˢ, ꟷ];
    if (!ok) {
        return (default!, default!);
    }
    if (!chunked) {
        // Trailer and no chunking:
        // this is an invalid use case for trailer header.
        // Nevertheless, no error will be returned and we
        // let users decide if this is a valid HTTP message.
        // The Trailer header will be kept in Response.Header
        // but not populate Response.Trailer.
        // See issue #27197.
        return (default!, default!);
    }
    header.Del(trailerˢ);
    var trailer = new ΔHeader(0);
    ref var err = ref heap<error>(out var Ꮡerr);
    foreach (var (_, v) in vv) {
        var trailerʗ1 = trailer;
        foreachHeaderElement(v, (@string key) => {
            key = CanonicalHeaderKey(key);
            var exprᴛ1 = key;
            if (exprᴛ1 == "Transfer-Encoding"u8 || exprᴛ1 == "Trailer"u8 || exprᴛ1 == "Content-Length"u8) {
                if (Ꮡerr.ValueSlot == default!) {
                    Ꮡerr.ValueSlot = badStringError(badTrailerKeyˢ, key);
                    return;
                }
            }

            trailerʗ1[key] = default!;
        });
    }
    if (err != default!) {
        return (default!, err);
    }
    if (builtin.len(trailer) == 0) {
        return (default!, default!);
    }
    return (trailer, default!);
}

// body turns a Reader into a ReadCloser.
// Close ensures that the body has been fully read
// and then reads the trailer if necessary.
[GoType] partial struct body {
    internal io.Reader src;
    internal any hdr;           // non-nil (Response or Request) value means read trailer
    internal ж<bufio.Reader> r; // underlying wire-format reader for the trailer
    internal bool closing;          // is the connection to be closed after reading body?
    internal bool doEarlyClose;          // whether Close should stop early
    internal sync.Mutex mu; // guards following, and calls to Read and Close
    internal bool sawEOF;
    internal bool closed;
    internal bool earlyClose;   // Close called and we didn't read to the end of src
    internal Action onHitEOF; // if non-nil, func to call when EOF is Read
}

// ErrBodyReadAfterClose is returned when reading a [Request] or [Response]
// Body after the body has been closed. This typically happens when the body is
// read after an HTTP [Handler] calls WriteHeader or Write on its
// [ResponseWriter].
public static error ErrBodyReadAfterClose = errors.New("http: invalid Read on closed Body"u8);

internal static (nint n, error err) Read(this ж<body> Ꮡb, slice<byte> p) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.mu.Lock();
        defer(Ꮡb.of(body.Ꮡmu).Unlock, ref ᒐ);
        if (b.closed) {
            (n, err) = (0, ErrBodyReadAfterClose); goto ᒐdone;
        }
        (n, err) = b.readLocked(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

// Must hold b.mu.
[GoRecv] internal static (nint n, error err) readLocked(this ref body b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (b.sawEOF) {
        return (0, io.EOF);
    }
    (n, err) = b.src.Read(p);
    if (AreEqual(err, io.EOF)) {
        b.sawEOF = true;
        // Chunked case. Read the trailer.
        if (b.hdr != default!){
            {
                var e = b.readTrailer(); if (e != default!) {
                    err = e;
                    // Something went wrong in the trailer, we must not allow any
                    // further reads of any kind to succeed from body, nor any
                    // subsequent requests on the server connection. See
                    // golang.org/issue/12027
                    b.sawEOF = false;
                    b.closed = true;
                }
            }
            b.hdr = default!;
        } else {
            // If the server declared the Content-Length, our body is a LimitedReader
            // and we need to check whether this EOF arrived early.
            {
                var (lr, ok) = b.src._<ж<io.LimitedReader>>(ᐧ); if (ok && (~lr).N > 0) {
                    err = io.ErrUnexpectedEOF;
                }
            }
        }
    }
    // If we can return an EOF here along with the read data, do
    // so. This is optional per the io.Reader contract, but doing
    // so helps the HTTP transport code recycle its connection
    // earlier (since it will see this EOF itself), even if the
    // client doesn't do future reads or Close.
    if (err == default! && n > 0) {
        {
            var (lr, ok) = b.src._<ж<io.LimitedReader>>(ᐧ); if (ok && (~lr).N == 0) {
                err = io.EOF;
                b.sawEOF = true;
            }
        }
    }
    if (b.sawEOF && b.onHitEOF != default!) {
        b.onHitEOF();
    }
    return (n, err);
}

internal static slice<byte> singleCRLF = slice<byte>("\r\n"u8);
internal static slice<byte> doubleCRLF = slice<byte>("\r\n\r\n"u8);

internal static bool seeUpcomingDoubleCRLF(ж<bufio.Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    for (nint peekSize = 4; ᐧ ; peekSize++) {
        // This loop stops when Peek returns an error,
        // which it does when r's buffer has been filled.
        var (buf, err) = r.Peek(peekSize);
        if (bytes.HasSuffix(buf, doubleCRLF)) {
            return true;
        }
        if (err != default!) {
            break;
        }
    }
    return false;
}

internal static error errTrailerEOF = errors.New("http: unexpected EOF reading trailer"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpSuspiciouslyLongˢ = "http: suspiciously long trailer after chunked body"u8;

[GoRecv] internal static error readTrailer(this ref body b) {
    // The common case, since nobody uses trailers.
    var (buf, err) = b.r.Peek(2);
    if (bytes.Equal(buf, singleCRLF)) {
        b.r.Discard(2);
        return default!;
    }
    if (builtin.len(buf) < 2) {
        return errTrailerEOF;
    }
    if (err != default!) {
        return err;
    }
    // Make sure there's a header terminator coming up, to prevent
    // a DoS with an unbounded size Trailer. It's not easy to
    // slip in a LimitReader here, as textproto.NewReader requires
    // a concrete *bufio.Reader. Also, we can't get all the way
    // back up to our conn's LimitedReader that *might* be backing
    // this bufio.Reader. Instead, a hack: we iteratively Peek up
    // to the bufio.Reader's max size, looking for a double CRLF.
    // This limits the trailer to the underlying buffer size, typically 4kB.
    if (!seeUpcomingDoubleCRLF(b.r)) {
        return errors.New(httpSuspiciouslyLongˢ);
    }
    (var hdr, err) = textproto.NewReader(b.r).ReadMIMEHeader();
    if (err != default!) {
        if (AreEqual(err, io.EOF)) {
            return errTrailerEOF;
        }
        return err;
    }
    switch (b.hdr.type()) {
    case ж<Request> rr: {
        mergeSetHeader(ref (rr.of(Request.ᏑTrailer)).DerefOrNull(), ((ΔHeader)(map<@string, slice<@string>>)hdr));
        break;
    }
    case ж<Response> rr: {
        mergeSetHeader(ref (rr.of(Response.ᏑTrailer)).DerefOrNull(), ((ΔHeader)(map<@string, slice<@string>>)hdr));
        break;
    }}
    return default!;
}

internal static void mergeSetHeader(ref ΔHeader dst, ΔHeader src) {
    if (dst == default!) {
        dst = src;
        return;
    }
    maps.Copy<ΔHeader, ΔHeader, @string, slice<@string>>(dst, src);
}

// unreadDataSizeLocked returns the number of bytes of unread input.
// It returns -1 if unknown.
// b.mu must be held.
[GoRecv] internal static int64 unreadDataSizeLocked(this ref body b) {
    {
        var (lr, ok) = b.src._<ж<io.LimitedReader>>(ᐧ); if (ok) {
            return (~lr).N;
        }
    }
    return -1;
}

internal static error Close(this ж<body> Ꮡb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.mu.Lock();
        ᒐd1 = true;
        if (b.closed) {
            return default!;
        }
        error err = default!;
        switch (ᐧ) {
        case {} when b.sawEOF: {
            break;
        }
        case {} when b.hdr == default! && b.closing: {
            break;
        }
        case {} when b.doEarlyClose: {
            {
                var (lr, ok) = b.src._<ж<io.LimitedReader>>(ᐧ); if (ok && (~lr).N > maxPostHandlerReadBytes){
                    // Already saw EOF, so no need going to look for it.
                    // no trailer and closing the connection next.
                    // no point in reading to EOF.
                    // Read up to maxPostHandlerReadBytes bytes of the body, looking
                    // for EOF (and trailers), so we can re-use this connection.
                    // There was a declared Content-Length, and we have more bytes remaining
                    // than our maxPostHandlerReadBytes tolerance. So, give up.
                    b.earlyClose = true;
                } else {
                    int64 n = default!;
                    // Consume the body, or, which will also lead to us reading
                    // the trailer headers after the body, if present.
                    (n, err) = io.CopyN(io.Discard, new bodyLocked(Ꮡb), maxPostHandlerReadBytes);
                    if (AreEqual(err, io.EOF)) {
                        err = default!;
                    }
                    if (n == maxPostHandlerReadBytes) {
                        b.earlyClose = true;
                    }
                }
            }
            break;
        }
        default: {
            (_, err) = io.Copy(io.Discard, // Fully consume the body, which will also lead to us reading
 // the trailer headers after the body, if present.
 new bodyLocked(Ꮡb));
            break;
        }}

        b.closed = true;
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static bool didEarlyClose(this ж<body> Ꮡb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.mu.Lock();
        ᒐd1 = true;
        return b.earlyClose;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// bodyRemains reports whether future Read calls might
// yield data.
internal static bool bodyRemains(this ж<body> Ꮡb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.mu.Lock();
        ᒐd1 = true;
        return !b.sawEOF;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void registerOnHitEOF(this ж<body> Ꮡb, Action fn) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.mu.Lock();
        ᒐd1 = true;
        b.onHitEOF = fn;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// bodyLocked is an io.Reader reading from a *body when its mutex is
// already held.
[GoType] partial struct bodyLocked {
    internal ж<body> b;
}

internal static (nint n, error err) Read(this bodyLocked bl, slice<byte> p) {
    if ((~bl.b).closed) {
        return (0, ErrBodyReadAfterClose);
    }
    return bl.b.readLocked(p);
}

internal static ж<godebug.Setting> httplaxcontentlength = godebug.New("httplaxcontentlength"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidEmptyContentˢ = "invalid empty Content-Length"u8;
internal static readonly @string badContentLengthˢ = "bad Content-Length"u8;

// parseContentLength checks that the header is valid and then trims
// whitespace. It returns -1 if no value is set otherwise the value
// if it's >= 0.
internal static (int64, error) parseContentLength(slice<@string> clHeaders) {
    if (builtin.len(clHeaders) == 0) {
        return (-1, default!);
    }
    @string cl = textproto.TrimString(clHeaders[0]);
    // The Content-Length must be a valid numeric value.
    // See: https://datatracker.ietf.org/doc/html/rfc2616/#section-14.13
    if (cl == ""u8) {
        if (httplaxcontentlength.Value() == "1"u8) {
            httplaxcontentlength.IncNonDefault();
            return (-1, default!);
        }
        return (0, badStringError(invalidEmptyContentˢ, cl));
    }
    var (n, err) = strconv.ParseUint(cl, 10, 63);
    if (err != default!) {
        return (0, badStringError(badContentLengthˢ, cl));
    }
    return ((int64)n, default!);
}

// finishAsyncByteRead finishes reading the 1-byte sniff
// from the ContentLength==0, Body!=nil case.
[GoType] partial struct finishAsyncByteRead {
    internal ж<transferWriter> tw;
}

internal static (nint n, error err) Read(this finishAsyncByteRead fr, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (builtin.len(p) == 0) {
        return (n, err);
    }
    var rres = ᐸꟷ((~fr.tw).ByteReadCh);
    (n, err) = (rres.n, rres.err);
    if (n == 1) {
        p[0] = rres.b;
    }
    if (err == default!) {
        err = io.EOF;
    }
    return (n, err);
}

internal static reflectꓸType nopCloserType = reflect.TypeOf(io.NopCloser(default!));


    [GoType("dyn")] partial struct rᴛ1 {
        public io_package.Reader Reader;
        public io_package.WriterTo WriterTo;
    }
internal static reflectꓸType nopCloserWriterToType = reflect.TypeOf(io.NopCloser(new rᴛ1()));

// unwrapNopCloser return the underlying reader and true if r is a NopCloser
// else it return false.
internal static (io.Reader underlyingReader, bool isNopCloser) unwrapNopCloser(io.Reader r) {
    var exprᴛ1 = reflect.TypeOf(r);
    if (AreEqual(exprᴛ1, nopCloserType) || AreEqual(exprᴛ1, nopCloserWriterToType)) {
        return (reflect.ValueOf(r).Field(0).Interface()._<io.Reader>(), true);
    }
    { /* default: */
        return (default!, false);
    }

}

// isKnownInMemoryReader reports whether r is a type known to not
// block on Read. Its caller uses this as an optional optimization to
// send fewer TCP packets.
internal static bool isKnownInMemoryReader(io.Reader r) {
    switch (r.type()) {
    case ж<bytes.Reader> _:
    case ж<bytes.Buffer> _:
    case ж<strings.Reader> _: {
        return true;
    }}

    {
        var (rΔ1, ok) = unwrapNopCloser(r); if (ok) {
            return isKnownInMemoryReader(rΔ1);
        }
    }
    {
        var (rΔ2, ok) = r._<ж<readTrackingBody>>(ᐧ); if (ok) {
            return isKnownInMemoryReader((~rΔ2).ReadCloser);
        }
    }
    return false;
}

// bufioFlushWriter is an io.Writer wrapper that flushes all writes
// on its wrapped writer if it's a *bufio.Writer.
[GoType] partial struct bufioFlushWriter {
    internal io.Writer w;
}

internal static (nint n, error err) Write(this bufioFlushWriter fw, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = fw.w.Write(p);
    {
        var (bw, ok) = fw.w._<ж<bufio.Writer>>(ᐧ); if (n > 0 && ok) {
            var ferr = bw.Flush();
            if (ferr != default! && err == default!) {
                err = ferr;
            }
        }
    }
    return (n, err);
}

} // end http_package
