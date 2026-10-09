// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// CTR AES test vectors.
// See U.S. National Institute of Standards and Technology (NIST)
// Special Publication 800-38A, ``Recommendation for Block Cipher
// Modes of Operation,'' 2001 Edition, pp. 55-58.
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using boring = go.crypto.@internal.boring_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fipsaes = go.crypto.@internal.fips140.aes_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using rand = math.rand_package;
using sort = sort_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using math;

partial class cipher_test_package {

internal static slice<byte> commonCounter = new byte[]{0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff}.slice();

// NIST SP 800-38A pp 55-58
internal static slice<cbcAESTestsᴛ1> ctrAESTests;
internal static void initᴛctrAESTests() { ctrAESTests = new cbcAESTestsᴛ1[]{
    new(
        "CTR-AES128"u8,
        commonKey128,
        commonCounter,
        commonInput,
        new byte[]{
            0x87, 0x4d, 0x61, 0x91, 0xb6, 0x20, 0xe3, 0x26, 0x1b, 0xef, 0x68, 0x64, 0x99, 0x0d, 0xb6, 0xce,
            0x98, 0x06, 0xf6, 0x6b, 0x79, 0x70, 0xfd, 0xff, 0x86, 0x17, 0x18, 0x7b, 0xb9, 0xff, 0xfd, 0xff,
            0x5a, 0xe4, 0xdf, 0x3e, 0xdb, 0xd5, 0xd3, 0x5e, 0x5b, 0x4f, 0x09, 0x02, 0x0d, 0xb0, 0x3e, 0xab,
            0x1e, 0x03, 0x1d, 0xda, 0x2f, 0xbe, 0x03, 0xd1, 0x79, 0x21, 0x70, 0xa0, 0xf3, 0x00, 0x9c, 0xee
        }.slice()
    ),
    new(
        "CTR-AES192"u8,
        commonKey192,
        commonCounter,
        commonInput,
        new byte[]{
            0x1a, 0xbc, 0x93, 0x24, 0x17, 0x52, 0x1c, 0xa2, 0x4f, 0x2b, 0x04, 0x59, 0xfe, 0x7e, 0x6e, 0x0b,
            0x09, 0x03, 0x39, 0xec, 0x0a, 0xa6, 0xfa, 0xef, 0xd5, 0xcc, 0xc2, 0xc6, 0xf4, 0xce, 0x8e, 0x94,
            0x1e, 0x36, 0xb2, 0x6b, 0xd1, 0xeb, 0xc6, 0x70, 0xd1, 0xbd, 0x1d, 0x66, 0x56, 0x20, 0xab, 0xf7,
            0x4f, 0x78, 0xa7, 0xf6, 0xd2, 0x98, 0x09, 0x58, 0x5a, 0x97, 0xda, 0xec, 0x58, 0xc6, 0xb0, 0x50
        }.slice()
    ),
    new(
        "CTR-AES256"u8,
        commonKey256,
        commonCounter,
        commonInput,
        new byte[]{
            0x60, 0x1e, 0xc3, 0x13, 0x77, 0x57, 0x89, 0xa5, 0xb7, 0xa7, 0xf5, 0x04, 0xbb, 0xf3, 0xd2, 0x28,
            0xf4, 0x43, 0xe3, 0xca, 0x4d, 0x62, 0xb5, 0x9a, 0xca, 0x84, 0xe9, 0x90, 0xca, 0xca, 0xf5, 0xc5,
            0x2b, 0x09, 0x30, 0xda, 0xa2, 0x3d, 0xe9, 0x4c, 0xe8, 0x70, 0x17, 0xba, 0x2d, 0x84, 0x98, 0x8d,
            0xdf, 0xc9, 0xc5, 0x8d, 0xb6, 0x7a, 0xad, 0xa6, 0x13, 0xc2, 0xdd, 0x08, 0x45, 0x79, 0x41, 0xa6
        }.slice()
    )
}.slice(); }

public static void TestCTR_AES(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, testCTR_AES);
}

