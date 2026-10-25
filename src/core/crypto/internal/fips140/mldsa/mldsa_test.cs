// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha256 = go.crypto.sha256_package;
using hex = encoding.hex_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal.fips140;
using static go.crypto.@internal.fips140.mldsa_package;

partial class mldsa_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathˢ = "/Path/"u8;
internal static readonly object rejectionˢ = (@string)"rejection"u8;

internal partial struct TestACVPRejectionKATs_testCases /*dyn*/ {
    internal @string name;
    internal @string seed; // input to ML-DSA.KeyGen_internal
    internal @string keyHash; // SHA2-256(pk || sk)
    internal @string msg; // M' input to ML-DSA.Sign_internal
    internal @string sigHash; // SHA2-256(sig)
    internal Func<slice<byte>, (ж<global::go.crypto.@internal.fips140.mldsa_package.PrivateKey>, error)> newPrivateKey;
    internal Func<slice<byte>, (ж<global::go.crypto.@internal.fips140.mldsa_package.ΔPublicKey>, error)> newPublicKey;
}

// Most tests are in crypto/internal/fips140test/mldsa_test.go, so they can
// apply to all FIPS 140-3 module versions. This file contains only tests that
// need access to the unexported symbol testingOnlyRejectionReason.
public static void TestACVPRejectionKATs(ж<testing.T> Ꮡt) {
    var testCases = new TestACVPRejectionKATs_testCases[]{
        // https://pages.nist.gov/ACVP/draft-celi-acvp-ml-dsa.html#table-1
        // ML-DSA Algorithm 7 ML-DSA.Sign_internal() Known Answer Tests for Rejection Cases
        new(
            "Path/ML-DSA-44/1"u8,
            "5C624FCC1862452452D0C665840D8237F43108E5499EDCDC108FBC49D596E4B7"u8,
            "AC825C59D8A4C453A2C4EFEA8395741CA404F3000E28D56B25D03BB402E5CB2F"u8,
            "951FDF5473A4CBA6D9E5B5DB7E79FB8173921BA5B13E9271401B8F907B8B7D5B"u8,
            "DCC71A421BC6FFAFB7DF0C7F6D018A19ADA154D1E2EE360ED533CECD5DC980AD"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Path/ML-DSA-44/2"u8,
            "836EABEDB4D2CD9BE6A4D957CF5EE6BF489304136864C55C2C5F01DA5047D18B"u8,
            "E1FF40D96E3552FAB531D1715084B7E38CCDBACC0A8AF94C30959FB4C7F5A445"u8,
            "199A0AB735E9004163DD02D319A61CFE81638E3BF47BB1E90E90D6E3EA545247"u8,
            "A2608BC27E60541D27B6A14F460D54A48C0298DCC3F45999F29047A3135C4941"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Path/ML-DSA-44/3"u8,
            "CA5A01E1EA6552CB5C9803462B94C2F1DC9D13BB17A6ACE510D157056A2C6114"u8,
            "A4652DC4A271095268DD84A5B0744DFDBE2E642E4D41FBC4329C2FBA534C0E13"u8,
            "8C8CACA88FFF52B9330510537B3701B3993F3726136A650F48F8604551550832"u8,
            "B4B142209137397DAD504CAED01D390ADAF49973D8D2414FC3457FB7AF775189"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Path/ML-DSA-44/4"u8,
            "9C005F1550B4F31855C6B92F978736733F37791CB39DD182D7BA5732BDC2483E"u8,
            "2485AA99345F1B334D4D94B610FBFFCCB626CBFD4E9FF0E1F6FC35093C423544"u8,
            "B744343F30F7FEE088998BA574E799F1BF3939C06C29BF9AC10F3588A57E21E2"u8,
            "5B80A60BAA480B9D0C7D2C05B50928C4BF6808DDA693642058A3EB77EAA768FC"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Path/ML-DSA-44/5"u8,
            "4FAB5485B009399E8AE6FC3D3EEFBFE8E09796E4477AABD5EB1CC908FA734DE3"u8,
            "CB56909A7CF3008A662DC635EDCB79DC151CA7ACBAE17B544384ABD91BBBC1E9"u8,
            "7CAB0FDCF4BEA5F039137478AA45C9C48EF96D906FC49F6E2F138111BF1B4A4E"u8,
            "6CC38D73D639682ABC556DC6DCF436DE24033091F34004F410FABC6887F77AB0"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Path/ML-DSA-65/1"u8,
            "464756A985E5DF03739D95DD309C1ED9C5B04254CC294E7E7EB9B9365EE15117"u8,
            "AE95EA0DAA80199E7B4A74EB5A1B1DC6C3805BD01D2FA78D7C4FBA8C255AA13D"u8,
            "491101BBA044DE6E44A63796C33CDA051BB05A60725B87AF4BA9DB940C03AC09"u8,
            "8E08EA0C8DB941685B9905A73B0B57BAD3500B1F73490480B24375B41230CC04"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Path/ML-DSA-65/2"u8,
            "235A48DB4CA7916B884F424A8586EFD517E87C64AECEC0FCE9A3CC212BA1522E"u8,
            "1AC58A909DB4D7BC2473AB5E24AF768279C76F86A82D448258E24EEA4EA6B713"u8,
            "F8CE85CB2EC474FFBF5A3FFAE029CE6F4526B8D597655067F97F438B81071E9B"u8,
            "AE9531A01738615B6D33C77B3FF618A86E101FDC4C8504681F0EDFA64511AD63"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Path/ML-DSA-65/3"u8,
            "E13131B705A760305FEFFEBFE99082E2691A444BBEFCC3EDF67D909886200207"u8,
            "B422093F95CC489C52F4FA2B8973A2FDDD44426D1D04D1AAEEFC8715D417181F"u8,
            "CD365512C7E61BBAA130800B37F3BB46AAF1BEEF3742EA8A9010A6DD4576ED0B"u8,
            "3C55E604DECA7B89A99305D7A391C35F66A17C1923F467675EC951C0948D21C9"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Path/ML-DSA-65/4"u8,
            "0A4793E040A4BC0D0F37643D12C1EA1F10648724609936C76E0EC83E37209E92"u8,
            "622D26D536D4D66CD94956B33A74E2E830ED265D25C34FF7C3E5243403146ADF"u8,
            "6D9C7A795E48D80A892CBF4D4558429787277E3806EB5D0BCE1640EEBBBF9AEC"u8,
            "3B141110B9F56540B2D49AACDE6399974A4EAC40621E367E68D4504F294DB21B"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Path/ML-DSA-65/5"u8,
            "F865B889E5022D54BABC81CA67E7EB39F1AC42F92CF5295C3DA5C9667DB1B924"u8,
            "45BC8EDD1A620C46E973E346844270721824D97888BC174281852D98B7E8F4A3"u8,
            "047AFAADBE020ED2D766DA85317DEDE80BE550545F0B21E3F555A990F8004258"u8,
            "56308A3578360C41356BA9C97D3240E01767FA76BBBA9FD0CC6CFA9ADD088DB9"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Path/ML-DSA-87/1"u8,
            "0D58219132746BE077DFE821E9F8FD87857B28AB91D6A567E312A73E2636032C"u8,
            "4D261270341A7AC6B66900DDC2B8AB34AB483C897410DDF3B2C072BDDA416434"u8,
            "3AA49EF72D010AEC19383BA1E83EC2DD3DCC207A96FFCEB9FFA269E3E3D66400"u8,
            "5049DC39045618B903C71595B3A3E07A731F95D37304623ACC98BCEF4258B4CA"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Path/ML-DSA-87/2"u8,
            "146C47AB9F88408EB76A813294D533B29D7E0FDA75DA5A4E7C69EB61EFEEBB78"u8,
            "05194438AF855B79DB8CCCCB647D6BA5C7AAF901BBD09D3B29395F0EA431D164"u8,
            "82C44F998A8D24F056084D0E80ECFD8434493385A284C69974923C270D397782"u8,
            "CFFC5988A351E14A3EE1282F042A143679C4503814296B27993949A7FF966F57"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Path/ML-DSA-87/3"u8,
            "049D9B0B646A2AC7F50B63CE5E4BFE44C9B87634F4FF6C14C513E388B8A1F808"u8,
            "AC8FE6B2FE26591B129EA536A9A001C785D8ACBDD9489F6E51469A156E9E635D"u8,
            "FEBC9F8AE159002BE1A11D395959DD7FC20718135690CDAA2BCFB5801C02AB89"u8,
            "FF4006089BDF7337E868F86DDF48F239D2A52EA1D0F686E0103BF19C3B571DB1"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Path/ML-DSA-87/4"u8,
            "9823DDDE446A8EA883DAD3AC6477F79839FDC2D2DEF2416BE0A8B71CFBC3F5C6"u8,
            "525010E307C4EA7667D54EE27007C219B01F4CF88DC3AB2DE8E9AAA59440A884"u8,
            "F7592C97C1A96A2F4053588F5CDAD4C50BF7C3752709854FA27779B445DD2BA2"u8,
            "FD7757602B83B0A67A314CD5BCC880E7AE47ACDF4D6AF98269028EFB486838F7"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Path/ML-DSA-87/5"u8,
            "AE213FE8589B414F53780D8B9B6837179967E13CB474C5AD365C043778D2BC90"u8,
            "D4988E91064E5DF6D867434D1DED16DCD8533E39E420DC2B4EB9E40A84146F7D"u8,
            "19C1913BA76FF04596BB7CC80FD825A5AEDEF5D5AD61CEDB5203E6D7EDB18877"u8,
            "23FE743EDD101970D499E7EB57A7AA245BAF417E851B260C55DD525A445F08DA"u8,
            NewPrivateKey87, NewPublicKey87
        ),

        // https://pages.nist.gov/ACVP/draft-celi-acvp-ml-dsa.html#table-2
        // ML-DSA Algorithm 7 ML-DSA.Sign_internal() Known Answer Tests for Number of Rejection Cases

        new(
            "Count/ML-DSA-44/77"u8,
            "090D97C1F4166EB32CA67C5FB564ACBE0735DB4AF4B8DB3A7C2CE7402357CA44"u8,
            "26D79E4068040E996BC9EB5034C20489C0AD38DC2FEC1918D0760C8621872408"u8,
            "E3838364B37F47EDFCA2B577B20B80C3CB51B9F56E0E4CDB7DF002C874039252"u8,
            "CD91150C610FF02DE1DD7049C309EFE800CE5C1BC2E5A32D752AB62C5BF5E16F"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Count/ML-DSA-44/100"u8,
            "CFC73D07A883543A804F770070861825143A62F2F97D05FCE00FD8B25D29A43F"u8,
            "89142AB26D6EB6C01FA3F189A9C877597740D685983F29BBDD3596648266AE0E"u8,
            "0960C13E9BA467A938450120CC96FF6F04B7E557C99A838619A48F9A38738AB8"u8,
            "B6296FFF0C1F23DE4906D58144B00A2DB13AD25E49B4B8573A62EFEECB544DD7"u8,
            NewPrivateKey44, NewPublicKey44
        ),
        new(
            "Count/ML-DSA-65/64"u8,
            "26B605C78AC762FA1634C6F91DD117C4FBFF7F3A7E7781F0CC83B6281F04AD7F"u8,
            "5DA13E571DF80867A8F27E0FF81BE7252A1ABF89B3D6A03D4036AF643EFBB04B"u8,
            "C9B07E7DDC0274468F312F5C692A54AC73D1E34D8638E20A2CD3C788F27D4355"u8,
            "12A4637E3A833A5A2A46F6A991399E544B62A230B7AA82F7366840FF6A88DE61"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Count/ML-DSA-65/73"u8,
            "9191CF381BEE17475C011986EFB6AFB1EFA6997442FD33427353F1DA1AA39FC0"u8,
            "7930D4E52BA03B61DAA57743B39E291D824DC156356C6B1A8232574D5C8BDD08"u8,
            "E616E36E81AA1EC39262109421AE0DDDA5E3B5A8F4A252BCA27AE882538DF618"u8,
            "3D758ACE312433D780403B3D4273171FB93D008B395352142C6DC5173E517310"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Count/ML-DSA-65/66"u8,
            "516912C7B90A3DBE009B7478DBCAF0F5C5C9ED9699A20D0CA56CC516E5A444CD"u8,
            "0FD15951B93A4D19446B48D47D32D2CA2253FF43BB8CCCB34C07E5F1A3181B7A"u8,
            "9247CA75F9456226A0C783DABCC33FF5B4B489575ADED543E74B29B45F9C8EF2"u8,
            "E5CE267800EDF33588451050F9B4A5BF97030D045132A7E3ED9210E74028D23B"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Count/ML-DSA-65/65"u8,
            "D4B841F882D50AB9E590066BAFABA0F0D04D32641C0B978E54CCAA69A6E8D2C4"u8,
            "0039C128DDE6923EA08FF14F5C5C66DCB282B471FD1917DBEBE07C8C45B73F8A"u8,
            "175231657B0F3C7065947999467C342064F29BFAEB553E97561407D5560E3AEB"u8,
            "8830EA254AF2854BF67C2B907E2321C94FD6EFB2FDAA77669FC3A5C4426C57C9"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Count/ML-DSA-65/64"u8,
            "5492EB8D811072C030A30CC66B23A173059EBA0D4868CCB92FBE2510B4A5915F"u8,
            "573DCD99C86DAE81F6F80CB00AF40846028EA8F9FE63102FE4A78238BC7B660E"u8,
            "33D2753ED87D0003B44C1AF5F72EB931F559C6B4931AF7E249F65D3FA7613295"u8,
            "84D4AF50933D6E13D4332B86AF0692A66F5030AB01C2EAC4131A5EEBF78CE9E5"u8,
            NewPrivateKey65, NewPublicKey65
        ),
        new(
            "Count/ML-DSA-87/64"u8,
            "B5C07ECEFE9E7C3B885FDEF032BDF9F807B4011E2DFE6806C088D2081631C8EB"u8,
            "5D22F4C40F6EEB96BB891DB15884ED4B0009EA02A24D9D1E9ADFC81C7A42EA7F"u8,
            "D1D5C2D167D6E62906790A5FEDF5A0A754CFAF47E6A11AEB93FB8C41934C31F8"u8,
            "54F0A9CB26F98B394A35918ECA6760EBD10753FC5CDBA8BE508873AD83538131"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Count/ML-DSA-87/65"u8,
            "E8FC3C9FAD711DDA2946334FBBD331468D6E9AB48EB86DCD03F300A17AEBC5E5"u8,
            "B6C4DC9B20CE5D0F445931EE316CF0676E806D1A6A98868881D060EA27CEB139"u8,
            "3B435F7A2CE431C7AB8EAE0991C5DAC610827C99D27803046FBC6C567D6B71F2"u8,
            "E337495F08773F14FB26A3E229B9B26D086644C7FDC300267F9DCDD5D78DB849"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Count/ML-DSA-87/64"u8,
            "151F80886D6CE8C3B428964FE02C40CA0C8EFFA100EE089E54D785344FCCF719"u8,
            "127972C33323FEFBF6B69C19E0C86F41558D9AB2B1A8AD6F39BD0A0245DC8D7E"u8,
            "C628CE94D2AA99AA50CF15B147D4F9A9C62A3D4612152DE0A502C377F472D614"u8,
            "99B552B21432544248BFF47AC8F24CB78DBB25C9683F3ADCB75614BED58A0358"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Count/ML-DSA-87/64"u8,
            "48BEFFB4C97E59E474E1906F39888BE5AE62F6A011C05EF6A6B8D1E54F2171B7"u8,
            "72DA77CF563CBB530129F60129AF989CA4036BA1058267BFBA34A2C70BE803C4"u8,
            "D2756A8FB4E47F796AF704ED0FC8C6E573D42DFAB443B329F00F8DB2FF12C465"u8,
            "E643914B8556D05360C65EB3E7A06BE7C398B82D49973EEFDC711E65B11EB5E8"u8,
            NewPrivateKey87, NewPublicKey87
        ),
        new(
            "Count/ML-DSA-87/69"u8,
            "FE2DA9DD93A077FCB6452AC88D0A5762EB896BAAAC6CE7D01CB1370BA8322390"u8,
            "7422DBE3F476FFE41A4EFB33F3DDFD8B328029BA3050603866C36CFBC2EE4B87"u8,
            "A86B29ADF2300D2636E21D4A350CD18E55A254379C3659A7A95D8734CEC1F005"u8,
            "8D25818DD972FFF5B9E9B4CC534A95100A1340C1C81D1486A68939D340E0A58B"u8,
            NewPrivateKey87, NewPublicKey87
        )
    }.slice();

    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestACVPRejectionKATs_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var seed = fromHex(tcʗ1.seed);
                var (priv, err) = tcʗ1.newPrivateKey(seed);
                if (err != default!) {
                    tΔ1.Fatalf("NewPrivateKey: %v"u8, err);
                }

                if (strings.Contains(tΔ1.Name(), pathˢ)) {
                    // For path coverage tests, check that we hit all rejection paths.
                    var reached = new map<@string, bool>{["z"u8] = false, ["r0"u8] = false, ["ct0"u8] = false, ["h"u8] = false};
                    // The ct0 rejection is only reachable for ML-DSA-44.
                    if (priv.PublicKey().Parameters() != "ML-DSA-44"u8) {
                        delete(reached, "ct0"u8);
                    }
                    var reachedʗ1 = reached;
                    testingOnlyRejectionReason = (@string reason) => {
                        tΔ1.Log(reason, rejectionˢ);
                        reachedʗ1[reason] = true;
                    };
                    tΔ1.Cleanup(() => {
                        testingOnlyRejectionReason = default!;
                    });
                    var reachedʗ2 = reached;
                    defer(() => {
                        foreach (var (reason, hit) in reachedʗ2) {
                            if (!hit) {
                                tΔ1.Errorf("Rejection path %q not hit"u8, reason);
                            }
                        }
                    }, ref ᒐ);
                }

                var pk = priv.PublicKey().Bytes();
                var sk = TestingOnlyPrivateKeySemiExpandedBytes(priv);
                var keyHashGot = sha256.Sum256(appendꓸꓸꓸ(pk, sk));
                var keyHashWant = fromHex(tcʗ1.keyHash);

                if (!bytes.Equal(keyHashGot[..], keyHashWant)) {
                    tΔ1.Errorf("Key hash mismatch:\n  got:  %X\n  want: %X"u8, keyHashGot, keyHashWant);
                }

                (var pub, err) = tcʗ1.newPublicKey(pk);
                if (err != default!) {
                    tΔ1.Fatalf("NewPublicKey: %v"u8, err);
                }
                if (!pub.Equal(priv.PublicKey())) {
                    tΔ1.Errorf("Parsed public key not equal to original"u8);
                }
                if (pub.Value != priv.PublicKey().Value) {
                    tΔ1.Errorf("Parsed public key not identical to original"u8);
                }

                // The table provides a Sign_internal input (not actually formatted
                // like one), which is part of the pre-image of μ.
                var M = fromHex(tcʗ1.msg);
                var H = sha3.NewShake256();
                var tr = computePublicKeyHash(pk);
                H.Write(tr[..]);
                H.Write(M);
                var μ = new slice<byte>(64);
                H.Read(μ);
                tΔ1.Logf("Computed μ: %x"u8, μ);
                (var sig, err) = SignExternalMuDeterministic(priv, μ);
                if (err != default!) {
                    tΔ1.Fatalf("SignExternalMuDeterministic: %v"u8, err);
                }

                var sigHashGot = sha256.Sum256(sig);
                var sigHashWant = fromHex(tcʗ1.sigHash);

                if (!bytes.Equal(sigHashGot[..], sigHashWant)) {
                    tΔ1.Errorf("Signature hash mismatch:\n  got:  %X\n  want: %X"u8, sigHashGot, sigHashWant);
                }

                {
                    var errΔ1 = VerifyExternalMu(priv.PublicKey(), μ, sig); if (errΔ1 != default!) {
                        tΔ1.Errorf("Verify: %v"u8, errΔ1);
                    }
                }
                var wrong = new slice<byte>(len(μ));
                {
                    var errΔ2 = VerifyExternalMu(priv.PublicKey(), wrong, sig); if (errΔ2 == default!) {
                        tΔ1.Errorf("Verify passed on wrong message"u8);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestCASTRejectionPaths(ж<testing.T> Ꮡt) {
    var reached = new map<@string, bool>{["z"u8] = false, ["r0"u8] = false, ["ct0"u8] = false, ["h"u8] = false};
    var reachedʗ1 = reached;
    testingOnlyRejectionReason = (@string reason) => {
        Ꮡt.Log(reason, rejectionˢ);
        reachedʗ1[reason] = true;
    };
    Ꮡt.Cleanup(() => {
        testingOnlyRejectionReason = default!;
    });

    {
        var err = fips140CAST(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }

    foreach (var (reason, hit) in reached) {
        if (!hit) {
            Ꮡt.Errorf("Rejection path %q not hit"u8, reason);
        }
    }
}

public static void BenchmarkCAST(ж<testing.B> Ꮡb) {
    // IG 10.3.A says "ML-DSA digital signature generation CASTs should cover
    // all applicable rejection sampling loop paths". For ML-DSA-44, there are
    // four paths. For ML-DSA-65 and ML-DSA-87, only three. This benchmark helps
    // us figure out which is faster: four rejections of ML-DSA-44, or three of
    // ML-DSA-65. (It's the former, but only barely.)
    Ꮡb.Run(mlDsa44ˢ, (ж<testing.B> bΔ1) => {
        // Same as TestACVPRejectionKATs/Test/Path/ML-DSA-44/1.
        var seed = fromHex("5C624FCC1862452452D0C665840D8237F43108E5499EDCDC108FBC49D596E4B7"u8);
        var μ = fromHex("2ad1c72bb0fcbe28099ce8bd2ed836dfebe520aad38fbac66ef785a3cfb10fb4"u8 + "19327fa57818ee4e3718da4be48d24b59a208f8807271fdb7eda6e60141bd263"u8);
        var skHash = fromHex("29374951cb2bc3cda7315ce7f0ab99c7d2d65292e6c5156e8aa62ac14b1412af"u8);
        var sigHash = fromHex("dcc71a421bc6ffafb7df0c7f6d018a19ada154d1e2ee360ed533cecd5dc980ad"u8);
        while (bΔ1.Loop()) {
            var (priv, err) = NewPrivateKey44(seed);
            if (err != default!) {
                bΔ1.Fatalf("NewPrivateKey: %v"u8, err);
            }
            var sk = TestingOnlyPrivateKeySemiExpandedBytes(priv);
            if (sha256.Sum256(sk) != new array<byte>(skHash, 32)) {
                bΔ1.Fatalf("sk hash mismatch, got %x"u8, sha256.Sum256(sk));
            }
            (var sig, err) = SignExternalMuDeterministic(priv, μ);
            if (err != default!) {
                bΔ1.Fatalf("SignExternalMuDeterministic: %v"u8, err);
            }
            if (sha256.Sum256(sig) != new array<byte>(sigHash, 32)) {
                bΔ1.Fatalf("sig hash mismatch, got %x"u8, sha256.Sum256(sig));
            }
            {
                var errΔ1 = VerifyExternalMu(priv.PublicKey(), μ, sig); if (errΔ1 != default!) {
                    bΔ1.Fatalf("Verify: %v"u8, errΔ1);
                }
            }
        }
    });
    Ꮡb.Run(mlDsa65ˢ, (ж<testing.B> bΔ2) => {
        // Same as TestACVPRejectionKATs/Path/ML-DSA-65/4, which is the only one
        // actually covering all three rejection paths, despite IG 10.3.A
        // pointing explicitly at these vectors for this check. See
        // https://groups.google.com/a/list.nist.gov/g/pqc-forum/c/6U34L4ISYzk/m/hel75x07AQAJ
        var seed = fromHex("F215BA2280D86F142012FC05FFC04F2C7D22FF5DD7D69AA0EFB081E3A53E9318"u8);
        var μ = fromHex("35cdb7dddbed44af4641bac659f46598ed769ea9693fd4ed2152b84c45811d2e"u8 + "66eded1eb20cde1c1f4b82642a330d8e86ac432a2aefaa56cd9b2b5f4affd450"u8);
        var skHash = fromHex("2e6f5ff659310b8ca1457a65d8b448b297a905dc08e06c1246a97daad0af6f7d"u8);
        var sigHash = fromHex("c027d21b21fa75abe7f35cd84a54e2e83bd352140bc8c49eab2c45004e7268a7"u8);
        while (bΔ2.Loop()) {
            var (priv, err) = NewPrivateKey65(seed);
            if (err != default!) {
                bΔ2.Fatalf("NewPrivateKey: %v"u8, err);
            }
            var sk = TestingOnlyPrivateKeySemiExpandedBytes(priv);
            if (sha256.Sum256(sk) != new array<byte>(skHash, 32)) {
                bΔ2.Fatalf("sk hash mismatch, got %x"u8, sha256.Sum256(sk));
            }
            (var sig, err) = SignExternalMuDeterministic(priv, μ);
            if (err != default!) {
                bΔ2.Fatalf("SignExternalMuDeterministic: %v"u8, err);
            }
            if (sha256.Sum256(sig) != new array<byte>(sigHash, 32)) {
                bΔ2.Fatalf("sig hash mismatch, got %x"u8, sha256.Sum256(sig));
            }
            {
                var errΔ1 = VerifyExternalMu(priv.PublicKey(), μ, sig); if (errΔ1 != default!) {
                    bΔ2.Fatalf("Verify: %v"u8, errΔ1);
                }
            }
        }
    });
}

internal static slice<byte> fromHex(@string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

} // end mldsa_internal_test_package
