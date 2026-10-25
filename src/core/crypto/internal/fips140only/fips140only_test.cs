// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using crypto = crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using dsa = go.crypto.dsa_package;
using ecdh = go.crypto.ecdh_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using hkdf = go.crypto.hkdf_package;
using hmac = go.crypto.hmac_package;
using hpke = go.crypto.hpke_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using Δfips140 = go.crypto.@internal.fips140_package;
using fips140only = go.crypto.@internal.fips140only_package;
using md5 = go.crypto.md5_package;
using mlkem = go.crypto.mlkem_package;
using mlkemtest = go.crypto.mlkem.mlkemtest_package;
using pbkdf2 = go.crypto.pbkdf2_package;
using rand = go.crypto.rand_package;
using rc4 = go.crypto.rc4_package;
using rsa = go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
// blank import: go.crypto.sha3_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.sha512_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using x509 = go.crypto.x509_package;
using pem = encoding.pem_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using big = math.big_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using chacha20poly1305 = vendor.golang.org.x.crypto.chacha20poly1305_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.mlkem;
using go.os;
using hash = hash_package;
using math;
using vendor.golang.org.x.crypto;

partial class fips140only_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRunTestFIPS140Onlyˢ = "-test.run=^TestFIPS140Only$"u8;
private static readonly @string testVˢ = "-test.v"u8;
private static readonly @string cryptocustomrand0ˢ = "cryptocustomrand=0"u8;
private static readonly @string godebugˢ = "GODEBUG"u8;
private static readonly @string cryptocustomrand1ˢ = "cryptocustomrand=1"u8;

