// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using godebug = go.@internal.godebug_package;
using slices = slices_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using go.@internal;

partial class tls_package {

// Defaults are collected in this file to allow distributions to more easily patch
// them to apply local policies.
internal static ж<godebug.Setting> tlsmlkem = godebug.New("tlsmlkem"u8);

internal static ж<godebug.Setting> tlssecpmlkem = godebug.New("tlssecpmlkem"u8);

// defaultCurvePreferences is the default set of supported key exchanges, as
// well as the preference order.
internal static slice<CurveID> defaultCurvePreferences() {
    switch (ᐧ) {
    // tlsmlkem=0 restores the pre-Go 1.24 default.
    case {} when tlsmlkem.Value() == "0"u8: {
        return new CurveID[]{X25519, CurveP256, CurveP384, CurveP521}.slice();
    }
    // tlssecpmlkem=0 restores the pre-Go 1.26 default.
    case {} when tlssecpmlkem.Value() == "0"u8: {
        return new CurveID[]{X25519MLKEM768, X25519, CurveP256, CurveP384, CurveP521}.slice();
    }
    default: {
        return new CurveID[]{
            X25519MLKEM768, SecP256r1MLKEM768, SecP384r1MLKEM1024,
            X25519, CurveP256, CurveP384, CurveP521
        }.slice();
    }}
}

// defaultSupportedSignatureAlgorithms returns the signature and hash algorithms that
// the code advertises and supports in a TLS 1.2+ ClientHello and in a TLS 1.2+
// CertificateRequest. The two fields are merged to match with TLS 1.3.
// Note that in TLS 1.2, the ECDSA algorithms are not constrained to P-256, etc.
internal static slice<SignatureScheme> defaultSupportedSignatureAlgorithms() {
    return new SignatureScheme[]{
        PSSWithSHA256,
        ECDSAWithP256AndSHA256,
        Ed25519,
        PSSWithSHA384,
        PSSWithSHA512,
        PKCS1WithSHA256,
        PKCS1WithSHA384,
        PKCS1WithSHA512,
        ECDSAWithP384AndSHA384,
        ECDSAWithP521AndSHA512,
        PKCS1WithSHA1,
        ECDSAWithSHA1
    }.slice();
}

internal static ж<godebug.Setting> tlsrsakex = godebug.New("tlsrsakex"u8);

internal static ж<godebug.Setting> tls3des = godebug.New("tls3des"u8);

internal static slice<uint16> supportedCipherSuites(bool aesGCMPreferred) {
    if (aesGCMPreferred){
        return slices.Clone<slice<uint16>, uint16>(cipherSuitesPreferenceOrder);
    } else {
        return slices.Clone<slice<uint16>, uint16>(cipherSuitesPreferenceOrderNoAES);
    }
}

internal static slice<uint16> defaultCipherSuites(bool aesGCMPreferred) {
    var ΔcipherSuites = supportedCipherSuites(aesGCMPreferred);
    return slices.DeleteFunc(ΔcipherSuites, (uint16 c) => disabledCipherSuites[c] || tlsrsakex.Value() != "1"u8 && rsaKexCiphers[c] || tls3des.Value() != "1"u8 && tdesCiphers[c]);
}

// defaultCipherSuitesTLS13 is also the preference order, since there are no
// disabled by default TLS 1.3 cipher suites. The same AES vs ChaCha20 logic as
// cipherSuitesPreferenceOrder applies.
//
// defaultCipherSuitesTLS13 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quic-go/quic-go
//   - github.com/sagernet/quic-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultCipherSuitesTLS13
public static slice<uint16> defaultCipherSuitesTLS13 = new uint16[]{
    TLS_AES_128_GCM_SHA256,
    TLS_AES_256_GCM_SHA384,
    TLS_CHACHA20_POLY1305_SHA256
}.slice();

// defaultCipherSuitesTLS13NoAES should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quic-go/quic-go
//   - github.com/sagernet/quic-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultCipherSuitesTLS13NoAES
public static slice<uint16> defaultCipherSuitesTLS13NoAES = new uint16[]{
    TLS_CHACHA20_POLY1305_SHA256,
    TLS_AES_128_GCM_SHA256,
    TLS_AES_256_GCM_SHA384
}.slice();

} // end tls_package
