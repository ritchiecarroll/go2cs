// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tls13 implements the TLS 1.3 Key Schedule as specified in RFC 8446,
// Section 7.1 and allowed by FIPS 140-3 IG 2.4.B Resolution 7.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using hkdf = go.crypto.@internal.fips140.hkdf_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;

partial class tls13_package {

// We don't set the service indicator in this package but we delegate that to
// the underlying functions because the TLS 1.3 KDF does not have a standard of
// its own.

// ExpandLabel implements HKDF-Expand-Label from RFC 8446, Section 7.1.
public static slice<byte> ExpandLabel<H>(Func<H> hash, slice<byte> secret, @string label, slice<byte> context, nint length)
    where H : fips140.Hash
{
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
    hkdfLabel = append(hkdfLabel, (byte)(len("tls13 ") + len(label)));
    hkdfLabel = append(hkdfLabel, ((@string)"tls13 "u8).ꓸꓸꓸ);
    hkdfLabel = append(hkdfLabel, label.ꓸꓸꓸ);
    hkdfLabel = append(hkdfLabel, (byte)len(context));
    hkdfLabel = appendꓸꓸꓸ(hkdfLabel, context);
    return hkdf.Expand(hash, secret, ((@string)hkdfLabel), length);
}

internal static slice<byte> extract<H>(Func<H> hash, slice<byte> newSecret, slice<byte> currentSecret)
    where H : fips140.Hash
{
    if (newSecret == default!) {
        newSecret = new slice<byte>(hash().Size());
    }
    return hkdf.Extract(hash, newSecret, currentSecret);
}

internal static slice<byte> deriveSecret<H>(Func<H> hash, slice<byte> secret, @string label, fips140.Hash transcript)
    where H : fips140.Hash
{
    if (transcript == default!) {
        transcript = hash();
    }
    return ExpandLabel(hash, secret, label, transcript.Sum(default!), transcript.Size());
}

internal static readonly @string resumptionBinderLabel = "res binder"u8;
internal static readonly @string clientEarlyTrafficLabel = "c e traffic"u8;
internal static readonly @string clientHandshakeTrafficLabel = "c hs traffic"u8;
internal static readonly @string serverHandshakeTrafficLabel = "s hs traffic"u8;
internal static readonly @string clientApplicationTrafficLabel = "c ap traffic"u8;
internal static readonly @string serverApplicationTrafficLabel = "s ap traffic"u8;
internal static readonly @string earlyExporterLabel = "e exp master"u8;
internal static readonly @string exporterLabel = "exp master"u8;
internal static readonly @string resumptionLabel = "res master"u8;

partial struct EarlySecret {
    internal slice<byte> secret;
    internal Func<fips140.Hash> hash;
}

public static ж<EarlySecret> NewEarlySecret<H>(Func<H> hash, slice<byte> psk)
    where H : fips140.Hash
{
    return Ꮡ(new EarlySecret(
        secret: extract(hash, psk, default!),
        hash: () => hash()
    ));
}

public static slice<byte> ResumptionBinderKey(this ref EarlySecret s) {
    return deriveSecret(s.hash, s.secret, resumptionBinderLabel, default!);
}

// ClientEarlyTrafficSecret derives the client_early_traffic_secret from the
// early secret and the transcript up to the ClientHello.
public static slice<byte> ClientEarlyTrafficSecret(this ref EarlySecret s, fips140.Hash transcript) {
    return deriveSecret(s.hash, s.secret, clientEarlyTrafficLabel, transcript);
}

partial struct ΔHandshakeSecret {
    internal slice<byte> secret;
    internal Func<fips140.Hash> hash;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string derivedˢ = "derived"u8;

public static ж<ΔHandshakeSecret> HandshakeSecret(this ref EarlySecret s, slice<byte> sharedSecret) {
    var derived = deriveSecret(s.hash, s.secret, derivedˢ, default!);
    return Ꮡ(new ΔHandshakeSecret(
        secret: extract(s.hash, sharedSecret, derived),
        hash: s.hash
    ));
}

// ClientHandshakeTrafficSecret derives the client_handshake_traffic_secret from
// the handshake secret and the transcript up to the ServerHello.
public static slice<byte> ClientHandshakeTrafficSecret(this ref ΔHandshakeSecret s, fips140.Hash transcript) {
    return deriveSecret(s.hash, s.secret, clientHandshakeTrafficLabel, transcript);
}

// ServerHandshakeTrafficSecret derives the server_handshake_traffic_secret from
// the handshake secret and the transcript up to the ServerHello.
public static slice<byte> ServerHandshakeTrafficSecret(this ref ΔHandshakeSecret s, fips140.Hash transcript) {
    return deriveSecret(s.hash, s.secret, serverHandshakeTrafficLabel, transcript);
}

partial struct ΔMasterSecret {
    internal slice<byte> secret;
    internal Func<fips140.Hash> hash;
}

public static ж<ΔMasterSecret> MasterSecret(this ref ΔHandshakeSecret s) {
    var derived = deriveSecret(s.hash, s.secret, derivedˢ, default!);
    return Ꮡ(new ΔMasterSecret(
        secret: extract(s.hash, default!, derived),
        hash: s.hash
    ));
}

// ClientApplicationTrafficSecret derives the client_application_traffic_secret_0
// from the master secret and the transcript up to the server Finished.
public static slice<byte> ClientApplicationTrafficSecret(this ref ΔMasterSecret s, fips140.Hash transcript) {
    return deriveSecret(s.hash, s.secret, clientApplicationTrafficLabel, transcript);
}

// ServerApplicationTrafficSecret derives the server_application_traffic_secret_0
// from the master secret and the transcript up to the server Finished.
public static slice<byte> ServerApplicationTrafficSecret(this ref ΔMasterSecret s, fips140.Hash transcript) {
    return deriveSecret(s.hash, s.secret, serverApplicationTrafficLabel, transcript);
}

// ResumptionMasterSecret derives the resumption_master_secret from the master secret
// and the transcript up to the client Finished.
public static slice<byte> ResumptionMasterSecret(this ref ΔMasterSecret s, fips140.Hash transcript) {
    return deriveSecret(s.hash, s.secret, resumptionLabel, transcript);
}

partial struct ΔExporterMasterSecret {
    internal slice<byte> secret;
    internal Func<fips140.Hash> hash;
}

// ExporterMasterSecret derives the exporter_master_secret from the master secret
// and the transcript up to the server Finished.
public static ж<ΔExporterMasterSecret> ExporterMasterSecret(this ref ΔMasterSecret s, fips140.Hash transcript) {
    return Ꮡ(new ΔExporterMasterSecret(
        secret: deriveSecret(s.hash, s.secret, exporterLabel, transcript),
        hash: s.hash
    ));
}

// EarlyExporterMasterSecret derives the exporter_master_secret from the early secret
// and the transcript up to the ClientHello.
public static ж<ΔExporterMasterSecret> EarlyExporterMasterSecret(this ref EarlySecret s, fips140.Hash transcript) {
    return Ꮡ(new ΔExporterMasterSecret(
        secret: deriveSecret(s.hash, s.secret, earlyExporterLabel, transcript),
        hash: s.hash
    ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string exporterˢ = "exporter"u8;

public static slice<byte> Exporter(this ref ΔExporterMasterSecret s, @string label, slice<byte> context, nint length) {
    var secret = deriveSecret(s.hash, s.secret, label, default!);
    var h = s.hash();
    h.Write(context);
    return ExpandLabel(s.hash, secret, exporterˢ, h.Sum(default!), length);
}

public static slice<byte> TestingOnlyExporterSecret(ж<ΔExporterMasterSecret> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.secret;
}

} // end tls13_package
