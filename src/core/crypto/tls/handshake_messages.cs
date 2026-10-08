// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using errors = errors_package;
using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using vendor.golang.org.x.crypto;

partial class tls_package {

internal delegate error marshalingFunction(ж<cryptobyte.Builder> b);

internal static error Marshal(this marshalingFunction f, ж<cryptobyte.Builder> Ꮡb) {
    return f(Ꮡb);
}

// addBytesWithLength appends a sequence of bytes to the cryptobyte.Builder. If
// the length of the sequence is not the value specified, it produces an error.
internal static void addBytesWithLength(ж<cryptobyte.Builder> Ꮡb, slice<byte> v, nint n) {
    var vʗ1 = v;
    Ꮡb.AddValue(new marshalingFunctionᴠMarshalingValue(new marshalingFunction(error (ж<cryptobyte.Builder> bΔ1) => {
        if (len(vʗ1) != n) {
            return fmt.Errorf("invalid value length: expected %d, got %d"u8, n, len(vʗ1));
        }
        bΔ1.AddBytes(vʗ1);
        return default!;
    })));
}

// addUint64 appends a big-endian, 64-bit value to the cryptobyte.Builder.
internal static void addUint64(ж<cryptobyte.Builder> Ꮡb, uint64 v) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.AddUint32((uint32)((v >> (int)(32))));
    b.AddUint32((uint32)v);
}

// readUint64 decodes a big-endian, 64-bit value into out and advances over it.
// It reports whether the read was successful.
internal static bool readUint64(ж<cryptobyte.String> Ꮡs, ref uint64 @out) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var hi = ref heap(new uint32(), out var Ꮡhi);
    ref var lo = ref heap(new uint32(), out var Ꮡlo);
    if (!s.ReadUint32(Ꮡhi) || !s.ReadUint32(Ꮡlo)) {
        return false;
    }
    @out = (uint64)(((uint64)hi << (int)(32)) | (uint64)lo);
    return true;
}

// readUint8LengthPrefixed acts like s.ReadUint8LengthPrefixed, but targets a
// []byte instead of a cryptobyte.String.
internal static bool readUint8LengthPrefixed(ж<cryptobyte.String> Ꮡs, ж<slice<byte>> Ꮡout) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.ReadUint8LengthPrefixed(Ꮡout.Reinterpret<slice<byte>, cryptobyte.String>());
}

// readUint16LengthPrefixed acts like s.ReadUint16LengthPrefixed, but targets a
// []byte instead of a cryptobyte.String.
internal static bool readUint16LengthPrefixed(ж<cryptobyte.String> Ꮡs, ж<slice<byte>> Ꮡout) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.ReadUint16LengthPrefixed(Ꮡout.Reinterpret<slice<byte>, cryptobyte.String>());
}

// readUint24LengthPrefixed acts like s.ReadUint24LengthPrefixed, but targets a
// []byte instead of a cryptobyte.String.
internal static bool readUint24LengthPrefixed(ж<cryptobyte.String> Ꮡs, ж<slice<byte>> Ꮡout) {
    ref var s = ref Ꮡs.DerefOrNull();

    return s.ReadUint24LengthPrefixed(Ꮡout.Reinterpret<slice<byte>, cryptobyte.String>());
}

partial struct clientHelloMsg {
    internal slice<byte> original;
    internal uint16 vers;
    internal slice<byte> random;
    internal slice<byte> sessionId;
    internal slice<uint16> cipherSuites;
    internal slice<uint8> compressionMethods;
    internal @string serverName;
    internal bool ocspStapling;
    internal slice<CurveID> supportedCurves;
    internal slice<uint8> supportedPoints;
    internal bool ticketSupported;
    internal slice<uint8> sessionTicket;
    internal slice<SignatureScheme> supportedSignatureAlgorithms;
    internal slice<SignatureScheme> supportedSignatureAlgorithmsCert;
    internal bool secureRenegotiationSupported;
    internal slice<byte> secureRenegotiation;
    internal bool extendedMasterSecret;
    internal slice<@string> alpnProtocols;
    internal bool scts;
    internal slice<uint16> supportedVersions;
    internal slice<byte> cookie;
    internal slice<keyShare> keyShares;
    internal bool earlyData;
    internal slice<uint8> pskModes;
    internal slice<pskIdentity> pskIdentities;
    internal slice<slice<byte>> pskBinders;
    internal slice<byte> quicTransportParameters;
    internal slice<byte> encryptedClientHello;
    // extensions are only populated on the server-side of a handshake
    internal slice<uint16> extensions;
}

