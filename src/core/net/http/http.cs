// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate bundle -o=h2_bundle.go -prefix=http2 -tags=!nethttpomithttp2 golang.org/x/net/http2
namespace go.net;

using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf8 = go.unicode.utf8_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using go.unicode;
using vendor.golang.org.x.net.http;

partial class http_package {

// Protocols is a set of HTTP protocols.
// The zero value is an empty set of protocols.
//
// The supported protocols are:
//
//   - HTTP1 is the HTTP/1.0 and HTTP/1.1 protocols.
//     HTTP1 is supported on both unsecured TCP and secured TLS connections.
//
//   - HTTP2 is the HTTP/2 protcol over a TLS connection.
//
//   - UnencryptedHTTP2 is the HTTP/2 protocol over an unsecured TCP connection.
[GoType] partial struct Protocols {
    internal uint8 bits;
}

internal static UntypedInt protoHTTP1 => /* 1 << iota */ 1;
internal static UntypedInt protoHTTP2 => 2;
internal static UntypedInt protoUnencryptedHTTP2 => 4;

// HTTP1 reports whether p includes HTTP/1.
public static bool HTTP1(this Protocols p) {
    return (uint8)(p.bits & (uint8)protoHTTP1) != 0;
}

// SetHTTP1 adds or removes HTTP/1 from p.
[GoRecv] public static void SetHTTP1(this ref Protocols p, bool ok) {
    p.setBit(protoHTTP1, ok);
}

// HTTP2 reports whether p includes HTTP/2.
public static bool HTTP2(this Protocols p) {
    return (uint8)(p.bits & (uint8)protoHTTP2) != 0;
}

// SetHTTP2 adds or removes HTTP/2 from p.
[GoRecv] public static void SetHTTP2(this ref Protocols p, bool ok) {
    p.setBit(protoHTTP2, ok);
}

// UnencryptedHTTP2 reports whether p includes unencrypted HTTP/2.
public static bool UnencryptedHTTP2(this Protocols p) {
    return (uint8)(p.bits & (uint8)protoUnencryptedHTTP2) != 0;
}

// SetUnencryptedHTTP2 adds or removes unencrypted HTTP/2 from p.
[GoRecv] public static void SetUnencryptedHTTP2(this ref Protocols p, bool ok) {
    p.setBit(protoUnencryptedHTTP2, ok);
}

[GoRecv] internal static void setBit(this ref Protocols p, uint8 bit, bool ok) {
    if (ok){
        p.bits |= (uint8)(bit);
    } else {
        p.bits &= unchecked((uint8)~(uint8)(bit));
    }
}

public static @string String(this Protocols p) {
    slice<@string> s = default!;
    if (p.HTTP1()) {
        s = append(s, "HTTP1"u8);
    }
    if (p.HTTP2()) {
        s = append(s, "HTTP2"u8);
    }
    if (p.UnencryptedHTTP2()) {
        s = append(s, "UnencryptedHTTP2"u8);
    }
    return "{"u8 + strings.Join(s, ","u8) + "}"u8;
}

[GoType("[0]Action")] partial struct incomparable;

// maxInt64 is the effective "infinite" value for the Server and
// Transport's byte-limiting readers.
internal static UntypedInt maxInt64 => /* 1<<63 - 1 */ 9223372036854775807;

// aLongTimeAgo is a non-zero time, far in the past, used for
// immediate cancellation of network operations.
internal static time.Time aLongTimeAgo = time.Unix(1, 0);

// omitBundledHTTP2 is set by omithttp2.go when the nethttpomithttp2
// build tag is set. That means h2_bundle.go isn't compiled in and we
// shouldn't try to use it.
internal static bool omitBundledHTTP2;

// TODO(bradfitz): move common stuff here. The other files have accumulated
// generic http stuff in random places.

// contextKey is a value for use with context.WithValue. It's used as
// a pointer so it fits in an interface{} without allocation.
[GoType] public partial struct contextKey {
    internal @string name;
}

[GoRecv] public static @string String(this ref contextKey k) {
    return "net/http context value "u8 + k.name;
}

// Given a string of the form "host", "host:port", or "[ipv6::address]:port",
// return true if the string includes a port.
internal static bool hasPort(@string s) {
    return strings.LastIndex(s, ":"u8) > strings.LastIndex(s, "]"u8);
}

// removeEmptyPort strips the empty port in ":port" to ""
// as mandated by RFC 3986 Section 6.2.3.
internal static @string removeEmptyPort(@string host) {
    if (hasPort(host)) {
        return strings.TrimSuffix(host, ":"u8);
    }
    return host;
}

internal static bool isNotToken(rune r) {
    return !httpguts.IsTokenRune(r);
}

// stringContainsCTLByte reports whether s contains any ASCII control character.
internal static bool stringContainsCTLByte(@string s) {
    for (nint i = 0; i < builtin.len(s); i++) {
        var b = s[i];
        if (b < (rune)' ' || b == 0x7f) {
            return true;
        }
    }
    return false;
}

internal static @string hexEscapeNonASCII(@string s) {
    nint newLen = 0;
    for (nint i = 0; i < builtin.len(s); i++) {
        if (s[i] >= utf8.RuneSelf){
            newLen += 3;
        } else {
            newLen++;
        }
    }
    if (newLen == builtin.len(s)) {
        return s;
    }
    var b = new slice<byte>(0, newLen);
    nint pos = default!;
    for (nint i = 0; i < builtin.len(s); i++) {
        if (s[i] >= utf8.RuneSelf) {
            if (pos < i) {
                b = append(b, s.slice(pos, i).ꓸꓸꓸ);
            }
            b = append(b, (byte)((rune)'%'));
            b = strconv.AppendInt(b, (int64)s[i], 16);
            pos = i + 1;
        }
    }
    if (pos < builtin.len(s)) {
        b = append(b, s.slice(pos).ꓸꓸꓸ);
    }
    return ((@string)b);
}

// NoBody is an [io.ReadCloser] with no bytes. Read always returns EOF
// and Close always returns nil. It can be used in an outgoing client
// request to explicitly signal that a request has zero bytes.
// An alternative, however, is to simply set [Request.Body] to nil.
public static noBody NoBody = new noBody(nil);

[GoType] public partial struct noBody {
}

public static (nint, error) Read(this noBody _Δp0, slice<byte> _Δp1) {
    return (0, io.EOF);
}

public static error Close(this noBody _) {
    return default!;
}

public static (int64, error) WriteTo(this noBody _Δp0, io.Writer _Δp1) {
    return (0, default!);
}

internal static io.WriterTo _ᴛ7ʗ = NoBody;
internal static io.ReadCloser _ᴛ8ʗ = NoBody;

// PushOptions describes options for [Pusher.Push].
[GoType] partial struct PushOptions {
    // Method specifies the HTTP method for the promised request.
    // If set, it must be "GET" or "HEAD". Empty means "GET".
    public @string Method;
    // Header specifies additional promised request headers. This cannot
    // include HTTP/2 pseudo header fields like ":path" and ":scheme",
    // which will be added automatically.
    public ΔHeader Header;
}

// Pusher is the interface implemented by ResponseWriters that support
// HTTP/2 server push. For more background, see
// https://tools.ietf.org/html/rfc7540#section-8.2.
[GoType] partial interface Pusher {
    // Push initiates an HTTP/2 server push. This constructs a synthetic
    // request using the given target and options, serializes that request
    // into a PUSH_PROMISE frame, then dispatches that request using the
    // server's request handler. If opts is nil, default options are used.
    //
    // The target must either be an absolute path (like "/path") or an absolute
    // URL that contains a valid host and the same scheme as the parent request.
    // If the target is a path, it will inherit the scheme and host of the
    // parent request.
    //
    // The HTTP/2 spec disallows recursive pushes and cross-authority pushes.
    // Push may or may not detect these invalid pushes; however, invalid
    // pushes will be detected and canceled by conforming clients.
    //
    // Handlers that wish to push URL X should call Push before sending any
    // data that may trigger a request for URL X. This avoids a race where the
    // client issues requests for X before receiving the PUSH_PROMISE for X.
    //
    // Push will run in a separate goroutine making the order of arrival
    // non-deterministic. Any required synchronization needs to be implemented
    // by the caller.
    //
    // Push returns ErrNotSupported if the client has disabled push or if push
    // is not supported on the underlying connection.
    error Push(@string target, ж<PushOptions> opts);
}

// HTTP2Config defines HTTP/2 configuration parameters common to
// both [Transport] and [Server].
[GoType] partial struct HTTP2Config {
    // MaxConcurrentStreams optionally specifies the number of
    // concurrent streams that a peer may have open at a time.
    // If zero, MaxConcurrentStreams defaults to at least 100.
    public nint MaxConcurrentStreams;
    // MaxDecoderHeaderTableSize optionally specifies an upper limit for the
    // size of the header compression table used for decoding headers sent
    // by the peer.
    // A valid value is less than 4MiB.
    // If zero or invalid, a default value is used.
    public nint MaxDecoderHeaderTableSize;
    // MaxEncoderHeaderTableSize optionally specifies an upper limit for the
    // header compression table used for sending headers to the peer.
    // A valid value is less than 4MiB.
    // If zero or invalid, a default value is used.
    public nint MaxEncoderHeaderTableSize;
    // MaxReadFrameSize optionally specifies the largest frame
    // this endpoint is willing to read.
    // A valid value is between 16KiB and 16MiB, inclusive.
    // If zero or invalid, a default value is used.
    public nint MaxReadFrameSize;
    // MaxReceiveBufferPerConnection is the maximum size of the
    // flow control window for data received on a connection.
    // A valid value is at least 64KiB and less than 4MiB.
    // If invalid, a default value is used.
    public nint MaxReceiveBufferPerConnection;
    // MaxReceiveBufferPerStream is the maximum size of
    // the flow control window for data received on a stream (request).
    // A valid value is less than 4MiB.
    // If zero or invalid, a default value is used.
    public nint MaxReceiveBufferPerStream;
    // SendPingTimeout is the timeout after which a health check using a ping
    // frame will be carried out if no frame is received on a connection.
    // If zero, no health check is performed.
    public time.Duration SendPingTimeout;
    // PingTimeout is the timeout after which a connection will be closed
    // if a response to a ping is not received.
    // If zero, a default of 15 seconds is used.
    public time.Duration PingTimeout;
    // WriteByteTimeout is the timeout after which a connection will be
    // closed if no data can be written to it. The timeout begins when data is
    // available to write, and is extended whenever any bytes are written.
    public time.Duration WriteByteTimeout;
    // PermitProhibitedCipherSuites, if true, permits the use of
    // cipher suites prohibited by the HTTP/2 spec.
    public bool PermitProhibitedCipherSuites;
    // CountError, if non-nil, is called on HTTP/2 errors.
    // It is intended to increment a metric for monitoring.
    // The errType contains only lowercase letters, digits, and underscores
    // (a-z, 0-9, _).
    public Action<@string> CountError;
}

} // end http_package
