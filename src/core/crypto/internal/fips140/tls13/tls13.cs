// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tls13 implements the TLS 1.3 Key Schedule as specified in RFC 8446,
// Section 7.1 and allowed by FIPS 140-3 IG 2.4.B Resolution 7.
namespace go.crypto.@internal.fips140;

using hkdf = go.crypto.@internal.fips140.hkdf_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using hash = hash_package;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;
using static tls13_package;

partial class tls13_package {

// We don't set the service indicator in this package but we delegate that to
// the underlying functions because the TLS 1.3 KDF does not have a standard of
// its own.

// ExpandLabel implements HKDF-Expand-Label from RFC 8446, Section 7.1.
public static slice<byte> ExpandLabel<H>(Func<H> hashΔ1, slice<byte> secret, @string label, slice<byte> context, nint length)
    where H : hash.Hash
{
    var hkdfLabelᴷ = new stackAppend<byte>();

    if (len("tls13 ") + len(label) > 255 || len(context) > 255) {
        // It should be impossible for this to panic: labels are fixed strings,
        // and context is either a fixed-length computed hash, or parsed from a
        // field which has the same length limitation.
        //
        // Another reasonable approach might be to return a randomized slice if
        // we encounter an error, which would break the connection, but avoid
        // panicking. This would perhaps be safer but significantly more
        // confusing to users.
        throw panic("tls13: label or context too long");
    }
    var hkdfLabel = new slice<byte>(0, 2 + 1 + len("tls13 ") + len(label) + 1 + len(context));
    hkdfLabel = byteorder.BEAppendUint16(hkdfLabel, (uint16)length);
    hkdfLabel = append(hkdfLabelᴷ, hkdfLabel, (byte)(len("tls13 ") + len(label)));
    hkdfLabel = append(hkdfLabel, ((@string)"tls13 ").ꓸꓸꓸ);
    hkdfLabel = append(hkdfLabel, label.ꓸꓸꓸ);
    hkdfLabel = append(hkdfLabel, (byte)len(context));
    hkdfLabel = appendꓸꓸꓸ(hkdfLabel, context);
    return hkdf.Expand(hashΔ1, secret, (@string)hkdfLabel, length);
}

internal static slice<byte> extract<H>(Func<H> hashΔ1, slice<byte> newSecret, slice<byte> currentSecret)
    where H : hash.Hash
{
    if (newSecret == nil) {
        newSecret = new slice<byte>(hashΔ1().Size());
    }
    return hkdf.Extract(hashΔ1, newSecret, currentSecret);
}

internal static slice<byte> deriveSecret<H>(Func<H> hashΔ1, slice<byte> secret, @string label, hash.Hash transcript)
    where H : hash.Hash
{
    if (transcript == nil) {
        transcript = hashΔ1();
    }
    return ExpandLabel(hashΔ1, secret, label, transcript.Sum(default), transcript.Size());
}

internal static ref readonly @string resumptionBinderLabel => ref resumptionBinderLabelᵇ;
private static readonly @string resumptionBinderLabelᵇ = "res binder"u8;
internal static ref readonly @string clientEarlyTrafficLabel => ref clientEarlyTrafficLabelᵇ;
private static readonly @string clientEarlyTrafficLabelᵇ = "c e traffic"u8;
internal static ref readonly @string clientHandshakeTrafficLabel => ref clientHandshakeTrafficLabelᵇ;
private static readonly @string clientHandshakeTrafficLabelᵇ = "c hs traffic"u8;
internal static ref readonly @string serverHandshakeTrafficLabel => ref serverHandshakeTrafficLabelᵇ;
private static readonly @string serverHandshakeTrafficLabelᵇ = "s hs traffic"u8;
internal static ref readonly @string clientApplicationTrafficLabel => ref clientApplicationTrafficLabelᵇ;
private static readonly @string clientApplicationTrafficLabelᵇ = "c ap traffic"u8;
internal static ref readonly @string serverApplicationTrafficLabel => ref serverApplicationTrafficLabelᵇ;
private static readonly @string serverApplicationTrafficLabelᵇ = "s ap traffic"u8;
internal static ref readonly @string earlyExporterLabel => ref earlyExporterLabelᵇ;
private static readonly @string earlyExporterLabelᵇ = "e exp master"u8;
internal static ref readonly @string exporterLabel => ref exporterLabelᵇ;
private static readonly @string exporterLabelᵇ = "exp master"u8;
internal static ref readonly @string resumptionLabel => ref resumptionLabelᵇ;
private static readonly @string resumptionLabelᵇ = "res master"u8;

partial struct EarlySecret {
    internal slice<byte> secret;
    internal Func<hash.Hash> hash;
}

public static ж<EarlySecret> NewEarlySecret<H>(Func<H> h, slice<byte> psk)
    where H : hash.Hash
{
    return Ꮡ(new EarlySecret(
        secret: extract(h, psk, default),
        hash: () => h()
    ));
}

} partial class tls13ᴛEarlySecret {

public static slice<byte> ResumptionBinderKey(this ref EarlySecret s) {
    return deriveSecret(s.hash, s.secret, resumptionBinderLabel, default);
}

// ClientEarlyTrafficSecret derives the client_early_traffic_secret from the
// early secret and the transcript up to the ClientHello.
public static slice<byte> ClientEarlyTrafficSecret(this ref EarlySecret s, hash.Hash transcript) {
    return deriveSecret(s.hash, s.secret, clientEarlyTrafficLabel, transcript);
}

} partial class tls13_package {

partial struct HandshakeSecret {
    internal slice<byte> secret;
    internal Func<hash.Hash> hash;
}

} partial class tls13ᴛEarlySecret {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string derivedˢ => ref derivedˢᵇ;
private static readonly @string derivedˢᵇ = "derived"u8;

public static ж<HandshakeSecret> HandshakeSecret(this ref EarlySecret s, slice<byte> sharedSecret) {
    var derived = deriveSecret(s.hash, s.secret, derivedˢ, default);
    return Ꮡ(new HandshakeSecret(
        secret: extract(s.hash, sharedSecret, derived),
        hash: s.hash
    ));
}

} partial class tls13ᴛHandshakeSecret {

// ClientHandshakeTrafficSecret derives the client_handshake_traffic_secret from
// the handshake secret and the transcript up to the ServerHello.
public static slice<byte> ClientHandshakeTrafficSecret(this ref HandshakeSecret s, hash.Hash transcript) {
    return deriveSecret(s.hash, s.secret, clientHandshakeTrafficLabel, transcript);
}

// ServerHandshakeTrafficSecret derives the server_handshake_traffic_secret from
// the handshake secret and the transcript up to the ServerHello.
public static slice<byte> ServerHandshakeTrafficSecret(this ref HandshakeSecret s, hash.Hash transcript) {
    return deriveSecret(s.hash, s.secret, serverHandshakeTrafficLabel, transcript);
}

} partial class tls13_package {

partial struct MasterSecret {
    internal slice<byte> secret;
    internal Func<hash.Hash> hash;
}

} partial class tls13ᴛHandshakeSecret {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string derivedˢ => ref derivedˢᵇ;
private static readonly @string derivedˢᵇ = "derived"u8;

public static ж<MasterSecret> MasterSecret(this ref HandshakeSecret s) {
    var derived = deriveSecret(s.hash, s.secret, derivedˢ, default);
    return Ꮡ(new MasterSecret(
        secret: extract(s.hash, default, derived),
        hash: s.hash
    ));
}

} partial class tls13ᴛMasterSecret {

// ClientApplicationTrafficSecret derives the client_application_traffic_secret_0
// from the master secret and the transcript up to the server Finished.
public static slice<byte> ClientApplicationTrafficSecret(this ref MasterSecret s, hash.Hash transcript) {
    return deriveSecret(s.hash, s.secret, clientApplicationTrafficLabel, transcript);
}

// ServerApplicationTrafficSecret derives the server_application_traffic_secret_0
// from the master secret and the transcript up to the server Finished.
public static slice<byte> ServerApplicationTrafficSecret(this ref MasterSecret s, hash.Hash transcript) {
    return deriveSecret(s.hash, s.secret, serverApplicationTrafficLabel, transcript);
}

// ResumptionMasterSecret derives the resumption_master_secret from the master secret
// and the transcript up to the client Finished.
public static slice<byte> ResumptionMasterSecret(this ref MasterSecret s, hash.Hash transcript) {
    return deriveSecret(s.hash, s.secret, resumptionLabel, transcript);
}

} partial class tls13_package {

partial struct ExporterMasterSecret {
    internal slice<byte> secret;
    internal Func<hash.Hash> hash;
}

} partial class tls13ᴛMasterSecret {

// ExporterMasterSecret derives the exporter_master_secret from the master secret
// and the transcript up to the server Finished.
public static ж<ExporterMasterSecret> ExporterMasterSecret(this ref MasterSecret s, hash.Hash transcript) {
    return Ꮡ(new ExporterMasterSecret(
        secret: deriveSecret(s.hash, s.secret, exporterLabel, transcript),
        hash: s.hash
    ));
}

} partial class tls13ᴛEarlySecret {

// EarlyExporterMasterSecret derives the exporter_master_secret from the early secret
// and the transcript up to the ClientHello.
public static ж<ExporterMasterSecret> EarlyExporterMasterSecret(this ref EarlySecret s, hash.Hash transcript) {
    return Ꮡ(new ExporterMasterSecret(
        secret: deriveSecret(s.hash, s.secret, earlyExporterLabel, transcript),
        hash: s.hash
    ));
}

} partial class tls13ᴛExporterMasterSecret {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string exporterˢ => ref exporterˢᵇ;
private static readonly @string exporterˢᵇ = "exporter"u8;

public static slice<byte> Exporter(this ref ExporterMasterSecret s, @string label, slice<byte> context, nint length) {
    var secret = deriveSecret(s.hash, s.secret, label, default);
    var h = s.hash();
    h.Write(context);
    return ExpandLabel(s.hash, secret, exporterˢ, h.Sum(default), length);
}

} partial class tls13_package {

public static slice<byte> TestingOnlyExporterSecret(ж<ExporterMasterSecret> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.secret;
}

} // end tls13_package
