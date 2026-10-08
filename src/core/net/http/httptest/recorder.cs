// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using http = go.net.http_package;
using textproto = go.net.textproto_package;
using strconv = strconv_package;
using strings = strings_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using go.net;
using vendor.golang.org.x.net.http;

partial class httptest_package {

// ResponseRecorder is an implementation of [http.ResponseWriter] that
// records its mutations for later inspection in tests.
partial struct ResponseRecorder {
    // Code is the HTTP response code set by WriteHeader.
    //
    // Note that if a Handler never calls WriteHeader or Write,
    // this might end up being 0, rather than the implicit
    // http.StatusOK. To get the implicit value, use the Result
    // method.
    public nint Code;
    // HeaderMap contains the headers explicitly set by the Handler.
    // It is an internal detail.
    //
    // Deprecated: HeaderMap exists for historical compatibility
    // and should not be used. To access the headers returned by a handler,
    // use the Response.Header map as returned by the Result method.
    public httpꓸHeader HeaderMap;
    // Body is the buffer to which the Handler's Write calls are sent.
    // If nil, the Writes are silently discarded.
    public ж<bytes.Buffer> Body;
    // Flushed is whether the Handler called Flush.
    public bool Flushed;
    internal ж<http.Response> result; // cache of Result's return value
    internal httpꓸHeader snapHeader;    // snapshot of HeaderMap at first Write
    internal bool wroteHeader;
}

// NewRecorder returns an initialized [ResponseRecorder].
public static ж<ResponseRecorder> NewRecorder() {
    return Ꮡ(new ResponseRecorder(
        HeaderMap: new httpꓸHeader(0),
        Body: @new<bytes.Buffer>(),
        Code: 200
    ));
}

// DefaultRemoteAddr is the default remote address to return in RemoteAddr if
// an explicit DefaultRemoteAddr isn't set on [ResponseRecorder].
public static readonly @string DefaultRemoteAddr = "1.2.3.4"u8;

// Header implements [http.ResponseWriter]. It returns the response
// headers to mutate within a handler. To test the headers that were
// written after a handler completes, use the [ResponseRecorder.Result] method and see
// the returned Response value's Header.
public static httpꓸHeader Header(this ref ResponseRecorder rw) {
    var m = rw.HeaderMap;
    if (m == default!) {
        m = new httpꓸHeader(0);
        rw.HeaderMap = m;
    }
    return m;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTypeˢ = "Content-Type"u8;
internal static readonly @string transferEncodingˢ = "Transfer-Encoding"u8;

// writeHeader writes a header if it was not written yet and
// detects Content-Type if needed.
//
// bytes or str are the beginning of the response body.
// We pass both to avoid unnecessarily generate garbage
// in rw.WriteString which was created for performance reasons.
// Non-nil bytes win.
internal static void writeHeader(this ref ResponseRecorder rw, slice<byte> b, @string str) {
    if (rw.wroteHeader) {
        return;
    }
    if (len(str) > 512) {
        str = str[..512];
    }
    var m = rw.Header();
    var (_, hasType) = m[contentTypeˢ, ꟷ];
    var hasTE = m.Get(transferEncodingˢ) != ""u8;
    if (!hasType && !hasTE) {
        if (b == default!) {
            b = slice<byte>(str);
        }
        m.Set(contentTypeˢ, http.DetectContentType(b));
    }
    rw.WriteHeader(200);
}

// Write implements http.ResponseWriter. The data in buf is written to
// rw.Body, if not nil.
public static (nint, error) Write(this ref ResponseRecorder rw, slice<byte> buf) {
    rw.writeHeader(buf, ""u8);
    if (rw.Body != nil) {
        rw.Body.Write(buf);
    }
    return (len(buf), default!);
}

// WriteString implements [io.StringWriter]. The data in str is written
// to rw.Body, if not nil.
public static (nint, error) WriteString(this ref ResponseRecorder rw, @string str) {
    rw.writeHeader(default!, str);
    if (rw.Body != nil) {
        rw.Body.WriteString(str);
    }
    return (len(str), default!);
}

internal static void checkWriteHeaderCode(nint code) {
    // Issue 22880: require valid WriteHeader status codes.
    // For now we only enforce that it's three digits.
    // In the future we might block things over 599 (600 and above aren't defined
    // at https://httpwg.org/specs/rfc7231.html#status.codes)
    // and we might block under 200 (once we have more mature 1xx support).
    // But for now any three digits.
    //
    // We used to send "HTTP/1.1 000 0" on the wire in responses but there's
    // no equivalent bogus thing we can realistically send in HTTP/2,
    // so we'll consistently panic instead and help people find their bugs
    // early. (We can't return an error from WriteHeader even if we wanted to.)
    if (code < 100 || code > 999) {
        throw panic(fmt.Sprintf("invalid WriteHeader code %v"u8, code));
    }
}

// WriteHeader implements [http.ResponseWriter].
public static void WriteHeader(this ref ResponseRecorder rw, nint code) {
    if (rw.wroteHeader) {
        return;
    }
    checkWriteHeaderCode(code);
    rw.Code = code;
    rw.wroteHeader = true;
    if (rw.HeaderMap == default!) {
        rw.HeaderMap = new httpꓸHeader(0);
    }
    rw.snapHeader = rw.HeaderMap.Clone();
}

// Flush implements [http.Flusher]. To test whether Flush was
// called, see rw.Flushed.
public static void Flush(this ref ResponseRecorder rw) {
    if (!rw.wroteHeader) {
        rw.WriteHeader(200);
    }
    rw.Flushed = true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentLengthˢ = "Content-Length"u8;
internal static readonly @string trailerˢ = "Trailer"u8;

// Result returns the response generated by the handler.
//
// The returned Response will have at least its StatusCode,
// Header, Body, and optionally Trailer populated.
// More fields may be populated in the future, so callers should
// not DeepEqual the result in tests.
//
// The Response.Header is a snapshot of the headers at the time of the
// first write call, or at the time of this call, if the handler never
// did a write.
//
// The Response.Body is guaranteed to be non-nil and Body.Read call is
// guaranteed to not return any error other than [io.EOF].
//
// Result must only be called after the handler has finished running.
public static ж<http.Response> Result(this ref ResponseRecorder rw) {
    if (rw.result != nil) {
        return rw.result;
    }
    if (rw.snapHeader == default!) {
        rw.snapHeader = rw.HeaderMap.Clone();
    }
    var res = Ꮡ(new http.Response(
        Proto: "HTTP/1.1"u8,
        ProtoMajor: 1,
        ProtoMinor: 1,
        StatusCode: rw.Code,
        Header: rw.snapHeader
    ));
    rw.result = res;
    if ((~res).StatusCode == 0) {
        res.Value.StatusCode = 200;
    }
    res.Value.Status = fmt.Sprintf("%03d %s"u8, (~res).StatusCode, http.StatusText((~res).StatusCode));
    if (rw.Body != nil){
        res.Value.Body = io.NopCloser(new bytes_ReaderжReader(bytes.NewReader(rw.Body.Bytes())));
    } else {
        res.Value.Body = http.NoBody;
    }
    res.Value.ContentLength = parseContentLength((~res).Header.Get(contentLengthˢ));
    {
        var (trailers, ok) = rw.snapHeader[trailerˢ, ꟷ]; if (ok) {
            res.Value.Trailer = new httpꓸHeader(len(trailers));
            foreach (var (_, k) in trailers) {
                foreach (var (_, vᴛ1) in strings.Split(k, ","u8)) {
                    var kΔ1 = vᴛ1;

                    kΔ1 = http.CanonicalHeaderKey(textproto.TrimString(kΔ1));
                    if (!httpguts.ValidTrailerHeader(kΔ1)) {
                        // Ignore since forbidden by RFC 7230, section 4.1.2.
                        continue;
                    }
                    var (vv, okΔ1) = rw.HeaderMap[kΔ1, ꟷ];
                    if (!okΔ1) {
                        continue;
                    }
                    var vv2 = new slice<@string>(len(vv));
                    copy(vv2, vv);
                    res.Value.Trailer[kΔ1] = vv2;
                }
            }
        }
    }
    foreach (var (k, vv) in rw.HeaderMap) {
        if (!strings.HasPrefix(k, http.TrailerPrefix)) {
            continue;
        }
        if ((~res).Trailer == default!) {
            res.Value.Trailer = new httpꓸHeader(0);
        }
        foreach (var (_, v) in vv) {
            (~res).Trailer.Add(strings.TrimPrefix(k, http.TrailerPrefix), v);
        }
    }
    return res;
}

// parseContentLength trims whitespace from s and returns -1 if no value
// is set, or the value if it's >= 0.
//
// This a modified version of same function found in net/http/transfer.go. This
// one just ignores an invalid header.
internal static int64 parseContentLength(@string cl) {
    cl = textproto.TrimString(cl);
    if (cl == ""u8) {
        return -1;
    }
    var (n, err) = strconv.ParseUint(cl, 10, 63);
    if (err != default!) {
        return -1;
    }
    return (int64)n;
}

} // end httptest_package
