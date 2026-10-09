// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using asn1 = go.encoding.asn1_package;
using pem = go.encoding.pem_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using cryptobyte_asn1 = vendor.golang.org.x.crypto.cryptobyte.asn1_package;
using go.crypto;
using go.encoding;
using io = io_package;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

internal partial struct TestParseASN1String_tests /*dyn*/ {
    internal @string name;
    internal cryptobyte_asn1.Tag tag;
    internal slice<byte> value;
    internal @string expected;
    internal @string expectedErr;
}

public static void TestParseASN1String(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestParseASN1String_tests[]{
        new(
            name: "T61String"u8,
            tag: cryptobyte_asn1.T61String,
            value: new byte[]{80, 81, 82}.slice(),
            expected: ((@string)"PQR"u8)
        ),
        new(
            name: "PrintableString"u8,
            tag: cryptobyte_asn1.PrintableString,
            value: new byte[]{80, 81, 82}.slice(),
            expected: ((@string)"PQR"u8)
        ),
        new(
            name: "PrintableString (invalid)"u8,
            tag: cryptobyte_asn1.PrintableString,
            value: new byte[]{1, 2, 3}.slice(),
            expectedErr: "invalid PrintableString"u8
        ),
        new(
            name: "UTF8String"u8,
            tag: cryptobyte_asn1.UTF8String,
            value: new byte[]{80, 81, 82}.slice(),
            expected: ((@string)"PQR"u8)
        ),
        new(
            name: "UTF8String (invalid)"u8,
            tag: cryptobyte_asn1.UTF8String,
            value: new byte[]{255}.slice(),
            expectedErr: "invalid UTF-8 string"u8
        ),
        new(
            name: "BMPString"u8,
            tag: ((cryptobyte_asn1.Tag)asn1.TagBMPString),
            value: new byte[]{80, 81}.slice(),
            expected: ((@string)"偑"u8)
        ),
        new(
            name: "BMPString (invalid length)"u8,
            tag: ((cryptobyte_asn1.Tag)asn1.TagBMPString),
            value: new byte[]{255}.slice(),
            expectedErr: "invalid BMPString"u8
        ),
        new(
            name: "IA5String"u8,
            tag: cryptobyte_asn1.IA5String,
            value: new byte[]{80, 81}.slice(),
            expected: ((@string)"PQ"u8)
        ),
        new(
            name: "IA5String (invalid)"u8,
            tag: cryptobyte_asn1.IA5String,
            value: new byte[]{255}.slice(),
            expectedErr: "invalid IA5String"u8
        ),
        new(
            name: "NumericString"u8,
            tag: ((cryptobyte_asn1.Tag)asn1.TagNumericString),
            value: new byte[]{49, 50}.slice(),
            expected: ((@string)"12"u8)
        ),
        new(
            name: "NumericString (invalid)"u8,
            tag: ((cryptobyte_asn1.Tag)asn1.TagNumericString),
            value: new byte[]{80}.slice(),
            expectedErr: "invalid NumericString"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestParseASN1String_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (@out, err) = parseASN1String(tcʗ1.tag, tcʗ1.value);
            if (err != default! && err.Error() != tcʗ1.expectedErr){
                tΔ1.Fatalf("parseASN1String returned unexpected error: got %q, want %q"u8, err, tcʗ1.expectedErr);
            } else 
            if (err == default! && tcʗ1.expectedErr != ""u8) {
                tΔ1.Fatalf("parseASN1String didn't fail, expected: %s"u8, tcʗ1.expectedErr);
            }
            if (@out != tcʗ1.expected) {
                tΔ1.Fatalf("parseASN1String returned unexpected value: got %q, want %q"u8, @out, tcʗ1.expected);
            }
        });
    }
}

