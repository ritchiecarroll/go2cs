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

internal static void testSignAndVerify(ж<testing.T> t, nint i, ж<global::go.crypto.dsa_package.PrivateKey> priv) {
    var hashed = slice<byte>("testing"u8);
    var (r, s, err) = Sign(rand.Reader, priv, hashed);
    if (err != nil) {
        t.Errorf("%d: error signing: %s", i, err);
        return;
    }

    if (!Verify(priv.ᏑPublicKey, hashed, r, s)) {
        t.Errorf("%d: Verify failed", i);
    }
}

internal static void testParameterGeneration(ж<testing.T> t, global::go.crypto.dsa_package.ParameterSizes sizes, nint L, nint N) {
    t.Helper();
    ref var priv = ref heap(new global::go.crypto.dsa_package.PrivateKey(), out var Ꮡpriv);
    var @params = Ꮡpriv.of(global::go.crypto.dsa_package.PrivateKey.ᏑParameters);

    var err = GenerateParameters(@params, rand.Reader, sizes);
    if (err != nil) {
        t.Errorf("%d: %s", (nint)sizes, err);
        return;
    }

    if (@params.P.BitLen() != L) {
        t.Errorf("%d: params.BitLen got:%d want:%d", (nint)sizes, @params.P.BitLen(), L);
    }

    if (@params.Q.BitLen() != N) {
        t.Errorf("%d: q.BitLen got:%d want:%d", (nint)sizes, @params.Q.BitLen(), L);
    }

    var one = @new<bigꓸInt>();
    one.SetInt64(1);
    var pm1 = @new<bigꓸInt>().Sub(@params.P, one);
    var (quo, rem) = @new<bigꓸInt>().DivMod(pm1, @params.Q, @new<bigꓸInt>());
    if (rem.Sign() != 0) {
        t.Errorf("%d: p-1 mod q != 0", (nint)sizes);
    }
    var x = @new<bigꓸInt>().Exp(@params.G, quo, @params.P);
    if (x.Cmp(one) == 0) {
        t.Errorf("%d: invalid generator", (nint)sizes);
    }

    err = GenerateKey(Ꮡpriv, rand.Reader);
    if (err != nil) {
        t.Errorf("error generating key: %s", err);
        return;
    }

    testSignAndVerify(t, (nint)sizes, Ꮡpriv);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingParameterˢ = (@string)"skipping parameter generation test in short mode"u8;

public static void TestParameterGeneration(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingParameterˢ);
    }

    testParameterGeneration(t, L1024N160, 1024, 160);
    testParameterGeneration(t, L2048N224, 2048, 224);
    testParameterGeneration(t, L2048N256, 2048, 256);
    testParameterGeneration(t, L3072N256, 3072, 256);
}

internal static ж<bigꓸInt> fromHex(@string s) {
    var (result, ok) = @new<bigꓸInt>().SetString(s, 16);
    if (!ok) {
        throw panic(s);
    }
    return result;
}

public static void TestSignAndVerify(ж<testing.T> t) {
    ref var priv = ref heap<global::go.crypto.dsa_package.PrivateKey>(new PrivateKey(
        PublicKey: new PublicKey(
            Parameters: new Parameters(
                P: fromHex("A9B5B793FB4785793D246BAE77E8FF63CA52F442DA763C440259919FE1BC1D6065A9350637A04F75A2F039401D49F08E066C4D275A5A65DA5684BC563C14289D7AB8A67163BFBF79D85972619AD2CFF55AB0EE77A9002B0EF96293BDD0F42685EBB2C66C327079F6C98000FBCB79AACDE1BC6F9D5C7B1A97E3D9D54ED7951FEF"),
                Q: fromHex("E1D3391245933D68A0714ED34BBCB7A1F422B9C1"),
                G: fromHex("634364FC25248933D01D1993ECABD0657CC0CB2CEED7ED2E3E8AECDFCDC4A25C3B15E9E3B163ACA2984B5539181F3EFF1A5E8903D71D5B95DA4F27202B77D2C44B430BB53741A8D59A8F86887525C9F2A6A5980A195EAA7F2FF910064301DEF89D3AA213E1FAC7768D89365318E370AF54A112EFBA9246D9158386BA1B4EEFDA")
            ),
            Y: fromHex("32969E5780CFE1C849A1C276D7AEB4F38A23B591739AA2FE197349AEEBD31366AEE5EB7E6C6DDB7C57D02432B30DB5AA66D9884299FAA72568944E4EEDC92EA3FBC6F39F53412FBCC563208F7C15B737AC8910DBC2D9C9B8C001E72FDC40EB694AB1F06A5A2DBD18D9E36C66F31F566742F11EC0A52E9F7B89355C02FB5D32D2")
        ),
        X: fromHex("5078D4D29795CBE76D3AACFE48C9AF0BCDBEE91A")
    ), out var Ꮡpriv);

    testSignAndVerify(t, 0, Ꮡpriv);
}