internal static (slice<byte>, error) marshalMsg(this ж<clientHelloMsg> Ꮡm, bool echInner) {
    ref var m = ref Ꮡm.DerefOrNull();

    ref var exts = ref heap(new cryptobyte.Builder(), out var Ꮡexts);
    if (len(m.serverName) > 0) {
        // RFC 6066, Section 3
        exts.AddUint16(extensionServerName);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ1) => {
            extsΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ2) => {
                extsΔ2.AddUint8(0); // name_type = host_name
                extsΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ3) => {
                    extsΔ3.AddBytes(slice<byte>(Ꮡm.Value.serverName));
                });
            });
        });
    }
    if (len(m.supportedPoints) > 0 && !echInner) {
        // RFC 4492, Section 5.1.2
        exts.AddUint16(extensionSupportedPoints);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ4) => {
            extsΔ4.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ5) => {
                extsΔ5.AddBytes(Ꮡm.Value.supportedPoints);
            });
        });
    }
    if (m.ticketSupported && !echInner) {
        // RFC 5077, Section 3.2
        exts.AddUint16(extensionSessionTicket);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ6) => {
            extsΔ6.AddBytes(Ꮡm.Value.sessionTicket);
        });
    }
    if (m.secureRenegotiationSupported && !echInner) {
        // RFC 5746, Section 3.2
        exts.AddUint16(extensionRenegotiationInfo);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ7) => {
            extsΔ7.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ8) => {
                extsΔ8.AddBytes(Ꮡm.Value.secureRenegotiation);
            });
        });
    }
    if (m.extendedMasterSecret && !echInner) {
        // RFC 7627
        exts.AddUint16(extensionExtendedMasterSecret);
        exts.AddUint16(0); // empty extension_data
    }
    if (m.scts) {
        // RFC 6962, Section 3.3.1
        exts.AddUint16(extensionSCT);
        exts.AddUint16(0); // empty extension_data
    }
    if (m.earlyData) {
        // RFC 8446, Section 4.2.10
        exts.AddUint16(extensionEarlyData);
        exts.AddUint16(0); // empty extension_data
    }
    if (m.quicTransportParameters != default!) {
        // marshal zero-length parameters when present
        // RFC 9001, Section 8.2
        exts.AddUint16(extensionQUICTransportParameters);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ9) => {
            extsΔ9.AddBytes(Ꮡm.Value.quicTransportParameters);
        });
    }
    if (len(m.encryptedClientHello) > 0) {
        exts.AddUint16(extensionEncryptedClientHello);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ10) => {
            extsΔ10.AddBytes(Ꮡm.Value.encryptedClientHello);
        });
    }
    // Note that any extension that can be compressed during ECH must be
    // contiguous. If any additional extensions are to be compressed they must
    // be added to the following block, so that they can be properly
    // decompressed on the other side.
    slice<uint16> echOuterExts = default!;
    if (m.ocspStapling) {
        // RFC 4366, Section 3.6
        if (echInner){
            echOuterExts = append(echOuterExts, extensionStatusRequest);
        } else {
            exts.AddUint16(extensionStatusRequest);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ11) => {
                extsΔ11.AddUint8(1); // status_type = ocsp
                extsΔ11.AddUint16(0); // empty responder_id_list
                extsΔ11.AddUint16(0); // empty request_extensions
            });
        }
    }
    if (len(m.supportedCurves) > 0) {
        // RFC 4492, sections 5.1.1 and RFC 8446, Section 4.2.7
        if (echInner){
            echOuterExts = append(echOuterExts, extensionSupportedCurves);
        } else {
            exts.AddUint16(extensionSupportedCurves);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ12) => {
                extsΔ12.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ13) => {
                    foreach (var (_, curve) in Ꮡm.Value.supportedCurves) {
                        extsΔ13.AddUint16((uint16)curve);
                    }
                });
            });
        }
    }
    if (len(m.supportedSignatureAlgorithms) > 0) {
        // RFC 5246, Section 7.4.1.4.1
        if (echInner){
            echOuterExts = append(echOuterExts, extensionSignatureAlgorithms);
        } else {
            exts.AddUint16(extensionSignatureAlgorithms);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ14) => {
                extsΔ14.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ15) => {
                    foreach (var (_, sigAlgo) in Ꮡm.Value.supportedSignatureAlgorithms) {
                        extsΔ15.AddUint16((uint16)sigAlgo);
                    }
                });
            });
        }
    }
    if (len(m.supportedSignatureAlgorithmsCert) > 0) {
        // RFC 8446, Section 4.2.3
        if (echInner){
            echOuterExts = append(echOuterExts, extensionSignatureAlgorithmsCert);
        } else {
            exts.AddUint16(extensionSignatureAlgorithmsCert);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ16) => {
                extsΔ16.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ17) => {
                    foreach (var (_, sigAlgo) in Ꮡm.Value.supportedSignatureAlgorithmsCert) {
                        extsΔ17.AddUint16((uint16)sigAlgo);
                    }
                });
            });
        }
    }
    if (len(m.alpnProtocols) > 0) {
        // RFC 7301, Section 3.1
        if (echInner){
            echOuterExts = append(echOuterExts, extensionALPN);
        } else {
            exts.AddUint16(extensionALPN);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ18) => {
                extsΔ18.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ19) => {
                    foreach (var (_, proto) in Ꮡm.Value.alpnProtocols) {
                        extsΔ19.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ20) => {
                            extsΔ20.AddBytes(slice<byte>(proto));
                        });
                    }
                });
            });
        }
    }
    if (len(m.supportedVersions) > 0) {
        // RFC 8446, Section 4.2.1
        if (echInner){
            echOuterExts = append(echOuterExts, extensionSupportedVersions);
        } else {
            exts.AddUint16(extensionSupportedVersions);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ21) => {
                extsΔ21.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ22) => {
                    foreach (var (_, vers) in Ꮡm.Value.supportedVersions) {
                        extsΔ22.AddUint16(vers);
                    }
                });
            });
        }
    }
    if (len(m.cookie) > 0) {
        // RFC 8446, Section 4.2.2
        if (echInner){
            echOuterExts = append(echOuterExts, extensionCookie);
        } else {
            exts.AddUint16(extensionCookie);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ23) => {
                extsΔ23.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ24) => {
                    extsΔ24.AddBytes(Ꮡm.Value.cookie);
                });
            });
        }
    }
    if (len(m.keyShares) > 0) {
        // RFC 8446, Section 4.2.8
        if (echInner){
            echOuterExts = append(echOuterExts, extensionKeyShare);
        } else {
            exts.AddUint16(extensionKeyShare);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ25) => {
                extsΔ25.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ26) => {
                    foreach (var (_, vᴛ1) in Ꮡm.Value.keyShares) {
                        ref var ks = ref heap(new keyShare(), out var Ꮡks);
                        ks = vᴛ1;

                        extsΔ26.AddUint16((uint16)ks.group);
                        var ksʗ1 = ks;
                        extsΔ26.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ27) => {
                            extsΔ27.AddBytes(ksʗ1.data);
                        });
                    }
                });
            });
        }
    }
    if (len(m.pskModes) > 0) {
        // RFC 8446, Section 4.2.9
        if (echInner){
            echOuterExts = append(echOuterExts, extensionPSKModes);
        } else {
            exts.AddUint16(extensionPSKModes);
            Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ28) => {
                extsΔ28.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ29) => {
                    extsΔ29.AddBytes(Ꮡm.Value.pskModes);
                });
            });
        }
    }
    if (len(echOuterExts) > 0 && echInner) {
        exts.AddUint16(extensionECHOuterExtensions);
        var echOuterExtsʗ1 = echOuterExts;
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ30) => {
            var echOuterExtsʗ2 = echOuterExtsʗ1;
            extsΔ30.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ31) => {
                foreach (var (_, e) in echOuterExtsʗ2) {
                    extsΔ31.AddUint16(e);
                }
            });
        });
    }
    if (len(m.pskIdentities) > 0) {
        // pre_shared_key must be the last extension
        // RFC 8446, Section 4.2.11
        exts.AddUint16(extensionPreSharedKey);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ32) => {
            extsΔ32.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ33) => {
                foreach (var (_, vᴛ2) in Ꮡm.Value.pskIdentities) {
                    ref var psk = ref heap(new pskIdentity(), out var Ꮡpsk);
                    psk = vᴛ2;

                    var pskʗ1 = psk;
                    extsΔ33.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ34) => {
                        extsΔ34.AddBytes(pskʗ1.label);
                    });
                    extsΔ33.AddUint32(psk.obfuscatedTicketAge);
                }
            });
            extsΔ32.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ35) => {
                foreach (var (_, binder) in Ꮡm.Value.pskBinders) {
                    var binderʗ1 = binder;
                    extsΔ35.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ36) => {
                        extsΔ36.AddBytes(binderʗ1);
                    });
                }
            });
        });
    }
    var (extBytes, err) = exts.Bytes();
    if (err != default!) {
        return (default!, err);
    }
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeClientHello);
    var extBytesʗ1 = extBytes;
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddUint16(Ꮡm.Value.vers);
        addBytesWithLength(bΔ1, Ꮡm.Value.random, 32);
        bΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            if (!echInner) {
                bΔ2.AddBytes(Ꮡm.Value.sessionId);
            }
        });
        bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
            foreach (var (_, suite) in Ꮡm.Value.cipherSuites) {
                bΔ3.AddUint16(suite);
            }
        });
        bΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
            bΔ4.AddBytes(Ꮡm.Value.compressionMethods);
        });
        if (len(extBytesʗ1) > 0) {
            var extBytesʗ2 = extBytesʗ1;
            bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                bΔ5.AddBytes(extBytesʗ2);
            });
        }
    });
    return b.Bytes();
}

internal static (slice<byte>, error) marshal(this ж<clientHelloMsg> Ꮡm) {
    return Ꮡm.marshalMsg(false);
}

