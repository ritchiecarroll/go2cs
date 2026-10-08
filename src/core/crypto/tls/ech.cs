// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using hpke = go.crypto.@internal.hpke_package;
using errors = errors_package;
using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using ecdh = go.crypto.ecdh_package;
using go.crypto.@internal;
using vendor.golang.org.x.crypto;

partial class tls_package {

// sortedSupportedAEADs is just a sorted version of hpke.SupportedAEADS.
// We need this so that when we insert them into ECHConfigs the ordering
// is stable.
internal static slice<uint16> sortedSupportedAEADs;

[GoInit] internal static void init() {
    foreach (var (aeadID, _) in hpke.SupportedAEADs) {
        sortedSupportedAEADs = append(sortedSupportedAEADs, aeadID);
    }
    slices.Sort<slice<uint16>, uint16>(sortedSupportedAEADs);
}

public partial struct echCipher {
    public uint16 KDFID;
    public uint16 AEADID;
}

public partial struct echExtension {
    public uint16 Type;
    public slice<byte> Data;
}

partial struct echConfig {
    internal slice<byte> raw;
    public uint16 Version;
    public uint16 Length;
    public uint8 ConfigID;
    public uint16 KemID;
    public slice<byte> PublicKey;
    public slice<echCipher> SymmetricCipherSuite;
    public uint8 MaxNameLength;
    public slice<byte> PublicName;
    public slice<echExtension> Extensions;
}

internal static error errMalformedECHConfig = errors.New("tls: malformed ECHConfigList"u8);

internal static (bool skip, echConfig ec, error err) parseECHConfig(slice<byte> enc) {
    ref var ec = ref heap(new echConfig(), out var Ꮡec);

    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)enc);
    ec.raw = ((slice<byte>)enc);
    if (!s.ReadUint16(Ꮡec.of(echConfig.ᏑVersion))) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    if (!s.ReadUint16(Ꮡec.of(echConfig.ᏑLength))) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    if (len(ec.raw) < (nint)ec.Length + 4) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    ec.raw = ec.raw.slice(0, (uint16)(ec.Length + 4));
    if (ec.Version != extensionEncryptedClientHello) {
        s.Skip((nint)ec.Length);
        return (true, new echConfig(nil), default!);
    }
    if (!s.ReadUint8(Ꮡec.of(echConfig.ᏑConfigID))) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    if (!s.ReadUint16(Ꮡec.of(echConfig.ᏑKemID))) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    if (!readUint16LengthPrefixed(Ꮡs, Ꮡec.of(echConfig.ᏑPublicKey))) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    ref var ΔcipherSuites = ref heap<cryptobyte.String>(out var ᏑcipherSuites);
    if (!s.ReadUint16LengthPrefixed(ᏑcipherSuites)) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    while (!ΔcipherSuites.Empty()) {
        ref var c = ref heap(new echCipher(), out var Ꮡc);
        if (!ΔcipherSuites.ReadUint16(Ꮡc.of(echCipher.ᏑKDFID))) {
            return (false, new echConfig(nil), errMalformedECHConfig);
        }
        if (!ΔcipherSuites.ReadUint16(Ꮡc.of(echCipher.ᏑAEADID))) {
            return (false, new echConfig(nil), errMalformedECHConfig);
        }
        ec.SymmetricCipherSuite = append(ec.SymmetricCipherSuite, c);
    }
    if (!s.ReadUint8(Ꮡec.of(echConfig.ᏑMaxNameLength))) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    ref var publicName = ref heap<cryptobyte.String>(out var ᏑpublicName);
    if (!s.ReadUint8LengthPrefixed(ᏑpublicName)) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    ec.PublicName = publicName;
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.ReadUint16LengthPrefixed(Ꮡextensions)) {
        return (false, new echConfig(nil), errMalformedECHConfig);
    }
    while (!extensions.Empty()) {
        ref var e = ref heap(new echExtension(), out var Ꮡe);
        if (!extensions.ReadUint16(Ꮡe.of(echExtension.ᏑType))) {
            return (false, new echConfig(nil), errMalformedECHConfig);
        }
        if (!extensions.ReadUint16LengthPrefixed(Ꮡe.of(echExtension.ᏑData).Reinterpret<slice<byte>, cryptobyte.String>())) {
            return (false, new echConfig(nil), errMalformedECHConfig);
        }
        ec.Extensions = append(ec.Extensions, e);
    }
    return (false, ec, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsMalformedECHConfigˢ = "tls: malformed ECHConfig"u8;

// parseECHConfigList parses a draft-ietf-tls-esni-18 ECHConfigList, returning a
// slice of parsed ECHConfigs, in the same order they were parsed, or an error
// if the list is malformed.
internal static (slice<echConfig>, error) parseECHConfigList(slice<byte> data) {
    var s = ((cryptobyte.String)data);
    ref var length = ref heap(new uint16(), out var Ꮡlength);
    if (!s.ReadUint16(Ꮡlength)) {
        return (default!, errMalformedECHConfig);
    }
    if (length != (uint16)(len(data) - 2)) {
        return (default!, errMalformedECHConfig);
    }
    slice<echConfig> configs = default!;
    while (len(s) > 0) {
        if (len(s) < 4) {
            return (default!, errors.New(tlsMalformedECHConfigˢ));
        }
        var configLen = (uint16)((uint16)((uint16)s[2] << (int)(8)) | (uint16)s[3]);
        var (skip, ec, err) = parseECHConfig(s);
        if (err != default!) {
            return (default!, err);
        }
        s = s.slice((uint16)(configLen + 4));
        if (!skip) {
            configs = append(configs, ec);
        }
    }
    return (configs, default!);
}

internal static ж<echConfig> pickECHConfig(slice<echConfig> list) {
    foreach (var (_, vᴛ1) in list) {
        ref var ec = ref heap(new echConfig(), out var Ꮡec);
        ec = vᴛ1;

        {
            var (_, ok) = hpke.SupportedKEMs[ec.KemID, ꟷ]; if (!ok) {
                continue;
            }
        }
        bool validSCS = default!;
        foreach (var (_, cs) in ec.SymmetricCipherSuite) {
            {
                var (_, ok) = hpke.SupportedAEADs[cs.AEADID, ꟷ]; if (!ok) {
                    continue;
                }
            }
            {
                var (_, ok) = hpke.SupportedKDFs[cs.KDFID, ꟷ]; if (!ok) {
                    continue;
                }
            }
            validSCS = true;
            break;
        }
        if (!validSCS) {
            continue;
        }
        if (!validDNSName(((@string)ec.PublicName))) {
            continue;
        }
        bool unsupportedExt = default!;
        foreach (var (_, ext) in ec.Extensions) {
            // If high order bit is set to 1 the extension is mandatory.
            // Since we don't support any extensions, if we see a mandatory
            // bit, we skip the config.
            if ((uint16)(ext.Type & (uint16)((uint16)(1 << (int)(15)))) != 0) {
                unsupportedExt = true;
            }
        }
        if (unsupportedExt) {
            continue;
        }
        return Ꮡec;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsNoSupportedSymmetricˢ = "tls: no supported symmetric ciphersuites for ECH"u8;

internal static (echCipher, error) pickECHCipherSuite(slice<echCipher> suites) {
    foreach (var (_, s) in suites) {
        // NOTE: all of the supported AEADs and KDFs are fine, rather than
        // imposing some sort of preference here, we just pick the first valid
        // suite.
        {
            var (_, ok) = hpke.SupportedAEADs[s.AEADID, ꟷ]; if (!ok) {
                continue;
            }
        }
        {
            var (_, ok) = hpke.SupportedKDFs[s.KDFID, ꟷ]; if (!ok) {
                continue;
            }
        }
        return (s, default!);
    }
    return (new echCipher(nil), errors.New(tlsNoSupportedSymmetricˢ));
}

internal static (slice<byte>, error) encodeInnerClientHello(ж<clientHelloMsg> Ꮡinner, nint maxNameLength) {
    ref var inner = ref Ꮡinner.DerefOrNull();

    var (h, err) = Ꮡinner.marshalMsg(true);
    if (err != default!) {
        return (default!, err);
    }
    h = h[4..]; // strip four byte prefix
    nint paddingLen = default!;
    if (inner.serverName != ""u8){
        paddingLen = max(0, maxNameLength - len(inner.serverName));
    } else {
        paddingLen = maxNameLength + 9;
    }
    paddingLen = 31 - ((len(h) + paddingLen - 1) % 32);
    return (appendꓸꓸꓸ(h, makeꓸꓸꓸ<byte>(paddingLen)), default!);
}

internal static bool skipUint8LengthPrefixed(ж<cryptobyte.String> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var skip = ref heap(new uint8(), out var Ꮡskip);
    if (!s.ReadUint8(Ꮡskip)) {
        return false;
    }
    return s.Skip((nint)skip);
}

internal static bool skipUint16LengthPrefixed(ж<cryptobyte.String> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var skip = ref heap(new uint16(), out var Ꮡskip);
    if (!s.ReadUint16(Ꮡskip)) {
        return false;
    }
    return s.Skip((nint)skip);
}

partial struct rawExtension {
    internal uint16 extType;
    internal slice<byte> data;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsMalformedOuterClientˢ = "tls: malformed outer client hello"u8;
internal static readonly @string tlsInvalidInnerClientˢ = "tls: invalid inner client hello"u8;

internal static (slice<rawExtension>, error) extractRawExtensions(ref clientHelloMsg hello) {
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)hello.original);
    if (!s.Skip(4 + 2 + 32) || !skipUint8LengthPrefixed(Ꮡs) || !skipUint16LengthPrefixed(Ꮡs) || !skipUint8LengthPrefixed(Ꮡs)) {
        // header, version, random
        // session ID
        // cipher suites
        // compression methods
        return (default!, errors.New(tlsMalformedOuterClientˢ));
    }
    slice<rawExtension> rawExtensions = default!;
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.ReadUint16LengthPrefixed(Ꮡextensions)) {
        return (default!, errors.New(tlsMalformedOuterClientˢ));
    }
    while (!extensions.Empty()) {
        ref var extension = ref heap(new uint16(), out var Ꮡextension);
        ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
        if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
            return (default!, errors.New(tlsInvalidInnerClientˢ));
        }
        rawExtensions = append(rawExtensions, new rawExtension(extension, extData));
    }
    return (rawExtensions, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInvalidOuterˢ = "tls: invalid outer extensions"u8;
internal static readonly @string tlsInvalidReconstructedˢ = "tls: invalid reconstructed inner client hello"u8;
internal static readonly @string tlsClientSentEncryptedˢ = "tls: client sent encrypted_client_hello extension with unsupported versions"u8;
internal static readonly @string tlsClientSentEncryptedˢ2 = "tls: client sent encrypted_client_hello extension but did not offer TLS 1.3"u8;

internal static (ж<clientHelloMsg>, error) decodeInnerClientHello(ж<clientHelloMsg> Ꮡouter, slice<byte> encoded) {
    ref var outer = ref Ꮡouter.DerefOrNull();

    // Reconstructing the inner client hello from its encoded form is somewhat
    // complicated. It is missing its header (message type and length), session
    // ID, and the extensions may be compressed. Since we need to put the
    // extensions back in the same order as they were in the raw outer hello,
    // and since we don't store the raw extensions, or the order we parsed them
    // in, we need to reparse the raw extensions from the outer hello in order
    // to properly insert them into the inner hello. This _should_ result in raw
    // bytes which match the hello as it was generated by the client.
    ref var innerReader = ref heap<cryptobyte.String>(out var ᏑinnerReader);
    innerReader = ((cryptobyte.String)encoded);
    ref var versionAndRandom = ref heap<slice<byte>>(out var ᏑversionAndRandom);
    ref var sessionID = ref heap<slice<byte>>(out var ᏑsessionID);
    ref var ΔcipherSuites = ref heap<slice<byte>>(out var ᏑcipherSuites);
    ref var compressionMethods = ref heap<slice<byte>>(out var ᏑcompressionMethods);
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!innerReader.ReadBytes(ᏑversionAndRandom, 2 + 32) || !readUint8LengthPrefixed(ᏑinnerReader, ᏑsessionID) || len(sessionID) != 0 || !readUint16LengthPrefixed(ᏑinnerReader, ᏑcipherSuites) || !readUint8LengthPrefixed(ᏑinnerReader, ᏑcompressionMethods) || !innerReader.ReadUint16LengthPrefixed(Ꮡextensions)) {
        return (default!, errors.New(tlsInvalidInnerClientˢ));
    }
    // The specification says we must verify that the trailing padding is all
    // zeros. This is kind of weird for TLS messages, where we generally just
    // throw away any trailing garbage.
    foreach (var (_, p) in innerReader) {
        if (p != 0) {
            return (default!, errors.New(tlsInvalidInnerClientˢ));
        }
    }
    var (rawOuterExts, err) = extractRawExtensions(ref (Ꮡouter).DerefOrNull());
    if (err != default!) {
        return (default!, err);
    }
    var recon = cryptobyte.NewBuilder(default!);
    recon.AddUint8(typeClientHello);
    var rawOuterExtsʗ1 = rawOuterExts;
    recon.AddUint24LengthPrefixed((ж<cryptobyte.Builder> reconΔ1) => {
        reconΔ1.AddBytes(ᏑversionAndRandom.ValueSlot);
        reconΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> reconΔ2) => {
            reconΔ2.AddBytes(Ꮡouter.Value.sessionId);
        });
        reconΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> reconΔ3) => {
            reconΔ3.AddBytes(ᏑcipherSuites.ValueSlot);
        });
        reconΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> reconΔ4) => {
            reconΔ4.AddBytes(ᏑcompressionMethods.ValueSlot);
        });
        var rawOuterExtsʗ2 = rawOuterExtsʗ1;
        reconΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> reconΔ5) => {
            while (!Ꮡextensions.ValueSlot.Empty()) {
                ref var extension = ref heap(new uint16(), out var Ꮡextension);
                ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
                if (!Ꮡextensions.ValueSlot.ReadUint16(Ꮡextension) || !Ꮡextensions.ValueSlot.ReadUint16LengthPrefixed(ᏑextData)) {
                    reconΔ5.SetError(errors.New(tlsInvalidInnerClientˢ));
                    return;
                }
                if (extension == extensionECHOuterExtensions){
                    if (!ᏑextData.ValueSlot.ReadUint8LengthPrefixed(ᏑextData)) {
                        reconΔ5.SetError(errors.New(tlsInvalidInnerClientˢ));
                        return;
                    }
                    nint i = default!;
                    while (!ᏑextData.ValueSlot.Empty()) {
                        ref var extType = ref heap(new uint16(), out var ᏑextType);
                        if (!ᏑextData.ValueSlot.ReadUint16(ᏑextType)) {
                            reconΔ5.SetError(errors.New(tlsInvalidInnerClientˢ));
                            return;
                        }
                        if (extType == extensionEncryptedClientHello) {
                            reconΔ5.SetError(errors.New(tlsInvalidOuterˢ));
                            return;
                        }
                        for (; i <= len(rawOuterExtsʗ2); i++) {
                            if (i == len(rawOuterExtsʗ2)) {
                                reconΔ5.SetError(errors.New(tlsInvalidOuterˢ));
                                return;
                            }
                            if (rawOuterExtsʗ2[i].extType == extType) {
                                break;
                            }
                        }
                        reconΔ5.AddUint16(rawOuterExtsʗ2[i].extType);
                        var rawOuterExtsʗ3 = rawOuterExtsʗ2;
                        reconΔ5.AddUint16LengthPrefixed((ж<cryptobyte.Builder> reconΔ6) => {
                            reconΔ6.AddBytes(rawOuterExtsʗ3[i].data);
                        });
                    }
                } else {
                    reconΔ5.AddUint16(extension);
                    reconΔ5.AddUint16LengthPrefixed((ж<cryptobyte.Builder> reconΔ7) => {
                        reconΔ7.AddBytes(ᏑextData.ValueSlot);
                    });
                }
            }
        });
    });
    (var reconBytes, err) = recon.Bytes();
    if (err != default!) {
        return (default!, err);
    }
    var inner = Ꮡ(new clientHelloMsg(nil));
    if (!inner.unmarshal(reconBytes)) {
        return (default!, errors.New(tlsInvalidReconstructedˢ));
    }
    if (!bytes.Equal((~inner).encryptedClientHello, new byte[]{(uint8)innerECHExt}.slice())) {
        return (default!, errInvalidECHExt);
    }
    var hasTLS13 = false;
    foreach (var (_, v) in (~inner).supportedVersions) {
        // Skip GREASE values (values of the form 0x?A0A).
        // GREASE (Generate Random Extensions And Sustain Extensibility) is a mechanism used by
        // browsers like Chrome to ensure TLS implementations correctly ignore unknown values.
        // GREASE values follow a specific pattern: 0x?A0A, where ? can be any hex digit.
        // These values should be ignored when processing supported TLS versions.
        if ((uint16)(v & 0x0F0F) == 0x0A0A && (uint16)(v & 0xff) == (uint16)((v >> (int)(8)))) {
            continue;
        }
        // Ensure at least TLS 1.3 is offered.
        if (v == VersionTLS13){
            hasTLS13 = true;
        } else 
        if (v < VersionTLS13) {
            // Reject if any non-GREASE value is below TLS 1.3, as ECH requires TLS 1.3+.
            return (default!, errors.New(tlsClientSentEncryptedˢ));
        }
    }
    if (!hasTLS13) {
        return (default!, errors.New(tlsClientSentEncryptedˢ2));
    }
    return (inner, default!);
}

