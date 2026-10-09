// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using cipher = go.crypto.cipher_package;
using ecdh = go.crypto.ecdh_package;
using rand = go.crypto.rand_package;
using sha256 = go.crypto.sha256_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using chacha20 = vendor.golang.org.x.crypto.chacha20_package;
using encoding;
using fs = go.io.fs_package;
using go.@internal;
using go.crypto;
using go.os;
using path;
using vendor.golang.org.x.crypto;
using ꓸꓸꓸstring = Span<@string>;

partial class ecdh_test_package {

// Check that PublicKey and PrivateKey implement the interfaces documented in
// crypto.PublicKey and crypto.PrivateKey.

partial interface _ᴛ1 /*dyn*/ {
    bool Equal(cryptoꓸPublicKey x);
}
internal static _ᴛ1 _ᴛ1ʗ = new go.crypto.ecdh_package.ΔPublicKeyж_ᴛ1(Ꮡ(new go.crypto.ecdh_package.ΔPublicKey(nil)));


partial interface _ᴛ2 /*dyn*/ {
    cryptoꓸPublicKey Public();
    bool Equal(cryptoꓸPrivateKey x);
}
internal static _ᴛ2 _ᴛ2ʗ = new ecdh.PrivateKeyж_ᴛ2(Ꮡ(new ecdh.PrivateKey(nil)));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object encodedAndDecodedPublicˢ = (@string)"encoded and decoded public keys are different"u8;
private static readonly object encodedAndDecodedPrivateˢ = (@string)"encoded and decoded private keys are different"u8;
private static readonly object twoEcdhComputationsCameˢ = (@string)"two ECDH computations came out different"u8;

public static void TestECDH(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, (ж<testing.T> tΔ1, ecdhꓸCurve curve) => {
        var (aliceKey, err) = curve.GenerateKey(rand.Reader);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        (var bobKey, err) = curve.GenerateKey(rand.Reader);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        (var alicePubKey, err) = curve.NewPublicKey(aliceKey.PublicKey().Bytes());
        if (err != default!) {
            tΔ1.Error(err);
        }
        if (!bytes.Equal(aliceKey.PublicKey().Bytes(), alicePubKey.Bytes())) {
            tΔ1.Error(encodedAndDecodedPublicˢ);
        }
        if (!aliceKey.PublicKey().Equal(alicePubKey.OrTypedNil())) {
            tΔ1.Error(encodedAndDecodedPublicˢ);
        }
        (var alicePrivKey, err) = curve.NewPrivateKey(aliceKey.Bytes());
        if (err != default!) {
            tΔ1.Error(err);
        }
        if (!bytes.Equal(aliceKey.Bytes(), alicePrivKey.Bytes())) {
            tΔ1.Error(encodedAndDecodedPrivateˢ);
        }
        if (!aliceKey.Equal(alicePrivKey.OrTypedNil())) {
            tΔ1.Error(encodedAndDecodedPrivateˢ);
        }
        (var bobSecret, err) = bobKey.ECDH(aliceKey.PublicKey());
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        (var aliceSecret, err) = aliceKey.ECDH(bobKey.PublicKey());
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        if (!bytes.Equal(bobSecret, aliceSecret)) {
            tΔ1.Error(twoEcdhComputationsCameˢ);
        }
    });
}

partial struct countingReader {
    internal io.Reader r;
    internal nint n;
}

internal static (nint, error) Read(this ref countingReader r, slice<byte> p) {
    var (n, err) = r.r.Read(p);
    r.n += n;
    return (n, err);
}

public static void TestGenerateKey(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, (ж<testing.T> tΔ1, ecdhꓸCurve curve) => {
        var r = Ꮡ(new countingReader(r: rand.Reader));
        var (k, err) = curve.GenerateKey(new countingReaderжReader(r));
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        // GenerateKey does rejection sampling. If the masking works correctly,
        // the probability of a rejection is 1-ord(G)/2^ceil(log2(ord(G))),
        // which for all curves is small enough (at most 2^-32, for P-256) that
        // a bit flip is more likely to make this test fail than bad luck.
        // Account for the extra MaybeReadByte byte, too.
        {
            nint got = r.Value.n;
            nint expected = len(k.Bytes()) + 1; if (got > expected) {
                tΔ1.Errorf("expected GenerateKey to consume at most %v bytes, got %v"u8, expected, got);
            }
        }
    });
}

