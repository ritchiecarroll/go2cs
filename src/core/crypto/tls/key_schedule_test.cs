// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using sha256 = go.crypto.sha256_package;
using hex = encoding.hex_package;
using strings = strings_package;
using testing = testing_package;
using unicode = unicode_package;
using encoding;
using go.crypto;
using go.crypto.@internal.fips140;
using hash = hash_package;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

public static void TestACVPVectors(ж<testing.T> Ꮡt) {
    // https://github.com/usnistgov/ACVP-Server/blob/3a7333f63/gen-val/json-files/TLS-v1.3-KDF-RFC8446/prompt.json#L428-L436
    var psk = fromHex("56288B726C73829F7A3E47B103837C8139ACF552E7530C7A710B35ED41191698"u8);
    var dhe = fromHex("EFFE9EC26AA29FD750DFA6A10B944D74071595B27EE88887D5E11C84590B5CC3"u8);
    var helloClientRandom = fromHex("E9137679E582BA7C1DB41CF725F86C6D09C8C05F297BAD9A65B552EAF524FDE4"u8);
    var helloServerRandom = fromHex("23ECCFD030790748C8F8D8A656FD98D717F1B62AF3712F97211D2070B499F98A"u8);
    var finishedClientRandom = fromHex("62A62FA75563ED4FDCAA0BC16567B314871C304ACF06B0FFC3F08C1797594D43"u8);
    var finishedServerRandom = fromHex("C750EDA6696CD101B142BD79E00E6AC8C5F2C0ABC78DD64F4D991326659E9299"u8);

    // https://github.com/usnistgov/ACVP-Server/blob/3a7333f63/gen-val/json-files/TLS-v1.3-KDF-RFC8446/expectedResults.json#L571-L581
    var clientEarlyTrafficSecret = fromHex("3272189698C3594D18F58EFA3F12B638A249515099BE7A2FA9836BABE74F0111"u8);
    var earlyExporterMasterSecret = fromHex("88E078F562CDC930219F6A5E98A1CE8C6E5F3DAC5AC516459A96F2EF8F114C66"u8);
    var clientHandshakeTrafficSecret = fromHex("B32306C3CE9932C460A1FE6C0F060593974842036B96FA45049B7352E71C2AD2"u8);
    var serverHandshakeTrafficSecret = fromHex("22787F8CA269D34BC549AC8BA19F2040938A3AA370D7CC9D60F720882B88D01B"u8);
    var clientApplicationTrafficSecret = fromHex("47D7EA08397B5871154B0FE85584BCC30A87C69E84D69B56007C5B21F76493BA"u8);
    var serverApplicationTrafficSecret = fromHex("EFBDB0C873C0480DA57307083839A8984BE25B9A8545E4FCA029940FE2800565"u8);
    var exporterMasterSecret = fromHex("8A43D787EE3804EAD4A2A5B32972F9896B696295645D7222E1FD081DDD939834"u8);
    var resumptionMasterSecret = fromHex("5F4C961329C91044011ACBECB0B289282E0E3FED045CB3EA924DFFE5FE654B3D"u8);

    // The "Random" values are undocumented, but they are meant to be written to
    // the hash in sequence to develop the transcript.
    var transcript = sha256.New();

    var es = tls13.NewEarlySecret<hash.Hash>(sha256.New, psk);

    transcript.Write(helloClientRandom);

    {
        var got = es.ClientEarlyTrafficSecret(transcript); if (!bytes.Equal(got, clientEarlyTrafficSecret)) {
            Ꮡt.Errorf("clientEarlyTrafficSecret = %x, want %x"u8, got, clientEarlyTrafficSecret);
        }
    }
    {
        var got = tls13.TestingOnlyExporterSecret(es.EarlyExporterMasterSecret(transcript)); if (!bytes.Equal(got, earlyExporterMasterSecret)) {
            Ꮡt.Errorf("earlyExporterMasterSecret = %x, want %x"u8, got, earlyExporterMasterSecret);
        }
    }

    var hs = es.HandshakeSecret(dhe);

    transcript.Write(helloServerRandom);

    {
        var got = hs.ClientHandshakeTrafficSecret(transcript); if (!bytes.Equal(got, clientHandshakeTrafficSecret)) {
            Ꮡt.Errorf("clientHandshakeTrafficSecret = %x, want %x"u8, got, clientHandshakeTrafficSecret);
        }
    }
    {
        var got = hs.ServerHandshakeTrafficSecret(transcript); if (!bytes.Equal(got, serverHandshakeTrafficSecret)) {
            Ꮡt.Errorf("serverHandshakeTrafficSecret = %x, want %x"u8, got, serverHandshakeTrafficSecret);
        }
    }

    var ms = hs.MasterSecret();

    transcript.Write(finishedServerRandom);

    {
        var got = ms.ClientApplicationTrafficSecret(transcript); if (!bytes.Equal(got, clientApplicationTrafficSecret)) {
            Ꮡt.Errorf("clientApplicationTrafficSecret = %x, want %x"u8, got, clientApplicationTrafficSecret);
        }
    }
    {
        var got = ms.ServerApplicationTrafficSecret(transcript); if (!bytes.Equal(got, serverApplicationTrafficSecret)) {
            Ꮡt.Errorf("serverApplicationTrafficSecret = %x, want %x"u8, got, serverApplicationTrafficSecret);
        }
    }
    {
        var got = tls13.TestingOnlyExporterSecret(ms.ExporterMasterSecret(transcript)); if (!bytes.Equal(got, exporterMasterSecret)) {
            Ꮡt.Errorf("exporterMasterSecret = %x, want %x"u8, got, exporterMasterSecret);
        }
    }

    transcript.Write(finishedClientRandom);

    {
        var got = ms.ResumptionMasterSecret(transcript); if (!bytes.Equal(got, resumptionMasterSecret)) {
            Ꮡt.Errorf("resumptionMasterSecret = %x, want %x"u8, got, resumptionMasterSecret);
        }
    }
}