public static partial void TestSignAndVerifyWithBadPublicKey(ж<testing.T> t) {
    ref var pub = ref heap<global::go.crypto.dsa_package.PublicKey>(new PublicKey(
        Parameters: new Parameters(
            P: fromHex("A9B5B793FB4785793D246BAE77E8FF63CA52F442DA763C440259919FE1BC1D6065A9350637A04F75A2F039401D49F08E066C4D275A5A65DA5684BC563C14289D7AB8A67163BFBF79D85972619AD2CFF55AB0EE77A9002B0EF96293BDD0F42685EBB2C66C327079F6C98000FBCB79AACDE1BC6F9D5C7B1A97E3D9D54ED7951FEF"),
            Q: fromHex("FA"),
            G: fromHex("634364FC25248933D01D1993ECABD0657CC0CB2CEED7ED2E3E8AECDFCDC4A25C3B15E9E3B163ACA2984B5539181F3EFF1A5E8903D71D5B95DA4F27202B77D2C44B430BB53741A8D59A8F86887525C9F2A6A5980A195EAA7F2FF910064301DEF89D3AA213E1FAC7768D89365318E370AF54A112EFBA9246D9158386BA1B4EEFDA")
        ),
        Y: fromHex("32969E5780CFE1C849A1C276D7AEB4F38A23B591739AA2FE197349AEEBD31366AEE5EB7E6C6DDB7C57D02432B30DB5AA66D9884299FAA72568944E4EEDC92EA3FBC6F39F53412FBCC563208F7C15B737AC8910DBC2D9C9B8C001E72FDC40EB694AB1F06A5A2DBD18D9E36C66F31F566742F11EC0A52E9F7B89355C02FB5D32D2")
    ), out var Ꮡpub);

    if (Verify(Ꮡpub, slice<byte>("testing"u8), fromHex("2"), fromHex("4"))) {
        t.Errorf("Verify unexpected success with non-existent mod inverse of Q");
    }
}

internal partial struct TestSigningWithDegenerateKeys_badKeys /*dyn*/ {
    internal @string p, q, g, y, x;
}

public static void TestSigningWithDegenerateKeys(ж<testing.T> t) {
    // Signing with degenerate private keys should not cause an infinite
    // loop.
    var badKeys = slice<TestSigningWithDegenerateKeys_badKeys>([
        new("00", "01", "00", "00", "00"),
        new("01", "ff", "00", "00", "00")
    ]);

    foreach (var (i, test) in range(badKeys)) {
        ref var priv = ref heap<global::go.crypto.dsa_package.PrivateKey>(new PrivateKey(
            PublicKey: new PublicKey(
                Parameters: new Parameters(
                    P: fromHex(test.p),
                    Q: fromHex(test.q),
                    G: fromHex(test.g)
                ),
                Y: fromHex(test.y)
            ),
            X: fromHex(test.x)
        ), out var Ꮡpriv);

        var hashed = slice<byte>("testing"u8);
        var (_, _, err) = Sign(rand.Reader, Ꮡpriv, hashed); if (err == nil) {
            t.Errorf("#%d: unexpected success", i);
        }
    }
}

} // end dsa_internal_test_package
