// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using rand = go.crypto.rand_package;
using big = math.big_package;
using testing = testing_package;
using go.crypto;
using io = io_package;
using math;
using static go.crypto.dsa_package;

partial class dsa_internal_test_package {

internal static void testSignAndVerify(ж<testing.T> Ꮡt, nint i, ж<global::go.crypto.dsa_package.PrivateKey> Ꮡpriv) {
    var hashed = slice<byte>("testing"u8);
    var (r, s, err) = Sign(rand.Reader, Ꮡpriv, hashed);
    if (err != default!) {
        Ꮡt.Errorf("%d: error signing: %s"u8, i, err);
        return;
    }
    if (!Verify(Ꮡpriv.of(global::go.crypto.dsa_package.PrivateKey.ᏑPublicKey), hashed, r, s)) {
        Ꮡt.Errorf("%d: Verify failed"u8, i);
    }
}

internal static void testParameterGeneration(ж<testing.T> Ꮡt, global::go.crypto.dsa_package.ParameterSizes sizes, nint L, nint N) {
    Ꮡt.Helper();
    ref var priv = ref heap(new global::go.crypto.dsa_package.PrivateKey(), out var Ꮡpriv);
    var @params = Ꮡpriv.of(global::go.crypto.dsa_package.PrivateKey.ᏑParameters);
    var err = GenerateParameters(@params, rand.Reader, sizes);
    if (err != default!) {
        Ꮡt.Errorf("%d: %s"u8, (nint)sizes, err);
        return;
    }
    if ((~@params).P.BitLen() != L) {
        Ꮡt.Errorf("%d: params.BitLen got:%d want:%d"u8, (nint)sizes, (~@params).P.BitLen(), L);
    }
    if ((~@params).Q.BitLen() != N) {
        Ꮡt.Errorf("%d: q.BitLen got:%d want:%d"u8, (nint)sizes, (~@params).Q.BitLen(), L);
    }
    var one = @new<bigꓸInt>();
    one.SetInt64(1);
    var pm1 = @new<bigꓸInt>().Sub((~@params).P, one);
    var (quo, rem) = @new<bigꓸInt>().DivMod(pm1, (~@params).Q, @new<bigꓸInt>());
    if (rem.Sign() != 0) {
        Ꮡt.Errorf("%d: p-1 mod q != 0"u8, (nint)sizes);
    }
    var x = @new<bigꓸInt>().Exp((~@params).G, quo, (~@params).P);
    if (x.Cmp(one) == 0) {
        Ꮡt.Errorf("%d: invalid generator"u8, (nint)sizes);
    }
    err = GenerateKey(Ꮡpriv, rand.Reader);
    if (err != default!) {
        Ꮡt.Errorf("error generating key: %s"u8, err);
        return;
    }
    testSignAndVerify(Ꮡt, (nint)sizes, Ꮡpriv);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingParameterˢ = (@string)"skipping parameter generation test in short mode"u8;

public static void TestParameterGeneration(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingParameterˢ);
    }
    testParameterGeneration(Ꮡt, L1024N160, 1024, 160);
    testParameterGeneration(Ꮡt, L2048N224, 2048, 224);
    testParameterGeneration(Ꮡt, L2048N256, 2048, 256);
    testParameterGeneration(Ꮡt, L3072N256, 3072, 256);
}

internal static ж<bigꓸInt> fromHex(@string s) {
    var (result, ok) = @new<bigꓸInt>().SetString(s, 16);
    if (!ok) {
        throw panic(s);
    }
    return result;
}

