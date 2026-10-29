// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httptest provides utilities for HTTP testing.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using io = io_package;
using http = go.net.http_package;
using strings = strings_package;
using crypto;
using go.net;

partial class httptest_package {

// NewRequest wraps NewRequestWithContext using context.Background.
public static ж<http.Request> NewRequest(@string method, @string target, io.Reader body) {
    return NewRequestWithContext(context.Background(), method, target, body);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ = "GET"u8;
internal static readonly @string http11ˢ = "HTTP/1.1"u8;
internal static readonly @string exampleComˢ = "example.com"u8;
internal static readonly @string httpsˢ = "https://"u8;

// NewRequestWithContext returns a new incoming server Request, suitable
// for passing to an [http.Handler] for testing.
//
// The target is the RFC 7230 "request-target": it may be either a
// path or an absolute URL. If target is an absolute URL, the host name
// from the URL is used. Otherwise, "example.com" is used.
//
// The TLS field is set to a non-nil dummy value if target has scheme
// "https".
//
// The Request.Proto is always HTTP/1.1.
//
// An empty method means "GET".
//
// The provided body may be nil. If the body is of type [bytes.Reader],
// [strings.Reader], [bytes.Buffer], or the value [http.NoBody],
// the Request.ContentLength is set.
//
// NewRequest panics on error for ease of use in testing, where a
// panic is acceptable.
//
// To generate a client HTTP request instead of a server request, see
// the NewRequest function in the net/http package.
public static ж<http.Request> NewRequestWithContext(context.Context ctx, @string method, @string target, io.Reader body) {
    if (method == ""u8) {
        method = getˢ;
    }
    var (req, err) = http.ReadRequest(bufio.NewReader(new strings_ReaderжReader(strings.NewReader(method + " "u8 + target + " HTTP/1.0\r\n\r\n"u8))));
    if (err != nil) {
        throw panic("invalid NewRequest arguments; " + err.Error());
    }
    req = req.WithContext(ctx);

    // HTTP/1.0 was used above to avoid needing a Host field. Change it to 1.1 here.
    req.Proto = http11ˢ;
    req.ProtoMinor = 1;
    req.Close = false;

    if (body != nil) {
        switch (body.type()) {
        case ж<bytes.Buffer> v: {
            req.ContentLength = (int64)v.Len();
            break;
        }
        case ж<bytes.Reader> v: {
            req.ContentLength = (int64)v.Len();
            break;
        }
        case ж<strings.Reader> v: {
            req.ContentLength = (int64)v.Len();
            break;
        }
        default: {
            var v = body;
            req.ContentLength = -1;
            break;
        }}
        if (AreEqual(body, http.NoBody)) {
            req.ContentLength = 0;
        }
        {
            var (rc, ok) = body._<io.ReadCloser>(ᐧ); if (ok) {
                req.Body = rc;
            } else {
                req.Body = io.NopCloser(body);
            }
        }
    }

    // 192.0.2.0/24 is "TEST-NET" in RFC 5737 for use solely in
    // documentation and example source code and should not be
    // used publicly.
    req.RemoteAddr = "192.0.2.1:1234"u8;

    if (req.Host == ""u8) {
        req.Host = exampleComˢ;
    }

    if (strings.HasPrefix(target, httpsˢ)) {
        req.TLS = Ꮡ(new tlsꓸConnectionState(
            Version: tls.VersionTLS12,
            HandshakeComplete: true,
            ServerName: req.Host
        ));
    }

    return req;
}

} // end httptest_package
