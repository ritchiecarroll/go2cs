// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bufio = bufio_package;
using bytes = bytes_package;
using bzip2 = compress.bzip2_package;
using crypto = crypto_package;
using elliptic = go.crypto.elliptic_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using rand = go.crypto.rand_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using sha512 = go.crypto.sha512_package;
using hex = encoding.hex_package;
using hash = hash_package;
using io = io_package;
using big = math.big_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using compress;
using encoding;
using go.crypto;
using go.crypto.@internal;
using math;
using static go.crypto.ecdsa_package;

partial class ecdsa_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaˢ = "ecdsa"u8;

[GoType("dyn")] internal partial struct testAllCurves_tests {
    internal @string name;
    internal elliptic.Curve curve;
}

internal static void testAllCurves(ж<testing.T> Ꮡt, Action<ж<testing.T>, elliptic.Curve> f) {
    var tests = new testAllCurves_tests[]{
        new("P256"u8, elliptic.P256()),
        new("P224"u8, elliptic.P224()),
        new("P384"u8, elliptic.P384()),
        new("P521"u8, elliptic.P521()),
        new("P256/Generic"u8, new elliptic.CurveParamsжCurve(genericParamsForCurve(elliptic.P256())))
    }.slice();
    if (testing.Short()) {
        tests = tests[..1];
    }
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new testAllCurves_tests(), out var Ꮡtest);
        test = vᴛ1;

        var curve = test.curve;
        var curveʗ1 = curve;
        var testʗ1 = test;
        cryptotest.TestAllImplementations(Ꮡt, ecdsaˢ, (ж<testing.T> tΔ1) => {
            var curveʗ2 = curveʗ1;
            tΔ1.Run(testʗ1.name, (ж<testing.T> tΔ2) => {
                tΔ2.Parallel();
                f(tΔ2, curveʗ2);
            });
        });
    }
}

// genericParamsForCurve returns the dereferenced CurveParams for
// the specified curve. This is used to avoid the logic for
// upgrading a curve to its specific implementation, forcing
// usage of the generic implementation.
internal static ж<elliptic.CurveParams> genericParamsForCurve(elliptic.Curve c) {
    ref var d = ref heap<elliptic.CurveParams>(out var Ꮡd);
    d = (c.Params()).Value;
    return Ꮡd;
}

public static void TestKeyGeneration(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testKeyGeneration);
}

internal static void testKeyGeneration(ж<testing.T> Ꮡt, elliptic.Curve c) {
    var (priv, err) = GenerateKey(c, rand.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!c.IsOnCurve((~priv).PublicKey.X, (~priv).PublicKey.Y)) {
        Ꮡt.Errorf("public key invalid: %s"u8, err);
    }
}

public static void TestSignAndVerify(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testSignAndVerify);
}

internal static void testSignAndVerify(ж<testing.T> Ꮡt, elliptic.Curve c) {
    var (priv, _) = GenerateKey(c, rand.Reader);
    var hashed = slice<byte>("testing"u8);
    var (r, s, err) = Sign(rand.Reader, priv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("error signing: %s"u8, err);
        return;
    }
    if (!Verify(priv.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), hashed, r, s)) {
        Ꮡt.Errorf("Verify failed"u8);
    }
    hashed[0] ^= (byte)(0xff);
    if (Verify(priv.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), hashed, r, s)) {
        Ꮡt.Errorf("Verify always works!"u8);
    }
}

public static void TestSignAndVerifyASN1(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testSignAndVerifyASN1);
}

internal static void testSignAndVerifyASN1(ж<testing.T> Ꮡt, elliptic.Curve c) {
    var (priv, _) = GenerateKey(c, rand.Reader);
    var hashed = slice<byte>("testing"u8);
    var (sig, err) = SignASN1(rand.Reader, priv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("error signing: %s"u8, err);
        return;
    }
    if (!VerifyASN1(priv.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), hashed, sig)) {
        Ꮡt.Errorf("VerifyASN1 failed"u8);
    }
    hashed[0] ^= (byte)(0xff);
    if (VerifyASN1(priv.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), hashed, sig)) {
        Ꮡt.Errorf("VerifyASN1 always works!"u8);
    }
}