public static void TestSignAndVerify(ж<testing.T> Ꮡt) {
    ref var priv = ref heap<global::go.crypto.dsa_package.PrivateKey>(out var Ꮡpriv);
    priv = new PrivateKey(
        PublicKey: new PublicKey(
            Parameters: new Parameters(
                P: fromHex("A9B5B793FB4785793D246BAE77E8FF63CA52F442DA763C440259919FE1BC1D6065A9350637A04F75A2F039401D49F08E066C4D275A5A65DA5684BC563C14289D7AB8A67163BFBF79D85972619AD2CFF55AB0EE77A9002B0EF96293BDD0F42685EBB2C66C327079F6C98000FBCB79AACDE1BC6F9D5C7B1A97E3D9D54ED7951FEF"u8),
                Q: fromHex("E1D3391245933D68A0714ED34BBCB7A1F422B9C1"u8),
                G: fromHex("634364FC25248933D01D1993ECABD0657CC0CB2CEED7ED2E3E8AECDFCDC4A25C3B15E9E3B163ACA2984B5539181F3EFF1A5E8903D71D5B95DA4F27202B77D2C44B430BB53741A8D59A8F86887525C9F2A6A5980A195EAA7F2FF910064301DEF89D3AA213E1FAC7768D89365318E370AF54A112EFBA9246D9158386BA1B4EEFDA"u8)
            ),
            Y: fromHex("32969E5780CFE1C849A1C276D7AEB4F38A23B591739AA2FE197349AEEBD31366AEE5EB7E6C6DDB7C57D02432B30DB5AA66D9884299FAA72568944E4EEDC92EA3FBC6F39F53412FBCC563208F7C15B737AC8910DBC2D9C9B8C001E72FDC40EB694AB1F06A5A2DBD18D9E36C66F31F566742F11EC0A52E9F7B89355C02FB5D32D2"u8)
        ),
        X: fromHex("5078D4D29795CBE76D3AACFE48C9AF0BCDBEE91A"u8)
    );
    testSignAndVerify(Ꮡt, 0, Ꮡpriv);
}

public static void TestSignAndVerifyWithBadPublicKey(ж<testing.T> Ꮡt) {
    ref var pub = ref heap<global::go.crypto.dsa_package.PublicKey>(out var Ꮡpub);
    pub = new PublicKey(
        Parameters: new Parameters(
            P: fromHex("A9B5B793FB4785793D246BAE77E8FF63CA52F442DA763C440259919FE1BC1D6065A9350637A04F75A2F039401D49F08E066C4D275A5A65DA5684BC563C14289D7AB8A67163BFBF79D85972619AD2CFF55AB0EE77A9002B0EF96293BDD0F42685EBB2C66C327079F6C98000FBCB79AACDE1BC6F9D5C7B1A97E3D9D54ED7951FEF"u8),
            Q: fromHex("FA"u8),
            G: fromHex("634364FC25248933D01D1993ECABD0657CC0CB2CEED7ED2E3E8AECDFCDC4A25C3B15E9E3B163ACA2984B5539181F3EFF1A5E8903D71D5B95DA4F27202B77D2C44B430BB53741A8D59A8F86887525C9F2A6A5980A195EAA7F2FF910064301DEF89D3AA213E1FAC7768D89365318E370AF54A112EFBA9246D9158386BA1B4EEFDA"u8)
        ),
        Y: fromHex("32969E5780CFE1C849A1C276D7AEB4F38A23B591739AA2FE197349AEEBD31366AEE5EB7E6C6DDB7C57D02432B30DB5AA66D9884299FAA72568944E4EEDC92EA3FBC6F39F53412FBCC563208F7C15B737AC8910DBC2D9C9B8C001E72FDC40EB694AB1F06A5A2DBD18D9E36C66F31F566742F11EC0A52E9F7B89355C02FB5D32D2"u8)
    );
    if (Verify(Ꮡpub, slice<byte>("testing"u8), fromHex("2"u8), fromHex("4"u8))) {
        Ꮡt.Errorf("Verify unexpected success with non-existent mod inverse of Q"u8);
    }
}

internal partial struct TestSigningWithDegenerateKeys_badKeys /*dyn*/ {
    internal @string p, q, g, y, x;
}

public static void TestSigningWithDegenerateKeys(ж<testing.T> Ꮡt) {
    // Signing with degenerate private keys should not cause an infinite
    // loop.
    var badKeys = new TestSigningWithDegenerateKeys_badKeys[]{
        new("00"u8, "01"u8, "00"u8, "00"u8, "00"u8),
        new("01"u8, "ff"u8, "00"u8, "00"u8, "00"u8)
    }.slice();
    foreach (var (i, test) in badKeys) {
        ref var priv = ref heap<global::go.crypto.dsa_package.PrivateKey>(out var Ꮡpriv);
        priv = new PrivateKey(
            PublicKey: new PublicKey(
                Parameters: new Parameters(
                    P: fromHex(test.p),
                    Q: fromHex(test.q),
                    G: fromHex(test.g)
                ),
                Y: fromHex(test.y)
            ),
            X: fromHex(test.x)
        );
        var hashed = slice<byte>("testing"u8);
        {
            var (_, _, err) = Sign(rand.Reader, Ꮡpriv, hashed); if (err == default!) {
                Ꮡt.Errorf("#%d: unexpected success"u8, i);
            }
        }
    }
}

} // end dsa_internal_test_package