internal static void testCTR_AES(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ctrAESTests) {
        @string test = tt.name;
        var (c, err) = aes.NewCipher(tt.key);
        if (err != default!) {
            Ꮡt.Errorf("%s: NewCipher(%d bytes) = %s"u8, test, len(tt.key), err);
            continue;
        }
        for (nint j = 0; j <= 5; j += 5) {
            var @in = tt.@in.slice(0, len(tt.@in) - j);
            var ctr = cipher.NewCTR(c, tt.iv);
            var encrypted = new slice<byte>(len(@in));
            ctr.XORKeyStream(encrypted, @in);
            {
                var @out = tt.@out.slice(0, len(@in)); if (!bytes.Equal(@out, encrypted)) {
                    Ꮡt.Errorf("%s/%d: CTR\ninpt %x\nhave %x\nwant %x"u8, test, len(@in), @in, encrypted, @out);
                }
            }
        }
        for (nint j = 0; j <= 7; j += 7) {
            var @in = tt.@out.slice(0, len(tt.@out) - j);
            var ctr = cipher.NewCTR(c, tt.iv);
            var plain = new slice<byte>(len(@in));
            ctr.XORKeyStream(plain, @in);
            {
                var @out = tt.@in.slice(0, len(@in)); if (!bytes.Equal(@out, plain)) {
                    Ꮡt.Errorf("%s/%d: CTRReader\nhave %x\nwant %x"u8, test, len(@out), plain, @out);
                }
            }
        }
        if (Ꮡt.Failed()) {
            break;
        }
    }
}

