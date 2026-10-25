// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using ecdh = go.crypto.ecdh_package;
using Δmlkem = go.crypto.mlkem_package;
using mlkemtest = go.crypto.mlkem.mlkemtest_package;
using sha3 = go.crypto.sha3_package;
using hex = encoding.hex_package;
using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.mlkem;
using static go.crypto.hpke_package;

partial class hpke_internal_test_package {

public static void Example() {
    // In this example, we use MLKEM768-X25519 as the KEM, HKDF-SHA256 as the
    // KDF, and AES-256-GCM as the AEAD to encrypt a single message from a
    // sender to a recipient using the one-shot API.

    var (kem, kdf, Δaead) = (MLKEM768X25519(), HKDFSHA256(), AES256GCM());

    // Recipient side
    global::go.crypto.hpke_package.PrivateKey recipientPrivateKey = default!;
    slice<byte> publicKeyBytes = default!;
    {
        var (k, err) = kem.GenerateKey();
        if (err != default!) {
            throw panic(err);
        }
        recipientPrivateKey = k;
        publicKeyBytes = k.PublicKey().Bytes();
    }

    // Sender side
    slice<byte> ciphertext = default!;
    {
        var (publicKey, err) = kem.NewPublicKey(publicKeyBytes);
        if (err != default!) {
            throw panic(err);
        }

        var message = slice<byte>("|-()-|"u8);
        (var ct, err) = Seal(publicKey, kdf, Δaead, slice<byte>("example"u8), message);
        if (err != default!) {
            throw panic(err);
        }

        ciphertext = ct;
    }

    // Recipient side
    {
        var (plaintext, err) = Open(recipientPrivateKey, kdf, Δaead, slice<byte>("example"u8), ciphertext);
        if (err != default!) {
            throw panic(err);
        }
        fmt.Printf("Decrypted message: %s\n"u8, plaintext);
    }

    // Output:
    // Decrypted message: |-()-|
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sealInfoˢ = "Seal info"u8;
internal static readonly @string sealPlaintextˢ = "Seal plaintext"u8;
internal static readonly @string openInfoˢ = "Open info"u8;
internal static readonly @string openCiphertextˢ = "Open ciphertext"u8;
internal static readonly @string newSenderInfoˢ = "NewSender info"u8;
internal static readonly @string senderSealAadˢ = "Sender.Seal aad"u8;
internal static readonly @string senderSealPlaintextˢ = "Sender.Seal plaintext"u8;
internal static readonly @string newRecipientInfoˢ = "NewRecipient info"u8;
internal static readonly @string newRecipientEncˢ = "NewRecipient enc"u8;
internal static readonly @string recipientOpenAadˢ = "Recipient.Open aad"u8;
internal static readonly @string recipientOpenCiphertextˢ = "Recipient.Open ciphertext"u8;

public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    var kems = new global::go.crypto.hpke_package.ΔKEM[]{DHKEM(ecdh.P256()), DHKEM(ecdh.P384()), DHKEM(ecdh.P521()), DHKEM(ecdh.X25519()), MLKEM768(), MLKEM1024(), MLKEM768P256(), MLKEM1024P384(), MLKEM768X25519()
    }.slice();
    var kdfs = new global::go.crypto.hpke_package.KDF[]{HKDFSHA256(), HKDFSHA384(), HKDFSHA512(), SHAKE128(), SHAKE256()
    }.slice();
    var aeads = new global::go.crypto.hpke_package.AEAD[]{AES128GCM(), AES256GCM(), ChaCha20Poly1305()
    }.slice();

    foreach (var (_, kem) in kems) {
        var aeadsʗ1 = aeads;
        var kdfsʗ1 = kdfs;
        var kemʗ1 = kem;
        Ꮡt.Run(fmt.Sprintf("KEM_%04x"u8, kem.ID()), (ж<testing.T> tΔ1) => {
            var (k, err) = kemʗ1.GenerateKey();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (var kb, err) = k.Bytes();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (var kk, err) = kemʗ1.NewPrivateKey(kb);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                var (got, errΔ1) = kk.Bytes(); if (errΔ1 != default!){
                    tΔ1.Fatal(errΔ1);
                } else 
                if (!bytes.Equal(got, kb)) {
                    tΔ1.Errorf("re-serialized key mismatch: got %x, want %x"u8, got, kb);
                }
            }
            (var pk, err) = kemʗ1.NewPublicKey(k.PublicKey().Bytes());
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                var got = pk.Bytes(); if (!bytes.Equal(got, k.PublicKey().Bytes())) {
                    tΔ1.Errorf("re-serialized public key mismatch: got %x, want %x"u8, got, k.PublicKey().Bytes());
                }
            }

            foreach (var (_, kdf) in kdfsʗ1) {
                var aeadsʗ2 = aeadsʗ1;
                var kʗ1 = k;
                var kdfʗ1 = kdf;
                var kkʗ1 = kk;
                var pkʗ1 = pk;
                tΔ1.Run(fmt.Sprintf("KDF_%04x"u8, kdf.ID()), (ж<testing.T> tΔ2) => {
                    foreach (var (_, Δaead) in aeadsʗ2) {
                        var aeadʗ1 = Δaead;
                        var kʗ2 = kʗ1;
                        var kdfʗ2 = kdfʗ1;
                        var kkʗ2 = kkʗ1;
                        var pkʗ2 = pkʗ1;
                        tΔ2.Run(fmt.Sprintf("AEAD_%04x"u8, Δaead.ID()), (ж<testing.T> tΔ3) => {
                            var (c, errΔ2) = Seal(pkʗ2, kdfʗ2, aeadʗ1, slice<byte>("info"u8), slice<byte>("plaintext"u8));
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            (var p, errΔ2) = Open(kkʗ2, kdfʗ2, aeadʗ1, slice<byte>("info"u8), c);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            if (!bytes.Equal(p, slice<byte>("plaintext"u8))) {
                                tΔ3.Errorf("unexpected plaintext: got %x, want %x"u8, p, slice<byte>("plaintext"u8));
                            }

                            (p, errΔ2) = Open(kkʗ2, kdfʗ2, aeadʗ1, slice<byte>("wrong"u8), c);
                            if (errΔ2 == default!) {
                                tΔ3.Errorf("expected error when opening with wrong info, got plaintext %x"u8, p);
                            }
                            c[len(c) - 1] ^= (byte)(0xFF);
                            (p, errΔ2) = Open(kkʗ2, kdfʗ2, aeadʗ1, slice<byte>("info"u8), c);
                            if (errΔ2 == default!) {
                                tΔ3.Errorf("expected error when opening with corrupted ciphertext, got plaintext %x"u8, p);
                            }

                            (c, errΔ2) = Seal(kʗ2.PublicKey(), kdfʗ2, aeadʗ1, default!, default!);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            (p, errΔ2) = Open(kʗ2, kdfʗ2, aeadʗ1, default!, c);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            if (len(p) != 0) {
                                tΔ3.Errorf("unexpected plaintext: got %x, want empty"u8, p);
                            }

                            // Test that Seal and Open don't modify the excess capacity of input
                            // slices. This is a regression test for a bug where decap would
                            // append to the enc slice, corrupting the ciphertext if they shared
                            // a backing array.
                            slice<byte> padSlice(slice<byte> b) {
                                var s = new slice<byte>(len(b), len(b) + 2000);
                                copy(s, b);
                                for (nint i = len(b); i < cap(s); i++) {
                                    s.slice(0, cap(s))[i] = 0xAA;
                                }
                                return s.slice(0, len(b));
                            }
                            void checkSlice(@string name, slice<byte> s) {
                                for (nint i = len(s); i < cap(s); i++) {
                                    if (s.slice(0, cap(s))[i] != 0xAA) {
                                        tΔ3.Errorf("%s: modified byte at index %d beyond slice length"u8, name, i);
                                        return;
                                    }
                                }
                            }

                            var infoS = padSlice(slice<byte>("info"u8));
                            var plaintextS = padSlice(slice<byte>("plaintext"u8));
                            (c, errΔ2) = Seal(pkʗ2, kdfʗ2, aeadʗ1, infoS, plaintextS);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            checkSlice(sealInfoˢ, infoS);
                            checkSlice(sealPlaintextˢ, plaintextS);

                            var infoO = padSlice(slice<byte>("info"u8));
                            var ciphertextO = padSlice(c);
                            (p, errΔ2) = Open(kkʗ2, kdfʗ2, aeadʗ1, infoO, ciphertextO);
                            if (errΔ2 != default!) {
                                tΔ3.Fatalf("Open with large capacity slices failed: %v"u8, errΔ2);
                            }
                            if (!bytes.Equal(p, slice<byte>("plaintext"u8))) {
                                tΔ3.Errorf("unexpected plaintext: got %x, want %x"u8, p, slice<byte>("plaintext"u8));
                            }
                            checkSlice(openInfoˢ, infoO);
                            checkSlice(openCiphertextˢ, ciphertextO);

                            // Also test the Sender.Seal and Recipient.Open methods.
                            var infoSender = padSlice(slice<byte>("info"u8));
                            (var enc, var sender, errΔ2) = NewSender(pkʗ2, kdfʗ2, aeadʗ1, infoSender);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            checkSlice(newSenderInfoˢ, infoSender);

                            var aadSeal = padSlice(slice<byte>("aad"u8));
                            var plaintextSeal = padSlice(slice<byte>("plaintext"u8));
                            (var ct, errΔ2) = sender.Seal(aadSeal, plaintextSeal);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            checkSlice(senderSealAadˢ, aadSeal);
                            checkSlice(senderSealPlaintextˢ, plaintextSeal);

                            var infoRecipient = padSlice(slice<byte>("info"u8));
                            var encPadded = padSlice(enc);
                            (var recipient, errΔ2) = NewRecipient(encPadded, kkʗ2, kdfʗ2, aeadʗ1, infoRecipient);
                            if (errΔ2 != default!) {
                                tΔ3.Fatal(errΔ2);
                            }
                            checkSlice(newRecipientInfoˢ, infoRecipient);
                            checkSlice(newRecipientEncˢ, encPadded);

                            var aadOpen = padSlice(slice<byte>("aad"u8));
                            var ctPadded = padSlice(ct);
                            (p, errΔ2) = recipient.Open(aadOpen, ctPadded);
                            if (errΔ2 != default!) {
                                tΔ3.Fatalf("Recipient.Open failed: %v"u8, errΔ2);
                            }
                            if (!bytes.Equal(p, slice<byte>("plaintext"u8))) {
                                tΔ3.Errorf("unexpected plaintext: got %x, want %x"u8, p, slice<byte>("plaintext"u8));
                            }
                            checkSlice(recipientOpenAadˢ, aadOpen);
                            checkSlice(recipientOpenCiphertextˢ, ctPadded);
                        });
                    }
                });
            }
        });
    }
}