public static void TestNonceSafety(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testNonceSafety);
}

internal static void testNonceSafety(ж<testing.T> Ꮡt, elliptic.Curve c) {
    var (priv, _) = GenerateKey(c, rand.Reader);
    var hashed = slice<byte>("testing"u8);
    var (r0, s0, err) = Sign(new ecdsa_internal_test_package.readerFuncᴠReader(zeroReader), priv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("error signing: %s"u8, err);
        return;
    }
    hashed = slice<byte>("testing..."u8);
    (var r1, var s1, err) = Sign(new ecdsa_internal_test_package.readerFuncᴠReader(zeroReader), priv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("error signing: %s"u8, err);
        return;
    }
    if (s0.Cmp(s1) == 0) {
        // This should never happen.
        Ꮡt.Errorf("the signatures on two different messages were the same"u8);
    }
    if (r0.Cmp(r1) == 0) {
        Ꮡt.Errorf("the nonce used for two different messages was the same"u8);
    }
}

internal delegate (nint, error) readerFunc(slice<byte> _Δp0);

internal static (nint, error) Read(this readerFunc f, slice<byte> b) {
    return f(b);
}

internal static readerFunc zeroReader = new readerFunc((slice<byte> b) => {
    clear(b);
    return (len(b), default!);
});

public static void TestINDCCA(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testINDCCA);
}

internal static void testINDCCA(ж<testing.T> Ꮡt, elliptic.Curve c) {
    var (priv, _) = GenerateKey(c, rand.Reader);
    var hashed = slice<byte>("testing"u8);
    var (r0, s0, err) = Sign(rand.Reader, priv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("error signing: %s"u8, err);
        return;
    }
    (var r1, var s1, err) = Sign(rand.Reader, priv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("error signing: %s"u8, err);
        return;
    }
    if (s0.Cmp(s1) == 0) {
        Ꮡt.Errorf("two signatures of the same message produced the same result"u8);
    }
    if (r0.Cmp(r1) == 0) {
        Ꮡt.Errorf("two signatures of the same message produced the same nonce"u8);
    }
}

internal static ж<bigꓸInt> fromHex(@string s) {
    var (r, ok) = @new<bigꓸInt>().SetString(s, 16);
    if (!ok) {
        throw panic("bad hex");
    }
    return r;
}

public static void TestVectors(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, ecdsaˢ, testVectors);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataSigVerRspBz2ˢ = "testdata/SigVer.rsp.bz2"u8;
internal static readonly @string msgˢ = "Msg = "u8;
internal static readonly @string resultˢ = "Result = "u8;