public static void TestFIPS140Only(ж<testing.T> Ꮡt) {
    cryptotest.MustSupportFIPS140(Ꮡt);
    if (!fips140only.Enforced()) {
        var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), testRunTestFIPS140Onlyˢ, testVˢ);
        cmd.Value.Env = append(cmd.Environ(), "GODEBUG=fips140=only"u8);
        var (@out, err) = cmd.CombinedOutput();
        Ꮡt.Logf("running with GODEBUG=fips140=only:\n%s"u8, @out);
        if (err != default!) {
            Ꮡt.Errorf("fips140=only subprocess failed: %v"u8, err);
        }
        return;
    }
    Ꮡt.Run(cryptocustomrand0ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Setenv(godebugˢ, os.Getenv(godebugˢ) + ",cryptocustomrand=0"u8);
        testFIPS140Only(tΔ1);
    });
    Ꮡt.Run(cryptocustomrand1ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Setenv(godebugˢ, os.Getenv(godebugˢ) + ",cryptocustomrand=1"u8);
        testFIPS140Only(tΔ2);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fips140OnlyModeNotˢ = (@string)"FIPS 140-only mode not enforced"u8;
private static readonly @string passwordˢ = "password"u8;
private static readonly @string beginRsaTestingKeyˢ = """
-----BEGIN RSA TESTING KEY-----
MIICXQIBAAKBgQDMrln6XoAa3Rjts+kRi5obbP86qSf/562RcuDO+yMXeTLHfi4M
8ubyhoFY+UKBCGBLmmTO7ikbvQgdipkT3xVkU8nM3XTW4sxrnw0X5QXsl4PGlMo0
5UufxYyQxe7bbjuwFz2XnN6Jz4orpOfO0s36/KVHj9lZRl+REpr/Jy+nJQIDAQAB
AoGAJ9WEwGO01cWSzOwXH2mGX/EKCQ4TsUuS7XwogU/B6BcXyVhmuPFq/ecsdDbq
ePc62mvdU6JpELNsyWcIXKQtYsRgJHxNS+KJkCQIq6YeiAWRG0XL6q+qVj+HtT8a
1Qrmul9ZBd23Y9wLF8pg/xWDQYvb8DPAb/xJ0e/KEBZcWU8CQQDXFCFCGpCfwyxY
Cq8G/3B94D9UYwk5mK6jRIH5m8LbaX9bKKetf8+If8TWVgeuiRjjN4WEQ78lPoSg
3Fsz2qs3AkEA85/JCudNUf2FnY+T6h1c/2SWekZiZ1NS4lCh/C7iYuAN3oa8zGkf
gjjR5e0+Z8rUAcZkTukxyLLaNqy6rs9GgwJAVR6pXvEGhcQHe7yWso1LpvWl+q7L
StkrXIBTdEb54j4pYhl/6wFnUB1I+I7JsYCeseYaWFM7hfDtKoCrM6V6FwJBANxh
KmfmnJcSkw/YlaEuNrYAs+6gRNvbEBsRfba2Yqu2qlUl5Ruz7IDMDXPEjLMvU2DX
ql2HrTU0NRlIXwdLESkCQQDGJ54H6WK1eE1YvtxCaLm28zmogcFlvc21pym+PpM1
bXVL8iKLrG91IYQByUHZIn3WVAd2bfi4MfKagRt0ggd4
-----END RSA TESTING KEY-----
"""u8.Raw();
private static readonly object fips1403ModuleV100Doesˢ = (@string)"FIPS 140-3 Module v1.0.0 does not provide HPKE GCM modes"u8;

internal static void testFIPS140Only(ж<testing.T> Ꮡt) {
    if (!fips140only.Enforced()) {
        Ꮡt.Fatal(fips140OnlyModeNotˢ);
    }
    Ꮡt.Logf("GODEBUG=fips140=only enabled"u8);
    Δfips140.ResetServiceIndicator();

    var (aesBlock, err) = aes.NewCipher(new slice<byte>(16));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var notAESBlock = ref heap<blockWrap>(out var ᏑnotAESBlock);
    notAESBlock = new blockWrap(aesBlock);
    var iv = new slice<byte>(aes.BlockSize);

    cipher.NewCBCEncrypter(aesBlock, iv);
    var ivʗ1 = iv;
    var notAESBlockʗ1 = notAESBlock;
    expectPanic(Ꮡt, () => {
        cipher.NewCBCEncrypter(notAESBlockʗ1, ivʗ1);
    });
    cipher.NewCBCDecrypter(aesBlock, iv);
    var ivʗ2 = iv;
    var notAESBlockʗ2 = notAESBlock;
    expectPanic(Ꮡt, () => {
        cipher.NewCBCDecrypter(notAESBlockʗ2, ivʗ2);
    });

    var aesBlockʗ1 = aesBlock;
    var ivʗ3 = iv;
    expectPanic(Ꮡt, () => {
        cipher.NewCFBEncrypter(aesBlockʗ1, ivʗ3);
    });
    var aesBlockʗ2 = aesBlock;
    var ivʗ4 = iv;
    expectPanic(Ꮡt, () => {
        cipher.NewCFBDecrypter(aesBlockʗ2, ivʗ4);
    });

    cipher.NewCTR(aesBlock, iv);
    var ivʗ5 = iv;
    var notAESBlockʗ3 = notAESBlock;
    expectPanic(Ꮡt, () => {
        cipher.NewCTR(notAESBlockʗ3, ivʗ5);
    });

    var aesBlockʗ3 = aesBlock;
    var ivʗ6 = iv;
    expectPanic(Ꮡt, () => {
        cipher.NewOFB(aesBlockʗ3, ivʗ6);
    });

    var (ᴛ1, ᴛ2) = cipher.NewGCM(aesBlock);
    expectErr(Ꮡt, errRet2(ᴛ1, ᴛ2));
    var (ᴛ3, ᴛ4) = cipher.NewGCMWithNonceSize(aesBlock, 12);
    expectErr(Ꮡt, errRet2(ᴛ3, ᴛ4));
    var (ᴛ5, ᴛ6) = cipher.NewGCMWithTagSize(aesBlock, 12);
    expectErr(Ꮡt, errRet2(ᴛ5, ᴛ6));
    var (ᴛ7, ᴛ8) = cipher.NewGCMWithRandomNonce(aesBlock);
    expectNoErr(Ꮡt, errRet2(ᴛ7, ᴛ8));

    var (ᴛ9, ᴛ10) = des.NewCipher(new slice<byte>(8));
    expectErr(Ꮡt, errRet2(ᴛ9, ᴛ10));
    var (ᴛ11, ᴛ12) = des.NewTripleDESCipher(new slice<byte>(24));
    expectErr(Ꮡt, errRet2(ᴛ11, ᴛ12));

    var (ᴛ13, ᴛ14) = rc4.NewCipher(new slice<byte>(16));
    expectErr(Ꮡt, errRet2(ᴛ13, ᴛ14));

    var (ᴛ15, ᴛ16) = chacha20poly1305.New(new slice<byte>(chacha20poly1305.KeySize));
    expectErr(Ꮡt, errRet2(ᴛ15, ᴛ16));
    var (ᴛ17, ᴛ18) = chacha20poly1305.NewX(new slice<byte>(chacha20poly1305.KeySize));
    expectErr(Ꮡt, errRet2(ᴛ17, ᴛ18));

    expectPanic(Ꮡt, () => {
        md5.New().Sum(default!);
    });
    var (ᴛ19, ᴛ20) = md5.New().Write(new slice<byte>(16));
    expectErr(Ꮡt, errRet2(ᴛ19, ᴛ20));
    expectPanic(Ꮡt, () => {
        md5.Sum(slice<byte>("foo"u8));
    });

    expectPanic(Ꮡt, () => {
        sha1.New().Sum(default!);
    });
    var (ᴛ21, ᴛ22) = sha1.New().Write(new slice<byte>(16));
    expectErr(Ꮡt, errRet2(ᴛ21, ᴛ22));
    expectPanic(Ꮡt, () => {
        sha1.Sum(slice<byte>("foo"u8));
    });

    withApprovedHash((crypto.Hash h) => {
        h.New().Sum(default!);
    });
    withNonApprovedHash((crypto.Hash h) => {
        expectPanic(Ꮡt, () => {
            h.New().Sum(default!);
        });
    });

    var (ᴛ23, ᴛ24) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, new slice<byte>(16), 1, 10);
    expectErr(Ꮡt, errRet2(ᴛ23, ᴛ24));
    var (ᴛ25, ᴛ26) = pbkdf2.Key<hash.Hash>(sha256.New, passwordˢ, new slice<byte>(10), 1, 14);
    expectErr(Ꮡt, errRet2(ᴛ25, ᴛ26));
    withNonApprovedHash((crypto.Hash h) => {
        var hʗ1 = h;

        var (ᴛ27, ᴛ28) = pbkdf2.Key<hash.Hash>(() => hʗ1.New(), passwordˢ, new slice<byte>(16), 1, 14);
        expectErr(Ꮡt, errRet2(ᴛ27, ᴛ28));
    });
    withApprovedHash([MethodImpl(MethodImplOptions.NoInlining)] (crypto.Hash h) => {
        var hʗ2 = h;

        var (ᴛ29, ᴛ30) = pbkdf2.Key<hash.Hash>(() => hʗ2.New(), passwordˢ, new slice<byte>(16), 1, 14);
        expectNoErr(Ꮡt, errRet2(ᴛ29, ᴛ30));
    });

    expectPanic(Ꮡt, () => {
        hmac.New(sha256.New, new slice<byte>(10));
    });
    withNonApprovedHash((crypto.Hash h) => {
        expectPanic(Ꮡt, () => {
            var hʗ3 = h;
            hmac.New(() => hʗ3.New(), new slice<byte>(16));
        });
    });
    withApprovedHash((crypto.Hash h) => {
        var hʗ4 = h;
        hmac.New(() => hʗ4.New(), new slice<byte>(16));
    });

    var (ᴛ31, ᴛ32) = hkdf.Key<hash.Hash>(sha256.New, new slice<byte>(10), default!, ""u8, 16);
    expectErr(Ꮡt, errRet2(ᴛ31, ᴛ32));
    withNonApprovedHash((crypto.Hash h) => {
        var hʗ5 = h;

        var (ᴛ33, ᴛ34) = hkdf.Key<hash.Hash>(() => hʗ5.New(), new slice<byte>(16), default!, ""u8, 16);
        expectErr(Ꮡt, errRet2(ᴛ33, ᴛ34));
    });
    withApprovedHash([MethodImpl(MethodImplOptions.NoInlining)] (crypto.Hash h) => {
        var hʗ6 = h;

        var (ᴛ35, ᴛ36) = hkdf.Key<hash.Hash>(() => hʗ6.New(), new slice<byte>(16), default!, ""u8, 16);
        expectNoErr(Ꮡt, errRet2(ᴛ35, ᴛ36));
    });

    var (ᴛ37, ᴛ38) = hkdf.Extract<hash.Hash>(sha256.New, new slice<byte>(10), default!);
    expectErr(Ꮡt, errRet2(ᴛ37, ᴛ38));
    withNonApprovedHash((crypto.Hash h) => {
        var hʗ7 = h;

        var (ᴛ39, ᴛ40) = hkdf.Extract<hash.Hash>(() => hʗ7.New(), new slice<byte>(16), default!);
        expectErr(Ꮡt, errRet2(ᴛ39, ᴛ40));
    });
    withApprovedHash([MethodImpl(MethodImplOptions.NoInlining)] (crypto.Hash h) => {
        var hʗ8 = h;

        var (ᴛ41, ᴛ42) = hkdf.Extract<hash.Hash>(() => hʗ8.New(), new slice<byte>(16), default!);
        expectNoErr(Ꮡt, errRet2(ᴛ41, ᴛ42));
    });

    var (ᴛ43, ᴛ44) = hkdf.Expand<hash.Hash>(sha256.New, new slice<byte>(10), ""u8, 16);
    expectErr(Ꮡt, errRet2(ᴛ43, ᴛ44));
    withNonApprovedHash((crypto.Hash h) => {
        var hʗ9 = h;

        var (ᴛ45, ᴛ46) = hkdf.Expand<hash.Hash>(() => hʗ9.New(), new slice<byte>(16), ""u8, 16);
        expectErr(Ꮡt, errRet2(ᴛ45, ᴛ46));
    });
    withApprovedHash([MethodImpl(MethodImplOptions.NoInlining)] (crypto.Hash h) => {
        var hʗ10 = h;

        var (ᴛ47, ᴛ48) = hkdf.Expand<hash.Hash>(() => hʗ10.New(), new slice<byte>(16), ""u8, 16);
        expectNoErr(Ꮡt, errRet2(ᴛ47, ᴛ48));
    });

    var (ᴛ49, ᴛ50) = rand.Prime(rand.Reader, 10);
    expectErr(Ꮡt, errRet2(ᴛ49, ᴛ50));

    expectErr(Ꮡt, dsa.GenerateParameters(Ꮡ(new dsa.Parameters(nil)), rand.Reader, dsa.L1024N160));
    expectErr(Ꮡt, dsa.GenerateKey(Ꮡ(new dsa.PrivateKey(nil)), rand.Reader));
    var (ᴛ51, ᴛ52, ᴛ53) = dsa.Sign(rand.Reader, Ꮡ(new dsa.PrivateKey(nil)), new slice<byte>(16));
    expectErr(Ꮡt, errRet3(ᴛ51, ᴛ52, ᴛ53));
    expectPanic(Ꮡt, () => {
        dsa.Verify(Ꮡ(new dsa.PublicKey(nil)), new slice<byte>(16), big.NewInt(1), big.NewInt(1));
    });

    var (ᴛ54, ᴛ55) = ecdh.X25519().GenerateKey(rand.Reader);
    expectErr(Ꮡt, errRet2(ᴛ54, ᴛ55));
    var (ᴛ56, ᴛ57) = ecdh.X25519().NewPrivateKey(new slice<byte>(32));
    expectErr(Ꮡt, errRet2(ᴛ56, ᴛ57));
    var (ᴛ58, ᴛ59) = ecdh.X25519().NewPublicKey(new slice<byte>(32));
    expectErr(Ꮡt, errRet2(ᴛ58, ᴛ59));
    foreach (var (_, curve) in new ecdhꓸCurve[]{ecdh.P256(), ecdh.P384(), ecdh.P521()}.slice()) {
        var (ᴛ60, ᴛ61) = curve.GenerateKey(new readerWrap(rand.Reader));
        expectErrIfCustomRand(Ꮡt, errRet2(ᴛ60, ᴛ61));
        var (kΔ1, errΔ1) = curve.GenerateKey(rand.Reader);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        var (ᴛ62, ᴛ63) = curve.NewPrivateKey(kΔ1.Bytes());
        expectNoErr(Ꮡt, errRet2(ᴛ62, ᴛ63));
        var (ᴛ64, ᴛ65) = curve.NewPublicKey(kΔ1.PublicKey().Bytes());
        expectNoErr(Ꮡt, errRet2(ᴛ64, ᴛ65));
    }

    foreach (var (_, curve) in new elliptic.Curve[]{elliptic.P256(), elliptic.P384(), elliptic.P521()}.slice()) {
        var (ᴛ66, ᴛ67) = ecdsa.GenerateKey(curve, new readerWrap(rand.Reader));
        expectErrIfCustomRand(Ꮡt, errRet2(ᴛ66, ᴛ67));
        var (kΔ2, errΔ2) = ecdsa.GenerateKey(curve, rand.Reader);
        if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }

        var (ᴛ68, ᴛ69) = kΔ2.Sign(new readerWrap(rand.Reader), new slice<byte>(32), default!);
        expectErrIfCustomRand(Ꮡt, errRet2(ᴛ68, ᴛ69));
        var (ᴛ70, ᴛ71) = ecdsa.SignASN1(new readerWrap(rand.Reader), kΔ2, new slice<byte>(32));
        expectErrIfCustomRand(Ꮡt, errRet2(ᴛ70, ᴛ71));
        var (ᴛ72, ᴛ73, ᴛ74) = ecdsa.Sign(new readerWrap(rand.Reader), kΔ2, new slice<byte>(32));
        expectErrIfCustomRand(Ꮡt, errRet3(ᴛ72, ᴛ73, ᴛ74));
        var (ᴛ75, ᴛ76) = kΔ2.Sign(rand.Reader, new slice<byte>(32), default!);
        expectNoErr(Ꮡt, errRet2(ᴛ75, ᴛ76));
        var (ᴛ77, ᴛ78) = ecdsa.SignASN1(rand.Reader, kΔ2, new slice<byte>(32));
        expectNoErr(Ꮡt, errRet2(ᴛ77, ᴛ78));
        var (ᴛ79, ᴛ80, ᴛ81) = ecdsa.Sign(rand.Reader, kΔ2, new slice<byte>(32));
        expectNoErr(Ꮡt, errRet3(ᴛ79, ᴛ80, ᴛ81));

        var kʗ1 = kΔ2;
        withNonApprovedHash((crypto.Hash h) => {
            var (ᴛ82, ᴛ83) = kʗ1.Sign(default!, new slice<byte>(h.Size()), h);
            expectErr(Ꮡt, errRet2(ᴛ82, ᴛ83));
        });
        var kʗ2 = kΔ2;
        withApprovedHash([MethodImpl(MethodImplOptions.NoInlining)] (crypto.Hash h) => {
            var (ᴛ84, ᴛ85) = kʗ2.Sign(default!, new slice<byte>(h.Size()), h);
            expectNoErr(Ꮡt, errRet2(ᴛ84, ᴛ85));
        });
    }
    var customCurve = Ꮡ(new elliptic.CurveParams(Name: "custom"u8, P: big.NewInt(1)));
    var (ᴛ86, ᴛ87) = ecdsa.GenerateKey(new elliptic.CurveParamsжCurve(customCurve), rand.Reader);
    expectErr(Ꮡt, errRet2(ᴛ86, ᴛ87));

    (_, var ed25519Key, err) = ed25519.GenerateKey(rand.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (ᴛ88, ᴛ89) = ed25519Key.Sign(default!, new slice<byte>(32), (crypto.Hash)0);
    expectNoErr(Ꮡt, errRet2(ᴛ88, ᴛ89));
    var (ᴛ90, ᴛ91) = ed25519Key.Sign(default!, new slice<byte>(64), crypto.SHA512);
    expectNoErr(Ꮡt, errRet2(ᴛ90, ᴛ91));
    // ed25519ctx is not allowed (but ed25519ph with context is).
    var (ᴛ92, ᴛ93) = ed25519Key.Sign(default!, new slice<byte>(32), new ed25519_OptionsжSignerOpts(Ꮡ(new ed25519.Options(
        Context: "test"u8
    ))));
    expectErr(Ꮡt, errRet2(ᴛ92, ᴛ93));
    var (ᴛ94, ᴛ95) = ed25519Key.Sign(default!, new slice<byte>(64), new ed25519_OptionsжSignerOpts(Ꮡ(new ed25519.Options(
        Hash: crypto.SHA512, Context: "test"u8
    ))));
    expectNoErr(Ꮡt, errRet2(ᴛ94, ᴛ95));
    var (ᴛ96, ᴛ97) = ed25519Key.Sign(default!, new slice<byte>(64), new ed25519_OptionsжSignerOpts(Ꮡ(new ed25519.Options(
        Hash: crypto.SHA512
    ))));
    expectNoErr(Ꮡt, errRet2(ᴛ96, ᴛ97));

    var (ᴛ98, ᴛ99) = rsa.GenerateMultiPrimeKey(rand.Reader, 3, 2048);
    expectErr(Ꮡt, errRet2(ᴛ98, ᴛ99));
    var (ᴛ100, ᴛ101) = rsa.GenerateKey(rand.Reader, 1024);
    expectErr(Ꮡt, errRet2(ᴛ100, ᴛ101));
    var (ᴛ102, ᴛ103) = rsa.GenerateKey(rand.Reader, 2049);
    expectErr(Ꮡt, errRet2(ᴛ102, ᴛ103));
    var (ᴛ104, ᴛ105) = rsa.GenerateKey(new readerWrap(rand.Reader), 2048);
    expectErrIfCustomRand(Ꮡt, errRet2(ᴛ104, ᴛ105));
    (var rsaKey, err) = rsa.GenerateKey(rand.Reader, 2048);
    expectNoErr(Ꮡt, err);

    var smallKey = parseKey(testingKey(beginRsaTestingKeyˢ));

    var (ᴛ106, ᴛ107) = rsaKey.Sign(rand.Reader, new slice<byte>(32), crypto.SHA256);
    expectNoErr(Ꮡt, errRet2(ᴛ106, ᴛ107));
    var (ᴛ108, ᴛ109) = smallKey.Sign(rand.Reader, new slice<byte>(32), crypto.SHA256);
    expectErr(Ꮡt, errRet2(ᴛ108, ᴛ109));
    var (ᴛ110, ᴛ111) = rsaKey.Sign(rand.Reader, new slice<byte>(20), crypto.SHA1);
    expectErr(Ꮡt, errRet2(ᴛ110, ᴛ111));
    // rand is always ignored for PKCS1v15 signing
    var (ᴛ112, ᴛ113) = rsaKey.Sign(new readerWrap(rand.Reader), new slice<byte>(32), crypto.SHA256);
    expectNoErr(Ꮡt, errRet2(ᴛ112, ᴛ113));

    (var sigPKCS1v15, err) = rsa.SignPKCS1v15(rand.Reader, rsaKey, crypto.SHA256, new slice<byte>(32));
    expectNoErr(Ꮡt, err);
    var (ᴛ114, ᴛ115) = rsa.SignPKCS1v15(rand.Reader, smallKey, crypto.SHA256, new slice<byte>(32));
    expectErr(Ꮡt, errRet2(ᴛ114, ᴛ115));
    var (ᴛ116, ᴛ117) = rsa.SignPKCS1v15(rand.Reader, rsaKey, crypto.SHA1, new slice<byte>(20));
    expectErr(Ꮡt, errRet2(ᴛ116, ᴛ117));
    // rand is always ignored for PKCS1v15 signing
    var (ᴛ118, ᴛ119) = rsa.SignPKCS1v15(new readerWrap(rand.Reader), rsaKey, crypto.SHA256, new slice<byte>(32));
    expectNoErr(Ꮡt, errRet2(ᴛ118, ᴛ119));

    expectNoErr(Ꮡt, rsa.VerifyPKCS1v15(rsaKey.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, new slice<byte>(32), sigPKCS1v15));
    expectErr(Ꮡt, rsa.VerifyPKCS1v15(smallKey.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, new slice<byte>(32), sigPKCS1v15));
    expectErr(Ꮡt, rsa.VerifyPKCS1v15(rsaKey.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA1, new slice<byte>(20), sigPKCS1v15));

    (var sigPSS, err) = rsa.SignPSS(rand.Reader, rsaKey, crypto.SHA256, new slice<byte>(32), nil);
    expectNoErr(Ꮡt, err);
    var (ᴛ120, ᴛ121) = rsa.SignPSS(rand.Reader, smallKey, crypto.SHA256, new slice<byte>(32), nil);
    expectErr(Ꮡt, errRet2(ᴛ120, ᴛ121));
    var (ᴛ122, ᴛ123) = rsa.SignPSS(rand.Reader, rsaKey, crypto.SHA1, new slice<byte>(20), nil);
    expectErr(Ꮡt, errRet2(ᴛ122, ᴛ123));
    var (ᴛ124, ᴛ125) = rsa.SignPSS(new readerWrap(rand.Reader), rsaKey, crypto.SHA256, new slice<byte>(32), nil);
    expectErr(Ꮡt, errRet2(ᴛ124, ᴛ125));

    expectNoErr(Ꮡt, rsa.VerifyPSS(rsaKey.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, new slice<byte>(32), sigPSS, nil));
    expectErr(Ꮡt, rsa.VerifyPSS(smallKey.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, new slice<byte>(32), sigPSS, nil));
    expectErr(Ꮡt, rsa.VerifyPSS(rsaKey.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA1, new slice<byte>(20), sigPSS, nil));

    (var k, err) = mlkem.GenerateKey768();
    expectNoErr(Ꮡt, err);
    var (ᴛ126, ᴛ127, ᴛ128) = mlkemtest.Encapsulate768(k.EncapsulationKey(), new slice<byte>(32));
    expectErr(Ꮡt, errRet3(ᴛ126, ᴛ127, ᴛ128));
    (var k1024, err) = mlkem.GenerateKey1024();
    expectNoErr(Ꮡt, err);
    var (ᴛ129, ᴛ130, ᴛ131) = mlkemtest.Encapsulate1024(k1024.EncapsulationKey(), new slice<byte>(32));
    expectErr(Ꮡt, errRet3(ᴛ129, ᴛ130, ᴛ131));

    foreach (var (_, kem) in new hpkeꓸKEM[]{hpke.DHKEM(ecdh.P256()), hpke.DHKEM(ecdh.P384()), hpke.DHKEM(ecdh.P521()), hpke.MLKEM768(), hpke.MLKEM1024(), hpke.MLKEM768P256(), hpke.MLKEM1024P384(), hpke.MLKEM768X25519()
    }.slice()) {
        // allowed as hybrid
        var kemʗ1 = kem;
        Ꮡt.Run(fmt.Sprintf("HKPE KEM %04x"u8, kem.ID()), (ж<testing.T> tΔ1) => {
            var (kΔ3, errΔ3) = kemʗ1.GenerateKey();
            expectNoErr(tΔ1, errΔ3);
            var (ᴛ132, ᴛ133) = kemʗ1.DeriveKeyPair(new slice<byte>(64));
            expectNoErr(tΔ1, errRet2(ᴛ132, ᴛ133));
            (var kb, errΔ3) = kΔ3.Bytes();
            expectNoErr(tΔ1, errΔ3);
            var (ᴛ134, ᴛ135) = kemʗ1.NewPrivateKey(kb);
            expectNoErr(tΔ1, errRet2(ᴛ134, ᴛ135));
            var (ᴛ136, ᴛ137) = kemʗ1.NewPublicKey(kΔ3.PublicKey().Bytes());
            expectNoErr(tΔ1, errRet2(ᴛ136, ᴛ137));
            if (Δfips140.Version() == "v1.0.0"u8) {
                tΔ1.Skip(fips1403ModuleV100Doesˢ);
            }
            (var c, errΔ3) = hpke.Seal(kΔ3.PublicKey(), hpke.HKDFSHA256(), hpke.AES128GCM(), default!, default!);
            expectNoErr(tΔ1, errΔ3);
            (_, errΔ3) = hpke.Open(kΔ3, hpke.HKDFSHA256(), hpke.AES128GCM(), default!, c);
            expectNoErr(tΔ1, errΔ3);
        });
    }
    var (ᴛ138, ᴛ139) = hpke.DHKEM(ecdh.X25519()).GenerateKey();
    expectErr(Ꮡt, errRet2(ᴛ138, ᴛ139));
    var (ᴛ140, ᴛ141) = hpke.DHKEM(ecdh.X25519()).DeriveKeyPair(new slice<byte>(64));
    expectErr(Ꮡt, errRet2(ᴛ140, ᴛ141));
    var (ᴛ142, ᴛ143) = hpke.DHKEM(ecdh.X25519()).NewPrivateKey(new slice<byte>(32));
    expectErr(Ꮡt, errRet2(ᴛ142, ᴛ143));
    var (ᴛ144, ᴛ145) = hpke.DHKEM(ecdh.X25519()).NewPublicKey(new slice<byte>(32));
    expectErr(Ꮡt, errRet2(ᴛ144, ᴛ145));
    (var hpkeK, err) = hpke.MLKEM768().GenerateKey();
    expectNoErr(Ꮡt, err);
    var (ᴛ146, ᴛ147) = hpke.Seal(hpkeK.PublicKey(), hpke.HKDFSHA256(), hpke.ChaCha20Poly1305(), default!, default!);
    expectErr(Ꮡt, errRet2(ᴛ146, ᴛ147));
    var (ᴛ148, ᴛ149) = hpke.Open(hpkeK, hpke.HKDFSHA256(), hpke.ChaCha20Poly1305(), default!, new slice<byte>(2000));
    expectErr(Ꮡt, errRet2(ᴛ148, ᴛ149));

    // fips140=only mode should prevent any operation that would make the FIPS
    // 140-3 module set its service indicator to false.
    if (!Δfips140.ServiceIndicator()) {
        Ꮡt.Errorf("service indicator not set"u8);
    }
}