// marshalWithoutBinders returns the ClientHello through the
// PreSharedKeyExtension.identities field, according to RFC 8446, Section
// 4.2.11.2. Note that m.pskBinders must be set to slices of the correct length.
internal static (slice<byte>, error) marshalWithoutBinders(this ж<clientHelloMsg> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    nint bindersLen = 2; // uint16 length prefix
    foreach (var (_, binder) in m.pskBinders) {
        bindersLen += 1; // uint8 length prefix
        bindersLen += len(binder);
    }
    slice<byte> fullMessage = default!;
    if (m.original != default!){
        fullMessage = m.original;
    } else {
        error err = default!;
        (fullMessage, err) = Ꮡm.marshal();
        if (err != default!) {
            return (default!, err);
        }
    }
    return (fullMessage.slice(0, len(fullMessage) - bindersLen), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorˢ5 = "tls: internal error: pskBinders length mismatch"u8;

// updateBinders updates the m.pskBinders field. The supplied binders must have
// the same length as the current m.pskBinders.
internal static error updateBinders(this ref clientHelloMsg m, slice<slice<byte>> pskBinders) {
    if (len(pskBinders) != len(m.pskBinders)) {
        return errors.New(tlsInternalErrorˢ5);
    }
    foreach (var (i, _) in m.pskBinders) {
        if (len(pskBinders[i]) != len(m.pskBinders[i])) {
            return errors.New(tlsInternalErrorˢ5);
        }
    }
    m.pskBinders = pskBinders;
    return default!;
}

internal static bool unmarshal(this ж<clientHelloMsg> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    m = new clientHelloMsg(original: data);
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    if (!s.Skip(4) || !s.ReadUint16(Ꮡm.of(clientHelloMsg.Ꮡvers)) || !s.ReadBytes(Ꮡm.of(clientHelloMsg.Ꮡrandom), // message type and uint24 length field
 32) || !readUint8LengthPrefixed(Ꮡs, Ꮡm.of(clientHelloMsg.ᏑsessionId))) {
        return false;
    }
    ref var ΔcipherSuites = ref heap<cryptobyte.String>(out var ᏑcipherSuites);
    if (!s.ReadUint16LengthPrefixed(ᏑcipherSuites)) {
        return false;
    }
    m.cipherSuites = new uint16[]{}.slice();
    m.secureRenegotiationSupported = false;
    while (!ΔcipherSuites.Empty()) {
        ref var suite = ref heap(new uint16(), out var Ꮡsuite);
        if (!ΔcipherSuites.ReadUint16(Ꮡsuite)) {
            return false;
        }
        if (suite == scsvRenegotiation) {
            m.secureRenegotiationSupported = true;
        }
        m.cipherSuites = append(m.cipherSuites, suite);
    }
    if (!readUint8LengthPrefixed(Ꮡs, Ꮡm.of(clientHelloMsg.ᏑcompressionMethods))) {
        return false;
    }
    if (s.Empty()) {
        // ClientHello is optionally followed by extension data
        return true;
    }
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.ReadUint16LengthPrefixed(Ꮡextensions) || !s.Empty()) {
        return false;
    }
    var seenExts = new map<uint16, bool>();
    while (!extensions.Empty()) {
        ref var extension = ref heap(new uint16(), out var Ꮡextension);
        ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
        if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
            return false;
        }
        if (seenExts[extension]) {
            return false;
        }
        seenExts[extension] = true;
        m.extensions = append(m.extensions, extension);
        switch (extension) {
        case extensionServerName: {
            // RFC 6066, Section 3
            ref var nameList = ref heap<cryptobyte.String>(out var ᏑnameList);
            if (!extData.ReadUint16LengthPrefixed(ᏑnameList) || nameList.Empty()) {
                return false;
            }
            while (!nameList.Empty()) {
                ref var nameType = ref heap(new uint8(), out var ᏑnameType);
                ref var serverName = ref heap<cryptobyte.String>(out var ᏑserverName);
                if (!nameList.ReadUint8(ᏑnameType) || !nameList.ReadUint16LengthPrefixed(ᏑserverName) || serverName.Empty()) {
                    return false;
                }
                if (nameType != 0) {
                    continue;
                }
                if (len(m.serverName) != 0) {
                    // Multiple names of the same name_type are prohibited.
                    return false;
                }
                m.serverName = ((@string)(slice<byte>)serverName);
                // An SNI value may not include a trailing dot.
                if (strings.HasSuffix(m.serverName, "."u8)) {
                    return false;
                }
            }
            break;
        }
        case extensionStatusRequest: {
            // RFC 4366, Section 3.6
            ref var statusType = ref heap(new uint8(), out var ᏑstatusType);
            ref var ignored = ref heap<cryptobyte.String>(out var Ꮡignored);
            if (!extData.ReadUint8(ᏑstatusType) || !extData.ReadUint16LengthPrefixed(Ꮡignored) || !extData.ReadUint16LengthPrefixed(Ꮡignored)) {
                return false;
            }
            m.ocspStapling = statusType == statusTypeOCSP;
            break;
        }
        case extensionSupportedCurves: {
            // RFC 4492, sections 5.1.1 and RFC 8446, Section 4.2.7
            ref var curves = ref heap<cryptobyte.String>(out var Ꮡcurves);
            if (!extData.ReadUint16LengthPrefixed(Ꮡcurves) || curves.Empty()) {
                return false;
            }
            while (!curves.Empty()) {
                ref var curve = ref heap(new uint16(), out var Ꮡcurve);
                if (!curves.ReadUint16(Ꮡcurve)) {
                    return false;
                }
                m.supportedCurves = append(m.supportedCurves, ((CurveID)curve));
            }
            break;
        }
        case extensionSupportedPoints: {
            if (!readUint8LengthPrefixed(ᏑextData, // RFC 4492, Section 5.1.2
 Ꮡm.of(clientHelloMsg.ᏑsupportedPoints)) || len(m.supportedPoints) == 0) {
                return false;
            }
            break;
        }
        case extensionSessionTicket: {
            m.ticketSupported = true;
            extData.ReadBytes(Ꮡm.of(clientHelloMsg.ᏑsessionTicket), // RFC 5077, Section 3.2
 len(extData));
            break;
        }
        case extensionSignatureAlgorithms: {
            // RFC 5246, Section 7.4.1.4.1
            ref var sigAndAlgs = ref heap<cryptobyte.String>(out var ᏑsigAndAlgs);
            if (!extData.ReadUint16LengthPrefixed(ᏑsigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                ref var sigAndAlg = ref heap(new uint16(), out var ᏑsigAndAlg);
                if (!sigAndAlgs.ReadUint16(ᏑsigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithms = append(
                    m.supportedSignatureAlgorithms, ((SignatureScheme)sigAndAlg));
            }
            break;
        }
        case extensionSignatureAlgorithmsCert: {
            // RFC 8446, Section 4.2.3
            ref var sigAndAlgs = ref heap<cryptobyte.String>(out var ᏑsigAndAlgs);
            if (!extData.ReadUint16LengthPrefixed(ᏑsigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                ref var sigAndAlg = ref heap(new uint16(), out var ᏑsigAndAlg);
                if (!sigAndAlgs.ReadUint16(ᏑsigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithmsCert = append(
                    m.supportedSignatureAlgorithmsCert, ((SignatureScheme)sigAndAlg));
            }
            break;
        }
        case extensionRenegotiationInfo: {
            if (!readUint8LengthPrefixed(ᏑextData, // RFC 5746, Section 3.2
 Ꮡm.of(clientHelloMsg.ᏑsecureRenegotiation))) {
                return false;
            }
            m.secureRenegotiationSupported = true;
            break;
        }
        case extensionExtendedMasterSecret: {
            m.extendedMasterSecret = true;
            break;
        }
        case extensionALPN: {
// RFC 7627

            // RFC 7301, Section 3.1
            ref var protoList = ref heap<cryptobyte.String>(out var ᏑprotoList);
            if (!extData.ReadUint16LengthPrefixed(ᏑprotoList) || protoList.Empty()) {
                return false;
            }
            while (!protoList.Empty()) {
                ref var proto = ref heap<cryptobyte.String>(out var Ꮡproto);
                if (!protoList.ReadUint8LengthPrefixed(Ꮡproto) || proto.Empty()) {
                    return false;
                }
                m.alpnProtocols = append(m.alpnProtocols, ((@string)(slice<byte>)proto));
            }
            break;
        }
        case extensionSCT: {
            m.scts = true;
            break;
        }
        case extensionSupportedVersions: {
// RFC 6962, Section 3.3.1

            // RFC 8446, Section 4.2.1
            ref var versList = ref heap<cryptobyte.String>(out var ᏑversList);
            if (!extData.ReadUint8LengthPrefixed(ᏑversList) || versList.Empty()) {
                return false;
            }
            while (!versList.Empty()) {
                ref var vers = ref heap(new uint16(), out var Ꮡvers);
                if (!versList.ReadUint16(Ꮡvers)) {
                    return false;
                }
                m.supportedVersions = append(m.supportedVersions, vers);
            }
            break;
        }
        case extensionCookie: {
            if (!readUint16LengthPrefixed(ᏑextData, // RFC 8446, Section 4.2.2
 Ꮡm.of(clientHelloMsg.Ꮡcookie)) || len(m.cookie) == 0) {
                return false;
            }
            break;
        }
        case extensionKeyShare: {
            // RFC 8446, Section 4.2.8
            ref var clientShares = ref heap<cryptobyte.String>(out var ᏑclientShares);
            if (!extData.ReadUint16LengthPrefixed(ᏑclientShares)) {
                return false;
            }
            while (!clientShares.Empty()) {
                ref var ks = ref heap(new keyShare(), out var Ꮡks);
                if (!clientShares.ReadUint16(Ꮡks.of(keyShare.Ꮡgroup).Reinterpret<CurveID, uint16>()) || !readUint16LengthPrefixed(ᏑclientShares, Ꮡks.of(keyShare.Ꮡdata)) || len(ks.data) == 0) {
                    return false;
                }
                m.keyShares = append(m.keyShares, ks);
            }
            break;
        }
        case extensionEarlyData: {
            m.earlyData = true;
            break;
        }
        case extensionPSKModes: {
            if (!readUint8LengthPrefixed(ᏑextData, // RFC 8446, Section 4.2.10
 // RFC 8446, Section 4.2.9
 Ꮡm.of(clientHelloMsg.ᏑpskModes))) {
                return false;
            }
            break;
        }
        case extensionQUICTransportParameters: {
            m.quicTransportParameters = new slice<byte>(len(extData));
            if (!extData.CopyBytes(m.quicTransportParameters)) {
                return false;
            }
            break;
        }
        case extensionPreSharedKey: {
            if (!extensions.Empty()) {
                // RFC 8446, Section 4.2.11
                return false; // pre_shared_key must be the last extension
            }
            ref var identities = ref heap<cryptobyte.String>(out var Ꮡidentities);
            if (!extData.ReadUint16LengthPrefixed(Ꮡidentities) || identities.Empty()) {
                return false;
            }
            while (!identities.Empty()) {
                ref var psk = ref heap(new pskIdentity(), out var Ꮡpsk);
                if (!readUint16LengthPrefixed(Ꮡidentities, Ꮡpsk.of(pskIdentity.Ꮡlabel)) || !identities.ReadUint32(Ꮡpsk.of(pskIdentity.ᏑobfuscatedTicketAge)) || len(psk.label) == 0) {
                    return false;
                }
                m.pskIdentities = append(m.pskIdentities, psk);
            }
            ref var binders = ref heap<cryptobyte.String>(out var Ꮡbinders);
            if (!extData.ReadUint16LengthPrefixed(Ꮡbinders) || binders.Empty()) {
                return false;
            }
            while (!binders.Empty()) {
                ref var binder = ref heap<slice<byte>>(out var Ꮡbinder);
                if (!readUint8LengthPrefixed(Ꮡbinders, Ꮡbinder) || len(binder) == 0) {
                    return false;
                }
                m.pskBinders = append(m.pskBinders, binder);
            }
            break;
        }
        case extensionEncryptedClientHello: {
            if (!extData.ReadBytes(Ꮡm.of(clientHelloMsg.ᏑencryptedClientHello), len(extData))) {
                return false;
            }
            break;
        }
        default: {
            continue;
            break;
        }}

        // Ignore unknown extensions.
        if (!extData.Empty()) {
            return false;
        }
    }
    return true;
}

internal static slice<byte> originalBytes(this ref clientHelloMsg m) {
    return m.original;
}

internal static ж<clientHelloMsg> clone(this ref clientHelloMsg m) {
    return Ꮡ(new clientHelloMsg(
        original: slices.Clone<slice<byte>, byte>(m.original),
        vers: m.vers,
        random: slices.Clone<slice<byte>, byte>(m.random),
        sessionId: slices.Clone<slice<byte>, byte>(m.sessionId),
        cipherSuites: slices.Clone<slice<uint16>, uint16>(m.cipherSuites),
        compressionMethods: slices.Clone<slice<uint8>, uint8>(m.compressionMethods),
        serverName: m.serverName,
        ocspStapling: m.ocspStapling,
        supportedCurves: slices.Clone<slice<CurveID>, CurveID>(m.supportedCurves),
        supportedPoints: slices.Clone<slice<uint8>, uint8>(m.supportedPoints),
        ticketSupported: m.ticketSupported,
        sessionTicket: slices.Clone<slice<uint8>, uint8>(m.sessionTicket),
        supportedSignatureAlgorithms: slices.Clone<slice<SignatureScheme>, SignatureScheme>(m.supportedSignatureAlgorithms),
        supportedSignatureAlgorithmsCert: slices.Clone<slice<SignatureScheme>, SignatureScheme>(m.supportedSignatureAlgorithmsCert),
        secureRenegotiationSupported: m.secureRenegotiationSupported,
        secureRenegotiation: slices.Clone<slice<byte>, byte>(m.secureRenegotiation),
        extendedMasterSecret: m.extendedMasterSecret,
        alpnProtocols: slices.Clone<slice<@string>, @string>(m.alpnProtocols),
        scts: m.scts,
        supportedVersions: slices.Clone<slice<uint16>, uint16>(m.supportedVersions),
        cookie: slices.Clone<slice<byte>, byte>(m.cookie),
        keyShares: slices.Clone<slice<keyShare>, keyShare>(m.keyShares),
        earlyData: m.earlyData,
        pskModes: slices.Clone<slice<uint8>, uint8>(m.pskModes),
        pskIdentities: slices.Clone<slice<pskIdentity>, pskIdentity>(m.pskIdentities),
        pskBinders: slices.Clone<slice<slice<byte>>, slice<byte>>(m.pskBinders),
        quicTransportParameters: slices.Clone<slice<byte>, byte>(m.quicTransportParameters),
        encryptedClientHello: slices.Clone<slice<byte>, byte>(m.encryptedClientHello)
    ));
}

partial struct serverHelloMsg {
    internal slice<byte> original;
    internal uint16 vers;
    internal slice<byte> random;
    internal slice<byte> sessionId;
    internal uint16 cipherSuite;
    internal uint8 compressionMethod;
    internal bool ocspStapling;
    internal bool ticketSupported;
    internal bool secureRenegotiationSupported;
    internal slice<byte> secureRenegotiation;
    internal bool extendedMasterSecret;
    internal @string alpnProtocol;
    internal slice<slice<byte>> scts;
    internal uint16 supportedVersion;
    internal keyShare serverShare;
    internal bool selectedIdentityPresent;
    internal uint16 selectedIdentity;
    internal slice<uint8> supportedPoints;
    internal slice<byte> encryptedClientHello;
    internal bool serverNameAck;
    // HelloRetryRequest extensions
    internal slice<byte> cookie;
    internal CurveID selectedGroup;
}

internal static (slice<byte>, error) marshal(this ж<serverHelloMsg> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    ref var exts = ref heap(new cryptobyte.Builder(), out var Ꮡexts);
    if (m.ocspStapling) {
        exts.AddUint16(extensionStatusRequest);
        exts.AddUint16(0); // empty extension_data
    }
    if (m.ticketSupported) {
        exts.AddUint16(extensionSessionTicket);
        exts.AddUint16(0); // empty extension_data
    }
    if (m.secureRenegotiationSupported) {
        exts.AddUint16(extensionRenegotiationInfo);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ1) => {
            extsΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ2) => {
                extsΔ2.AddBytes(Ꮡm.Value.secureRenegotiation);
            });
        });
    }
    if (m.extendedMasterSecret) {
        exts.AddUint16(extensionExtendedMasterSecret);
        exts.AddUint16(0); // empty extension_data
    }
    if (len(m.alpnProtocol) > 0) {
        exts.AddUint16(extensionALPN);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ3) => {
            extsΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ4) => {
                extsΔ4.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ5) => {
                    extsΔ5.AddBytes(slice<byte>(Ꮡm.Value.alpnProtocol));
                });
            });
        });
    }
    if (len(m.scts) > 0) {
        exts.AddUint16(extensionSCT);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ6) => {
            extsΔ6.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ7) => {
                foreach (var (_, sct) in Ꮡm.Value.scts) {
                    var sctʗ1 = sct;
                    extsΔ7.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ8) => {
                        extsΔ8.AddBytes(sctʗ1);
                    });
                }
            });
        });
    }
    if (m.supportedVersion != 0) {
        exts.AddUint16(extensionSupportedVersions);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ9) => {
            extsΔ9.AddUint16(Ꮡm.Value.supportedVersion);
        });
    }
    if (m.serverShare.group != 0) {
        exts.AddUint16(extensionKeyShare);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ10) => {
            extsΔ10.AddUint16((uint16)Ꮡm.Value.serverShare.group);
            extsΔ10.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ11) => {
                extsΔ11.AddBytes(Ꮡm.Value.serverShare.data);
            });
        });
    }
    if (m.selectedIdentityPresent) {
        exts.AddUint16(extensionPreSharedKey);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ12) => {
            extsΔ12.AddUint16(Ꮡm.Value.selectedIdentity);
        });
    }
    if (len(m.cookie) > 0) {
        exts.AddUint16(extensionCookie);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ13) => {
            extsΔ13.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ14) => {
                extsΔ14.AddBytes(Ꮡm.Value.cookie);
            });
        });
    }
    if (m.selectedGroup != 0) {
        exts.AddUint16(extensionKeyShare);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ15) => {
            extsΔ15.AddUint16((uint16)Ꮡm.Value.selectedGroup);
        });
    }
    if (len(m.supportedPoints) > 0) {
        exts.AddUint16(extensionSupportedPoints);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ16) => {
            extsΔ16.AddUint8LengthPrefixed((ж<cryptobyte.Builder> extsΔ17) => {
                extsΔ17.AddBytes(Ꮡm.Value.supportedPoints);
            });
        });
    }
    if (len(m.encryptedClientHello) > 0) {
        exts.AddUint16(extensionEncryptedClientHello);
        Ꮡexts.AddUint16LengthPrefixed((ж<cryptobyte.Builder> extsΔ18) => {
            extsΔ18.AddBytes(Ꮡm.Value.encryptedClientHello);
        });
    }
    if (m.serverNameAck) {
        exts.AddUint16(extensionServerName);
        exts.AddUint16(0);
    }
    var (extBytes, err) = exts.Bytes();
    if (err != default!) {
        return (default!, err);
    }
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeServerHello);
    var extBytesʗ1 = extBytes;
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddUint16(Ꮡm.Value.vers);
        addBytesWithLength(bΔ1, Ꮡm.Value.random, 32);
        bΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            bΔ2.AddBytes(Ꮡm.Value.sessionId);
        });
        bΔ1.AddUint16(Ꮡm.Value.cipherSuite);
        bΔ1.AddUint8(Ꮡm.Value.compressionMethod);
        if (len(extBytesʗ1) > 0) {
            var extBytesʗ2 = extBytesʗ1;
            bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
                bΔ3.AddBytes(extBytesʗ2);
            });
        }
    });
    return b.Bytes();
}