internal static void testVectors(ж<testing.T> Ꮡt) {
    // This test runs the full set of NIST test vectors from
    // https://csrc.nist.gov/groups/STM/cavp/documents/dss/186-3ecdsatestvectors.zip
    //
    // The SigVer.rsp file has been edited to remove test vectors for
    // unsupported algorithms and has been compressed.
    if (testing.Short()) {
        return;
    }
    var (f, err) = os.Open(testdataSigVerRspBz2ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var buf = bufio.NewReader(bzip2.NewReader(new ecdsa_test_package.os_FileжReader(f)));
    nint lineNo = 1;
    hash.Hash h = default!;
    slice<byte> msg = default!;
    slice<byte> hashed = default!;
    ж<bigꓸInt> r = default!;
    ж<bigꓸInt> s = default!;
    var pub = @new<global::go.crypto.ecdsa_package.PublicKey>();
    while (ᐧ) {
        var (line, errΔ1) = buf.ReadString((rune)'\n');
        if (len(line) == 0) {
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            Ꮡt.Fatalf("error reading from input: %s"u8, errΔ1);
        }
        lineNo++;
        // Need to remove \r\n from the end of the line.
        if (!strings.HasSuffix(line, "\r\n"u8)) {
            Ꮡt.Fatalf("bad line ending (expected \\r\\n) on line %d"u8, lineNo);
        }
        line = line.slice(0, len(line) - 2);
        if (len(line) == 0 || line[0] == (rune)'#') {
            continue;
        }
        if (line[0] == (rune)'[') {
            line = line.slice(1, len(line) - 1);
            var (curve, hashΔ1, _) = strings.Cut(line, ","u8);
            var exprᴛ1 = curve;
            if (exprᴛ1 == "P-224"u8) {
                pub.Value.Curve = elliptic.P224();
            }
            else if (exprᴛ1 == "P-256"u8) {
                pub.Value.Curve = elliptic.P256();
            }
            else if (exprᴛ1 == "P-384"u8) {
                pub.Value.Curve = elliptic.P384();
            }
            else if (exprᴛ1 == "P-521"u8) {
                pub.Value.Curve = elliptic.P521();
            }
            else { /* default: */
                pub.Value.Curve = default!;
            }

            var exprᴛ2 = hashΔ1;
            if (exprᴛ2 == "SHA-1"u8) {
                h = sha1.New();
            }
            else if (exprᴛ2 == "SHA-224"u8) {
                h = sha256.New224();
            }
            else if (exprᴛ2 == "SHA-256"u8) {
                h = sha256.New();
            }
            else if (exprᴛ2 == "SHA-384"u8) {
                h = sha512.New384();
            }
            else if (exprᴛ2 == "SHA-512"u8) {
                h = sha512.New();
            }
            else { /* default: */
                h = default!;
            }

            continue;
        }
        if (h == default! || (~pub).Curve == default!) {
            continue;
        }
        switch (ᐧ) {
        case {} when strings.HasPrefix(line, msgˢ): {
            {
                (msg, errΔ1) = hex.DecodeString(line[6..]); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("failed to decode message on line %d: %s"u8, lineNo, errΔ1);
                }
            }
            break;
        }
        case {} when strings.HasPrefix(line, "Qx = "u8): {
            pub.Value.X = fromHex(line[5..]);
            break;
        }
        case {} when strings.HasPrefix(line, "Qy = "u8): {
            pub.Value.Y = fromHex(line[5..]);
            break;
        }
        case {} when strings.HasPrefix(line, "R = "u8): {
            r = fromHex(line[4..]);
            break;
        }
        case {} when strings.HasPrefix(line, "S = "u8): {
            s = fromHex(line[4..]);
            break;
        }
        case {} when strings.HasPrefix(line, resultˢ): {
            var expected = line[9] == (rune)'P';
            h.Reset();
            h.Write(msg);
            var hashedΔ2 = h.Sum(hashed[..0]);
            if (Verify(pub, hashedΔ2, r, s) != expected) {
                Ꮡt.Fatalf("incorrect result on line %d"u8, lineNo);
            }
            break;
        }
        default: {
            Ꮡt.Fatalf("unknown variable on line %d: %s"u8, lineNo, line);
            break;
        }}

    }
}

public static void TestNegativeInputs(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testNegativeInputs);
}

internal static void testNegativeInputs(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var (key, err) = GenerateKey(curve, rand.Reader);
    if (err != default!) {
        Ꮡt.Errorf("failed to generate key"u8);
    }
    array<byte> hash = new(32);
    var r = @new<bigꓸInt>().SetInt64(1);
    r.Lsh(r, 550);
    /* larger than any supported curve */
    r.Neg(r);
    if (Verify(key.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), hash[..], r, r)) {
        Ꮡt.Errorf("bogus signature accepted"u8);
    }
}

public static void TestZeroHashSignature(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testZeroHashSignature);
}

