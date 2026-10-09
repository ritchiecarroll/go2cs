// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using boring = go.crypto.@internal.boring_package;
using fips140 = go.crypto.@internal.fips140_package;
using pbkdf2 = go.crypto.pbkdf2_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using hash = hash_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal;

partial class pbkdf2_test_package {

partial struct testVector {
    internal @string password;
    internal @string salt;
    internal nint iter;
    internal slice<byte> output;
}

// // This one takes too long
// {
// 	"password",
// 	"salt",
// 	16777216,
// 	[]byte{
// 		0xee, 0xfe, 0x3d, 0x61, 0xcd, 0x4d, 0xa4, 0xe4,
// 		0xe9, 0x94, 0x5b, 0x3d, 0x6b, 0xa2, 0x15, 0x8c,
// 		0x26, 0x34, 0xe9, 0x84,
// 	},
// },
// Test vectors from RFC 6070, http://tools.ietf.org/html/rfc6070
internal static slice<testVector> sha1TestVectors = new testVector[]{
    new(
        "password"u8,
        "salt"u8,
        1,
        new byte[]{
            0x0c, 0x60, 0xc8, 0x0f, 0x96, 0x1f, 0x0e, 0x71,
            0xf3, 0xa9, 0xb5, 0x24, 0xaf, 0x60, 0x12, 0x06,
            0x2f, 0xe0, 0x37, 0xa6
        }.slice()
    ),
    new(
        "password"u8,
        "salt"u8,
        2,
        new byte[]{
            0xea, 0x6c, 0x01, 0x4d, 0xc7, 0x2d, 0x6f, 0x8c,
            0xcd, 0x1e, 0xd9, 0x2a, 0xce, 0x1d, 0x41, 0xf0,
            0xd8, 0xde, 0x89, 0x57
        }.slice()
    ),
    new(
        "password"u8,
        "salt"u8,
        4096,
        new byte[]{
            0x4b, 0x00, 0x79, 0x01, 0xb7, 0x65, 0x48, 0x9a,
            0xbe, 0xad, 0x49, 0xd9, 0x26, 0xf7, 0x21, 0xd0,
            0x65, 0xa4, 0x29, 0xc1
        }.slice()
    ),
    new(
        "passwordPASSWORDpassword"u8,
        "saltSALTsaltSALTsaltSALTsaltSALTsalt"u8,
        4096,
        new byte[]{
            0x3d, 0x2e, 0xec, 0x4f, 0xe4, 0x1c, 0x84, 0x9b,
            0x80, 0xc8, 0xd8, 0x36, 0x62, 0xc0, 0xe4, 0x4a,
            0x8b, 0x29, 0x1a, 0x96, 0x4c, 0xf2, 0xf0, 0x70,
            0x38
        }.slice()
    ),
    new(
        "pass\u0000word"u8,
        "sa\u0000lt"u8,
        4096,
        new byte[]{
            0x56, 0xfa, 0x6a, 0xa7, 0x55, 0x48, 0x09, 0x9d,
            0xcc, 0x37, 0xd7, 0xf0, 0x34, 0x25, 0xe0, 0xc3
        }.slice()
    )
}.slice();

// Test vectors from
// http://stackoverflow.com/questions/5130513/pbkdf2-hmac-sha2-test-vectors
internal static slice<testVector> sha256TestVectors = new testVector[]{
    new(
        "password"u8,
        "salt"u8,
        1,
        new byte[]{
            0x12, 0x0f, 0xb6, 0xcf, 0xfc, 0xf8, 0xb3, 0x2c,
            0x43, 0xe7, 0x22, 0x52, 0x56, 0xc4, 0xf8, 0x37,
            0xa8, 0x65, 0x48, 0xc9
        }.slice()
    ),
    new(
        "password"u8,
        "salt"u8,
        2,
        new byte[]{
            0xae, 0x4d, 0x0c, 0x95, 0xaf, 0x6b, 0x46, 0xd3,
            0x2d, 0x0a, 0xdf, 0xf9, 0x28, 0xf0, 0x6d, 0xd0,
            0x2a, 0x30, 0x3f, 0x8e
        }.slice()
    ),
    new(
        "password"u8,
        "salt"u8,
        4096,
        new byte[]{
            0xc5, 0xe4, 0x78, 0xd5, 0x92, 0x88, 0xc8, 0x41,
            0xaa, 0x53, 0x0d, 0xb6, 0x84, 0x5c, 0x4c, 0x8d,
            0x96, 0x28, 0x93, 0xa0
        }.slice()
    ),
    new(
        "passwordPASSWORDpassword"u8,
        "saltSALTsaltSALTsaltSALTsaltSALTsalt"u8,
        4096,
        new byte[]{
            0x34, 0x8c, 0x89, 0xdb, 0xcb, 0xd3, 0x2b, 0x2f,
            0x32, 0xd8, 0x14, 0xb8, 0x11, 0x6e, 0x84, 0xcf,
            0x2b, 0x17, 0x34, 0x7e, 0xbc, 0x18, 0x00, 0x18,
            0x1c
        }.slice()
    ),
    new(
        "pass\u0000word"u8,
        "sa\u0000lt"u8,
        4096,
        new byte[]{
            0x89, 0xb6, 0x9d, 0x05, 0x16, 0xf8, 0x29, 0x89,
            0x3c, 0x69, 0x62, 0x26, 0x65, 0x0a, 0x86, 0x87
        }.slice()
    )
}.slice();

internal static void testHash(ж<testing.T> Ꮡt, Func<hash.Hash> h, @string hashName, slice<testVector> vectors) {
    foreach (var (i, v) in vectors) {
        var (o, err) = pbkdf2.Key(h, v.password, slice<byte>(v.salt), v.iter, len(v.output));
        if (err != default!) {
            Ꮡt.Error(err);
        }
        if (!bytes.Equal(o, v.output)) {
            Ꮡt.Errorf("%s %d: expected %x, got %x"u8, hashName, i, v.output, o);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sha1ˢ = "SHA1"u8;

public static void TestWithHMACSHA1(ж<testing.T> Ꮡt) {
    testHash(Ꮡt, sha1.New, sha1ˢ, sha1TestVectors);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sha256ˢ = "SHA256"u8;

public static void TestWithHMACSHA256(ж<testing.T> Ꮡt) {
    testHash(Ꮡt, sha256.New, sha256ˢ, sha256TestVectors);
}

internal static uint8 sink;

internal static void benchmark(ж<testing.B> Ꮡb, Func<hash.Hash> h) {
    ref var b = ref Ꮡb.DerefOrNull();

    error err = default!;
    var password = new slice<byte>(h().Size());
    var salt = new slice<byte>(8);
    for (nint i = 0; i < b.N; i++) {
        (password, err) = pbkdf2.Key(h, ((@string)password), salt, 4096, len(password));
        if (err != default!) {
            Ꮡb.Error(err);
        }
    }
    sink += password[0];
}

public static void BenchmarkHMACSHA1(ж<testing.B> Ꮡb) {
    benchmark(Ꮡb, sha1.New);
}

public static void BenchmarkHMACSHA256(ж<testing.B> Ꮡb) {
    benchmark(Ꮡb, sha256.New);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object inBoringCryptoModePbkdf2ˢ = (@string)"in BoringCrypto mode PBKDF2 is not from the Go FIPS module"u8;
private static readonly @string passwordˢ = "password"u8;
private static readonly object fipsServiceIndicatorˢ = (@string)"FIPS service indicator should be set"u8;
private static readonly object fipsServiceIndicatorˢ2 = (@string)"FIPS service indicator should not be set"u8;

public static void TestPBKDF2ServiceIndicator(ж<testing.T> Ꮡt) {
    if (boring.Enabled) {
        Ꮡt.Skip(inBoringCryptoModePbkdf2ˢ);
    }
    var goodSalt = new byte[]{0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10}.slice();
    fips140.ResetServiceIndicator();
    var (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, goodSalt, 1, 32);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (!fips140.ServiceIndicator()) {
        Ꮡt.Error(fipsServiceIndicatorˢ);
    }
    // Salt too short
    fips140.ResetServiceIndicator();
    (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, goodSalt[..8], 1, 32);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (fips140.ServiceIndicator()) {
        Ꮡt.Error(fipsServiceIndicatorˢ2);
    }
    // Key length too short
    fips140.ResetServiceIndicator();
    (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, goodSalt, 1, 10);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (fips140.ServiceIndicator()) {
        Ꮡt.Error(fipsServiceIndicatorˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object cannotBeReplicatedOnˢ = (@string)"cannot be replicated on platforms where int is 31 bits"u8;
private static readonly object expectedPbkdf2KeyToFailˢ = (@string)"expected pbkdf2.Key to fail with extremely large keyLength"u8;

public static void TestMaxKeyLength(ж<testing.T> Ꮡt) {
    // This error cannot be triggered on platforms where int is 31 bits (i.e.
    // 32-bit platforms), since the max value for keyLength is 1<<31-1 and
    // 1<<31-1 * hLen will always be less than 1<<32-1 * hLen.
    var keySize = (int64)(9223372036854775807L);
    if ((int64)(nint)keySize != keySize) {
        Ꮡt.Skip(cannotBeReplicatedOnˢ);
    }
    var (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, slice<byte>("salt"u8), 1, (nint)keySize);
    if (err == default!) {
        Ꮡt.Fatal(expectedPbkdf2KeyToFailˢ);
    }
    keySize = 141733920735L;
    (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, slice<byte>("salt"u8), 1, (nint)keySize);
    if (err == default!) {
        Ꮡt.Fatal(expectedPbkdf2KeyToFailˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object expectedPbkdf2KeyToFailˢ2 = (@string)"expected pbkdf2.Key to fail with zero keyLength"u8;
private static readonly object expectedPbkdf2KeyToFailˢ3 = (@string)"expected pbkdf2.Key to fail with negative keyLength"u8;

public static void TestZeroKeyLength(ж<testing.T> Ꮡt) {
    var (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, slice<byte>("salt"u8), 1, 0);
    if (err == default!) {
        Ꮡt.Fatal(expectedPbkdf2KeyToFailˢ2);
    }
    (_, err) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, slice<byte>("salt"u8), 1, -1);
    if (err == default!) {
        Ꮡt.Fatal(expectedPbkdf2KeyToFailˢ3);
    }
}

} // end pbkdf2_test_package