internal static bool unmarshal(this ж<serverHelloMsg> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    m = new serverHelloMsg(original: data);
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    if (!s.Skip(4) || !s.ReadUint16(Ꮡm.of(serverHelloMsg.Ꮡvers)) || !s.ReadBytes(Ꮡm.of(serverHelloMsg.Ꮡrandom), // message type and uint24 length field
 32) || !readUint8LengthPrefixed(Ꮡs, Ꮡm.of(serverHelloMsg.ᏑsessionId)) || !s.ReadUint16(Ꮡm.of(serverHelloMsg.ᏑcipherSuite)) || !s.ReadUint8(Ꮡm.of(serverHelloMsg.ᏑcompressionMethod))) {
        return false;
    }
    if (s.Empty()) {
        // ServerHello is optionally followed by extension data
        return true;
    }
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.ReadUint16LengthPrefixed(Ꮡextensions) || !s.Empty()) {
        return false;
    }
    var seenExts = new map<uint16, bool>();
    while (!extensions.Empty()) {
        ref var extension = ref heap(new uint16(), out var Ꮡextension);
        ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
        if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
            return false;
        }
        if (seenExts[extension]) {
            return false;
        }
        seenExts[extension] = true;
        switch (extension) {
        case extensionStatusRequest: {
            m.ocspStapling = true;
            break;
        }
        case extensionSessionTicket: {
            m.ticketSupported = true;
            break;
        }
        case extensionRenegotiationInfo: {
            if (!readUint8LengthPrefixed(ᏑextData, Ꮡm.of(serverHelloMsg.ᏑsecureRenegotiation))) {
                return false;
            }
            m.secureRenegotiationSupported = true;
            break;
        }
        case extensionExtendedMasterSecret: {
            m.extendedMasterSecret = true;
            break;
        }
        case extensionALPN: {
            ref var protoList = ref heap<cryptobyte.String>(out var ᏑprotoList);
            if (!extData.ReadUint16LengthPrefixed(ᏑprotoList) || protoList.Empty()) {
                return false;
            }
            ref var proto = ref heap<cryptobyte.String>(out var Ꮡproto);
            if (!protoList.ReadUint8LengthPrefixed(Ꮡproto) || proto.Empty() || !protoList.Empty()) {
                return false;
            }
            m.alpnProtocol = ((@string)(slice<byte>)proto);
            break;
        }
        case extensionSCT: {
            ref var sctList = ref heap<cryptobyte.String>(out var ᏑsctList);
            if (!extData.ReadUint16LengthPrefixed(ᏑsctList) || sctList.Empty()) {
                return false;
            }
            while (!sctList.Empty()) {
                ref var sct = ref heap<slice<byte>>(out var Ꮡsct);
                if (!readUint16LengthPrefixed(ᏑsctList, Ꮡsct) || len(sct) == 0) {
                    return false;
                }
                m.scts = append(m.scts, sct);
            }
            break;
        }
        case extensionSupportedVersions: {
            if (!extData.ReadUint16(Ꮡm.of(serverHelloMsg.ᏑsupportedVersion))) {
                return false;
            }
            break;
        }
        case extensionCookie: {
            if (!readUint16LengthPrefixed(ᏑextData, Ꮡm.of(serverHelloMsg.Ꮡcookie)) || len(m.cookie) == 0) {
                return false;
            }
            break;
        }
        case extensionKeyShare: {
            if (len(extData) == 2){
                // This extension has different formats in SH and HRR, accept either
                // and let the handshake logic decide. See RFC 8446, Section 4.2.8.
                if (!extData.ReadUint16(Ꮡm.of(serverHelloMsg.ᏑselectedGroup).Reinterpret<CurveID, uint16>())) {
                    return false;
                }
            } else {
                if (!extData.ReadUint16(Ꮡm.of(serverHelloMsg.ᏑserverShare).of(keyShare.Ꮡgroup).Reinterpret<CurveID, uint16>()) || !readUint16LengthPrefixed(ᏑextData, Ꮡm.of(serverHelloMsg.ᏑserverShare).of(keyShare.Ꮡdata))) {
                    return false;
                }
            }
            break;
        }
        case extensionPreSharedKey: {
            m.selectedIdentityPresent = true;
            if (!extData.ReadUint16(Ꮡm.of(serverHelloMsg.ᏑselectedIdentity))) {
                return false;
            }
            break;
        }
        case extensionSupportedPoints: {
            if (!readUint8LengthPrefixed(ᏑextData, // RFC 4492, Section 5.1.2
 Ꮡm.of(serverHelloMsg.ᏑsupportedPoints)) || len(m.supportedPoints) == 0) {
                return false;
            }
            break;
        }
        case extensionEncryptedClientHello: {
            m.encryptedClientHello = new slice<byte>(len(extData));
            if (!extData.CopyBytes(m.encryptedClientHello)) {
                // encrypted_client_hello
                return false;
            }
            break;
        }
        case extensionServerName: {
            if (len(extData) != 0) {
                return false;
            }
            m.serverNameAck = true;
            break;
        }
        default: {
            continue;
            break;
        }}

        // Ignore unknown extensions.
        if (!extData.Empty()) {
            return false;
        }
    }
    return true;
}