internal static void testZeroHashSignature(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var zeroHash = new slice<byte>(64);
    var (privKey, err) = GenerateKey(curve, rand.Reader);
    if (err != default!) {
        throw panic(err);
    }
    // Sign a hash consisting of all zeros.
    (var r, var s, err) = Sign(rand.Reader, privKey, zeroHash);
    if (err != default!) {
        throw panic(err);
    }
    // Confirm that it can be verified.
    if (!Verify(privKey.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), zeroHash, r, s)) {
        Ꮡt.Errorf("zero hash signature verify failed for %T"u8, curve);
    }
}

public static void TestZeroSignature(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testZeroSignature);
}

internal static void testZeroSignature(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var (privKey, err) = GenerateKey(curve, rand.Reader);
    if (err != default!) {
        throw panic(err);
    }
    if (Verify(privKey.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), new slice<byte>(64), big.NewInt(0), big.NewInt(0))) {
        Ꮡt.Errorf("Verify with r,s=0 succeeded: %T"u8, curve);
    }
}

public static void TestNegativeSignature(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testNegativeSignature);
}

internal static void testNegativeSignature(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var zeroHash = new slice<byte>(64);
    var (privKey, err) = GenerateKey(curve, rand.Reader);
    if (err != default!) {
        throw panic(err);
    }
    (var r, var s, err) = Sign(rand.Reader, privKey, zeroHash);
    if (err != default!) {
        throw panic(err);
    }
    r = r.Neg(r);
    if (Verify(privKey.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), zeroHash, r, s)) {
        Ꮡt.Errorf("Verify with r=-r succeeded: %T"u8, curve);
    }
}

public static void TestRPlusNSignature(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testRPlusNSignature);
}

internal static void testRPlusNSignature(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var zeroHash = new slice<byte>(64);
    var (privKey, err) = GenerateKey(curve, rand.Reader);
    if (err != default!) {
        throw panic(err);
    }
    (var r, var s, err) = Sign(rand.Reader, privKey, zeroHash);
    if (err != default!) {
        throw panic(err);
    }
    r = r.Add(r, (~curve.Params()).N);
    if (Verify(privKey.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), zeroHash, r, s)) {
        Ꮡt.Errorf("Verify with r=r+n succeeded: %T"u8, curve);
    }
}

public static void TestRMinusNSignature(ж<testing.T> Ꮡt) {
    testAllCurves(Ꮡt, testRMinusNSignature);
}

