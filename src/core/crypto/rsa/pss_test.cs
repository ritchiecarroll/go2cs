// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bufio = bufio_package;
using bzip2 = compress.bzip2_package;
using crypto = crypto_package;
using fips140 = go.crypto.@internal.fips140_package;
using rand = go.crypto.rand_package;
using static go.crypto.rsa_package;
using sha256 = go.crypto.sha256_package;
using sha512 = go.crypto.sha512_package;
using hex = encoding.hex_package;
using big = math.big_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using compress;
using encoding;
using go.crypto;
using go.crypto.@internal;
using hash = hash_package;
using io = io_package;
using math;
using rsa = go.crypto.rsa_package;
using static go.crypto.rsa_internal_test_package;

partial class rsa_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataPssVectTxtBz2ˢ = "testdata/pss-vect.txt.bz2"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string newKeyMarkerᶜ = "START NEW KEY"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string newSignatureMarkerᶜ = "START NEW SIGNATURE"u8;

// TestPSSGolden tests all the test vectors in pss-vect.txt from
// ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-1/pkcs-1v2-1-vec.zip
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPSSGolden(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var (inFile, err) = os.Open(testdataPssVectTxtBz2ˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Failed to open input file: %s"u8, err);
        }
        var inFileʗ1 = inFile;
        defer(() => inFileʗ1.Close(), ref ᒐ);
        // The pss-vect.txt file contains RSA keys and then a series of
        // signatures. A goroutine is used to preprocess the input by merging
        // lines, removing spaces in hex values and identifying the start of
        // new keys and signature blocks.
        @string newKeyMarker = newKeyMarkerᶜ;
        @string newSignatureMarker = newSignatureMarkerᶜ;
        var values = new channel<@string>(0);
        var inFileʗ2 = inFile;
        var valuesʗ1 = values;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), valuesʗ1, ref ᒐ);
                var scanner = bufio.NewScanner(bzip2.NewReader(new rsa_test_package.os_FileжReader(inFileʗ2)));
                @string partialValue = default!;
                var lastWasValue = true;
                while (scanner.Scan()) {
                    @string line = scanner.Text();
                    switch (ᐧ) {
                    case {} when len(line) is 0: {
                        if (len(partialValue) > 0) {
                            valuesʗ1.ᐸꟷ(strings.ReplaceAll(partialValue, " "u8, ""u8));
                            partialValue = ""u8;
                            lastWasValue = true;
                        }
                        continue;
                        break;
                    }
                    case {} when strings.HasPrefix(line, "# ======"u8) && lastWasValue: {
                        valuesʗ1.ᐸꟷ(newKeyMarker);
                        lastWasValue = false;
                        break;
                    }
                    case {} when strings.HasPrefix(line, "# ------"u8) && lastWasValue: {
                        valuesʗ1.ᐸꟷ(newSignatureMarker);
                        lastWasValue = false;
                        break;
                    }
                    case {} when strings.HasPrefix(line, "#"u8): {
                        continue;
                        break;
                    }
                    default: {
                        partialValue += line;
                        break;
                    }}

                }
                {
                    var errΔ1 = scanner.Err(); if (errΔ1 != default!) {
                        throw panic(errΔ1);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        ж<rsa.PublicKey> key = default!;
        slice<byte> hashed = default!;
        crypto.Hash hash = crypto.SHA1;
        var h = hash.New();
        var opts = Ꮡ(new PSSOptions(
            SaltLength: PSSSaltLengthEqualsHash
        ));
        foreach (var marker in values) {
            var exprᴛ1 = marker;
            if (exprᴛ1 == newKeyMarker) {
                key = @new<rsa.PublicKey>();
                var (nHex, ok) = ᐸꟷ(values, ꟷ);
                if (!ok) {
                    continue;
                }
                key.Value.N = bigFromHex(nHex);
                key.Value.E = intFromHex(ᐸꟷ(values));
                for (nint i = 0; i < 6; i++) {
                    // We don't care for d, p, q, dP, dQ or qInv.
                    ᐸꟷ(values);
                }
            }
            else if (exprᴛ1 == newSignatureMarker) {
                var msg = fromHex(ᐸꟷ(values));
                ᐸꟷ(values); // skip salt
                var sig = fromHex(ᐸꟷ(values));
                h.Reset();
                h.Write(msg);
                hashed = h.Sum(hashed[..0]);
                {
                    var errΔ3 = VerifyPSS(key, hash, hashed, sig, opts); if (errΔ3 != default!) {
                        Ꮡt.Error(errΔ3);
                    }
                }
            }
            else { /* default: */
                Ꮡt.Fatalf("unknown marker: %s"u8, marker);
            }

        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestPSSOpenSSL ensures that we can verify a PSS signature from OpenSSL with
// the default options. OpenSSL sets the salt length to be maximal.
public static void TestPSSOpenSSL(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    crypto.Hash hash = crypto.SHA256;
    var h = hash.New();
    h.Write(slice<byte>("testing"u8));
    var hashed = h.Sum(default!);
    // Generated with `echo -n testing | openssl dgst -sign key.pem -sigopt rsa_padding_mode:pss -sha256 > sig`
    var sig = new byte[]{
        0x95, 0x59, 0x6f, 0xd3, 0x10, 0xa2, 0xe7, 0xa2, 0x92, 0x9d,
        0x4a, 0x07, 0x2e, 0x2b, 0x27, 0xcc, 0x06, 0xc2, 0x87, 0x2c,
        0x52, 0xf0, 0x4a, 0xcc, 0x05, 0x94, 0xf2, 0xc3, 0x2e, 0x20,
        0xd7, 0x3e, 0x66, 0x62, 0xb5, 0x95, 0x2b, 0xa3, 0x93, 0x9a,
        0x66, 0x64, 0x25, 0xe0, 0x74, 0x66, 0x8c, 0x3e, 0x92, 0xeb,
        0xc6, 0xe6, 0xc0, 0x44, 0xf3, 0xb4, 0xb4, 0x2e, 0x8c, 0x66,
        0x0a, 0x37, 0x9c, 0x69
    }.slice();
    {
        var err = VerifyPSS(test512Key.of(rsa.PrivateKey.ᏑPublicKey), hash, hashed, sig, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestPSSNilOpts(ж<testing.T> Ꮡt) {
    crypto.Hash hash = crypto.SHA256;
    var h = hash.New();
    h.Write(slice<byte>("testing"u8));
    var hashed = h.Sum(default!);
    SignPSS(rand.Reader, rsaPrivateKey, hash, hashed, nil);
}

[GoType("dyn")] internal partial struct TestPSSSigning_type {
    internal nint signSaltLength, verifySaltLength;
    internal bool good, fipsGood;
}

public static void TestPSSSigning(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// In FIPS mode, PSSSaltLengthAuto is capped at PSSSaltLengthEqualsHash.
    slice<TestPSSSigning_type> saltLengthCombinations = new TestPSSSigning_type[]{
        new(PSSSaltLengthAuto, PSSSaltLengthAuto, true, true),
        new(PSSSaltLengthEqualsHash, PSSSaltLengthAuto, true, true),
        new(PSSSaltLengthEqualsHash, PSSSaltLengthEqualsHash, true, true),
        new(PSSSaltLengthEqualsHash, 8, false, false),
        new(8, 8, true, true),
        new(8, PSSSaltLengthAuto, true, true),
        new(42, PSSSaltLengthAuto, true, true),
        new(PSSSaltLengthAuto, PSSSaltLengthEqualsHash, false, true),
        new(PSSSaltLengthAuto, 106, true, false),
        new(PSSSaltLengthAuto, 20, false, true),
        new(PSSSaltLengthAuto, -2, false, false)
    }.slice();
    crypto.Hash hash = crypto.SHA1;
    var h = hash.New();
    h.Write(slice<byte>("testing"u8));
    var hashed = h.Sum(default!);
    ref var opts = ref heap(new rsa.PSSOptions(), out var Ꮡopts);
    foreach (var (i, test) in saltLengthCombinations) {
        opts.SaltLength = test.signSaltLength;
        var (sig, err) = SignPSS(rand.Reader, rsaPrivateKey, hash, hashed, Ꮡopts);
        if (err != default!) {
            Ꮡt.Errorf("#%d: error while signing: %s"u8, i, err);
            continue;
        }
        opts.SaltLength = test.verifySaltLength;
        err = VerifyPSS(rsaPrivateKey.of(rsa.PrivateKey.ᏑPublicKey), hash, hashed, sig, Ꮡopts);
        var good = test.good;
        if (fips140.Enabled) {
            good = test.fipsGood;
        }
        if ((err == default!) != good) {
            Ꮡt.Errorf("#%d: bad result, wanted: %t, got: %s"u8, i, test.good, err);
        }
    }
}

public static void TestPSS513(ж<testing.T> Ꮡt) {
    // See Issue 42741, and separately, RFC 8017: "Note that the octet length of
    // EM will be one less than k if modBits - 1 is divisible by 8 and equal to
    // k otherwise, where k is the length in octets of the RSA modulus n."
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    var (key, err) = GenerateKey(rand.Reader, 513);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var digest = sha256.Sum256(slice<byte>("message"u8));
    (var signature, err) = key.Sign(rand.Reader, digest[..], new rsa_test_package.rsa_PSSOptionsжSignerOpts(Ꮡ(new PSSOptions(
        SaltLength: PSSSaltLengthAuto,
        Hash: crypto.SHA256
    ))));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = VerifyPSS(key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, digest[..], signature, nil);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

internal static ж<bigꓸInt> bigFromHex(@string hex) {
    var (n, ok) = @new<bigꓸInt>().SetString(hex, 16);
    if (!ok) {
        throw panic("bad hex: " + hex);
    }
    return n;
}

internal static nint intFromHex(@string hex) {
    var (i, err) = strconv.ParseInt(hex, 16, 32);
    if (err != default!) {
        throw panic(err);
    }
    return (nint)i;
}

internal static slice<byte> fromHex(@string hexStr) {
    var (s, err) = hex.DecodeString(hexStr);
    if (err != default!) {
        throw panic(err);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cryptoRsaInvalidPssSaltˢ = (@string)"crypto/rsa: invalid PSS salt length"u8;
internal static readonly object verifyPSSUnexpectedˢ = (@string)"VerifyPSS unexpected success"u8;

public static void TestInvalidPSSSaltLength(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    var (key, err) = GenerateKey(rand.Reader, 245);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var digest = sha256.Sum256(slice<byte>("message"u8));
    {
        var (_, errΔ1) = SignPSS(rand.Reader, key, crypto.SHA256, digest[..], Ꮡ(new PSSOptions(
            SaltLength: -2,
            Hash: crypto.SHA256
        ))); if (errΔ1.Error() != "crypto/rsa: invalid PSS salt length"u8) {
            Ꮡt.Fatalf("SignPSS unexpected error: got %v, want %v"u8, errΔ1, cryptoRsaInvalidPssSaltˢ);
        }
    }
    // We don't check the specific error here, because crypto/rsa and crypto/internal/boring
    // return different errors, so we just check that _an error_ was returned.
    {
        var errΔ2 = VerifyPSS(key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, new byte[]{1, 2, 3}.slice(), new slice<byte>(31), Ꮡ(new PSSOptions(
            SaltLength: -2
        ))); if (errΔ2 == default!) {
            Ꮡt.Fatal(verifyPSSUnexpectedˢ);
        }
    }
}

public static void TestHashOverride(ж<testing.T> Ꮡt) {
    var digest = sha512.Sum512(slice<byte>("message"u8));
    // opts.Hash overrides the passed hash argument.
    var (sig, err) = SignPSS(rand.Reader, test2048Key, crypto.SHA256, digest[..], Ꮡ(new PSSOptions(Hash: crypto.SHA512)));
    if (err != default!) {
        Ꮡt.Fatalf("SignPSS unexpected error: got %v, want nil"u8, err);
    }
    // VerifyPSS has the inverse behavior, opts.Hash is always ignored, check this is true.
    {
        var errΔ1 = VerifyPSS(test2048Key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA512, digest[..], sig, Ꮡ(new PSSOptions(Hash: crypto.SHA256))); if (errΔ1 != default!) {
            Ꮡt.Fatalf("VerifyPSS unexpected error: got %v, want nil"u8, errΔ1);
        }
    }
}

} // end rsa_test_package