internal static slice<byte> originalBytes(this ref serverHelloMsg m) {
    return m.original;
}

partial struct encryptedExtensionsMsg {
    internal @string alpnProtocol;
    internal slice<byte> quicTransportParameters;
    internal bool earlyData;
    internal slice<byte> echRetryConfigs;
}

internal static (slice<byte>, error) marshal(this ж<encryptedExtensionsMsg> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeEncryptedExtensions);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            if (len(Ꮡm.Value.alpnProtocol) > 0) {
                bΔ2.AddUint16(extensionALPN);
                bΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
                    bΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
                        bΔ4.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                            bΔ5.AddBytes(slice<byte>(Ꮡm.Value.alpnProtocol));
                        });
                    });
                });
            }
            if (Ꮡm.Value.quicTransportParameters != default!) {
                // marshal zero-length parameters when present
                // draft-ietf-quic-tls-32, Section 8.2
                bΔ2.AddUint16(extensionQUICTransportParameters);
                bΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ6) => {
                    bΔ6.AddBytes(Ꮡm.Value.quicTransportParameters);
                });
            }
            if (Ꮡm.Value.earlyData) {
                // RFC 8446, Section 4.2.10
                bΔ2.AddUint16(extensionEarlyData);
                bΔ2.AddUint16(0); // empty extension_data
            }
            if (len(Ꮡm.Value.echRetryConfigs) > 0) {
                bΔ2.AddUint16(extensionEncryptedClientHello);
                bΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ7) => {
                    bΔ7.AddBytes(Ꮡm.Value.echRetryConfigs);
                });
            }
        });
    });
    return b.Bytes();
}

internal static bool unmarshal(this ref encryptedExtensionsMsg m, slice<byte> data) {
    m = new encryptedExtensionsMsg(nil);
    var s = ((cryptobyte.String)data);
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.Skip(4) || !s.ReadUint16LengthPrefixed(Ꮡextensions) || !s.Empty()) {
        // message type and uint24 length field
        return false;
    }
    while (!extensions.Empty()) {
        ref var extension = ref heap(new uint16(), out var Ꮡextension);
        ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
        if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
            return false;
        }
        switch (extension) {
        case extensionALPN: {
            ref var protoList = ref heap<cryptobyte.String>(out var ᏑprotoList);
            if (!extData.ReadUint16LengthPrefixed(ᏑprotoList) || protoList.Empty()) {
                return false;
            }
            ref var proto = ref heap<cryptobyte.String>(out var Ꮡproto);
            if (!protoList.ReadUint8LengthPrefixed(Ꮡproto) || proto.Empty() || !protoList.Empty()) {
                return false;
            }
            m.alpnProtocol = ((@string)(slice<byte>)proto);
            break;
        }
        case extensionQUICTransportParameters: {
            m.quicTransportParameters = new slice<byte>(len(extData));
            if (!extData.CopyBytes(m.quicTransportParameters)) {
                return false;
            }
            break;
        }
        case extensionEarlyData: {
            m.earlyData = true;
            break;
        }
        case extensionEncryptedClientHello: {
            m.echRetryConfigs = new slice<byte>(len(extData));
            if (!extData.CopyBytes(m.echRetryConfigs)) {
                // RFC 8446, Section 4.2.10
                return false;
            }
            break;
        }
        default: {
            continue;
            break;
        }}

        // Ignore unknown extensions.
        if (!extData.Empty()) {
            return false;
        }
    }
    return true;
}

partial struct endOfEarlyDataMsg {
}

internal static (slice<byte>, error) marshal(this ref endOfEarlyDataMsg m) {
    var x = new slice<byte>(4);
    x[0] = typeEndOfEarlyData;
    return (x, default!);
}

internal static bool unmarshal(this ref endOfEarlyDataMsg m, slice<byte> data) {
    return len(data) == 4;
}

partial struct keyUpdateMsg {
    internal bool updateRequested;
}

internal static (slice<byte>, error) marshal(this ж<keyUpdateMsg> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeKeyUpdate);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        if (Ꮡm.Value.updateRequested){
            bΔ1.AddUint8(1);
        } else {
            bΔ1.AddUint8(0);
        }
    });
    return b.Bytes();
}

internal static bool unmarshal(this ref keyUpdateMsg m, slice<byte> data) {
    var s = ((cryptobyte.String)data);
    ref var updateRequested = ref heap(new uint8(), out var ᏑupdateRequested);
    if (!s.Skip(4) || !s.ReadUint8(ᏑupdateRequested) || !s.Empty()) {
        // message type and uint24 length field
        return false;
    }
    switch (updateRequested) {
    case 0: {
        m.updateRequested = false;
        break;
    }
    case 1: {
        m.updateRequested = true;
        break;
    }
    default: {
        return false;
    }}

    return true;
}

partial struct newSessionTicketMsgTLS13 {
    internal uint32 lifetime;
    internal uint32 ageAdd;
    internal slice<byte> nonce;
    internal slice<byte> label;
    internal uint32 maxEarlyData;
}