// This file contains tests derived from draft-ietf-tls-tls13-vectors-07.
internal static slice<byte> parseVector(@string v) {
    v = strings.Map((rune c) => {
        if (unicode.IsSpace(c)) {
            return -1;
        }
        return c;
    }, v);
    var parts = strings.Split(v, ":"u8);
    v = parts[len(parts) - 1];
    var (res, err) = hex.DecodeString(v);
    if (err != default!) {
        throw panic(err);
    }
    return res;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string prk32OctetsB67b7d690cC1ˢ = """
PRK (32 octets):  b6 7b 7d 69 0c c1 6c 4e 75 e5 42 13 cb 2d 37 b4
		e9 c9 12 bc de d9 10 5d 42 be fd 59 d3 91 ad 38
"""u8.Raw();
internal static readonly @string keyExpanded16Octets3fCeˢ = """
key expanded (16 octets):  3f ce 51 60 09 c2 17 27 d0 f2 e4 e8 6e
		e4 03 bc
"""u8.Raw();
internal static readonly @string ivExpanded12Octets5d313eˢ = @"iv expanded (12 octets):  5d 31 3e b2 67 12 76 ee 13 00 0b 30"u8;

public static void TestTrafficKey(ж<testing.T> Ꮡt) {
    var trafficSecret = parseVector(
        prk32OctetsB67b7d690cC1ˢ);
    var wantKey = parseVector(
        keyExpanded16Octets3fCeˢ);
    var wantIV = parseVector(
        ivExpanded12Octets5d313eˢ);

    var c = cipherSuitesTLS13[0];
    var (gotKey, gotIV) = c.trafficKey(trafficSecret);
    if (!bytes.Equal(gotKey, wantKey)) {
        Ꮡt.Errorf("cipherSuiteTLS13.trafficKey() gotKey = % x, want % x"u8, gotKey, wantKey);
    }
    if (!bytes.Equal(gotIV, wantIV)) {
        Ꮡt.Errorf("cipherSuiteTLS13.trafficKey() gotIV = % x, want % x"u8, gotIV, wantIV);
    }
}

} // end tls_internal_test_package
