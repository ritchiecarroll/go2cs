// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using hex = encoding.hex_package;
using fmt = fmt_package;
using big = math.big_package;
using testing = testing_package;
using encoding;
using math;
using static go.crypto.elliptic_package;

partial class elliptic_internal_test_package {

internal partial struct baseMultTest {
    internal @string k;
    internal @string x, y;
}

internal static slice<baseMultTest> p224BaseMultTests = new baseMultTest[]{
    new(
        "1"u8,
        "b70e0cbd6bb4bf7f321390b94a03c1d356c21122343280d6115c1d21"u8,
        "bd376388b5f723fb4c22dfe6cd4375a05a07476444d5819985007e34"u8
    ),
    new(
        "2"u8,
        "706a46dc76dcb76798e60e6d89474788d16dc18032d268fd1a704fa6"u8,
        "1c2b76a7bc25e7702a704fa986892849fca629487acf3709d2e4e8bb"u8
    ),
    new(
        "3"u8,
        "df1b1d66a551d0d31eff822558b9d2cc75c2180279fe0d08fd896d04"u8,
        "a3f7f03cadd0be444c0aa56830130ddf77d317344e1af3591981a925"u8
    ),
    new(
        "4"u8,
        "ae99feebb5d26945b54892092a8aee02912930fa41cd114e40447301"u8,
        "482580a0ec5bc47e88bc8c378632cd196cb3fa058a7114eb03054c9"u8
    ),
    new(
        "5"u8,
        "31c49ae75bce7807cdff22055d94ee9021fedbb5ab51c57526f011aa"u8,
        "27e8bff1745635ec5ba0c9f1c2ede15414c6507d29ffe37e790a079b"u8
    ),
    new(
        "6"u8,
        "1f2483f82572251fca975fea40db821df8ad82a3c002ee6c57112408"u8,
        "89faf0ccb750d99b553c574fad7ecfb0438586eb3952af5b4b153c7e"u8
    ),
    new(
        "7"u8,
        "db2f6be630e246a5cf7d99b85194b123d487e2d466b94b24a03c3e28"u8,
        "f3a30085497f2f611ee2517b163ef8c53b715d18bb4e4808d02b963"u8
    ),
    new(
        "8"u8,
        "858e6f9cc6c12c31f5df124aa77767b05c8bc021bd683d2b55571550"u8,
        "46dcd3ea5c43898c5c5fc4fdac7db39c2f02ebee4e3541d1e78047a"u8
    ),
    new(
        "9"u8,
        "2fdcccfee720a77ef6cb3bfbb447f9383117e3daa4a07e36ed15f78d"u8,
        "371732e4f41bf4f7883035e6a79fcedc0e196eb07b48171697517463"u8
    ),
    new(
        "10"u8,
        "aea9e17a306517eb89152aa7096d2c381ec813c51aa880e7bee2c0fd"u8,
        "39bb30eab337e0a521b6cba1abe4b2b3a3e524c14a3fe3eb116b655f"u8
    ),
    new(
        "11"u8,
        "ef53b6294aca431f0f3c22dc82eb9050324f1d88d377e716448e507c"u8,
        "20b510004092e96636cfb7e32efded8265c266dfb754fa6d6491a6da"u8
    ),
    new(
        "12"u8,
        "6e31ee1dc137f81b056752e4deab1443a481033e9b4c93a3044f4f7a"u8,
        "207dddf0385bfdeab6e9acda8da06b3bbef224a93ab1e9e036109d13"u8
    ),
    new(
        "13"u8,
        "34e8e17a430e43289793c383fac9774247b40e9ebd3366981fcfaeca"u8,
        "252819f71c7fb7fbcb159be337d37d3336d7feb963724fdfb0ecb767"u8
    ),
    new(
        "14"u8,
        "a53640c83dc208603ded83e4ecf758f24c357d7cf48088b2ce01e9fa"u8,
        "d5814cd724199c4a5b974a43685fbf5b8bac69459c9469bc8f23ccaf"u8
    ),
    new(
        "15"u8,
        "baa4d8635511a7d288aebeedd12ce529ff102c91f97f867e21916bf9"u8,
        "979a5f4759f80f4fb4ec2e34f5566d595680a11735e7b61046127989"u8
    ),
    new(
        "16"u8,
        "b6ec4fe1777382404ef679997ba8d1cc5cd8e85349259f590c4c66d"u8,
        "3399d464345906b11b00e363ef429221f2ec720d2f665d7dead5b482"u8
    ),
    new(
        "17"u8,
        "b8357c3a6ceef288310e17b8bfeff9200846ca8c1942497c484403bc"u8,
        "ff149efa6606a6bd20ef7d1b06bd92f6904639dce5174db6cc554a26"u8
    ),
    new(
        "18"u8,
        "c9ff61b040874c0568479216824a15eab1a838a797d189746226e4cc"u8,
        "ea98d60e5ffc9b8fcf999fab1df7e7ef7084f20ddb61bb045a6ce002"u8
    ),
    new(
        "19"u8,
        "a1e81c04f30ce201c7c9ace785ed44cc33b455a022f2acdbc6cae83c"u8,
        "dcf1f6c3db09c70acc25391d492fe25b4a180babd6cea356c04719cd"u8
    ),
    new(
        "20"u8,
        "fcc7f2b45df1cd5a3c0c0731ca47a8af75cfb0347e8354eefe782455"u8,
        "d5d7110274cba7cdee90e1a8b0d394c376a5573db6be0bf2747f530"u8
    ),
    new(
        "112233445566778899"u8,
        "61f077c6f62ed802dad7c2f38f5c67f2cc453601e61bd076bb46179e"u8,
        "2272f9e9f5933e70388ee652513443b5e289dd135dcc0d0299b225e4"u8
    ),
    new(
        "112233445566778899112233445566778899"u8,
        "29895f0af496bfc62b6ef8d8a65c88c613949b03668aab4f0429e35"u8,
        "3ea6e53f9a841f2019ec24bde1a75677aa9b5902e61081c01064de93"u8
    ),
    new(
        "6950511619965839450988900688150712778015737983940691968051900319680"u8,
        "ab689930bcae4a4aa5f5cb085e823e8ae30fd365eb1da4aba9cf0379"u8,
        "3345a121bbd233548af0d210654eb40bab788a03666419be6fbd34e7"u8
    ),
    new(
        "13479972933410060327035789020509431695094902435494295338570602119423"u8,
        "bdb6a8817c1f89da1c2f3dd8e97feb4494f2ed302a4ce2bc7f5f4025"u8,
        "4c7020d57c00411889462d77a5438bb4e97d177700bf7243a07f1680"u8
    ),
    new(
        "13479971751745682581351455311314208093898607229429740618390390702079"u8,
        "d58b61aa41c32dd5eba462647dba75c5d67c83606c0af2bd928446a9"u8,
        "d24ba6a837be0460dd107ae77725696d211446c5609b4595976b16bd"u8
    ),
    new(
        "13479972931865328106486971546324465392952975980343228160962702868479"u8,
        "dc9fa77978a005510980e929a1485f63716df695d7a0c18bb518df03"u8,
        "ede2b016f2ddffc2a8c015b134928275ce09e5661b7ab14ce0d1d403"u8
    ),
    new(
        "11795773708834916026404142434151065506931607341523388140225443265536"u8,
        "499d8b2829cfb879c901f7d85d357045edab55028824d0f05ba279ba"u8,
        "bf929537b06e4015919639d94f57838fa33fc3d952598dcdbb44d638"u8
    ),
    new(
        "784254593043826236572847595991346435467177662189391577090"u8,
        "8246c999137186632c5f9eddf3b1b0e1764c5e8bd0e0d8a554b9cb77"u8,
        "e80ed8660bc1cb17ac7d845be40a7a022d3306f116ae9f81fea65947"u8
    ),
    new(
        "13479767645505654746623887797783387853576174193480695826442858012671"u8,
        "6670c20afcceaea672c97f75e2e9dd5c8460e54bb38538ebb4bd30eb"u8,
        "f280d8008d07a4caf54271f993527d46ff3ff46fd1190a3f1faa4f74"u8
    ),
    new(
        "205688069665150753842126177372015544874550518966168735589597183"u8,
        "eca934247425cfd949b795cb5ce1eff401550386e28d1a4c5a8eb"u8,
        "d4c01040dba19628931bc8855370317c722cbd9ca6156985f1c2e9ce"u8
    ),
    new(
        "13479966930919337728895168462090683249159702977113823384618282123295"u8,
        "ef353bf5c73cd551b96d596fbc9a67f16d61dd9fe56af19de1fba9cd"u8,
        "21771b9cdce3e8430c09b3838be70b48c21e15bc09ee1f2d7945b91f"u8
    ),
    new(
        "50210731791415612487756441341851895584393717453129007497216"u8,
        "4036052a3091eb481046ad3289c95d3ac905ca0023de2c03ecd451cf"u8,
        "d768165a38a2b96f812586a9d59d4136035d9c853a5bf2e1c86a4993"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368041"u8,
        "fcc7f2b45df1cd5a3c0c0731ca47a8af75cfb0347e8354eefe782455"u8,
        "f2a28eefd8b345832116f1e574f2c6b2c895aa8c24941f40d8b80ad1"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368042"u8,
        "a1e81c04f30ce201c7c9ace785ed44cc33b455a022f2acdbc6cae83c"u8,
        "230e093c24f638f533dac6e2b6d01da3b5e7f45429315ca93fb8e634"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368043"u8,
        "c9ff61b040874c0568479216824a15eab1a838a797d189746226e4cc"u8,
        "156729f1a003647030666054e208180f8f7b0df2249e44fba5931fff"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368044"u8,
        "b8357c3a6ceef288310e17b8bfeff9200846ca8c1942497c484403bc"u8,
        "eb610599f95942df1082e4f9426d086fb9c6231ae8b24933aab5db"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368045"u8,
        "b6ec4fe1777382404ef679997ba8d1cc5cd8e85349259f590c4c66d"u8,
        "cc662b9bcba6f94ee4ff1c9c10bd6ddd0d138df2d099a282152a4b7f"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368046"u8,
        "baa4d8635511a7d288aebeedd12ce529ff102c91f97f867e21916bf9"u8,
        "6865a0b8a607f0b04b13d1cb0aa992a5a97f5ee8ca1849efb9ed8678"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368047"u8,
        "a53640c83dc208603ded83e4ecf758f24c357d7cf48088b2ce01e9fa"u8,
        "2a7eb328dbe663b5a468b5bc97a040a3745396ba636b964370dc3352"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368048"u8,
        "34e8e17a430e43289793c383fac9774247b40e9ebd3366981fcfaeca"u8,
        "dad7e608e380480434ea641cc82c82cbc92801469c8db0204f13489a"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368049"u8,
        "6e31ee1dc137f81b056752e4deab1443a481033e9b4c93a3044f4f7a"u8,
        "df82220fc7a4021549165325725f94c3410ddb56c54e161fc9ef62ee"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368050"u8,
        "ef53b6294aca431f0f3c22dc82eb9050324f1d88d377e716448e507c"u8,
        "df4aefffbf6d1699c930481cd102127c9a3d992048ab05929b6e5927"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368051"u8,
        "aea9e17a306517eb89152aa7096d2c381ec813c51aa880e7bee2c0fd"u8,
        "c644cf154cc81f5ade49345e541b4d4b5c1adb3eb5c01c14ee949aa2"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368052"u8,
        "2fdcccfee720a77ef6cb3bfbb447f9383117e3daa4a07e36ed15f78d"u8,
        "c8e8cd1b0be40b0877cfca1958603122f1e6914f84b7e8e968ae8b9e"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368053"u8,
        "858e6f9cc6c12c31f5df124aa77767b05c8bc021bd683d2b55571550"u8,
        "fb9232c15a3bc7673a3a03b0253824c53d0fd1411b1cabe2e187fb87"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368054"u8,
        "db2f6be630e246a5cf7d99b85194b123d487e2d466b94b24a03c3e28"u8,
        "f0c5cff7ab680d09ee11dae84e9c1072ac48ea2e744b1b7f72fd469e"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368055"u8,
        "1f2483f82572251fca975fea40db821df8ad82a3c002ee6c57112408"u8,
        "76050f3348af2664aac3a8b05281304ebc7a7914c6ad50a4b4eac383"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368056"u8,
        "31c49ae75bce7807cdff22055d94ee9021fedbb5ab51c57526f011aa"u8,
        "d817400e8ba9ca13a45f360e3d121eaaeb39af82d6001c8186f5f866"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368057"u8,
        "ae99feebb5d26945b54892092a8aee02912930fa41cd114e40447301"u8,
        "fb7da7f5f13a43b81774373c879cd32d6934c05fa758eeb14fcfab38"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368058"u8,
        "df1b1d66a551d0d31eff822558b9d2cc75c2180279fe0d08fd896d04"u8,
        "5c080fc3522f41bbb3f55a97cfecf21f882ce8cbb1e50ca6e67e56dc"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368059"u8,
        "706a46dc76dcb76798e60e6d89474788d16dc18032d268fd1a704fa6"u8,
        "e3d4895843da188fd58fb0567976d7b50359d6b78530c8f62d1b1746"u8
    ),
    new(
        "26959946667150639794667015087019625940457807714424391721682722368060"u8,
        "b70e0cbd6bb4bf7f321390b94a03c1d356c21122343280d6115c1d21"u8,
        "42c89c774a08dc04b3dd201932bc8a5ea5f8b89bbb2a7e667aff81cd"u8
    )
}.slice();

public static void TestP224BaseMult(ж<testing.T> Ꮡt) {
    var p224 = P224();
    foreach (var (i, e) in p224BaseMultTests) {
        var (k, ok) = @new<bigꓸInt>().SetString(e.k, 10);
        if (!ok) {
            Ꮡt.Errorf("%d: bad value for k: %s"u8, i, e.k);
        }
        var (x, y) = p224.ScalarBaseMult(k.Bytes());
        if (fmt.Sprintf("%x"u8, x.OrTypedNil()) != e.x || fmt.Sprintf("%x"u8, y.OrTypedNil()) != e.y) {
            Ꮡt.Errorf("%d: bad output for k=%s: got (%x, %x), want (%s, %s)"u8, i, e.k, x.OrTypedNil(), y.OrTypedNil(), e.x, e.y);
        }
        if (testing.Short() && i > 5) {
            break;
        }
    }
}

public static void TestP224GenericBaseMult(ж<testing.T> Ꮡt) {
    // We use the P224 CurveParams directly in order to test the generic implementation.
    var p224 = genericParamsForCurve(P224());
    foreach (var (i, e) in p224BaseMultTests) {
        var (k, ok) = @new<bigꓸInt>().SetString(e.k, 10);
        if (!ok) {
            Ꮡt.Errorf("%d: bad value for k: %s"u8, i, e.k);
        }
        var (x, y) = p224.ScalarBaseMult(k.Bytes());
        if (fmt.Sprintf("%x"u8, x.OrTypedNil()) != e.x || fmt.Sprintf("%x"u8, y.OrTypedNil()) != e.y) {
            Ꮡt.Errorf("%d: bad output for k=%s: got (%x, %x), want (%s, %s)"u8, i, e.k, x.OrTypedNil(), y.OrTypedNil(), e.x, e.y);
        }
        if (testing.Short() && i > 5) {
            break;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object p224FailedToValidateAˢ = (@string)"P224 failed to validate a correct point"u8;

public static void TestP224Overflow(ж<testing.T> Ꮡt) {
    // This tests for a specific bug in the P224 implementation.
    var p224 = P224();
    var (pointData, _) = hex.DecodeString("049B535B45FB0A2072398A6831834624C7E32CCFD5A4B933BCEAF77F1DD945E08BBE5178F5EDF5E733388F196D2A631D2E075BB16CBFEEA15B"u8);
    var (x, y) = Unmarshal(p224, pointData);
    if (!p224.IsOnCurve(x, y)) {
        Ꮡt.Error(p224FailedToValidateAˢ);
    }
}

} // end elliptic_internal_test_package