internal static (slice<byte>, error) marshal(this ж<newSessionTicketMsgTLS13> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeNewSessionTicket);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddUint32(Ꮡm.Value.lifetime);
        bΔ1.AddUint32(Ꮡm.Value.ageAdd);
        bΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            bΔ2.AddBytes(Ꮡm.Value.nonce);
        });
        bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
            bΔ3.AddBytes(Ꮡm.Value.label);
        });
        bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
            if (Ꮡm.Value.maxEarlyData > 0) {
                bΔ4.AddUint16(extensionEarlyData);
                bΔ4.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                    bΔ5.AddUint32(Ꮡm.Value.maxEarlyData);
                });
            }
        });
    });
    return b.Bytes();
}

internal static bool unmarshal(this ж<newSessionTicketMsgTLS13> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    m = new newSessionTicketMsgTLS13(nil);
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.Skip(4) || !s.ReadUint32(Ꮡm.of(newSessionTicketMsgTLS13.Ꮡlifetime)) || !s.ReadUint32(Ꮡm.of(newSessionTicketMsgTLS13.ᏑageAdd)) || !readUint8LengthPrefixed(Ꮡs, // message type and uint24 length field
 Ꮡm.of(newSessionTicketMsgTLS13.Ꮡnonce)) || !readUint16LengthPrefixed(Ꮡs, Ꮡm.of(newSessionTicketMsgTLS13.Ꮡlabel)) || !s.ReadUint16LengthPrefixed(Ꮡextensions) || !s.Empty()) {
        return false;
    }
    while (!extensions.Empty()) {
        ref var extension = ref heap(new uint16(), out var Ꮡextension);
        ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
        if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
            return false;
        }
        switch (extension) {
        case extensionEarlyData: {
            if (!extData.ReadUint32(Ꮡm.of(newSessionTicketMsgTLS13.ᏑmaxEarlyData))) {
                return false;
            }
            break;
        }
        default: {
            continue;
            break;
        }}

        // Ignore unknown extensions.
        if (!extData.Empty()) {
            return false;
        }
    }
    return true;
}

partial struct certificateRequestMsgTLS13 {
    internal bool ocspStapling;
    internal bool scts;
    internal slice<SignatureScheme> supportedSignatureAlgorithms;
    internal slice<SignatureScheme> supportedSignatureAlgorithmsCert;
    internal slice<slice<byte>> certificateAuthorities;
}

internal static (slice<byte>, error) marshal(this ж<certificateRequestMsgTLS13> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeCertificateRequest);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        // certificate_request_context (SHALL be zero length unless used for
        // post-handshake authentication)
        bΔ1.AddUint8(0);
        bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            if (Ꮡm.Value.ocspStapling) {
                bΔ2.AddUint16(extensionStatusRequest);
                bΔ2.AddUint16(0); // empty extension_data
            }
            if (Ꮡm.Value.scts) {
                // RFC 8446, Section 4.4.2.1 makes no mention of
                // signed_certificate_timestamp in CertificateRequest, but
                // "Extensions in the Certificate message from the client MUST
                // correspond to extensions in the CertificateRequest message
                // from the server." and it appears in the table in Section 4.2.
                bΔ2.AddUint16(extensionSCT);
                bΔ2.AddUint16(0); // empty extension_data
            }
            if (len(Ꮡm.Value.supportedSignatureAlgorithms) > 0) {
                bΔ2.AddUint16(extensionSignatureAlgorithms);
                bΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
                    bΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
                        foreach (var (_, sigAlgo) in Ꮡm.Value.supportedSignatureAlgorithms) {
                            bΔ4.AddUint16((uint16)sigAlgo);
                        }
                    });
                });
            }
            if (len(Ꮡm.Value.supportedSignatureAlgorithmsCert) > 0) {
                bΔ2.AddUint16(extensionSignatureAlgorithmsCert);
                bΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                    bΔ5.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ6) => {
                        foreach (var (_, sigAlgo) in Ꮡm.Value.supportedSignatureAlgorithmsCert) {
                            bΔ6.AddUint16((uint16)sigAlgo);
                        }
                    });
                });
            }
            if (len(Ꮡm.Value.certificateAuthorities) > 0) {
                bΔ2.AddUint16(extensionCertificateAuthorities);
                bΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ7) => {
                    bΔ7.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ8) => {
                        foreach (var (_, ca) in Ꮡm.Value.certificateAuthorities) {
                            var caʗ1 = ca;
                            bΔ8.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ9) => {
                                bΔ9.AddBytes(caʗ1);
                            });
                        }
                    });
                });
            }
        });
    });
    return b.Bytes();
}

internal static bool unmarshal(this ref certificateRequestMsgTLS13 m, slice<byte> data) {
    m = new certificateRequestMsgTLS13(nil);
    var s = ((cryptobyte.String)data);
    ref var context = ref heap<cryptobyte.String>(out var Ꮡcontext);
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    if (!s.Skip(4) || !s.ReadUint8LengthPrefixed(Ꮡcontext) || !context.Empty() || !s.ReadUint16LengthPrefixed(Ꮡextensions) || !s.Empty()) {
        // message type and uint24 length field
        return false;
    }
    while (!extensions.Empty()) {
        ref var extension = ref heap(new uint16(), out var Ꮡextension);
        ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
        if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
            return false;
        }
        switch (extension) {
        case extensionStatusRequest: {
            m.ocspStapling = true;
            break;
        }
        case extensionSCT: {
            m.scts = true;
            break;
        }
        case extensionSignatureAlgorithms: {
            ref var sigAndAlgs = ref heap<cryptobyte.String>(out var ᏑsigAndAlgs);
            if (!extData.ReadUint16LengthPrefixed(ᏑsigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                ref var sigAndAlg = ref heap(new uint16(), out var ᏑsigAndAlg);
                if (!sigAndAlgs.ReadUint16(ᏑsigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithms = append(
                    m.supportedSignatureAlgorithms, ((SignatureScheme)sigAndAlg));
            }
            break;
        }
        case extensionSignatureAlgorithmsCert: {
            ref var sigAndAlgs = ref heap<cryptobyte.String>(out var ᏑsigAndAlgs);
            if (!extData.ReadUint16LengthPrefixed(ᏑsigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                ref var sigAndAlg = ref heap(new uint16(), out var ᏑsigAndAlg);
                if (!sigAndAlgs.ReadUint16(ᏑsigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithmsCert = append(
                    m.supportedSignatureAlgorithmsCert, ((SignatureScheme)sigAndAlg));
            }
            break;
        }
        case extensionCertificateAuthorities: {
            ref var auths = ref heap<cryptobyte.String>(out var Ꮡauths);
            if (!extData.ReadUint16LengthPrefixed(Ꮡauths) || auths.Empty()) {
                return false;
            }
            while (!auths.Empty()) {
                ref var ca = ref heap<slice<byte>>(out var Ꮡca);
                if (!readUint16LengthPrefixed(Ꮡauths, Ꮡca) || len(ca) == 0) {
                    return false;
                }
                m.certificateAuthorities = append(m.certificateAuthorities, ca);
            }
            break;
        }
        default: {
            continue;
            break;
        }}

        // Ignore unknown extensions.
        if (!extData.Empty()) {
            return false;
        }
    }
    return true;
}

partial struct certificateMsg {
    internal slice<slice<byte>> certificates;
}

internal static (slice<byte>, error) marshal(this ref certificateMsg m) {
    nint i = default!;
    foreach (var (_, Δslice) in m.certificates) {
        i += len(Δslice);
    }
    nint length = 3 + 3 * len(m.certificates) + i;
    var x = new slice<byte>(4 + length);
    x[0] = typeCertificate;
    x[1] = (uint8)((length >> (int)(16)));
    x[2] = (uint8)((length >> (int)(8)));
    x[3] = (uint8)length;
    nint certificateOctets = length - 3;
    x[4] = (uint8)((certificateOctets >> (int)(16)));
    x[5] = (uint8)((certificateOctets >> (int)(8)));
    x[6] = (uint8)certificateOctets;
    var y = x[7..];
    foreach (var (_, Δslice) in m.certificates) {
        y[0] = (uint8)((len(Δslice) >> (int)(16)));
        y[1] = (uint8)((len(Δslice) >> (int)(8)));
        y[2] = (uint8)len(Δslice);
        copy(y[3..], Δslice);
        y = y.slice(3 + len(Δslice));
    }
    return (x, default!);
}

internal static bool unmarshal(this ref certificateMsg m, slice<byte> data) {
    if (len(data) < 7) {
        return false;
    }
    var certsLen = (uint32)((uint32)(((uint32)data[4] << (int)(16)) | ((uint32)data[5] << (int)(8))) | (uint32)data[6]);
    if ((uint32)len(data) != certsLen + 7) {
        return false;
    }
    nint numCerts = 0;
    var d = data[7..];
    while (certsLen > 0) {
        if (len(d) < 4) {
            return false;
        }
        var certLen = (uint32)((uint32)(((uint32)d[0] << (int)(16)) | ((uint32)d[1] << (int)(8))) | (uint32)d[2]);
        if ((uint32)len(d) < 3 + certLen) {
            return false;
        }
        d = d.slice((nint)(3 + certLen));
        certsLen -= 3 + certLen;
        numCerts++;
    }
    m.certificates = new slice<slice<byte>>(numCerts);
    d = data[7..];
    for (nint i = 0; i < numCerts; i++) {
        var certLen = (uint32)((uint32)(((uint32)d[0] << (int)(16)) | ((uint32)d[1] << (int)(8))) | (uint32)d[2]);
        m.certificates[i] = d.slice(3, (nint)(3 + certLen));
        d = d.slice((nint)(3 + certLen));
    }
    return true;
}

partial struct certificateMsgTLS13 {
    internal Certificate certificate;
    internal bool ocspStapling;
    internal bool scts;
}

internal static (slice<byte>, error) marshal(this ж<certificateMsgTLS13> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeCertificate);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddUint8(0); // certificate_request_context
        var certificate = Ꮡm.Value.certificate;
        if (!Ꮡm.Value.ocspStapling) {
            certificate.OCSPStaple = default!;
        }
        if (!Ꮡm.Value.scts) {
            certificate.SignedCertificateTimestamps = default!;
        }
        marshalCertificate(bΔ1, certificate);
    });
    return b.Bytes();
}

internal static void marshalCertificate(ж<cryptobyte.Builder> Ꮡb, Certificate certificate) {
    var certificateʗ1 = certificate;
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        foreach (var (i, cert) in certificateʗ1.ΔCertificate) {
            var certʗ1 = cert;
            bΔ1.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
                bΔ2.AddBytes(certʗ1);
            });
            var certificateʗ2 = certificateʗ1;
            bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
                if (i > 0) {
                    // This library only supports OCSP and SCT for leaf certificates.
                    return;
                }
                if (certificateʗ2.OCSPStaple != default!) {
                    bΔ3.AddUint16(extensionStatusRequest);
                    var certificateʗ3 = certificateʗ2;
                    bΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
                        bΔ4.AddUint8(statusTypeOCSP);
                        var certificateʗ4 = certificateʗ3;
                        bΔ4.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                            bΔ5.AddBytes(certificateʗ4.OCSPStaple);
                        });
                    });
                }
                if (certificateʗ2.SignedCertificateTimestamps != default!) {
                    bΔ3.AddUint16(extensionSCT);
                    var certificateʗ5 = certificateʗ2;
                    bΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ6) => {
                        var certificateʗ6 = certificateʗ5;
                        bΔ6.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ7) => {
                            foreach (var (_, sct) in certificateʗ6.SignedCertificateTimestamps) {
                                var sctʗ1 = sct;
                                bΔ7.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ8) => {
                                    bΔ8.AddBytes(sctʗ1);
                                });
                            }
                        });
                    });
                }
            });
        }
    });
}