internal static void testRMinusNSignature(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var zeroHash = new slice<byte>(64);
    var (privKey, err) = GenerateKey(curve, rand.Reader);
    if (err != default!) {
        throw panic(err);
    }
    (var r, var s, err) = Sign(rand.Reader, privKey, zeroHash);
    if (err != default!) {
        throw panic(err);
    }
    r = r.Sub(r, (~curve.Params()).N);
    if (Verify(privKey.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), zeroHash, r, s)) {
        Ꮡt.Errorf("Verify with r=r-n succeeded: %T"u8, curve);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string p224ˢ = "P-224"u8;
internal static readonly @string sampleˢ = "sample"u8;
internal static readonly @string testˢ = "test"u8;
internal static readonly @string p256ˢ = "P-256"u8;
internal static readonly @string wvVnXˢ = "wv[vnX"u8;
internal static readonly @string p384ˢ = "P-384"u8;
internal static readonly @string p521ˢ = "P-521"u8;

public static void TestRFC6979(ж<testing.T> Ꮡt) {
    Ꮡt.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testRFC6979(tΔ1, elliptic.P224(),
            "F220266E1105BFE3083E03EC7A3A654651F45E37167E88600BF257C1"u8,
            "00CF08DA5AD719E42707FA431292DEA11244D64FC51610D94B130D6C"u8,
            "EEAB6F3DEBE455E3DBF85416F7030CBD94F34F2D6F232C69F3C1385A"u8,
            sampleˢ,
            "61AA3DA010E8E8406C656BC477A7A7189895E7E840CDFE8FF42307BA"u8,
            "BC814050DAB5D23770879494F9E0A680DC1AF7161991BDE692B10101"u8);
        testRFC6979(tΔ1, elliptic.P224(),
            "F220266E1105BFE3083E03EC7A3A654651F45E37167E88600BF257C1"u8,
            "00CF08DA5AD719E42707FA431292DEA11244D64FC51610D94B130D6C"u8,
            "EEAB6F3DEBE455E3DBF85416F7030CBD94F34F2D6F232C69F3C1385A"u8,
            testˢ,
            "AD04DDE87B84747A243A631EA47A1BA6D1FAA059149AD2440DE6FBA6"u8,
            "178D49B1AE90E3D8B629BE3DB5683915F4E8C99FDF6E666CF37ADCFD"u8);
    });
    Ꮡt.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        // This vector was bruteforced to find a message that causes the
        // generation of k to loop. It was checked against
        // github.com/codahale/rfc6979 (https://go.dev/play/p/FK5-fmKf7eK),
        // OpenSSL 3.2.0 (https://github.com/openssl/openssl/pull/23130),
        // and python-ecdsa:
        //
        //    ecdsa.keys.SigningKey.from_secret_exponent(
        //        0xC9AFA9D845BA75166B5C215767B1D6934E50C3DB36E89B127B8A622B120F6721,
        //        ecdsa.curves.curve_by_name("NIST256p"), hashlib.sha256).sign_deterministic(
        //        b"wv[vnX", hashlib.sha256, lambda r, s, order: print(hex(r), hex(s)))
        //
        testRFC6979(tΔ2, elliptic.P256(),
            "C9AFA9D845BA75166B5C215767B1D6934E50C3DB36E89B127B8A622B120F6721"u8,
            "60FED4BA255A9D31C961EB74C6356D68C049B8923B61FA6CE669622E60F29FB6"u8,
            "7903FE1008B8BC99A41AE9E95628BC64F2F1B20C2D7E9F5177A3C294D4462299"u8,
            wvVnXˢ,
            "EFD9073B652E76DA1B5A019C0E4A2E3FA529B035A6ABB91EF67F0ED7A1F21234"u8,
            "3DB4706C9D9F4A4FE13BB5E08EF0FAB53A57DBAB2061C83A35FA411C68D2BA33"u8);
        // The remaining vectors are from RFC 6979.
        testRFC6979(tΔ2, elliptic.P256(),
            "C9AFA9D845BA75166B5C215767B1D6934E50C3DB36E89B127B8A622B120F6721"u8,
            "60FED4BA255A9D31C961EB74C6356D68C049B8923B61FA6CE669622E60F29FB6"u8,
            "7903FE1008B8BC99A41AE9E95628BC64F2F1B20C2D7E9F5177A3C294D4462299"u8,
            sampleˢ,
            "EFD48B2AACB6A8FD1140DD9CD45E81D69D2C877B56AAF991C34D0EA84EAF3716"u8,
            "F7CB1C942D657C41D436C7A1B6E29F65F3E900DBB9AFF4064DC4AB2F843ACDA8"u8);
        testRFC6979(tΔ2, elliptic.P256(),
            "C9AFA9D845BA75166B5C215767B1D6934E50C3DB36E89B127B8A622B120F6721"u8,
            "60FED4BA255A9D31C961EB74C6356D68C049B8923B61FA6CE669622E60F29FB6"u8,
            "7903FE1008B8BC99A41AE9E95628BC64F2F1B20C2D7E9F5177A3C294D4462299"u8,
            testˢ,
            "F1ABB023518351CD71D881567B1EA663ED3EFCF6C5132B354F28D3B0B7D38367"u8,
            "019F4113742A2B14BD25926B49C649155F267E60D3814B4C0CC84250E46F0083"u8);
    });
    Ꮡt.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testRFC6979(tΔ3, elliptic.P384(),
            "6B9D3DAD2E1B8C1C05B19875B6659F4DE23C3B667BF297BA9AA47740787137D896D5724E4C70A825F872C9EA60D2EDF5"u8,
            "EC3A4E415B4E19A4568618029F427FA5DA9A8BC4AE92E02E06AAE5286B300C64DEF8F0EA9055866064A254515480BC13"u8,
            "8015D9B72D7D57244EA8EF9AC0C621896708A59367F9DFB9F54CA84B3F1C9DB1288B231C3AE0D4FE7344FD2533264720"u8,
            sampleˢ,
            "21B13D1E013C7FA1392D03C5F99AF8B30C570C6F98D4EA8E354B63A21D3DAA33BDE1E888E63355D92FA2B3C36D8FB2CD"u8,
            "F3AA443FB107745BF4BD77CB3891674632068A10CA67E3D45DB2266FA7D1FEEBEFDC63ECCD1AC42EC0CB8668A4FA0AB0"u8);
        testRFC6979(tΔ3, elliptic.P384(),
            "6B9D3DAD2E1B8C1C05B19875B6659F4DE23C3B667BF297BA9AA47740787137D896D5724E4C70A825F872C9EA60D2EDF5"u8,
            "EC3A4E415B4E19A4568618029F427FA5DA9A8BC4AE92E02E06AAE5286B300C64DEF8F0EA9055866064A254515480BC13"u8,
            "8015D9B72D7D57244EA8EF9AC0C621896708A59367F9DFB9F54CA84B3F1C9DB1288B231C3AE0D4FE7344FD2533264720"u8,
            testˢ,
            "6D6DEFAC9AB64DABAFE36C6BF510352A4CC27001263638E5B16D9BB51D451559F918EEDAF2293BE5B475CC8F0188636B"u8,
            "2D46F3BECBCC523D5F1A1256BF0C9B024D879BA9E838144C8BA6BAEB4B53B47D51AB373F9845C0514EEFB14024787265"u8);
    });
    Ꮡt.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testRFC6979(tΔ4, elliptic.P521(),
            "0FAD06DAA62BA3B25D2FB40133DA757205DE67F5BB0018FEE8C86E1B68C7E75CAA896EB32F1F47C70855836A6D16FCC1466F6D8FBEC67DB89EC0C08B0E996B83538"u8,
            "1894550D0785932E00EAA23B694F213F8C3121F86DC97A04E5A7167DB4E5BCD371123D46E45DB6B5D5370A7F20FB633155D38FFA16D2BD761DCAC474B9A2F5023A4"u8,
            "0493101C962CD4D2FDDF782285E64584139C2F91B47F87FF82354D6630F746A28A0DB25741B5B34A828008B22ACC23F924FAAFBD4D33F81EA66956DFEAA2BFDFCF5"u8,
            sampleˢ,
            "1511BB4D675114FE266FC4372B87682BAECC01D3CC62CF2303C92B3526012659D16876E25C7C1E57648F23B73564D67F61C6F14D527D54972810421E7D87589E1A7"u8,
            "04A171143A83163D6DF460AAF61522695F207A58B95C0644D87E52AA1A347916E4F7A72930B1BC06DBE22CE3F58264AFD23704CBB63B29B931F7DE6C9D949A7ECFC"u8);
        testRFC6979(tΔ4, elliptic.P521(),
            "0FAD06DAA62BA3B25D2FB40133DA757205DE67F5BB0018FEE8C86E1B68C7E75CAA896EB32F1F47C70855836A6D16FCC1466F6D8FBEC67DB89EC0C08B0E996B83538"u8,
            "1894550D0785932E00EAA23B694F213F8C3121F86DC97A04E5A7167DB4E5BCD371123D46E45DB6B5D5370A7F20FB633155D38FFA16D2BD761DCAC474B9A2F5023A4"u8,
            "0493101C962CD4D2FDDF782285E64584139C2F91B47F87FF82354D6630F746A28A0DB25741B5B34A828008B22ACC23F924FAAFBD4D33F81EA66956DFEAA2BFDFCF5"u8,
            testˢ,
            "00E871C4A14F993C6C7369501900C4BC1E9C7B0B4BA44E04868B30B41D8071042EB28C4C250411D0CE08CD197E4188EA4876F279F90B3D8D74A3C76E6F1E4656AA8"u8,
            "0CD52DBAA33B063C3A6CD8058A1FB0A46A4754B034FCC644766CA14DA8CA5CA9FDE00E88C1AD60CCBA759025299079D7A427EC3CC5B619BFBC828E7769BCD694E86"u8);
    });
}

