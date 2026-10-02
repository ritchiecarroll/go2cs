// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using url = go.net.url_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;
using go.net;

partial class httputil_package {

// drainBody reads all of b to memory and then returns two equivalent
// ReadClosers yielding the same bytes.
//
// It returns an error if the initial slurp of all bytes fails. It does not attempt
// to make the returned ReadClosers have identical error-matching behavior.
internal static (io.ReadCloser r1, io.ReadCloser r2, error err) drainBody(io.ReadCloser b) {
    error err = default!;

    if (b == default! || AreEqual(b, http.NoBody)) {
        // No copying needed. Preserve the magic sentinel meaning of NoBody.
        return (http.NoBody, http.NoBody, default!);
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        (_, err) = buf.ReadFrom(b); if (err != default!) {
            return (default!, b, err);
        }
    }
    {
        err = b.Close(); if (err != default!) {
            return (default!, b, err);
        }
    }
    return (io.NopCloser(new bytes_BufferжReader(Ꮡbuf)), io.NopCloser(new bytes_ReaderжReader(bytes.NewReader(buf.Bytes()))), default!);
}

// dumpConn is a net.Conn which writes to Writer and reads from Reader
[GoType] partial struct dumpConn {
    [GoEmbedded] public io_package.Writer Writer;
    [GoEmbedded] public io_package.Reader Reader;
}

[GoRecv] internal static error Close(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static netꓸAddr LocalAddr(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static netꓸAddr RemoteAddr(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static error SetDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetReadDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoType("num:byte")] partial struct neverEnding;

internal static (nint n, error err) Read(this neverEnding b, slice<byte> p) {
    foreach (var (i, _) in p) {
        p[i] = (byte)b;
    }
    return (len(p), default!);
}

// outgoingLength is a copy of the unexported
// (*http.Request).outgoingLength method.
internal static int64 outgoingLength(ref http.Request req) {
    if (req.Body == default! || AreEqual(req.Body, http.NoBody)) {
        return 0;
    }
    if (req.ContentLength != 0) {
        return req.ContentLength;
    }
    return -1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpˢ = "http"u8;
internal static readonly @string http11204NoContentˢ = "HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"u8;

// DumpRequestOut is like [DumpRequest] but for outgoing client requests. It
// includes any headers that the standard [http.Transport] adds, such as
// User-Agent.
[MethodImpl(MethodImplOptions.NoInlining)] public static (slice<byte>, error) DumpRequestOut(ж<http.Request> Ꮡreq, bool body) {
    GoFrame ᒐ = default;
    try {
        ref var req = ref Ꮡreq.DerefOrNull();

        var save = req.Body;
        var dummyBody = false;
        if (!body){
            var contentLength = outgoingLength(ref (Ꮡreq).DerefOrNull());
            if (contentLength != 0) {
                req.Body = io.NopCloser(io.LimitReader(((neverEnding)(rune)'x'), contentLength));
                dummyBody = true;
            }
        } else {
            error errΔ1 = default!;
            (save, req.Body, errΔ1) = drainBody(req.Body);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        // Since we're using the actual Transport code to write the request,
        // switch to http so the Transport doesn't try to do an SSL
        // negotiation with our dumpConn and its bytes.Buffer & pipe.
        // The wire format for https and http are the same, anyway.
        var reqSend = Ꮡreq;
        if ((~req.URL).Scheme == "https"u8) {
            reqSend = @new<http.Request>();
            reqSend.Value = req;
            reqSend.Value.URL = @new<url.URL>();
            (~reqSend).URL.Value = req.URL.Value;
            reqSend.Value.URL.Value.Scheme = httpˢ;
        }
        // Use the actual Transport code to record what we would send
        // on the wire, but not using TCP.  Use a Transport with a
        // custom dialer that returns a fake net.Conn that waits
        // for the full input (and recording it), and then responds
        // with a dummy response.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);                       // records the output
        var (pr, pw) = io.Pipe();
        var prʗ1 = pr;
        defer(() => prʗ1.Close(), ref ᒐ);
        var pwʗ1 = pw;
        defer(() => pwʗ1.Close(), ref ᒐ);
        var dr = Ꮡ(new delegateReader(c: new channel<io.Reader>(0)));
            var drʗ1 = dr;
            var pwʗ2 = pw;
        var t = Ꮡ(new http.Transport(
            Dial: (@string netΔ1, @string addr) => (new dumpConnжConn(Ꮡ(new dumpConn(io.MultiWriter(new bytes_BufferжWriter(Ꮡbuf), new io.PipeWriterжWriter(pwʗ2)), new delegateReaderжReader(drʗ1)))), default!)
        ));
        var tʗ1 = t;
        defer(tʗ1.CloseIdleConnections, ref ᒐ);
        // We need this channel to ensure that the reader
        // goroutine exits if t.RoundTrip returns an error.
        // See golang.org/issue/32571.
        var quitReadCh = new channel<EmptyStruct>(0);
        // Wait for the request before replying with a dummy response:
        var drʗ2 = dr;
        var prʗ2 = pr;
        var quitReadChʗ1 = quitReadCh;
        goǃ(() => {
            var (reqΔ1, errΔ2) = http.ReadRequest(bufio.NewReader(new io.PipeReaderжReader(prʗ2)));
            if (errΔ2 == default!) {
                // Ensure all the body is read; otherwise
                // we'll get a partial dump.
                io.Copy(io.Discard, (~reqΔ1).Body);
                (~reqΔ1).Body.Close();
            }
            var selᴛ1 = (~drʗ2).c.ᐸꟷ(new strings_ReaderжReader(strings.NewReader(http11204NoContentˢ)), ꓸꓸꓸ);
            var selᴛ2 = quitReadChʗ1;
            switch (select(selᴛ1, ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0: {
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
                close((~drʗ2).c);
                break;
            }}
        });
        // Ensure delegateReader.Read doesn't block forever if we get an error.
        var (_, err) = t.RoundTrip(reqSend);
        req.Body = save;
        if (err != default!) {
            pw.Close();
            dr.Value.err = err;
            close(quitReadCh);
            return (default!, err);
        }
        var dump = buf.Bytes();
        // If we used a dummy body above, remove it now.
        // TODO: if the req.ContentLength is large, we allocate memory
        // unnecessarily just to slice it off here. But this is just
        // a debug function, so this is acceptable for now. We could
        // discard the body earlier if this matters.
        if (dummyBody) {
            {
                nint i = bytes.Index(dump, slice<byte>("\r\n\r\n"u8)); if (i >= 0) {
                    dump = dump.slice(0, i + 4);
                }
            }
        }
        return (dump, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// delegateReader is a reader that delegates to another reader,
// once it arrives on a channel.
[GoType] partial struct delegateReader {
    internal channel<io.Reader> c;
    internal error err;     // only used if r is nil and c is closed.
    internal io.Reader r; // nil until received from c
}

[GoRecv] internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) {
    if (r.r == default!) {
        bool ok = default!;
        {
            (r.r, ok) = ᐸꟷ(r.c, ꟷ); if (!ok) {
                return (0, r.err);
            }
        }
    }
    return r.r.Read(p);
}

// Return value if nonempty, def otherwise.
internal static @string valueOrDefault(@string value, @string def) {
    if (value != ""u8) {
        return value;
    }
    return def;
}

// not in Header map anyway
internal static map<@string, bool> reqWriteExcludeHeaderDump = new map<@string, bool>{
    ["Host"u8] = true,
    ["Transfer-Encoding"u8] = true,
    ["Trailer"u8] = true
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ = "GET"u8;
internal static readonly @string httpˢ2 = "http://"u8;
internal static readonly @string httpsˢ = "https://"u8;

// DumpRequest returns the given request in its HTTP/1.x wire
// representation. It should only be used by servers to debug client
// requests. The returned representation is an approximation only;
// some details of the initial request are lost while parsing it into
// an [http.Request]. In particular, the order and case of header field
// names are lost. The order of values in multi-valued headers is kept
// intact. HTTP/2 requests are dumped in HTTP/1.x form, not in their
// original binary representations.
//
// If body is true, DumpRequest also returns the body. To do so, it
// consumes req.Body and then replaces it with a new [io.ReadCloser]
// that yields the same bytes. If DumpRequest returns an error,
// the state of req is undefined.
//
// The documentation for [http.Request.Write] details which fields
// of req are included in the dump.
public static (slice<byte>, error) DumpRequest(ж<http.Request> Ꮡreq, bool body) {
    ref var req = ref Ꮡreq.DerefOrNull();

    error err = default!;
    var save = req.Body;
    if (!body || req.Body == default!){
        req.Body = default!;
    } else {
        (save, req.Body, err) = drainBody(req.Body);
        if (err != default!) {
            return (default!, err);
        }
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    // By default, print out the unmodified req.RequestURI, which
    // is always set for incoming server requests. But because we
    // previously used req.URL.RequestURI and the docs weren't
    // always so clear about when to use DumpRequest vs
    // DumpRequestOut, fall back to the old way if the caller
    // provides a non-server Request.
    @string reqURI = req.RequestURI;
    if (reqURI == ""u8) {
        reqURI = req.URL.RequestURI();
    }
    fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "%s %s HTTP/%d.%d\r\n"u8, valueOrDefault(req.Method, getˢ),
        reqURI, req.ProtoMajor, req.ProtoMinor);
    var absRequestURI = strings.HasPrefix(req.RequestURI, httpˢ2) || strings.HasPrefix(req.RequestURI, httpsˢ);
    if (!absRequestURI) {
        @string host = req.Host;
        if (host == ""u8 && req.URL != nil) {
            host = req.URL.Value.Host;
        }
        if (host != ""u8) {
            fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "Host: %s\r\n"u8, host);
        }
    }
    var chunked = len(req.TransferEncoding) > 0 && req.TransferEncoding[0] == "chunked";
    if (len(req.TransferEncoding) > 0) {
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "Transfer-Encoding: %s\r\n"u8, strings.Join(req.TransferEncoding, ","u8));
    }
    err = req.Header.WriteSubset(new bytes_BufferжWriter(Ꮡb), reqWriteExcludeHeaderDump);
    if (err != default!) {
        return (default!, err);
    }
    io.WriteString(new bytes_BufferжWriter(Ꮡb), "\r\n"u8);
    if (req.Body != default!) {
        io.Writer dest = new bytes_BufferжWriter(Ꮡb);
        if (chunked) {
            dest = new io_WriteCloserᴠWriter(NewChunkedWriter(dest));
        }
        (_, err) = io.Copy(dest, req.Body);
        if (chunked) {
            dest._<io.Closer>().Close();
            io.WriteString(new bytes_BufferжWriter(Ꮡb), "\r\n"u8);
        }
    }
    req.Body = save;
    if (err != default!) {
        return (default!, err);
    }
    return (b.Bytes(), default!);
}

// errNoBody is a sentinel error value used by failureToReadBody so we
// can detect that the lack of body was intentional.
internal static error errNoBody = errors.New("sentinel error value"u8);

// failureToReadBody is an io.ReadCloser that just returns errNoBody on
// Read. It's swapped in when we don't actually want to consume
// the body, but need a non-nil one, and want to distinguish the
// error from reading the dummy body.
[GoType] partial struct failureToReadBody {
}

internal static (nint, error) Read(this failureToReadBody _Δp0, slice<byte> _Δp1) {
    return (0, errNoBody);
}

internal static error Close(this failureToReadBody _) {
    return default!;
}

// emptyBody is an instance of empty reader.
internal static io.ReadCloser emptyBody = io.NopCloser(new strings_ReaderжReader(strings.NewReader(""u8)));

// DumpResponse is like DumpRequest but dumps a response.
public static (slice<byte>, error) DumpResponse(ж<http.Response> Ꮡresp, bool body) {
    ref var resp = ref Ꮡresp.DerefOrNull();

    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    error err = default!;
    var save = resp.Body;
    var savecl = resp.ContentLength;
    if (!body){
        // For content length of zero. Make sure the body is an empty
        // reader, instead of returning error through failureToReadBody{}.
        if (resp.ContentLength == 0){
            resp.Body = emptyBody;
        } else {
            resp.Body = new failureToReadBody(nil);
        }
    } else 
    if (resp.Body == default!){
        resp.Body = emptyBody;
    } else {
        (save, resp.Body, err) = drainBody(resp.Body);
        if (err != default!) {
            return (default!, err);
        }
    }
    err = resp.Write(new bytes_BufferжWriter(Ꮡb));
    if (AreEqual(err, errNoBody)) {
        err = default!;
    }
    resp.Body = save;
    resp.ContentLength = savecl;
    if (err != default!) {
        return (default!, err);
    }
    return (b.Bytes(), default!);
}

} // end httputil_package