internal static bool unmarshal(this ж<certificateMsgTLS13> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    m = new certificateMsgTLS13(nil);
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    ref var context = ref heap<cryptobyte.String>(out var Ꮡcontext);
    if (!s.Skip(4) || !s.ReadUint8LengthPrefixed(Ꮡcontext) || !context.Empty() || !unmarshalCertificate(Ꮡs, // message type and uint24 length field
 Ꮡm.of(certificateMsgTLS13.Ꮡcertificate)) || !s.Empty()) {
        return false;
    }
    m.scts = m.certificate.SignedCertificateTimestamps != default!;
    m.ocspStapling = m.certificate.OCSPStaple != default!;
    return true;
}

internal static bool unmarshalCertificate(ж<cryptobyte.String> Ꮡs, ж<Certificate> Ꮡcertificate) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var certificate = ref Ꮡcertificate.DerefOrNull();

    ref var certList = ref heap<cryptobyte.String>(out var ᏑcertList);
    if (!s.ReadUint24LengthPrefixed(ᏑcertList)) {
        return false;
    }
    while (!certList.Empty()) {
        ref var cert = ref heap<slice<byte>>(out var Ꮡcert);
        ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
        if (!readUint24LengthPrefixed(ᏑcertList, Ꮡcert) || !certList.ReadUint16LengthPrefixed(Ꮡextensions)) {
            return false;
        }
        certificate.ΔCertificate = append(certificate.ΔCertificate, cert);
        while (!extensions.Empty()) {
            ref var extension = ref heap(new uint16(), out var Ꮡextension);
            ref var extData = ref heap<cryptobyte.String>(out var ᏑextData);
            if (!extensions.ReadUint16(Ꮡextension) || !extensions.ReadUint16LengthPrefixed(ᏑextData)) {
                return false;
            }
            if (len(certificate.ΔCertificate) > 1) {
                // This library only supports OCSP and SCT for leaf certificates.
                continue;
            }
            switch (extension) {
            case extensionStatusRequest: {
                ref var statusType = ref heap(new uint8(), out var ᏑstatusType);
                if (!extData.ReadUint8(ᏑstatusType) || statusType != statusTypeOCSP || !readUint24LengthPrefixed(ᏑextData, Ꮡcertificate.of(Certificate.ᏑOCSPStaple)) || len(certificate.OCSPStaple) == 0) {
                    return false;
                }
                break;
            }
            case extensionSCT: {
                ref var sctList = ref heap<cryptobyte.String>(out var ᏑsctList);
                if (!extData.ReadUint16LengthPrefixed(ᏑsctList) || sctList.Empty()) {
                    return false;
                }
                while (!sctList.Empty()) {
                    ref var sct = ref heap<slice<byte>>(out var Ꮡsct);
                    if (!readUint16LengthPrefixed(ᏑsctList, Ꮡsct) || len(sct) == 0) {
                        return false;
                    }
                    certificate.SignedCertificateTimestamps = append(
                        certificate.SignedCertificateTimestamps, sct);
                }
                break;
            }
            default: {
                continue;
                break;
            }}

            // Ignore unknown extensions.
            if (!extData.Empty()) {
                return false;
            }
        }
    }
    return true;
}

partial struct serverKeyExchangeMsg {
    internal slice<byte> key;
}

internal static (slice<byte>, error) marshal(this ref serverKeyExchangeMsg m) {
    nint length = len(m.key);
    var x = new slice<byte>(length + 4);
    x[0] = typeServerKeyExchange;
    x[1] = (uint8)((length >> (int)(16)));
    x[2] = (uint8)((length >> (int)(8)));
    x[3] = (uint8)length;
    copy(x[4..], m.key);
    return (x, default!);
}

internal static bool unmarshal(this ref serverKeyExchangeMsg m, slice<byte> data) {
    if (len(data) < 4) {
        return false;
    }
    m.key = data[4..];
    return true;
}

partial struct certificateStatusMsg {
    internal slice<byte> response;
}

internal static (slice<byte>, error) marshal(this ж<certificateStatusMsg> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeCertificateStatus);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddUint8(statusTypeOCSP);
        bΔ1.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            bΔ2.AddBytes(Ꮡm.Value.response);
        });
    });
    return b.Bytes();
}

internal static bool unmarshal(this ж<certificateStatusMsg> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    ref var statusType = ref heap(new uint8(), out var ᏑstatusType);
    if (!s.Skip(4) || !s.ReadUint8(ᏑstatusType) || statusType != statusTypeOCSP || !readUint24LengthPrefixed(Ꮡs, // message type and uint24 length field
 Ꮡm.of(certificateStatusMsg.Ꮡresponse)) || len(m.response) == 0 || !s.Empty()) {
        return false;
    }
    return true;
}

partial struct serverHelloDoneMsg {
}

internal static (slice<byte>, error) marshal(this ref serverHelloDoneMsg m) {
    var x = new slice<byte>(4);
    x[0] = typeServerHelloDone;
    return (x, default!);
}

internal static bool unmarshal(this ref serverHelloDoneMsg m, slice<byte> data) {
    return len(data) == 4;
}

partial struct clientKeyExchangeMsg {
    internal slice<byte> ciphertext;
}

internal static (slice<byte>, error) marshal(this ref clientKeyExchangeMsg m) {
    nint length = len(m.ciphertext);
    var x = new slice<byte>(length + 4);
    x[0] = typeClientKeyExchange;
    x[1] = (uint8)((length >> (int)(16)));
    x[2] = (uint8)((length >> (int)(8)));
    x[3] = (uint8)length;
    copy(x[4..], m.ciphertext);
    return (x, default!);
}

internal static bool unmarshal(this ref clientKeyExchangeMsg m, slice<byte> data) {
    if (len(data) < 4) {
        return false;
    }
    nint l = (nint)((nint)(((nint)data[1] << (int)(16)) | ((nint)data[2] << (int)(8))) | (nint)data[3]);
    if (l != len(data) - 4) {
        return false;
    }
    m.ciphertext = data[4..];
    return true;
}

partial struct finishedMsg {
    internal slice<byte> verifyData;
}

internal static (slice<byte>, error) marshal(this ж<finishedMsg> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeFinished);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddBytes(Ꮡm.Value.verifyData);
    });
    return b.Bytes();
}