// NIST vectors from CAVS 14.1, ECC CDH Primitive (SP800-56A).
// For some reason all field elements in the test vector (both scalars and
// base field elements), but not the shared secret output, have two extra
// leading zero bytes (which in big-endian are irrelevant). Removed here.
// X25519 test vector from RFC 7748, Section 6.1.

partial struct vectorsᴛ1 /*dyn*/ {
    public @string PrivateKey, PublicKey;
    public @string PeerPublicKey;
    public @string SharedSecret;
}
internal static map<ecdhꓸCurve, vectorsᴛ1> vectors = new map<ecdhꓸCurve, vectorsᴛ1>{
    [ecdh.P256()] = new(
        PrivateKey: "7d7dc5f71eb29ddaf80d6214632eeae03d9058af1fb6d22ed80badb62bc1a534"u8,
        PublicKey: "04ead218590119e8876b29146ff89ca61770c4edbbf97d38ce385ed281d8a6b230"u8 + "28af61281fd35e2fa7002523acc85a429cb06ee6648325389f59edfce1405141"u8,
        PeerPublicKey: "04700c48f77f56584c5cc632ca65640db91b6bacce3a4df6b42ce7cc838833d287"u8 + "db71e509e3fd9b060ddb20ba5c51dcc5948d46fbf640dfe0441782cab85fa4ac"u8,
        SharedSecret: "46fc62106420ff012e54a434fbdd2d25ccc5852060561e68040dd7778997bd7b"u8
    ),
    [ecdh.P384()] = new(
        PrivateKey: "3cc3122a68f0d95027ad38c067916ba0eb8c38894d22e1b15618b6818a661774ad463b205da88cf699ab4d43c9cf98a1"u8,
        PublicKey: "049803807f2f6d2fd966cdd0290bd410c0190352fbec7ff6247de1302df86f25d34fe4a97bef60cff548355c015dbb3e5f"u8 + "ba26ca69ec2f5b5d9dad20cc9da711383a9dbe34ea3fa5a2af75b46502629ad54dd8b7d73a8abb06a3a3be47d650cc99"u8,
        PeerPublicKey: "04a7c76b970c3b5fe8b05d2838ae04ab47697b9eaf52e764592efda27fe7513272734466b400091adbf2d68c58e0c50066"u8 + "ac68f19f2e1cb879aed43a9969b91a0839c4c38a49749b661efedf243451915ed0905a32b060992b468c64766fc8437a"u8,
        SharedSecret: "5f9d29dc5e31a163060356213669c8ce132e22f57c9a04f40ba7fcead493b457e5621e766c40a2e3d4d6a04b25e533f1"u8
    ),
    [ecdh.P521()] = new(
        PrivateKey: "017eecc07ab4b329068fba65e56a1f8890aa935e57134ae0ffcce802735151f4eac6564f6ee9974c5e6887a1fefee5743ae2241bfeb95d5ce31ddcb6f9edb4d6fc47"u8,
        PublicKey: "0400602f9d0cf9e526b29e22381c203c48a886c2b0673033366314f1ffbcba240ba42f4ef38a76174635f91e6b4ed34275eb01c8467d05ca80315bf1a7bbd945f550a5"u8 + "01b7c85f26f5d4b2d7355cf6b02117659943762b6d1db5ab4f1dbc44ce7b2946eb6c7de342962893fd387d1b73d7a8672d1f236961170b7eb3579953ee5cdc88cd2d"u8,
        PeerPublicKey: "0400685a48e86c79f0f0875f7bc18d25eb5fc8c0b07e5da4f4370f3a9490340854334b1e1b87fa395464c60626124a4e70d0f785601d37c09870ebf176666877a2046d"u8 + "01ba52c56fc8776d9e8f5db4f0cc27636d0b741bbe05400697942e80b739884a83bde99e0f6716939e632bc8986fa18dccd443a348b6c3e522497955a4f3c302f676"u8,
        SharedSecret: "005fc70477c3e63bc3954bd0df3ea0d1f41ee21746ed95fc5e1fdf90930d5e136672d72cc770742d1711c3c3a4c334a0ad9759436a4d3c5bf6e74b9578fac148c831"u8
    ),
    [ecdh.X25519()] = new(
        PrivateKey: "77076d0a7318a57d3c16c17251b26645df4c2f87ebc0992ab177fba51db92c2a"u8,
        PublicKey: "8520f0098930a754748b7ddcb43ef75a0dbf3a0d26381af4eba4a98eaa9b4e6a"u8,
        PeerPublicKey: "de9edb7d7b7dc1b4d35b61c2ece435373f8343c85b78674dadfc7e146f882b4f"u8,
        SharedSecret: "4a5d9d5ba4ce2de1728e3bf480350f25e07e21c947d19e3376f09b3c1e161742"u8
    )
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object publicKeyDerivedFromTheˢ = (@string)"public key derived from the private key does not match"u8;

public static void TestVectors(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, (ж<testing.T> tΔ1, ecdhꓸCurve curve) => {
        var v = vectors[curve];
        var (key, err) = curve.NewPrivateKey(hexDecode(tΔ1, v.PrivateKey));
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        if (!bytes.Equal(key.PublicKey().Bytes(), hexDecode(tΔ1, v.PublicKey))) {
            tΔ1.Error(publicKeyDerivedFromTheˢ);
        }
        (var peer, err) = curve.NewPublicKey(hexDecode(tΔ1, v.PeerPublicKey));
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        (var secret, err) = key.ECDH(peer);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        if (!bytes.Equal(secret, hexDecode(tΔ1, v.SharedSecret))) {
            tΔ1.Errorf("shared secret does not match: %x %x %s %x"u8, secret, sha256.Sum256(secret), v.SharedSecret,
                sha256.Sum256(hexDecode(tΔ1, v.SharedSecret)));
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object invalidHexStringˢ = (@string)"invalid hex string:"u8;

internal static slice<byte> hexDecode(ж<testing.T> Ꮡt, @string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        Ꮡt.Fatal(invalidHexStringˢ, s);
    }
    return b;
}

public static void TestString(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, (ж<testing.T> tΔ1, ecdhꓸCurve curve) => {
        @string s = fmt.Sprintf("%s"u8, curve);
        if (s[..1] != "P" && s[..1] != "X") {
            tΔ1.Errorf("unexpected Curve string encoding: %q"u8, s);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string identityPointˢ = "identity point"u8;
private static readonly @string lowOrderPointˢ = "low order point"u8;

public static void TestX25519Failure(ж<testing.T> Ꮡt) {
    var identity = hexDecode(Ꮡt, "0000000000000000000000000000000000000000000000000000000000000000"u8);
    var lowOrderPoint = hexDecode(Ꮡt, "e0eb7a7c3b41b8ae1656e3faf19fc46ada098deb9c32b1fd866205165f49b800"u8);
    var randomScalar = new slice<byte>(32);
    rand.Read(randomScalar);
    var identityʗ1 = identity;
    var randomScalarʗ1 = randomScalar;
    Ꮡt.Run(identityPointˢ, (ж<testing.T> tΔ1) => {
        testX25519Failure(tΔ1, randomScalarʗ1, identityʗ1);
    });
    var lowOrderPointʗ1 = lowOrderPoint;
    var randomScalarʗ2 = randomScalar;
    Ꮡt.Run(lowOrderPointˢ, (ж<testing.T> tΔ2) => {
        testX25519Failure(tΔ2, randomScalarʗ2, lowOrderPointʗ1);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object expectedEcdhErrorˢ = (@string)"expected ECDH error"u8;

internal static void testX25519Failure(ж<testing.T> Ꮡt, slice<byte> @private, slice<byte> @public) {
    var (priv, err) = ecdh.X25519().NewPrivateKey(@private);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var pub, err) = ecdh.X25519().NewPublicKey(@public);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var secret, err) = priv.ECDH(pub);
    if (err == default!) {
        Ꮡt.Error(expectedEcdhErrorˢ);
    }
    if (secret != default!) {
        Ꮡt.Errorf("unexpected ECDH output: %x"u8, secret);
    }
}

// Bad lengths.
// Zero.
// Order of the curve and above.
// Bad lengths.
// Zero.
// Order of the curve and above.
// Bad lengths.
// Zero.
// Order of the curve and above.
// X25519 only rejects bad lengths.
internal static map<ecdhꓸCurve, slice<@string>> invalidPrivateKeys = new map<ecdhꓸCurve, slice<@string>>{
    [ecdh.P256()] = new @string[]{
        ""u8,
        "01"u8,
        "01010101010101010101010101010101010101010101010101010101010101"u8,
        "000101010101010101010101010101010101010101010101010101010101010101"u8,
        strings.Repeat("01"u8, 200),
        "0000000000000000000000000000000000000000000000000000000000000000"u8,
        "ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551"u8,
        "ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632552"u8,
        "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8}.slice(),
    [ecdh.P384()] = new @string[]{
        ""u8,
        "01"u8,
        "0101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101"u8,
        "00010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101"u8,
        strings.Repeat("01"u8, 200),
        "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8,
        "ffffffffffffffffffffffffffffffffffffffffffffffffc7634d81f4372ddf581a0db248b0a77aecec196accc52973"u8,
        "ffffffffffffffffffffffffffffffffffffffffffffffffc7634d81f4372ddf581a0db248b0a77aecec196accc52974"u8,
        "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8}.slice(),
    [ecdh.P521()] = new @string[]{
        ""u8,
        "01"u8,
        "0101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101"u8,
        "00010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101"u8,
        strings.Repeat("01"u8, 200),
        "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8,
        "01fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffa51868783bf2f966b7fcc0148f709a5d03bb5c9b8899c47aebb6fb71e91386409"u8,
        "01fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffa51868783bf2f966b7fcc0148f709a5d03bb5c9b8899c47aebb6fb71e9138640a"u8,
        "11fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffa51868783bf2f966b7fcc0148f709a5d03bb5c9b8899c47aebb6fb71e91386409"u8,
        "03fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff4a30d0f077e5f2cd6ff980291ee134ba0776b937113388f5d76df6e3d2270c812"u8}.slice(),
    [ecdh.X25519()] = new @string[]{
        ""u8,
        "01"u8,
        "01010101010101010101010101010101010101010101010101010101010101"u8,
        "000101010101010101010101010101010101010101010101010101010101010101"u8,
        strings.Repeat("01"u8, 200)}.slice()
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object privateKeyWasNotNilOnˢ = (@string)"PrivateKey was not nil on error"u8;
private static readonly @string boringcryptoˢ = "boringcrypto"u8;

public static void TestNewPrivateKey(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, (ж<testing.T> tΔ1, ecdhꓸCurve curve) => {
        foreach (var (_, input) in invalidPrivateKeys[curve]) {
            var (k, err) = curve.NewPrivateKey(hexDecode(tΔ1, input));
            if (err == default!){
                tΔ1.Errorf("unexpectedly accepted %q"u8, input);
            } else 
            if (k != nil){
                tΔ1.Error(privateKeyWasNotNilOnˢ);
            } else 
            if (strings.Contains(err.Error(), boringcryptoˢ)) {
                tΔ1.Errorf("boringcrypto error leaked out: %v"u8, err);
            }
        }
    });
}

// Bad lengths.
// Infinity.
// Compressed encodings.
// Points not on the curve.
// Bad lengths.
// Infinity.
// Compressed encodings.
// Points not on the curve.
// Bad lengths.
// Infinity.
// Compressed encodings.
// Points not on the curve.
internal static map<ecdhꓸCurve, slice<@string>> invalidPublicKeys = new map<ecdhꓸCurve, slice<@string>>{
    [ecdh.P256()] = new @string[]{
        ""u8,
        "04"u8,
        strings.Repeat("04"u8, 200),
        "00"u8,
        "036b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"u8,
        "02e2534a3532d08fbba02dde659ee62bd0031fe2db785596ef509302446b030852"u8,
        "046b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c2964fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f6"u8,
        "0400000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8}.slice(),
    [ecdh.P384()] = new @string[]{
        ""u8,
        "04"u8,
        strings.Repeat("04"u8, 200),
        "00"u8,
        "03aa87ca22be8b05378eb1c71ef320ad746e1d3b628ba79b9859f741e082542a385502f25dbf55296c3a545e3872760ab7"u8,
        "0208d999057ba3d2d969260045c55b97f089025959a6f434d651d207d19fb96e9e4fe0e86ebe0e64f85b96a9c75295df61"u8,
        "04aa87ca22be8b05378eb1c71ef320ad746e1d3b628ba79b9859f741e082542a385502f25dbf55296c3a545e3872760ab73617de4a96262c6f5d9e98bf9292dc29f8f41dbd289a147ce9da3113b5f0b8c00a60b1ce1d7e819d7a431d7c90ea0e60"u8,
        "04000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8}.slice(),
    [ecdh.P521()] = new @string[]{
        ""u8,
        "04"u8,
        strings.Repeat("04"u8, 200),
        "00"u8,
        "030035b5df64ae2ac204c354b483487c9070cdc61c891c5ff39afc06c5d55541d3ceac8659e24afe3d0750e8b88e9f078af066a1d5025b08e5a5e2fbc87412871902f3"u8,
        "0200c6858e06b70404e9cd9e3ecb662395b4429c648139053fb521f828af606b4d3dbaa14b5e77efe75928fe1dc127a2ffa8de3348b3c1856a429bf97e7e31c2e5bd66"u8,
        "0400c6858e06b70404e9cd9e3ecb662395b4429c648139053fb521f828af606b4d3dbaa14b5e77efe75928fe1dc127a2ffa8de3348b3c1856a429bf97e7e31c2e5bd66011839296a789a3bc0045c8a5fb42c7d1bd998f54449579b446817afbd17273e662c97ee72995ef42640c550b9013fad0761353c7086a272c24088be94769fd16651"u8,
        "04000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8}.slice(),
    [ecdh.X25519()] = new @string[]{}.slice()
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object publicKeyWasNotNilOnˢ = (@string)"PublicKey was not nil on error"u8;

public static void TestNewPublicKey(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, (ж<testing.T> tΔ1, ecdhꓸCurve curve) => {
        foreach (var (_, input) in invalidPublicKeys[curve]) {
            var (k, err) = curve.NewPublicKey(hexDecode(tΔ1, input));
            if (err == default!){
                tΔ1.Errorf("unexpectedly accepted %q"u8, input);
            } else 
            if (k != nil){
                tΔ1.Error(publicKeyWasNotNilOnˢ);
            } else 
            if (strings.Contains(err.Error(), boringcryptoˢ)) {
                tΔ1.Errorf("boringcrypto error leaked out: %v"u8, err);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string p256ˢ = "P256"u8;
private static readonly @string p384ˢ = "P384"u8;
private static readonly @string p521ˢ = "P521"u8;
private static readonly @string x25519ˢ = "X25519"u8;

internal static void testAllCurves(ж<testing.T> Ꮡt, Action<ж<testing.T>, ecdhꓸCurve> f) {
    Ꮡt.Run(p256ˢ, (ж<testing.T> tΔ1) => {
        f(tΔ1, ecdh.P256());
    });
    Ꮡt.Run(p384ˢ, (ж<testing.T> tΔ2) => {
        f(tΔ2, ecdh.P384());
    });
    Ꮡt.Run(p521ˢ, (ж<testing.T> tΔ3) => {
        f(tΔ3, ecdh.P521());
    });
    Ꮡt.Run(x25519ˢ, (ж<testing.T> tΔ4) => {
        f(tΔ4, ecdh.X25519());
    });
}

public static void BenchmarkECDH(ж<testing.B> Ꮡb) {
    benchmarkAllCurves(Ꮡb, (ж<testing.B> bΔ1, ecdhꓸCurve curve) => {
        var (c, err) = chacha20.NewUnauthenticatedCipher(new slice<byte>(32), new slice<byte>(12));
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        var rand = new cipher.StreamReader(
            S: new chacha20.CipherжStream(c), R: zeroReader
        );
        (var peerKey, err) = curve.GenerateKey(new cipher_StreamReaderᴠReader(rand));
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        var peerShare = peerKey.PublicKey().Bytes();
        bΔ1.ResetTimer();
        bΔ1.ReportAllocs();
        byte allocationsSink = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (key, errΔ1) = curve.GenerateKey(new cipher_StreamReaderᴠReader(rand));
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            var share = key.PublicKey().Bytes();
            (var peerPubKey, errΔ1) = curve.NewPublicKey(peerShare);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            (var secret, errΔ1) = key.ECDH(peerPubKey);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            allocationsSink ^= (byte)((byte)(secret[0] ^ share[0]));
        }
    });
}

internal static void benchmarkAllCurves(ж<testing.B> Ꮡb, Action<ж<testing.B>, ecdhꓸCurve> f) {
    Ꮡb.Run(p256ˢ, (ж<testing.B> bΔ1) => {
        f(bΔ1, ecdh.P256());
    });
    Ꮡb.Run(p384ˢ, (ж<testing.B> bΔ2) => {
        f(bΔ2, ecdh.P384());
    });
    Ꮡb.Run(p521ˢ, (ж<testing.B> bΔ3) => {
        f(bΔ3, ecdh.P521());
    });
    Ꮡb.Run(x25519ˢ, (ж<testing.B> bΔ4) => {
        f(bΔ4, ecdh.X25519());
    });
}

partial struct zr {
}

// Read replaces the contents of dst with zeros. It is safe for concurrent use.
internal static (nint n, error err) Read(this zr _, slice<byte> dst) {
    clear(dst);
    return (len(dst), default!);
}

internal static zr zeroReader = new zr(nil);

internal static readonly @string linkerTestProgram = """

package main
import "crypto/ecdh"
import "crypto/rand"
func main() {
	// Use P-256, since that's what the always-enabled CAST uses.
	curve := ecdh.P256()
	key, err := curve.GenerateKey(rand.Reader)
	if err != nil { panic(err) }
	_, err = curve.NewPublicKey(key.PublicKey().Bytes())
	if err != nil { panic(err) }
	_, err = curve.NewPrivateKey(key.Bytes())
	if err != nil { panic(err) }
	_, err = key.ECDH(key.PublicKey())
	if err != nil { panic(err) }
	println("OK")
}

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object testRequiresRunningGoˢ = (@string)"test requires running 'go build'"u8;
private static readonly @string helloGoˢ = "hello.go"u8;
private static readonly @string buildˢ = "build"u8;
private static readonly @string helloExeˢ = "hello.exe"u8;
private static readonly @string helloExeˢ2 = "./hello.exe"u8;
private static readonly object unexpectedOutputˢ = (@string)"unexpected output:"u8;
private static readonly @string toolˢ = "tool"u8;
private static readonly @string mTCryptoˢ = @"(?m)T (crypto/.*)$"u8;
private static readonly @string p256ˢ2 = "p256"u8;
private static readonly @string p224ˢ = "p224"u8;
private static readonly @string p384ˢ2 = "p384"u8;
private static readonly @string p521ˢ2 = "p521"u8;
private static readonly object noP256SymbolsFoundInˢ = (@string)"no P256 symbols found in program using ecdh.P256, test is broken"u8;

// TestLinker ensures that using one curve does not bring all other
// implementations into the binary. This also guarantees that govulncheck can
// avoid warning about a curve-specific vulnerability if that curve is not used.
public static void TestLinker(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(testRequiresRunningGoˢ);
    }
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    @string dir = Ꮡt.TempDir();
    @string hello = filepath.Join(dir, helloGoˢ);
    var err = os.WriteFile(hello, slice<byte>(linkerTestProgram), 436);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string run(params ꓸꓸꓸstring argsʗp) {
        var args = argsʗp.slice();
        var cmd = exec.Command(args[0], args[1..].ꓸꓸꓸ);
        cmd.Value.Dir = dir;
        var (@out, errΔ1) = cmd.CombinedOutput();
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("%v: %v\n%s"u8, args, errΔ1, ((@string)@out));
        }
        return ((@string)@out);
    }
    @string goBin = testenv.GoToolPath(new testing_TжTB(Ꮡt));
    run(goBin, buildˢ, "-o", helloExeˢ, helloGoˢ);
    {
        @string @out = run(helloExeˢ2); if (@out != "OK\n"u8) {
            Ꮡt.Error(unexpectedOutputˢ, @out);
        }
    }
    // List all text symbols under crypto/... and make sure there are some for
    // P256, but none for the other curves.
    bool consistent = default!;
    @string nm = run(goBin, toolˢ, "nm", helloExeˢ);
    foreach (var (_, match) in regexp.MustCompile(mTCryptoˢ).FindAllStringSubmatch(nm, -1)) {
        @string symbol = strings.ToLower(match[1]);
        if (strings.Contains(symbol, p256ˢ2)) {
            consistent = true;
        }
        if (strings.Contains(symbol, p224ˢ) || strings.Contains(symbol, p384ˢ2) || strings.Contains(symbol, p521ˢ2)) {
            Ꮡt.Errorf("unexpected symbol in program using only ecdh.P256: %s"u8, match[1]);
        }
    }
    if (!consistent) {
        Ꮡt.Error(noP256SymbolsFoundInˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhPrivateKeyAndˢ = "crypto/ecdh: private key and public key curves do not match"u8;

internal partial struct TestMismatchedCurves_curves /*dyn*/ {
    internal @string name;
    internal ecdhꓸCurve curve;
}

public static void TestMismatchedCurves(ж<testing.T> Ꮡt) {
    var curves = new TestMismatchedCurves_curves[]{
        new("P256"u8, ecdh.P256()),
        new("P384"u8, ecdh.P384()),
        new("P521"u8, ecdh.P521()),
        new("X25519"u8, ecdh.X25519())
    }.slice();
    foreach (var (_, privCurve) in curves) {
        var (priv, err) = privCurve.curve.GenerateKey(rand.Reader);
        if (err != default!) {
            Ꮡt.Fatalf("failed to generate test key: %s"u8, err);
        }
        foreach (var (_, vᴛ1) in curves) {
            ref var pubCurve = ref heap(new TestMismatchedCurves_curves(), out var ᏑpubCurve);
            pubCurve = vᴛ1;

            if (privCurve == pubCurve) {
                continue;
            }
            var privʗ1 = priv;
            var pubCurveʗ1 = pubCurve;
            Ꮡt.Run(fmt.Sprintf("%s/%s"u8, privCurve.name, pubCurve.name), (ж<testing.T> tΔ1) => {
                var (pub, errΔ1) = pubCurveʗ1.curve.GenerateKey(rand.Reader);
                if (errΔ1 != default!) {
                    tΔ1.Fatalf("failed to generate test key: %s"u8, errΔ1);
                }
                @string expected = cryptoEcdhPrivateKeyAndˢ;
                (_, errΔ1) = privʗ1.ECDH(pub.PublicKey());
                if (errΔ1.Error() != expected) {
                    tΔ1.Fatalf("unexpected error: want %q, got %q"u8, expected, errΔ1);
                }
            });
        }
    }
}

} // end ecdh_test_package