internal static (slice<byte>, error) decryptECHPayload(ж<hpke.Receipient> Ꮡcontext, slice<byte> hello, slice<byte> payload) {
    ref var context = ref Ꮡcontext.DerefOrNull();

    var outerAAD = bytes.Replace(hello[4..], payload, new slice<byte>(len(payload)), 1);
    return context.Open(outerAAD, payload);
}

internal static (slice<byte>, error) generateOuterECHExt(uint8 id, uint16 kdfID, uint16 aeadID, slice<byte> encodedKey, slice<byte> payload) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(0); // outer
    b.AddUint16(kdfID);
    b.AddUint16(aeadID);
    b.AddUint8(id);
    var encodedKeyʗ1 = encodedKey;
    Ꮡb.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddBytes(encodedKeyʗ1);
    });
    var payloadʗ1 = payload;
    Ꮡb.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
        bΔ2.AddBytes(payloadʗ1);
    });
    return b.Bytes();
}

internal static error computeAndUpdateOuterECHExtension(ж<clientHelloMsg> Ꮡouter, ж<clientHelloMsg> Ꮡinner, ref echClientContext ech, bool useKey) {
    ref var outer = ref Ꮡouter.DerefOrNull();

    slice<byte> encapKey = default!;
    if (useKey) {
        encapKey = ech.encapsulatedKey;
    }
    var (encodedInner, err) = encodeInnerClientHello(Ꮡinner, (nint)(~ech.config).MaxNameLength);
    if (err != default!) {
        return err;
    }
    // NOTE: the tag lengths for all of the supported AEADs are the same (16
    // bytes), so we have hardcoded it here. If we add support for another AEAD
    // with a different tag length, we will need to change this.
    nint encryptedLen = len(encodedInner) + 16; // AEAD tag length
    (outer.encryptedClientHello, err) = generateOuterECHExt((~ech.config).ConfigID, ech.kdfID, ech.aeadID, encapKey, new slice<byte>(encryptedLen));
    if (err != default!) {
        return err;
    }
    (var serializedOuter, err) = Ꮡouter.marshal();
    if (err != default!) {
        return err;
    }
    serializedOuter = serializedOuter[4..]; // strip the four byte prefix
    (var encryptedInner, err) = ech.hpkeContext.Seal(serializedOuter, encodedInner);
    if (err != default!) {
        return err;
    }
    (outer.encryptedClientHello, err) = generateOuterECHExt((~ech.config).ConfigID, ech.kdfID, ech.aeadID, encapKey, encryptedInner);
    if (err != default!) {
        return err;
    }
    return default!;
}