internal static bool unmarshal(this ж<finishedMsg> Ꮡm, slice<byte> data) {
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    return s.Skip(1) && readUint24LengthPrefixed(Ꮡs, Ꮡm.of(finishedMsg.ᏑverifyData)) && s.Empty();
}

partial struct certificateRequestMsg {
    // hasSignatureAlgorithm indicates whether this message includes a list of
    // supported signature algorithms. This change was introduced with TLS 1.2.
    internal bool hasSignatureAlgorithm;
    internal slice<byte> certificateTypes;
    internal slice<SignatureScheme> supportedSignatureAlgorithms;
    internal slice<slice<byte>> certificateAuthorities;
}

internal static (slice<byte>, error) marshal(this ref certificateRequestMsg m) {
    // See RFC 4346, Section 7.4.4.
    nint length = 1 + len(m.certificateTypes) + 2;
    nint casLength = 0;
    foreach (var (_, ca) in m.certificateAuthorities) {
        casLength += 2 + len(ca);
    }
    length += casLength;
    if (m.hasSignatureAlgorithm) {
        length += 2 + 2 * len(m.supportedSignatureAlgorithms);
    }
    var x = new slice<byte>(4 + length);
    x[0] = typeCertificateRequest;
    x[1] = (uint8)((length >> (int)(16)));
    x[2] = (uint8)((length >> (int)(8)));
    x[3] = (uint8)length;
    x[4] = (uint8)len(m.certificateTypes);
    copy(x[5..], m.certificateTypes);
    var y = x.slice(5 + len(m.certificateTypes));
    if (m.hasSignatureAlgorithm) {
        nint n = len(m.supportedSignatureAlgorithms) * 2;
        y[0] = (uint8)((n >> (int)(8)));
        y[1] = (uint8)n;
        y = y[2..];
        foreach (var (_, sigAlgo) in m.supportedSignatureAlgorithms) {
            y[0] = (uint8)(uint16)((sigAlgo >> (int)(8)));
            y[1] = (uint8)(uint16)sigAlgo;
            y = y[2..];
        }
    }
    y[0] = (uint8)((casLength >> (int)(8)));
    y[1] = (uint8)casLength;
    y = y[2..];
    foreach (var (_, ca) in m.certificateAuthorities) {
        y[0] = (uint8)((len(ca) >> (int)(8)));
        y[1] = (uint8)len(ca);
        y = y[2..];
        copy(y, ca);
        y = y.slice(len(ca));
    }
    return (x, default!);
}

internal static bool unmarshal(this ref certificateRequestMsg m, slice<byte> data) {
    if (len(data) < 5) {
        return false;
    }
    var length = (uint32)((uint32)(((uint32)data[1] << (int)(16)) | ((uint32)data[2] << (int)(8))) | (uint32)data[3]);
    if ((uint32)len(data) - 4 != length) {
        return false;
    }
    nint numCertTypes = (nint)data[4];
    data = data[5..];
    if (numCertTypes == 0 || len(data) <= numCertTypes) {
        return false;
    }
    m.certificateTypes = new slice<byte>(numCertTypes);
    if (copy(m.certificateTypes, data) != numCertTypes) {
        return false;
    }
    data = data.slice(numCertTypes);
    if (m.hasSignatureAlgorithm) {
        if (len(data) < 2) {
            return false;
        }
        var sigAndHashLen = (uint16)((uint16)((uint16)data[0] << (int)(8)) | (uint16)data[1]);
        data = data[2..];
        if ((uint16)(sigAndHashLen & 1) != 0) {
            return false;
        }
        if (len(data) < (nint)sigAndHashLen) {
            return false;
        }
        var numSigAlgos = (uint16)(sigAndHashLen / 2);
        m.supportedSignatureAlgorithms = new slice<SignatureScheme>(numSigAlgos);
        foreach (var (i, _) in m.supportedSignatureAlgorithms) {
            m.supportedSignatureAlgorithms[i] = (SignatureScheme)((SignatureScheme)(uint16)(((SignatureScheme)(uint16)data[0]) << (int)(8)) | ((SignatureScheme)(uint16)data[1]));
            data = data[2..];
        }
    }
    if (len(data) < 2) {
        return false;
    }
    var casLength = (uint16)((uint16)((uint16)data[0] << (int)(8)) | (uint16)data[1]);
    data = data[2..];
    if (len(data) < (nint)casLength) {
        return false;
    }
    var cas = new slice<byte>(casLength);
    copy(cas, data);
    data = data.slice(casLength);
    m.certificateAuthorities = default!;
    while (len(cas) > 0) {
        if (len(cas) < 2) {
            return false;
        }
        var caLen = (uint16)((uint16)((uint16)cas[0] << (int)(8)) | (uint16)cas[1]);
        cas = cas[2..];
        if (len(cas) < (nint)caLen) {
            return false;
        }
        m.certificateAuthorities = append(m.certificateAuthorities, cas.slice(0, caLen));
        cas = cas.slice(caLen);
    }
    return len(data) == 0;
}

partial struct certificateVerifyMsg {
    internal bool hasSignatureAlgorithm; // format change introduced in TLS 1.2
    internal SignatureScheme signatureAlgorithm;
    internal slice<byte> signature;
}

internal static (slice<byte>, error) marshal(this ж<certificateVerifyMsg> Ꮡm) {
    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint8(typeCertificateVerify);
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        if (Ꮡm.Value.hasSignatureAlgorithm) {
            bΔ1.AddUint16((uint16)Ꮡm.Value.signatureAlgorithm);
        }
        bΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            bΔ2.AddBytes(Ꮡm.Value.signature);
        });
    });
    return b.Bytes();
}

internal static bool unmarshal(this ж<certificateVerifyMsg> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    if (!s.Skip(4)) {
        // message type and uint24 length field
        return false;
    }
    if (m.hasSignatureAlgorithm) {
        if (!s.ReadUint16(Ꮡm.of(certificateVerifyMsg.ᏑsignatureAlgorithm).Reinterpret<SignatureScheme, uint16>())) {
            return false;
        }
    }
    return readUint16LengthPrefixed(Ꮡs, Ꮡm.of(certificateVerifyMsg.Ꮡsignature)) && s.Empty();
}

partial struct newSessionTicketMsg {
    internal slice<byte> ticket;
}

internal static (slice<byte>, error) marshal(this ref newSessionTicketMsg m) {
    // See RFC 5077, Section 3.3.
    nint ticketLen = len(m.ticket);
    nint length = 2 + 4 + ticketLen;
    var x = new slice<byte>(4 + length);
    x[0] = typeNewSessionTicket;
    x[1] = (uint8)((length >> (int)(16)));
    x[2] = (uint8)((length >> (int)(8)));
    x[3] = (uint8)length;
    x[8] = (uint8)((ticketLen >> (int)(8)));
    x[9] = (uint8)ticketLen;
    copy(x[10..], m.ticket);
    return (x, default!);
}

internal static bool unmarshal(this ref newSessionTicketMsg m, slice<byte> data) {
    if (len(data) < 10) {
        return false;
    }
    var length = (uint32)((uint32)(((uint32)data[1] << (int)(16)) | ((uint32)data[2] << (int)(8))) | (uint32)data[3]);
    if ((uint32)len(data) - 4 != length) {
        return false;
    }
    nint ticketLen = ((nint)data[8] << (int)(8)) + (nint)data[9];
    if (len(data) - 10 != ticketLen) {
        return false;
    }
    m.ticket = data[10..];
    return true;
}

partial struct helloRequestMsg {
}

internal static (slice<byte>, error) marshal(this ref helloRequestMsg _) {
    return (new byte[]{typeHelloRequest, 0, 0, 0}.slice(), default!);
}

internal static bool unmarshal(this ref helloRequestMsg _, slice<byte> data) {
    return len(data) == 4;
}

partial interface transcriptHash {
    (nint, error) Write(slice<byte> _);
}

// transcriptMsg is a helper used to hash messages which are not hashed when
// they are read from, or written to, the wire. This is typically the case for
// messages which are either not sent, or need to be hashed out of order from
// when they are read/written.
//
// For most messages, the message is marshalled using their marshal method,
// since their wire representation is idempotent. For clientHelloMsg and
// serverHelloMsg, we store the original wire representation of the message and
// use that for hashing, since unmarshal/marshal are not idempotent due to
// extension ordering and other malleable fields, which may cause differences
// between what was received and what we marshal.
internal static error transcriptMsg(handshakeMessage msg, transcriptHash h) {
    {
        var (msgWithOrig, ok) = msg._<handshakeMessageWithOriginalBytes>(ᐧ); if (ok) {
            {
                var orig = msgWithOrig.originalBytes(); if (orig != default!) {
                    h.Write(msgWithOrig.originalBytes());
                    return default!;
                }
            }
        }
    }
    var (data, err) = msg.marshal();
    if (err != default!) {
        return err;
    }
    h.Write(data);
    return default!;
}

} // end tls_package