internal static void testRFC6979(ж<testing.T> Ꮡt, elliptic.Curve curve, @string D, @string X, @string Y, @string msg, @string r, @string s) {
    var priv = Ꮡ(new PrivateKey(
        D: fromHex(D),
        PublicKey: new PublicKey(
            Curve: curve,
            X: fromHex(X),
            Y: fromHex(Y)
        )
    ));
    var h = sha256.Sum256(slice<byte>(msg));
    var (sig, err) = priv.Sign(default!, h[..], crypto.SHA256);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var expected, err) = encodeSignature(fromHex(r).Bytes(), fromHex(s).Bytes());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(sig, expected)) {
        Ꮡt.Errorf("signature mismatch:\n got: %x\nwant: %x"u8, sig, expected);
    }
}

[GoType("dyn")] internal partial struct benchmarkAllCurves_tests {
    internal @string name;
    internal elliptic.Curve curve;
}

internal static void benchmarkAllCurves(ж<testing.B> Ꮡb, Action<ж<testing.B>, elliptic.Curve> f) {
    var tests = new benchmarkAllCurves_tests[]{
        new("P256"u8, elliptic.P256()),
        new("P384"u8, elliptic.P384()),
        new("P521"u8, elliptic.P521())
    }.slice();
    foreach (var (_, test) in tests) {
        var curve = test.curve;
        var curveʗ1 = curve;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            f(bΔ1, curveʗ1);
        });
    }
}