internal static slice<byte> mustDecodeHex(ж<testing.T> Ꮡt, @string @in) {
    Ꮡt.Helper();
    var (b, err) = hex.DecodeString(@in);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rfc9180ˢ = "rfc9180"u8;
internal static readonly @string hpkePqˢ = "hpke-pq"u8;

public static void TestVectors(ж<testing.T> Ꮡt) {
    Ꮡt.Run(rfc9180ˢ, (ж<testing.T> tΔ1) => {
        testVectors(tΔ1, rfc9180ˢ);
    });
    Ꮡt.Run(hpkePqˢ, (ж<testing.T> tΔ2) => {
        testVectors(tΔ2, hpkePqˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object onlyMode0BaseIsSupportedˢ = (@string)"only mode 0 (base) is supported"u8;
internal static readonly object kem0x0021DhkemX448Notˢ = (@string)"KEM 0x0021 (DHKEM(X448)) not supported"u8;
internal static readonly object kem0x0040MlKem512Notˢ = (@string)"KEM 0x0040 (ML-KEM-512) not supported"u8;
internal static readonly object expectedErrorFromSealˢ = (@string)"expected error from Seal with export-only AEAD"u8;
internal static readonly object expectedErrorFromOpenˢ = (@string)"expected error from Open with export-only AEAD"u8;

internal partial struct testVectors_vectors_Encryptions /*dyn*/ {
    public @string Aad; /*`json:"aad"`*/
    public @string Ct; /*`json:"ct"`*/
    public @string Nonce; /*`json:"nonce"`*/
    public @string Pt; /*`json:"pt"`*/
}

internal partial struct testVectors_vectors_Exports /*dyn*/ {
    public @string Context; /*`json:"exporter_context"`*/
    public nint L; /*`json:"L"`*/
    public @string Value; /*`json:"exported_value"`*/
}

internal partial struct testVectors_vectors /*dyn*/ {
    public uint16 Mode; /*`json:"mode"`*/
    public uint16 KEM; /*`json:"kem_id"`*/
    public uint16 KDF; /*`json:"kdf_id"`*/
    public uint16 AEAD; /*`json:"aead_id"`*/
    public @string Info; /*`json:"info"`*/
    public @string IkmE; /*`json:"ikmE"`*/
    public @string IkmR; /*`json:"ikmR"`*/
    public @string SkRm; /*`json:"skRm"`*/
    public @string PkRm; /*`json:"pkRm"`*/
    public @string Enc; /*`json:"enc"`*/
    public slice<testVectors_vectors_Encryptions> Encryptions; /*`json:"encryptions"`*/
    public slice<testVectors_vectors_Exports> Exports; /*`json:"exports"`*/
    // Instead of checking in a very large rfc9180.json, we computed
    // alternative accumulated values.
    public @string AccEncryptions; /*`json:"encryptions_accumulated"`*/
    public @string AccExports; /*`json:"exports_accumulated"`*/
}

internal static void testVectors(ж<testing.T> Ꮡt, @string name) {
    var (vectorsJSON, err) = os.ReadFile("testdata/"u8 + name + ".json"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var vectors = ref heap<slice<testVectors_vectors>>(out var Ꮡvectors);
    {
        var errΔ1 = json.Unmarshal(vectorsJSON, Ꮡvectors); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }

    foreach (var (_, vᴛ1) in vectors) {
        ref var vector = ref heap(new testVectors_vectors(), out var Ꮡvector);
        vector = vᴛ1;

        @string nameΔ1 = fmt.Sprintf("mode %04x kem %04x kdf %04x aead %04x"u8,
            vector.Mode, vector.KEM, vector.KDF, vector.AEAD);
        var vectorʗ1 = vector;
        Ꮡt.Run(nameΔ1, (ж<testing.T> tΔ1) => {
            if (vectorʗ1.Mode != 0) {
                tΔ1.Skip(onlyMode0BaseIsSupportedˢ);
            }
            if (vectorʗ1.KEM == 0x0021) {
                tΔ1.Skip(kem0x0021DhkemX448Notˢ);
            }
            if (vectorʗ1.KEM == 0x0040) {
                tΔ1.Skip(kem0x0040MlKem512Notˢ);
            }
            if (vectorʗ1.KDF == 0x0012 || vectorʗ1.KDF == 0x0013) {
                tΔ1.Skipf("TurboSHAKE KDF not supported"u8);
            }

            var (kdf, errΔ2) = NewKDF(vectorʗ1.KDF);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (kdf.ID() != vectorʗ1.KDF) {
                tΔ1.Errorf("unexpected KDF ID: got %04x, want %04x"u8, kdf.ID(), vectorʗ1.KDF);
            }

            (var Δaead, errΔ2) = NewAEAD(vectorʗ1.AEAD);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (Δaead.ID() != vectorʗ1.AEAD) {
                tΔ1.Errorf("unexpected AEAD ID: got %04x, want %04x"u8, Δaead.ID(), vectorʗ1.AEAD);
            }

            (var kem, errΔ2) = NewKEM(vectorʗ1.KEM);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (kem.ID() != vectorʗ1.KEM) {
                tΔ1.Errorf("unexpected KEM ID: got %04x, want %04x"u8, kem.ID(), vectorʗ1.KEM);
            }

            var pubKeyBytes = mustDecodeHex(tΔ1, vectorʗ1.PkRm);
            (var kemSender, errΔ2) = kem.NewPublicKey(pubKeyBytes);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (!AreEqual(kemSender.KEM(), kem)) {
                tΔ1.Errorf("unexpected KEM from sender: got %04x, want %04x"u8, kemSender.KEM().ID(), kem.ID());
            }
            if (!bytes.Equal(kemSender.Bytes(), pubKeyBytes)) {
                tΔ1.Errorf("unexpected KEM bytes: got %x, want %x"u8, kemSender.Bytes(), pubKeyBytes);
            }

            var ikmE = mustDecodeHex(tΔ1, vectorʗ1.IkmE);
            setupDerandomizedEncap(tΔ1, ikmE, kemSender);

            var info = mustDecodeHex(tΔ1, vectorʗ1.Info);
            (var encap, var sender, errΔ2) = NewSender(kemSender, kdf, Δaead, info);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (len(encap) != kem.encSize()) {
                tΔ1.Errorf("unexpected encapsulated key size: got %d, want %d"u8, len(encap), kem.encSize());
            }

            var expectedEncap = mustDecodeHex(tΔ1, vectorʗ1.Enc);
            if (!bytes.Equal(encap, expectedEncap)) {
                tΔ1.Errorf("unexpected encapsulated key, got: %x, want %x"u8, encap, expectedEncap);
            }

            var privKeyBytes = mustDecodeHex(tΔ1, vectorʗ1.SkRm);
            (var kemRecipient, errΔ2) = kem.NewPrivateKey(privKeyBytes);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (!AreEqual(kemRecipient.KEM(), kem)) {
                tΔ1.Errorf("unexpected KEM from recipient: got %04x, want %04x"u8, kemRecipient.KEM().ID(), kem.ID());
            }
            (var kemRecipientBytes, errΔ2) = kemRecipient.Bytes();
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            // X25519 serialized keys must be clamped, so the bytes might not match.
            if (!bytes.Equal(kemRecipientBytes, privKeyBytes) && vectorʗ1.KEM != DHKEM(ecdh.X25519()).ID()) {
                tΔ1.Errorf("unexpected KEM bytes: got %x, want %x"u8, kemRecipientBytes, privKeyBytes);
            }
            if (vectorʗ1.KEM == DHKEM(ecdh.X25519()).ID()) {
                var (kem2, errΔ3) = kem.NewPrivateKey(kemRecipientBytes);
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                (var kemRecipientBytes2, errΔ3) = kem2.Bytes();
                if (errΔ3 != default!) {
                    tΔ1.Fatal(errΔ3);
                }
                if (!bytes.Equal(kemRecipientBytes2, kemRecipientBytes)) {
                    tΔ1.Errorf("X25519 re-serialized key differs: got %x, want %x"u8, kemRecipientBytes2, kemRecipientBytes);
                }
                if (!bytes.Equal(kem2.PublicKey().Bytes(), pubKeyBytes)) {
                    tΔ1.Errorf("X25519 re-derived public key differs: got %x, want %x"u8, kem2.PublicKey().Bytes(), pubKeyBytes);
                }
            }
            if (!bytes.Equal(kemRecipient.PublicKey().Bytes(), pubKeyBytes)) {
                tΔ1.Errorf("unexpected KEM sender bytes: got %x, want %x"u8, kemRecipient.PublicKey().Bytes(), pubKeyBytes);
            }

            var ikm = mustDecodeHex(tΔ1, vectorʗ1.IkmR);
            (var derivRecipient, errΔ2) = kem.DeriveKeyPair(ikm);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            (var derivRecipientBytes, errΔ2) = derivRecipient.Bytes();
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
            if (!bytes.Equal(derivRecipientBytes, privKeyBytes) && vectorʗ1.KEM != DHKEM(ecdh.X25519()).ID()) {
                tΔ1.Errorf("unexpected KEM bytes from seed: got %x, want %x"u8, derivRecipientBytes, privKeyBytes);
            }
            if (!bytes.Equal(derivRecipient.PublicKey().Bytes(), pubKeyBytes)) {
                tΔ1.Errorf("unexpected KEM sender bytes from seed: got %x, want %x"u8, derivRecipient.PublicKey().Bytes(), pubKeyBytes);
            }

            (var recipient, errΔ2) = NewRecipient(encap, kemRecipient, kdf, Δaead, info);
            if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }

            if (!AreEqual(Δaead, ExportOnly()) && len(vectorʗ1.AccEncryptions) != 0){
                var (source, sink) = (sha3.NewSHAKE128(), sha3.NewSHAKE128());
                foreach (var _ᴛ1 in range(1000)) {
                    var (aad, plaintext) = (drawRandomInput(tΔ1, new hpke_internal_test_package.sha3_SHAKEжReader(source)), drawRandomInput(tΔ1, new hpke_internal_test_package.sha3_SHAKEжReader(source)));
                    var (ciphertext, errΔ4) = sender.Seal(aad, plaintext);
                    if (errΔ4 != default!) {
                        tΔ1.Fatal(errΔ4);
                    }
                    sink.Write(ciphertext);
                    (var got, errΔ4) = recipient.Open(aad, ciphertext);
                    if (errΔ4 != default!) {
                        tΔ1.Fatal(errΔ4);
                    }
                    if (!bytes.Equal(got, plaintext)) {
                        tΔ1.Errorf("unexpected plaintext: got %x want %x"u8, got, plaintext);
                    }
                }
                var encryptions = new slice<byte>(16);
                sink.Read(encryptions);
                var expectedEncryptions = mustDecodeHex(tΔ1, vectorʗ1.AccEncryptions);
                if (!bytes.Equal(encryptions, expectedEncryptions)) {
                    tΔ1.Errorf("unexpected accumulated encryptions, got: %x, want %x"u8, encryptions, expectedEncryptions);
                }
            } else 
            if (!AreEqual(Δaead, ExportOnly())){
                foreach (var (_, enc) in vectorʗ1.Encryptions) {
                    var aad = mustDecodeHex(tΔ1, enc.Aad);
                    var plaintext = mustDecodeHex(tΔ1, enc.Pt);
                    var expectedCiphertext = mustDecodeHex(tΔ1, enc.Ct);

                    var (ciphertext, errΔ5) = sender.Seal(aad, plaintext);
                    if (errΔ5 != default!) {
                        tΔ1.Fatal(errΔ5);
                    }
                    if (!bytes.Equal(ciphertext, expectedCiphertext)) {
                        tΔ1.Errorf("unexpected ciphertext, got: %x, want %x"u8, ciphertext, expectedCiphertext);
                    }

                    (var got, errΔ5) = recipient.Open(aad, ciphertext);
                    if (errΔ5 != default!) {
                        tΔ1.Fatal(errΔ5);
                    }
                    if (!bytes.Equal(got, plaintext)) {
                        tΔ1.Errorf("unexpected plaintext: got %x want %x"u8, got, plaintext);
                    }
                }
            } else {
                {
                    var (_, errΔ6) = sender.Seal(default!, default!); if (errΔ6 == default!) {
                        tΔ1.Error(expectedErrorFromSealˢ);
                    }
                }
                {
                    var (_, errΔ7) = recipient.Open(default!, default!); if (errΔ7 == default!) {
                        tΔ1.Error(expectedErrorFromOpenˢ);
                    }
                }
            }

            if (len(vectorʗ1.AccExports) != 0){
                var (source, sink) = (sha3.NewSHAKE128(), sha3.NewSHAKE128());
                foreach (var l in range(1000)) {
                    @string context = (@string)drawRandomInput(tΔ1, new hpke_internal_test_package.sha3_SHAKEжReader(source));
                    var (value, errΔ8) = sender.Export(context, l);
                    if (errΔ8 != default!) {
                        tΔ1.Fatal(errΔ8);
                    }
                    sink.Write(value);
                    (var got, errΔ8) = recipient.Export(context, l);
                    if (errΔ8 != default!) {
                        tΔ1.Fatal(errΔ8);
                    }
                    if (!bytes.Equal(got, value)) {
                        tΔ1.Errorf("recipient: unexpected exported secret: got %x want %x"u8, got, value);
                    }
                }
                var exports = new slice<byte>(16);
                sink.Read(exports);
                var expectedExports = mustDecodeHex(tΔ1, vectorʗ1.AccExports);
                if (!bytes.Equal(exports, expectedExports)) {
                    tΔ1.Errorf("unexpected accumulated exports, got: %x, want %x"u8, exports, expectedExports);
                }
            } else {
                foreach (var (_, exp) in vectorʗ1.Exports) {
                    @string context = (@string)mustDecodeHex(tΔ1, exp.Context);
                    var expectedValue = mustDecodeHex(tΔ1, exp.Value);

                    var (value, errΔ9) = sender.Export(context, exp.L);
                    if (errΔ9 != default!) {
                        tΔ1.Fatal(errΔ9);
                    }
                    if (!bytes.Equal(value, expectedValue)) {
                        tΔ1.Errorf("unexpected exported value, got: %x, want %x"u8, value, expectedValue);
                    }

                    (var got, errΔ9) = recipient.Export(context, exp.L);
                    if (errΔ9 != default!) {
                        tΔ1.Fatal(errΔ9);
                    }
                    if (!bytes.Equal(got, value)) {
                        tΔ1.Errorf("recipient: unexpected exported secret: got %x want %x"u8, got, value);
                    }
                }
            }
        });
    }
}

internal static slice<byte> drawRandomInput(ж<testing.T> Ꮡt, io.Reader r) {
    Ꮡt.Helper();
    var l = new slice<byte>(1);
    {
        var (_, err) = r.Read(l); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    nint n = (nint)l[0];
    var b = new slice<byte>(n);
    {
        var (_, err) = r.Read(b); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    return b;
}

internal static void setupDerandomizedEncap(ж<testing.T> Ꮡt, slice<byte> randBytes, global::go.crypto.hpke_package.ΔPublicKey pk) {
    Ꮡt.Cleanup(() => {
        testingOnlyGenerateKey = default!;
        testingOnlyEncapsulate = default!;
    });
    var exprᴛ1 = pk.KEM();
    if (AreEqual(exprᴛ1, DHKEM(ecdh.P256())) || AreEqual(exprᴛ1, DHKEM(ecdh.P384())) || AreEqual(exprᴛ1, DHKEM(ecdh.P521())) || AreEqual(exprᴛ1, DHKEM(ecdh.X25519()))) {
        var (r, err) = pk.KEM().DeriveKeyPair(randBytes);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        testingOnlyGenerateKey = () => (~rʗ1._<ж<global::go.crypto.hpke_package.dhKEMPrivateKey>>()).priv._<ж<ecdh.PrivateKey>>();
    }
    else if (AreEqual(exprᴛ1, mlkem768)) {
        var pq = (~pk._<ж<global::go.crypto.hpke_package.mlkemPublicKey>>()).pq._<ж<Δmlkem.EncapsulationKey768>>();
        var pqʗ1 = pq;
        var randBytesʗ1 = randBytes;
        testingOnlyEncapsulate = () => {
            var (ss, ct, err) = mlkemtest.Encapsulate768(pqʗ1, randBytesʗ1);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            return (ss, ct);
        };
    }
    else if (AreEqual(exprᴛ1, mlkem1024)) {
        var pq = (~pk._<ж<global::go.crypto.hpke_package.mlkemPublicKey>>()).pq._<ж<Δmlkem.EncapsulationKey1024>>();
        var pqʗ2 = pq;
        var randBytesʗ2 = randBytes;
        testingOnlyEncapsulate = () => {
            var (ss, ct, err) = mlkemtest.Encapsulate1024(pqʗ2, randBytesʗ2);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            return (ss, ct);
        };
    }
    else if (AreEqual(exprᴛ1, mlkem768X25519)) {
        var (pqRand, tRand) = (randBytes[..32], randBytes[32..]);
        var pq = (~pk._<ж<global::go.crypto.hpke_package.hybridPublicKey>>()).pq._<ж<Δmlkem.EncapsulationKey768>>();
        var (k, err) = ecdh.X25519().NewPrivateKey(tRand);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var kʗ1 = k;
        testingOnlyGenerateKey = () => kʗ1;
        var pqʗ3 = pq;
        var pqRandʗ1 = pqRand;
        testingOnlyEncapsulate = () => {
            var (ss, ct, errΔ1) = mlkemtest.Encapsulate768(pqʗ3, pqRandʗ1);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            return (ss, ct);
        };
    }
    else if (AreEqual(exprᴛ1, mlkem768P256)) {
        // The rest of randBytes are the following candidates for rejection
        // sampling, but they are never reached.
        var (pqRand, tRand) = (randBytes[..32], randBytes[32..64]);
        var pq = (~pk._<ж<global::go.crypto.hpke_package.hybridPublicKey>>()).pq._<ж<Δmlkem.EncapsulationKey768>>();
        var (k, err) = ecdh.P256().NewPrivateKey(tRand);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var kʗ2 = k;
        testingOnlyGenerateKey = () => kʗ2;
        var pqʗ4 = pq;
        var pqRandʗ2 = pqRand;
        testingOnlyEncapsulate = () => {
            var (ss, ct, errΔ1) = mlkemtest.Encapsulate768(pqʗ4, pqRandʗ2);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            return (ss, ct);
        };
    }
    else if (AreEqual(exprᴛ1, mlkem1024P384)) {
        var (pqRand, tRand) = (randBytes[..32], randBytes[32..]);
        var pq = (~pk._<ж<global::go.crypto.hpke_package.hybridPublicKey>>()).pq._<ж<Δmlkem.EncapsulationKey1024>>();
        var (k, err) = ecdh.P384().NewPrivateKey(tRand);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var kʗ3 = k;
        testingOnlyGenerateKey = () => kʗ3;
        var pqʗ5 = pq;
        var pqRandʗ3 = pqRand;
        testingOnlyEncapsulate = () => {
            var (ss, ct, errΔ1) = mlkemtest.Encapsulate1024(pqʗ5, pqRandʗ3);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            return (ss, ct);
        };
    }
    else { /* default: */
        Ꮡt.Fatalf("unsupported KEM %04x"u8, pk.KEM().ID());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object hkdfsha256Hkdfsha256ˢ = (@string)"HKDFSHA256() != HKDFSHA256()"u8;
internal static readonly object hkdfsha384Hkdfsha384ˢ = (@string)"HKDFSHA384() != HKDFSHA384()"u8;
internal static readonly object hkdfsha512Hkdfsha512ˢ = (@string)"HKDFSHA512() != HKDFSHA512()"u8;
internal static readonly object aes128gcmAes128gcmˢ = (@string)"AES128GCM() != AES128GCM()"u8;
internal static readonly object aes256gcmAes256gcmˢ = (@string)"AES256GCM() != AES256GCM()"u8;
internal static readonly object chaCha20Poly1305ˢ = (@string)"ChaCha20Poly1305() != ChaCha20Poly1305()"u8;
internal static readonly object exportOnlyExportOnlyˢ = (@string)"ExportOnly() != ExportOnly()"u8;
internal static readonly object dhkemP256DhkemP256ˢ = (@string)"DHKEM(P-256) != DHKEM(P-256)"u8;
internal static readonly object dhkemP384DhkemP384ˢ = (@string)"DHKEM(P-384) != DHKEM(P-384)"u8;
internal static readonly object dhkemP521DhkemP521ˢ = (@string)"DHKEM(P-521) != DHKEM(P-521)"u8;
internal static readonly object dhkemX25519DhkemX25519ˢ = (@string)"DHKEM(X25519) != DHKEM(X25519)"u8;
internal static readonly object mlkem768Mlkem768ˢ = (@string)"MLKEM768() != MLKEM768()"u8;
internal static readonly object mlkem1024Mlkem1024ˢ = (@string)"MLKEM1024() != MLKEM1024()"u8;
internal static readonly object mlkem768x25519ˢ = (@string)"MLKEM768X25519() != MLKEM768X25519()"u8;
internal static readonly object mlkem768p256Mlkem768p256ˢ = (@string)"MLKEM768P256() != MLKEM768P256()"u8;
internal static readonly object mlkem1024p384ˢ = (@string)"MLKEM1024P384() != MLKEM1024P384()"u8;

public static void TestSingletons(ж<testing.T> Ꮡt) {
    if (!AreEqual(HKDFSHA256(), HKDFSHA256())) {
        Ꮡt.Error(hkdfsha256Hkdfsha256ˢ);
    }
    if (!AreEqual(HKDFSHA384(), HKDFSHA384())) {
        Ꮡt.Error(hkdfsha384Hkdfsha384ˢ);
    }
    if (!AreEqual(HKDFSHA512(), HKDFSHA512())) {
        Ꮡt.Error(hkdfsha512Hkdfsha512ˢ);
    }
    if (!AreEqual(AES128GCM(), AES128GCM())) {
        Ꮡt.Error(aes128gcmAes128gcmˢ);
    }
    if (!AreEqual(AES256GCM(), AES256GCM())) {
        Ꮡt.Error(aes256gcmAes256gcmˢ);
    }
    if (!AreEqual(ChaCha20Poly1305(), ChaCha20Poly1305())) {
        Ꮡt.Error(chaCha20Poly1305ˢ);
    }
    if (!AreEqual(ExportOnly(), ExportOnly())) {
        Ꮡt.Error(exportOnlyExportOnlyˢ);
    }
    if (!AreEqual(DHKEM(ecdh.P256()), DHKEM(ecdh.P256()))) {
        Ꮡt.Error(dhkemP256DhkemP256ˢ);
    }
    if (!AreEqual(DHKEM(ecdh.P384()), DHKEM(ecdh.P384()))) {
        Ꮡt.Error(dhkemP384DhkemP384ˢ);
    }
    if (!AreEqual(DHKEM(ecdh.P521()), DHKEM(ecdh.P521()))) {
        Ꮡt.Error(dhkemP521DhkemP521ˢ);
    }
    if (!AreEqual(DHKEM(ecdh.X25519()), DHKEM(ecdh.X25519()))) {
        Ꮡt.Error(dhkemX25519DhkemX25519ˢ);
    }
    if (!AreEqual(MLKEM768(), MLKEM768())) {
        Ꮡt.Error(mlkem768Mlkem768ˢ);
    }
    if (!AreEqual(MLKEM1024(), MLKEM1024())) {
        Ꮡt.Error(mlkem1024Mlkem1024ˢ);
    }
    if (!AreEqual(MLKEM768X25519(), MLKEM768X25519())) {
        Ꮡt.Error(mlkem768x25519ˢ);
    }
    if (!AreEqual(MLKEM768P256(), MLKEM768P256())) {
        Ꮡt.Error(mlkem768p256Mlkem768p256ˢ);
    }
    if (!AreEqual(MLKEM1024P384(), MLKEM1024P384())) {
        Ꮡt.Error(mlkem1024p384ˢ);
    }
}

} // end hpke_internal_test_package
