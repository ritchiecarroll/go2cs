// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using hex = encoding.hex_package;
using testing = testing_package;
using encoding;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal partial struct testSplitPreMasterSecretTest {
    internal @string @in, out1, out2;
}

internal static slice<testSplitPreMasterSecretTest> testSplitPreMasterSecretTests = new testSplitPreMasterSecretTest[]{
    new(""u8, ""u8, ""u8),
    new("00"u8, "00"u8, "00"u8),
    new("0011"u8, "00"u8, "11"u8),
    new("001122"u8, "0011"u8, "1122"u8),
    new("00112233"u8, "0011"u8, "2233"u8)
}.slice();

public static void TestSplitPreMasterSecret(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in testSplitPreMasterSecretTests) {
        var (@in, _) = hex.DecodeString(test.@in);
        var (out1, out2) = splitPreMasterSecret(@in);
        @string s1 = hex.EncodeToString(out1);
        @string s2 = hex.EncodeToString(out2);
        if (s1 != test.out1 || s2 != test.out2) {
            Ꮡt.Errorf("#%d: got: (%s, %s) want: (%s, %s)"u8, i, s1, s2, test.out1, test.out2);
        }
    }
}

internal partial struct testKeysFromTest {
    internal uint16 version;
    internal ж<global::go.crypto.tls_package.cipherSuite> suite;
    internal @string preMasterSecret;
    internal @string clientRandom, serverRandom;
    internal @string masterSecret;
    internal @string clientMAC, serverMAC;
    internal @string clientKey, serverKey;
    internal nint macLen, keyLen;
    internal @string contextKeyingMaterial, noContextKeyingMaterial;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string labelˢ = "label"u8;

public static void TestKeysFromPreMasterSecret(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in testKeysFromTests) {
        var (@in, _) = hex.DecodeString(test.preMasterSecret);
        var (clientRandom, _) = hex.DecodeString(test.clientRandom);
        var (serverRandom, _) = hex.DecodeString(test.serverRandom);
        var masterSecret = masterFromPreMasterSecret(test.version, ref (test.suite).DerefOrNull(), @in, clientRandom, serverRandom);
        {
            @string s = hex.EncodeToString(masterSecret); if (s != test.masterSecret) {
                Ꮡt.Errorf("#%d: bad master secret %s, want %s"u8, i, s, test.masterSecret);
                continue;
            }
        }
        var (clientMAC, serverMAC, clientKey, serverKey, _, _) = keysFromMasterSecret(test.version, ref (test.suite).DerefOrNull(), masterSecret, clientRandom, serverRandom, test.macLen, test.keyLen, 0);
        @string clientMACString = hex.EncodeToString(clientMAC);
        @string serverMACString = hex.EncodeToString(serverMAC);
        @string clientKeyString = hex.EncodeToString(clientKey);
        @string serverKeyString = hex.EncodeToString(serverKey);
        if (clientMACString != test.clientMAC || serverMACString != test.serverMAC || clientKeyString != test.clientKey || serverKeyString != test.serverKey) {
            Ꮡt.Errorf("#%d: got: (%s, %s, %s, %s) want: (%s, %s, %s, %s)"u8, i, clientMACString, serverMACString, clientKeyString, serverKeyString, test.clientMAC, test.serverMAC, test.clientKey, test.serverKey);
        }
        var ekm = ekmFromMasterSecret(test.version, test.suite, masterSecret, clientRandom, serverRandom);
        var (contextKeyingMaterial, err) = ekm(labelˢ, slice<byte>("context"u8), 32);
        if (err != default!) {
            Ꮡt.Fatalf("ekmFromMasterSecret failed: %v"u8, err);
        }
        (var noContextKeyingMaterial, err) = ekm(labelˢ, default!, 32);
        if (err != default!) {
            Ꮡt.Fatalf("ekmFromMasterSecret failed: %v"u8, err);
        }
        if (hex.EncodeToString(contextKeyingMaterial) != test.contextKeyingMaterial || hex.EncodeToString(noContextKeyingMaterial) != test.noContextKeyingMaterial) {
            Ꮡt.Errorf("#%d: got keying material: (%s, %s) want: (%s, %s)"u8, i, contextKeyingMaterial, noContextKeyingMaterial, test.contextKeyingMaterial, test.noContextKeyingMaterial);
        }
    }
}

