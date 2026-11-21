// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using hmac = crypto.hmac_package;
using md5 = crypto.md5_package;
using errors = errors_package;
using fmt = fmt_package;
using crypto;
using hash = hash_package;
using static smtp_package;

partial class smtp_package {

// Auth is implemented by an SMTP authentication mechanism.
partial interface Auth {
    // Start begins an authentication with a server.
    // It returns the name of the authentication protocol
    // and optionally data to include in the initial AUTH message
    // sent to the server.
    // If it returns a non-nil error, the SMTP client aborts
    // the authentication attempt and closes the connection.
    (@string proto, slice<byte> toServer, error err) Start(ж<ServerInfo> server);
    // Next continues the authentication. The server has just sent
    // the fromServer data. If more is true, the server expects a
    // response, which Next should return as toServer; otherwise
    // Next should return toServer == nil.
    // If Next returns a non-nil error, the SMTP client aborts
    // the authentication attempt and closes the connection.
    (slice<byte> toServer, error err) Next(slice<byte> fromServer, bool more);
}

// ServerInfo records information about an SMTP server.
partial struct ServerInfo {
    public @string Name;        // SMTP server name
    public bool TLS;            // using TLS, with valid certificate for Name
    public slice<@string> Auth; // advertised authentication mechanisms
}

partial struct plainAuth {
    internal @string identity, username, password;
    internal @string host;
}

// PlainAuth returns an [Auth] that implements the PLAIN authentication
// mechanism as defined in RFC 4616. The returned Auth uses the given
// username and password to authenticate to host and act as identity.
// Usually identity should be the empty string, to act as username.
//
// PlainAuth will only send the credentials if the connection is using TLS
// or is connected to localhost. Otherwise authentication will fail with an
// error, without sending the credentials.
public static Auth PlainAuth(@string identity, @string username, @string password, @string host) {
    return new plainAuthжAuth(Ꮡ(new plainAuth(identity, username, password, host)));
}

internal static bool isLocalhost(@string name) {
    return name == "localhost"u8 || name == "127.0.0.1"u8 || name == "::1"u8;
}

} partial class smtp_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unencryptedConnectionˢᴍplainAuth => ref unencryptedConnectionˢᵇᴍplainAuth;
private static readonly @string unencryptedConnectionˢᵇᴍplainAuth = "unencrypted connection"u8;
internal static ref readonly @string wrongHostNameˢᴍplainAuth => ref wrongHostNameˢᵇᴍplainAuth;
private static readonly @string wrongHostNameˢᵇᴍplainAuth = "wrong host name"u8;
internal static ref readonly @string plainˢᴍplainAuth => ref plainˢᵇᴍplainAuth;
private static readonly @string plainˢᵇᴍplainAuth = "PLAIN"u8;

[GoMethodBody("Start")] internal static (@string, slice<byte>, error) Startᴍ(ref plainAuth a, ж<ServerInfo> Ꮡserver) {
    ref var server = ref Ꮡserver.DerefOrNull();

    // Must have TLS, or else localhost server.
    // Note: If TLS is not true, then we can't trust ANYTHING in ServerInfo.
    // In particular, it doesn't matter if the server advertises PLAIN auth.
    // That might just be the attacker saying
    // "it's ok, you can trust me with your password."
    if (!server.TLS && !isLocalhost(server.Name)) {
        return ("", default, errors.New(unencryptedConnectionˢᴍplainAuth));
    }
    if (server.Name != a.host) {
        return ("", default, errors.New(wrongHostNameˢᴍplainAuth));
    }
    var resp = slice<byte>(a.identity + "\x00" + a.username + "\x00" + a.password);
    return (plainˢᴍplainAuth, resp, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedServerˢᴍplainAuth => ref unexpectedServerˢᵇᴍplainAuth;
private static readonly @string unexpectedServerˢᵇᴍplainAuth = "unexpected server challenge"u8;

[GoMethodBody("Next")] internal static (slice<byte>, error) Nextᴍ(ref plainAuth a, slice<byte> fromServer, bool more) {
    if (more) {
        // We've already sent everything.
        return (default, errors.New(unexpectedServerˢᴍplainAuth));
    }
    return (default, default);
}

} partial class smtp_package {

partial struct cramMD5Auth {
    internal @string username, secret;
}

// CRAMMD5Auth returns an [Auth] that implements the CRAM-MD5 authentication
// mechanism as defined in RFC 2195.
// The returned Auth uses the given username and secret to authenticate
// to the server using the challenge-response mechanism.
public static Auth CRAMMD5Auth(@string username, @string secret) {
    return new cramMD5AuthжAuth(Ꮡ(new cramMD5Auth(username, secret)));
}

} partial class smtp_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cramMd5ˢᴍcramMD5Auth => ref cramMd5ˢᵇᴍcramMD5Auth;
private static readonly @string cramMd5ˢᵇᴍcramMD5Auth = "CRAM-MD5"u8;

[GoMethodBody("Start")] internal static (@string, slice<byte>, error) Startᴍ(ref cramMD5Auth a, ж<ServerInfo> server) {
    return (cramMd5ˢᴍcramMD5Auth, default, default);
}

[GoMethodBody("Next")] internal static (slice<byte>, error) Nextᴍ(ref cramMD5Auth a, slice<byte> fromServer, bool more) {
    if (more) {
        var d = hmac.New(md5.New, slice<byte>(a.secret));
        d.Write(fromServer);
        var s = new slice<byte>(0, d.Size());
        return (fmt.Appendf(default, "%s %x"u8, a.username, d.Sum(s)), default);
    }
    return (default, default);
}

}
