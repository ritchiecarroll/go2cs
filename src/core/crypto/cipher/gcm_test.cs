// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using boring = go.crypto.@internal.boring_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fips140 = go.crypto.@internal.fips140_package;
using fipsaes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140.aes;

partial class cipher_test_package {

internal static cipher.Block _ᴛ1ʗ = new wrapperжBlock(((ж<wrapper>)nil));

[GoType] partial struct wrapper {
    internal cipher.Block block;
}

[GoRecv] internal static nint BlockSize(this ref wrapper w) {
    return w.block.BlockSize();
}

[GoRecv] internal static void Encrypt(this ref wrapper w, slice<byte> dst, slice<byte> src) {
    w.block.Encrypt(dst, src);
}

[GoRecv] internal static void Decrypt(this ref wrapper w, slice<byte> dst, slice<byte> src) {
    w.block.Decrypt(dst, src);
}

// wrap wraps the Block so that it does not type-asserts to *aes.Block.
internal static cipher.Block wrap(cipher.Block b) {
    return new wrapperжBlock(Ꮡ(new wrapper(b)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gcmˢ = "gcm"u8;
private static readonly @string fallbackˢ = "Fallback"u8;

internal static void testAllImplementations(ж<testing.T> Ꮡt, Action<ж<testing.T>, Func<slice<byte>, cipher.Block>> f) {
    cryptotest.TestAllImplementations(Ꮡt, gcmˢ, (ж<testing.T> tΔ1) => {
        f(tΔ1, (slice<byte> b) => {
            var (c, err) = aes.NewCipher(b);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            return c;
        });
    });
    Ꮡt.Run(fallbackˢ, (ж<testing.T> tΔ2) => {
        f(tΔ2, (slice<byte> b) => {
            var (c, err) = aes.NewCipher(b);
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            return wrap(c);
        });
    });
}

// key=16, plaintext=null
// key=24, plaintext=null
// key=32, plaintext=null
// key=16, plaintext=16
// key=24, plaintext=16
// key=32, plaintext=16
// key=16, plaintext=23
// key=24, plaintext=23
// key=32, plaintext=23
// key=16, plaintext=51
//key=24 plaintext=51
//key-32 plaintext=51
//key=16 plaintext=138
//key=24 plaintext=138
//key-32 plaintext=138
// key=16, plaintext=13
// key=16, plaintext=13
// key=24, plaintext=13
// key=32, plaintext=13
// These cases test non-standard nonce sizes.
// key=16, plaintext=0
//key=16, plaintext=32
//key=24, plaintext=32
//key=32, plaintext=32
// key=16, plaintext=0
//key=16, plaintext=13
//key=16, plaintext=128
//key=24, plaintext=128
//key=32, plaintext=128
//key=16, plaintext=512
//key=24, plaintext=512
//key=32, plaintext=512
//key=16, plaintext=293
//key=24, plaintext=293
//key=32, plaintext=293
// These cases test non-standard tag sizes.

[GoType("dyn")] partial struct aesGCMTestsᴛ1 {
    internal @string key, nonce, plaintext, ad, result;
}
internal static slice<aesGCMTestsᴛ1> aesGCMTests = new aesGCMTestsᴛ1[]{
    new(
        "11754cd72aec309bf52f7687212e8957"u8,
        "3c819d9a9bed087615030b65"u8,
        ""u8,
        ""u8,
        "250327c674aaf477aef2675748cf6971"u8
    ),
    new(
        "e2e001a36c60d2bf40d69ff5b2b1161ea218db263be16a4e"u8,
        "3c819d9a9bed087615030b65"u8,
        ""u8,
        ""u8,
        "c7b8da1fe2e3dccc4071ba92a0a57ba8"u8
    ),
    new(
        "5394e890d37ba55ec9d5f327f15680f6a63ef5279c79331643ad0af6d2623525"u8,
        "3c819d9a9bed087615030b65"u8,
        ""u8,
        ""u8,
        "d9b260d4bc4630733ffb642f5ce45726"u8
    ),
    new(
        "ca47248ac0b6f8372a97ac43508308ed"u8,
        "ffd2b598feabc9019262d2be"u8,
        ""u8,
        ""u8,
        "60d20404af527d248d893ae495707d1a"u8
    ),
    new(
        "fbe3467cc254f81be8e78d765a2e6333"u8,
        "c6697351ff4aec29cdbaabf2"u8,
        ""u8,
        "67"u8,
        "3659cdc25288bf499ac736c03bfc1159"u8
    ),
    new(
        "8a7f9d80d08ad0bd5a20fb689c88f9fc"u8,
        "88b7b27d800937fda4f47301"u8,
        ""u8,
        "50edd0503e0d7b8c91608eb5a1"u8,
        "ed6f65322a4740011f91d2aae22dd44e"u8
    ),
    new(
        "051758e95ed4abb2cdc69bb454110e82"u8,
        "c99a66320db73158a35a255d"u8,
        ""u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339f"u8,
        "6ce77f1a5616c505b6aec09420234036"u8
    ),
    new(
        "77be63708971c4e240d1cb79e8d77feb"u8,
        "e0e00f19fed7ba0136a797f3"u8,
        ""u8,
        "7a43ec1d9c0a5a78a0b16533a6213cab"u8,
        "209fcc8d3675ed938e9c7166709dd946"u8
    ),
    new(
        "7680c5d3ca6154758e510f4d25b98820"u8,
        "f8f105f9c3df4965780321f8"u8,
        ""u8,
        "c94c410194c765e3dcc7964379758ed3"u8,
        "94dca8edfcf90bb74b153c8d48a17930"u8
    ),
    new(
        "7fddb57453c241d03efbed3ac44e371c"u8,
        "ee283a3fc75575e33efd4887"u8,
        "d5de42b461646c255c87bd2962d3b9a2"u8,
        ""u8,
        "2ccda4a5415cb91e135c2a0f78c9b2fdb36d1df9b9d5e596f83e8b7f52971cb3"u8
    ),
    new(
        "ab72c77b97cb5fe9a382d9fe81ffdbed"u8,
        "54cc7dc2c37ec006bcc6d1da"u8,
        "007c5e5b3e59df24a7c355584fc1518d"u8,
        ""u8,
        "0e1bde206a07a9c2c1b65300f8c649972b4401346697138c7a4891ee59867d0c"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "54cc7dc2c37ec006bcc6d1da"u8,
        "007c5e5b3e59df24a7c355584fc1518d"u8,
        ""u8,
        "7bd53594c28b6c6596feb240199cad4c9badb907fd65bde541b8df3bd444d3a8"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "54cc7dc2c37ec006bcc6d1da"u8,
        "007c5e5b3e59df24a7c355584fc1518d"u8,
        ""u8,
        "d50b9e252b70945d4240d351677eb10f937cdaef6f2822b6a3191654ba41b197"u8
    ),
    new(
        "ab72c77b97cb5fe9a382d9fe81ffdbed"u8,
        "54cc7dc2c37ec006bcc6d1da"u8,
        "007c5e5b3e59df24a7c355584fc1518dabcdefab"u8,
        ""u8,
        "0e1bde206a07a9c2c1b65300f8c64997b73381a6ff6bc24c5146fbd73361f4fe"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "54cc7dc2c37ec006bcc6d1da"u8,
        "007c5e5b3e59df24a7c355584fc1518dabcdefab"u8,
        ""u8,
        "7bd53594c28b6c6596feb240199cad4c23b86a96d423cffa929e68541dc16b28"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "54cc7dc2c37ec006bcc6d1da"u8,
        "007c5e5b3e59df24a7c355584fc1518dabcdefab"u8,
        ""u8,
        "d50b9e252b70945d4240d351677eb10f27fd385388ad3b72b96a2d5dea1240ae"u8
    ),
    new(
        "fe47fcce5fc32665d2ae399e4eec72ba"u8,
        "5adb9609dbaeb58cbd6e7275"u8,
        "7c0e88c88899a779228465074797cd4c2e1498d259b54390b85e3eef1c02df60e743f1b840382c4bccaf3bafb4ca8429bea063"u8,
        "88319d6e1d3ffa5f987199166c8a9b56c2aeba5a"u8,
        "98f4826f05a265e6dd2be82db241c0fbbbf9ffb1c173aa83964b7cf5393043736365253ddbc5db8778371495da76d269e5db3e291ef1982e4defedaa2249f898556b47"u8
    ),
    new(
        "ec0c2ba17aa95cd6afffe949da9cc3a8"u8,
        "296bce5b50b7d66096d627ef"u8,
        "b85b3753535b825cbe5f632c0b843c741351f18aa484281aebec2f45bb9eea2d79d987b764b9611f6c0f8641843d5d58f3a242"u8,
        "f8d00f05d22bf68599bcdeb131292ad6e2df5d14"u8,
        "a7443d31c26bdf2a1c945e29ee4bd344a99cfaf3aa71f8b3f191f83c2adfc7a07162995506fde6309ffc19e716eddf1a828c5a890147971946b627c40016da1ecf3e77"u8
    ),
    new(
        "2c1f21cf0f6fb3661943155c3e3d8492"u8,
        "23cb5ff362e22426984d1907"u8,
        "42f758836986954db44bf37c6ef5e4ac0adaf38f27252a1b82d02ea949c8a1a2dbc0d68b5615ba7c1220ff6510e259f06655d8"u8,
        "5d3624879d35e46849953e45a32a624d6a6c536ed9857c613b572b0333e701557a713e3f010ecdf9a6bd6c9e3e44b065208645aff4aabee611b391528514170084ccf587177f4488f33cfb5e979e42b6e1cfc0a60238982a7aec"u8,
        "81824f0e0d523db30d3da369fdc0d60894c7a0a20646dd015073ad2732bd989b14a222b6ad57af43e1895df9dca2a5344a62cc57a3ee28136e94c74838997ae9823f3a"u8
    ),
    new(
        "d9f7d2411091f947b4d6f1e2d1f0fb2e"u8,
        "e1934f5db57cc983e6b180e7"u8,
        "73ed042327f70fe9c572a61545eda8b2a0c6e1d6c291ef19248e973aee6c312012f490c2c6f6166f4a59431e182663fcaea05a"u8,
        "0a8a18a7150e940c3d87b38e73baee9a5c049ee21795663e264b694a949822b639092d0e67015e86363583fcf0ca645af9f43375f05fdb4ce84f411dcbca73c2220dea03a20115d2e51398344b16bee1ed7c499b353d6c597af8"u8,
        "aaadbd5c92e9151ce3db7210b8714126b73e43436d242677afa50384f2149b831f1d573c7891c2a91fbc48db29967ec9542b2321b51ca862cb637cdd03b99a0f93b134"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "e1934f5db57cc983e6b180e7"u8,
        "73ed042327f70fe9c572a61545eda8b2a0c6e1d6c291ef19248e973aee6c312012f490c2c6f6166f4a59431e182663fcaea05a"u8,
        "0a8a18a7150e940c3d87b38e73baee9a5c049ee21795663e264b694a949822b639092d0e67015e86363583fcf0ca645af9f43375f05fdb4ce84f411dcbca73c2220dea03a20115d2e51398344b16bee1ed7c499b353d6c597af8"u8,
        "0736378955001d50773305975b3a534a4cd3614dd7300916301ae508cb7b45aa16e79435ca16b5557bcad5991bc52b971806863b15dc0b055748919b8ee91bc8477f68"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "e1934f5db57cc983e6b180e7"u8,
        "73ed042327f70fe9c572a61545eda8b2a0c6e1d6c291ef19248e973aee6c312012f490c2c6f6166f4a59431e182663fcaea05a"u8,
        "0a8a18a7150e940c3d87b38e73baee9a5c049ee21795663e264b694a949822b639092d0e67015e86363583fcf0ca645af9f43375f05fdb4ce84f411dcbca73c2220dea03a20115d2e51398344b16bee1ed7c499b353d6c597af8"u8,
        "fc1ae2b5dcd2c4176c3f538b4c3cc21197f79e608cc3730167936382e4b1e5a7b75ae1678bcebd876705477eb0e0fdbbcda92fb9a0dc58c8d8f84fb590e0422e6077ef"u8
    ),
    new(
        "d9f7d2411091f947b4d6f1e2d1f0fb2e"u8,
        "e1934f5db57cc983e6b180e7"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b3aabbccddee"u8,
        "0a8a18a7150e940c3d87b38e73baee9a5c049ee21795663e264b694a949822b639092d0e67015e86363583fcf0ca645af9f43375f05fdb4ce84f411dcbca73c2220dea03a20115d2e51398344b16bee1ed7c499b353d6c597af8"u8,
        "be86d00ce4e150190f646eae0f670ad26b3af66db45d2ee3fd71badd2fe763396bdbca498f3f779c70b80ed2695943e15139b406e5147b3855a1441dfb7bd64954b581e3db0ddf26b1c759e2276a4c18a8e4ad4b890f473e61c78e60074bd0633961e87e66d0a1be77c51ab6b9bb3318ccdd43794ffc18a03a83c1d368eeea590a13407c7ef48efc66e26047f3ab9deed0412ce89e"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "e1934f5db57cc983e6b180e7"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b3aabbccddee"u8,
        "0a8a18a7150e940c3d87b38e73baee9a5c049ee21795663e264b694a949822b639092d0e67015e86363583fcf0ca645af9f43375f05fdb4ce84f411dcbca73c2220dea03a20115d2e51398344b16bee1ed7c499b353d6c597af8"u8,
        "131d5ad9230858559b8c1929ec2c18be90d7d4630e49018262ce5c511688bd10622109403db8006014ce93905b0a16bf1d1411acc9e14edf09518bd5967ff4bc202805d4c2810810a093e996a0f56c9a3e3e593c783f68528c1a282ff6f4925902bb2b3d4cdd04b873663bf5fd9dd53b5df462e0424d038f249b10a99c0523200f8c92c3e8a178a25ee8e23b71308c88ec2cfe047e"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "e1934f5db57cc983e6b180e7"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b3aabbccddee"u8,
        "0a8a18a7150e940c3d87b38e73baee9a5c049ee21795663e264b694a949822b639092d0e67015e86363583fcf0ca645af9f43375f05fdb4ce84f411dcbca73c2220dea03a20115d2e51398344b16bee1ed7c499b353d6c597af8"u8,
        "e8318fe5aada811280804f35fb2a89e54bf32b4e55ba7b953547dadb39421d1dc39c7c127c6008b208010177f02fc093c8bbb8b3834d0e060d96dda96ba386c7c01224a4cac1edebffda4f9a64692bfbffb9f7c2999069fab84205224978a10d815d5ab8fa31e4e11630ba01c3b6cb99bef5772357ce86b83b4fb45ea7146402d560b6ad07de635b9366865e788a6bcdb132dcd079"u8
    ),
    new(
        "fe9bb47deb3a61e423c2231841cfd1fb"u8,
        "4d328eb776f500a2f7fb47aa"u8,
        "f1cc3818e421876bb6b8bbd6c9"u8,
        ""u8,
        "b88c5c1977b35b517b0aeae96743fd4727fe5cdb4b5b42818dea7ef8c9"u8
    ),
    new(
        "6703df3701a7f54911ca72e24dca046a"u8,
        "12823ab601c350ea4bc2488c"u8,
        "793cd125b0b84a043e3ac67717"u8,
        ""u8,
        "b2051c80014f42f08735a7b0cd38e6bcd29962e5f2c13626b85a877101"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "12823ab601c350ea4bc2488c"u8,
        "793cd125b0b84a043e3ac67717"u8,
        ""u8,
        "e888c2f438caedd4189d26c59f53439b8a7caec29e98c33ebf7e5712d6"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "12823ab601c350ea4bc2488c"u8,
        "793cd125b0b84a043e3ac67717"u8,
        ""u8,
        "e796c39074c7783a38193e3f8d46b355adacca7198d16d879fbfeac6e3"u8
    ),
    new(
        "1672c3537afa82004c6b8a46f6f0d026"u8,
        "05"u8,
        ""u8,
        ""u8,
        "8e2ad721f9455f74d8b53d3141f27e8e"u8
    ),
    new(
        "9a4fea86a621a91ab371e492457796c0"u8,
        "75"u8,
        "ca6131faf0ff210e4e693d6c31c109fc5b6f54224eb120f37de31dc59ec669b6"u8,
        "4f6e2585c161f05a9ae1f2f894e9f0ab52b45d0f"u8,
        "5698c0a384241d30004290aac56bb3ece6fe8eacc5c4be98954deb9c3ff6aebf5d50e1af100509e1fba2a5e8a0af9670"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "75"u8,
        "ca6131faf0ff210e4e693d6c31c109fc5b6f54224eb120f37de31dc59ec669b6"u8,
        "4f6e2585c161f05a9ae1f2f894e9f0ab52b45d0f"u8,
        "2709b357ec8334a074dbd5c4c352b216cfd1c8bd66343c5d43bfc6bd3b2b6cd0e3a82315d56ea5e4961c9ef3bc7e4042"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "75"u8,
        "ca6131faf0ff210e4e693d6c31c109fc5b6f54224eb120f37de31dc59ec669b6"u8,
        "4f6e2585c161f05a9ae1f2f894e9f0ab52b45d0f"u8,
        "d73bebe722c5e312fe910ba71d5a6a063a4297203f819103dfa885a8076d095545a999affde3dbac2b5be6be39195ed0"u8
    ),
    new(
        "d0f1f4defa1e8c08b4b26d576392027c"u8,
        "42b4f01eb9f5a1ea5b1eb73b0fb0baed54f387ecaa0393c7d7dffc6af50146ecc021abf7eb9038d4303d91f8d741a11743166c0860208bcc02c6258fd9511a2fa626f96d60b72fcff773af4e88e7a923506e4916ecbd814651e9f445adef4ad6a6b6c7290cc13b956130eef5b837c939fcac0cbbcc9656cd75b13823ee5acdac"u8,
        ""u8,
        ""u8,
        "7ab49b57ddf5f62c427950111c5c4f0d"u8
    ),
    new(
        "4a0c00a3d284dea9d4bf8b8dde86685e"u8,
        "f8cbe82588e784bcacbe092cd9089b51e01527297f635bf294b3aa787d91057ef23869789698ac960707857f163ecb242135a228ad93964f5dc4a4d7f88fd7b3b07dd0a5b37f9768fb05a523639f108c34c661498a56879e501a2321c8a4a94d7e1b89db255ac1f685e185263368e99735ebe62a7f2931b47282be8eb165e4d7"u8,
        "6d4bf87640a6a48a50d28797b7"u8,
        "8d8c7ffc55086d539b5a8f0d1232654c"u8,
        "0d803ec309482f35b8e6226f2b56303239298e06b281c2d51aaba3c125"u8
    ),
    new(
        "0e18a844ac5bf38e4cd72d9b0942e506"u8,
        "0870d4b28a2954489a0abcd5"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b3"u8,
        "05eff700e9a13ae5ca0bcbd0484764bd1f231ea81c7b64c514735ac55e4b79633b706424119e09dcaad4acf21b10af3b33cde3504847155cbb6f2219ba9b7df50be11a1c7f23f829f8a41b13b5ca4ee8983238e0794d3d34bc5f4e77facb6c05ac86212baa1a55a2be70b5733b045cd33694b3afe2f0e49e4f321549fd824ea9"u8,
        "cace28f4976afd72e3c5128167eb788fbf6634dda0a2f53148d00f6fa557f5e9e8f736c12e450894af56cb67f7d99e1027258c8571bd91ee3b7360e0d508aa1f382411a16115f9c05251cc326d4016f62e0eb8151c048465b0c6c8ff12558d43310e18b2cb1889eec91557ce21ba05955cf4c1d4847aadfb1b0a83f3a3b82b7efa62a5f03c5d6eda381a85dd78dbc55c"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "0870d4b28a2954489a0abcd5"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b3"u8,
        "05eff700e9a13ae5ca0bcbd0484764bd1f231ea81c7b64c514735ac55e4b79633b706424119e09dcaad4acf21b10af3b33cde3504847155cbb6f2219ba9b7df50be11a1c7f23f829f8a41b13b5ca4ee8983238e0794d3d34bc5f4e77facb6c05ac86212baa1a55a2be70b5733b045cd33694b3afe2f0e49e4f321549fd824ea9"u8,
        "303157d398376a8d51e39eabdd397f45b65f81f09acbe51c726ae85867e1675cad178580bb31c7f37c1af3644bd36ac436e9459139a4903d95944f306e415da709134dccde9d2b2d7d196b6740c196d9d10caa45296cf577a6e15d7ddf3576c20c503617d6a9e6b6d2be09ae28410a1210700a463a5b3b8d391abe9dac217e76a6f78306b5ebe759a5986b7d6682db0b"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "0870d4b28a2954489a0abcd5"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b3"u8,
        "05eff700e9a13ae5ca0bcbd0484764bd1f231ea81c7b64c514735ac55e4b79633b706424119e09dcaad4acf21b10af3b33cde3504847155cbb6f2219ba9b7df50be11a1c7f23f829f8a41b13b5ca4ee8983238e0794d3d34bc5f4e77facb6c05ac86212baa1a55a2be70b5733b045cd33694b3afe2f0e49e4f321549fd824ea9"u8,
        "e4f13934744125b9c35935ed4c5ac7d0c16434d52eadef1da91c6abb62bc757f01e3e42f628f030d750826adceb961f0675b81de48376b181d8781c6a0ccd0f34872ef6901b97ff7c2e152426b3257fb91f6a43f47befaaf7a2136fd0c97de8c48517ce047a5641141092c717b151b44f0794a164b5861f0a77271d1bdbc332e9e43d3b9828ccfdbd4ae338da5baf7a9"u8
    ),
    new(
        "1f6c3a3bc0542aabba4ef8f6c7169e73"u8,
        "f3584606472b260e0dd2ebb2"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b305eff700e9a13ae5ca0bcbd0484764bd1f231ea81c7b64c514735ac55e4b79633b706424119e09dcaad4acf21b10af3b33cde3504847155cbb6f2219ba9b7df50be11a1c7f23f829f8a41b13b5ca4ee8983238e0794d3d34bc5f4e77facb6c05ac86212baa1a55a2be70b5733b045cd33694b3afe2f0e49e4f321549fd824ea90870d4b28a2954489a0abcd50e18a844ac5bf38e4cd72d9b0942e506c433afcda3847f2dadd47647de321cec4ac430f62023856cfbb20704f4ec0bb920ba86c33e05f1ecd96733b79950a3e314d3d934f75ea0f210a8f6059401beb4bc4478fa4969e623d01ada696a7e4c7e5125b34884533a94fb319990325744ee9bbce9e525cf08f5e9e25e5360aad2b2d085fa54d835e8d466826498d9a8877565705a8a3f62802944de7ca5894e5759d351adac869580ec17e485f18c0c66f17cc07cbb22fce466da610b63af62bc83b4692f3affaf271693ac071fb86d11342d8def4f89d4b66335c1c7e4248367d8ed9612ec453902d8e50af89d7709d1a596c1f41f"u8,
        "95aa82ca6c49ae90cd1668baac7aa6f2b4a8ca99b2c2372acb08cf61c9c3805e6e0328da4cd76a19edd2d3994c798b0022569ad418d1fee4d9cd45a391c601ffc92ad91501432fee150287617c13629e69fc7281cd7165a63eab49cf714bce3a75a74f76ea7e64ff81eb61fdfec39b67bf0de98c7e4e32bdf97c8c6ac75ba43c02f4b2ed7216ecf3014df000108b67cf99505b179f8ed4980a6103d1bca70dbe9bbfab0ed59801d6e5f2d6f67d3ec5168e212e2daf02c6b963c98a1f7097de0c56891a2b211b01070dd8fd8b16c2a1a4e3cfd292d2984b3561d555d16c33ddc2bcf7edde13efe520c7e2abdda44d81881c531aeeeb66244c3b791ea8acfb6a68"u8,
        "55864065117e07650ca650a0f0d9ef4b02aee7c58928462fddb49045bf85355b4653fa26158210a7f3ef5b3ca48612e8b7adf5c025c1b821960af770d935df1c9a1dd25077d6b1c7f937b2e20ce981b07980880214698f3fad72fa370b3b7da257ce1d0cf352bc5304fada3e0f8927bd4e5c1abbffa563bdedcb567daa64faaed748cb361732200ba3506836a3c1c82aafa14c76dc07f6c4277ff2c61325f91fdbd6c1883e745fcaadd5a6d692eeaa5ad56eead6a9d74a595d22757ed89532a4b8831e2b9e2315baea70a9b95d228f09d491a5ed5ab7076766703457e3159bbb9b17b329525669863153079448c68cd2f200c0be9d43061a60639cb59d50993d276c05caaa565db8ce633b2673e4012bebbca02b1a64d779d04066f3e949ece173825885ec816468c819a8129007cc05d8785c48077d09eb1abcba14508dde85a6f16a744bc95faef24888d53a8020515ab20307efaecbdf143a26563c67989bceedc2d6d2bb9699bb6c615d93767e4158c1124e3b6c723aaa47796e59a60d3696cd85adfae9a62f2c02c22009f80ed494bdc587f31dd892c253b5c6d6b7db078fa72d23474ee54f8144d6561182d71c862941dbc0b2cb37a4d4b23cbad5637e6be901cc73f16d5aec39c60dddee631511e57b47520b61ae1892d2d1bd2b486e30faec892f171b6de98d96108016fac805604761f8e74742b3bb7dc8a290a46bf697c3e4446e6e65832cbae7cf1aaad1"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c"u8,
        "f3584606472b260e0dd2ebb2"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b305eff700e9a13ae5ca0bcbd0484764bd1f231ea81c7b64c514735ac55e4b79633b706424119e09dcaad4acf21b10af3b33cde3504847155cbb6f2219ba9b7df50be11a1c7f23f829f8a41b13b5ca4ee8983238e0794d3d34bc5f4e77facb6c05ac86212baa1a55a2be70b5733b045cd33694b3afe2f0e49e4f321549fd824ea90870d4b28a2954489a0abcd50e18a844ac5bf38e4cd72d9b0942e506c433afcda3847f2dadd47647de321cec4ac430f62023856cfbb20704f4ec0bb920ba86c33e05f1ecd96733b79950a3e314d3d934f75ea0f210a8f6059401beb4bc4478fa4969e623d01ada696a7e4c7e5125b34884533a94fb319990325744ee9bbce9e525cf08f5e9e25e5360aad2b2d085fa54d835e8d466826498d9a8877565705a8a3f62802944de7ca5894e5759d351adac869580ec17e485f18c0c66f17cc07cbb22fce466da610b63af62bc83b4692f3affaf271693ac071fb86d11342d8def4f89d4b66335c1c7e4248367d8ed9612ec453902d8e50af89d7709d1a596c1f41f"u8,
        "95aa82ca6c49ae90cd1668baac7aa6f2b4a8ca99b2c2372acb08cf61c9c3805e6e0328da4cd76a19edd2d3994c798b0022569ad418d1fee4d9cd45a391c601ffc92ad91501432fee150287617c13629e69fc7281cd7165a63eab49cf714bce3a75a74f76ea7e64ff81eb61fdfec39b67bf0de98c7e4e32bdf97c8c6ac75ba43c02f4b2ed7216ecf3014df000108b67cf99505b179f8ed4980a6103d1bca70dbe9bbfab0ed59801d6e5f2d6f67d3ec5168e212e2daf02c6b963c98a1f7097de0c56891a2b211b01070dd8fd8b16c2a1a4e3cfd292d2984b3561d555d16c33ddc2bcf7edde13efe520c7e2abdda44d81881c531aeeeb66244c3b791ea8acfb6a68"u8,
        "9daa466c7174dfde72b435fb6041ed7ff8ab8b1b96edb90437c3cc2e7e8a7c2c3629bae3bcaede99ee926ef4c55571e504e1c516975f6c719611c4da74acc23bbc79b3a67491f84d573e0293aa0cf5d775dde93fc466d5babd3e93a6506c0261021ac184f571ab190df83c32b41a67eaaa8dde27c02b08f15cabc75e46d1f9634f32f9233b2cb975386ff3a5e16b6ea2e2e4215cb33beb4de39a861d7f4a02165cd763f8252b2d60ac45d65a70735a8806a8fec3ca9d37c2cdcb21d2bd5c08d350e4bbdfb11dca344b9bee17e71ee0df3449fd9f9581c6b5483843b457534afb4240585f38ac22aa59a68a167fed6f1be0a5b072b2461f16c976b9aa0f5f2f5988818b01faa025ac7788212d92d222f7c14fe6e8f644c8cd117bb8def5a0217dad4f05cbb334ff9ccf819a4a085ed7c19928ddc40edc931b47339f456ccd423b5c0c1cdc96278006b29de945cdceb0737771e14562fff2aba40606f6046da5031647308682060412812317962bb68be3b42876f0905d52da51ec6345677fe86613828f488cc5685a4b973e48babd109a56d1a1effb286133dc2a94b4ada5707d3a7825941fea1a7502693afc7fe5d810bb0050d98aa6b80801e13b563954a35c31f57d5ba1ddb1a2be26426e2fe7bcd13ba183d80ac1c556b7ec2069b01de1450431a1c2e27848e1f5f4af013bce9080aebd2bb0f1de9f7bb460771c266d48ff4cf84a66f82630657db861c032971079"u8
    ),
    new(
        "feffe9928665731c6d6a8f9467308308feffe9928665731c6d6a8f9467308308"u8,
        "f3584606472b260e0dd2ebb2"u8,
        "67c6697351ff4aec29cdbaabf2fbe3467cc254f81be8e78d765a2e63339fc99a66320db73158a35a255d051758e95ed4abb2cdc69bb454110e827441213ddc8770e93ea141e1fc673e017e97eadc6b968f385c2aecb03bfb32af3c54ec18db5c021afe43fbfaaa3afb29d1e6053c7c9475d8be6189f95cbba8990f95b1ebf1b305eff700e9a13ae5ca0bcbd0484764bd1f231ea81c7b64c514735ac55e4b79633b706424119e09dcaad4acf21b10af3b33cde3504847155cbb6f2219ba9b7df50be11a1c7f23f829f8a41b13b5ca4ee8983238e0794d3d34bc5f4e77facb6c05ac86212baa1a55a2be70b5733b045cd33694b3afe2f0e49e4f321549fd824ea90870d4b28a2954489a0abcd50e18a844ac5bf38e4cd72d9b0942e506c433afcda3847f2dadd47647de321cec4ac430f62023856cfbb20704f4ec0bb920ba86c33e05f1ecd96733b79950a3e314d3d934f75ea0f210a8f6059401beb4bc4478fa4969e623d01ada696a7e4c7e5125b34884533a94fb319990325744ee9bbce9e525cf08f5e9e25e5360aad2b2d085fa54d835e8d466826498d9a8877565705a8a3f62802944de7ca5894e5759d351adac869580ec17e485f18c0c66f17cc07cbb22fce466da610b63af62bc83b4692f3affaf271693ac071fb86d11342d8def4f89d4b66335c1c7e4248367d8ed9612ec453902d8e50af89d7709d1a596c1f41f"u8,
        "95aa82ca6c49ae90cd1668baac7aa6f2b4a8ca99b2c2372acb08cf61c9c3805e6e0328da4cd76a19edd2d3994c798b0022569ad418d1fee4d9cd45a391c601ffc92ad91501432fee150287617c13629e69fc7281cd7165a63eab49cf714bce3a75a74f76ea7e64ff81eb61fdfec39b67bf0de98c7e4e32bdf97c8c6ac75ba43c02f4b2ed7216ecf3014df000108b67cf99505b179f8ed4980a6103d1bca70dbe9bbfab0ed59801d6e5f2d6f67d3ec5168e212e2daf02c6b963c98a1f7097de0c56891a2b211b01070dd8fd8b16c2a1a4e3cfd292d2984b3561d555d16c33ddc2bcf7edde13efe520c7e2abdda44d81881c531aeeeb66244c3b791ea8acfb6a68"u8,
        "793d34afb982ab70b0e204e1e7243314a19e987d9ab7662f58c3dc6064c9be35667ad53b115c610cfc229f4e5b3e8aae7aac97ce66d1d20b92da3860701b5006dd1385e173e3af7a5a9bb7da85c0434cd55a40fb9c482a0b36f0782846a7f16d05b40a08f0ad9a633f9a1e99e69e6b8039a0f2a91be40f193f4ce3bed1886dab1b0a6112f91503684c1e5afb938b9497166a7147badd1cc19c73e8b9f22e0dcbd18996868d7ad47755e677ee6e6ec87094cab7ee35feb96017c474261ba7391b18a72451e6daa7f38e162358c5d84788c974e614acc362b887c56b756df5aeacdda09b11d35a1f97daaceb5ca1b40a78b6058f7e1d26ad945be6ef74a8e72729f9ab2e3e7dda88d8f803e26e84a34ac07a7cecf5b6be23a4aa1ac6897f23169d894d53369b27673cf2438af9c6b53a2fa412c74dc075c617029e571f4c2951b1cdd63d33765af9d9d20e12430a83784c2bca8603f11521fa97f2e45398b4a385176701c6f416720ca0816bf51a3e0b4c7a28a89f0616a296423760f0f2f471e1def8a2f43956f79790a6b64dfdbb8159236ebd7fe1049e8e005e231e5f1936bfdccbda8cf0cb5116af758dfd6732dfa77ac3e6faf0996c13473292da363f01ddcb6a524dbf1d5d608f57c146173a9b169f979e101fe581f749764fd87119ae301958c8e9a9bfd16249e564ffbb304bc2ca4c34713a20fb858b47c83ce768e04f149884504c0515345631401f829e3259"u8
    ),
    new(
        "0795d80bc7f40f4d41c280271a2e4f7f"u8,
        "ff824c906594aff365d3cb1f"u8,
        "1ad4e74d127f935beee57cff920665babe7ce56227377afe570ba786193ded3412d4812453157f42fafc418c02a746c1232c234a639d49baa8f041c12e2ef540027764568ce49886e0d913e28059a3a485c6eee96337a30b28e4cd5612c2961539fa6bc5de034cbedc5fa15db844013e0bef276e27ca7a4faf47a5c1093bd643354108144454d221b3737e6cb87faac36ed131959babe44af2890cfcc4e23ffa24470e689ce0894f5407bb0c8665cff536008ad2ac6f1c9ef8289abd0bd9b72f21c597bda5210cf928c805af2dd4a464d52e36819d521f967bba5386930ab5b4cf4c71746d7e6e964673457348e9d71d170d9eb560bd4bdb779e610ba816bf776231ebd0af5966f5cdab6815944032ab4dd060ad8dab880549e910f1ffcf6862005432afad"u8,
        "98a47a430d8fd74dc1829a91e3481f8ed024d8ba34c9b903321b04864db333e558ae28653dffb2"u8,
        "3b8f91443480e647473a0a0b03d571c622b7e70e4309a02c9bb7980053010d865e6aec161354dc9f481b2cd5213e09432b57ec4e58fbd0a8549dd15c8c4e74a6529f75fad0ce5a9e20e2beeb2f91eb638bf88999968de438d2f1cedbfb0a1c81f9e8e7362c738e0fddd963692a4f4df9276b7f040979ce874cf6fa3de26da0713784bdb25e4efcb840554ef5b38b5fe8380549a496bd8e423a7456df6f4ae78a07ebe2276a8e22fc2243ec4f78abe0c99c733fd67c8c492699fa5ee2289cdd0a8d469bf883520ee74efb854bfadc7366a49ee65ca4e894e3335e2b672618d362eee12a577dd8dc2ba55c49c1fc3ad68180e9b112d0234d4aa28f5661f1e036450ca6f18be0166676bd80f8a4890c6ddea306fabb7ff3cb2860aa32a827e3a312912a2dfa70f6bc1c07de238448f2d751bd0cf15bf7"u8
    ),
    new(
        "e2e001a36c60d2bf40d69ff5b2b1161ea218db263be16a4e"u8,
        "84230643130d05425826641e"u8,
        "adb034f3f4a7ca45e2993812d113a9821d50df151af978bccc6d3bc113e15bc0918fb385377dca1916022ce816d56a332649484043c0fc0f2d37d040182b00a9bbb42ef231f80b48fb3730110d9a4433e38c73264c703579a705b9c031b969ec6d98de9f90e9e78b21179c2eb1e061946cd4bbb844f031ecf6eaac27a4151311adf1b03eda97c9fbae66295f468af4b35faf6ba39f9d8f95873bbc2b51cf3dfec0ed3c9b850696336cc093b24a8765a936d14dd56edc6bf518272169f75e67b74ba452d0aae90416a997c8f31e2e9d54ffea296dc69462debc8347b3e1af6a2d53bdfdfda601134f98db42b609df0a08c9347590c8d86e845bb6373d65a26ab85f67b50569c85401a396b8ad76c2b53ff62bcfbf033e435ef47b9b591d05117c6dc681d68e"u8,
        "d5d7316b8fdee152942148bff007c22e4b2022c6bc7be3c18c5f2e52e004e0b5dc12206bf002bd"u8,
        "f2c39423ee630dfe961da81909159dba018ce09b1073a12a477108316af5b7a31f86be6a0548b572d604bd115ea737dde899e0bd7f7ac9b23e38910dc457551ecc15c814a9f46d8432a1a36097dc1afe2712d1ba0838fa88cb55d9f65a2e9bece0dbf8999562503989041a2c87d7eb80ef649769d2f4978ce5cf9664f2bd0849646aa81cb976e45e1ade2f17a8126219e917aadbb4bae5e2c4b3f57bbc7f13fcc807df7842d9727a1b389e0b749e5191482adacabd812627c6eae2c7a30caf0844ad2a22e08f39edddf0ae10413e47db433dfe3febbb5a5cec9ade21fbba1e548247579395880b747669a8eb7e2ec0c1bff7fed2defdb92b07a14edf07b1bde29c31ab052ff1214e6b5ebbefcb8f21b5d6f8f6e07ee57ad6e14d4e142cb3f51bb465ab3a28a2a12f01b7514ad0463f2bde0d71d221"u8
    ),
    new(
        "5394e890d37ba55ec9d5f327f15680f6a63ef5279c79331643ad0af6d2623525"u8,
        "815e840b7aca7af3b324583f"u8,
        "8e63067cd15359f796b43c68f093f55fdf3589fc5f2fdfad5f9d156668a617f7091d73da71cdd207810e6f71a165d0809a597df9885ca6e8f9bb4e616166586b83cc45f49917fc1a256b8bc7d05c476ab5c4633e20092619c4747b26dad3915e9fd65238ee4e5213badeda8a3a22f5efe6582d0762532026c89b4ca26fdd000eb45347a2a199b55b7790e6b1b2dba19833ce9f9522c0bcea5b088ccae68dd99ae0203c81b9f1dd3181c3e2339e83ccd1526b67742b235e872bea5111772aab574ae7d904d9b6355a79178e179b5ae8edc54f61f172bf789ea9c9af21f45b783e4251421b077776808f04972a5e801723cf781442378ce0e0568f014aea7a882dcbcb48d342be53d1c2ebfb206b12443a8a587cc1e55ca23beca385d61d0d03e9d84cbc1b0a"u8,
        "0feccdfae8ed65fa31a0858a1c466f79e8aa658c2f3ba93c3f92158b4e30955e1c62580450beff"u8,
        "b69a7e17bb5af688883274550a4ded0d1aff49a0b18343f4b382f745c163f7f714c9206a32a1ff012427e19431951edd0a755e5f491b0eedfd7df68bbc6085dd2888607a2f998c3e881eb1694109250db28291e71f4ad344a125624fb92e16ea9815047cd1111cabfdc9cb8c3b4b0f40aa91d31774009781231400789ed545404af6c3f76d07ddc984a7bd8f52728159782832e298cc4d529be96d17be898efd83e44dc7b0e2efc645849fd2bba61fef0ae7be0dcab233cc4e2b7ba4e887de9c64b97f2a1818aa54371a8d629dae37975f7784e5e3cc77055ed6e975b1e5f55e6bbacdc9f295ce4ada2c16113cd5b323cf78b7dde39f4a87aa8c141a31174e3584ccbd380cf5ec6d1dba539928b084fa9683e9c0953acf47cc3ac384a2c38914f1da01fb2cfd78905c2b58d36b2574b9df15535d82"u8
    ),
    new(
        "89c54b0d3bc3c397d5039058c220685f"u8,
        "bc7f45c00868758d62d4bb4d"u8,
        "582670b0baf5540a3775b6615605bd05"u8,
        "48d16cda0337105a50e2ed76fd18e114"u8,
        "fc2d4c4eee2209ddbba6663c02765e6955e783b00156f5da0446e2970b877f"u8
    ),
    new(
        "bad6049678bf75c9087b3e3ae7e72c13"u8,
        "a0a017b83a67d8f1b883e561"u8,
        "a1be93012f05a1958440f74a5311f4a1"u8,
        "f7c27b51d5367161dc2ff1e9e3edc6f2"u8,
        "36f032f7e3dc3275ca22aedcdc68436b99a2227f8bb69d45ea5d8842cd08"u8
    ),
    new(
        "66a3c722ccf9709525650973ecc100a9"u8,
        "1621d42d3a6d42a2d2bf9494"u8,
        "61fa9dbbed2190fbc2ffabf5d2ea4ff8"u8,
        "d7a9b6523b8827068a6354a6d166c6b9"u8,
        "fef3b20f40e08a49637cc82f4c89b8603fd5c0132acfab97b5fff651c4"u8
    ),
    new(
        "562ae8aadb8d23e0f271a99a7d1bd4d1"u8,
        "f7a5e2399413b89b6ad31aff"u8,
        "bbdc3504d803682aa08a773cde5f231a"u8,
        "2b9680b886b3efb7c6354b38c63b5373"u8,
        "e2b7e5ed5ff27fc8664148f5a628a46dcbf2015184fffb82f2651c36"u8
    ),
    new(
        "11754cd72aec309bf52f7687212e8957"u8,
        ""u8,
        ""u8,
        ""u8,
        "250327c674aaf477aef2675748cf6971"u8
    )
}.slice();

public static void TestAESGCM(ж<testing.T> Ꮡt) {
    testAllImplementations(Ꮡt, testAESGCM);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object expectedErrorForZeroˢ = (@string)"expected error for zero nonce size"u8;

internal static void testAESGCM(ж<testing.T> Ꮡt, Func<slice<byte>, cipher.Block> newCipher) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, test) in aesGCMTests) {
        var (key, _) = hex.DecodeString(test.key);
        var aes = newCipher(key);
        var (nonce, _) = hex.DecodeString(test.nonce);
        var (plaintext, _) = hex.DecodeString(test.plaintext);
        var (ad, _) = hex.DecodeString(test.ad);
        nint tagSize = (len(test.result) - len(test.plaintext)) / 2;
        error err = default!;
        cipher.AEAD aesgcm = default!;
        switch (ᐧ) {
        case {} when tagSize is not 16: {
            (aesgcm, err) = cipher.NewGCMWithTagSize(aes, // Handle non-standard tag sizes
 tagSize);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            break;
        }
        case {} when len(nonce) is 0: {
            (aesgcm, err) = cipher.NewGCMWithNonceSize(aes, // Handle 0 nonce size (expect error and continue)
 0);
            if (err == default!) {
                Ꮡt.Fatal(expectedErrorForZeroˢ);
            }
            continue;
            break;
        }
        case {} when len(nonce) is not 12: {
            (aesgcm, err) = cipher.NewGCMWithNonceSize(aes, // Handle non-standard nonce sizes
 len(nonce));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            break;
        }
        default: {
            (aesgcm, err) = cipher.NewGCM(aes);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            break;
        }}

        var ct = aesgcm.Seal(default!, nonce, plaintext, ad);
        {
            @string ctHex = hex.EncodeToString(ct); if (ctHex != test.result) {
                Ꮡt.Errorf("#%d: got %s, want %s"u8, i, ctHex, test.result);
                continue;
            }
        }
        (var plaintext2, err) = aesgcm.Open(default!, nonce, ct, ad);
        if (err != default!) {
            Ꮡt.Errorf("#%d: Open failed"u8, i);
            continue;
        }
        if (!bytes.Equal(plaintext, plaintext2)) {
            Ꮡt.Errorf("#%d: plaintext's don't match: got %x vs %x"u8, i, plaintext2, plaintext);
            continue;
        }
        if (len(ad) > 0) {
            ad[0] ^= (byte)(0x80);
            {
                var (_, errΔ1) = aesgcm.Open(default!, nonce, ct, ad); if (errΔ1 == default!) {
                    Ꮡt.Errorf("#%d: Open was successful after altering additional data"u8, i);
                }
            }
            ad[0] ^= (byte)(0x80);
        }
        nonce[0] ^= (byte)(0x80);
        {
            var (_, errΔ2) = aesgcm.Open(default!, nonce, ct, ad); if (errΔ2 == default!) {
                Ꮡt.Errorf("#%d: Open was successful after altering nonce"u8, i);
            }
        }
        nonce[0] ^= (byte)(0x80);
        ct[0] ^= (byte)(0x80);
        {
            var (_, errΔ3) = aesgcm.Open(default!, nonce, ct, ad); if (errΔ3 == default!) {
                Ꮡt.Errorf("#%d: Open was successful after altering ciphertext"u8, i);
            }
        }
        ct[0] ^= (byte)(0x80);
    }
}

public static void TestGCMInvalidTagSize(ж<testing.T> Ꮡt) {
    testAllImplementations(Ꮡt, testGCMInvalidTagSize);
}

internal static void testGCMInvalidTagSize(ж<testing.T> Ꮡt, Func<slice<byte>, cipher.Block> newCipher) {
    var (key, _) = hex.DecodeString("ab72c77b97cb5fe9a382d9fe81ffdbed"u8);
    var aes = newCipher(key);
    foreach (var (_, tagSize) in new nint[]{0, 1, aes.BlockSize() + 1}.slice()) {
        var (aesgcm, err) = cipher.NewGCMWithTagSize(aes, tagSize);
        if (aesgcm != default! || err == default!) {
            Ꮡt.Fatalf("NewGCMWithTagSize was successful with an invalid %d-byte tag size"u8, tagSize);
        }
    }
}

public static void TestTagFailureOverwrite(ж<testing.T> Ꮡt) {
    testAllImplementations(Ꮡt, testTagFailureOverwrite);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object badOpenStillResultedInˢ = (@string)"Bad Open still resulted in nil error."u8;
private static readonly object failedOpenReturnedNonNilˢ = (@string)"Failed Open returned non-nil result."u8;
private static readonly object failedOpenDidnTZeroDstˢ = (@string)"Failed Open didn't zero dst buffer"u8;

internal static void testTagFailureOverwrite(ж<testing.T> Ꮡt, Func<slice<byte>, cipher.Block> newCipher) {
    // The AESNI GCM code decrypts and authenticates concurrently and so
    // overwrites the output buffer before checking the authentication tag.
    // In order to be consistent across platforms, all implementations
    // should do this and this test checks that.
    var (key, _) = hex.DecodeString("ab72c77b97cb5fe9a382d9fe81ffdbed"u8);
    var (nonce, _) = hex.DecodeString("54cc7dc2c37ec006bcc6d1db"u8);
    var (ciphertext, _) = hex.DecodeString("0e1bde206a07a9c2c1b65300f8c649972b4401346697138c7a4891ee59867d0c"u8);
    var aes = newCipher(key);
    var (aesgcm, _) = cipher.NewGCM(aes);
    var dst = new slice<byte>(len(ciphertext) - 16);
    foreach (var (i, _) in dst) {
        dst[i] = 42;
    }
    var (result, err) = aesgcm.Open(dst[..0], nonce, ciphertext, default!);
    if (err == default!) {
        Ꮡt.Fatal(badOpenStillResultedInˢ);
    }
    if (result != default!) {
        Ꮡt.Fatal(failedOpenReturnedNonNilˢ);
    }
    foreach (var (i, _) in dst) {
        if (dst[i] != 0) {
            Ꮡt.Fatal(failedOpenDidnTZeroDstˢ);
        }
    }
}

public static void TestGCMCounterWrap(ж<testing.T> Ꮡt) {
    testAllImplementations(Ꮡt, testGCMCounterWrap);
}

[GoType("dyn")] internal partial struct testGCMCounterWrap_tests {
    internal @string nonce, tag;
}

internal static void testGCMCounterWrap(ж<testing.T> Ꮡt, Func<slice<byte>, cipher.Block> newCipher) {
    // Test that the last 32-bits of the counter wrap correctly.
    var tests = new testGCMCounterWrap_tests[]{
        new("0fa72e25"u8, "37e1948cdfff09fbde0c40ad99fee4a7"u8), // counter: 7eb59e4d961dad0dfdd75aaffffffff0

        new("afe05cc1"u8, "438f3aa9fee5e54903b1927bca26bbdf"u8), // counter: 75d492a7e6e6bfc979ad3a8ffffffff4

        new("9ffecbef"u8, "7b88ca424df9703e9e8611071ec7e16e"u8), // counter: c8bb108b0ecdc71747b9d57ffffffff5

        new("ffc3e5b3"u8, "38d49c86e0abe853ac250e66da54c01a"u8), // counter: 706414d2de9b36ab3b900a9ffffffff6

        new("cfdd729d"u8, "e08402eaac36a1a402e09b1bd56500e8"u8), // counter: cd0b96fe36b04e750584e56ffffffff7

        new("010ae3d486"u8, "5405bb490b1f95d01e2ba735687154bc"u8), // counter: e36c18e69406c49722808104fffffff8

        new("01b1107a9d"u8, "939a585f342e01e17844627492d44dbf"u8)
    }.slice();
    // counter: e6d56eaf9127912b6d62c6dcffffffff
    var key = newCipher(new slice<byte>(16));
    var plaintext = new slice<byte>(16 * 17 + 1);
    foreach (var (i, test) in tests) {
        var (nonce, _) = hex.DecodeString(test.nonce);
        var (want, _) = hex.DecodeString(test.tag);
        var (aead, err) = cipher.NewGCMWithNonceSize(key, len(nonce));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var got = aead.Seal(default!, nonce, plaintext, default!);
        if (!bytes.Equal(got.slice(len(plaintext)), want)) {
            Ꮡt.Errorf("test[%v]: got: %x, want: %x"u8, i, got.slice(len(plaintext)), want);
        }
        (_, err) = aead.Open(default!, nonce, got, default!);
        if (err != default!) {
            Ꮡt.Errorf("test[%v]: authentication failed"u8, i);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string incorrectSealOutputˢ = "incorrect Seal output"u8;
private static readonly @string authenticationFailedˢ = "authentication failed"u8;
private static readonly @string incorrectOpenOutputˢ = "incorrect Open output"u8;

// generate permutations
[GoType("dyn")] internal partial struct TestGCMAsm_pair {
    internal nint align, length;
}

public static void TestGCMAsm(ж<testing.T> Ꮡt) {
    // Create a new pair of AEADs, one using the assembly implementation
    // and one using the generic Go implementation.
    (cipher.AEAD asm, cipher.AEAD generic, error err) newAESGCM(slice<byte> keyΔ1) {
        cipher.AEAD asmΔ1 = default!;
        cipher.AEAD genericΔ1 = default!;
        error errΔ1 = default!;
        (var block, errΔ1) = aes.NewCipher(keyΔ1[..]);
        if (errΔ1 != default!) {
            return (default!, default!, errΔ1);
        }
        (asmΔ1, errΔ1) = cipher.NewGCM(block);
        if (errΔ1 != default!) {
            return (default!, default!, errΔ1);
        }
        (genericΔ1, errΔ1) = cipher.NewGCM(wrap(block));
        if (errΔ1 != default!) {
            return (default!, default!, errΔ1);
        }
        return (asmΔ1, genericΔ1, default!);
    }
    // check for assembly implementation
    array<byte> key = new(16);
    var (asm, generic, err) = newAESGCM(key[..]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (AreEqual(reflect.TypeOf(asm), reflect.TypeOf(generic))) {
        Ꮡt.Skipf("no assembly implementation of GCM"u8);
    }
    var lengths = new nint[]{0, 156, 8192, 8193, 8208}.slice();
    var keySizes = new nint[]{16, 24, 32}.slice();
    var alignments = new nint[]{0, 1, 2, 3}.slice();
    if (testing.Short()) {
        keySizes = new nint[]{16}.slice();
        alignments = new nint[]{1}.slice();
    }
    var perms = new slice<TestGCMAsm_pair>(0);
    foreach (var (_, l) in lengths) {
        foreach (var (_, a) in alignments) {
            if (a != 0 && l == 0) {
                continue;
            }
            perms = append(perms, new TestGCMAsm_pair(align: a, length: l));
        }
    }
    // run test for all permutations
    var newAESGCMʗ1 = newAESGCM;
    error test(nint ks, slice<byte> pt, slice<byte> ad) {
        var keyΔ2 = new slice<byte>(ks);
        {
            var (_, errΔ2) = io.ReadFull(rand.Reader, keyΔ2); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
        var (asmΔ2, genericΔ2, errΔ3) = newAESGCMʗ1(keyΔ2);
        if (errΔ3 != default!) {
            return errΔ3;
        }
        {
            var (_, errΔ4) = io.ReadFull(rand.Reader, pt); if (errΔ4 != default!) {
                return errΔ4;
            }
        }
        {
            var (_, errΔ5) = io.ReadFull(rand.Reader, ad); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
        var nonce = new slice<byte>(12);
        {
            var (_, errΔ6) = io.ReadFull(rand.Reader, nonce); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
        var want = genericΔ2.Seal(default!, nonce, pt, ad);
        var got = asmΔ2.Seal(default!, nonce, pt, ad);
        if (!bytes.Equal(want, got)) {
            return errors.New(incorrectSealOutputˢ);
        }
        (got, errΔ3) = asmΔ2.Open(default!, nonce, want, ad);
        if (errΔ3 != default!) {
            return errors.New(authenticationFailedˢ);
        }
        if (!bytes.Equal(pt, got)) {
            return errors.New(incorrectOpenOutputˢ);
        }
        return default!;
    }
    foreach (var (_, a) in perms) {
        var ad = new slice<byte>(a.align + a.length);
        ad = ad.slice(a.align);
        foreach (var (_, p) in perms) {
            var pt = new slice<byte>(p.align + p.length);
            pt = pt.slice(p.align);
            foreach (var (_, ks) in keySizes) {
                {
                    var errΔ7 = test(ks, pt, ad); if (errΔ7 != default!) {
                        Ꮡt.Error(errΔ7);
                        Ꮡt.Errorf("	key size: %v"u8, ks);
                        Ꮡt.Errorf("	plaintext alignment: %v"u8, p.align);
                        Ꮡt.Errorf("	plaintext length: %v"u8, p.length);
                        Ꮡt.Errorf("	additionalData alignment: %v"u8, a.align);
                        Ꮡt.Fatalf("	additionalData length: %v"u8, a.length);
                    }
                }
            }
        }
    }
}

// Test GCM against the general cipher.AEAD interface tester.
public static void TestGCMAEAD(ж<testing.T> Ꮡt) {
    testAllImplementations(Ꮡt, testGCMAEAD);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string minTagSizeˢ = "MinTagSize"u8;
private static readonly @string gcmWithRandomNonceˢ = "GCMWithRandomNonce"u8;
private static readonly object newGCMWithRandomNonceˢ = (@string)"NewGCMWithRandomNonce requires an AES block cipher"u8;

internal static void testGCMAEAD(ж<testing.T> Ꮡt, Func<slice<byte>, cipher.Block> newCipher) {
    nint minTagSize = 12;
    foreach (var (_, keySize) in new nint[]{128, 192, 256}.slice()) {
        // Use AES as underlying block cipher at different key sizes for GCM.
        Ꮡt.Run(fmt.Sprintf("AES-%d"u8, keySize), (ж<testing.T> tΔ1) => {
            var rng = newRandReader(tΔ1);
            var key = new slice<byte>(keySize / 8);
            rng.Read(key);
            var block = newCipher(key);
            // Test GCM with the current AES block with the standard nonce and tag
            // sizes.
            var blockʗ1 = block;
            cryptotest.TestAEAD(tΔ1, () => cipher.NewGCM(blockʗ1));
            // Test non-standard tag sizes.
            var blockʗ2 = block;
            tΔ1.Run(minTagSizeˢ, (ж<testing.T> tΔ2) => {
                var blockʗ3 = blockʗ2;
                cryptotest.TestAEAD(tΔ2, () => cipher.NewGCMWithTagSize(blockʗ3, minTagSize));
            });
            // Test non-standard nonce sizes.
            foreach (var (_, nonceSize) in new nint[]{1, 16, 100}.slice()) {
                var blockʗ4 = block;
                tΔ1.Run(fmt.Sprintf("NonceSize-%d"u8, nonceSize), (ж<testing.T> tΔ3) => {
                    var blockʗ5 = blockʗ4;
                    cryptotest.TestAEAD(tΔ3, () => cipher.NewGCMWithNonceSize(blockʗ5, nonceSize));
                });
            }
            // Test NewGCMWithRandomNonce.
            var blockʗ6 = block;
            tΔ1.Run(gcmWithRandomNonceˢ, (ж<testing.T> tΔ4) => {
                {
                    var (_, ok) = blockʗ6._<ж<wrapper>>(ᐧ); if (ok || boring.Enabled) {
                        tΔ4.Skip(newGCMWithRandomNonceˢ);
                    }
                }
                var blockʗ7 = blockʗ6;
                cryptotest.TestAEAD(tΔ4, () => cipher.NewGCMWithRandomNonce(blockʗ7));
            });
        });
    }
}

public static void TestFIPSServiceIndicator(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    cipher.AEAD newGCM() {
        var key = new slice<byte>(16);
        var (block, _) = fipsaes.New(key);
        var (aead, _) = gcm.NewGCMWithCounterNonce(block);
        return new gcm_GCMWithCounterNonceжAEAD(aead);
    }
    bool tryNonce(cipher.AEAD aead, slice<byte> nonce) {
        fips140.ResetServiceIndicator();
        aead.Seal(default!, nonce, slice<byte>("x"u8), default!);
        return fips140.ServiceIndicator();
    }
    var tryNonceʗ1 = tryNonce;
    void expectTrue(ж<testing.T> tΔ1, cipher.AEAD aead, slice<byte> nonce) {
        tΔ1.Helper();
        if (!tryNonceʗ1(aead, nonce)) {
            tΔ1.Errorf("expected service indicator true for %x"u8, nonce);
        }
    }
    var tryNonceʗ2 = tryNonce;
    void expectPanic(ж<testing.T> tΔ2, cipher.AEAD aead, slice<byte> nonce) {
        GoFrame ᒐ = default;
        try {
            tΔ2.Helper();
            var nonceʗ1 = nonce;
            defer(() => {
                tΔ2.Helper();
                if (recover() == default!) {
                    tΔ2.Errorf("expected panic for %x"u8, nonceʗ1);
                }
            }, ref ᒐ);
            tryNonceʗ2(aead, nonce);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var g = newGCM();
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0}.slice());
    // Changed name.
    expectPanic(Ꮡt, g, new byte[]{0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
    g = newGCM();
    expectTrue(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}.slice());
    // Went down.
    expectPanic(Ꮡt, g, new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
    g = newGCM();
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}.slice());
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13}.slice());
    // Did not increment.
    expectPanic(Ꮡt, g, new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13}.slice());
    g = newGCM();
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00}.slice());
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice());
    // Wrap is ok as long as we don't run out of values.
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xfe}.slice());
    // Run out of counters.
    expectPanic(Ꮡt, g, new byte[]{1, 2, 3, 4, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff}.slice());
    g = newGCM();
    expectTrue(Ꮡt, g, new byte[]{1, 2, 3, 4, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice());
    // Wrap with overflow.
    expectPanic(Ꮡt, g, new byte[]{1, 2, 3, 5, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
}

public static void TestGCMForSSH(ж<testing.T> Ꮡt) {
    // incIV from x/crypto/ssh/cipher.go.
    void incIV(slice<byte> ivΔ1) {
        for (nint i = 4 + 7; i >= 4; i--) {
            ivΔ1[i]++;
            if (ivΔ1[i] != 0) {
                break;
            }
        }
    }
    void expectOK(cipher.AEAD aeadΔ1, slice<byte> ivΔ2) {
        aeadΔ1.Seal(default!, ivΔ2, slice<byte>("hello, world"u8), default!);
    }
    void expectPanic(cipher.AEAD aeadΔ2, slice<byte> ivΔ3) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                if (recover() == default!) {
                    Ꮡt.Errorf("expected panic"u8);
                }
            }, ref ᒐ);
            aeadΔ2.Seal(default!, ivΔ3, slice<byte>("hello, world"u8), default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var key = new slice<byte>(16);
    var (block, _) = fipsaes.New(key);
    var (aead, err) = gcm.NewGCMForSSH(block);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var iv = decodeHex(Ꮡt, "11223344"u8 + "0000000000000000"u8);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    iv = decodeHex(Ꮡt, "11223344"u8 + "fffffffffffffffe"u8);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectPanic(new gcm_GCMForSSHжAEAD(aead), iv);
    (aead, _) = gcm.NewGCMForSSH(block);
    iv = decodeHex(Ꮡt, "11223344"u8 + "fffffffffffffffe"u8);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    (aead, _) = gcm.NewGCMForSSH(block);
    iv = decodeHex(Ꮡt, "11223344"u8 + "aaaaaaaaaaaaaaaa"u8);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    iv = decodeHex(Ꮡt, "11223344"u8 + "ffffffffffffffff"u8);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    iv = decodeHex(Ꮡt, "11223344"u8 + "aaaaaaaaaaaaaaa8"u8);
    expectOK(new gcm_GCMForSSHжAEAD(aead), iv);
    incIV(iv);
    expectPanic(new gcm_GCMForSSHжAEAD(aead), iv);
    iv = decodeHex(Ꮡt, "11223344"u8 + "bbbbbbbbbbbbbbbb"u8);
    expectPanic(new gcm_GCMForSSHжAEAD(aead), iv);
}

internal static slice<byte> decodeHex(ж<testing.T> Ꮡt, @string s) {
    Ꮡt.Helper();
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return b;
}

} // end cipher_test_package