// These test vectors were generated from GnuTLS using `gnutls-cli --insecure -d 9 `
internal static slice<testKeysFromTest> testKeysFromTests;
internal static void initᴛtestKeysFromTests() { testKeysFromTests = new testKeysFromTest[]{
    new(
        VersionTLS10,
        cipherSuiteByID(TLS_RSA_WITH_RC4_128_SHA),
        "0302cac83ad4b1db3b9ab49ad05957de2a504a634a386fc600889321e1a971f57479466830ac3e6f468e87f5385fa0c5"u8,
        "4ae66303755184a3917fcb44880605fcc53baa01912b22ed94473fc69cebd558"u8,
        "4ae663020ec16e6bb5130be918cfcafd4d765979a3136a5d50c593446e4e44db"u8,
        "3d851bab6e5556e959a16bc36d66cfae32f672bfa9ecdef6096cbb1b23472df1da63dbbd9827606413221d149ed08ceb"u8,
        "805aaa19b3d2c0a0759a4b6c9959890e08480119"u8,
        "2d22f9fe519c075c16448305ceee209fc24ad109"u8,
        "d50b5771244f850cd8117a9ccafe2cf1"u8,
        "e076e33206b30507a85c32855acd0919"u8,
        20,
        16,
        "4d1bb6fc278c37d27aa6e2a13c2e079095d143272c2aa939da33d88c1c0cec22"u8,
        "93fba89599b6321ae538e27c6548ceb8b46821864318f5190d64a375e5d69d41"u8
    ),
    new(
        VersionTLS10,
        cipherSuiteByID(TLS_RSA_WITH_RC4_128_SHA),
        "03023f7527316bc12cbcd69e4b9e8275d62c028f27e65c745cfcddc7ce01bd3570a111378b63848127f1c36e5f9e4890"u8,
        "4ae66364b5ea56b20ce4e25555aed2d7e67f42788dd03f3fee4adae0459ab106"u8,
        "4ae66363ab815cbf6a248b87d6b556184e945e9b97fbdf247858b0bdafacfa1c"u8,
        "7d64be7c80c59b740200b4b9c26d0baaa1c5ae56705acbcf2307fe62beb4728c19392c83f20483801cce022c77645460"u8,
        "97742ed60a0554ca13f04f97ee193177b971e3b0"u8,
        "37068751700400e03a8477a5c7eec0813ab9e0dc"u8,
        "207cddbc600d2a200abac6502053ee5c"u8,
        "df3f94f6e1eacc753b815fe16055cd43"u8,
        20,
        16,
        "2c9f8961a72b97cbe76553b5f954caf8294fc6360ef995ac1256fe9516d0ce7f"u8,
        "274f19c10291d188857ad8878e2119f5aa437d4da556601cf1337aff23154016"u8
    ),
    new(
        VersionTLS10,
        cipherSuiteByID(TLS_RSA_WITH_RC4_128_SHA),
        "832d515f1d61eebb2be56ba0ef79879efb9b527504abb386fb4310ed5d0e3b1f220d3bb6b455033a2773e6d8bdf951d278a187482b400d45deb88a5d5a6bb7d6a7a1decc04eb9ef0642876cd4a82d374d3b6ff35f0351dc5d411104de431375355addc39bfb1f6329fb163b0bc298d658338930d07d313cd980a7e3d9196cac1"u8,
        "4ae663b2ee389c0de147c509d8f18f5052afc4aaf9699efe8cb05ece883d3a5e"u8,
        "4ae664d503fd4cff50cfc1fb8fc606580f87b0fcdac9554ba0e01d785bdf278e"u8,
        "1aff2e7a2c4279d0126f57a65a77a8d9d0087cf2733366699bec27eb53d5740705a8574bb1acc2abbe90e44f0dd28d6c"u8,
        "3c7647c93c1379a31a609542aa44e7f117a70085"u8,
        "0d73102994be74a575a3ead8532590ca32a526d4"u8,
        "ac7581b0b6c10d85bbd905ffbf36c65e"u8,
        "ff07edde49682b45466bd2e39464b306"u8,
        20,
        16,
        "678b0d43f607de35241dc7e9d1a7388a52c35033a1a0336d4d740060a6638fe2"u8,
        "f3b4ac743f015ef21d79978297a53da3e579ee047133f38c234d829c0f907dab"u8
    )
}.slice(); }

} // end tls_internal_test_package