// validDNSName is a rather rudimentary check for the validity of a DNS name.
// This is used to check if the public_name in a ECHConfig is valid when we are
// picking a config. This can be somewhat lax because even if we pick a
// valid-looking name, the DNS layer will later reject it anyway.
internal static bool validDNSName(@string name) {
    if (len(name) > 253) {
        return false;
    }
    var labels = strings.Split(name, "."u8);
    if (len(labels) <= 1) {
        return false;
    }
    foreach (var (_, l) in labels) {
        nint labelLen = len(l);
        if (labelLen == 0) {
            return false;
        }
        foreach (var (i, r) in l) {
            if (r == (rune)'-' && (i == 0 || i == labelLen - 1)) {
                return false;
            }
            if ((r < (rune)'0' || r > (rune)'9') && (r < (rune)'a' || r > (rune)'z') && (r < (rune)'A' || r > (rune)'Z') && r != (rune)'-') {
                return false;
            }
        }
    }
    return true;
}

// ECHRejectionError is the error type returned when ECH is rejected by a remote
// server. If the server offered a ECHConfigList to use for retries, the
// RetryConfigList field will contain this list.
//
// The client may treat an ECHRejectionError with an empty set of RetryConfigs
// as a secure signal from the server.
partial struct ECHRejectionError {
    public slice<byte> RetryConfigList;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerRejectedEchˢ = "tls: server rejected ECH"u8;

public static @string Error(this ref ECHRejectionError e) {
    return tlsServerRejectedEchˢ;
}

internal static error errMalformedECHExt = errors.New("tls: malformed encrypted_client_hello extension"u8);

internal static error errInvalidECHExt = errors.New("tls: client sent invalid encrypted_client_hello extension"u8);

partial struct echExtType /*num:uint8*/;

internal static echExtType innerECHExt => 1;
internal static echExtType outerECHExt => 0;

internal static (echExtType echType, echCipher cs, uint8 configID, slice<byte> encap, slice<byte> payload, error err) parseECHExt(slice<byte> ext) {
    echExtType echType = default!;
    ref var cs = ref heap(new echCipher(), out var Ꮡcs);
    ref var configID = ref heap(new uint8(), out var ᏑconfigID);
    ref var encap = ref heap<slice<byte>>(out var Ꮡencap);
    ref var payload = ref heap<slice<byte>>(out var Ꮡpayload);
    error err = default!;

    var data = new slice<byte>(len(ext));
    copy(data, ext);
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    ref var echInt = ref heap(new uint8(), out var ᏑechInt);
    if (!s.ReadUint8(ᏑechInt)) {
        err = errMalformedECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    echType = ((echExtType)echInt);
    if (echType == innerECHExt) {
        if (!s.Empty()) {
            err = errMalformedECHExt;
            return (echType, cs, configID, encap, payload, err);
        }
        return (echType, cs, 0, default!, default!, default!);
    }
    if (echType != outerECHExt) {
        err = errInvalidECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    if (!s.ReadUint16(Ꮡcs.of(echCipher.ᏑKDFID))) {
        err = errMalformedECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    if (!s.ReadUint16(Ꮡcs.of(echCipher.ᏑAEADID))) {
        err = errMalformedECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    if (!s.ReadUint8(ᏑconfigID)) {
        err = errMalformedECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    if (!readUint16LengthPrefixed(Ꮡs, Ꮡencap)) {
        err = errMalformedECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    if (!readUint16LengthPrefixed(Ꮡs, Ꮡpayload)) {
        err = errMalformedECHExt;
        return (echType, cs, configID, encap, payload, err);
    }
    // NOTE: clone encap and payload so that mutating them does not mutate the
    // raw extension bytes.
    return (echType, cs, configID, bytes.Clone(encap), bytes.Clone(payload), default!);
}

internal static (slice<byte>, error) marshalEncryptedClientHelloConfigList(slice<EncryptedClientHelloKey> configs) {
    var builder = cryptobyte.NewBuilder(default!);
    var configsʗ1 = configs;
    builder.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ1) => {
        foreach (var (_, c) in configsʗ1) {
            builderΔ1.AddBytes(c.Config);
        }
    });
    return builder.Bytes();
}

internal static (ж<clientHelloMsg>, ж<echServerContext>, error) processECHClientHello(this ж<Conn> Ꮡc, ж<clientHelloMsg> Ꮡouter) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var outer = ref Ꮡouter.DerefOrNull();

    ref var echCiphersuite = ref heap<echCipher>(out var ᏑechCiphersuite);
    ref var configID = ref heap<uint8>(out var ᏑconfigID);
    (var echType, echCiphersuite, configID, var encap, var payload, var err) = parseECHExt(outer.encryptedClientHello);
    if (err != default!) {
        if (errors.Is(err, errInvalidECHExt)){
            Ꮡc.sendAlert(alertIllegalParameter);
        } else {
            Ꮡc.sendAlert(alertDecodeError);
        }
        return (default!, default!, errInvalidECHExt);
    }
    if (echType == innerECHExt) {
        return (Ꮡouter, Ꮡ(new echServerContext(inner: true)), default!);
    }
    if (len((~c.config).EncryptedClientHelloKeys) == 0) {
        return (Ꮡouter, default!, default!);
    }
    foreach (var (_, echKey) in (~c.config).EncryptedClientHelloKeys) {
        var (skip, config, errΔ1) = parseECHConfig(echKey.Config);
        if (errΔ1 != default! || skip) {
            Ꮡc.sendAlert(alertInternalError);
            return (default!, default!, fmt.Errorf("tls: invalid EncryptedClientHelloKeys Config: %s"u8, errΔ1));
        }
        if (skip) {
            continue;
        }
        (var echPriv, errΔ1) = hpke.ParseHPKEPrivateKey(config.KemID, echKey.PrivateKey);
        if (errΔ1 != default!) {
            Ꮡc.sendAlert(alertInternalError);
            return (default!, default!, fmt.Errorf("tls: invalid EncryptedClientHelloKeys PrivateKey: %s"u8, errΔ1));
        }
        var info = appendꓸꓸꓸ(slice<byte>("tls ech\x00"u8), echKey.Config);
        (var hpkeContext, errΔ1) = hpke.SetupReceipient(hpke.DHKEM_X25519_HKDF_SHA256, echCiphersuite.KDFID, echCiphersuite.AEADID, echPriv, info, encap);
        if (errΔ1 != default!) {
            // attempt next trial decryption
            continue;
        }
        (var encodedInner, errΔ1) = decryptECHPayload(hpkeContext, outer.original, payload);
        if (errΔ1 != default!) {
            // attempt next trial decryption
            continue;
        }
        // NOTE: we do not enforce that the sent server_name matches the ECH
        // configs PublicName, since this is not particularly important, and
        // the client already had to know what it was in order to properly
        // encrypt the payload. This is only a MAY in the spec, so we're not
        // doing anything revolutionary.
        (var echInner, errΔ1) = decodeInnerClientHello(Ꮡouter, encodedInner);
        if (errΔ1 != default!) {
            Ꮡc.sendAlert(alertIllegalParameter);
            return (default!, default!, errInvalidECHExt);
        }
        c.echAccepted = true;
        return (echInner, Ꮡ(new echServerContext(
            hpkeContext: hpkeContext,
            configID: configID,
            ciphersuite: echCiphersuite
        )), default!);
    }
    return (Ꮡouter, default!, default!);
}

internal static (slice<byte>, error) buildRetryConfigList(slice<EncryptedClientHelloKey> keys) {
    bool atLeastOneRetryConfig = default!;
    ref var retryBuilder = ref heap(new cryptobyte.Builder(), out var ᏑretryBuilder);
    var keysʗ1 = keys;
    ᏑretryBuilder.AddUint16LengthPrefixed((ж<cryptobyte.Builder> b) => {
        foreach (var (_, c) in keysʗ1) {
            if (!c.SendAsRetry) {
                continue;
            }
            atLeastOneRetryConfig = true;
            b.AddBytes(c.Config);
        }
    });
    if (!atLeastOneRetryConfig) {
        return (default!, default!);
    }
    return retryBuilder.Bytes();
}

} // end tls_package
