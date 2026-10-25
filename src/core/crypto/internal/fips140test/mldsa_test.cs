// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto.@internal.fips140test;

using cryptotest = go.crypto.@internal.cryptotest_package;
using fips140 = go.crypto.@internal.fips140_package;
using static go.crypto.@internal.fips140.mldsa_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using hex = encoding.hex_package;
using flag = flag_package;
using rand = math.rand_package;
using testing = testing_package;
using encoding;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using math;
using mldsa = go.crypto.@internal.fips140.mldsa_package;

partial class fipstest_internal_test_package {

internal static ж<bool> sixtyMillionFlag = flag.Bool("60million"u8, false, "run 60M-iterations accumulated test"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mlDsa44100ˢ = "ML-DSA-44/100"u8;
internal static readonly @string mlDsa65100ˢ = "ML-DSA-65/100"u8;
internal static readonly @string mlDsa87100ˢ = "ML-DSA-87/100"u8;
internal static readonly @string mlDsa4410kˢ = "ML-DSA-44/10k"u8;
internal static readonly @string mlDsa6510kˢ = "ML-DSA-65/10k"u8;
internal static readonly @string mlDsa8710kˢ = "ML-DSA-87/10k"u8;
internal static readonly @string mlDsa4460mˢ = "ML-DSA-44/60M"u8;
internal static readonly @string mlDsa6560mˢ = "ML-DSA-65/60M"u8;
internal static readonly @string mlDsa8760mˢ = "ML-DSA-87/60M"u8;

// TestMLDSAAccumulated accumulates 10k (or 100, or 60M) random vectors and checks
// the hash of the result, to avoid checking in megabytes of test vectors.
//
// 60M in particular is enough to give a 99.9% chance of hitting every value in
// the base field.
//
//	1-((q-1)/q)^60000000 ~= 0.9992
//
// If setting -60million, remember to also set -timeout 0.
public static void TestMLDSAAccumulated(ж<testing.T> Ꮡt) {
    Ꮡt.Run(mlDsa44100ˢ, (ж<testing.T> tΔ1) => {
        testMLDSAAccumulated(tΔ1, NewPrivateKey44, NewPublicKey44, 100,
            "d51148e1f9f4fa1a723a6cf42e25f2a99eb5c1b378b3d2dbbd561b1203beeae4"u8);
    });
    Ꮡt.Run(mlDsa65100ˢ, (ж<testing.T> tΔ2) => {
        testMLDSAAccumulated(tΔ2, NewPrivateKey65, NewPublicKey65, 100,
            "8358a1843220194417cadbc2651295cd8fc65125b5a5c1a239a16dc8b57ca199"u8);
    });
    Ꮡt.Run(mlDsa87100ˢ, (ж<testing.T> tΔ3) => {
        testMLDSAAccumulated(tΔ3, NewPrivateKey87, NewPublicKey87, 100,
            "8c3ad714777622b8f21ce31bb35f71394f23bc0fcf3c78ace5d608990f3b061b"u8);
    });
    if (!testing.Short()) {
        Ꮡt.Run(mlDsa4410kˢ, (ж<testing.T> tΔ4) => {
            tΔ4.Parallel();
            testMLDSAAccumulated(tΔ4, NewPrivateKey44, NewPublicKey44, 10000,
                "e7fd21f6a59bcba60d65adc44404bb29a7c00e5d8d3ec06a732c00a306a7d143"u8);
        });
        Ꮡt.Run(mlDsa6510kˢ, (ж<testing.T> tΔ5) => {
            tΔ5.Parallel();
            testMLDSAAccumulated(tΔ5, NewPrivateKey65, NewPublicKey65, 10000,
                "5ff5e196f0b830c3b10a9eb5358e7c98a3a20136cb677f3ae3b90175c3ace329"u8);
        });
        Ꮡt.Run(mlDsa8710kˢ, (ж<testing.T> tΔ6) => {
            tΔ6.Parallel();
            testMLDSAAccumulated(tΔ6, NewPrivateKey87, NewPublicKey87, 10000,
                "80a8cf39317f7d0be0e24972c51ac152bd2a3e09bc0c32ce29dd82c4e7385e60"u8);
        });
    }
    if (sixtyMillionFlag.Value) {
        Ꮡt.Run(mlDsa4460mˢ, (ж<testing.T> tΔ7) => {
            tΔ7.Parallel();
            testMLDSAAccumulated(tΔ7, NewPrivateKey44, NewPublicKey44, 60000000,
                "080b48049257f5cd30dee17d6aa393d6c42fe52a29099df84a460ebaf4b02330"u8);
        });
        Ꮡt.Run(mlDsa6560mˢ, (ж<testing.T> tΔ8) => {
            tΔ8.Parallel();
            testMLDSAAccumulated(tΔ8, NewPrivateKey65, NewPublicKey65, 60000000,
                "0af0165db2b180f7a83dbecad1ccb758b9c2d834b7f801fc49dd572a9d4b1e83"u8);
        });
        Ꮡt.Run(mlDsa8760mˢ, (ж<testing.T> tΔ9) => {
            tΔ9.Parallel();
            testMLDSAAccumulated(tΔ9, NewPrivateKey87, NewPublicKey87, 60000000,
                "011166e9d5032c9bdc5c9bbb5dbb6c86df1c3d9bf3570b65ebae942dd9830057"u8);
        });
    }
}

internal static void testMLDSAAccumulated(ж<testing.T> Ꮡt, Func<slice<byte>, (ж<mldsa.PrivateKey>, error)> newPrivateKey, Func<slice<byte>, (ж<mldsaꓸPublicKey>, error)> newPublicKey, nint n, @string expected) {
    var s = sha3.NewShake128();
    var o = sha3.NewShake128();
    var seed = new slice<byte>(PrivateKeySize);
    var msg = new slice<byte>(0);

    for (nint i = 0; i < n; i++) {
        s.Read(seed);
        var (dk, err) = newPrivateKey(seed);
        if (err != default!) {
            Ꮡt.Fatalf("NewPrivateKey: %v"u8, err);
        }
        var pk = dk.PublicKey().Bytes();
        o.Write(pk);
        (var sig, err) = SignDeterministic(dk, msg, ""u8);
        if (err != default!) {
            Ꮡt.Fatalf("SignDeterministic: %v"u8, err);
        }
        o.Write(sig);
        (var pub, err) = newPublicKey(pk);
        if (err != default!) {
            Ꮡt.Fatalf("NewPublicKey: %v"u8, err);
        }
        if (pub.Value != dk.PublicKey().Value) {
            Ꮡt.Fatalf("public key mismatch"u8);
        }
        {
            var errΔ1 = Verify(dk.PublicKey(), msg, sig, ""u8); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Verify: %v"u8, errΔ1);
            }
        }
    }

    @string got = hex.EncodeToString(o.Sum(default!));
    if (got != expected) {
        Ꮡt.Errorf("got %s, expected %s"u8, got, expected);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mlDsa44ˢ = "ML-DSA-44"u8;
internal static readonly @string mlDsa65ˢ = "ML-DSA-65"u8;
internal static readonly @string mlDsa87ˢ = "ML-DSA-87"u8;

public static void TestMLDSAGenerateKey(ж<testing.T> Ꮡt) {
    Ꮡt.Run(mlDsa44ˢ, (ж<testing.T> tΔ1) => {
        testMLDSAGenerateKey(tΔ1, GenerateKey44, NewPrivateKey44);
    });
    Ꮡt.Run(mlDsa65ˢ, (ж<testing.T> tΔ2) => {
        testMLDSAGenerateKey(tΔ2, GenerateKey65, NewPrivateKey65);
    });
    Ꮡt.Run(mlDsa87ˢ, (ж<testing.T> tΔ3) => {
        testMLDSAGenerateKey(tΔ3, GenerateKey87, NewPrivateKey87);
    });
}

internal static void testMLDSAGenerateKey(ж<testing.T> Ꮡt, Func<ж<mldsa.PrivateKey>> generateKey, Func<slice<byte>, (ж<mldsa.PrivateKey>, error)> newPrivateKey) {
    var k1 = generateKey();
    var k2 = generateKey();
    if (k1.Equal(k2)) {
        Ꮡt.Errorf("two generated keys are equal"u8);
    }
    var (k1x, err) = newPrivateKey(k1.Bytes());
    if (err != default!) {
        Ꮡt.Fatalf("NewPrivateKey: %v"u8, err);
    }
    if (!k1.Equal(k1x)) {
        Ꮡt.Errorf("generated key and re-parsed key are not equal"u8);
    }
}

public static void TestMLDSAAllocations(ж<testing.T> Ꮡt) {
    // We allocate the PrivateKey (k and kk) and PublicKey (pk) structs and the
    // public key (pkBytes) and signature (sig) byte slices on the heap. They
    // are all large and for the byte slices variable-length. Still, check we
    // are not slipping more allocations in.
    float64 expected = 5D;
    if (fips140.Enabled) {
        // The PCT does a sign/verify cycle, which allocates a signature slice.
        expected += 1D;
    }
    cryptotest.SkipTestAllocations(Ꮡt);
    {
        var allocs = testing.AllocsPerRun(100, () => {
            var k = GenerateKey44();
            var seed = k.Bytes();
            var (kk, err) = NewPrivateKey44(seed);
            if (err != default!) {
                Ꮡt.Fatalf("NewPrivateKey44: %v"u8, err);
            }
            if (!k.Equal(kk)) {
                Ꮡt.Fatalf("keys not equal"u8);
            }
            var pkBytes = k.PublicKey().Bytes();
            (var pk, err) = NewPublicKey44(pkBytes);
            if (err != default!) {
                Ꮡt.Fatalf("NewPublicKey44: %v"u8, err);
            }
            var message = slice<byte>("Hello, world!"u8);
            @string context = testˢ;
            (var sig, err) = Sign(k, message, context);
            if (err != default!) {
                Ꮡt.Fatalf("Sign: %v"u8, err);
            }
            {
                var errΔ1 = Verify(pk, message, sig, context); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("Verify: %v"u8, errΔ1);
                }
            }
        }); if (allocs > expected) {
            Ꮡt.Errorf("expected %0.0f allocations, got %0.1f"u8, expected, allocs);
        }
    }
}

public static void BenchmarkMLDSASign(ж<testing.B> Ꮡb) {
    // Signing works by rejection sampling, which introduces massive variance in
    // individual signing times. To get stable but correct results, we benchmark
    // a series of representative operations, engineered to have the same
    // distribution of rejection counts and reasons as the average case. See also
    // https://words.filippo.io/rsa-keygen-bench/ for a similar approach.
    Ꮡb.Run(mlDsa44ˢ, (ж<testing.B> bΔ1) => {
        benchmarkMLDSASign(bΔ1, NewPrivateKey44, benchmarkMessagesMLDSA44);
    });
    Ꮡb.Run(mlDsa65ˢ, (ж<testing.B> bΔ2) => {
        benchmarkMLDSASign(bΔ2, NewPrivateKey65, benchmarkMessagesMLDSA65);
    });
    Ꮡb.Run(mlDsa87ˢ, (ж<testing.B> bΔ3) => {
        benchmarkMLDSASign(bΔ3, NewPrivateKey87, benchmarkMessagesMLDSA87);
    });
}

internal static void benchmarkMLDSASign(ж<testing.B> Ꮡb, Func<slice<byte>, (ж<mldsa.PrivateKey>, error)> newPrivateKey, slice<@string> messages) {
    var seed = new slice<byte>(32);
    var (priv, err) = newPrivateKey(seed);
    if (err != default!) {
        Ꮡb.Fatalf("NewPrivateKey: %v"u8, err);
    }
    var messagesʗ1 = messages;
    rand.Shuffle(len(messages), (nint iΔ1, nint j) => {
        (messagesʗ1[iΔ1], messagesʗ1[j]) = (messagesʗ1[j], messagesʗ1[iΔ1]);
    });
    nint i = 0;
    while (Ꮡb.Loop()) {
        @string msg = messages[i];
        {
            i++; if (i >= len(messages)) {
                i = 0;
            }
        }
        SignDeterministic(priv, slice<byte>(msg), ""u8);
    }
}

// BenchmarkMLDSAVerify runs both public key parsing and signature verification,
// since pre-computation can be easily moved between the two, but in practice
// most uses of verification are for fresh public keys (unlike signing).
public static void BenchmarkMLDSAVerify(ж<testing.B> Ꮡb) {
    Ꮡb.Run(mlDsa44ˢ, (ж<testing.B> bΔ1) => {
        benchmarkMLDSAVerify(bΔ1, GenerateKey44, NewPublicKey44);
    });
    Ꮡb.Run(mlDsa65ˢ, (ж<testing.B> bΔ2) => {
        benchmarkMLDSAVerify(bΔ2, GenerateKey65, NewPublicKey65);
    });
    Ꮡb.Run(mlDsa87ˢ, (ж<testing.B> bΔ3) => {
        benchmarkMLDSAVerify(bΔ3, GenerateKey87, NewPublicKey87);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextˢ = "context"u8;

internal static void benchmarkMLDSAVerify(ж<testing.B> Ꮡb, Func<ж<mldsa.PrivateKey>> generateKey, Func<slice<byte>, (ж<mldsaꓸPublicKey>, error)> newPublicKey) {
    var priv = generateKey();
    var msg = new slice<byte>(128);
    var (sig, err) = SignDeterministic(priv, msg, contextˢ);
    if (err != default!) {
        Ꮡb.Fatalf("SignDeterministic: %v"u8, err);
    }
    var pub = priv.PublicKey().Bytes();
    while (Ꮡb.Loop()) {
        var (pk, errΔ1) = newPublicKey(pub);
        if (errΔ1 != default!) {
            Ꮡb.Fatalf("NewPublicKey: %v"u8, errΔ1);
        }
        {
            var errΔ2 = Verify(pk, msg, sig, contextˢ); if (errΔ2 != default!) {
                Ꮡb.Fatalf("Verify: %v"u8, errΔ2);
            }
        }
    }
}

public static void BenchmarkMLDSAKeygen(ж<testing.B> Ꮡb) {
    Ꮡb.Run(mlDsa44ˢ, (ж<testing.B> bΔ1) => {
        while (bΔ1.Loop()) {
            NewPrivateKey44(new slice<byte>(32));
        }
    });
    Ꮡb.Run(mlDsa65ˢ, (ж<testing.B> bΔ2) => {
        while (bΔ2.Loop()) {
            NewPrivateKey65(new slice<byte>(32));
        }
    });
    Ꮡb.Run(mlDsa87ˢ, (ж<testing.B> bΔ3) => {
        while (bΔ3.Loop()) {
            NewPrivateKey87(new slice<byte>(32));
        }
    });
}

internal static slice<@string> benchmarkMessagesMLDSA44 = new @string[]{
    "BUS7IAZWYOZ4JHJQYDWRTJL4V7"u8,
    "MK5HFFNP4TB5S6FM4KUFZSIXPD"u8,
    "DBFETUV4O56J57FXTXTIVCDIAR"u8,
    "I4FCMZ7UNLYAE2VVPKTE5ETXKL"u8,
    "56U76XRPOVFX3AU7MB2JHAP6JX"u8,
    "3ER6UPKIIDGCXLGLPU7KI3ODTN"u8,
    "JPQDX2IL3W5CYAFRZ4XUJOHQ3G"u8,
    "6AJOEI33Z3MLEBVC2Q67AYWK5L"u8,
    "WE3U36HYOPJ72RN3C74F6IOTTJ"u8,
    "NMPF5I3B2BKQG5RK26LMPQECCX"u8,
    "JRGAN2FA6IY7ESFGZ7PVI2RGWA"u8,
    "UIKLF6KNSIUHIIVNRKNUFRNR4W"u8,
    "HA252APFYUWHSZZFKP7CWGIBRY"u8,
    "JFY774TXRITQ6CIR56P2ZOTOL6"u8,
    "ZASYLW5Y3RAOC5NDZ2NCH5A4UY"u8,
    "42X4JXNPXMFRCFAE5AKR7XTFO7"u8,
    "YAHQUWUH534MUI2TYEKQR7VR3A"u8,
    "HBP7FGEXGSOZ5HNOVRGXZJU2KG"u8,
    "HG4O7DCRMYMQXASFLMYQ6NMIXK"u8,
    "2KPQMDZKS65CLJU4DHTMVV5WI3"u8,
    "G6YSUTEX4HHL44ISK2JVVK45BV"u8,
    "PUJGPEQUBQM3IK2EXDQFJ2WGBG"u8,
    "PNS6HMQAWA3RORSMSNEUAINMIR"u8,
    "L35MZS4XYIJK453OFXCZG4WHIK"u8,
    "CRY54YZMFRF6JTB3FPNNBWPUOG"u8,
    "Y25TSZBWGU4HJCRMWZHAWXQ2DN"u8,
    "23W64TW3AKZPKCM4HMKEHFI6VQ"u8,
    "PWQAOZ24B4VLNEQR4XKN7LZHDI"u8,
    "YINPDR3ZSAKPPXP6J6VAXHIPYO"u8,
    "JDBB52ZRAB3PYBPNE7P4COY5PJ"u8,
    "4DYU52LQLVG3LTREOTLBCJK3XC"u8,
    "AB45MV6RKUGPCW4EUK7DX23MJX"u8,
    "HEJSITE5K7J6YJ74OEATVTCERV"u8,
    "ZKI5QCFCGM26UK7F5KYTENXKD2"u8,
    "VH5G3ZLF5XC22QAEJ6JDGOBE5Y"u8,
    "HYGXFHH3JW5SENG26MXLL54IGV"u8,
    "MJUCRL36JZ757UYHBFPCJBPZRH"u8,
    "IBH3T6NAVLCJQBYSVHAQFUITYA"u8,
    "VMWCS7JMIMFQB6TPRAMOUXIKWD"u8,
    "SXRPGPNNW2MMBKQS3HJURIQ3XV"u8,
    "YPPYMJZW6WYXPSCZIPI57NTP5L"u8,
    "N3SH6DUH6UOPU7YMQ6BJJEQSPI"u8,
    "Q243DGA6VC6CW66FFUAB5V3VLB"u8,
    "OUUBXEU4NJBRN5XZJ7YQUPIZLA"u8,
    "H5TWHVGC7FXG6MCKJQURD3RNWG"u8,
    "OONG2ZZ7H3P5BREEEURNJHBBQG"u8,
    "HWROSSRTBCQOAIQAY5S4EQG4FX"u8,
    "AJW6PW62JQNU72VKGIQMPBX64C"u8,
    "OXECVUVAWBBBXGGQGQBTYVEP4S"u8,
    "M5XN6V2LQJDEIN3G4Z6WJO6AVT"u8,
    "NHGJUX3WGRTEIRPFWC2I467ST4"u8,
    "SEOADTJDKAYYLDSC4VAES2CRDJ"u8,
    "J5AT674S577ZFGEURNIAGYOHKW"u8,
    "VJQVNMGHG4ITFX2XSPSDEWVZWD"u8,
    "ZWY3KJPXTAVWWVHNAJDUXZ52TG"u8,
    "HY46PBUGP4EMH34C6Q56MO7CJP"u8,
    "MQTUO7CF6R6CRJPVV6F673M6VW"u8,
    "35Z2Z5KV2RBJPQ7OZ24ZJE6BKR"u8,
    "OVUEVXBLCU2BBY25QP5WJACDIX"u8,
    "LNJX7PCLYL35WYJBW6CTXENPUU"u8,
    "IH7E766LCENOQ5ZKZVCMLEPACU"u8,
    "T2HZFGDDSFQ6YADB52NIFLBFEV"u8,
    "RHQUJMN4MB5SYY4FP4ARZH52QJ"u8,
    "W7GZC5ZM63UF2EJ7OC4WJM3OTH"u8,
    "T2NHNFVOMICY33AQZSR53HXFQ6"u8,
    "7ZVB4Y4K4Y2VAM5NC7HHAJNZIB"u8,
    "UX2I4VF62XJGP2XTNN6LDKXTOH"u8,
    "HJAMJR5RQTQW7JMW7ZLPRBZE7E"u8,
    "HKWSKX7MB5346PHYNWNBAYDSYK"u8,
    "BVWSB75HFLLE45MWA6EPHPTCFR"u8,
    "YDH2J6NMM7UINHGUOPIUI7PSSR"u8,
    "SYQPZLK52HMUAQFMVHGRJYKBEY"u8,
    "7AA6UQFGSPBGNUDPLWXSGNKKPP"u8,
    "AYXRJGRWZ5S3QOEDVWYHHCICHV"u8,
    "KFJYAWO7IATSBCSTDUAA5EPFAN"u8,
    "3JABTLB6T2ICHGVT3HXZZ3OAIT"u8,
    "WCM3IBOCQJ36WSG627CCNK3QA7"u8,
    "5FB5H3BZN2J4RGR2DUW7M37NKZ"u8,
    "VKDDAD3BVOMPSNEDGIRHKX5S6R"u8,
    "LFH5HVUR726OSFD3YVYM3ZHEIH"u8,
    "Y4ETQB2KZVFB4M7SALLCTHX2FB"u8,
    "E6SAU3C25MO2WBBVBKCKP2N4ZE"u8,
    "3JA54Q3NEKURB5EAPL2FOFIESD"u8,
    "FZPBW7BIQIW3FTKQD4TLKNWLMD"u8,
    "LY5W6XFA2ZRI53FTUJYGWZ5RX6"u8,
    "QID236JY3ICR55O5YRED33O7YT"u8,
    "HDRU3L6MFEBCBQFNLF5IRPMOAL"u8,
    "232ANKJBDBG4TSKQ7GJMWTHT23"u8,
    "CDWE3CELZM5AOJGYEFHMUNSP5O"u8,
    "7LNJRBOKN6W7RXUU34MDJ2SNKL"u8,
    "S3IZOADTW2A6E5IGRO5WKX7FVH"u8,
    "ZAISTLXC55EBMTN6KZ6QX5S7OS"u8,
    "4Z5ZIVCMFR2PY2PY4Z47T4YPYA"u8,
    "NE36L53Z6AMYQU7Q5REFUF76MK"u8,
    "WND5UP5M6KWPBRFP5WIWTOWV3I"u8,
    "7OC54DLFWMADJEMKEJ3Y2FMMZS"u8,
    "BWJVZHGEN43ULNIOZCPZOB64HG"u8,
    "VDFPQSR7RE54A75GT4JDZY5JK2"u8,
    "HFCD5EPBZBSVMXIDA47DZ6MRD6"u8,
    "RNBVFIUUJUM7EHRE3VNWSTORGO"u8,
    "VO5NLQJBR22CRRYUETGTU6JLMR"u8,
    "RZOMNFHBTL6HMGWH4PEEDASK7U"u8,
    "QL73UBTOLK5O2TW43YWAIKS6T3"u8,
    "NE3QVSMWS5G3W5C3BMKTJNMI2L"u8,
    "YHI6EYQ4GZMB2QPGHPUG2ZUOEL"u8,
    "6MBATW7MFNRUQBFD3GM35B7YPM"u8,
    "AIYRY6P5T4XU44CGVPEV6W43FR"u8,
    "MIAQ2FHXMAPY5NXSS45VRDPRMG"u8,
    "2SNLHQYKK2K6NSWOF6KPGZ3CPC"u8,
    "RVBHIQO5LH77ZWEAO3SVL72M2V"u8,
    "XXTGJCJNRSNLE7ARAH2UU6LVKR"u8,
    "DQMGILY5IDMWN5OYQYYXH26ZGR"u8,
    "627VTXXMM455KMTFNUUTKNFXPY"u8,
    "HC7IBFGLZCWGUR4K7REPMPW6W4"u8,
    "CHL6JRQUS7D4NML3PFT37PPZAA"u8,
    "Y767HXJAGJ75KE3JLO4DTLQIXC"u8,
    "NTIODXI5I7TF2KXXWXOAYGT7G4"u8,
    "PKZYEK2WAI4D4HEYYZH6H5IOMP"u8,
    "FG6J6G7HZDEDF4JQBQOTC7RQGZ"u8,
    "3VHM2VZU77Y25E3UUYZJLB2QLA"u8,
    "WRZQJQW7ARH4DXYHVLCJ4HRTTB"u8,
    "LQXKV5HD2AZHENSJ2VFLJ5YU5L"u8,
    "MF6Q4OA2EN6TG6BUDK7RWCQNPU"u8,
    "3USKYKPC5CB3EC4ZRMZVE3R2UO"u8,
    "3WICO2GVS3IRBFUHNDLNKWVP7N"u8,
    "P6ZR2UZZOVUZKT4KUS5WICW5XE"u8,
    "PYPZUU76RYVOUZGUUX33HLDKYA"u8,
    "2FTSURHV34VYTVIUU7W6V5C3NK"u8,
    "YABDYMGXS2MD2CYF3S4ALG4FLG"u8,
    "MHIBDH25RRPWV3P4VAWT6SAX3I"u8,
    "OINSMWJQ2UTOOKZ3X6ICXXBQR7"u8,
    "PFTQS7JNU2Q3Q6L4CGBXVLOYNE"u8,
    "A4MZ7CCVYQUDJ2AFHNXBBQ3D24"u8,
    "CPUB5R3ORTCMSMCLUQURE6AN5O"u8,
    "NF5E7U3DFTXWFFXXHUXTEP4VZQ"u8,
    "AWB5WDFERWSSJG53YGJMDORQKR"u8,
    "U5JQUILKD6SEL6LXAMNFZP6VSW"u8,
    "M45NLOAFLO74EJKG5EXNET6J5Y"u8,
    "P2KTEUMZ5DZZMYSPOHDR2WJXAN"u8,
    "KVO7AXZNFBUBPYLOTZQQ42TFNS"u8,
    "WGJJ7SAEV6SBBWWYS4BTLD63WM"u8,
    "Y6GURVDV4ESRBPWSTV25T4PE4K"u8,
    "ESK7MPFPUZ5ZAQ52RP4SQIYCCC"u8,
    "623M3CIABZ3RANERQ2IREXAVYO"u8,
    "OQ4CQCFO42RS4BMMSGSDLUTOQO"u8,
    "AMFHRDVGM6G2TIR3TKIFGFSDVM"u8,
    "7VVSGGCVC53PLOYG7YHPFUJM5X"u8,
    "Z3HMESVL7EZUSZNZ33WXEBHA2N"u8,
    "AWWVRQD5W7IBSQPS26XOJVDV5H"u8,
    "OQBZ5ZST3U3NZYHSIWRNROIG6L"u8,
    "II573BW7DJLBYJSPSYIABQWDZD"u8,
    "MOKXOQFOCUCLQQH4UKH2DPE7VN"u8,
    "XR54NGUOU6BBUUTINNWBPJ35HX"u8,
    "DNK36COZGFXI6DY7WLCNUETIRT"u8,
    "R5M2PV7E3EHEM3TLGRCL3HSFMC"u8,
    "ITKENZQYDQMZFCUPOT7VF3BMU7"u8,
    "5GDCB74PPPHEP5N5G3DVRCYT7R"u8,
    "ZMKXVRPLI5PY5BDVEPOA3NQZGN"u8,
    "GBLIALWTHTUDTOMDERQFVB77CS"u8,
    "VKRTTXUTFOK4PJAQQZCCT7TV3T"u8,
    "ZJBUJJ4SW62BXOID3XO2W2M2PF"u8,
    "SKWT5T6QJTCD3FCINIK22KMVBJ"u8,
    "EHINNU6L33HRLOOJ3A2XFJSYQL"u8,
    "N4HRQJEFPAT5SU3YPO74WSMQIR"u8,
    "TGPTZ3ENMFWB5CZKJFR5WHIRI4"u8,
    "O4HNFTAUJJ2LZPQXPXRAXOVABA"u8,
    "4JVB5STP2YG5GYOXDWIF4KCKFB"u8,
    "MY554X3YZHBECLHNNZ7A3SPJTU"u8,
    "ASCJMAH7VCQAD2QJSWXPSVSM3H"u8,
    "NBNGL5DZ623KCG2JNZFGZMZ7KD"u8,
    "KGMZSW35AEQOJ6FA7IR7BHZI52"u8,
    "Q7QUHHS4OJFMJ4I3FY6TDKSMZQ"u8,
    "MZAE7TOEXAS76T7KIC73FEYRU4"u8,
    "2BVESR3REAWADCGYOYM7T646RG"u8,
    "EK3L2ORP4LT3HU3EMXDSQWFOKJ"u8,
    "3X4A6VMGMIDLVK72FZSDHSERWY"u8,
    "I3UHWI6M6HQFRBSQ6W2SABUNUP"u8,
    "REKPXW4DIB4MTKMPHN3RBVHVME"u8,
    "W37FNFZE35NX65Z7CVQ7L5U4L5"u8,
    "4AGYK6U2KP6RAOADCBUDDCBECV"u8,
    "IXM4SFQUDW2NOTXZIPWTNGET3F"u8,
    "6YE4G3VELF27MN3Z5B4VIQ3XYK"u8,
    "LPOZCPZAG3MD47MIWGR4FIOCDH"u8,
    "WGREKUL2LD7C7SYGKH7APIY2A6"u8,
    "WWW277FKTKUXQMP4BECSRHLWJI"u8,
    "UYE4IQPMSTXVQG7EJALKWWEGDN"u8,
    "TIV2L5Z6K7SNGNUVWSNKTAF4UE"u8,
    "I3FQOAW3PINUK26P62HCX657FO"u8
}.slice();

internal static slice<@string> benchmarkMessagesMLDSA65 = new @string[]{
    "NDGEUBUDWGRJJ3A4UNZZQOEKNL"u8,
    "ACGYQUXN4POOFUENCLNCIPHFAZ"u8,
    "Z3XETEYKROVJH7SIHOIAYCTO42"u8,
    "DXWCVCEFULV7XHRWHJWSEXWES7"u8,
    "BCR2D5PNLGFYX6B3QFQFV23JZP"u8,
    "2DVP5HNG54ES64QK4D37PWUYTJ"u8,
    "UJM4ADPJLURAIQH4XA6QYUGNJ6"u8,
    "B5WRCIPK5IVZW52R6TJOKNPKZH"u8,
    "7QNL6JTSP62IGX6RCM2NHRMTKK"u8,
    "EJSZQYLM7G7AJCGIEVBV2UW7NN"u8,
    "UFNA2NKJ3QFWNHHL5CXZ4R5H46"u8,
    "QZAXRTT3E4DOGVTJCOTBG3WXQV"u8,
    "KH2ETOYZO5UHIHIKATWJMUVG27"u8,
    "V5HVVQTOWRXZ2PB4XWXSEKXUN5"u8,
    "5LA7NAFI2LESMH533XY45QVCQW"u8,
    "SMF4TWPTMJA2Z4F4OVETTLVRAY"u8,
    "FWZ5OJAFMLTQRREPYF4VDRPPGI"u8,
    "OK3QMNO3OZSKSR6Q4BFVOVRWTH"u8,
    "NQOVN6F6AOBOEGMJTVMF67KTIJ"u8,
    "CCLC4Y6YT3AQ3HGT2QNSYAUGNV"u8,
    "CAZJHCHBUYQ6OKZ7DMWMDDLIZQ"u8,
    "LVW5XDTHPKOW5D452SYD7AFO6Q"u8,
    "EYA6O6FTYPC6TRKZPRPX5N2KQ4"u8,
    "Z6SGAEZ2SAAZHPQO7GL7CUMBAG"u8,
    "FKUCKW6JQVF4WQYXUSXYZQMAVY"u8,
    "LN2KDF4DANPE4SC4GKJ4BES3IZ"u8,
    "AVCRTWB6ALOQHY34XI7NTMP2JH"u8,
    "A5WHIS6CBWPCYIEC6N2MBAOEZ6"u8,
    "JC2BH476BXUQFIDA6UCR5V4G4F"u8,
    "NU6XH6VLSSFHVSRZCYXPFYKYCD"u8,
    "GSUXVZBDDYSZYFGXNP6AZW3PTC"u8,
    "XJPRNJ26XP4MIYH2Q7M7MPZ73M"u8,
    "INUTUP3IRFWIIT23DNFTIYKCFY"u8,
    "T4KH7HKLEYGXHBIRFGFCRUZCC4"u8,
    "GGQX4JFVWZHE5Y73YTLMSSOXNS"u8,
    "BUA4Q3TQZGLVHMMJU62GQOSHLV"u8,
    "WXW3SJXLSZO2MYF4YFIMXL2IQP"u8,
    "Q32XBVVGFQTSXAIDJE6XSEPRZG"u8,
    "6TEXT6SA7INRCTDSCSVZJEQ2YG"u8,
    "ZBN4UL43C3SJIG4HYR236PXCVS"u8,
    "TVWPLLC7NROBREWOM75VA3XCR3"u8,
    "CCDGL2FURLBABQ4IJBYCB75JFR"u8,
    "XBZGCOVTZHCPAARBTMAKPIE6GJ"u8,
    "TPRAENJ7I54XRIVH6LL6FDIA3I"u8,
    "RKOM3PHFILPIIQZL4ILQWGRYWI"u8,
    "CEEZIZ2WUXHQQFATYYGQ3ZDBTI"u8,
    "SLKOVAP6WLIVJBVU7VZG3ZGEOW"u8,
    "TWMCLJJSWEEQQPQGGDKEJ5SU2R"u8,
    "IFMUXXCD2LC7IGQLZ2QEK5UOQ2"u8,
    "C7IWFEBHW2CXN4XBJS7VLWH3VK"u8,
    "7KJYUEW3F264727TM4LE6RMGDO"u8,
    "BPG2XAPBMBTA4VMPUM7IZVZPK3"u8,
    "Y5X577BWRZNPLNUHJVSKGMUXYB"u8,
    "ZCKMKM23E4IUPTNQDFN2LTLZVX"u8,
    "4RKK223JNBDAP4G5DOAHHZ3VNO"u8,
    "5UZ3TQZHZT22ISTB4WJEVO6MC4"u8,
    "YMVS4HFSJ32CRZRL23PXZUEJFJ"u8,
    "UQEUJUTPSZLZARNBXWMCTMHPFF"u8,
    "CZAAZ5WK7EIPMW7NA3EZNNBF45"u8,
    "227PBHH23WM7F2QLEZSPFYXVW4"u8,
    "YUYS2J5CRFXZ4J4KJT2ZKIZVW3"u8,
    "MFLHZJOZV44SN4AH6OJ3QZWM2O"u8,
    "H2B3CRBCXYN7QWDGYUPHQZP23A"u8,
    "T4L6YWQUQ3CTACENAJ5WUXZWFH"u8,
    "N723H6MUGPZSRZ72C635OD4BP7"u8,
    "NI4TUMVA6LQPQV2TXPN4QOIGBZ"u8,
    "CQI3S4LSTQASSJJVZXEFPOVW7K"u8,
    "ANPY4HJ64LLSB3GK2R4C6WDBS3"u8,
    "RGWQCZKQLMT5FZRDE4B3VMASVK"u8,
    "Q3WCCF2HA3CA4WWRJBMGBW7WI7"u8,
    "2AKJRXFHXLUQPOXPTLSZN5PW4A"u8,
    "IJWOOTI4N7RWXJIHAPXN6KEWEN"u8,
    "4D53T6N6ATOVTD4LKSTAAWBJMU"u8,
    "B4G5HDD6RITG6NIH6FXCRZDYZM"u8,
    "TJCDFKMRUY2OG6KRSMNVCGQFUP"u8,
    "PB33IHQKALAY6H6GVBVLI6ZRXK"u8,
    "SCCWGW2J5S4WL4FTTMQ435F6DB"u8,
    "ZVJH2HSMTLHGXMGPMXLJCKCLLE"u8,
    "62LG37U6JXR77YRZQQCDSBHVCS"u8,
    "BU4CBWOXQ352TEOKIXO245ID4O"u8,
    "UEZOH7KEIODSEVRUF6GMWGA2RB"u8,
    "IPJWROME4GM66CGLUWP5BJ4SX6"u8,
    "355GDC7TG64AZJ7IJX6K62KZCZ"u8,
    "AHTFKX3V7XUB3EWOMQVCGZYGUE"u8,
    "N4RV2GKXJ4SPHHJ52Z7K5EGLER"u8,
    "ZY7V7NE5F66XHDHWM6YNFEWZA6"u8,
    "DIKFO5KAVT4WAP7BOEFM56ZUSR"u8,
    "4TDFOFKDAPIOM3MU5GD7NPXNWQ"u8,
    "AD7YZO756HDK6YWFILAKW3JWA7"u8,
    "NUA53JS2ZK2BGHH3A7BJTJZYW7"u8,
    "QLCNC3AQNKLRMSYR62WQSQP5VI"u8,
    "SJ7OBS7ZYXSGXOYXPE5KW2XKN6"u8,
    "44HBMOGMIMJS63CEXQU7FCXE2E"u8,
    "KCK3J7ZL6QF4SLHHSWTJURK7PG"u8,
    "HLH4CLUGBSOOBSS3BPO62N5MC3"u8,
    "3FNS4GITO6OEUBAVDDXK4WOBTD"u8,
    "IAC3K3I4AQGY3G6UHG7PL2N6TE"u8,
    "KUKLNH74POJI5DYAEWUD7RABTQ"u8,
    "ETM6N7VU3GBSQ7P5MCD6UF3E3S"u8,
    "IZITM5NYBGJZLSI3BI4VEMW43U"u8,
    "46OPQU4LL6N3Z2U7KYPKUMBAGI"u8,
    "EV7YZ5DMAV7VKYJQUFSRD37GPP"u8,
    "AV7W2PGYDJIAKLFVEBL6BXQSGC"u8,
    "M2FOX5QZEZKV4QXKPI5XUZDHEM"u8,
    "R4IFPLVMOVYCHRTR6LXAUGP3LL"u8,
    "JGH6XJUMP4DRVAM27P2JNOKXVO"u8,
    "D2XN3ZLLU6VFPMDYM7NBHSQEOI"u8,
    "2PO3BYENOMQK6SHQDCFSRPJQI3"u8,
    "IBVQ7U3QEUC6PQRE4PV53JTZTK"u8,
    "ZBCOX4P7NG2IXXFB2R43MG2SLV"u8,
    "5NJDPQVVDO7ADNZ2CV7L6QBNGZ"u8,
    "V7ASFIIYUMXFGW4B7ZM6LOGUTE"u8,
    "PX5IJZ7W2LUPKM6YN4PMZ43ZLM"u8,
    "AYK7SZ23DHC7Q56MWAJXBG76LB"u8,
    "UYCAPXJM4HNGKLIDSZ4NCEDJLN"u8,
    "UWMDZ3C2ODLACKGJPGETNQ3TA4"u8,
    "Q6OI6R3WYYJ4CCZCDJBQMCRCZR"u8,
    "LCMJHLP7354APCEGPKE7HHWTWB"u8,
    "N7T7ZKOYPAMEYTTDOWZNCN6PRD"u8,
    "UZADPU4UNHAF7L7LQDMTKA2EQH"u8,
    "DC2OEPQDECVLRVNNCS6BMH4CRA"u8,
    "37IZ427XHUMZ66EJ62U2YEZDAC"u8,
    "6BCZDQZDPZLS5OGESKNUBPSSFV"u8,
    "ST2LEMJ4OLQ32TJTLH2WCWT4WA"u8,
    "GA2TL4SFLEW4G2B5PQMIKJT5XG"u8,
    "L7PPBIET26EH7LQTLEFC4I4EIA"u8,
    "6YSM7MC2W4DEV6ULAHMX27LH56"u8,
    "QL26Z5KZ4YRRG2BXXGDRRLV357"u8,
    "677TWRAJ5NSNHCE243POQPEG7K"u8,
    "66MEBQJLGAGVXDX3KZ2YFTTVJM"u8,
    "6D4VUWAQD6R65ICSDLFAATC67V"u8,
    "7GXLD5CNU3TDUQSSW42SHL7B5D"u8,
    "RQETUMEBG2ZM2NF2EZAQHGHWWE"u8,
    "DCRX5ANWDMXZFIDVAXYLQZYMRN"u8,
    "5SDWT7YAF7L4WWANAGYINZAYXH"u8,
    "PZILRV7I2S6WKUSHKYRLA2JQY3"u8,
    "2G66TK2PZ5MOTAZDN7BFS3LAIH"u8,
    "QOLJ3WGJ6JS3FMMXBNTNAIKXVK"u8,
    "FMAL67YTHDCCYVZ5CRMN2XJPDN"u8,
    "UOTZDXTJKQ3YAIRKHTYNX6G55P"u8,
    "X3DLNPJ3V62LRHGEY4DTT35H3R"u8,
    "DKU7CHNXPB5QRZVGIQZW46XCKC"u8,
    "RAKBD4LQKEDTVDSK3DVTRWG23B"u8,
    "INTRA7BWHLVQMBRKBJNUSMF7MU"u8,
    "AUYRBNVCOYYHOHUYOOFIZ2FWMD"u8,
    "22EJVDEQ7PASLBAMTVKXOQP5RJ"u8,
    "3S6NATWA57SFTZEW7UZUOUYAEU"u8
}.slice();

internal static slice<@string> benchmarkMessagesMLDSA87 = new @string[]{
    "LQQPGPNUME6QDNDTQTS4BA7I7M"u8,
    "PTYEEJ7RMI6MXNN6PZH222Y6QI"u8,
    "R6DTHAADKNMEADDK5ECPNOTOAT"u8,
    "S2QM7VDC6UKRQNRETZMNAZ6SJT"u8,
    "EYULPTSJORQJCNYNYVHDFN4N3F"u8,
    "YETZNHZ75SXFU672VQ5WXYEPV2"u8,
    "KTSND3JGA4AN3PCMG4455JEXGR"u8,
    "JGE6HK37O6XMWZQZCHFUPNUEXP"u8,
    "CRYB2FZD2BYNANBFFO2HRZEHGZ"u8,
    "7MLNDZJ7OIEPBJZOMULOMQH2BA"u8,
    "4WQCNTIFVSX2DNALMWUKZRA6CI"u8,
    "Y5NK4OBDSDWC5WLL27CEEXYYOT"u8,
    "C4SSWSPBVCDAWJXH2CDMXR36LH"u8,
    "THDBKXRTKWJUGJMAAYTWTFMX7Z"u8,
    "NWXPUD4DAA6QOREW4AFFYQYQNG"u8,
    "3RQIJXMO7WYHBEBL3G6EOLNZNQ"u8,
    "R7JEOHFP2C7O4AVPRPRELXWOMM"u8,
    "LU6MWR7SZXVIKS54BY62X67NPA"u8,
    "FG2FFM4F2ECKHCSJ75KXK632JP"u8,
    "BF76ZDSVVUSYS5KK4FFD22YPS7"u8,
    "HCLBWZRLHEMYZLFWHLAN2BKCZ7"u8,
    "HGFVS4QC7AWXYPVRSWAK77KTQF"u8,
    "LUZ3C53PUUHBWCDJ7WAHK2UT3K"u8,
    "Y3WR6SMDUBW34N3MUT7EQYIJCV"u8,
    "F2X35AQTXVZBMPXTWNAAH4ZX2W"u8,
    "6MKFFDYWD6ZAKS3C6GRCRLZLRF"u8,
    "AFMZYYFRHKMQRNKU5UTSKQ74H6"u8,
    "TDTN7J3O367OVPWLESRNPLN4M2"u8,
    "WYMLD2X6N4CZ2RDOKF5CFTSYTG"u8,
    "UNPTSBLJ6HZRNR72T2VEEHCFX2"u8,
    "SNCM4R2P27AJOXBS67RMCARS3U"u8,
    "OU7QBE5QOXO7CIYTBJR3KOW2WK"u8,
    "2NNQOBQKZ2OD4ZAXI3SNEURYUP"u8,
    "YQTUPOYBT67XPCHIGKSGSKC3BZ"u8,
    "HGB4ZM3G76IXYWWCMVT3HONRIS"u8,
    "WZC6QUKRZZ2TOVA277JYKQITEW"u8,
    "XO2WT46A5HYL6CUJF7SGJ6YWOG"u8,
    "4QJA35PMYQIDRZ7ZHG7RLZJVGF"u8,
    "BMJZELWZ4I2UWXESU3NR6ATC4M"u8,
    "XWLFB7FN6D5PRY6YUXC5JUIBFM"u8,
    "WRAFFF27AVTIOYIBYA2IPTXI3R"u8,
    "VOXUTYTN2XZ362OJFO2R53UCUF"u8,
    "UHN73ARJ737WUJ6QYEI7U46OPO"u8,
    "3Y3K5E2A4ML3VYVNAFWEEIXTSN"u8,
    "QMU4322NKPRLE7JBGYFGS36H2S"u8,
    "NJAQTNCXPVDICTDVUKTPRCD2AX"u8,
    "OC373ZFBNV2H46T6OY3XRPSUHG"u8,
    "UBLAS6CDWE3A662MLKP7QDEOCC"u8,
    "BKFDLAL2RTPMERYVW3B7UJ5W3H"u8,
    "QFKFGXKGW5SAKLBAWQXUWW77OS"u8,
    "EJNUQHTLLOVB4ARETOGLY4WUTJ"u8,
    "N243OCMVLLAO6I2XLCYOIMQYGY"u8,
    "YRRFLWK7ZASUKYX7ZLQMW2PJ6X"u8,
    "3DGVPBWD2BIK6KQE65K72DNJNM"u8,
    "TJRYMNOAIW33VIHKLJG4GXAVUK"u8,
    "6DSRINAYXL34U54U355U7IVFGS"u8,
    "6CHA4MX7LVS77XKRWG7IYC3XVL"u8,
    "GM2CEGBEPBOHAPIOBUWJ4MJNTG"u8,
    "VJKHGBY33VUIJFEQLX3JVUNQBD"u8,
    "DTOHAD5M2KL46IZHE4TPLJWHTI"u8,
    "IYFG3UDN7ROOY2ZFSLM2BU2LMQ"u8,
    "A5OGJHPOE4PW6QSZYHZ5TKPGIC"u8,
    "FX4BCN67AEGCLUTLFPNDL3SQU5"u8,
    "MWIZQVOZOHTTBUXC3BEX62MNI5"u8,
    "BYHVJHBLK4O6LFSKEIQ3CAAKU7"u8,
    "QJU7P6KWSSKAA5GVA6RH4OV7MX"u8,
    "I3T3XM5Z5TAJHAYDQHFA2ZV7PU"u8,
    "L46MQCHV3TJ6FYIQQ2FCJXES74"u8,
    "QXZRQIYAJMXYR6PU3VDYGCIT5W"u8,
    "MFS53RR2XEYS22NYOJLGTHVTTM"u8,
    "FRWIWJRP4AQMXWX4WJ4WYVKM3E"u8,
    "X6GK6IGVLJWYSHLKHGXSW3TJDP"u8,
    "L5LPJ2HIWA4UY6G6FMZXGDEDAM"u8,
    "GD6FYOYUGDHXEQ5S2KLJEGNSN7"u8,
    "ODAL7ZRKXSPAAN5DVRBWJQCFQX"u8,
    "CV3QFBDXBPT3SCPJGUYSMDN6ZS"u8,
    "IGSLSACRZ6XID466KQIB4YNGYO"u8,
    "WZ2EACBN26RAML2S52YXRYP2OF"u8,
    "LB76VEVNOBYFMKFZ7SDFCBCHQE"u8,
    "TLFA7EU3JJFAP6EMUKNV2ZXRBM"u8,
    "SIIJF6OXAKRP25CBUYFBRCDDVP"u8,
    "TEPNI7TJ7HASJWIQMBS4VFLRQC"u8,
    "VK2JINYWEDV7IQFWH4OTAD4W5O"u8,
    "GILUH5AMVE4TM7EKPXJBZGT6EJ"u8,
    "DV7ALFRAW3TI4WMQQLDTO6RNHN"u8,
    "CAIB5G3NXC5ASPLFIWAFPVHS5B"u8,
    "MLFJXZUOAGN7EGPMXOOVTB2CL4"u8,
    "6MZYT3ANWHBOS67WGHZI3QPEAP"u8,
    "LVJDQB52C2PERSSQJRMRCJ4UBF"u8,
    "QY4VKAZAYQIZOX2L2VO2QHAQVC"u8,
    "UAA5SST2XA76JPKM3XOZ5RUHFI"u8,
    "VLZWF53JSQ6SCRUFDKVPXWAS4L"u8,
    "NX2DZIKMJIYXUNSAHFP23FHTBU"u8,
    "F5OAKDDDA34A2RPIKDPM5CYPMZ"u8,
    "E5PEP3ANIK2L4VLOST4NIYNKBD"u8,
    "IPBGFLHSMP4UFXF6XJX42T6CAL"u8,
    "XHPU7DBFTZB2TX5K34AD6DJTK3"u8,
    "2ZU7EJN2DG2UMT6HX5KGS2RFT6"u8,
    "SD5S7U34WSE4GBPKVDUDZLBIEH"u8,
    "WZFFL3BTQAV4VQMSAGCS45SGG3"u8,
    "QE7ZT2LI4CA5DLSVMHV6CP3E3V"u8,
    "YIWMS6AS72Z5N2ALZNFGCYC5QL"u8,
    "A4QJ5FNY54THAKBOB65K2JBIV7"u8,
    "6LORQGA3QO7TNADHEIINQZEE26"u8,
    "5V45M6RAKOZDMONYY4DIH3ZBL2"u8,
    "SVP7UYIZ5RTLWRKFLCWHAQV3Y2"u8,
    "C2UYQL2BBE4VLUJ3IFNFMHAN7O"u8,
    "P4DS44LGP2ERZB3OB7JISQKBXA"u8,
    "A6B4O5MWALOEHLILSVDOIXHQ4Z"u8,
    "DKQJTW5QF7KDZA3IR4X5R5F3CG"u8,
    "H6QFQX2C2QTH3YKEOO57SQS23J"u8,
    "DIF373ML2RWZMEOIVUHFXKUG7O"u8,
    "Z5PPIA3GJ74QXFFCOSUAQMN5YN"u8,
    "PM6XIDECSS5S77UXMB55VZHZSE"u8
}.slice();

} // end fipstest_internal_test_package
