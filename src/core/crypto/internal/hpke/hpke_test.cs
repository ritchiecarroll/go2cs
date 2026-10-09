// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using hex = encoding.hex_package;
using json = encoding.json_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using ecdh = go.crypto.ecdh_package;
// blank import: go.crypto.sha256_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.sha512_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using encoding;
using go.crypto;
using static go.crypto.@internal.hpke_package;

partial class hpke_internal_test_package {

internal static slice<byte> mustDecodeHex(ж<testing.T> Ꮡt, @string @in) {
    Ꮡt.Helper();
    var (b, err) = hex.DecodeString(@in);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return b;
}

internal static map<@string, @string> parseVectorSetup(@string vector) {
    var vals = new map<@string, @string>{};
    foreach (var (_, l) in strings.Split(vector, "\n"u8)) {
        var fields = strings.Split(l, ": "u8);
        vals[fields[0]] = fields[1];
    }
    return vals;
}

internal static slice<map<@string, @string>> parseVectorEncryptions(@string vector) {
    var vals = new map<@string, @string>[]{}.slice();
    foreach (var (_, section) in strings.Split(vector, "\n\n"u8)) {
        var e = new map<@string, @string>{};
        foreach (var (_, l) in strings.Split(section, "\n"u8)) {
            var fields = strings.Split(l, ": "u8);
            e[fields[0]] = fields[1];
        }
        vals = append(vals, e);
    }
    return vals;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataRfc9180Vectorsˢ = "testdata/rfc9180-vectors.json"u8;
internal static readonly @string kemIdˢ = "kem_id"u8;
internal static readonly object unsupportedKemˢ = (@string)"unsupported KEM"u8;
internal static readonly @string kdfIdˢ = "kdf_id"u8;
internal static readonly object unsupportedKdfˢ = (@string)"unsupported KDF"u8;
internal static readonly @string aeadIdˢ = "aead_id"u8;
internal static readonly object unsupportedAeadˢ = (@string)"unsupported AEAD"u8;
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string pkRmˢ = "pkRm"u8;
internal static readonly @string skEmˢ = "skEm"u8;
internal static readonly @string encˢ = "enc"u8;
internal static readonly @string skRmˢ = "skRm"u8;
internal static readonly @string exporterSecretˢ = "exporter_secret"u8;
internal static readonly @string sequenceNumberˢ = "sequence number"u8;
internal static readonly @string nonceˢ = "nonce"u8;
internal static readonly @string aadˢ = "aad"u8;

internal partial struct TestRFC9180Vectors_vectors /*dyn*/ {
    public @string Name;
    public @string Setup;
    public @string Encryptions;
}

public static void TestRFC9180Vectors(ж<testing.T> Ꮡt) {
    var (vectorsJSON, err) = os.ReadFile(testdataRfc9180Vectorsˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var vectors = ref heap<slice<TestRFC9180Vectors_vectors>>(out var Ꮡvectors);
    {
        var errΔ1 = json.Unmarshal(vectorsJSON, Ꮡvectors); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    foreach (var (_, vᴛ1) in vectors) {
        ref var vector = ref heap(new TestRFC9180Vectors_vectors(), out var Ꮡvector);
        vector = vᴛ1;

        var vectorʗ1 = vector;
        Ꮡt.Run(vector.Name, (ж<testing.T> tΔ1) => {
            var setup = parseVectorSetup(vectorʗ1.Setup);
            var (kemID, errΔ2) = strconv.Atoi(setup[kemIdˢ]);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            {
                var (_, ok) = SupportedKEMs[(uint16)kemID, ꟷ]; if (!ok) {
                    tΔ1.Skip(unsupportedKemˢ);
                }
            }
            (var kdfID, errΔ2) = strconv.Atoi(setup[kdfIdˢ]);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            {
                var (_, ok) = SupportedKDFs[(uint16)kdfID, ꟷ]; if (!ok) {
                    tΔ1.Skip(unsupportedKdfˢ);
                }
            }
            (var aeadID, errΔ2) = strconv.Atoi(setup[aeadIdˢ]);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            {
                var (_, ok) = SupportedAEADs[(uint16)aeadID, ꟷ]; if (!ok) {
                    tΔ1.Skip(unsupportedAeadˢ);
                }
            }
            var info = mustDecodeHex(tΔ1, setup[infoˢ]);
            var pubKeyBytes = mustDecodeHex(tΔ1, setup[pkRmˢ]);
            (var pub, errΔ2) = ParseHPKEPublicKey((uint16)kemID, pubKeyBytes);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            var ephemeralPrivKey = mustDecodeHex(tΔ1, setup[skEmˢ]);
            var ephemeralPrivKeyʗ1 = ephemeralPrivKey;
            testingOnlyGenerateKey = () => SupportedKEMs[(uint16)kemID].curve.NewPrivateKey(ephemeralPrivKeyʗ1);
            tΔ1.Cleanup(() => {
                testingOnlyGenerateKey = default!;
            });
            (var encap, var sender, errΔ2) = SetupSender(
                (uint16)kemID,
                (uint16)kdfID,
                (uint16)aeadID,
                pub,
                info);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            var expectedEncap = mustDecodeHex(tΔ1, setup[encˢ]);
            if (!bytes_package.Equal(encap, expectedEncap)) {
                tΔ1.Errorf("unexpected encapsulated key, got: %x, want %x"u8, encap, expectedEncap);
            }
            var privKeyBytes = mustDecodeHex(tΔ1, setup[skRmˢ]);
            (var priv, errΔ2) = ParseHPKEPrivateKey((uint16)kemID, privKeyBytes);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            (var receipient, errΔ2) = SetupReceipient(
                (uint16)kemID,
                (uint16)kdfID,
                (uint16)aeadID,
                priv,
                info,
                encap);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            foreach (var (_, ctx) in new ж<global::go.crypto.@internal.hpke_package.context>[]{(~sender).context, (~receipient).context}.slice()) {
                var expectedSharedSecret = mustDecodeHex(tΔ1, setup[sharedSecretˢ]);
                if (!bytes_package.Equal((~ctx).sharedSecret, expectedSharedSecret)) {
                    tΔ1.Errorf("unexpected shared secret, got: %x, want %x"u8, (~ctx).sharedSecret, expectedSharedSecret);
                }
                var expectedKey = mustDecodeHex(tΔ1, setup[keyˢ]);
                if (!bytes_package.Equal((~ctx).key, expectedKey)) {
                    tΔ1.Errorf("unexpected key, got: %x, want %x"u8, (~ctx).key, expectedKey);
                }
                var expectedBaseNonce = mustDecodeHex(tΔ1, setup[baseNonceˢ]);
                if (!bytes_package.Equal((~ctx).baseNonce, expectedBaseNonce)) {
                    tΔ1.Errorf("unexpected base nonce, got: %x, want %x"u8, (~ctx).baseNonce, expectedBaseNonce);
                }
                var expectedExporterSecret = mustDecodeHex(tΔ1, setup[exporterSecretˢ]);
                if (!bytes_package.Equal((~ctx).exporterSecret, expectedExporterSecret)) {
                    tΔ1.Errorf("unexpected exporter secret, got: %x, want %x"u8, (~ctx).exporterSecret, expectedExporterSecret);
                }
            }
            foreach (var (_, enc) in parseVectorEncryptions(vectorʗ1.Encryptions)) {
                var encʗ1 = enc;
                var receipientʗ1 = receipient;
                var senderʗ1 = sender;
                tΔ1.Run("seq num " + enc[sequenceNumberˢ], (ж<testing.T> tΔ2) => {
                    var (seqNum, errΔ3) = strconv.Atoi(encʗ1[sequenceNumberˢ]);
                    if (errΔ3 != default!) {
                        tΔ2.Fatal(errΔ3);
                    }
                    senderʗ1.Value.seqNum = new uint128(lo: (uint64)seqNum);
                    receipientʗ1.Value.seqNum = new uint128(lo: (uint64)seqNum);
                    var expectedNonce = mustDecodeHex(tΔ2, encʗ1[nonceˢ]);
                    var computedNonce = senderʗ1.nextNonce();
                    if (!bytes_package.Equal(computedNonce, expectedNonce)) {
                        tΔ2.Errorf("unexpected nonce: got %x, want %x"u8, computedNonce, expectedNonce);
                    }
                    var expectedCiphertext = mustDecodeHex(tΔ2, encʗ1["ct"u8]);
                    (var ciphertext, errΔ3) = senderʗ1.Seal(mustDecodeHex(tΔ2, encʗ1[aadˢ]), mustDecodeHex(tΔ2, encʗ1["pt"u8]));
                    if (errΔ3 != default!) {
                        tΔ2.Fatal(errΔ3);
                    }
                    if (!bytes_package.Equal(ciphertext, expectedCiphertext)) {
                        tΔ2.Errorf("unexpected ciphertext: got %x want %x"u8, ciphertext, expectedCiphertext);
                    }
                    var expectedPlaintext = mustDecodeHex(tΔ2, encʗ1["pt"u8]);
                    (var plaintext, errΔ3) = receipientʗ1.Open(mustDecodeHex(tΔ2, encʗ1[aadˢ]), mustDecodeHex(tΔ2, encʗ1["ct"u8]));
                    if (errΔ3 != default!) {
                        tΔ2.Fatal(errΔ3);
                    }
                    if (!bytes_package.Equal(plaintext, expectedPlaintext)) {
                        tΔ2.Errorf("unexpected plaintext: got %x want %x"u8, plaintext, expectedPlaintext);
                    }
                });
            }
        });
    }
}

} // end hpke_internal_test_package
