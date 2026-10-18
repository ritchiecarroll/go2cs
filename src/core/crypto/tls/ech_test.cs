// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using hex = encoding.hex_package;
using testing = testing_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using encoding;
using hpke = go.crypto.hpke_package;
using static go.crypto.tls_package;
using vendor.golang.org.x.crypto;
using ꓸꓸꓸuint16 = Span<uint16>;

partial class tls_internal_test_package {

internal partial struct TestDecodeECHConfigLists_type /*dyn*/ {
    internal @string list;
    internal nint numConfigs;
}

public static void TestDecodeECHConfigLists(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in new TestDecodeECHConfigLists_type[]{
        new("0045fe0d0041590020002092a01233db2218518ccbbbbc24df20686af417b37388de6460e94011974777090004000100010012636c6f7564666c6172652d6563682e636f6d0000"u8, 1),
        new("0105badd00050504030201fe0d0066000010004104e62b69e2bf659f97be2f1e0d948a4cd5976bb7a91e0d46fbdda9a91e9ddcba5a01e7d697a80a18f9c3c4a31e56e27c8348db161a1cf51d7ef1942d4bcf7222c1000c000100010001000200010003400e7075626c69632e6578616d706c650000fe0d003d00002000207d661615730214aeee70533366f36a609ead65c0c208e62322346ab5bcd8de1c000411112222400e7075626c69632e6578616d706c650000fe0d004d000020002085bd6a03277c25427b52e269e0c77a8eb524ba1eb3d2f132662d4b0ac6cb7357000c000100010001000200010003400e7075626c69632e6578616d706c650008aaaa000474657374"u8, 3)
    }.slice()) {
        var (b, err) = hex.DecodeString(tc.list);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var configs, err) = parseECHConfigList(b);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(configs) != tc.numConfigs) {
            Ꮡt.Fatalf("unexpected number of configs parsed: got %d want %d"u8, len(configs), tc.numConfigs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object pickECHConfigPickedAnˢ = (@string)"pickECHConfig picked an invalid config"u8;

public static void TestSkipBadConfigs(ж<testing.T> Ꮡt) {
    var (b, err) = hex.DecodeString("00c8badd00050504030201fe0d0029006666000401020304000c000100010001000200010003400e7075626c69632e6578616d706c650000fe0d003d000020002072e8a23b7aef67832bcc89d652e3870a60f88ca684ec65d6eace6b61f136064c000411112222400e7075626c69632e6578616d706c650000fe0d004d00002000200ce95810a81d8023f41e83679bc92701b2acd46c75869f95c72bc61c6b12297c000c000100010001000200010003400e7075626c69632e6578616d706c650008aaaa000474657374"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var configs, err) = parseECHConfigList(b);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (config, _, _, _) = pickECHConfig(configs);
    if (config != nil) {
        Ꮡt.Fatal(pickECHConfigPickedAnˢ);
    }
}

internal partial struct TestDecodeInnerClientHelloOuterExtensions_type /*dyn*/ {
    internal @string name;
    internal Action<ж<cryptobyte.Builder>> buildExts;
    internal @string wantErr;
}

public static void TestDecodeInnerClientHelloOuterExtensions(ж<testing.T> Ꮡt) {
    var outer = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_AES_128_GCM_SHA256}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice(),
        ocspStapling: true,
        supportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
        encryptedClientHello: new byte[]{(byte)innerECHExt}.slice()
    ));
    outer.Value.original = mustMarshal(Ꮡt, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(outer));

    slice<byte> encodeInner(Action<ж<cryptobyte.Builder>> buildExts) {
        ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
        b.AddUint16(VersionTLS12);
        b.AddBytes(new slice<byte>(32));
        b.AddUint8(0);
        Ꮡb.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
            bΔ1.AddUint16(TLS_AES_128_GCM_SHA256);
        });
        Ꮡb.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
            bΔ2.AddUint8(compressionNone);
        });
        Ꮡb.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
            buildExts(bΔ3);
            bΔ3.AddUint16(extensionEncryptedClientHello);
            bΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
                bΔ4.AddUint8((uint8)innerECHExt);
            });
            bΔ3.AddUint16(extensionSupportedVersions);
            bΔ3.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                bΔ5.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ6) => {
                    bΔ6.AddUint16(VersionTLS13);
                });
            });
        });
        return b.BytesOrPanic();
    }

    void outerExts(ж<cryptobyte.Builder> b, params ꓸꓸꓸuint16 extTypesʗp) {
        var extTypes = extTypesʗp.slice();
        b.AddUint16(extensionECHOuterExtensions);
        var extTypesʗ1 = extTypes;
        b.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
            var extTypesʗ2 = extTypesʗ1;
            bΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
                foreach (var (_, extType) in extTypesʗ2) {
                    bΔ2.AddUint16(extType);
                }
            });
        });
    }

            var outerExtsʗ1 = outerExts;

            var outerExtsʗ2 = outerExts;

            var outerExtsʗ3 = outerExts;

            var outerExtsʗ4 = outerExts;

            var outerExtsʗ5 = outerExts;


    foreach (var (_, vᴛ1) in new TestDecodeInnerClientHelloOuterExtensions_type[]{
        new(
            name: "valid order"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                outerExtsʗ1(b, extensionStatusRequest, extensionSupportedCurves);
            }
        ),
        new(
            name: "duplicate reference"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                outerExtsʗ2(b, extensionStatusRequest, extensionStatusRequest);
            },
            wantErr: "tls: invalid outer extensions"u8
        ),
        new(
            name: "references encrypted_client_hello"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                outerExtsʗ3(b, extensionEncryptedClientHello);
            },
            wantErr: "tls: invalid outer extensions"u8
        ),
        new(
            name: "references ech_outer_extensions"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                outerExtsʗ4(b, extensionECHOuterExtensions);
            },
            wantErr: "tls: invalid outer extensions"u8
        ),
        new(
            name: "multiple ech_outer_extensions"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                outerExtsʗ5(b, extensionStatusRequest);
                outerExtsʗ5(b, extensionSupportedCurves);
            },
            wantErr: "tls: invalid outer extensions"u8
        ),
        new(
            name: "odd-length reference list"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                b.AddUint16(extensionECHOuterExtensions);
                b.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
                    bΔ1.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
                        bΔ2.AddUint8(0);
                    });
                });
            },
            wantErr: "tls: invalid inner client hello"u8
        ),
        new(
            name: "empty reference list"u8,
            buildExts: (ж<cryptobyte.Builder> b) => {
                b.AddUint16(extensionECHOuterExtensions);
                b.AddUint16LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
                    bΔ1.AddUint8(0);
                });
            },
            wantErr: "tls: invalid inner client hello"u8
        )
    }.slice()) {
        ref var tc = ref heap(new TestDecodeInnerClientHelloOuterExtensions_type(), out var Ꮡtc);
        tc = vᴛ1;

        var encodeInnerʗ1 = encodeInner;
        var outerʗ1 = outer;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var encoded = encodeInnerʗ1(tcʗ1.buildExts);
            var (_, err) = decodeInnerClientHello(outerʗ1, encoded);
            if (tcʗ1.wantErr == ""u8){
                if (err != default!) {
                    tΔ1.Fatalf("decodeInnerClientHello returned %v, want nil"u8, err);
                }
            } else 
            if (err == default! || err.Error() != tcʗ1.wantErr) {
                tΔ1.Fatalf("decodeInnerClientHello returned %v, want %q"u8, err, tcʗ1.wantErr);
            }
        });
    }
}

} // end tls_internal_test_package