internal static (cipher.Stream genericCtr, cipher.Stream multiblockCtr) makeTestingCiphers(cipher.Block aesBlock, slice<byte> iv) {
    return (cipher.NewCTR(wrap(aesBlock), iv), cipher.NewCTR(aesBlock, iv));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object shortReadFromRandˢ = (@string)"short read from Rand"u8;

internal static slice<byte> randBytes(ж<testing.T> Ꮡt, ж<rand.Rand> Ꮡr, nint count) {
    Ꮡt.Helper();
    var buf = new slice<byte>(count);
    var (n, err) = Ꮡr.Read(buf);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (n != count) {
        Ꮡt.Fatal(shortReadFromRandˢ);
    }
    return buf;
}

internal static UntypedInt aesBlockSize => 16;

partial interface ctrAble {
    cipher.Stream NewCTR(slice<byte> iv);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object multiblockCtrSOutputDoesˢ = (@string)"multiblock CTR's output does not match generic CTR's output"u8;

// Verify that multiblock AES CTR (src/crypto/aes/ctr_*.s)
// produces the same results as generic single-block implementation.
// This test runs checks on random IV.
public static void TestCTR_AES_multiblock_random_IV(ж<testing.T> Ꮡt) {
    var r = rand.New(rand.NewSource(54321));
    var iv = randBytes(Ꮡt, r, aesBlockSize);
    const nint Size = 100;
    foreach (var (_, keySize) in new nint[]{16, 24, 32}.slice()) {
        nint keySizeΔ1 = keySize;
        var ivʗ1 = iv;
        var rʗ1 = r;
        Ꮡt.Run(fmt.Sprintf("keySize=%d"u8, keySizeΔ1), (ж<testing.T> tΔ1) => {
            var key = randBytes(tΔ1, rʗ1, keySizeΔ1);
            var (aesBlock, err) = aes.NewCipher(key);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var (genericCtr, _) = makeTestingCiphers(aesBlock, ivʗ1);
            var plaintext = randBytes(tΔ1, rʗ1, Size);
            // Generate reference ciphertext.
            var genericCiphertext = new slice<byte>(len(plaintext));
            genericCtr.XORKeyStream(genericCiphertext, plaintext);
            // Split the text in 3 parts in all possible ways and encrypt them
            // individually using multiblock implementation to catch edge cases.
            for (nint part1 = 0; part1 <= Size; part1++) {
                nint part1Δ1 = part1;
                var aesBlockʗ1 = aesBlock;
                var genericCiphertextʗ1 = genericCiphertext;
                var ivʗ2 = ivʗ1;
                var plaintextʗ1 = plaintext;
                tΔ1.Run(fmt.Sprintf("part1=%d"u8, part1Δ1), (ж<testing.T> tΔ2) => {
                    for (nint part2 = 0; part2 <= Size - part1Δ1; part2++) {
                        nint part2Δ1 = part2;
                        var aesBlockʗ2 = aesBlockʗ1;
                        var genericCiphertextʗ2 = genericCiphertextʗ1;
                        var ivʗ3 = ivʗ2;
                        var plaintextʗ2 = plaintextʗ1;
                        tΔ2.Run(fmt.Sprintf("part2=%d"u8, part2Δ1), (ж<testing.T> tΔ3) => {
                            var (_, multiblockCtr) = makeTestingCiphers(aesBlockʗ2, ivʗ3);
                            var multiblockCiphertext = new slice<byte>(len(plaintextʗ2));
                            multiblockCtr.XORKeyStream(multiblockCiphertext.slice(0, part1Δ1), plaintextʗ2.slice(0, part1Δ1));
                            multiblockCtr.XORKeyStream(multiblockCiphertext.slice(part1Δ1, part1Δ1 + part2Δ1), plaintextʗ2.slice(part1Δ1, part1Δ1 + part2Δ1));
                            multiblockCtr.XORKeyStream(multiblockCiphertext.slice(part1Δ1 + part2Δ1), plaintextʗ2.slice(part1Δ1 + part2Δ1));
                            if (!bytes.Equal(genericCiphertextʗ2, multiblockCiphertext)) {
                                tΔ3.Fatal(multiblockCtrSOutputDoesˢ);
                            }
                        });
                    }
                });
            }
        });
    }
}

internal static slice<byte> parseHex(@string str) {
    var (b, err) = hex.DecodeString(strings.ReplaceAll(str, " "u8, ""u8));
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ffFfFfFfFfFfFfFfˢ = "00 00 00 00 00 00 00 00   FF FF FF FF FF FF FF FF"u8;
private static readonly @string ffFfFfFfFfFfFfFfFfFfFfFfˢ = "FF FF FF FF FF FF FF FF   FF FF FF FF FF FF FF FF"u8;
private static readonly @string ffFfFfFfFfFfFfFf00000000ˢ = "FF FF FF FF FF FF FF FF   00 00 00 00 00 00 00 00"u8;
private static readonly @string ffFfFfFfFfFfFfFfFfFfFfFfˢ2 = "FF FF FF FF FF FF FF FF   FF FF FF FF FF FF FF fe"u8;
private static readonly @string ffFfFfFfFfFfFfFeˢ = "00 00 00 00 00 00 00 00   FF FF FF FF FF FF FF fe"u8;
private static readonly @string ffFfFfFfFfFfFfFfFfFfFfFfˢ3 = "FF FF FF FF FF FF FF FF   FF FF FF FF FF FF FF 00"u8;
private static readonly @string ffFfFfFfFfFfFf00ˢ = "00 00 00 00 00 00 00 01   FF FF FF FF FF FF FF 00"u8;
private static readonly @string ffFfFfFfFfFfFfFfˢ2 = "00 00 00 00 00 00 00 01   FF FF FF FF FF FF FF FF"u8;
private static readonly @string ffFfFfFfFfFfFfFeˢ2 = "00 00 00 00 00 00 00 01   FF FF FF FF FF FF FF fe"u8;

// Verify that multiblock AES CTR (src/crypto/aes/ctr_*.s)
// produces the same results as generic single-block implementation.
// This test runs checks on edge cases (IV overflows).
public static void TestCTR_AES_multiblock_overflow_IV(ж<testing.T> Ꮡt) {
    var r = rand.New(rand.NewSource(987654));
    const nint Size = 4096;
    var plaintext = randBytes(Ꮡt, r, Size);
    var ivs = new slice<byte>[]{
        parseHex(ffFfFfFfFfFfFfFfˢ),
        parseHex(ffFfFfFfFfFfFfFfFfFfFfFfˢ),
        parseHex(ffFfFfFfFfFfFfFf00000000ˢ),
        parseHex(ffFfFfFfFfFfFfFfFfFfFfFfˢ2),
        parseHex(ffFfFfFfFfFfFfFeˢ),
        parseHex(ffFfFfFfFfFfFfFfFfFfFfFfˢ3),
        parseHex(ffFfFfFfFfFfFf00ˢ),
        parseHex(ffFfFfFfFfFfFfFfˢ2),
        parseHex(ffFfFfFfFfFfFfFeˢ2),
        parseHex(ffFfFfFfFfFfFf00ˢ)
    }.slice();
    foreach (var (_, keySize) in new nint[]{16, 24, 32}.slice()) {
        nint keySizeΔ1 = keySize;
        var ivsʗ1 = ivs;
        var plaintextʗ1 = plaintext;
        var rʗ1 = r;
        Ꮡt.Run(fmt.Sprintf("keySize=%d"u8, keySizeΔ1), (ж<testing.T> tΔ1) => {
            foreach (var (_, iv) in ivsʗ1) {
                var key = randBytes(tΔ1, rʗ1, keySizeΔ1);
                var (aesBlock, err) = aes.NewCipher(key);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var aesBlockʗ1 = aesBlock;
                var ivʗ1 = iv;
                var plaintextʗ2 = plaintextʗ1;
                tΔ1.Run(fmt.Sprintf("iv=%s"u8, hex.EncodeToString(iv)), (ж<testing.T> tΔ2) => {
                    foreach (var (_, offset) in new nint[]{0, 1, 16, 1024}.slice()) {
                        nint offsetΔ1 = offset;
                        var aesBlockʗ2 = aesBlockʗ1;
                        var ivʗ2 = ivʗ1;
                        var plaintextʗ3 = plaintextʗ2;
                        tΔ2.Run(fmt.Sprintf("offset=%d"u8, offsetΔ1), (ж<testing.T> tΔ3) => {
                            var (genericCtr, multiblockCtr) = makeTestingCiphers(aesBlockʗ2, ivʗ2);
                            // Generate reference ciphertext.
                            var genericCiphertext = new slice<byte>(Size);
                            genericCtr.XORKeyStream(genericCiphertext, plaintextʗ3);
                            var multiblockCiphertext = new slice<byte>(Size);
                            multiblockCtr.XORKeyStream(multiblockCiphertext, plaintextʗ3.slice(0, offsetΔ1));
                            multiblockCtr.XORKeyStream(multiblockCiphertext.slice(offsetΔ1), plaintextʗ3.slice(offsetΔ1));
                            if (!bytes.Equal(genericCiphertext, multiblockCiphertext)) {
                                tΔ3.Fatal(multiblockCtrSOutputDoesˢ);
                            }
                        });
                    }
                });
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object xorKeyStreamAtIsNotˢ = (@string)"XORKeyStreamAt is not available in boring mode"u8;

// Check that method XORKeyStreamAt works correctly.
public static void TestCTR_AES_multiblock_XORKeyStreamAt(ж<testing.T> Ꮡt) {
    if (boring.Enabled) {
        Ꮡt.Skip(xorKeyStreamAtIsNotˢ);
    }
    var r = rand.New(rand.NewSource(12345));
    const nint Size = /* 32 * 1024 * 1024 */ 33554432;
    var plaintext = randBytes(Ꮡt, r, Size);
    foreach (var (_, keySize) in new nint[]{16, 24, 32}.slice()) {
        nint keySizeΔ1 = keySize;
        var plaintextʗ1 = plaintext;
        var rʗ1 = r;
        Ꮡt.Run(fmt.Sprintf("keySize=%d"u8, keySizeΔ1), (ж<testing.T> tΔ1) => {
            var key = randBytes(tΔ1, rʗ1, keySizeΔ1);
            var iv = randBytes(tΔ1, rʗ1, aesBlockSize);
            var (aesBlock, err) = aes.NewCipher(key);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var (genericCtr, _) = makeTestingCiphers(aesBlock, iv);
            var ctrAt = fipsaes.NewCTR(aesBlock._<ж<fipsaes.Block>>(), iv);
            // Generate reference ciphertext.
            var genericCiphertext = new slice<byte>(Size);
            genericCtr.XORKeyStream(genericCiphertext, plaintextʗ1);
            var multiblockCiphertext = new slice<byte>(Size);
            // Split the range to random slices.
            UntypedInt N = 1000;
            var boundaries = new slice<nint>(0, N + 2);
            for (nint i = 0; i < N; i++) {
                boundaries = append(boundaries, rʗ1.Intn(Size));
            }
            boundaries = append(boundaries, (nint)(0));
            boundaries = append(boundaries, Size);
            sort.Ints(boundaries);
            foreach (var (_, i) in rʗ1.Perm(N + 1)) {
                nint begin = boundaries[i];
                nint end = boundaries[i + 1];
                ctrAt.XORKeyStreamAt(
                    multiblockCiphertext.slice(begin, end),
                    plaintextʗ1.slice(begin, end),
                    (uint64)begin);
            }
            if (!bytes.Equal(genericCiphertext, multiblockCiphertext)) {
                tΔ1.Fatal(multiblockCtrSOutputDoesˢ);
            }
        });
    }
}

} // end cipher_test_package