internal static readonly @string policyPEM = """
-----BEGIN CERTIFICATE-----
MIIGeDCCBWCgAwIBAgIUED9KQBi0ScBDoufB2mgAJ63G5uIwDQYJKoZIhvcNAQEL
BQAwVTELMAkGA1UEBhMCVVMxGDAWBgNVBAoTD1UuUy4gR292ZXJubWVudDENMAsG
A1UECxMERlBLSTEdMBsGA1UEAxMURmVkZXJhbCBCcmlkZ2UgQ0EgRzQwHhcNMjAx
MDIyMTcwNDE5WhcNMjMxMDIyMTcwNDE5WjCBgTELMAkGA1UEBhMCVVMxHTAbBgNV
BAoTFFN5bWFudGVjIENvcnBvcmF0aW9uMR8wHQYDVQQLExZTeW1hbnRlYyBUcnVz
dCBOZXR3b3JrMTIwMAYDVQQDEylTeW1hbnRlYyBDbGFzcyAzIFNTUCBJbnRlcm1l
ZGlhdGUgQ0EgLSBHMzCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAL2p
75cMpx86sS2aH4r+0o8r+m/KTrPrknWP0RA9Kp6sewAzkNa7BVwg0jOhyamiv1iP
Cns10usoH93nxYbXLWF54vOLRdYU/53KEPNmgkj2ipMaTLuaReBghNibikWSnAmy
S8RItaDMs8tdF2goKPI4xWiamNwqe92VC+pic2tq0Nva3Y4kvMDJjtyje3uduTtL
oyoaaHkrX7i7gE67psnMKj1THUtre1JV1ohl9+oOuyot4p3eSxVlrMWiiwb11bnk
CakecOz/mP2DHMGg6pZ/BeJ+ThaLUylAXECARIqHc9UwRPKC9BfLaCX4edIoeYiB
loRs4KdqLdg/I9eTwKkCAwEAAaOCAxEwggMNMB0GA1UdDgQWBBQ1Jn1QleGhwb0F
1cOdd0LHDBOWjDAfBgNVHSMEGDAWgBR58ABJ6393wl1BAmU0ipAjmx4HbzAOBgNV
HQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zCBiAYDVR0gBIGAMH4wDAYKYIZI
AWUDAgEDAzAMBgpghkgBZQMCAQMMMAwGCmCGSAFlAwIBAw4wDAYKYIZIAWUDAgED
DzAMBgpghkgBZQMCAQMSMAwGCmCGSAFlAwIBAxMwDAYKYIZIAWUDAgEDFDAMBgpg
hkgBZQMCAQMlMAwGCmCGSAFlAwIBAyYwggESBgNVHSEEggEJMIIBBTAbBgpghkgB
ZQMCAQMDBg1ghkgBhvhFAQcXAwEGMBsGCmCGSAFlAwIBAwwGDWCGSAGG+EUBBxcD
AQcwGwYKYIZIAWUDAgEDDgYNYIZIAYb4RQEHFwMBDjAbBgpghkgBZQMCAQMPBg1g
hkgBhvhFAQcXAwEPMBsGCmCGSAFlAwIBAxIGDWCGSAGG+EUBBxcDARIwGwYKYIZI
AWUDAgEDEwYNYIZIAYb4RQEHFwMBETAbBgpghkgBZQMCAQMUBg1ghkgBhvhFAQcX
AwEUMBsGCmCGSAFlAwIBAyUGDWCGSAGG+EUBBxcDAQgwGwYKYIZIAWUDAgEDJgYN
YIZIAYb4RQEHFwMBJDBgBggrBgEFBQcBCwRUMFIwUAYIKwYBBQUHMAWGRGh0dHA6
Ly9zc3Atc2lhLnN5bWF1dGguY29tL1NUTlNTUC9DZXJ0c19Jc3N1ZWRfYnlfQ2xh
c3MzU1NQQ0EtRzMucDdjMA8GA1UdJAQIMAaAAQCBAQAwCgYDVR02BAMCAQAwUQYI
KwYBBQUHAQEERTBDMEEGCCsGAQUFBzAChjVodHRwOi8vcmVwby5mcGtpLmdvdi9i
cmlkZ2UvY2FDZXJ0c0lzc3VlZFRvZmJjYWc0LnA3YzA3BgNVHR8EMDAuMCygKqAo
hiZodHRwOi8vcmVwby5mcGtpLmdvdi9icmlkZ2UvZmJjYWc0LmNybDANBgkqhkiG
9w0BAQsFAAOCAQEAA751TycC1f/WTkHmedF9ZWxP58Jstmwvkyo8bKueJ0eF7LTG
BgQlzE2B9vke4sFhd4V+BdgOPGE1dsGzllYKCWg0BhkCBs5kIJ7F6Ay6G1TBuGU1
Ie8247GL+P9pcC5TVvXHC/62R2w3DuD/vAPLbYEbSQjobXlsqt8Kmtd6yK/jVuDV
BTZMdZmvoNtjemqmgcBXHsf0ctVm0m6tH5uYqyVxu8tfyUis6Cf303PHj+spWP1k
gc5PYnVF0ot7qAmNFENIpbKg3BdusBkF9rGxLaDSUBvSc7+s9iQz9d/iRuAebrYu
+eqUlJ2lsjS1U8qyPmlH+spfPNbAEQEsuP32Aw==
-----END CERTIFICATE-----

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedˢ = (@string)"expected RequireExplicitPolicyZero to be set"u8;
internal static readonly object expectedˢ2 = (@string)"expected InhibitPolicyMappingZero to be set"u8;
internal static readonly object expectedˢ3 = (@string)"expected InhibitAnyPolicyZero to be set"u8;

public static void TestPolicyParse(ж<testing.T> Ꮡt) {
    var (b, _) = pem.Decode(slice<byte>(policyPEM));
    var (c, err) = ParseCertificate((~b).Bytes);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (builtin.len((~c).Policies) != 9) {
        Ꮡt.Errorf("unexpected number of policies: got %d, want %d"u8, builtin.len((~c).Policies), (nint)(9));
    }
    if (builtin.len((~c).PolicyMappings) != 9) {
        Ꮡt.Errorf("unexpected number of policy mappings: got %d, want %d"u8, builtin.len((~c).PolicyMappings), (nint)(9));
    }
    if (!(~c).RequireExplicitPolicyZero) {
        Ꮡt.Error(expectedˢ);
    }
    if (!(~c).InhibitPolicyMappingZero) {
        Ꮡt.Error(expectedˢ2);
    }
    if (!(~c).InhibitAnyPolicyZero) {
        Ꮡt.Error(expectedˢ3);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parsingShouldVeFailedˢ = (@string)"parsing should've failed"u8;

public static void TestParsePolicies(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in new @string[]{
        "testdata/policy_leaf_duplicate.pem"u8,
        "testdata/policy_leaf_invalid.pem"u8
    }.slice()) {
        Ꮡt.Run(tc, (ж<testing.T> tΔ1) => {
            var (b, err) = os.ReadFile(tc);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var (p, _) = pem.Decode(b);
            (_, err) = ParseCertificate((~p).Bytes);
            if (err == default!) {
                tΔ1.Error(parsingShouldVeFailedˢ);
            }
        });
    }
}

internal partial struct TestDomainNameValid_type /*dyn*/ {
    internal @string name;
    internal @string dnsName;
    internal bool constraint;
    internal bool valid;
}

public static void TestDomainNameValid(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in new TestDomainNameValid_type[]{ // TODO(#75835): these tests are for stricter name validation, which we
 // had to disable. Once we reenable these strict checks, behind a
 // GODEBUG, we should add them back in.
 // {"empty name, name", "", false, false},
 // {"254 char label, name", strings.Repeat("a.a", 84) + "aaa", false, false},
 // {"254 char label, constraint", strings.Repeat("a.a", 84) + "aaa", true, false},
 // {"253 char label, name", strings.Repeat("a.a", 84) + "aa", false, false},
 // {"253 char label, constraint", strings.Repeat("a.a", 84) + "aa", true, false},
 // {"64 char single label, name", strings.Repeat("a", 64), false, false},
 // {"64 char single label, constraint", strings.Repeat("a", 64), true, false},
 // {"64 char label, name", "a." + strings.Repeat("a", 64), false, false},
 // {"64 char label, constraint", "a." + strings.Repeat("a", 64), true, false},
 // TODO(#75835): these are the inverse of the tests above, they should be removed
 // once the strict checking is enabled.

        new("254 char label, name"u8, strings.Repeat("a.a"u8, 84) + "aaa"u8, false, true),
        new("254 char label, constraint"u8, strings.Repeat("a.a"u8, 84) + "aaa"u8, true, true),
        new("253 char label, name"u8, strings.Repeat("a.a"u8, 84) + "aa"u8, false, true),
        new("253 char label, constraint"u8, strings.Repeat("a.a"u8, 84) + "aa"u8, true, true),
        new("64 char single label, name"u8, strings.Repeat("a"u8, 64), false, true),
        new("64 char single label, constraint"u8, strings.Repeat("a"u8, 64), true, true),
        new("64 char label, name"u8, "a."u8 + strings.Repeat("a"u8, 64), false, true),
        new("64 char label, constraint"u8, "a."u8 + strings.Repeat("a"u8, 64), true, true), // Check we properly enforce properties of domain names.

        new("empty name, constraint"u8, ""u8, true, true),
        new("empty label, name"u8, "a..a"u8, false, false),
        new("empty label, constraint"u8, "a..a"u8, true, false),
        new("period, name"u8, "."u8, false, false),
        new("period, constraint"u8, "."u8, true, false), // TODO(roland): not entirely clear if this is a valid constraint (require at least one label?)

        new("valid, name"u8, "a.b.c"u8, false, true),
        new("valid, constraint"u8, "a.b.c"u8, true, true),
        new("leading period, name"u8, ".a.b.c"u8, false, false),
        new("leading period, constraint"u8, ".a.b.c"u8, true, true),
        new("trailing period, name"u8, "a."u8, false, false),
        new("trailing period, constraint"u8, "a."u8, true, false),
        new("bare label, name"u8, "a"u8, false, true),
        new("bare label, constraint"u8, "a"u8, true, true),
        new("63 char single label, name"u8, strings.Repeat("a"u8, 63), false, true),
        new("63 char single label, constraint"u8, strings.Repeat("a"u8, 63), true, true),
        new("63 char label, name"u8, "a."u8 + strings.Repeat("a"u8, 63), false, true),
        new("63 char label, constraint"u8, "a."u8 + strings.Repeat("a"u8, 63), true, true)
    }.slice()) {
        ref var tc = ref heap(new TestDomainNameValid_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var valid = domainNameValid(tcʗ1.dnsName, tcʗ1.constraint);
            if (tcʗ1.valid != valid) {
                tΔ1.Errorf("domainNameValid(%q, %t) = %v; want %v"u8, tcʗ1.dnsName, tcʗ1.constraint, !tcʗ1.valid, tcʗ1.valid);
            }
            // Also check that we enforce the same properties as domainToReverseLabels
            @string trimmedName = tcʗ1.dnsName;
            if (tcʗ1.constraint && builtin.len(trimmedName) > 1 && trimmedName[0] == (rune)'.') {
                trimmedName = trimmedName[1..];
            }
            var (_, revValid) = domainToReverseLabels(trimmedName);
            if (valid != revValid) {
                tΔ1.Errorf("domainNameValid(%q, %t) = %t != domainToReverseLabels(%q) = %t"u8, tcʗ1.dnsName, tcʗ1.constraint, valid, trimmedName, revValid);
            }
        });
    }
}

public static void TestRoundtripWeirdSANs(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // TODO(#75835): check that certificates we create with CreateCertificate that have malformed SAN values
    // can be parsed by ParseCertificate. We should eventually restrict this, but for now we have to maintain
    // this property as people have been relying on it.
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var badNames = new @string[]{
        "baredomain"u8,
        "baredomain."u8,
        strings.Repeat("a"u8, 255),
        strings.Repeat("a"u8, 65) + ".com"u8
    }.slice();
    var tmpl = Ꮡ(new Certificate(
        EmailAddresses: badNames,
        DNSNames: badNames
    ));
    (var b, err) = CreateCertificate(rand.Reader, tmpl, tmpl, k.of(ecdsa.PrivateKey.ᏑPublicKey), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = ParseCertificate(b);
    if (err != default!) {
        Ꮡt.Fatalf("Couldn't roundtrip certificate: %v"u8, err);
    }
}

public static void FuzzDomainNameValid(ж<testing.F> Ꮡf) {
    Ꮡf.Fuzz((ж<testing.T> t, @string data) => {
        domainNameValid(data, false);
        domainNameValid(data, true);
    });
}

} // end x509_internal_test_package
