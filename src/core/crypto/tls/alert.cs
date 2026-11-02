// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using strconv = strconv_package;

partial class tls_package {

// An AlertError is a TLS alert.
//
// When using a QUIC transport, QUICConn methods will return an error
// which wraps AlertError rather than sending a TLS alert.
partial struct AlertError /*num:uint8*/;

public static @string Error(this AlertError e) {
    return ((alert)(uint8)e).String();
}

partial struct alert /*num:uint8*/;

// alert level
internal static UntypedInt alertLevelWarning => 1;
internal static UntypedInt alertLevelError => 2;

internal static alert alertCloseNotify => 0;
internal static alert alertUnexpectedMessage => 10;
internal static alert alertBadRecordMAC => 20;
internal static alert alertDecryptionFailed => 21;
internal static alert alertRecordOverflow => 22;
internal static alert alertDecompressionFailure => 30;
internal static alert alertHandshakeFailure => 40;
internal static alert alertBadCertificate => 42;
internal static alert alertUnsupportedCertificate => 43;
internal static alert alertCertificateRevoked => 44;
internal static alert alertCertificateExpired => 45;
internal static alert alertCertificateUnknown => 46;
internal static alert alertIllegalParameter => 47;
internal static alert alertUnknownCA => 48;
internal static alert alertAccessDenied => 49;
internal static alert alertDecodeError => 50;
internal static alert alertDecryptError => 51;
internal static alert alertExportRestriction => 60;
internal static alert alertProtocolVersion => 70;
internal static alert alertInsufficientSecurity => 71;
internal static alert alertInternalError => 80;
internal static alert alertInappropriateFallback => 86;
internal static alert alertUserCanceled => 90;
internal static alert alertNoRenegotiation => 100;
internal static alert alertMissingExtension => 109;
internal static alert alertUnsupportedExtension => 110;
internal static alert alertCertificateUnobtainable => 111;
internal static alert alertUnrecognizedName => 112;
internal static alert alertBadCertificateStatusResponse => 113;
internal static alert alertBadCertificateHashValue => 114;
internal static alert alertUnknownPSKIdentity => 115;
internal static alert alertCertificateRequired => 116;
internal static alert alertNoApplicationProtocol => 120;
internal static alert alertECHRequired => 121;

[GoLazy] partial class lazyᴛalertText { internal static map<alert, @string> alertText = new map<alert, @string>{
    [alertCloseNotify] = "close notify",
    [alertUnexpectedMessage] = "unexpected message",
    [alertBadRecordMAC] = "bad record MAC",
    [alertDecryptionFailed] = "decryption failed",
    [alertRecordOverflow] = "record overflow",
    [alertDecompressionFailure] = "decompression failure",
    [alertHandshakeFailure] = "handshake failure",
    [alertBadCertificate] = "bad certificate",
    [alertUnsupportedCertificate] = "unsupported certificate",
    [alertCertificateRevoked] = "revoked certificate",
    [alertCertificateExpired] = "expired certificate",
    [alertCertificateUnknown] = "unknown certificate",
    [alertIllegalParameter] = "illegal parameter",
    [alertUnknownCA] = "unknown certificate authority",
    [alertAccessDenied] = "access denied",
    [alertDecodeError] = "error decoding message",
    [alertDecryptError] = "error decrypting message",
    [alertExportRestriction] = "export restriction",
    [alertProtocolVersion] = "protocol version not supported",
    [alertInsufficientSecurity] = "insufficient security level",
    [alertInternalError] = "internal error",
    [alertInappropriateFallback] = "inappropriate fallback",
    [alertUserCanceled] = "user canceled",
    [alertNoRenegotiation] = "no renegotiation",
    [alertMissingExtension] = "missing extension",
    [alertUnsupportedExtension] = "unsupported extension",
    [alertCertificateUnobtainable] = "certificate unobtainable",
    [alertUnrecognizedName] = "unrecognized name",
    [alertBadCertificateStatusResponse] = "bad certificate status response",
    [alertBadCertificateHashValue] = "bad certificate hash value",
    [alertUnknownPSKIdentity] = "unknown PSK identity",
    [alertCertificateRequired] = "certificate required",
    [alertNoApplicationProtocol] = "no application protocol",
    [alertECHRequired] = "encrypted client hello required"
}; }

internal static @string String(this alert e) {
    var (s, ok) = alertText[e, ꟷ];
    if (ok) {
        return "tls: "u8 + s;
    }
    return "tls: alert("u8 + strconv.Itoa((nint)e) + ")"u8;
}

internal static @string Error(this alert e) {
    return e.String();
}

} // end tls_package