public static void BenchmarkSign(ж<testing.B> Ꮡb) {
    benchmarkAllCurves(Ꮡb, (ж<testing.B> bΔ1, elliptic.Curve curve) => {
        var r = bufio.NewReaderSize(rand.Reader, (1 << (int)(15)));
        var (priv, err) = GenerateKey(curve, new ecdsa_test_package.bufio_ReaderжReader(r));
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        var hashed = slice<byte>("testing"u8);
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (sig, errΔ1) = SignASN1(new ecdsa_test_package.bufio_ReaderжReader(r), priv, hashed);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            // Prevent the compiler from optimizing out the operation.
            hashed[0] = sig[0];
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object verifyFailedˢ = (@string)"verify failed"u8;

public static void BenchmarkVerify(ж<testing.B> Ꮡb) {
    benchmarkAllCurves(Ꮡb, (ж<testing.B> bΔ1, elliptic.Curve curve) => {
        var r = bufio.NewReaderSize(rand.Reader, (1 << (int)(15)));
        var (priv, err) = GenerateKey(curve, new ecdsa_test_package.bufio_ReaderжReader(r));
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        var hashed = slice<byte>("testing"u8);
        (var sig, err) = SignASN1(new ecdsa_test_package.bufio_ReaderжReader(r), priv, hashed);
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            if (!VerifyASN1(priv.of(global::go.crypto.ecdsa_package.PrivateKey.ᏑPublicKey), hashed, sig)) {
                bΔ1.Fatal(verifyFailedˢ);
            }
        }
    });
}

public static void BenchmarkGenerateKey(ж<testing.B> Ꮡb) {
    benchmarkAllCurves(Ꮡb, (ж<testing.B> bΔ1, elliptic.Curve curve) => {
        var r = bufio.NewReaderSize(rand.Reader, (1 << (int)(15)));
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            {
                var (_, err) = GenerateKey(curve, new ecdsa_test_package.bufio_ReaderжReader(r)); if (err != default!) {
                    bΔ1.Fatal(err);
                }
            }
        }
    });
}

} // end ecdsa_internal_test_package