partial struct blockWrap {
    /*embed*/ public go.crypto.cipher_package.Block Block;
}

partial struct readerWrap {
    /*embed*/ public io_package.Reader Reader;
}

internal static void withApprovedHash(Action<crypto.Hash> f) {
    f(crypto.SHA224);
    f(crypto.SHA256);
    f(crypto.SHA384);
    f(crypto.SHA512);
    f(crypto.SHA3_224);
    f(crypto.SHA3_256);
    f(crypto.SHA3_384);
    f(crypto.SHA3_512);
    f(crypto.SHA512_224);
    f(crypto.SHA512_256);
}

internal static void withNonApprovedHash(Action<crypto.Hash> f) {
    f(crypto.MD5);
    f(crypto.SHA1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fips140Onlyˢ = "FIPS 140-only"u8;

internal static void expectPanic(ж<testing.T> Ꮡt, Action f) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        defer(() => {
            Ꮡt.Helper();
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Errorf("expected panic"u8);
                } else {
                    {
                        var (s, ok) = err._<@string>(ᐧ); if (!ok || !strings.Contains(s, fips140Onlyˢ)) {
                            Ꮡt.Errorf("unexpected panic: %v"u8, err);
                        }
                    }
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<godebug.Setting> cryptocustomrand = godebug.New("cryptocustomrand"u8);

internal static void expectErr(ж<testing.T> Ꮡt, error err) {
    Ꮡt.Helper();
    if (err == default!){
        Ꮡt.Errorf("expected error"u8);
    } else 
    if (!strings.Contains(err.Error(), fips140Onlyˢ)) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }
}

internal static partial void expectNoErr(ж<testing.T> Ꮡt, error err) {
    Ꮡt.Helper();
    if (err != default!) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }
}

internal static void expectErrIfCustomRand(ж<testing.T> Ꮡt, error err) {
    Ꮡt.Helper();
    if (cryptocustomrand.Value() == "1"u8){
        expectErr(Ꮡt, err);
    } else {
        expectNoErr(Ꮡt, err);
    }
}

internal static error errRet2<T>(T _, error err) {
    return err;
}

internal static error errRet3<T>(T _Δp0, T _Δp1, error err) {
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testingKeyˢ = "TESTING KEY"u8;
private static readonly @string privateKeyˢ = "PRIVATE KEY"u8;

internal static @string testingKey(@string s) {
    return strings.ReplaceAll(s, testingKeyˢ, privateKeyˢ);
}

internal static ж<rsa.PrivateKey> parseKey(@string s) {
    var (p, _) = pem.Decode(slice<byte>(s));
    var (k, err) = x509.ParsePKCS1PrivateKey((~p).Bytes);
    if (err != default!) {
        throw panic(err);
    }
    return k;
}

} // end fips140only_test_package
