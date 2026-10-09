// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using dsa = go.crypto.dsa_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = go.encoding.asn1_package;
using pem = go.encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using log = log_package;
using big = go.math.big_package;
using net = net_package;
using os = os_package;
using exec = go.os.exec_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using go.crypto;
using go.crypto.x509;
using go.encoding;
using go.math;
using go.os;
using io = io_package;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

internal partial struct verifyTest {
    internal @string name;
    internal @string leaf;
    internal slice<@string> intermediates;
    internal slice<@string> roots;
    internal int64 currentTime;
    internal @string dnsName;
    internal bool systemSkip;
    internal bool systemLax;
    internal slice<global::go.crypto.x509_package.ExtKeyUsage> keyUsages;
    internal Action<ж<testing.T>, error> errorCallback;
    internal slice<slice<@string>> expectedChains;
}

// does not chain to a system root
// does not chain to a system root
// Skip when using systemVerify, since Windows
// *will* find the missing intermediate cert.
// CAPI doesn't build the chain with the duplicated GeoTrust
// entry so the results don't match.
// The specific error message may not occur when using system
// verification.
// EKULeaf tests use an unconstrained chain leading to a leaf certificate
// with an E-mail Protection EKU but not a Server Auth one, checking that
// the EKUs on the leaf are enforced.
// Check that a name constrained intermediate works even when
// it lists multiple constraints.
// Check that SHA-384 intermediates (which are popping up)
// work.
// CryptoAPI can find alternative validation paths.
// Putting a certificate as a root directly should work as a
// way of saying “exactly this”.
// does not chain to a system root
// Putting a certificate as a root directly should not skip
// other checks however.
// does not chain to a system root
// An X.509 v1 certificate should not be accepted as an
// intermediate.
// does not chain to a system root
// does not chain to a system root
// Test that excluded names are respected.
// does not chain to a system root
// Test that unknown critical extensions in a leaf cause a
// verify error.
// does not chain to a system root
// Test that unknown critical extensions in an intermediate
// cause a verify error.
// does not chain to a system root
// does not chain to a system root
// A certificate with an AKID should still chain to a parent without SKID.
// See Issue 30079.
// does not chain to a system root
// When there are two parents, one with an incorrect subject but matching SKID
// and one with a correct subject but missing SKID, the latter should be
// considered as a possible parent.
internal static slice<verifyTest> verifyTests;
internal static void initᴛverifyTests() { verifyTests = new verifyTest[]{
    new(
        name: "Valid"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "www.google.com"u8,
        expectedChains: new slice<@string>[]{
            new @string[]{"www.google.com"u8, "GTS CA 1C3"u8, "GTS Root R1"u8}.slice()
        }.slice()
    ),
    new(
        name: "Valid (fqdn)"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "www.google.com."u8,
        expectedChains: new slice<@string>[]{
            new @string[]{"www.google.com"u8, "GTS CA 1C3"u8, "GTS Root R1"u8}.slice()
        }.slice()
    ),
    new(
        name: "MixedCase"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "WwW.GooGLE.coM"u8,
        expectedChains: new slice<@string>[]{
            new @string[]{"www.google.com"u8, "GTS CA 1C3"u8, "GTS Root R1"u8}.slice()
        }.slice()
    ),
    new(
        name: "HostnameMismatch"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "www.example.com"u8,
        errorCallback: expectHostnameError("certificate is valid for"u8)
    ),
    new(
        name: "TooManyDNS"u8,
        leaf: generatePEMCertWithRepeatSAN(1677615892, 200, "fake.dns"u8),
        roots: new @string[]{generatePEMCertWithRepeatSAN(1677615892, 200, "fake.dns"u8)}.slice(),
        currentTime: 1677615892,
        dnsName: "www.example.com"u8,
        systemSkip: true,
        errorCallback: expectHostnameError("certificate is valid for 200 names, but none matched"u8)
    ),
    new(
        name: "TooManyIPs"u8,
        leaf: generatePEMCertWithRepeatSAN(1677615892, 150, "4.3.2.1"u8),
        roots: new @string[]{generatePEMCertWithRepeatSAN(1677615892, 150, "4.3.2.1"u8)}.slice(),
        currentTime: 1677615892,
        dnsName: "1.2.3.4"u8,
        systemSkip: true,
        errorCallback: expectHostnameError("certificate is valid for 150 IP SANs, but none matched"u8)
    ),
    new(
        name: "IPMissing"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "1.2.3.4"u8,
        errorCallback: expectHostnameError("doesn't contain any IP SANs"u8)
    ),
    new(
        name: "Expired"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1,
        dnsName: "www.example.com"u8,
        errorCallback: expectExpired
    ),
    new(
        name: "MissingIntermediate"u8,
        leaf: googleLeaf,
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "www.google.com"u8,
        systemSkip: true,
        errorCallback: expectAuthorityUnknown
    ),
    new(
        name: "RootInIntermediates"u8,
        leaf: googleLeaf,
        intermediates: new @string[]{gtsRoot, gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "www.google.com"u8,
        expectedChains: new slice<@string>[]{
            new @string[]{"www.google.com"u8, "GTS CA 1C3"u8, "GTS Root R1"u8}.slice()
        }.slice(),
        systemLax: true
    ),
    new(
        name: "InvalidHash"u8,
        leaf: googleLeafWithInvalidHash,
        intermediates: new @string[]{gtsIntermediate}.slice(),
        roots: new @string[]{gtsRoot}.slice(),
        currentTime: 1677615892,
        dnsName: "www.google.com"u8,
        systemLax: true,
        errorCallback: expectHashError
    ),
    new(
        name: "EKULeaf"u8,
        leaf: smimeLeaf,
        intermediates: new @string[]{smimeIntermediate}.slice(),
        roots: new @string[]{smimeRoot}.slice(),
        currentTime: 1594673418,
        errorCallback: expectUsageError
    ),
    new(
        name: "EKULeafExplicit"u8,
        leaf: smimeLeaf,
        intermediates: new @string[]{smimeIntermediate}.slice(),
        roots: new @string[]{smimeRoot}.slice(),
        currentTime: 1594673418,
        keyUsages: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
        errorCallback: expectUsageError
    ),
    new(
        name: "EKULeafValid"u8,
        leaf: smimeLeaf,
        intermediates: new @string[]{smimeIntermediate}.slice(),
        roots: new @string[]{smimeRoot}.slice(),
        currentTime: 1594673418,
        keyUsages: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageEmailProtection}.slice(),
        expectedChains: new slice<@string>[]{
            new @string[]{"CORPORATIVO FICTICIO ACTIVO"u8, "EAEko Herri Administrazioen CA - CA AAPP Vascas (2)"u8, "IZENPE S.A."u8}.slice()
        }.slice()
    ),
    new(
        name: "MultipleConstraints"u8,
        leaf: nameConstraintsLeaf,
        intermediates: new @string[]{nameConstraintsIntermediate1, nameConstraintsIntermediate2}.slice(),
        roots: new @string[]{globalSignRoot}.slice(),
        currentTime: 1524771953,
        dnsName: "udctest.ads.vt.edu"u8,
        expectedChains: new slice<@string>[]{
            new @string[]{
                "udctest.ads.vt.edu"u8,
                "Virginia Tech Global Qualified Server CA"u8,
                "Trusted Root CA SHA256 G2"u8,
                "GlobalSign"u8}.slice()
        }.slice()
    ),
    new(
        name: "SHA-384"u8,
        leaf: trustAsiaLeaf,
        intermediates: new @string[]{trustAsiaSHA384Intermediate}.slice(),
        roots: new @string[]{digicertRoot}.slice(),
        currentTime: 1558051200,
        dnsName: "tm.cn"u8,
        systemLax: true,
        expectedChains: new slice<@string>[]{
            new @string[]{
                "tm.cn"u8,
                "TrustAsia ECC OV TLS Pro CA"u8,
                "DigiCert Global Root CA"u8}.slice()
        }.slice()
    ),
    new(
        name: "LeafInRoots"u8,
        leaf: selfSigned,
        roots: new @string[]{selfSigned}.slice(),
        currentTime: 1471624472,
        dnsName: "foo.example"u8,
        systemSkip: true,
        expectedChains: new slice<@string>[]{
            new @string[]{"Acme Co"u8}.slice()
        }.slice()
    ),
    new(
        name: "LeafInRootsInvalid"u8,
        leaf: selfSigned,
        roots: new @string[]{selfSigned}.slice(),
        currentTime: 1471624472,
        dnsName: "notfoo.example"u8,
        systemSkip: true,
        errorCallback: expectHostnameError("certificate is valid for"u8)
    ),
    new(
        name: "X509v1Intermediate"u8,
        leaf: x509v1TestLeaf,
        intermediates: new @string[]{x509v1TestIntermediate}.slice(),
        roots: new @string[]{x509v1TestRoot}.slice(),
        currentTime: 1481753183,
        systemSkip: true,
        errorCallback: expectNotAuthorizedError
    ),
    new(
        name: "IgnoreCNWithSANs"u8,
        leaf: ignoreCNWithSANLeaf,
        dnsName: "foo.example.com"u8,
        roots: new @string[]{ignoreCNWithSANRoot}.slice(),
        currentTime: 1486684488,
        systemSkip: true,
        errorCallback: expectHostnameError("certificate is not valid for any names"u8)
    ),
    new(
        name: "ExcludedNames"u8,
        leaf: excludedNamesLeaf,
        dnsName: "bender.local"u8,
        intermediates: new @string[]{excludedNamesIntermediate}.slice(),
        roots: new @string[]{excludedNamesRoot}.slice(),
        currentTime: 1486684488,
        systemSkip: true,
        errorCallback: expectNameConstraintsError
    ),
    new(
        name: "CriticalExtLeaf"u8,
        leaf: criticalExtLeafWithExt,
        intermediates: new @string[]{criticalExtIntermediate}.slice(),
        roots: new @string[]{criticalExtRoot}.slice(),
        currentTime: 1486684488,
        systemSkip: true,
        errorCallback: expectUnhandledCriticalExtension
    ),
    new(
        name: "CriticalExtIntermediate"u8,
        leaf: criticalExtLeaf,
        intermediates: new @string[]{criticalExtIntermediateWithExt}.slice(),
        roots: new @string[]{criticalExtRoot}.slice(),
        currentTime: 1486684488,
        systemSkip: true,
        errorCallback: expectUnhandledCriticalExtension
    ),
    new(
        name: "ValidCN"u8,
        leaf: validCNWithoutSAN,
        dnsName: "foo.example.com"u8,
        roots: new @string[]{invalidCNRoot}.slice(),
        currentTime: 1540000000,
        systemSkip: true,
        errorCallback: expectHostnameError("certificate relies on legacy Common Name field"u8)
    ),
    new(
        name: "AKIDNoSKID"u8,
        leaf: leafWithAKID,
        roots: new @string[]{rootWithoutSKID}.slice(),
        currentTime: 1550000000,
        dnsName: "example"u8,
        systemSkip: true,
        expectedChains: new slice<@string>[]{
            new @string[]{"Acme LLC"u8, "Acme Co"u8}.slice()
        }.slice()
    ),
    new(
        leaf: leafMatchingAKIDMatchingIssuer,
        roots: new @string[]{rootMatchingSKIDMismatchingSubject, rootMismatchingSKIDMatchingSubject}.slice(),
        currentTime: 1550000000,
        dnsName: "example"u8,
        systemSkip: true,
        expectedChains: new slice<@string>[]{
            new @string[]{"Leaf"u8, "Root B"u8}.slice()
        }.slice()
    )
}.slice(); }

internal static Action<ж<testing.T>, error> expectHostnameError(@string msg) {
    return (ж<testing.T> t, error err) => {
        {
            var (_, ok) = err._<HostnameError>(ᐧ); if (!ok) {
                t.Fatalf("error was not a HostnameError: %v"u8, err);
            }
        }
        if (!strings.Contains(err.Error(), msg)) {
            t.Fatalf("HostnameError did not contain %q: %v"u8, msg, err);
        }
    };
}

internal static void expectExpired(ж<testing.T> Ꮡt, error err) {
    {
        var (inval, ok) = err._<CertificateInvalidError>(ᐧ); if (!ok || inval.Reason != Expired) {
            Ꮡt.Fatalf("error was not Expired: %v"u8, err);
        }
    }
}

internal static void expectUsageError(ж<testing.T> Ꮡt, error err) {
    {
        var (inval, ok) = err._<CertificateInvalidError>(ᐧ); if (!ok || inval.Reason != IncompatibleUsage) {
            Ꮡt.Fatalf("error was not IncompatibleUsage: %v"u8, err);
        }
    }
}

internal static void expectAuthorityUnknown(ж<testing.T> Ꮡt, error err) {
    var (e, ok) = err._<UnknownAuthorityError>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("error was not UnknownAuthorityError: %v"u8, err);
    }
    if (e.Cert == nil) {
        Ꮡt.Fatalf("error was UnknownAuthorityError, but missing Cert: %v"u8, err);
    }
}

internal static void expectHashError(ж<testing.T> Ꮡt, error err) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (err == default!) {
        Ꮡt.Fatalf("no error resulted from invalid hash"u8);
    }
    {
        @string expected = "algorithm unimplemented"u8; if (!strings.Contains(err.Error(), expected)) {
            Ꮡt.Fatalf("error resulting from invalid hash didn't contain '%s', rather it was: %v"u8, expected, err);
        }
    }
}

internal static void expectNameConstraintsError(ж<testing.T> Ꮡt, error err) {
    {
        var (inval, ok) = err._<CertificateInvalidError>(ᐧ); if (!ok || inval.Reason != CANotAuthorizedForThisName) {
            Ꮡt.Fatalf("error was not a CANotAuthorizedForThisName: %v"u8, err);
        }
    }
}

internal static void expectNotAuthorizedError(ж<testing.T> Ꮡt, error err) {
    {
        var (inval, ok) = err._<CertificateInvalidError>(ᐧ); if (!ok || inval.Reason != NotAuthorizedToSign) {
            Ꮡt.Fatalf("error was not a NotAuthorizedToSign: %v"u8, err);
        }
    }
}

internal static void expectUnhandledCriticalExtension(ж<testing.T> Ꮡt, error err) {
    {
        var (_, ok) = err._<UnhandledCriticalExtension>(ᐧ); if (!ok) {
            Ꮡt.Fatalf("error was not an UnhandledCriticalExtension: %v"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedToDecodePemˢ = "failed to decode PEM"u8;

internal static (ж<global::go.crypto.x509_package.Certificate>, error) certificateFromPEM(@string pemBytes) {
    var (block, _) = pem.Decode(slice<byte>(pemBytes));
    if (block == nil) {
        return (default!, errors.New(failedToDecodePemˢ));
    }
    return ParseCertificate((~block).Bytes);
}

internal static void testVerify(ж<testing.T> Ꮡt, verifyTest test, bool useSystemRoots) {
    ref var t = ref Ꮡt.DerefOrNull();

    var opts = new VerifyOptions(
        Intermediates: NewCertPool(),
        DNSName: test.dnsName,
        CurrentTime: time.Unix(test.currentTime, 0),
        KeyUsages: test.keyUsages
    );
    if (!useSystemRoots) {
        opts.Roots = NewCertPool();
        foreach (var (j, root) in test.roots) {
            var ok = opts.Roots.AppendCertsFromPEM(slice<byte>(root));
            if (!ok) {
                Ꮡt.Fatalf("failed to parse root #%d"u8, j);
            }
        }
    }
    foreach (var (j, intermediate) in test.intermediates) {
        var ok = opts.Intermediates.AppendCertsFromPEM(slice<byte>(intermediate));
        if (!ok) {
            Ꮡt.Fatalf("failed to parse intermediate #%d"u8, j);
        }
    }
    var (leaf, err) = certificateFromPEM(test.leaf);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse leaf: %v"u8, err);
    }
    (var chains, err) = leaf.Verify(opts);
    if (test.errorCallback == default! && err != default!) {
        if (runtime.GOOS == "windows"u8 && strings.HasSuffix(testenv.Builder(), "-2008"u8) && err.Error() == "x509: certificate signed by unknown authority"u8) {
            testenv.SkipFlaky(new x509_test_package.testing_TжTB(Ꮡt), 19564);
        }
        Ꮡt.Fatalf("unexpected error: %v"u8, err);
    }
    if (test.errorCallback != default!) {
        if (useSystemRoots && test.systemLax){
            if (err == default!) {
                Ꮡt.Fatalf("expected error"u8);
            }
        } else {
            test.errorCallback(Ꮡt, err);
        }
    }
    bool doesMatch(slice<@string> expectedChain, slice<ж<global::go.crypto.x509_package.Certificate>> chain) {
        if (builtin.len(chain) != builtin.len(expectedChain)) {
            return false;
        }
        foreach (var (k, cert) in chain) {
            if (!strings.Contains(nameToKey(cert.of(global::go.crypto.x509_package.Certificate.ᏑSubject)), expectedChain[k])) {
                return false;
            }
        }
        return true;
    }
    // Every expected chain should match one (or more) returned chain. We tolerate multiple
    // matches, as due to root store semantics it is plausible that (at least on the system
    // verifiers) multiple identical (looking) chains may be returned when two roots with the
    // same subject are present.
    foreach (var (_, expectedChain) in test.expectedChains) {
        bool match = default!;
        foreach (var (_, chain) in chains) {
            if (doesMatch(expectedChain, chain)) {
                match = true;
                break;
            }
        }
        if (!match) {
            Ꮡt.Errorf("No match found for %v"u8, expectedChain);
        }
    }
    // Every returned chain should match 1 expected chain (or <2 if testing against the system)
    foreach (var (_, chain) in chains) {
        nint nMatched = 0;
        foreach (var (_, expectedChain) in test.expectedChains) {
            if (doesMatch(expectedChain, chain)) {
                nMatched++;
            }
        }
        // Allow additional unknown chains if systemLax is set
        if (nMatched == 0 && test.systemLax == false || nMatched > 1) {
            Ꮡt.Errorf("Got %v matches for chain %v"u8, nMatched, chainToDebugString(chain));
            foreach (var (_, expectedChain) in test.expectedChains) {
                if (doesMatch(expectedChain, chain)) {
                    Ꮡt.Errorf("\t matched %v"u8, expectedChain);
                }
            }
        }
    }
}

public static void TestGoVerify(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in verifyTests) {
        ref var test = ref heap(new verifyTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            testVerify(tΔ1, testʗ1, false);
        });
    }
}

public static void TestSystemVerify(ж<testing.T> Ꮡt) {
    if (runtime.GOOS != "windows"u8) {
        Ꮡt.Skipf("skipping verify test using system APIs on %q"u8, runtime.GOOS);
    }
    foreach (var (_, vᴛ1) in verifyTests) {
        ref var test = ref heap(new verifyTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            if (testʗ1.systemSkip) {
                tΔ1.SkipNow();
            }
            testVerify(tΔ1, testʗ1, true);
        });
    }
}

internal static @string chainToDebugString(slice<ж<global::go.crypto.x509_package.Certificate>> chain) {
    @string chainStr = default!;
    foreach (var (_, cert) in chain) {
        if (builtin.len(chainStr) > 0) {
            chainStr += " -> "u8;
        }
        chainStr += nameToKey(cert.of(global::go.crypto.x509_package.Certificate.ᏑSubject));
    }
    return chainStr;
}

internal static @string nameToKey(ж<pkix.Name> Ꮡname) {
    ref var name = ref Ꮡname.DerefOrNull();

    return strings.Join(name.Country, ","u8) + "/"u8 + strings.Join(name.Organization, ","u8) + "/"u8 + strings.Join(name.OrganizationalUnit, ","u8) + "/"u8 + name.CommonName;
}

internal static @string generatePEMCertWithRepeatSAN(int64 currentTime, nint count, @string san) {
    ref var cert = ref heap<global::go.crypto.x509_package.Certificate>(out var Ꮡcert);
    cert = new Certificate(
        NotBefore: time.Unix(currentTime, 0),
        NotAfter: time.Unix(currentTime, 0)
    );
    {
        var ip = net.ParseIP(san); if (ip != default!){
            cert.IPAddresses = slices.Repeat<slice<net.IP>, net.IP>(new net.IP[]{ip}.slice(), count);
        } else {
            cert.DNSNames = slices.Repeat<slice<@string>, @string>(new @string[]{san}.slice(), count);
        }
    }
    var (privKey, err) = rsa.GenerateKey(rand.Reader, 4096);
    if (err != default!) {
        log.Fatal(err);
    }
    (var certBytes, err) = CreateCertificate(rand.Reader, Ꮡcert, Ꮡcert, privKey.of(rsa.PrivateKey.ᏑPublicKey), privKey.OrTypedNil());
    if (err != default!) {
        log.Fatal(err);
    }
    return ((@string)pem.EncodeToMemory(Ꮡ(new pem.Block(
        Type: "CERTIFICATE"u8,
        Bytes: certBytes
    ))));
}

internal static readonly @string gtsIntermediate = """
-----BEGIN CERTIFICATE-----
MIIFljCCA36gAwIBAgINAgO8U1lrNMcY9QFQZjANBgkqhkiG9w0BAQsFADBHMQsw
CQYDVQQGEwJVUzEiMCAGA1UEChMZR29vZ2xlIFRydXN0IFNlcnZpY2VzIExMQzEU
MBIGA1UEAxMLR1RTIFJvb3QgUjEwHhcNMjAwODEzMDAwMDQyWhcNMjcwOTMwMDAw
MDQyWjBGMQswCQYDVQQGEwJVUzEiMCAGA1UEChMZR29vZ2xlIFRydXN0IFNlcnZp
Y2VzIExMQzETMBEGA1UEAxMKR1RTIENBIDFDMzCCASIwDQYJKoZIhvcNAQEBBQAD
ggEPADCCAQoCggEBAPWI3+dijB43+DdCkH9sh9D7ZYIl/ejLa6T/belaI+KZ9hzp
kgOZE3wJCor6QtZeViSqejOEH9Hpabu5dOxXTGZok3c3VVP+ORBNtzS7XyV3NzsX
lOo85Z3VvMO0Q+sup0fvsEQRY9i0QYXdQTBIkxu/t/bgRQIh4JZCF8/ZK2VWNAcm
BA2o/X3KLu/qSHw3TT8An4Pf73WELnlXXPxXbhqW//yMmqaZviXZf5YsBvcRKgKA
gOtjGDxQSYflispfGStZloEAoPtR28p3CwvJlk/vcEnHXG0g/Zm0tOLKLnf9LdwL
tmsTDIwZKxeWmLnwi/agJ7u2441Rj72ux5uxiZ0CAwEAAaOCAYAwggF8MA4GA1Ud
DwEB/wQEAwIBhjAdBgNVHSUEFjAUBggrBgEFBQcDAQYIKwYBBQUHAwIwEgYDVR0T
AQH/BAgwBgEB/wIBADAdBgNVHQ4EFgQUinR/r4XN7pXNPZzQ4kYU83E1HScwHwYD
VR0jBBgwFoAU5K8rJnEaK0gnhS9SZizv8IkTcT4waAYIKwYBBQUHAQEEXDBaMCYG
CCsGAQUFBzABhhpodHRwOi8vb2NzcC5wa2kuZ29vZy9ndHNyMTAwBggrBgEFBQcw
AoYkaHR0cDovL3BraS5nb29nL3JlcG8vY2VydHMvZ3RzcjEuZGVyMDQGA1UdHwQt
MCswKaAnoCWGI2h0dHA6Ly9jcmwucGtpLmdvb2cvZ3RzcjEvZ3RzcjEuY3JsMFcG
A1UdIARQME4wOAYKKwYBBAHWeQIFAzAqMCgGCCsGAQUFBwIBFhxodHRwczovL3Br
aS5nb29nL3JlcG9zaXRvcnkvMAgGBmeBDAECATAIBgZngQwBAgIwDQYJKoZIhvcN
AQELBQADggIBAIl9rCBcDDy+mqhXlRu0rvqrpXJxtDaV/d9AEQNMwkYUuxQkq/BQ
cSLbrcRuf8/xam/IgxvYzolfh2yHuKkMo5uhYpSTld9brmYZCwKWnvy15xBpPnrL
RklfRuFBsdeYTWU0AIAaP0+fbH9JAIFTQaSSIYKCGvGjRFsqUBITTcFTNvNCCK9U
+o53UxtkOCcXCb1YyRt8OS1b887U7ZfbFAO/CVMkH8IMBHmYJvJh8VNS/UKMG2Yr
PxWhu//2m+OBmgEGcYk1KCTd4b3rGS3hSMs9WYNRtHTGnXzGsYZbr8w0xNPM1IER
lQCh9BIiAfq0g3GvjLeMcySsN1PCAJA/Ef5c7TaUEDu9Ka7ixzpiO2xj2YC/WXGs
Yye5TBeg2vZzFb8q3o/zpWwygTMD0IZRcZk0upONXbVRWPeyk+gB9lm+cZv9TSjO
z23HFtz30dZGm6fKa+l3D/2gthsjgx0QGtkJAITgRNOidSOzNIb2ILCkXhAd4FJG
AJ2xDx8hcFH1mt0G/FX0Kw4zd8NLQsLxdxP8c4CU6x+7Nz/OAipmsHMdMqUybDKw
juDEI/9bfU1lcKwrmz3O2+BtjjKAvpafkmO8l7tdufThcV4q5O8DIrGKZTqPwJNl
1IXNDw9bg1kWRxYtnCQ6yICmJhSFm/Y3m6xv+cXDBlHz4n/FsRC6UfTd
-----END CERTIFICATE-----
"""u8;

internal static readonly @string gtsRoot = """
-----BEGIN CERTIFICATE-----
MIIFVzCCAz+gAwIBAgINAgPlk28xsBNJiGuiFzANBgkqhkiG9w0BAQwFADBHMQsw
CQYDVQQGEwJVUzEiMCAGA1UEChMZR29vZ2xlIFRydXN0IFNlcnZpY2VzIExMQzEU
MBIGA1UEAxMLR1RTIFJvb3QgUjEwHhcNMTYwNjIyMDAwMDAwWhcNMzYwNjIyMDAw
MDAwWjBHMQswCQYDVQQGEwJVUzEiMCAGA1UEChMZR29vZ2xlIFRydXN0IFNlcnZp
Y2VzIExMQzEUMBIGA1UEAxMLR1RTIFJvb3QgUjEwggIiMA0GCSqGSIb3DQEBAQUA
A4ICDwAwggIKAoICAQC2EQKLHuOhd5s73L+UPreVp0A8of2C+X0yBoJx9vaMf/vo
27xqLpeXo4xL+Sv2sfnOhB2x+cWX3u+58qPpvBKJXqeqUqv4IyfLpLGcY9vXmX7w
Cl7raKb0xlpHDU0QM+NOsROjyBhsS+z8CZDfnWQpJSMHobTSPS5g4M/SCYe7zUjw
TcLCeoiKu7rPWRnWr4+wB7CeMfGCwcDfLqZtbBkOtdh+JhpFAz2weaSUKK0Pfybl
qAj+lug8aJRT7oM6iCsVlgmy4HqMLnXWnOunVmSPlk9orj2XwoSPwLxAwAtcvfaH
szVsrBhQf4TgTM2S0yDpM7xSma8ytSmzJSq0SPly4cpk9+aCEI3oncKKiPo4Zor8
Y/kB+Xj9e1x3+naH+uzfsQ55lVe0vSbv1gHR6xYKu44LtcXFilWr06zqkUspzBmk
MiVOKvFlRNACzqrOSbTqn3yDsEB750Orp2yjj32JgfpMpf/VjsPOS+C12LOORc92
wO1AK/1TD7Cn1TsNsYqiA94xrcx36m97PtbfkSIS5r762DL8EGMUUXLeXdYWk70p
aDPvOmbsB4om3xPXV2V4J95eSRQAogB/mqghtqmxlbCluQ0WEdrHbEg8QOB+DVrN
VjzRlwW5y0vtOUucxD/SVRNuJLDWcfr0wbrM7Rv1/oFB2ACYPTrIrnqYNxgFlQID
AQABo0IwQDAOBgNVHQ8BAf8EBAMCAYYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4E
FgQU5K8rJnEaK0gnhS9SZizv8IkTcT4wDQYJKoZIhvcNAQEMBQADggIBAJ+qQibb
C5u+/x6Wki4+omVKapi6Ist9wTrYggoGxval3sBOh2Z5ofmmWJyq+bXmYOfg6LEe
QkEzCzc9zolwFcq1JKjPa7XSQCGYzyI0zzvFIoTgxQ6KfF2I5DUkzps+GlQebtuy
h6f88/qBVRRiClmpIgUxPoLW7ttXNLwzldMXG+gnoot7TiYaelpkttGsN/H9oPM4
7HLwEXWdyzRSjeZ2axfG34arJ45JK3VmgRAhpuo+9K4l/3wV3s6MJT/KYnAK9y8J
ZgfIPxz88NtFMN9iiMG1D53Dn0reWVlHxYciNuaCp+0KueIHoI17eko8cdLiA6Ef
MgfdG+RCzgwARWGAtQsgWSl4vflVy2PFPEz0tv/bal8xa5meLMFrUKTX5hgUvYU/
Z6tGn6D/Qqc6f1zLXbBwHSs09dR2CQzreExZBfMzQsNhFRAbd03OIozUhfJFfbdT
6u9AWpQKXCBfTkBdYiJ23//OYb2MI3jSNwLgjt7RETeJ9r/tSQdirpLsQBqvFAnZ
0E6yove+7u7Y/9waLd64NnHi/Hm3lCXRSHNboTXns5lndcEZOitHTtNCjv0xyBZm
2tIMPNuzjsmhDYAPexZ3FL//2wmUspO8IFgV6dtxQ/PeEMMA3KgqlbbC1j+Qa3bb
bP6MvPJwNQzcmRk13NfIRmPVNnGuV/u3gm3c
-----END CERTIFICATE-----
"""u8;

internal static readonly @string googleLeaf = """
-----BEGIN CERTIFICATE-----
MIIFUjCCBDqgAwIBAgIQERmRWTzVoz0SMeozw2RM3DANBgkqhkiG9w0BAQsFADBG
MQswCQYDVQQGEwJVUzEiMCAGA1UEChMZR29vZ2xlIFRydXN0IFNlcnZpY2VzIExM
QzETMBEGA1UEAxMKR1RTIENBIDFDMzAeFw0yMzAxMDIwODE5MTlaFw0yMzAzMjcw
ODE5MThaMBkxFzAVBgNVBAMTDnd3dy5nb29nbGUuY29tMIIBIjANBgkqhkiG9w0B
AQEFAAOCAQ8AMIIBCgKCAQEAq30odrKMT54TJikMKL8S+lwoCMT5geP0u9pWjk6a
wdB6i3kO+UE4ijCAmhbcZKeKaLnGJ38weZNwB1ayabCYyX7hDiC/nRcZU49LX5+o
55kDVaNn14YKkg2kCeX25HDxSwaOsNAIXKPTqiQL5LPvc4Twhl8HY51hhNWQrTEr
N775eYbixEULvyVLq5BLbCOpPo8n0/MTjQ32ku1jQq3GIYMJC/Rf2VW5doF6t9zs
KleflAN8OdKp0ME9OHg0T1P3yyb67T7n0SpisHbeG06AmQcKJF9g/9VPJtRf4l1Q
WRPDC+6JUqzXCxAGmIRGZ7TNMxPMBW/7DRX6w8oLKVNb0wIDAQABo4ICZzCCAmMw
DgYDVR0PAQH/BAQDAgWgMBMGA1UdJQQMMAoGCCsGAQUFBwMBMAwGA1UdEwEB/wQC
MAAwHQYDVR0OBBYEFBnboj3lf9+Xat4oEgo6ZtIMr8ZuMB8GA1UdIwQYMBaAFIp0
f6+Fze6VzT2c0OJGFPNxNR0nMGoGCCsGAQUFBwEBBF4wXDAnBggrBgEFBQcwAYYb
aHR0cDovL29jc3AucGtpLmdvb2cvZ3RzMWMzMDEGCCsGAQUFBzAChiVodHRwOi8v
cGtpLmdvb2cvcmVwby9jZXJ0cy9ndHMxYzMuZGVyMBkGA1UdEQQSMBCCDnd3dy5n
b29nbGUuY29tMCEGA1UdIAQaMBgwCAYGZ4EMAQIBMAwGCisGAQQB1nkCBQMwPAYD
VR0fBDUwMzAxoC+gLYYraHR0cDovL2NybHMucGtpLmdvb2cvZ3RzMWMzL1FPdkow
TjFzVDJBLmNybDCCAQQGCisGAQQB1nkCBAIEgfUEgfIA8AB2AHoyjFTYty22IOo4
4FIe6YQWcDIThU070ivBOlejUutSAAABhXHHOiUAAAQDAEcwRQIgBUkikUIXdo+S
3T8PP0/cvokhUlumRE3GRWGL4WRMLpcCIQDY+bwK384mZxyXGZ5lwNRTAPNzT8Fx
1+//nbaGK3BQMAB2AOg+0No+9QY1MudXKLyJa8kD08vREWvs62nhd31tBr1uAAAB
hXHHOfQAAAQDAEcwRQIgLoVydNfMFKV9IoZR+M0UuJ2zOqbxIRum7Sn9RMPOBGMC
IQD1/BgzCSDTvYvco6kpB6ifKSbg5gcb5KTnYxQYwRW14TANBgkqhkiG9w0BAQsF
AAOCAQEA2bQQu30e3OFu0bmvQHmcqYvXBu6tF6e5b5b+hj4O+Rn7BXTTmaYX3M6p
MsfRH4YVJJMB/dc3PROR2VtnKFC6gAZX+RKM6nXnZhIlOdmQnonS1ecOL19PliUd
VXbwKjXqAO0Ljd9y9oXaXnyPyHmUJNI5YXAcxE+XXiOZhcZuMYyWmoEKJQ/XlSga
zWfTn1IcKhA3IC7A1n/5bkkWD1Xi1mdWFQ6DQDMp//667zz7pKOgFMlB93aPDjvI
c78zEqNswn6xGKXpWF5xVwdFcsx9HKhJ6UAi2bQ/KQ1yb7LPUOR6wXXWrG1cLnNP
i8eNLnKL9PXQ+5SwJFCzfEhcIZuhzg==
-----END CERTIFICATE-----
"""u8;

// googleLeafWithInvalidHash is the same as googleLeaf, but the signature
// algorithm in the certificate contains a nonsense OID.
internal static readonly @string googleLeafWithInvalidHash = """
-----BEGIN CERTIFICATE-----
MIIFUjCCBDqgAwIBAgIQERmRWTzVoz0SMeozw2RM3DANBgkqhkiG9w0BAQ4FADBG
MQswCQYDVQQGEwJVUzEiMCAGA1UEChMZR29vZ2xlIFRydXN0IFNlcnZpY2VzIExM
QzETMBEGA1UEAxMKR1RTIENBIDFDMzAeFw0yMzAxMDIwODE5MTlaFw0yMzAzMjcw
ODE5MThaMBkxFzAVBgNVBAMTDnd3dy5nb29nbGUuY29tMIIBIjANBgkqhkiG9w0B
AQEFAAOCAQ8AMIIBCgKCAQEAq30odrKMT54TJikMKL8S+lwoCMT5geP0u9pWjk6a
wdB6i3kO+UE4ijCAmhbcZKeKaLnGJ38weZNwB1ayabCYyX7hDiC/nRcZU49LX5+o
55kDVaNn14YKkg2kCeX25HDxSwaOsNAIXKPTqiQL5LPvc4Twhl8HY51hhNWQrTEr
N775eYbixEULvyVLq5BLbCOpPo8n0/MTjQ32ku1jQq3GIYMJC/Rf2VW5doF6t9zs
KleflAN8OdKp0ME9OHg0T1P3yyb67T7n0SpisHbeG06AmQcKJF9g/9VPJtRf4l1Q
WRPDC+6JUqzXCxAGmIRGZ7TNMxPMBW/7DRX6w8oLKVNb0wIDAQABo4ICZzCCAmMw
DgYDVR0PAQH/BAQDAgWgMBMGA1UdJQQMMAoGCCsGAQUFBwMBMAwGA1UdEwEB/wQC
MAAwHQYDVR0OBBYEFBnboj3lf9+Xat4oEgo6ZtIMr8ZuMB8GA1UdIwQYMBaAFIp0
f6+Fze6VzT2c0OJGFPNxNR0nMGoGCCsGAQUFBwEBBF4wXDAnBggrBgEFBQcwAYYb
aHR0cDovL29jc3AucGtpLmdvb2cvZ3RzMWMzMDEGCCsGAQUFBzAChiVodHRwOi8v
cGtpLmdvb2cvcmVwby9jZXJ0cy9ndHMxYzMuZGVyMBkGA1UdEQQSMBCCDnd3dy5n
b29nbGUuY29tMCEGA1UdIAQaMBgwCAYGZ4EMAQIBMAwGCisGAQQB1nkCBQMwPAYD
VR0fBDUwMzAxoC+gLYYraHR0cDovL2NybHMucGtpLmdvb2cvZ3RzMWMzL1FPdkow
TjFzVDJBLmNybDCCAQQGCisGAQQB1nkCBAIEgfUEgfIA8AB2AHoyjFTYty22IOo4
4FIe6YQWcDIThU070ivBOlejUutSAAABhXHHOiUAAAQDAEcwRQIgBUkikUIXdo+S
3T8PP0/cvokhUlumRE3GRWGL4WRMLpcCIQDY+bwK384mZxyXGZ5lwNRTAPNzT8Fx
1+//nbaGK3BQMAB2AOg+0No+9QY1MudXKLyJa8kD08vREWvs62nhd31tBr1uAAAB
hXHHOfQAAAQDAEcwRQIgLoVydNfMFKV9IoZR+M0UuJ2zOqbxIRum7Sn9RMPOBGMC
IQD1/BgzCSDTvYvco6kpB6ifKSbg5gcb5KTnYxQYwRW14TANBgkqhkiG9w0BAQ4F
AAOCAQEA2bQQu30e3OFu0bmvQHmcqYvXBu6tF6e5b5b+hj4O+Rn7BXTTmaYX3M6p
MsfRH4YVJJMB/dc3PROR2VtnKFC6gAZX+RKM6nXnZhIlOdmQnonS1ecOL19PliUd
VXbwKjXqAO0Ljd9y9oXaXnyPyHmUJNI5YXAcxE+XXiOZhcZuMYyWmoEKJQ/XlSga
zWfTn1IcKhA3IC7A1n/5bkkWD1Xi1mdWFQ6DQDMp//667zz7pKOgFMlB93aPDjvI
c78zEqNswn6xGKXpWF5xVwdFcsx9HKhJ6UAi2bQ/KQ1yb7LPUOR6wXXWrG1cLnNP
i8eNLnKL9PXQ+5SwJFCzfEhcIZuhzg==
-----END CERTIFICATE-----
"""u8;

internal static readonly @string smimeLeaf = """
-----BEGIN CERTIFICATE-----
MIIIPDCCBiSgAwIBAgIQaMDxFS0pOMxZZeOBxoTJtjANBgkqhkiG9w0BAQsFADCB
nTELMAkGA1UEBhMCRVMxFDASBgNVBAoMC0laRU5QRSBTLkEuMTowOAYDVQQLDDFB
WlogWml1cnRhZ2lyaSBwdWJsaWtvYSAtIENlcnRpZmljYWRvIHB1YmxpY28gU0NB
MTwwOgYDVQQDDDNFQUVrbyBIZXJyaSBBZG1pbmlzdHJhemlvZW4gQ0EgLSBDQSBB
QVBQIFZhc2NhcyAoMikwHhcNMTcwNzEyMDg1MzIxWhcNMjEwNzEyMDg1MzIxWjCC
AQwxDzANBgNVBAoMBklaRU5QRTE4MDYGA1UECwwvWml1cnRhZ2lyaSBrb3Jwb3Jh
dGlib2EtQ2VydGlmaWNhZG8gY29ycG9yYXRpdm8xQzBBBgNVBAsMOkNvbmRpY2lv
bmVzIGRlIHVzbyBlbiB3d3cuaXplbnBlLmNvbSBub2xhIGVyYWJpbGkgamFraXRl
a28xFzAVBgNVBC4TDi1kbmkgOTk5OTk5ODlaMSQwIgYDVQQDDBtDT1JQT1JBVElW
TyBGSUNUSUNJTyBBQ1RJVk8xFDASBgNVBCoMC0NPUlBPUkFUSVZPMREwDwYDVQQE
DAhGSUNUSUNJTzESMBAGA1UEBRMJOTk5OTk5ODlaMIIBIjANBgkqhkiG9w0BAQEF
AAOCAQ8AMIIBCgKCAQEAwVOMwUDfBtsH0XuxYnb+v/L774jMH8valX7RPH8cl2Lb
SiqSo0RchW2RGA2d1yuYHlpChC9jGmt0X/g66/E/+q2hUJlfJtqVDJFwtFYV4u2S
yzA3J36V4PRkPQrKxAsbzZriFXAF10XgiHQz9aVeMMJ9GBhmh9+DK8Tm4cMF6i8l
+AuC35KdngPF1x0ealTYrYZplpEJFO7CiW42aLi6vQkDR2R7nmZA4AT69teqBWsK
0DZ93/f0G/3+vnWwNTBF0lB6dIXoaz8OMSyHLqGnmmAtMrzbjAr/O/WWgbB/BqhR
qjJQ7Ui16cuDldXaWQ/rkMzsxmsAox0UF+zdQNvXUQIDAQABo4IDBDCCAwAwgccG
A1UdEgSBvzCBvIYVaHR0cDovL3d3dy5pemVucGUuY29tgQ9pbmZvQGl6ZW5wZS5j
b22kgZEwgY4xRzBFBgNVBAoMPklaRU5QRSBTLkEuIC0gQ0lGIEEwMTMzNzI2MC1S
TWVyYy5WaXRvcmlhLUdhc3RlaXogVDEwNTUgRjYyIFM4MUMwQQYDVQQJDDpBdmRh
IGRlbCBNZWRpdGVycmFuZW8gRXRvcmJpZGVhIDE0IC0gMDEwMTAgVml0b3JpYS1H
YXN0ZWl6MB4GA1UdEQQXMBWBE2ZpY3RpY2lvQGl6ZW5wZS5ldXMwDgYDVR0PAQH/
BAQDAgXgMCkGA1UdJQQiMCAGCCsGAQUFBwMCBggrBgEFBQcDBAYKKwYBBAGCNxQC
AjAdBgNVHQ4EFgQUyeoOD4cgcljKY0JvrNuX2waFQLAwHwYDVR0jBBgwFoAUwKlK
90clh/+8taaJzoLSRqiJ66MwggEnBgNVHSAEggEeMIIBGjCCARYGCisGAQQB8zkB
AQEwggEGMDMGCCsGAQUFBwIBFidodHRwOi8vd3d3Lml6ZW5wZS5jb20vcnBhc2Nh
Y29ycG9yYXRpdm8wgc4GCCsGAQUFBwICMIHBGoG+Wml1cnRhZ2lyaWEgRXVza2Fs
IEF1dG9ub21pYSBFcmtpZGVnb2tvIHNla3RvcmUgcHVibGlrb2tvIGVyYWt1bmRl
ZW4gYmFybmUtc2FyZWV0YW4gYmFrYXJyaWsgZXJhYmlsIGRhaXRla2UuIFVzbyBy
ZXN0cmluZ2lkbyBhbCBhbWJpdG8gZGUgcmVkZXMgaW50ZXJuYXMgZGUgRW50aWRh
ZGVzIGRlbCBTZWN0b3IgUHVibGljbyBWYXNjbzAyBggrBgEFBQcBAQQmMCQwIgYI
KwYBBQUHMAGGFmh0dHA6Ly9vY3NwLml6ZW5wZS5jb20wOgYDVR0fBDMwMTAvoC2g
K4YpaHR0cDovL2NybC5pemVucGUuY29tL2NnaS1iaW4vY3JsaW50ZXJuYTIwDQYJ
KoZIhvcNAQELBQADggIBAIy5PQ+UZlCRq6ig43vpHwlwuD9daAYeejV0Q+ZbgWAE
GtO0kT/ytw95ZEJMNiMw3fYfPRlh27ThqiT0VDXZJDlzmn7JZd6QFcdXkCsiuv4+
ZoXAg/QwnA3SGUUO9aVaXyuOIIuvOfb9MzoGp9xk23SMV3eiLAaLMLqwB5DTfBdt
BGI7L1MnGJBv8RfP/TL67aJ5bgq2ri4S8vGHtXSjcZ0+rCEOLJtmDNMnTZxancg3
/H5edeNd+n6Z48LO+JHRxQufbC4mVNxVLMIP9EkGUejlq4E4w6zb5NwCQczJbSWL
i31rk2orsNsDlyaLGsWZp3JSNX6RmodU4KAUPor4jUJuUhrrm3Spb73gKlV/gcIw
bCE7mML1Kss3x1ySaXsis6SZtLpGWKkW2iguPWPs0ydV6RPhmsCxieMwPPIJ87vS
5IejfgyBae7RSuAIHyNFy4uI5xwvwUFf6OZ7az8qtW7ImFOgng3Ds+W9k1S2CNTx
d0cnKTfA6IpjGo8EeHcxnIXT8NPImWaRj0qqonvYady7ci6U4m3lkNSdXNn1afgw
mYust+gxVtOZs1gk2MUCgJ1V1X+g7r/Cg7viIn6TLkLrpS1kS1hvMqkl9M+7XqPo
Qd95nJKOkusQpy99X4dF/lfbYAQnnjnqh3DLD2gvYObXFaAYFaiBKTiMTV2X72F+
-----END CERTIFICATE-----
"""u8;

internal static readonly @string smimeIntermediate = """
-----BEGIN CERTIFICATE-----
MIIHNzCCBSGgAwIBAgIQJMXIqlZvjuhMvqcFXOFkpDALBgkqhkiG9w0BAQswODEL
MAkGA1UEBhMCRVMxFDASBgNVBAoMC0laRU5QRSBTLkEuMRMwEQYDVQQDDApJemVu
cGUuY29tMB4XDTEwMTAyMDA4MjMzM1oXDTM3MTIxMjIzMDAwMFowgZ0xCzAJBgNV
BAYTAkVTMRQwEgYDVQQKDAtJWkVOUEUgUy5BLjE6MDgGA1UECwwxQVpaIFppdXJ0
YWdpcmkgcHVibGlrb2EgLSBDZXJ0aWZpY2FkbyBwdWJsaWNvIFNDQTE8MDoGA1UE
AwwzRUFFa28gSGVycmkgQWRtaW5pc3RyYXppb2VuIENBIC0gQ0EgQUFQUCBWYXNj
YXMgKDIpMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAoIM7nEdI0N1h
rR5T4xuV/usKDoMIasaiKvfLhbwxaNtTt+a7W/6wV5bv3svQFIy3sUXjjdzV1nG2
To2wo/YSPQiOt8exWvOapvL21ogiof+kelWnXFjWaKJI/vThHYLgIYEMj/y4HdtU
ojI646rZwqsb4YGAopwgmkDfUh5jOhV2IcYE3TgJAYWVkj6jku9PLaIsHiarAHjD
PY8dig8a4SRv0gm5Yk7FXLmW1d14oxQBDeHZ7zOEXfpafxdEDO2SNaRJjpkh8XRr
PGqkg2y1Q3gT6b4537jz+StyDIJ3omylmlJsGCwqT7p8mEqjGJ5kC5I2VnjXKuNn
soShc72khWZVUJiJo5SGuAkNE2ZXqltBVm5Jv6QweQKsX6bkcMc4IZok4a+hx8FM
8IBpGf/I94pU6HzGXqCyc1d46drJgDY9mXa+6YDAJFl3xeXOOW2iGCfwXqhiCrKL
MYvyMZzqF3QH5q4nb3ZnehYvraeMFXJXDn+Utqp8vd2r7ShfQJz01KtM4hgKdgSg
jtW+shkVVN5ng/fPN85ovfAH2BHXFfHmQn4zKsYnLitpwYM/7S1HxlT61cdQ7Nnk
3LZTYEgAoOmEmdheklT40WAYakksXGM5VrzG7x9S7s1Tm+Vb5LSThdHC8bxxwyTb
KsDRDNJ84N9fPDO6qHnzaL2upQ43PycCAwEAAaOCAdkwggHVMIHHBgNVHREEgb8w
gbyGFWh0dHA6Ly93d3cuaXplbnBlLmNvbYEPaW5mb0BpemVucGUuY29tpIGRMIGO
MUcwRQYDVQQKDD5JWkVOUEUgUy5BLiAtIENJRiBBMDEzMzcyNjAtUk1lcmMuVml0
b3JpYS1HYXN0ZWl6IFQxMDU1IEY2MiBTODFDMEEGA1UECQw6QXZkYSBkZWwgTWVk
aXRlcnJhbmVvIEV0b3JiaWRlYSAxNCAtIDAxMDEwIFZpdG9yaWEtR2FzdGVpejAP
BgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB/wQEAwIBBjAdBgNVHQ4EFgQUwKlK90cl
h/+8taaJzoLSRqiJ66MwHwYDVR0jBBgwFoAUHRxlDqjyJXu0kc/ksbHmvVV0bAUw
OgYDVR0gBDMwMTAvBgRVHSAAMCcwJQYIKwYBBQUHAgEWGWh0dHA6Ly93d3cuaXpl
bnBlLmNvbS9jcHMwNwYIKwYBBQUHAQEEKzApMCcGCCsGAQUFBzABhhtodHRwOi8v
b2NzcC5pemVucGUuY29tOjgwOTQwMwYDVR0fBCwwKjAooCagJIYiaHR0cDovL2Ny
bC5pemVucGUuY29tL2NnaS1iaW4vYXJsMjALBgkqhkiG9w0BAQsDggIBAMbjc3HM
3DG9ubWPkzsF0QsktukpujbTTcGk4h20G7SPRy1DiiTxrRzdAMWGjZioOP3/fKCS
M539qH0M+gsySNie+iKlbSZJUyE635T1tKw+G7bDUapjlH1xyv55NC5I6wCXGC6E
3TEP5B/E7dZD0s9E4lS511ubVZivFgOzMYo1DO96diny/N/V1enaTCpRl1qH1OyL
xUYTijV4ph2gL6exwuG7pxfRcVNHYlrRaXWfTz3F6NBKyULxrI3P/y6JAtN1GqT4
VF/+vMygx22n0DufGepBwTQz6/rr1ulSZ+eMnuJiTXgh/BzQnkUsXTb8mHII25iR
0oYF2qAsk6ecWbLiDpkHKIDHmML21MZE13MS8NSvTHoqJO4LyAmDe6SaeNHtrPlK
b6mzE1BN2ug+ZaX8wLA5IMPFaf0jKhb/Cxu8INsxjt00brsErCc9ip1VNaH0M4bi
1tGxfiew2436FaeyUxW7Pl6G5GgkNbuUc7QIoRy06DdU/U38BxW3uyJMY60zwHvS
FlKAn0OvYp4niKhAJwaBVN3kowmJuOU5Rid+TUnfyxbJ9cttSgzaF3hP/N4zgMEM
5tikXUskeckt8LUK96EH0QyssavAMECUEb/xrupyRdYWwjQGvNLq6T5+fViDGyOw
k+lzD44wofy8paAy9uC9Owae0zMEzhcsyRm7
-----END CERTIFICATE-----
"""u8;

internal static readonly @string smimeRoot = """
-----BEGIN CERTIFICATE-----
MIIF8TCCA9mgAwIBAgIQALC3WhZIX7/hy/WL1xnmfTANBgkqhkiG9w0BAQsFADA4
MQswCQYDVQQGEwJFUzEUMBIGA1UECgwLSVpFTlBFIFMuQS4xEzARBgNVBAMMCkl6
ZW5wZS5jb20wHhcNMDcxMjEzMTMwODI4WhcNMzcxMjEzMDgyNzI1WjA4MQswCQYD
VQQGEwJFUzEUMBIGA1UECgwLSVpFTlBFIFMuQS4xEzARBgNVBAMMCkl6ZW5wZS5j
b20wggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQDJ03rKDx6sp4boFmVq
scIbRTJxldn+EFvMr+eleQGPicPK8lVx93e+d5TzcqQsRNiekpsUOqHnJJAKClaO
xdgmlOHZSOEtPtoKct2jmRXagaKH9HtuJneJWK3W6wyyQXpzbm3benhB6QiIEn6H
LmYRY2xU+zydcsC8Lv/Ct90NduM61/e0aL6i9eOBbsFGb12N4E3GVFWJGjMxCrFX
uaOKmMPsOzTFlUFpfnXCPCDFYbpRR6AgkJOhkEvzTnyFRVSa0QUmQbC1TR0zvsQD
yCV8wXDbO/QJLVQnSKwv4cSsPsjLkkxTOTcj7NMB+eAJRE1NZMDhDVqHIrytG6P+
JrUV86f8hBnp7KGItERphIPzidF0BqnMC9bC3ieFUCbKF7jJeodWLBoBHmy+E60Q
rLUk9TiRodZL2vG70t5HtfG8gfZZa88ZU+mNFctKy6lvROUbQc/hhqfK0GqfvEyN
BjNaooXlkDWgYlwWTvDjovoDGrQscbNYLN57C9saD+veIR8GdwYDsMnvmfzAuU8L
hij+0rnq49qlw0dpEuDb8PYZi+17cNcC1u2HGCgsBCRMd+RIihrGO5rUD8r6ddIB
QFqNeb+Lz0vPqhbBleStTIo+F5HUsWLlguWABKQDfo2/2n+iD5dPDNMN+9fR5XJ+
HMh3/1uaD7euBUbl8agW7EekFwIDAQABo4H2MIHzMIGwBgNVHREEgagwgaWBD2lu
Zm9AaXplbnBlLmNvbaSBkTCBjjFHMEUGA1UECgw+SVpFTlBFIFMuQS4gLSBDSUYg
QTAxMzM3MjYwLVJNZXJjLlZpdG9yaWEtR2FzdGVpeiBUMTA1NSBGNjIgUzgxQzBB
BgNVBAkMOkF2ZGEgZGVsIE1lZGl0ZXJyYW5lbyBFdG9yYmlkZWEgMTQgLSAwMTAx
MCBWaXRvcmlhLUdhc3RlaXowDwYDVR0TAQH/BAUwAwEB/zAOBgNVHQ8BAf8EBAMC
AQYwHQYDVR0OBBYEFB0cZQ6o8iV7tJHP5LGx5r1VdGwFMA0GCSqGSIb3DQEBCwUA
A4ICAQB4pgwWSp9MiDrAyw6lFn2fuUhfGI8NYjb2zRlrrKvV9pF9rnHzP7MOeIWb
laQnIUdCSnxIOvVFfLMMjlF4rJUT3sb9fbgakEyrkgPH7UIBzg/YsfqikuFgba56
awmqxinuaElnMIAkejEWOVt+8Rwu3WwJrfIxwYJOubv5vr8qhT/AQKM6WfxZSzwo
JNu0FXWuDYi6LnPAvViH5ULy617uHjAimcs30cQhbIHsvm0m5hzkQiCeR7Csg1lw
LDXWrzY0tM07+DKo7+N4ifuNRSzanLh+QBxh5z6ikixL8s36mLYp//Pye6kfLqCT
VyvehQP5aTfLnnhqBbTFMXiJ7HqnheG5ezzevh55hM6fcA5ZwjUukCox2eRFekGk
LhObNA5me0mrZJfQRsN5nXJQY6aYWwa9SG3YOYNw6DXwBdGqvOPbyALqfP2C2sJb
UjWumDqtujWTI6cfSN01RpiyEGjkpTHCClguGYEQyVB1/OpaFs4R1+7vUIgtYf8/
QnMFlEPVjjxOAToZpR9GTnfQXeWBIiGH/pR9hNiTrdZoQ0iy2+tzJOeRf1SktoA+
naM8THLCV8Sg1Mw4J87VBp6iSNnpn86CcDaTmjvfliHjWbcM2pE38P1ZWrOZyGls
QyYBNWNgVYkDOnXYukrZVP/u3oDYLdE41V4tC5h9Pmzb/CaIxw==
-----END CERTIFICATE-----
"""u8;

internal static @string nameConstraintsLeaf = """
-----BEGIN CERTIFICATE-----
MIIG+jCCBOKgAwIBAgIQWj9gbtPPkZs65N6TKyutRjANBgkqhkiG9w0BAQsFADCB
yzELMAkGA1UEBhMCVVMxETAPBgNVBAgTCFZpcmdpbmlhMRMwEQYDVQQHEwpCbGFj
a3NidXJnMSMwIQYDVQQLExpHbG9iYWwgUXVhbGlmaWVkIFNlcnZlciBDQTE8MDoG
A1UEChMzVmlyZ2luaWEgUG9seXRlY2huaWMgSW5zdGl0dXRlIGFuZCBTdGF0ZSBV
bml2ZXJzaXR5MTEwLwYDVQQDEyhWaXJnaW5pYSBUZWNoIEdsb2JhbCBRdWFsaWZp
ZWQgU2VydmVyIENBMB4XDTE4MDQyNjE5NDU1M1oXDTE5MTIxMDAwMDAwMFowgZAx
CzAJBgNVBAYTAlVTMREwDwYDVQQIEwhWaXJnaW5pYTETMBEGA1UEBxMKQmxhY2tz
YnVyZzE8MDoGA1UEChMzVmlyZ2luaWEgUG9seXRlY2huaWMgSW5zdGl0dXRlIGFu
ZCBTdGF0ZSBVbml2ZXJzaXR5MRswGQYDVQQDExJ1ZGN0ZXN0LmFkcy52dC5lZHUw
ggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCcoVBeV3AzdSGMzRWH0tuM
VluEj+sq4r9PuLDBAdgjjHi4ED8npT2/fgOalswInXspRvFS+pkEwTrmeZ7HPzRJ
HUE5YlX5Nc6WI8ZXPVg5E6GyoMy6gNlALwqsIvDCvqxBMc39oG6yOuGmQXdF6s0N
BJMrXc4aPz60s4QMWNO2OHL0pmnZqE1TxYRBHUY/dk3cfsIepIDDuSxRsNE/P/MI
pxm/uVOyiLEnPmOMsL430SZ7nC8PxUMqya9ok6Zaf7k54g7JJXDjE96VMCjMszIv
Ud9qe1PbokTOxlG/4QW7Qm0dPbiJhTUuoBzVAxzlOOkSFdXqSYKjC9tFcbr8y+pT
AgMBAAGjggIRMIICDTCBtgYIKwYBBQUHAQEEgakwgaYwXwYIKwYBBQUHMAKGU2h0
dHA6Ly93d3cucGtpLnZ0LmVkdS9nbG9iYWxxdWFsaWZpZWRzZXJ2ZXIvY2FjZXJ0
L2dsb2JhbHF1YWxpZmllZHNlcnZlcl9zaGEyNTYuY3J0MEMGCCsGAQUFBzABhjdo
dHRwOi8vdnRjYS5wa2kudnQuZWR1OjgwODAvZWpiY2EvcHVibGljd2ViL3N0YXR1
cy9vY3NwMB0GA1UdDgQWBBSzDLXee0wbgXpVQxvBQCophQDZbTAMBgNVHRMBAf8E
AjAAMB8GA1UdIwQYMBaAFLxiYCfV4zVIF+lLq0Vq0Miod3GMMGoGA1UdIARjMGEw
DgYMKwYBBAG0aAUCAgIBMA4GDCsGAQQBtGgFAgIBATA/BgwrBgEEAbRoBQICAwEw
LzAtBggrBgEFBQcCARYhaHR0cDovL3d3dy5wa2kudnQuZWR1L2dsb2JhbC9jcHMv
MEoGA1UdHwRDMEEwP6A9oDuGOWh0dHA6Ly93d3cucGtpLnZ0LmVkdS9nbG9iYWxx
dWFsaWZpZWRzZXJ2ZXIvY3JsL2NhY3JsLmNybDAOBgNVHQ8BAf8EBAMCBeAwHQYD
VR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMB0GA1UdEQQWMBSCEnVkY3Rlc3Qu
YWRzLnZ0LmVkdTANBgkqhkiG9w0BAQsFAAOCAgEAD79kuyZbwQJCSBOVq9lA0lj4
juHM7RMBfp2GuWvhk5F90OMKQCNdITva3oq4uQzt013TtwposYXq/d0Jobk6RHxj
OJzRZVvEPsXLvKm8oLhz7/qgI8gcVeJFR9WgdNhjN1upn++EnABHUdDR77fgixuH
FFwNC0WSZ6G0+WgYV7MKD4jYWh1DXEaJtQCN763IaWGxgvQaLUwS423xgwsx+8rw
hCRYns5u8myTbUlEu2b+GYimiogtDFMT01A7y88vKl9g+3bx42dJHQNNmSzmYPfs
IljtQbVwJIyNL/rwjoz7BTk8e9WY0qUK7ZYh+oGK8kla8yfPKtkvOJV29KdFKzTm
42kNm6cH+U5gGwEEg+Xj66Q2yFH5J9kAoBazTepgQ/13wwTY0mU9PtKVBtMH5Y/u
MoNVZz6p7vWWRrY5qSXIaW9qyF3bZnmPEHHYTplWsyAyh8blGlqPnpayDflPiQF/
9y37kax5yhT0zPZW1ZwIZ5hDTO7pu5i83bYh3pzhvJNHtv74Nn/SX1dTZrWBi/HG
OSWK3CLz8qAEBe72XGoBjBzuk9VQxg6k52qjxCyYf7CBSQpTZhsNMf0gzu+JNATc
b+XaOqJT6uI/RfqAJVe16ZeXZIFZgQlzIwRS9vobq9fqTIpH/QxqgXROGqAlbBVp
/ByH6FEe6+oH1UCklhg=
-----END CERTIFICATE-----
"""u8;

internal static @string nameConstraintsIntermediate1 = """
-----BEGIN CERTIFICATE-----
MIIHVTCCBj2gAwIBAgINAecHzcaPEeFvu7X4TTANBgkqhkiG9w0BAQsFADBjMQsw
CQYDVQQGEwJCRTEVMBMGA1UECxMMVHJ1c3RlZCBSb290MRkwFwYDVQQKExBHbG9i
YWxTaWduIG52LXNhMSIwIAYDVQQDExlUcnVzdGVkIFJvb3QgQ0EgU0hBMjU2IEcy
MB4XDTE3MTIwNjAwMDAwMFoXDTIyMTIwNjAwMDAwMFowgcsxCzAJBgNVBAYTAlVT
MREwDwYDVQQIEwhWaXJnaW5pYTETMBEGA1UEBxMKQmxhY2tzYnVyZzEjMCEGA1UE
CxMaR2xvYmFsIFF1YWxpZmllZCBTZXJ2ZXIgQ0ExPDA6BgNVBAoTM1Zpcmdpbmlh
IFBvbHl0ZWNobmljIEluc3RpdHV0ZSBhbmQgU3RhdGUgVW5pdmVyc2l0eTExMC8G
A1UEAxMoVmlyZ2luaWEgVGVjaCBHbG9iYWwgUXVhbGlmaWVkIFNlcnZlciBDQTCC
AiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBALgIZhEaptBWADBqdJ45ueFG
zMXaGHnzNxoxR1fQIaaRQNdCg4cw3A4dWKMeEgYLtsp65ai3Xfw62Qaus0+KJ3Rh
gV+rihqK81NUzkls78fJlADVDI4fCTlothsrE1CTOMiy97jKHai5mVTiWxmcxpmj
v7fm5Nhc+uHgh2hIz6npryq495mD51ZrUTIaqAQN6Pw/VHfAmR524vgriTOjtp1t
4lA9pXGWjF/vkhAKFFheOQSQ00rngo2wHgCqMla64UTN0oz70AsCYNZ3jDLx0kOP
0YmMR3Ih91VA63kLqPXA0R6yxmmhhxLZ5bcyAy1SLjr1N302MIxLM/pSy6aquEnb
ELhzqyp9yGgRyGJay96QH7c4RJY6gtcoPDbldDcHI9nXngdAL4DrZkJ9OkDkJLyq
G66WZTF5q4EIs6yMdrywz0x7QP+OXPJrjYpbeFs6tGZCFnWPFfmHCRJF8/unofYr
heq+9J7Jx3U55S/k57NXbAM1RAJOuMTlfn9Etf9Dpoac9poI4Liav6rBoUQk3N3J
WqnVHNx/NdCyJ1/6UbKMJUZsStAVglsi6lVPo289HHOE4f7iwl3SyekizVOp01wU
in3ycnbZB/rXmZbwapSxTTSBf0EIOr9i4EGfnnhCAVA9U5uLrI5OEB69IY8PNX00
71s3Z2a2fio5c8m3JkdrAgMBAAGjggKdMIICmTAOBgNVHQ8BAf8EBAMCAQYwHQYD
VR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMBIGA1UdEwEB/wQIMAYBAf8CAQAw
HQYDVR0OBBYEFLxiYCfV4zVIF+lLq0Vq0Miod3GMMB8GA1UdIwQYMBaAFMhjmwhp
VMKYyNnN4zO3UF74yQGbMIGNBggrBgEFBQcBAQSBgDB+MDcGCCsGAQUFBzABhito
dHRwOi8vb2NzcDIuZ2xvYmFsc2lnbi5jb20vdHJ1c3Ryb290c2hhMmcyMEMGCCsG
AQUFBzAChjdodHRwOi8vc2VjdXJlLmdsb2JhbHNpZ24uY29tL2NhY2VydC90cnVz
dHJvb3RzaGEyZzIuY3J0MIHyBgNVHR4EgeowgeeggbIwCIEGdnQuZWR1MAmCB2Jl
di5uZXQwCoIIdmNvbS5lZHUwCIIGdnQuZWR1MAyCCnZ0Y2dpdC5jb20wd6R1MHMx
CzAJBgNVBAYTAlVTMREwDwYDVQQIEwhWaXJnaW5pYTETMBEGA1UEBxMKQmxhY2tz
YnVyZzE8MDoGA1UEChMzVmlyZ2luaWEgUG9seXRlY2huaWMgSW5zdGl0dXRlIGFu
ZCBTdGF0ZSBVbml2ZXJzaXR5oTAwCocIAAAAAAAAAAAwIocgAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAwQQYDVR0fBDowODA2oDSgMoYwaHR0cDovL2Ny
bC5nbG9iYWxzaWduLmNvbS9ncy90cnVzdHJvb3RzaGEyZzIuY3JsMEwGA1UdIARF
MEMwQQYJKwYBBAGgMgE8MDQwMgYIKwYBBQUHAgEWJmh0dHBzOi8vd3d3Lmdsb2Jh
bHNpZ24uY29tL3JlcG9zaXRvcnkvMA0GCSqGSIb3DQEBCwUAA4IBAQArHocpEKTv
DW1Hw0USj60KN96aLJXTLm05s0LbjloeTePtDFtuisrbE85A0IhCwxdIl/VsQMZB
7mQZBEmLzR+NK1/Luvs7C6WTmkqrE8H7D73dSOab5fMZIXS91V/aEtEQGpJMhwi1
svd9TiiQrVkagrraeRWmTTz9BtUA3CeujuW2tShxF1ew4Q4prYw97EsE4HnKDJtu
RtyTqKsuh/rRvKMmgUdEPZbVI23yzUKhi/mTbyml/35x/f6f5p7OYIKcQ/34sts8
xoW9dfkWBQKAXCstXat3WJVilGXBFub6GoVZdnxTDipyMZhUT/vzXq2bPphjcdR5
YGbmwyYmChfa
-----END CERTIFICATE-----
"""u8;

internal static @string nameConstraintsIntermediate2 = """
-----BEGIN CERTIFICATE-----
MIIEXDCCA0SgAwIBAgILBAAAAAABNumCOV0wDQYJKoZIhvcNAQELBQAwTDEgMB4G
A1UECxMXR2xvYmFsU2lnbiBSb290IENBIC0gUjMxEzARBgNVBAoTCkdsb2JhbFNp
Z24xEzARBgNVBAMTCkdsb2JhbFNpZ24wHhcNMTIwNDI1MTEwMDAwWhcNMjcwNDI1
MTEwMDAwWjBjMQswCQYDVQQGEwJCRTEVMBMGA1UECxMMVHJ1c3RlZCBSb290MRkw
FwYDVQQKExBHbG9iYWxTaWduIG52LXNhMSIwIAYDVQQDExlUcnVzdGVkIFJvb3Qg
Q0EgU0hBMjU2IEcyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAz80+
/Q2PAhLuYwe04YTLBLGKr1/JScHtDvAY5E94GjGxCbSR1/1VhL880UPJyN85tddO
oxZPgtIyZixDvvK+CgpT5webyBBbqK/ap7aoByghAJ7X520XZMRwKA6cEWa6tjCL
WH1zscxQxGzgtV50rn2ux2SapoCPxMpM4+tpEVwWJf3KP3NT+jd9GRaXWgNei5JK
Quo9l+cZkSeuoWijvaer5hcLCufPywMMQd0r6XXIM/l7g9DjMaE24d+fa2bWxQXC
8WT/PZ+D1KUEkdtn/ixADqsoiIibGn7M84EE9/NLjbzPrwROlBUJFz6cuw+II0rZ
8OFFeZ/OkHHYZq2h9wIDAQABo4IBJjCCASIwDgYDVR0PAQH/BAQDAgEGMA8GA1Ud
EwEB/wQFMAMBAf8wRwYDVR0gBEAwPjA8BgRVHSAAMDQwMgYIKwYBBQUHAgEWJmh0
dHBzOi8vd3d3Lmdsb2JhbHNpZ24uY29tL3JlcG9zaXRvcnkvMB0GA1UdDgQWBBTI
Y5sIaVTCmMjZzeMzt1Be+MkBmzA2BgNVHR8ELzAtMCugKaAnhiVodHRwOi8vY3Js
Lmdsb2JhbHNpZ24ubmV0L3Jvb3QtcjMuY3JsMD4GCCsGAQUFBwEBBDIwMDAuBggr
BgEFBQcwAYYiaHR0cDovL29jc3AyLmdsb2JhbHNpZ24uY29tL3Jvb3RyMzAfBgNV
HSMEGDAWgBSP8Et/qC5FJK5NUPpjmove4t0bvDANBgkqhkiG9w0BAQsFAAOCAQEA
XzbLwBjJiY6j3WEcxD3eVnsIY4pY3bl6660tgpxCuLVx4o1xyiVkS/BcQFD7GIoX
FBRrf5HibO1uSEOw0QZoRwlsio1VPg1PRaccG5C1sB51l/TL1XH5zldZBCnRYrrF
qCPorxi0xoRogj8kqkS2xyzYLElhx9X7jIzfZ8dC4mgOeoCtVvwM9xvmef3n6Vyb
7/hl3w/zWwKxWyKJNaF7tScD5nvtLUzyBpr++aztiyJ1WliWcS6W+V2gKg9rxEC/
rc2yJS70DvfkPiEnBJ2x2AHZV3yKTALUqurkV705JledqUT9I5frAwYNXZ8pNzde
n+DIcSIo7yKy6MX9czbFWQ==
-----END CERTIFICATE-----
"""u8;

internal static @string globalSignRoot = """
-----BEGIN CERTIFICATE-----
MIIDXzCCAkegAwIBAgILBAAAAAABIVhTCKIwDQYJKoZIhvcNAQELBQAwTDEgMB4G
A1UECxMXR2xvYmFsU2lnbiBSb290IENBIC0gUjMxEzARBgNVBAoTCkdsb2JhbFNp
Z24xEzARBgNVBAMTCkdsb2JhbFNpZ24wHhcNMDkwMzE4MTAwMDAwWhcNMjkwMzE4
MTAwMDAwWjBMMSAwHgYDVQQLExdHbG9iYWxTaWduIFJvb3QgQ0EgLSBSMzETMBEG
A1UEChMKR2xvYmFsU2lnbjETMBEGA1UEAxMKR2xvYmFsU2lnbjCCASIwDQYJKoZI
hvcNAQEBBQADggEPADCCAQoCggEBAMwldpB5BngiFvXAg7aEyiie/QV2EcWtiHL8
RgJDx7KKnQRfJMsuS+FggkbhUqsMgUdwbN1k0ev1LKMPgj0MK66X17YUhhB5uzsT
gHeMCOFJ0mpiLx9e+pZo34knlTifBtc+ycsmWQ1z3rDI6SYOgxXG71uL0gRgykmm
KPZpO/bLyCiR5Z2KYVc3rHQU3HTgOu5yLy6c+9C7v/U9AOEGM+iCK65TpjoWc4zd
QQ4gOsC0p6Hpsk+QLjJg6VfLuQSSaGjlOCZgdbKfd/+RFO+uIEn8rUAVSNECMWEZ
XriX7613t2Saer9fwRPvm2L7DWzgVGkWqQPabumDk3F2xmmFghcCAwEAAaNCMEAw
DgYDVR0PAQH/BAQDAgEGMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFI/wS3+o
LkUkrk1Q+mOai97i3Ru8MA0GCSqGSIb3DQEBCwUAA4IBAQBLQNvAUKr+yAzv95ZU
RUm7lgAJQayzE4aGKAczymvmdLm6AC2upArT9fHxD4q/c2dKg8dEe3jgr25sbwMp
jjM5RcOO5LlXbKr8EpbsU8Yt5CRsuZRj+9xTaGdWPoO4zzUhw8lo/s7awlOqzJCK
6fBdRoyV3XpYKBovHd7NADdBj+1EbddTKJd+82cEHhXXipa0095MJ6RMG3NzdvQX
mcIfeg7jLQitChws/zyrVQ4PkX4268NXSb7hLi18YIvDQVETI53O9zJrlAGomecs
Mx86OyXShkDOOyyGeMlhLxS67ttVb9+E7gUJTb0o2HLO02JQZR7rkpeDMdmztcpH
WD9f
-----END CERTIFICATE-----
"""u8;

internal static readonly @string digicertRoot = """
-----BEGIN CERTIFICATE-----
MIIDrzCCApegAwIBAgIQCDvgVpBCRrGhdWrJWZHHSjANBgkqhkiG9w0BAQUFADBh
MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
d3cuZGlnaWNlcnQuY29tMSAwHgYDVQQDExdEaWdpQ2VydCBHbG9iYWwgUm9vdCBD
QTAeFw0wNjExMTAwMDAwMDBaFw0zMTExMTAwMDAwMDBaMGExCzAJBgNVBAYTAlVT
MRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5j
b20xIDAeBgNVBAMTF0RpZ2lDZXJ0IEdsb2JhbCBSb290IENBMIIBIjANBgkqhkiG
9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4jvhEXLeqKTTo1eqUKKPC3eQyaKl7hLOllsB
CSDMAZOnTjC3U/dDxGkAV53ijSLdhwZAAIEJzs4bg7/fzTtxRuLWZscFs3YnFo97
nh6Vfe63SKMI2tavegw5BmV/Sl0fvBf4q77uKNd0f3p4mVmFaG5cIzJLv07A6Fpt
43C/dxC//AH2hdmoRBBYMql1GNXRor5H4idq9Joz+EkIYIvUX7Q6hL+hqkpMfT7P
T19sdl6gSzeRntwi5m3OFBqOasv+zbMUZBfHWymeMr/y7vrTC0LUq7dBMtoM1O/4
gdW7jVg/tRvoSSiicNoxBN33shbyTApOB6jtSj1etX+jkMOvJwIDAQABo2MwYTAO
BgNVHQ8BAf8EBAMCAYYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUA95QNVbR
TLtm8KPiGxvDl7I90VUwHwYDVR0jBBgwFoAUA95QNVbRTLtm8KPiGxvDl7I90VUw
DQYJKoZIhvcNAQEFBQADggEBAMucN6pIExIK+t1EnE9SsPTfrgT1eXkIoyQY/Esr
hMAtudXH/vTBH1jLuG2cenTnmCmrEbXjcKChzUyImZOMkXDiqw8cvpOp/2PV5Adg
06O/nVsJ8dWO41P0jmP6P6fbtGbfYmbW0W5BjfIttep3Sp+dWOIrWcBAI+0tKIJF
PnlUkiaY4IBIqDfv8NZ5YBberOgOzW6sRBc4L0na4UU+Krk2U886UAb3LujEV0ls
YSEY1QSteDwsOoBrp+uvFRTp2InBuThs4pFsiv9kuXclVzDAGySj4dzp30d8tbQk
CAUw7C29C79Fv1C5qfPrmAESrciIxpg0X40KPMbp1ZWVbd4=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string trustAsiaSHA384Intermediate = """
-----BEGIN CERTIFICATE-----
MIID9zCCAt+gAwIBAgIQC965p4OR4AKrGlsyW0XrDzANBgkqhkiG9w0BAQwFADBh
MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
d3cuZGlnaWNlcnQuY29tMSAwHgYDVQQDExdEaWdpQ2VydCBHbG9iYWwgUm9vdCBD
QTAeFw0xODA0MjcxMjQyNTlaFw0yODA0MjcxMjQyNTlaMFoxCzAJBgNVBAYTAkNO
MSUwIwYDVQQKExxUcnVzdEFzaWEgVGVjaG5vbG9naWVzLCBJbmMuMSQwIgYDVQQD
ExtUcnVzdEFzaWEgRUNDIE9WIFRMUyBQcm8gQ0EwdjAQBgcqhkjOPQIBBgUrgQQA
IgNiAAQPIUn75M5BCQLKoPsSU2KTr3mDMh13usnAQ38XfKOzjXiyQ+W0inA7meYR
xS+XMQgvnbCigEsKj3ErPIzO68uC9V/KdqMaXWBJp85Ws9A4KL92NB4Okbn5dp6v
Qzy08PajggFeMIIBWjAdBgNVHQ4EFgQULdRyBx6HyIH/+LOvuexyH5p/3PwwHwYD
VR0jBBgwFoAUA95QNVbRTLtm8KPiGxvDl7I90VUwDgYDVR0PAQH/BAQDAgGGMB0G
A1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjASBgNVHRMBAf8ECDAGAQH/AgEA
MDcGCCsGAQUFBwEBBCswKTAnBggrBgEFBQcwAYYbaHR0cDovL29jc3AuZGlnaWNl
cnQtY24uY29tMEQGA1UdHwQ9MDswOaA3oDWGM2h0dHA6Ly9jcmwuZGlnaWNlcnQt
Y24uY29tL0RpZ2lDZXJ0R2xvYmFsUm9vdENBLmNybDBWBgNVHSAETzBNMDcGCWCG
SAGG/WwBATAqMCgGCCsGAQUFBwIBFhxodHRwczovL3d3dy5kaWdpY2VydC5jb20v
Q1BTMAgGBmeBDAECAjAIBgZngQwBAgMwDQYJKoZIhvcNAQEMBQADggEBACVRufYd
j81xUqngFCO+Pk8EYXie0pxHKsBZnOPygAyXKx+awUasKBAnHjmhoFPXaDGAP2oV
OeZTWgwnURVr6wUCuTkz2/8Tgl1egC7OrVcHSa0fIIhaVo9/zRA/hr31xMG7LFBk
GNd7jd06Up4f/UOGbcJsqJexc5QRcUeSwe1MiUDcTNiyCjZk74QCPdcfdFYM4xsa
SlUpboB5vyT7jFePZ2v95CKjcr0EhiQ0gwxpdgoipZdfYTiMFGxCLsk6v8pUv7Tq
PT/qadOGyC+PfLuZh1PtLp20mF06K+MzheCiv+w1NT5ofhmcObvukc68wvbvRFL6
rRzZxAYN36q1SX8=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string trustAsiaLeaf = """
-----BEGIN CERTIFICATE-----
MIIEwTCCBEegAwIBAgIQBOjomZfHfhgz2bVYZVuf2DAKBggqhkjOPQQDAzBaMQsw
CQYDVQQGEwJDTjElMCMGA1UEChMcVHJ1c3RBc2lhIFRlY2hub2xvZ2llcywgSW5j
LjEkMCIGA1UEAxMbVHJ1c3RBc2lhIEVDQyBPViBUTFMgUHJvIENBMB4XDTE5MDUx
NzAwMDAwMFoXDTIwMDcyODEyMDAwMFowgY0xCzAJBgNVBAYTAkNOMRIwEAYDVQQI
DAnnpo/lu7rnnIExEjAQBgNVBAcMCeWOpumXqOW4gjEqMCgGA1UECgwh5Y6m6Zeo
5Y+B546W5Y+B56eR5oqA5pyJ6ZmQ5YWs5Y+4MRgwFgYDVQQLDA/nn6Xor4bkuqfm
nYPpg6gxEDAOBgNVBAMMByoudG0uY24wWTATBgcqhkjOPQIBBggqhkjOPQMBBwNC
AARx/MDQ0oGnCLagQIzjIz57iqFYFmz4/W6gaU6N+GHBkzyvQU8aX02QkdlTTNYL
TCoGFJxHB0XlZVSxrqoIPlNKo4ICuTCCArUwHwYDVR0jBBgwFoAULdRyBx6HyIH/
+LOvuexyH5p/3PwwHQYDVR0OBBYEFGTyf5adc5smW8NvDZyummJwZRLEMBkGA1Ud
EQQSMBCCByoudG0uY26CBXRtLmNuMA4GA1UdDwEB/wQEAwIHgDAdBgNVHSUEFjAU
BggrBgEFBQcDAQYIKwYBBQUHAwIwRgYDVR0fBD8wPTA7oDmgN4Y1aHR0cDovL2Ny
bC5kaWdpY2VydC1jbi5jb20vVHJ1c3RBc2lhRUNDT1ZUTFNQcm9DQS5jcmwwTAYD
VR0gBEUwQzA3BglghkgBhv1sAQEwKjAoBggrBgEFBQcCARYcaHR0cHM6Ly93d3cu
ZGlnaWNlcnQuY29tL0NQUzAIBgZngQwBAgIwfgYIKwYBBQUHAQEEcjBwMCcGCCsG
AQUFBzABhhtodHRwOi8vb2NzcC5kaWdpY2VydC1jbi5jb20wRQYIKwYBBQUHMAKG
OWh0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LWNuLmNvbS9UcnVzdEFzaWFFQ0NPVlRM
U1Byb0NBLmNydDAMBgNVHRMBAf8EAjAAMIIBAwYKKwYBBAHWeQIEAgSB9ASB8QDv
AHUA7ku9t3XOYLrhQmkfq+GeZqMPfl+wctiDAMR7iXqo/csAAAFqxGMTnwAABAMA
RjBEAiAz13zKEoyqd4e/96SK/fxfjl7uR+xhfoDZeyA1BvtfOwIgTY+8nJMGekv8
leIVdW6AGh7oqH31CIGTAbNJJWzaSFYAdgCHdb/nWXz4jEOZX73zbv9WjUdWNv9K
tWDBtOr/XqCDDwAAAWrEYxTCAAAEAwBHMEUCIQDlWm7+limbRiurcqUwXav3NSmx
x/aMnolLbh6+f+b1XAIgQfinHwLw6pDr4R9UkndUsX8QFF4GXS3/IwRR8HCp+pIw
CgYIKoZIzj0EAwMDaAAwZQIwHg8JmjRtcq+OgV0vVmdVBPqehi1sQJ9PZ+51CG+Z
0GOu+2HwS/fyLRViwSc/MZoVAjEA7NgbgpPN4OIsZn2XjMGxemtVxGFS6ZR+1364
EEeHB9vhZAEjQSePAfjR9aAGhXRa
-----END CERTIFICATE-----
"""u8;

internal static readonly @string selfSigned = """
-----BEGIN CERTIFICATE-----
MIIC/DCCAeSgAwIBAgIRAK0SWRVmi67xU3z0gkgY+PkwDQYJKoZIhvcNAQELBQAw
EjEQMA4GA1UEChMHQWNtZSBDbzAeFw0xNjA4MTkxNjMzNDdaFw0xNzA4MTkxNjMz
NDdaMBIxEDAOBgNVBAoTB0FjbWUgQ28wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAw
ggEKAoIBAQDWkm1kdCwxyKEt6OTmZitkmLGH8cQu9z7rUdrhW8lWNm4kh2SuaUWP
pscBjda5iqg51aoKuWJR2rw6ElDne+X5eit2FT8zJgAU8v39lMFjbaVZfS9TFOYF
w0Tk0Luo/PyKJpZnwhsP++iiGQiteJbndy8aLKmJ2MpLfpDGIgxEIyNb5dgoDi0D
WReDCpE6K9WDYqvKVGnQ2Jvqqra6Gfx0tFkuqJxQuqA8aUOlPHcCH4KBZdNEoXdY
YL3E4dCAh0YiDs80wNZx4cHqEM3L8gTEFqW2Tn1TSuPZO6gjJ9QPsuUZVjaMZuuO
NVxqLGujZkDzARhC3fBpptMuaAfi20+BAgMBAAGjTTBLMA4GA1UdDwEB/wQEAwIF
oDATBgNVHSUEDDAKBggrBgEFBQcDATAMBgNVHRMBAf8EAjAAMBYGA1UdEQQPMA2C
C2Zvby5leGFtcGxlMA0GCSqGSIb3DQEBCwUAA4IBAQBPvvfnDhsHWt+/cfwdAVim
4EDn+hYOMkTQwU0pouYIvY8QXYkZ8MBxpBtBMK4JhFU+ewSWoBAEH2dCCvx/BDxN
UGTSJHMbsvJHcFvdmsvvRxOqQ/cJz7behx0cfoeHMwcs0/vWv8ms5wHesb5Ek7L0
pl01FCBGTcncVqr6RK1r4fTpeCCfRIERD+YRJz8TtPH6ydesfLL8jIV40H8NiDfG
vRAvOtNiKtPzFeQVdbRPOskC4rcHyPeiDAMAMixeLi63+CFty4da3r5lRezeedCE
cw3ESZzThBwWqvPOtJdpXdm+r57pDW8qD+/0lY8wfImMNkQAyCUCLg/1Lxt/hrBj
-----END CERTIFICATE-----
"""u8;

internal static readonly @string issuerSubjectMatchRoot = """
-----BEGIN CERTIFICATE-----
MIICIDCCAYmgAwIBAgIIAj5CwoHlWuYwDQYJKoZIhvcNAQELBQAwIzEPMA0GA1UE
ChMGR29sYW5nMRAwDgYDVQQDEwdSb290IGNhMB4XDTE1MDEwMTAwMDAwMFoXDTI1
MDEwMTAwMDAwMFowIzEPMA0GA1UEChMGR29sYW5nMRAwDgYDVQQDEwdSb290IGNh
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDpDn8RDOZa5oaDcPZRBy4CeBH1
siSSOO4mYgLHlPE+oXdqwI/VImi2XeJM2uCFETXCknJJjYG0iJdrt/yyRFvZTQZw
+QzGj+mz36NqhGxDWb6dstB2m8PX+plZw7jl81MDvUnWs8yiQ/6twgu5AbhWKZQD
JKcNKCEpqa6UW0r5nwIDAQABo10wWzAOBgNVHQ8BAf8EBAMCAgQwHQYDVR0lBBYw
FAYIKwYBBQUHAwEGCCsGAQUFBwMCMA8GA1UdEwEB/wQFMAMBAf8wGQYDVR0OBBIE
EEA31wH7QC+4HH5UBCeMWQEwDQYJKoZIhvcNAQELBQADgYEAb4TfSeCZ1HFmHTKG
VsvqWmsOAGrRWm4fBiMH/8vRGnTkJEMLqiqgc3Ulgry/P6n4SIis7TqUOw3TiMhn
RGEz33Fsxa/tFoy/gvlJu+MqB1M2NyV33pGkdwl/b7KRWMQFieqO+uE7Ge/49pS3
eyfm5ITdK/WT9TzYhsU4AVZcn20=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string issuerSubjectMatchLeaf = """
-----BEGIN CERTIFICATE-----
MIICODCCAaGgAwIBAgIJAOjwnT/iW+qmMA0GCSqGSIb3DQEBCwUAMCMxDzANBgNV
BAoTBkdvbGFuZzEQMA4GA1UEAxMHUm9vdCBDQTAeFw0xNTAxMDEwMDAwMDBaFw0y
NTAxMDEwMDAwMDBaMCAxDzANBgNVBAoTBkdvbGFuZzENMAsGA1UEAxMETGVhZjCB
nzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEA20Z9ky4SJwZIvAYoIat+xLaiXf4e
UkWIejZHpQgNkkJbwoHAvpd5mED7T20U/SsTi8KlLmfY1Ame1iI4t0oLdHMrwjTx
0ZPlltl0e/NYn2xhPMCwQdTZKyskI3dbHDu9dV3OIFTPoWOHHR4kxPMdGlCLqrYU
Q+2Xp3Vi9BTIUtcCAwEAAaN3MHUwDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQG
CCsGAQUFBwMBBggrBgEFBQcDAjAMBgNVHRMBAf8EAjAAMBkGA1UdDgQSBBCfkRYf
Q0M+SabebbaA159gMBsGA1UdIwQUMBKAEEA31wH7QC+4HH5UBCeMWQEwDQYJKoZI
hvcNAQELBQADgYEAjYYF2on1HcUWFEG5NIcrXDiZ49laW3pb3gtcCEUJbxydMV8I
ynqjmdqDCyK+TwI1kU5dXDe/iSJYfTB20i/QoO53nnfA1hnr7KBjNWqAm4AagN5k
vEA4PCJprUYmoj3q9MKSSRYDlq5kIbl87mSRR4GqtAwJKxIasvOvULOxziQ=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string x509v1TestRoot = """
-----BEGIN CERTIFICATE-----
MIICIDCCAYmgAwIBAgIIAj5CwoHlWuYwDQYJKoZIhvcNAQELBQAwIzEPMA0GA1UE
ChMGR29sYW5nMRAwDgYDVQQDEwdSb290IENBMB4XDTE1MDEwMTAwMDAwMFoXDTI1
MDEwMTAwMDAwMFowIzEPMA0GA1UEChMGR29sYW5nMRAwDgYDVQQDEwdSb290IENB
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDpDn8RDOZa5oaDcPZRBy4CeBH1
siSSOO4mYgLHlPE+oXdqwI/VImi2XeJM2uCFETXCknJJjYG0iJdrt/yyRFvZTQZw
+QzGj+mz36NqhGxDWb6dstB2m8PX+plZw7jl81MDvUnWs8yiQ/6twgu5AbhWKZQD
JKcNKCEpqa6UW0r5nwIDAQABo10wWzAOBgNVHQ8BAf8EBAMCAgQwHQYDVR0lBBYw
FAYIKwYBBQUHAwEGCCsGAQUFBwMCMA8GA1UdEwEB/wQFMAMBAf8wGQYDVR0OBBIE
EEA31wH7QC+4HH5UBCeMWQEwDQYJKoZIhvcNAQELBQADgYEAcIwqeNUpQr9cOcYm
YjpGpYkQ6b248xijCK7zI+lOeWN89zfSXn1AvfsC9pSdTMeDklWktbF/Ad0IN8Md
h2NtN34ard0hEfHc8qW8mkXdsysVmq6cPvFYaHz+dBtkHuHDoy8YQnC0zdN/WyYB
/1JmacUUofl+HusHuLkDxmadogI=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string x509v1TestIntermediate = """
-----BEGIN CERTIFICATE-----
MIIByjCCATMCCQCCdEMsT8ykqTANBgkqhkiG9w0BAQsFADAjMQ8wDQYDVQQKEwZH
b2xhbmcxEDAOBgNVBAMTB1Jvb3QgQ0EwHhcNMTUwMTAxMDAwMDAwWhcNMjUwMTAx
MDAwMDAwWjAwMQ8wDQYDVQQKEwZHb2xhbmcxHTAbBgNVBAMTFFguNTA5djEgaW50
ZXJtZWRpYXRlMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDJ2QyniAOT+5YL
jeinEBJr3NsC/Q2QJ/VKmgvp+xRxuKTHJiVmxVijmp0vWg8AWfkmuE4p3hXQbbqM
k5yxrk1n60ONhim2L4VXriEvCE7X2OXhTmBls5Ufr7aqIgPMikwjScCXwz8E8qI8
UxyAhnjeJwMYBU8TuwBImSd4LBHoQQIDAQABMA0GCSqGSIb3DQEBCwUAA4GBAIab
DRG6FbF9kL9jb/TDHkbVBk+sl/Pxi4/XjuFyIALlARgAkeZcPmL5tNW1ImHkwsHR
zWE77kJDibzd141u21ZbLsKvEdUJXjla43bdyMmEqf5VGpC3D4sFt3QVH7lGeRur
x5Wlq1u3YDL/j6s1nU2dQ3ySB/oP7J+vQ9V4QeM+
-----END CERTIFICATE-----
"""u8;

internal static readonly @string x509v1TestLeaf = """
-----BEGIN CERTIFICATE-----
MIICMzCCAZygAwIBAgIJAPo99mqJJrpJMA0GCSqGSIb3DQEBCwUAMDAxDzANBgNV
BAoTBkdvbGFuZzEdMBsGA1UEAxMUWC41MDl2MSBpbnRlcm1lZGlhdGUwHhcNMTUw
MTAxMDAwMDAwWhcNMjUwMTAxMDAwMDAwWjArMQ8wDQYDVQQKEwZHb2xhbmcxGDAW
BgNVBAMTD2Zvby5leGFtcGxlLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkC
gYEApUh60Z+a5/oKJxG//Dn8CihSo2CJHNIIO3zEJZ1EeNSMZCynaIR6D3IPZEIR
+RG2oGt+f5EEukAPYxwasp6VeZEezoQWJ+97nPCT6DpwLlWp3i2MF8piK2R9vxkG
Z5n0+HzYk1VM8epIrZFUXSMGTX8w1y041PX/yYLxbdEifdcCAwEAAaNaMFgwDgYD
VR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAMBgNV
HRMBAf8EAjAAMBkGA1UdDgQSBBBFozXe0SnzAmjy+1U6M/cvMA0GCSqGSIb3DQEB
CwUAA4GBADYzYUvaToO/ucBskPdqXV16AaakIhhSENswYVSl97/sODaxsjishKq9
5R7siu+JnIFotA7IbBe633p75xEnLN88X626N/XRFG9iScLzpj0o0PWXBUiB+fxL
/jt8qszOXCv2vYdUTPNuPqufXLWMoirpuXrr1liJDmedCcAHepY/
-----END CERTIFICATE-----
"""u8;

internal static readonly @string ignoreCNWithSANRoot = """
-----BEGIN CERTIFICATE-----
MIIDPzCCAiegAwIBAgIIJkzCwkNrPHMwDQYJKoZIhvcNAQELBQAwMDEQMA4GA1UE
ChMHVEVTVElORzEcMBoGA1UEAxMTKipUZXN0aW5nKiogUm9vdCBDQTAeFw0xNTAx
MDEwMDAwMDBaFw0yNTAxMDEwMDAwMDBaMDAxEDAOBgNVBAoTB1RFU1RJTkcxHDAa
BgNVBAMTEyoqVGVzdGluZyoqIFJvb3QgQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IB
DwAwggEKAoIBAQC4YAf5YqlXGcikvbMWtVrNICt+V/NNWljwfvSKdg4Inm7k6BwW
P6y4Y+n4qSYIWNU4iRkdpajufzctxQCO6ty13iw3qVktzcC5XBIiS6ymiRhhDgnY
VQqyakVGw9MxrPwdRZVlssUv3Hmy6tU+v5Ok31SLY5z3wKgYWvSyYs0b8bKNU8kf
2FmSHnBN16lxGdjhe3ji58F/zFMr0ds+HakrLIvVdFcQFAnQopM8FTHpoWNNzGU3
KaiO0jBbMFkd6uVjVnuRJ+xjuiqi/NWwiwQA+CEr9HKzGkxOF8nAsHamdmO1wW+w
OsCrC0qWQ/f5NTOVATTJe0vj88OMTvo3071VAgMBAAGjXTBbMA4GA1UdDwEB/wQE
AwICpDAdBgNVHSUEFjAUBggrBgEFBQcDAQYIKwYBBQUHAwIwDwYDVR0TAQH/BAUw
AwEB/zAZBgNVHQ4EEgQQQDfXAftAL7gcflQEJ4xZATANBgkqhkiG9w0BAQsFAAOC
AQEAGOn3XjxHyHbXLKrRmpwV447B7iNBXR5VlhwOgt1kWaHDL2+8f/9/h0HMkB6j
fC+/yyuYVqYuOeavqMGVrh33D2ODuTQcFlOx5lXukP46j3j+Lm0jjZ1qNX7vlP8I
VlUXERhbelkw8O4oikakwIY9GE8syuSgYf+VeBW/lvuAZQrdnPfabxe05Tre6RXy
nJHMB1q07YHpbwIkcV/lfCE9pig2nPXTLwYZz9cl46Ul5RCpPUi+IKURo3x8y0FU
aSLjI/Ya0zwUARMmyZ3RRGCyhIarPb20mKSaMf1/Nb23pS3k1QgmZhk5pAnXYsWu
BJ6bvwEAasFiLGP6Zbdmxb2hIA==
-----END CERTIFICATE-----
"""u8;

internal static readonly @string ignoreCNWithSANLeaf = """
-----BEGIN CERTIFICATE-----
MIIDaTCCAlGgAwIBAgIJAONakvRTxgJhMA0GCSqGSIb3DQEBCwUAMDAxEDAOBgNV
BAoTB1RFU1RJTkcxHDAaBgNVBAMTEyoqVGVzdGluZyoqIFJvb3QgQ0EwHhcNMTUw
MTAxMDAwMDAwWhcNMjUwMTAxMDAwMDAwWjAsMRAwDgYDVQQKEwdURVNUSU5HMRgw
FgYDVQQDEw9mb28uZXhhbXBsZS5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAw
ggEKAoIBAQDBqskp89V/JMIBBqcauKSOVLcMyIE/t0jgSWVrsI4sksBTabLsfMdS
ui2n+dHQ1dRBuw3o4g4fPrWwS3nMnV3pZUHEn2TPi5N1xkjTaxObXgKIY2GKmFP3
rJ9vYqHT6mT4K93kCHoRcmJWWySc7S3JAOhTcdB4G+tIdQJN63E+XRYQQfNrn5HZ
hxQoOzaguHFx+ZGSD4Ntk6BSZz5NfjqCYqYxe+iCpTpEEYhIpi8joSPSmkTMTxBW
S1W2gXbYNQ9KjNkGM6FnQsUJrSPMrWs4v3UB/U88N5LkZeF41SqD9ySFGwbGajFV
nyzj12+4K4D8BLhlOc0Eo/F/8GwOwvmxAgMBAAGjgYkwgYYwDgYDVR0PAQH/BAQD
AgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAMBgNVHRMBAf8EAjAA
MBkGA1UdDgQSBBCjeab27q+5pV43jBGANOJ1MBsGA1UdIwQUMBKAEEA31wH7QC+4
HH5UBCeMWQEwDwYDVR0RBAgwBocEfwAAATANBgkqhkiG9w0BAQsFAAOCAQEAGZfZ
ErTVxxpIg64s22mQpXSk/72THVQsfsKHzlXmztM0CJzH8ccoN67ZqKxJCfdiE/FI
Emb6BVV4cGPeIKpcxaM2dwX/Y+Y0JaxpQJvqLxs+EByRL0gPP3shgg86WWCjYLxv
AgOn862d/JXGDrC9vIlQ/DDQcyL5g0JV5UjG2G9TUigbnrXxBw7BoWK6wmoSaHnR
sZKEHSs3RUJvm7qqpA9Yfzm9jg+i9j32zh1xFacghAOmFRFXa9eCVeigZ/KK2mEY
j2kBQyvnyKsXHLAKUoUOpd6t/1PHrfXnGj+HmzZNloJ/BZ1kiWb4eLvMljoLGkZn
xZbqP3Krgjj4XNaXjg==
-----END CERTIFICATE-----
"""u8;

internal static readonly @string excludedNamesLeaf = """
-----BEGIN CERTIFICATE-----
MIID4DCCAsigAwIBAgIHDUSFtJknhzANBgkqhkiG9w0BAQsFADCBnjELMAkGA1UE
BhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExEjAQBgNVBAcMCUxvcyBHYXRvczEU
MBIGA1UECgwLTmV0ZmxpeCBJbmMxLTArBgNVBAsMJFBsYXRmb3JtIFNlY3VyaXR5
ICgzNzM0NTE1NTYyODA2Mzk3KTEhMB8GA1UEAwwYSW50ZXJtZWRpYXRlIENBIGZv
ciAzMzkyMB4XDTE3MDIwODIxMTUwNFoXDTE4MDIwODIwMjQ1OFowgZAxCzAJBgNV
BAYTAlVTMRMwEQYDVQQIDApDYWxpZm9ybmlhMRIwEAYDVQQHDAlMb3MgR2F0b3Mx
FDASBgNVBAoMC05ldGZsaXggSW5jMS0wKwYDVQQLDCRQbGF0Zm9ybSBTZWN1cml0
eSAoMzczNDUxNTc0ODUwMjY5NikxEzARBgNVBAMMCjE3Mi4xNi4wLjEwggEiMA0G
CSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCZ0oP1bMv6bOeqcKbzinnGpNOpenhA
zdFFsgea62znWsH3Wg4+1Md8uPCqlaQIsaJQKZHc50eKD3bg0Io7c6kxHkBQr1b8
Q7cGeK3CjdqG3NwS/aizzrLKOwL693hFwwy7JY7GGCvogbhyQRKn6iV0U9zMm7bu
/9pQVV/wx8u01u2uAlLttjyQ5LJkxo5t8cATFVqxdN5J9eY//VSDiTwXnlpQITBP
/Ow+zYuZ3kFlzH3CtCOhOEvNG3Ar1NvP3Icq35PlHV+Eki4otnKfixwByoiGpqCB
UEIY04VrZJjwBxk08y/3jY2B3VLYGgi+rryyCxIqkB7UpSNPMMWSG4UpAgMBAAGj
LzAtMAwGA1UdEwEB/wQCMAAwHQYDVR0RBBYwFIIMYmVuZGVyLmxvY2FshwSsEAAB
MA0GCSqGSIb3DQEBCwUAA4IBAQCLW3JO8L7LKByjzj2RciPjCGH5XF87Wd20gYLq
sNKcFwCIeyZhnQy5aZ164a5G9AIk2HLvH6HevBFPhA9Ivmyv/wYEfnPd1VcFkpgP
hDt8MCFJ8eSjCyKdtZh1MPMLrLVymmJV+Rc9JUUYM9TIeERkpl0rskcO1YGewkYt
qKlWE+0S16+pzsWvKn831uylqwIb8ANBPsCX4aM4muFBHavSWAHgRO+P+yXVw8Q+
VQDnMHUe5PbZd1/+1KKVs1K/CkBCtoHNHp1d/JT+2zUQJphwja9CcgfFdVhSnHL4
oEEOFtqVMIuQfR2isi08qW/JGOHc4sFoLYB8hvdaxKWSE19A
-----END CERTIFICATE-----
"""u8;

internal static readonly @string excludedNamesIntermediate = """
-----BEGIN CERTIFICATE-----
MIIDzTCCArWgAwIBAgIHDUSFqYeczDANBgkqhkiG9w0BAQsFADCBmTELMAkGA1UE
BhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExEjAQBgNVBAcMCUxvcyBHYXRvczEU
MBIGA1UECgwLTmV0ZmxpeCBJbmMxLTArBgNVBAsMJFBsYXRmb3JtIFNlY3VyaXR5
ICgzNzM0NTE1NDc5MDY0NjAyKTEcMBoGA1UEAwwTTG9jYWwgUm9vdCBmb3IgMzM5
MjAeFw0xNzAyMDgyMTE1MDRaFw0xODAyMDgyMDI0NThaMIGeMQswCQYDVQQGEwJV
UzETMBEGA1UECAwKQ2FsaWZvcm5pYTESMBAGA1UEBwwJTG9zIEdhdG9zMRQwEgYD
VQQKDAtOZXRmbGl4IEluYzEtMCsGA1UECwwkUGxhdGZvcm0gU2VjdXJpdHkgKDM3
MzQ1MTU1NjI4MDYzOTcpMSEwHwYDVQQDDBhJbnRlcm1lZGlhdGUgQ0EgZm9yIDMz
OTIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCOyEs6tJ/t9emQTvlx
3FS7uJSou5rKkuqVxZdIuYQ+B2ZviBYUnMRT9bXDB0nsVdKZdp0hdchdiwNXDG/I
CiWu48jkcv/BdynVyayOT+0pOJSYLaPYpzBx1Pb9M5651ct9GSbj6Tz0ChVonoIE
1AIZ0kkebucZRRFHd0xbAKVRKyUzPN6HJ7WfgyauUp7RmlC35wTmrmARrFohQLlL
7oICy+hIQePMy9x1LSFTbPxZ5AUUXVC3eUACU3vLClF/Xs8XGHebZpUXCdMQjOGS
nq1eFguFHR1poSB8uSmmLqm4vqUH9CDhEgiBAC8yekJ8//kZQ7lUEqZj3YxVbk+Y
E4H5AgMBAAGjEzARMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADggEB
ADxrnmNX5gWChgX9K5fYwhFDj5ofxZXAKVQk+WjmkwMcmCx3dtWSm++Wdksj/ZlA
V1cLW3ohWv1/OAZuOlw7sLf98aJpX+UUmIYYQxDubq+4/q7VA7HzEf2k/i/oN1NI
JgtrhpPcZ/LMO6k7DYx0qlfYq8pTSfd6MI4LnWKgLc+JSPJJjmvspgio2ZFcnYr7
A264BwLo6v1Mos1o1JUvFFcp4GANlw0XFiWh7JXYRl8WmS5DoouUC+aNJ3lmyF6z
LbIjZCSfgZnk/LK1KU1j91FI2bc2ULYZvAC1PAg8/zvIgxn6YM2Q7ZsdEgWw0FpS
zMBX1/lk4wkFckeUIlkD55Y=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string excludedNamesRoot = """
-----BEGIN CERTIFICATE-----
MIIEGTCCAwGgAwIBAgIHDUSFpInn/zANBgkqhkiG9w0BAQsFADCBozELMAkGA1UE
BhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExEjAQBgNVBAcMCUxvcyBHYXRvczEU
MBIGA1UECgwLTmV0ZmxpeCBJbmMxLTArBgNVBAsMJFBsYXRmb3JtIFNlY3VyaXR5
ICgzNzMxNTA5NDM3NDYyNDg1KTEmMCQGA1UEAwwdTmFtZSBDb25zdHJhaW50cyBU
ZXN0IFJvb3QgQ0EwHhcNMTcwMjA4MjExNTA0WhcNMTgwMjA4MjAyNDU4WjCBmTEL
MAkGA1UEBhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExEjAQBgNVBAcMCUxvcyBH
YXRvczEUMBIGA1UECgwLTmV0ZmxpeCBJbmMxLTArBgNVBAsMJFBsYXRmb3JtIFNl
Y3VyaXR5ICgzNzM0NTE1NDc5MDY0NjAyKTEcMBoGA1UEAwwTTG9jYWwgUm9vdCBm
b3IgMzM5MjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJymcnX29ekc
7+MLyr8QuAzoHWznmGdDd2sITwWRjM89/21cdlHCGKSpULUNdFp9HDLWvYECtxt+
8TuzKiQz7qAerzGUT1zI5McIjHy0e/i4xIkfiBiNeTCuB/N9QRbZlcfM80ErkaA4
gCAFK8qZAcWkHIl6e+KaQFMPLKk9kckgAnVDHEJe8oLNCogCJ15558b65g05p9eb
5Lg+E98hoPRTQaDwlz3CZPfTTA2EiEZInSi8qzodFCbTpJUVTbiVUH/JtVjlibbb
smdcx5PORK+8ZJkhLEh54AjaWOX4tB/7Tkk8stg2VBmrIARt/j4UVj7cTrIWU3bV
m8TwHJG+YgsCAwEAAaNaMFgwDwYDVR0TAQH/BAUwAwEB/zBFBgNVHR4EPjA8oBww
CocICgEAAP//AAAwDoIMYmVuZGVyLmxvY2FsoRwwCocICgEAAP//AAAwDoIMYmVu
ZGVyLmxvY2FsMA0GCSqGSIb3DQEBCwUAA4IBAQAMjbheffPxtSKSv9NySW+8qmHs
n7Mb5GGyCFu+cMZSoSaabstbml+zHEFJvWz6/1E95K4F8jKhAcu/CwDf4IZrSD2+
Hee0DolVSQhZpnHgPyj7ZATz48e3aJaQPUlhCEOh0wwF4Y0N4FV0t7R6woLylYRZ
yU1yRHUqUYpN0DWFpsPbBqgM6uUAVO2ayBFhPgWUaqkmSbZ/Nq7isGvknaTmcIwT
6mOAFN0qFb4RGzfGJW7x6z7KCULS7qVDp6fU3tRoScHFEgRubks6jzQ1W5ooSm4o
+NQCZDd5eFeU8PpNX7rgaYE4GPq+EEmLVCBYmdctr8QVdqJ//8Xu3+1phjDy
-----END CERTIFICATE-----
"""u8;

internal static readonly @string invalidCNRoot = """
-----BEGIN CERTIFICATE-----
MIIBFjCBvgIJAIsu4r+jb70UMAoGCCqGSM49BAMCMBQxEjAQBgNVBAsMCVRlc3Qg
cm9vdDAeFw0xODA3MTExODMyMzVaFw0yODA3MDgxODMyMzVaMBQxEjAQBgNVBAsM
CVRlc3Qgcm9vdDBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABF6oDgMg0LV6YhPj
QXaPXYCc2cIyCdqp0ROUksRz0pOLTc5iY2nraUheRUD1vRRneq7GeXOVNn7uXONg
oCGMjNwwCgYIKoZIzj0EAwIDRwAwRAIgDSiwgIn8g1lpruYH0QD1GYeoWVunfmrI
XzZZl0eW/ugCICgOfXeZ2GGy3wIC0352BaC3a8r5AAb2XSGNe+e9wNN6
-----END CERTIFICATE-----
"""u8;

internal static readonly @string validCNWithoutSAN = """
-----BEGIN CERTIFICATE-----
MIIBJzCBzwIUB7q8t9mrDAL+UB1OFaMN5BEWFKQwCgYIKoZIzj0EAwIwFDESMBAG
A1UECwwJVGVzdCByb290MB4XDTE4MDcxMTE4NDcyNFoXDTI4MDcwODE4NDcyNFow
GjEYMBYGA1UEAwwPZm9vLmV4YW1wbGUuY29tMFkwEwYHKoZIzj0CAQYIKoZIzj0D
AQcDQgAEp6Z8IjOnR38Iky1fYTUu2kVndvKXcxiwARJKGtW3b0E8uwVp9AZd/+sr
p4ULTPdFToFAeqnGHbu62bkms8pQkDAKBggqhkjOPQQDAgNHADBEAiBTbNe3WWFR
cqUYo0sNUuoV+tCTMDJUS+0PWIW4qBqCOwIgFHdLDn5PCk9kJpfc0O2qZx03hdq0
h7olHCpY9yMRiz0=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string rootWithoutSKID = """
-----BEGIN CERTIFICATE-----
MIIBbzCCARSgAwIBAgIQeCkq3C8SOX/JM5PqYTl9cDAKBggqhkjOPQQDAjASMRAw
DgYDVQQKEwdBY21lIENvMB4XDTE5MDIwNDIyNTYzNFoXDTI5MDIwMTIyNTYzNFow
EjEQMA4GA1UEChMHQWNtZSBDbzBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABISm
jGlTr4dLOWT+BCTm2PzWRjk1DpLcSAh+Al8eB1Nc2eBWxYIH9qPirfatvqBOA4c5
ZwycRpFoaw6O+EmXnVujTDBKMA4GA1UdDwEB/wQEAwICpDATBgNVHSUEDDAKBggr
BgEFBQcDATAPBgNVHRMBAf8EBTADAQH/MBIGA1UdEQQLMAmCB2V4YW1wbGUwCgYI
KoZIzj0EAwIDSQAwRgIhAMaBYWFCjTfn0MNyQ0QXvYT/iIFompkIqzw6wB7qjLrA
AiEA3sn65V7G4tsjZEOpN0Jykn9uiTjqniqn/S/qmv8gIec=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string leafWithAKID = """
-----BEGIN CERTIFICATE-----
MIIBjTCCATSgAwIBAgIRAPCKYvADhKLPaWOtcTu2XYwwCgYIKoZIzj0EAwIwEjEQ
MA4GA1UEChMHQWNtZSBDbzAeFw0xOTAyMDQyMzA2NTJaFw0yOTAyMDEyMzA2NTJa
MBMxETAPBgNVBAoTCEFjbWUgTExDMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE
Wk5N+/8X97YT6ClFNIE5/4yc2YwKn921l0wrIJEcT2u+Uydm7EqtCJNtZjYMAnBd
Acp/wynpTwC6tBTsxcM0s6NqMGgwDgYDVR0PAQH/BAQDAgWgMBMGA1UdJQQMMAoG
CCsGAQUFBwMBMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoAUwitfkXg0JglCjW9R
ssWvTAveakIwEgYDVR0RBAswCYIHZXhhbXBsZTAKBggqhkjOPQQDAgNHADBEAiBk
4LpWiWPOIl5PIhX9PDVkmjpre5oyoH/3aYwG8ABYuAIgCeSfbYueOOG2AdXuMqSU
ZZMqeJS7JldLx91sPUArY5A=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string rootMatchingSKIDMismatchingSubject = """
-----BEGIN CERTIFICATE-----
MIIBQjCB6aADAgECAgEAMAoGCCqGSM49BAMCMBExDzANBgNVBAMTBlJvb3QgQTAe
Fw0wOTExMTAyMzAwMDBaFw0xOTExMDgyMzAwMDBaMBExDzANBgNVBAMTBlJvb3Qg
QTBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABPK4p1uXq2aAeDtKDHIokg2rTcPM
2gq3N9Y96wiW6/7puBK1+INEW//cO9x6FpzkcsHw/TriAqy4sck/iDAvf9WjMjAw
MA8GA1UdJQQIMAYGBFUdJQAwDwYDVR0TAQH/BAUwAwEB/zAMBgNVHQ4EBQQDAQID
MAoGCCqGSM49BAMCA0gAMEUCIQDgtAp7iVHxMnKxZPaLQPC+Tv2r7+DJc88k2SKH
MPs/wQIgFjjNvBoQEl7vSHTcRGCCcFMdlN4l0Dqc9YwGa9fyrQs=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string rootMismatchingSKIDMatchingSubject = """
-----BEGIN CERTIFICATE-----
MIIBNDCB26ADAgECAgEAMAoGCCqGSM49BAMCMBExDzANBgNVBAMTBlJvb3QgQjAe
Fw0wOTExMTAyMzAwMDBaFw0xOTExMDgyMzAwMDBaMBExDzANBgNVBAMTBlJvb3Qg
QjBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABI1YRFcIlkWzm9BdEVrIsEQJ2dT6
qiW8/WV9GoIhmDtX9SEDHospc0Cgm+TeD2QYW2iMrS5mvNe4GSw0Jezg/bOjJDAi
MA8GA1UdJQQIMAYGBFUdJQAwDwYDVR0TAQH/BAUwAwEB/zAKBggqhkjOPQQDAgNI
ADBFAiEAukWOiuellx8bugRiwCS5XQ6IOJ1SZcjuZxj76WojwxkCIHqa71qNw8FM
DtA5yoL9M2pDFF6ovFWnaCe+KlzSwAW/
-----END CERTIFICATE-----
"""u8;

internal static readonly @string leafMatchingAKIDMatchingIssuer = """
-----BEGIN CERTIFICATE-----
MIIBNTCB26ADAgECAgEAMAoGCCqGSM49BAMCMBExDzANBgNVBAMTBlJvb3QgQjAe
Fw0wOTExMTAyMzAwMDBaFw0xOTExMDgyMzAwMDBaMA8xDTALBgNVBAMTBExlYWYw
WTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAASNWERXCJZFs5vQXRFayLBECdnU+qol
vP1lfRqCIZg7V/UhAx6LKXNAoJvk3g9kGFtojK0uZrzXuBksNCXs4P2zoyYwJDAO
BgNVHSMEBzAFgAMBAgMwEgYDVR0RBAswCYIHZXhhbXBsZTAKBggqhkjOPQQDAgNJ
ADBGAiEAnV9XV7a4h0nfJB8pWv+pBUXRlRFA2uZz3mXEpee8NYACIQCWa+wL70GL
ePBQCV1F9sE2q4ZrnsT9TZoNrSe/bMDjzA==
-----END CERTIFICATE-----
"""u8;


partial struct unknownAuthorityErrorTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string cert;
    internal @string expected;
}
internal static slice<unknownAuthorityErrorTestsᴛ1> unknownAuthorityErrorTests;
internal static void initᴛunknownAuthorityErrorTests() { unknownAuthorityErrorTests = new unknownAuthorityErrorTestsᴛ1[]{
    new("self-signed, cn"u8, selfSignedWithCommonName, "x509: certificate signed by unknown authority (possibly because of \"empty\" while trying to verify candidate authority certificate \"test\")"u8),
    new("self-signed, no cn, org"u8, selfSignedNoCommonNameWithOrgName, "x509: certificate signed by unknown authority (possibly because of \"empty\" while trying to verify candidate authority certificate \"ca\")"u8),
    new("self-signed, no cn, no org"u8, selfSignedNoCommonNameNoOrgName, "x509: certificate signed by unknown authority (possibly because of \"empty\" while trying to verify candidate authority certificate \"serial:0\")"u8)
}.slice(); }

public static void TestUnknownAuthorityError(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in unknownAuthorityErrorTests) {
        ref var tt = ref heap(new unknownAuthorityErrorTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (der, _) = pem.Decode(slice<byte>(ttʗ1.cert));
            if (der == nil) {
                tΔ1.Fatalf("#%d: Unable to decode PEM block"u8, i);
            }
            var (c, err) = ParseCertificate((~der).Bytes);
            if (err != default!) {
                tΔ1.Fatalf("#%d: Unable to parse certificate -> %v"u8, i, err);
            }
            var uae = Ꮡ(new UnknownAuthorityError(
                Cert: c,
                hintErr: fmt.Errorf("empty"u8),
                hintCert: c
            ));
            @string actual = (~uae).Error();
            if (actual != ttʗ1.expected) {
                tΔ1.Errorf("#%d: UnknownAuthorityError.Error() response invalid actual: %s expected: %s"u8, i, actual, ttʗ1.expected);
            }
        });
    }
}


partial struct nameConstraintTestsᴛ1 /*dyn*/ {
    internal @string constraint, domain;
    internal bool expectError;
    internal bool shouldMatch;
}
internal static slice<nameConstraintTestsᴛ1> nameConstraintTests = new nameConstraintTestsᴛ1[]{
    new(""u8, "anything.com"u8, false, true),
    new("example.com"u8, "example.com"u8, false, true),
    new("example.com."u8, "example.com"u8, true, false),
    new("example.com"u8, "example.com."u8, true, false),
    new("example.com"u8, "ExAmPle.coM"u8, false, true),
    new("example.com"u8, "exampl1.com"u8, false, false),
    new("example.com"u8, "www.ExAmPle.coM"u8, false, true),
    new("example.com"u8, "sub.www.ExAmPle.coM"u8, false, true),
    new("example.com"u8, "notexample.com"u8, false, false),
    new(".example.com"u8, "example.com"u8, false, false),
    new(".example.com"u8, "www.example.com"u8, false, true),
    new(".example.com"u8, "www..example.com"u8, true, false)
}.slice();

public static void TestNameConstraints(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, test) in nameConstraintTests) {
        var (result, err) = matchDomainConstraint(test.domain, test.constraint, false, new map<@string, slice<@string>>{}, new map<@string, slice<@string>>{});
        if (err != default! && !test.expectError) {
            Ꮡt.Errorf("unexpected error for test #%d: domain=%s, constraint=%s, err=%s"u8, i, test.domain, test.constraint, err);
            continue;
        }
        if (err == default! && test.expectError) {
            Ꮡt.Errorf("unexpected success for test #%d: domain=%s, constraint=%s"u8, i, test.domain, test.constraint);
            continue;
        }
        if (result != test.shouldMatch) {
            Ꮡt.Errorf("unexpected result for test #%d: domain=%s, constraint=%s, result=%t"u8, i, test.domain, test.constraint, result);
        }
    }
}

internal static readonly @string selfSignedWithCommonName = """
-----BEGIN CERTIFICATE-----
MIIDCjCCAfKgAwIBAgIBADANBgkqhkiG9w0BAQsFADAaMQswCQYDVQQKEwJjYTEL
MAkGA1UEAxMCY2EwHhcNMTYwODI4MTcwOTE4WhcNMjEwODI3MTcwOTE4WjAcMQsw
CQYDVQQKEwJjYTENMAsGA1UEAxMEdGVzdDCCASIwDQYJKoZIhvcNAQEBBQADggEP
ADCCAQoCggEBAOH55PfRsbvmcabfLLko1w/yuapY/hk13Cgmc3WE/Z1ZStxGiVxY
gQVH9n4W/TbUsrep/TmcC4MV7xEm5252ArcgaH6BeQ4QOTFj/6Jx0RT7U/ix+79x
8RRysf7OlzNpGIctwZEM7i/G+0ZfqX9ULxL/EW9tppSxMX1jlXZQarnU7BERL5cH
+G2jcbU9H28FXYishqpVYE9L7xrXMm61BAwvGKB0jcVW6JdhoAOSfQbbgp7JjIlq
czXqUsv1UdORO/horIoJptynTvuARjZzyWatya6as7wyOgEBllE6BjPK9zpn+lp3
tQ8dwKVqm/qBPhIrVqYG/Ec7pIv8mJfYabMCAwEAAaNZMFcwDgYDVR0PAQH/BAQD
AgOoMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATAMBgNVHRMBAf8EAjAA
MAoGA1UdDgQDBAEAMAwGA1UdIwQFMAOAAQAwDQYJKoZIhvcNAQELBQADggEBAAAM
XMFphzq4S5FBcRdB2fRrmcoz+jEROBWvIH/1QUJeBEBz3ZqBaJYfBtQTvqCA5Rjw
dxyIwVd1W3q3aSulM0tO62UCU6L6YeeY/eq8FmpD7nMJo7kCrXUUAMjxbYvS3zkT
v/NErK6SgWnkQiPJBZNX1Q9+aSbLT/sbaCTdbWqcGNRuLGJkmqfIyoxRt0Hhpqsx
jP5cBaVl50t4qoCuVIE9cOucnxYXnI7X5HpXWvu8Pfxo4SwVjb1az8Fk5s8ZnxGe
fPB6Q3L/pKBe0SEe5GywpwtokPLB3lAygcuHbxp/1FlQ1NQZqq+vgXRIla26bNJf
IuYkJwt6w+LH/9HZgf8=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string selfSignedNoCommonNameWithOrgName = """
-----BEGIN CERTIFICATE-----
MIIC+zCCAeOgAwIBAgIBADANBgkqhkiG9w0BAQsFADAaMQswCQYDVQQKEwJjYTEL
MAkGA1UEAxMCY2EwHhcNMTYwODI4MTgxMzQ4WhcNMjEwODI3MTgxMzQ4WjANMQsw
CQYDVQQKEwJjYTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAL5EjrUa
7EtOMxWiIgTzp2FlQvncPsG329O3l3uNGnbigb8TmNMw2M8UhoDjd84pnU5RAfqd
8t5TJyw/ybnIKBN131Q2xX+gPQ0dFyMvcO+i1CUgCxmYZomKVA2MXO1RD1hLTYGS
gOVjc3no3MBwd8uVQp0NStqJ1QvLtNG4Uy+B28qe+ZFGGbjGqx8/CU4A8Szlpf7/
xAZR8w5qFUUlpA2LQYeHHJ5fQVXw7kyL1diNrKNi0G3qcY0IrBh++hT+hnEEXyXu
g8a0Ux18hoE8D6rAr34rCZl6AWfqW5wjwm+N5Ns2ugr9U4N8uCKJYMPHb2CtdubU
46IzVucpTfGLdaMCAwEAAaNZMFcwDgYDVR0PAQH/BAQDAgOoMB0GA1UdJQQWMBQG
CCsGAQUFBwMCBggrBgEFBQcDATAMBgNVHRMBAf8EAjAAMAoGA1UdDgQDBAEAMAwG
A1UdIwQFMAOAAQAwDQYJKoZIhvcNAQELBQADggEBAEn5SgVpJ3zjsdzPqK7Qd/sB
bYd1qtPHlrszjhbHBg35C6mDgKhcv4o6N+fuC+FojZb8lIxWzJtvT9pQbfy/V6u3
wOb816Hm71uiP89sioIOKCvSAstj/p9doKDOUaKOcZBTw0PS2m9eja8bnleZzBvK
rD8cNkHf74v98KvBhcwBlDifVzmkWzMG6TL1EkRXUyLKiWgoTUFSkCDV927oXXMR
DKnszq+AVw+K8hbeV2A7GqT7YfeqOAvSbatTDnDtKOPmlCnQui8A149VgZzXv7eU
29ssJSqjUPyp58dlV6ZuynxPho1QVZUOQgnJToXIQ3/5vIvJRXy52GJCs4/Gh/w=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string selfSignedNoCommonNameNoOrgName = """
-----BEGIN CERTIFICATE-----
MIIC7jCCAdagAwIBAgIBADANBgkqhkiG9w0BAQsFADAaMQswCQYDVQQKEwJjYTEL
MAkGA1UEAxMCY2EwHhcNMTYwODI4MTgxOTQ1WhcNMjEwODI3MTgxOTQ1WjAAMIIB
IjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAp3E+Jl6DpgzogHUW/i/AAcCM
fnNJLOamNVKFGmmxhb4XTHxRaWoTzrlsyzIMS0WzivvJeZVe6mWbvuP2kZanKgIz
35YXRTR9HbqkNTMuvnpUESzWxbGWE2jmt2+a/Jnz89FS4WIYRhF7nI2z8PvZOfrI
2gETTT2tEpoF2S4soaYfm0DBeT8K0/rogAaf+oeUS6V+v3miRcAooJgpNJGu9kqm
S0xKPn1RCFVjpiRd6YNS0xZirjYQIBMFBvoSoHjaOdgJptNRBprYPOxVJ/ItzGf0
kPmzPFCx2tKfxV9HLYBPgxi+fP3IIx8aIYuJn8yReWtYEMYU11hDPeAFN5Gm+wID
AQABo1kwVzAOBgNVHQ8BAf8EBAMCA6gwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsG
AQUFBwMBMAwGA1UdEwEB/wQCMAAwCgYDVR0OBAMEAQAwDAYDVR0jBAUwA4ABADAN
BgkqhkiG9w0BAQsFAAOCAQEATZVOFeiCpPM5QysToLv+8k7Rjoqt6L5IxMUJGEpq
4ENmldmwkhEKr9VnYEJY3njydnnTm97d9vOfnLj9nA9wMBODeOO3KL2uJR2oDnmM
9z1NSe2aQKnyBb++DM3ZdikpHn/xEpGV19pYKFQVn35x3lpPh2XijqRDO/erKemb
w67CoNRb81dy+4Q1lGpA8ORoLWh5fIq2t2eNGc4qB8vlTIKiESzAwu7u3sRfuWQi
4R+gnfLd37FWflMHwztFbVTuNtPOljCX0LN7KcuoXYlr05RhQrmoN7fQHsrZMNLs
8FVjHdKKu+uPstwd04Uy4BR/H2y1yerN9j/L6ZkMl98iiA==
-----END CERTIFICATE-----
"""u8;

internal static readonly @string criticalExtRoot = """
-----BEGIN CERTIFICATE-----
MIIBqzCCAVGgAwIBAgIJAJ+mI/85cXApMAoGCCqGSM49BAMCMB0xDDAKBgNVBAoT
A09yZzENMAsGA1UEAxMEUm9vdDAeFw0xNTAxMDEwMDAwMDBaFw0yNTAxMDEwMDAw
MDBaMB0xDDAKBgNVBAoTA09yZzENMAsGA1UEAxMEUm9vdDBZMBMGByqGSM49AgEG
CCqGSM49AwEHA0IABJGp9joiG2QSQA+1FczEDAsWo84rFiP3GTL+n+ugcS6TyNib
gzMsdbJgVi+a33y0SzLZxB+YvU3/4KTk8yKLC+2jejB4MA4GA1UdDwEB/wQEAwIC
BDAdBgNVHSUEFjAUBggrBgEFBQcDAQYIKwYBBQUHAwIwDwYDVR0TAQH/BAUwAwEB
/zAZBgNVHQ4EEgQQQDfXAftAL7gcflQEJ4xZATAbBgNVHSMEFDASgBBAN9cB+0Av
uBx+VAQnjFkBMAoGCCqGSM49BAMCA0gAMEUCIFeSV00fABFceWR52K+CfIgOHotY
FizzGiLB47hGwjMuAiEA8e0um2Kr8FPQ4wmFKaTRKHMaZizCGl3m+RG5QsE1KWo=
-----END CERTIFICATE-----
"""u8;

internal static readonly @string criticalExtIntermediate = """
-----BEGIN CERTIFICATE-----
MIIBszCCAVmgAwIBAgIJAL2kcGZKpzVqMAoGCCqGSM49BAMCMB0xDDAKBgNVBAoT
A09yZzENMAsGA1UEAxMEUm9vdDAeFw0xNTAxMDEwMDAwMDBaFw0yNTAxMDEwMDAw
MDBaMCUxDDAKBgNVBAoTA09yZzEVMBMGA1UEAxMMSW50ZXJtZWRpYXRlMFkwEwYH
KoZIzj0CAQYIKoZIzj0DAQcDQgAESqVq92iPEq01cL4o99WiXDc5GZjpjNlzMS1n
rk8oHcVDp4tQRRQG3F4A6dF1rn/L923ha3b0fhDLlAvXZB+7EKN6MHgwDgYDVR0P
AQH/BAQDAgIEMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAPBgNVHRMB
Af8EBTADAQH/MBkGA1UdDgQSBBCMGmiotXbbXVd7H40UsgajMBsGA1UdIwQUMBKA
EEA31wH7QC+4HH5UBCeMWQEwCgYIKoZIzj0EAwIDSAAwRQIhAOhhNRb6KV7h3wbE
cdap8bojzvUcPD78fbsQPCNw1jPxAiBOeAJhlTwpKn9KHpeJphYSzydj9NqcS26Y
xXbdbm27KQ==
-----END CERTIFICATE-----
"""u8;

internal static readonly @string criticalExtLeafWithExt = """
-----BEGIN CERTIFICATE-----
MIIBxTCCAWugAwIBAgIJAJZAUtw5ccb1MAoGCCqGSM49BAMCMCUxDDAKBgNVBAoT
A09yZzEVMBMGA1UEAxMMSW50ZXJtZWRpYXRlMB4XDTE1MDEwMTAwMDAwMFoXDTI1
MDEwMTAwMDAwMFowJDEMMAoGA1UEChMDT3JnMRQwEgYDVQQDEwtleGFtcGxlLmNv
bTBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABF3ABa2+B6gUyg6ayCaRQWYY/+No
6PceLqEavZNUeVNuz7bS74Toy8I7R3bGMkMgbKpLSPlPTroAATvebTXoBaijgYQw
gYEwDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMBBggrBgEFBQcD
AjAMBgNVHRMBAf8EAjAAMBkGA1UdDgQSBBBRNtBL2vq8nCV3qVp7ycxMMBsGA1Ud
IwQUMBKAEIwaaKi1dttdV3sfjRSyBqMwCgYDUQMEAQH/BAAwCgYIKoZIzj0EAwID
SAAwRQIgVjy8GBgZFiagexEuDLqtGjIRJQtBcf7lYgf6XFPH1h4CIQCT6nHhGo6E
I+crEm4P5q72AnA/Iy0m24l7OvLuXObAmg==
-----END CERTIFICATE-----
"""u8;

internal static readonly @string criticalExtIntermediateWithExt = """
-----BEGIN CERTIFICATE-----
MIIB2TCCAX6gAwIBAgIIQD3NrSZtcUUwCgYIKoZIzj0EAwIwHTEMMAoGA1UEChMD
T3JnMQ0wCwYDVQQDEwRSb290MB4XDTE1MDEwMTAwMDAwMFoXDTI1MDEwMTAwMDAw
MFowPTEMMAoGA1UEChMDT3JnMS0wKwYDVQQDEyRJbnRlcm1lZGlhdGUgd2l0aCBD
cml0aWNhbCBFeHRlbnNpb24wWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAAQtnmzH
mcRm10bdDBnJE7xQEJ25cLCL5okuEphRR0Zneo6+nQZikoh+UBbtt5GV3Dms7LeP
oF5HOplYDCd8wi/wo4GHMIGEMA4GA1UdDwEB/wQEAwICBDAdBgNVHSUEFjAUBggr
BgEFBQcDAQYIKwYBBQUHAwIwDwYDVR0TAQH/BAUwAwEB/zAZBgNVHQ4EEgQQKxdv
UuQZ6sO3XvBsxgNZ3zAbBgNVHSMEFDASgBBAN9cB+0AvuBx+VAQnjFkBMAoGA1ED
BAEB/wQAMAoGCCqGSM49BAMCA0kAMEYCIQCQzTPd6XKex+OAPsKT/1DsoMsg8vcG
c2qZ4Q0apT/kvgIhAKu2TnNQMIUdcO0BYQIl+Uhxc78dc9h4lO+YJB47pHGx
-----END CERTIFICATE-----
"""u8;

internal static readonly @string criticalExtLeaf = """
-----BEGIN CERTIFICATE-----
MIIBzzCCAXWgAwIBAgIJANoWFIlhCI9MMAoGCCqGSM49BAMCMD0xDDAKBgNVBAoT
A09yZzEtMCsGA1UEAxMkSW50ZXJtZWRpYXRlIHdpdGggQ3JpdGljYWwgRXh0ZW5z
aW9uMB4XDTE1MDEwMTAwMDAwMFoXDTI1MDEwMTAwMDAwMFowJDEMMAoGA1UEChMD
T3JnMRQwEgYDVQQDEwtleGFtcGxlLmNvbTBZMBMGByqGSM49AgEGCCqGSM49AwEH
A0IABG1Lfh8A0Ho2UvZN5H0+ONil9c8jwtC0y0xIZftyQE+Fwr9XwqG3rV2g4M1h
GnJa9lV9MPHg8+b85Hixm0ZSw7SjdzB1MA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUE
FjAUBggrBgEFBQcDAQYIKwYBBQUHAwIwDAYDVR0TAQH/BAIwADAZBgNVHQ4EEgQQ
UNhY4JhezH9gQYqvDMWrWDAbBgNVHSMEFDASgBArF29S5Bnqw7de8GzGA1nfMAoG
CCqGSM49BAMCA0gAMEUCIQClA3d4tdrDu9Eb5ZBpgyC+fU1xTZB0dKQHz6M5fPZA
2AIgN96lM+CPGicwhN24uQI6flOsO3H0TJ5lNzBYLtnQtlc=
-----END CERTIFICATE-----
"""u8;

internal partial struct TestValidHostname_tests /*dyn*/ {
    internal @string host;
    internal bool validInput, validPattern;
}

public static void TestValidHostname(ж<testing.T> Ꮡt) {
    var tests = new TestValidHostname_tests[]{
        new(host: "example.com"u8, validInput: true, validPattern: true),
        new(host: "eXample123-.com"u8, validInput: true, validPattern: true),
        new(host: "-eXample123-.com"u8),
        new(host: ""u8),
        new(host: "."u8),
        new(host: "example..com"u8),
        new(host: ".example.com"u8),
        new(host: "example.com."u8, validInput: true),
        new(host: "*.example.com."u8),
        new(host: "*.example.com"u8, validPattern: true),
        new(host: "*foo.example.com"u8),
        new(host: "foo.*.example.com"u8),
        new(host: "exa_mple.com"u8, validInput: true, validPattern: true),
        new(host: "foo,bar"u8),
        new(host: "project-dev:us-central1:main"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            var got = validHostnamePattern(tt.host); if (got != tt.validPattern) {
                Ꮡt.Errorf("validHostnamePattern(%q) = %v, want %v"u8, tt.host, got, tt.validPattern);
            }
        }
        {
            var got = validHostnameInput(tt.host); if (got != tt.validInput) {
                Ꮡt.Errorf("validHostnameInput(%q) = %v, want %v"u8, tt.host, got, tt.validInput);
            }
        }
    }
}

internal static (ж<global::go.crypto.x509_package.Certificate>, cryptoꓸPrivateKey, error) generateCert(@string cn, bool isCA, ж<global::go.crypto.x509_package.Certificate> Ꮡissuer, cryptoꓸPrivateKey issuerKey) {
    ref var issuer = ref Ꮡissuer.DerefOrNull();

    var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        return (default!, default!, err);
    }
    var serialNumberLimit = @new<bigꓸInt>().Lsh(big.NewInt(1), 128);
    var (serialNumber, _) = rand.Int(rand.Reader, serialNumberLimit);
    var template = Ꮡ(new Certificate(
        SerialNumber: serialNumber,
        Subject: new pkix.Name(CommonName: cn),
        NotBefore: time.Now().Add((time.Duration)(-3600000000000L)),
        NotAfter: time.Now().Add((time.Duration)(86400000000000L)),
        KeyUsage: (global::go.crypto.x509_package.KeyUsage)((global::go.crypto.x509_package.KeyUsage)(KeyUsageKeyEncipherment | KeyUsageDigitalSignature) | KeyUsageCertSign),
        ExtKeyUsage: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
        BasicConstraintsValid: true,
        IsCA: isCA
    ));
    if (Ꮡissuer == nil) {
        Ꮡissuer = template; issuer = ref Ꮡissuer.DerefOrNull();
        issuerKey = priv.OrTypedNil();
    }
    (var derBytes, err) = CreateCertificate(rand.Reader, template, Ꮡissuer, priv.Public(), issuerKey);
    if (err != default!) {
        return (default!, default!, err);
    }
    (var cert, err) = ParseCertificate(derBytes);
    if (err != default!) {
        return (default!, default!, err);
    }
    return (cert, priv.OrTypedNil(), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingGenerationOfAˢ = (@string)"skipping generation of a long chain of certificates in short mode"u8;
internal static readonly @string rootCaˢ = "Root CA"u8;
internal static readonly @string intermediateCaˢ = "Intermediate CA"u8;
internal static readonly @string leafˢ = "Leaf"u8;
internal static readonly @string signatureCheckAttemptsˢ = "signature check attempts limit"u8;

public static void TestPathologicalChain(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingGenerationOfAˢ);
    }
    // Build a chain where all intermediates share the same subject, to hit the
    // path building worst behavior.
    var (roots, intermediates) = (NewCertPool(), NewCertPool());
    var (parent, parentKey, err) = generateCert(rootCaˢ, true, nil, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    roots.AddCert(parent);
    for (nint i = 1; i < 100; i++) {
        (parent, parentKey, err) = generateCert(intermediateCaˢ, true, parent, parentKey);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        intermediates.AddCert(parent);
    }
    (var leaf, _, err) = generateCert(leafˢ, false, parent, parentKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var start = time.Now();
    (_, err) = leaf.Verify(new VerifyOptions(
        Roots: roots,
        Intermediates: intermediates
    ));
    Ꮡt.Logf("verification took %v"u8, time.Since(start));
    if (err == default! || !strings.Contains(err.Error(), signatureCheckAttemptsˢ)) {
        Ꮡt.Errorf("expected verification to fail with a signature checks limit error; got %v"u8, err);
    }
}

public static void TestLongChain(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingGenerationOfAˢ);
    }
    var (roots, intermediates) = (NewCertPool(), NewCertPool());
    var (parent, parentKey, err) = generateCert(rootCaˢ, true, nil, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    roots.AddCert(parent);
    for (nint i = 1; i < 15; i++) {
        @string name = fmt.Sprintf("Intermediate CA #%d"u8, i);
        (parent, parentKey, err) = generateCert(name, true, parent, parentKey);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        intermediates.AddCert(parent);
    }
    (var leaf, _, err) = generateCert(leafˢ, false, parent, parentKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var start = time.Now();
    {
        var (_, errΔ1) = leaf.Verify(new VerifyOptions(
            Roots: roots,
            Intermediates: intermediates
        )); if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
        }
    }
    Ꮡt.Logf("verification took %v"u8, time.Since(start));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object windowsAndDarwinDoNotUseˢ = (@string)"Windows and darwin do not use (or support) systemRoots"u8;

public static void TestSystemRootsError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "windows"u8 || runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8) {
            Ꮡt.Skip(windowsAndDarwinDoNotUseˢ);
        }
        defer((ж<global::go.crypto.x509_package.CertPool> oldSystemRoots) => {
            systemRoots = oldSystemRoots;
        }, systemRootsPool(), ref ᒐ);
        var opts = new VerifyOptions(
            Intermediates: NewCertPool(),
            DNSName: "www.google.com"u8,
            CurrentTime: time.Unix(1677615892, 0)
        );
        {
            var ok = opts.Intermediates.AppendCertsFromPEM(slice<byte>(gtsIntermediate)); if (!ok) {
                Ꮡt.Fatalf("failed to parse intermediate"u8);
            }
        }
        var (leaf, err) = certificateFromPEM(googleLeaf);
        if (err != default!) {
            Ꮡt.Fatalf("failed to parse leaf: %v"u8, err);
        }
        systemRoots = default!;
        (_, err) = leaf.Verify(opts);
        {
            var (_, ok) = err._<SystemRootsError>(ᐧ); if (!ok) {
                Ꮡt.Errorf("error was not SystemRootsError: %v"u8, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string err1ˢ = "err1"u8;
internal static readonly object errorsIsFailedWantedˢ = (@string)"errors.Is failed, wanted success"u8;

public static void TestSystemRootsErrorUnwrap(ж<testing.T> Ꮡt) {
    error err1 = errors.New(err1ˢ);
    var err = new SystemRootsError(Err: err1);
    if (!errors.Is(err, err1)) {
        Ꮡt.Error(errorsIsFailedWantedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string swVersˢ = "sw_vers"u8;
internal static readonly @string productVersionˢ = "-productVersion"u8;

internal static (nint, error) macosMajorVersion(ж<testing.T> Ꮡt) {
    var cmd = testenv.Command(new x509_test_package.testing_TжTB(Ꮡt), swVersˢ, productVersionˢ);
    var (@out, err) = cmd.Output();
    if (err != default!) {
        {
            var (ee, okΔ1) = err._<ж<exec.ExitError>>(ᐧ); if (okΔ1 && builtin.len((~ee).Stderr) > 0) {
                return (0, fmt.Errorf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, (~ee).Stderr));
            }
        }
        return (0, fmt.Errorf("%v: %v"u8, cmd.OrTypedNil(), err));
    }
    var (before, _, ok) = strings.Cut(((@string)@out), "."u8);
    (var major, err) = strconv.Atoi(before);
    if (!ok || err != default!) {
        return (0, fmt.Errorf("%v: unexpected output: %q"u8, cmd.OrTypedNil(), @out));
    }
    return (major, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object onlyAffectsDarwinˢ = (@string)"only affects darwin"u8;
internal static readonly object unableToDetermineMacOSˢ = (@string)"unable to determine macOS version"u8;
internal static readonly object behaviorOnlyEnforcedInˢ = (@string)"behavior only enforced in macOS 11 and after"u8;
internal static readonly @string leafˢ2 = "leaf"u8;
internal static readonly @string invalidLeafCertificateˢ = "invalid leaf certificate"u8;
internal static readonly @string intermediateˢ = "intermediate"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string badCertDataᶜ = ((@string)(new byte[]{0x30, 0x82, 0x01, 0x55, 0x30, 0x82, 0x01, 0x07, 0xa0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x01, 0x02, 0x30, 0x05, 0x06, 0x03, 0x2b, 0x65, 0x70, 0x30, 0x52, 0x31, 0x50, 0x30, 0x4e, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x47, 0x64, 0x65, 0x72, 0x70, 0x6b, 0x65, 0x79, 0x38, 0x64, 0x63, 0x35, 0x38, 0x31, 0x30, 0x30, 0x62, 0x32, 0x34, 0x39, 0x33, 0x36, 0x31, 0x34, 0x65, 0x65, 0x31, 0x36, 0x39, 0x32, 0x38, 0x33, 0x31, 0x61, 0x34, 0x36, 0x31, 0x66, 0x33, 0x66, 0x34, 0x64, 0x64, 0x33, 0x66, 0x39, 0x62, 0x33, 0x62, 0x30, 0x38, 0x38, 0x65, 0x32, 0x34, 0x34, 0x66, 0x38, 0x38, 0x37, 0x66, 0x38, 0x31, 0x62, 0x34, 0x39, 0x30, 0x36, 0x61, 0x63, 0x32, 0x36, 0x30, 0x1e, 0x17, 0x0d, 0x32, 0x32, 0x30, 0x31, 0x31, 0x32, 0x32, 0x33, 0x35, 0x37, 0x35, 0x35, 0x5a, 0x17, 0x0d, 0x32, 0x32, 0x30, 0x33, 0x31, 0x33, 0x32, 0x33, 0x35, 0x37, 0x35, 0x35, 0x5a, 0x30, 0x52, 0x31, 0x50, 0x30, 0x4e, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x47, 0x64, 0x65, 0x72, 0x70, 0x6b, 0x65, 0x79, 0x38, 0x64, 0x63, 0x35, 0x38, 0x31, 0x30, 0x30, 0x62, 0x32, 0x34, 0x39, 0x33, 0x36, 0x31, 0x34, 0x65, 0x65, 0x31, 0x36, 0x39, 0x32, 0x38, 0x33, 0x31, 0x61, 0x34, 0x36, 0x31, 0x66, 0x33, 0x66, 0x34, 0x64, 0x64, 0x33, 0x66, 0x39, 0x62, 0x33, 0x62, 0x30, 0x38, 0x38, 0x65, 0x32, 0x34, 0x34, 0x66, 0x38, 0x38, 0x37, 0x66, 0x38, 0x31, 0x62, 0x34, 0x39, 0x30, 0x36, 0x61, 0x63, 0x32, 0x36, 0x30, 0x2a, 0x30, 0x05, 0x06, 0x03, 0x2b, 0x65, 0x70, 0x03, 0x21, 0x00, 0x62, 0x41, 0xd8, 0x65, 0xad, 0x57, 0xcb, 0xef, 0x5a, 0x89, 0xb5, 0x22, 0x1e, 0x52, 0x9d, 0xba, 0x0e, 0x3a, 0x10, 0x34, 0x32, 0x51, 0x40, 0x7f, 0xbd, 0xfb, 0x7b, 0x6b, 0x73, 0x04, 0xd1, 0xc2, 0xa3, 0x02, 0x30, 0x00, 0x30, 0x05, 0x06, 0x03, 0x2b, 0x65, 0x70, 0x03, 0x41, 0x00, 0x5b, 0xa7, 0x06, 0x79, 0x86, 0x28, 0x94, 0x97, 0x9e, 0x4c, 0x77, 0x41, 0x00, 0x01, 0x78, 0xaa, 0xbc, 0xbd, 0x20, 0xc3, 0x8a, 0x5d, 0x0a, 0x28, 0xce, 0x85, 0x21, 0xd9, 0x81, 0x30, 0xf5, 0x9a, 0x25, 0x49, 0x19, 0x3c, 0xff, 0x6f, 0xf1, 0xea, 0x61, 0x66, 0x40, 0xb1, 0xa7, 0xaf, 0xfd, 0xe9, 0x52, 0xc7, 0x0f, 0x8d, 0x26, 0xd5, 0xfc, 0x0f, 0x3b, 0xcf, 0x98, 0x82, 0x84, 0x61, 0xbc, 0x0d}));

public static void TestIssue51759(ж<testing.T> Ꮡt) {
    if (runtime.GOOS != "darwin"u8) {
        Ꮡt.Skip(onlyAffectsDarwinˢ);
    }
    testenv.MustHaveExecPath(new x509_test_package.testing_TжTB(Ꮡt), swVersˢ);
    {
        var (vers, errΔ1) = macosMajorVersion(Ꮡt); if (errΔ1 != default!){
            {
                @string builder = testenv.Builder(); if (builder != ""u8){
                    Ꮡt.Fatalf("unable to determine macOS version: %s"u8, errΔ1);
                } else {
                    Ꮡt.Skip(unableToDetermineMacOSˢ);
                }
            }
        } else 
        if (vers < 11) {
            Ꮡt.Skip(behaviorOnlyEnforcedInˢ);
        }
    }
    // badCertData contains a cert that we parse as valid
    // but that macOS SecCertificateCreateWithData rejects.
    @string badCertData = badCertDataᶜ;
    ref var err = ref heap<error>(out var Ꮡerr);
    (var badCert, err) = ParseCertificate(slice<byte>(badCertData));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var badCertʗ1 = badCert;
    Ꮡt.Run(leafˢ2, (ж<testing.T> tΔ1) => {
        var opts = new VerifyOptions(nil);
        @string expectedErr = invalidLeafCertificateˢ;
        (_, Ꮡerr.ValueSlot) = badCertʗ1.Verify(opts);
        if (Ꮡerr.ValueSlot == default! || Ꮡerr.ValueSlot.Error() != expectedErr) {
            tΔ1.Fatalf("unexpected error: want %q, got %q"u8, expectedErr, Ꮡerr.ValueSlot);
        }
    });
    (var goodCert, err) = certificateFromPEM(googleLeaf);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var badCertʗ2 = badCert;
    var goodCertʗ1 = goodCert;
    Ꮡt.Run(intermediateˢ, (ж<testing.T> tΔ2) => {
        var opts = new VerifyOptions(
            Intermediates: NewCertPool()
        );
        opts.Intermediates.AddCert(badCertʗ2);
        @string expectedErr = "SecCertificateCreateWithData: invalid certificate"u8;
        (_, Ꮡerr.ValueSlot) = goodCertʗ1.Verify(opts);
        if (Ꮡerr.ValueSlot == default! || Ꮡerr.ValueSlot.Error() != expectedErr) {
            tΔ2.Fatalf("unexpected error: want %q, got %q"u8, expectedErr, Ꮡerr.ValueSlot);
        }
    });
}

public partial struct trustGraphEdge {
    public @string Issuer;
    public @string Subject;
    public nint Type;
    public Action<ж<global::go.crypto.x509_package.Certificate>> MutateTemplate;
    public Func<slice<ж<global::go.crypto.x509_package.Certificate>>, error> Constraint;
}

public partial struct rootDescription {
    public @string Subject;
    public Action<ж<global::go.crypto.x509_package.Certificate>> MutateTemplate;
    public Func<slice<ж<global::go.crypto.x509_package.Certificate>>, error> Constraint;
}

internal partial struct trustGraphDescription {
    public slice<rootDescription> Roots;
    public @string Leaf;
    public slice<trustGraphEdge> Graph;
}

internal static ж<global::go.crypto.x509_package.Certificate> genCertEdge(ж<testing.T> Ꮡt, @string subject, crypto.Signer key, Action<ж<global::go.crypto.x509_package.Certificate>> mutateTmpl, nint certType, ж<global::go.crypto.x509_package.Certificate> Ꮡissuer, crypto.Signer signer) {
    ref var issuer = ref Ꮡissuer.DerefOrNull();

    Ꮡt.Helper();
    var (serial, err) = rand.Int(rand.Reader, big.NewInt(100));
    if (err != default!) {
        Ꮡt.Fatalf("failed to generate test serial: %s"u8, err);
    }
    var tmpl = Ꮡ(new Certificate(
        SerialNumber: serial,
        Subject: new pkix.Name(CommonName: subject),
        NotBefore: time.Now().Add(-time.ΔHour),
        NotAfter: time.Now().Add(time.ΔHour)
    ));
    if (certType == rootCertificate || certType == intermediateCertificate){
        (tmpl.Value.IsCA, tmpl.Value.BasicConstraintsValid) = (true, true);
        tmpl.Value.KeyUsage = KeyUsageCertSign;
    } else 
    if (certType == leafCertificate) {
        tmpl.Value.DNSNames = new @string[]{"localhost"u8}.slice();
    }
    if (mutateTmpl != default!) {
        mutateTmpl(tmpl);
    }
    if (certType == rootCertificate) {
        Ꮡissuer = tmpl; issuer = ref Ꮡissuer.DerefOrNull();
        signer = key;
    }
    (var d, err) = CreateCertificate(rand.Reader, tmpl, Ꮡissuer, key.Public(), signer);
    if (err != default!) {
        Ꮡt.Fatalf("failed to generate test cert: %s"u8, err);
    }
    (var c, err) = ParseCertificate(d);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse test cert: %s"u8, err);
    }
    return c;
}

internal static (ж<global::go.crypto.x509_package.CertPool>, ж<global::go.crypto.x509_package.CertPool>, ж<global::go.crypto.x509_package.Certificate>) buildTrustGraph(ж<testing.T> Ꮡt, trustGraphDescription d) {
    Ꮡt.Helper();
    var certs = new map<@string, ж<global::go.crypto.x509_package.Certificate>>{};
    var keys = new map<@string, crypto.Signer>{};
    var rootPool = NewCertPool();
    foreach (var (_, r) in d.Roots) {
        var (k, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
        if (err != default!) {
            Ꮡt.Fatalf("failed to generate test key: %s"u8, err);
        }
        var root = genCertEdge(Ꮡt, r.Subject, new x509_test_package.ecdsa_PrivateKeyжSigner(k), r.MutateTemplate, rootCertificate, nil, default!);
        if (r.Constraint != default!){
            rootPool.AddCertWithConstraint(root, r.Constraint);
        } else {
            rootPool.AddCert(root);
        }
        certs[r.Subject] = root;
        keys[r.Subject] = new x509_test_package.ecdsa_PrivateKeyжSigner(k);
    }
    var intermediatePool = NewCertPool();
    ж<global::go.crypto.x509_package.Certificate> leaf = default!;
    foreach (var (_, e) in d.Graph) {
        var (issuerCert, ok) = certs[e.Issuer, ꟷ];
        if (!ok) {
            Ꮡt.Fatalf("unknown issuer %s"u8, e.Issuer);
        }
        (var issuerKey, ok) = keys[e.Issuer, ꟷ];
        if (!ok) {
            Ꮡt.Fatalf("unknown issuer %s"u8, e.Issuer);
        }
        (var k, ok) = keys[e.Subject, ꟷ];
        if (!ok) {
            error err = default!;
            var (ᴛ1, ᴛ2) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
            (k, err) = (new x509_test_package.ecdsa_PrivateKeyжSigner(ᴛ1), ᴛ2);
            if (err != default!) {
                Ꮡt.Fatalf("failed to generate test key: %s"u8, err);
            }
            keys[e.Subject] = k;
        }
        var cert = genCertEdge(Ꮡt, e.Subject, k, e.MutateTemplate, e.Type, issuerCert, issuerKey);
        certs[e.Subject] = cert;
        if (e.Subject == d.Leaf){
            leaf = cert;
        } else {
            if (e.Constraint != default!){
                intermediatePool.AddCertWithConstraint(cert, e.Constraint);
            } else {
                intermediatePool.AddCert(cert);
            }
        }
    }
    return (rootPool, intermediatePool, leaf);
}

internal static slice<@string> chainsToStrings(slice<slice<ж<global::go.crypto.x509_package.Certificate>>> chains) {
    var chainStrings = new @string[]{}.slice();
    foreach (var (_, chain) in chains) {
        var names = new @string[]{}.slice();
        foreach (var (_, c) in chain) {
            names = append(names, (~c).Subject.String());
        }
        chainStrings = append(chainStrings, strings.Join(names, " -> "u8));
    }
    slices.Sort<slice<@string>, @string>(chainStrings);
    return chainStrings;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badˢ = "bad"u8;

internal partial struct TestPathBuilding_tests /*dyn*/ {
    internal @string name;
    internal trustGraphDescription graph;
    internal slice<@string> expectedChains;
    internal @string expectedErr;
}

public static void TestPathBuilding(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestPathBuilding_tests[]{
        new(
            name: "bad EKU"u8, // Build the following graph from RFC 4158, figure 7 (note that in this graph edges represent
 // certificates where the parent is the issuer and the child is the subject.) For the certificate
 // C->B, use an unsupported ExtKeyUsage (in this case ExtKeyUsageCodeSigning) which invalidates
 // the path Trust Anchor -> C -> B -> EE. The remaining valid paths should be:
 //   * Trust Anchor -> A -> B -> EE
 //   * Trust Anchor -> C -> A -> B -> EE
 //
 //     +---------+
 //     |  Trust  |
 //     | Anchor  |
 //     +---------+
 //      |       |
 //      v       v
 //   +---+    +---+
 //   | A |<-->| C |
 //   +---+    +---+
 //    |         |
 //    |  +---+  |
 //    +->| B |<-+
 //       +---+
 //         |
 //         v
 //       +----+
 //       | EE |
 //       +----+

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "root"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ1) => {
                            tΔ1.Value.ExtKeyUsage = new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageCodeSigning}.slice();
                        }
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter b"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate
                    )
                }.slice()
            ),
            expectedChains: new @string[]{
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=inter c -> CN=root"u8,
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=root"u8
            }.slice()
        ),
        new(
            name: "bad EKU"u8, // Build the following graph from RFC 4158, figure 7 (note that in this graph edges represent
 // certificates where the parent is the issuer and the child is the subject.) For the certificate
 // C->B, use a unconstrained SAN which invalidates the path Trust Anchor -> C -> B -> EE. The
 // remaining valid paths should be:
 //   * Trust Anchor -> A -> B -> EE
 //   * Trust Anchor -> C -> A -> B -> EE
 //
 //     +---------+
 //     |  Trust  |
 //     | Anchor  |
 //     +---------+
 //      |       |
 //      v       v
 //   +---+    +---+
 //   | A |<-->| C |
 //   +---+    +---+
 //    |         |
 //    |  +---+  |
 //    +->| B |<-+
 //       +---+
 //         |
 //         v
 //       +----+
 //       | EE |
 //       +----+

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "root"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ2) => {
                            tΔ2.Value.PermittedDNSDomains = new @string[]{"good"u8}.slice();
                            tΔ2.Value.DNSNames = new @string[]{"bad"u8}.slice();
                        }
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter b"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate
                    )
                }.slice()
            ),
            expectedChains: new @string[]{
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=inter c -> CN=root"u8,
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=root"u8
            }.slice()
        ),
        new(
            name: "all paths"u8, // Build the following graph, we should find both paths:
 //   * Trust Anchor -> A -> C -> EE
 //   * Trust Anchor -> A -> B -> C -> EE
 //
 //	       +---------+
 //	       |  Trust  |
 //	       | Anchor  |
 //	       +---------+
 //	            |
 //	            v
 //	          +---+
 //	          | A |
 //	          +---+
 //	           | |
 //	           | +----+
 //	           |      v
 //	           |    +---+
 //	           |    | B |
 //	           |    +---+
 //	           |      |
 //	           |  +---v
 //	           v  v
 //            +---+
 //            | C |
 //            +---+
 //              |
 //              v
 //            +----+
 //            | EE |
 //            +----+

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter b"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate
                    )
                }.slice()
            ),
            expectedChains: new @string[]{
                "CN=leaf -> CN=inter c -> CN=inter a -> CN=root"u8,
                "CN=leaf -> CN=inter c -> CN=inter b -> CN=inter a -> CN=root"u8
            }.slice()
        ),
        new(
            name: "ignore cross-sig loops"u8, // Build the following graph, which contains a cross-signature loop
 // (A and C cross sign each other). Paths that include the A -> C -> A
 // (and vice versa) loop should be ignored, resulting in the paths:
 //   * Trust Anchor -> A -> B -> EE
 //   * Trust Anchor -> C -> B -> EE
 //   * Trust Anchor -> A -> C -> B -> EE
 //   * Trust Anchor -> C -> A -> B -> EE
 //
 //     +---------+
 //     |  Trust  |
 //     | Anchor  |
 //     +---------+
 //      |       |
 //      v       v
 //   +---+    +---+
 //   | A |<-->| C |
 //   +---+    +---+
 //    |         |
 //    |  +---+  |
 //    +->| B |<-+
 //       +---+
 //         |
 //         v
 //       +----+
 //       | EE |
 //       +----+

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "root"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter b"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate
                    )
                }.slice()
            ),
            expectedChains: new @string[]{
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=inter c -> CN=root"u8,
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=root"u8,
                "CN=leaf -> CN=inter b -> CN=inter c -> CN=inter a -> CN=root"u8,
                "CN=leaf -> CN=inter b -> CN=inter c -> CN=root"u8
            }.slice()
        ),
        new(
            name: "leaf with same subject, key, as parent but with SAN"u8, // Build a simple two node graph, where the leaf is directly issued from
 // the root and both certificates have matching subject and public key, but
 // the leaf has SANs.

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "root"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "root"u8,
                        Type: leafCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> c) => {
                            c.Value.DNSNames = new @string[]{"localhost"u8}.slice();
                        }
                    )
                }.slice()
            ),
            expectedChains: new @string[]{
                "CN=root -> CN=root"u8
            }.slice()
        ),
        new(
            name: "ignore invalid EKU path"u8, // Build a basic graph with two paths from leaf to root, but the path passing
 // through C should be ignored, because it has invalid EKU nesting.

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "root"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ3) => {
                            tΔ3.Value.ExtKeyUsage = new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageCodeSigning}.slice();
                        }
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ4) => {
                            tΔ4.Value.ExtKeyUsage = new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice();
                        }
                    ),
                    new(
                        Issuer: "inter b"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ5) => {
                            tΔ5.Value.ExtKeyUsage = new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice();
                        }
                    )
                }.slice()
            ),
            expectedChains: new @string[]{
                "CN=leaf -> CN=inter b -> CN=inter a -> CN=root"u8
            }.slice()
        ),
        new(
            name: "constrained root, invalid intermediate"u8, // A name constraint on the root should apply to any names that appear
 // on the intermediate, meaning there is no valid chain.

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{
                    new(
                        Subject: "root"u8,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ6) => {
                            tΔ6.Value.PermittedDNSDomains = new @string[]{"example.com"u8}.slice();
                        }
                    )
                }.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter"u8,
                        Type: intermediateCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ7) => {
                            tΔ7.Value.DNSNames = new @string[]{"beep.com"u8}.slice();
                        }
                    ),
                    new(
                        Issuer: "inter"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ8) => {
                            tΔ8.Value.DNSNames = new @string[]{"www.example.com"u8}.slice();
                        }
                    )
                }.slice()
            ),
            expectedErr: "x509: a root or intermediate certificate is not authorized to sign for this name: DNS name \"beep.com\" is not permitted by any constraint"u8
        ),
        new(
            name: "constrained intermediate, non-matching SAN"u8, // A name constraint on the intermediate does not apply to the intermediate
 // itself, so this is a valid chain.

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8)}.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter"u8,
                        Type: intermediateCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ9) => {
                            tΔ9.Value.DNSNames = new @string[]{"beep.com"u8}.slice();
                            tΔ9.Value.PermittedDNSDomains = new @string[]{"example.com"u8}.slice();
                        }
                    ),
                    new(
                        Issuer: "inter"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate,
                        MutateTemplate: (ж<global::go.crypto.x509_package.Certificate> tΔ10) => {
                            tΔ10.Value.DNSNames = new @string[]{"www.example.com"u8}.slice();
                        }
                    )
                }.slice()
            ),
            expectedChains: new @string[]{"CN=leaf -> CN=inter -> CN=root"u8}.slice()
        ),
        new(
            name: "code constrained root, two paths, one valid"u8, // A code constraint on the root, applying to one of two intermediates in the graph, should
 // result in only one valid chain.

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8, Constraint: error (slice<ж<global::go.crypto.x509_package.Certificate>> chain) => {
                    foreach (var (_, c) in chain) {
                        if ((~c).Subject.CommonName == "inter a"u8) {
                            return errors.New(badˢ);
                        }
                    }
                    return default!;
                })
                }.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter a"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "root"u8,
                        Subject: "inter b"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter a"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter b"u8,
                        Subject: "inter c"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter c"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate
                    )
                }.slice()
            ),
            expectedChains: new @string[]{"CN=leaf -> CN=inter c -> CN=inter b -> CN=root"u8}.slice()
        ),
        new(
            name: "code constrained root, one invalid path"u8, // A code constraint on the root, applying to the only path, should result in an error.

            graph: new trustGraphDescription(
                Roots: new rootDescription[]{new(Subject: "root"u8, Constraint: error (slice<ж<global::go.crypto.x509_package.Certificate>> chain) => {
                    foreach (var (_, c) in chain) {
                        if ((~c).Subject.CommonName == "leaf"u8) {
                            return errors.New(badˢ);
                        }
                    }
                    return default!;
                })
                }.slice(),
                Leaf: "leaf"u8,
                Graph: new trustGraphEdge[]{
                    new(
                        Issuer: "root"u8,
                        Subject: "inter"u8,
                        Type: intermediateCertificate
                    ),
                    new(
                        Issuer: "inter"u8,
                        Subject: "leaf"u8,
                        Type: leafCertificate
                    )
                }.slice()
            ),
            expectedErr: "x509: certificate signed by unknown authority (possibly because of \"bad\" while trying to verify candidate authority certificate \"root\")"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestPathBuilding_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ11) => {
            var (roots, intermediates, leaf) = buildTrustGraph(tΔ11, tcʗ1.graph);
            var (chains, err) = leaf.Verify(new VerifyOptions(
                Roots: roots,
                Intermediates: intermediates
            ));
            if (err != default! && err.Error() != tcʗ1.expectedErr) {
                tΔ11.Fatalf("unexpected error: got %q, want %q"u8, err, tcʗ1.expectedErr);
            }
            if (builtin.len(tcʗ1.expectedChains) == 0) {
                return;
            }
            var gotChains = chainsToStrings(chains);
            if (!slices.Equal<slice<@string>, @string>(gotChains, tcʗ1.expectedChains)) {
                tΔ11.Errorf("unexpected chains returned:\ngot:\n\t%s\nwant:\n\t%s"u8, strings.Join(gotChains, "\n\t"u8), strings.Join(tcʗ1.expectedChains, "\n\t"u8));
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootˢ = "root"u8;

internal partial struct TestEKUEnforcement_ekuDescs /*dyn*/ {
    public slice<global::go.crypto.x509_package.ExtKeyUsage> EKUs;
    public slice<asn1.ObjectIdentifier> Unknown;
}

internal partial struct TestEKUEnforcement_tests /*dyn*/ {
    internal @string name;
    internal TestEKUEnforcement_ekuDescs root;
    internal slice<TestEKUEnforcement_ekuDescs> inters;
    internal TestEKUEnforcement_ekuDescs leaf;
    internal slice<global::go.crypto.x509_package.ExtKeyUsage> verifyEKUs;
    internal @string err;
}

public static void TestEKUEnforcement(ж<testing.T> Ꮡt) {
    var tests = new TestEKUEnforcement_tests[]{
        new(
            name: "valid, full chain"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice())}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()
        ),
        new(
            name: "valid, only leaf has EKU"u8,
            root: new TestEKUEnforcement_ekuDescs(nil),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(nil)}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()
        ),
        new(
            name: "invalid, serverAuth not nested"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageClientAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice())}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            err: "x509: certificate specifies an incompatible key usage"u8
        ),
        new(
            name: "valid, two EKUs, one path"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice())}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice()
        ),
        new(
            name: "invalid, ladder"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{
                new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice()),
                new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageClientAuth}.slice()),
                new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice()),
                new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice())
            }.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice(),
            err: "x509: certificate specifies an incompatible key usage"u8
        ),
        new(
            name: "valid, intermediate has no EKU"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(nil)}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()
        ),
        new(
            name: "invalid, intermediate has no EKU and no nested path"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageClientAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(nil)}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth, ExtKeyUsageClientAuth}.slice(),
            err: "x509: certificate specifies an incompatible key usage"u8
        ),
        new(
            name: "invalid, intermediate has unknown EKU"u8,
            root: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            inters: new TestEKUEnforcement_ekuDescs[]{new TestEKUEnforcement_ekuDescs(Unknown: new asn1.ObjectIdentifier[]{new nint[]{1, 2, 3}.slice()}.slice())}.slice(),
            leaf: new TestEKUEnforcement_ekuDescs(EKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            err: "x509: certificate specifies an incompatible key usage"u8
        )
    }.slice();
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        Ꮡt.Fatalf("failed to generate test key: %s"u8, err);
    }
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestEKUEnforcement_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var kʗ1 = k;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var rootPool = NewCertPool();
            var tcʗ2 = tcʗ1;
            var root = genCertEdge(tΔ1, rootˢ, new x509_test_package.ecdsa_PrivateKeyжSigner(kʗ1), (ж<global::go.crypto.x509_package.Certificate> c) => {
                c.Value.ExtKeyUsage = tcʗ2.root.EKUs;
                c.Value.UnknownExtKeyUsage = tcʗ2.root.Unknown;
            }, rootCertificate, nil, new x509_test_package.ecdsa_PrivateKeyжSigner(kʗ1));
            rootPool.AddCert(root);
            var parent = root;
            var interPool = NewCertPool();
            foreach (var (i, vᴛ2) in tcʗ1.inters) {
                ref var interEKUs = ref heap(new TestEKUEnforcement_ekuDescs(), out var ᏑinterEKUs);
                interEKUs = vᴛ2;

                var interEKUsʗ1 = interEKUs;
                var inter = genCertEdge(tΔ1, fmt.Sprintf("inter %d"u8, i), new x509_test_package.ecdsa_PrivateKeyжSigner(kʗ1), (ж<global::go.crypto.x509_package.Certificate> c) => {
                    c.Value.ExtKeyUsage = interEKUsʗ1.EKUs;
                    c.Value.UnknownExtKeyUsage = interEKUsʗ1.Unknown;
                }, intermediateCertificate, parent, new x509_test_package.ecdsa_PrivateKeyжSigner(kʗ1));
                interPool.AddCert(inter);
                parent = inter;
            }
            var tcʗ3 = tcʗ1;
            var leaf = genCertEdge(tΔ1, leafˢ2, new x509_test_package.ecdsa_PrivateKeyжSigner(kʗ1), (ж<global::go.crypto.x509_package.Certificate> c) => {
                c.Value.ExtKeyUsage = tcʗ3.leaf.EKUs;
                c.Value.UnknownExtKeyUsage = tcʗ3.leaf.Unknown;
            }, intermediateCertificate, parent, new x509_test_package.ecdsa_PrivateKeyжSigner(kʗ1));
            var (_, errΔ1) = leaf.Verify(new VerifyOptions(Roots: rootPool, Intermediates: interPool, KeyUsages: tcʗ1.verifyEKUs));
            if (errΔ1 == default! && tcʗ1.err != ""u8){
                tΔ1.Errorf("expected error"u8);
            } else 
            if (errΔ1 != default! && errΔ1.Error() != tcʗ1.err) {
                tΔ1.Errorf("unexpected error: got %q, want %q"u8, errΔ1.Error(), tcʗ1.err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object verificationSucceedˢ = (@string)"verification succeed"u8;

internal partial struct TestVerifyEKURootAsLeaf_type /*dyn*/ {
    internal slice<global::go.crypto.x509_package.ExtKeyUsage> rootEKUs;
    internal slice<global::go.crypto.x509_package.ExtKeyUsage> verifyEKUs;
    internal bool succeed;
}

public static void TestVerifyEKURootAsLeaf(ж<testing.T> Ꮡt) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        Ꮡt.Fatalf("failed to generate key: %s"u8, err);
    }
    foreach (var (_, vᴛ1) in new TestVerifyEKURootAsLeaf_type[]{
        new(
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            succeed: true
        ),
        new(
            rootEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            succeed: true
        ),
        new(
            rootEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            succeed: true
        ),
        new(
            rootEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageAny}.slice(),
            succeed: true
        ),
        new(
            rootEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageAny}.slice(),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            succeed: true
        ),
        new(
            rootEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageClientAuth}.slice(),
            verifyEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
            succeed: false
        )
    }.slice()) {
        ref var tc = ref heap(new TestVerifyEKURootAsLeaf_type(), out var Ꮡtc);
        tc = vᴛ1;

        var kʗ1 = k;
        var tcʗ1 = tc;
        Ꮡt.Run(fmt.Sprintf("root EKUs %#v, verify EKUs %#v"u8, tc.rootEKUs, tc.verifyEKUs), (ж<testing.T> tΔ1) => {
            var tmpl = Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                Subject: new pkix.Name(CommonName: "root"u8),
                NotBefore: time.Now().Add(-time.ΔHour),
                NotAfter: time.Now().Add(time.ΔHour),
                DNSNames: new @string[]{"localhost"u8}.slice(),
                ExtKeyUsage: tcʗ1.rootEKUs
            ));
            var (rootDER, errΔ1) = CreateCertificate(rand.Reader, tmpl, tmpl, kʗ1.Public(), kʗ1.OrTypedNil());
            if (errΔ1 != default!) {
                tΔ1.Fatalf("failed to create certificate: %s"u8, errΔ1);
            }
            (var root, errΔ1) = ParseCertificate(rootDER);
            if (errΔ1 != default!) {
                tΔ1.Fatalf("failed to parse certificate: %s"u8, errΔ1);
            }
            var roots = NewCertPool();
            roots.AddCert(root);
            (_, errΔ1) = root.Verify(new VerifyOptions(Roots: roots, KeyUsages: tcʗ1.verifyEKUs));
            if (errΔ1 == default! && !tcʗ1.succeed){
                tΔ1.Error(verificationSucceedˢ);
            } else 
            if (errΔ1 != default! && tcʗ1.succeed) {
                tΔ1.Errorf("verification failed: %q"u8, errΔ1);
            }
        });
    }
}

public static void TestVerifyNilPubKey(ж<testing.T> Ꮡt) {
    var c = Ꮡ(new Certificate(
        RawIssuer: new byte[]{1, 2, 3}.slice(),
        AuthorityKeyId: new byte[]{1, 2, 3}.slice()
    ));
    var opts = Ꮡ(new VerifyOptions(nil));
    opts.Value.Roots = NewCertPool();
    var r = Ꮡ(new Certificate(
        RawSubject: new byte[]{1, 2, 3}.slice(),
        SubjectKeyId: new byte[]{1, 2, 3}.slice()
    ));
    (~opts).Roots.AddCert(r);
    var (_, err) = c.buildChains(new ж<global::go.crypto.x509_package.Certificate>[]{r}.slice(), nil, opts);
    {
        var (_, ok) = err._<UnknownAuthorityError>(ᐧ); if (!ok) {
            Ꮡt.Fatalf("buildChains returned unexpected error, got: %v, want %v"u8, err, new UnknownAuthorityError(nil));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string labelˢ = "label"u8;

public static void TestVerifyBareWildcard(ж<testing.T> Ꮡt) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        Ꮡt.Fatalf("failed to generate key: %s"u8, err);
    }
    var tmpl = Ꮡ(new Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test"u8),
        NotBefore: time.Now().Add(-time.ΔHour),
        NotAfter: time.Now().Add(time.ΔHour),
        DNSNames: new @string[]{"*"u8}.slice()
    ));
    (var cDER, err) = CreateCertificate(rand.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("failed to create certificate: %s"u8, err);
    }
    (var c, err) = ParseCertificate(cDER);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse certificate: %s"u8, err);
    }
    {
        var errΔ1 = c.VerifyHostname(labelˢ); if (errΔ1 == default!) {
            Ꮡt.Fatalf("VerifyHostname unexpected success with bare wildcard SAN"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataPolicyRootPemˢ = "testdata/policy_root.pem"u8;
internal static readonly @string testdataPolicyRootCrossˢ = "testdata/policy_root_cross_inhibit_mapping.pem"u8;
internal static readonly @string testdataPolicyRoot2Pemˢ = "testdata/policy_root2.pem"u8;
internal static readonly @string testdataPolicyˢ = "testdata/policy_intermediate.pem"u8;
internal static readonly @string testdataPolicyˢ2 = "testdata/policy_intermediate_any.pem"u8;
internal static readonly @string testdataPolicyˢ3 = "testdata/policy_intermediate_mapped.pem"u8;
internal static readonly @string testdataPolicyˢ4 = "testdata/policy_intermediate_mapped_any.pem"u8;
internal static readonly @string testdataPolicyˢ5 = "testdata/policy_intermediate_mapped_oid3.pem"u8;
internal static readonly @string testdataPolicyˢ6 = "testdata/policy_intermediate_require.pem"u8;
internal static readonly @string testdataPolicyˢ7 = "testdata/policy_intermediate_require1.pem"u8;
internal static readonly @string testdataPolicyˢ8 = "testdata/policy_intermediate_require2.pem"u8;
internal static readonly @string testdataPolicyˢ9 = "testdata/policy_intermediate_require_no_policies.pem"u8;
internal static readonly @string testdataPolicyLeafPemˢ = "testdata/policy_leaf.pem"u8;
internal static readonly @string testdataPolicyLeafAnyPemˢ = "testdata/policy_leaf_any.pem"u8;
internal static readonly @string testdataPolicyLeafNoneˢ = "testdata/policy_leaf_none.pem"u8;
internal static readonly @string testdataPolicyLeafOid1ˢ = "testdata/policy_leaf_oid1.pem"u8;
internal static readonly @string testdataPolicyLeafOid2ˢ = "testdata/policy_leaf_oid2.pem"u8;
internal static readonly @string testdataPolicyLeafOid3ˢ = "testdata/policy_leaf_oid3.pem"u8;
internal static readonly @string testdataPolicyLeafOid4ˢ = "testdata/policy_leaf_oid4.pem"u8;
internal static readonly @string testdataPolicyLeafOid5ˢ = "testdata/policy_leaf_oid5.pem"u8;
internal static readonly @string testdataPolicyLeafˢ = "testdata/policy_leaf_require.pem"u8;
internal static readonly @string testdataPolicyLeafˢ2 = "testdata/policy_leaf_require1.pem"u8;

internal partial struct TestPoliciesValid_testCase /*dyn*/ {
    internal slice<ж<global::go.crypto.x509_package.Certificate>> chain;
    internal slice<global::go.crypto.x509_package.OID> policies;
    internal bool requireExplicitPolicy;
    internal bool inhibitPolicyMapping;
    internal bool inhibitAnyPolicy;
    internal bool valid;
}

public static void TestPoliciesValid(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // These test cases, the comments, and the certificates they rely on, are
    // stolen from BoringSSL [0]. We skip the tests which involve certificate
    // parsing as part of the verification process. Those tests are in
    // TestParsePolicies.
    //
    // [0] https://boringssl.googlesource.com/boringssl/+/264f4f7a958af6c4ccb04662e302a99dfa7c5b85/crypto/x509/x509_test.cc#5913
    var testOID1 = mustNewOIDFromInts(new uint64[]{1, 2, 840, 113554, 4, 1, 72585, 2, 1}.slice());
    var testOID2 = mustNewOIDFromInts(new uint64[]{1, 2, 840, 113554, 4, 1, 72585, 2, 2}.slice());
    var testOID3 = mustNewOIDFromInts(new uint64[]{1, 2, 840, 113554, 4, 1, 72585, 2, 3}.slice());
    var testOID4 = mustNewOIDFromInts(new uint64[]{1, 2, 840, 113554, 4, 1, 72585, 2, 4}.slice());
    var testOID5 = mustNewOIDFromInts(new uint64[]{1, 2, 840, 113554, 4, 1, 72585, 2, 5}.slice());
    ж<global::go.crypto.x509_package.Certificate> loadTestCert(ж<testing.T> tΔ1, @string path) {
        var (b, err) = os.ReadFile(path);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        var (p, _) = pem.Decode(b);
        (var c, err) = ParseCertificate((~p).Bytes);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        return c;
    }
    var root = loadTestCert(Ꮡt, testdataPolicyRootPemˢ);
    var root_cross_inhibit_mapping = loadTestCert(Ꮡt, testdataPolicyRootCrossˢ);
    var root2 = loadTestCert(Ꮡt, testdataPolicyRoot2Pemˢ);
    var intermediate = loadTestCert(Ꮡt, testdataPolicyˢ);
    var intermediate_any = loadTestCert(Ꮡt, testdataPolicyˢ2);
    var intermediate_mapped = loadTestCert(Ꮡt, testdataPolicyˢ3);
    var intermediate_mapped_any = loadTestCert(Ꮡt, testdataPolicyˢ4);
    var intermediate_mapped_oid3 = loadTestCert(Ꮡt, testdataPolicyˢ5);
    var intermediate_require = loadTestCert(Ꮡt, testdataPolicyˢ6);
    var intermediate_require1 = loadTestCert(Ꮡt, testdataPolicyˢ7);
    var intermediate_require2 = loadTestCert(Ꮡt, testdataPolicyˢ8);
    var intermediate_require_no_policies = loadTestCert(Ꮡt, testdataPolicyˢ9);
    var leaf = loadTestCert(Ꮡt, testdataPolicyLeafPemˢ);
    var leaf_any = loadTestCert(Ꮡt, testdataPolicyLeafAnyPemˢ);
    var leaf_none = loadTestCert(Ꮡt, testdataPolicyLeafNoneˢ);
    var leaf_oid1 = loadTestCert(Ꮡt, testdataPolicyLeafOid1ˢ);
    var leaf_oid2 = loadTestCert(Ꮡt, testdataPolicyLeafOid2ˢ);
    var leaf_oid3 = loadTestCert(Ꮡt, testdataPolicyLeafOid3ˢ);
    var leaf_oid4 = loadTestCert(Ꮡt, testdataPolicyLeafOid4ˢ);
    var leaf_oid5 = loadTestCert(Ꮡt, testdataPolicyLeafOid5ˢ);
    var leaf_require = loadTestCert(Ꮡt, testdataPolicyLeafˢ);
    var leaf_require1 = loadTestCert(Ꮡt, testdataPolicyLeafˢ2);
    var tests = new TestPoliciesValid_testCase[]{ // The chain is good for |oid1| and |oid2|, but not |oid3|.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID2}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: false
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1, testOID2}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1, testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ), // Without |X509_V_FLAG_EXPLICIT_POLICY|, the policy tree is built and
 // intersected with user-specified policies, but it is not required to result
 // in any valid policies.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: true
        ), // However, a CA with policy constraints can require an explicit policy.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_require, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_require, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: false
        ), // requireExplicitPolicy applies even if the application does not configure a
 // user-initial-policy-set. If the validation results in no policies, the
 // chain is invalid.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_none, intermediate_require, root}.slice(),
            requireExplicitPolicy: true,
            valid: false
        ), // A leaf can also set requireExplicitPolicy.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_require, intermediate, root}.slice(),
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_require, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_require, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: false
        ), // requireExplicitPolicy is a count of certificates to skip. If the value is
 // not zero by the end of the chain, it doesn't count.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_require1, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: false
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_require2, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_require1, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: true
        ), // If multiple certificates specify the constraint, the more constrained value
 // wins.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_require1, intermediate_require1, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: false
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_require, intermediate_require2, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            valid: false
        ), // An intermediate that requires an explicit policy, but then specifies no
 // policies should fail verification as a result.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_require_no_policies, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            valid: false
        ), // A constrained intermediate's policy extension has a duplicate policy, which
 // is invalid.
 // {
 // 	chain:    []*Certificate{leaf, intermediate_require_duplicate, root},
 // 	policies: []OID{testOID1},
 // 	valid:    false,
 // },
 // The leaf asserts anyPolicy, but the intermediate does not. The resulting
 // valid policies are the intersection.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_any, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_any, intermediate, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: false
        ), // The intermediate asserts anyPolicy, but the leaf does not. The resulting
 // valid policies are the intersection.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_any, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediate_any, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: false
        ), // Both assert anyPolicy. All policies are valid.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_any, intermediate_any, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_any, intermediate_any, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ), // With just a trust anchor, policy checking silently succeeds.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ), // Although |intermediate_mapped_oid3| contains many mappings, it only accepts
 // OID3. Nodes should not be created for the other mappings.

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid1, intermediate_mapped_oid3, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: true
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid4, intermediate_mapped_oid3, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID4}.slice(),
            requireExplicitPolicy: true,
            valid: false
        ), // Policy mapping can be inhibited, either by the caller or a certificate in
 // the chain, in which case mapped policies are unassertable (apart from some
 // anyPolicy edge cases).

        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid1, intermediate_mapped_oid3, root}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            inhibitPolicyMapping: true,
            valid: false
        ),
        new(
            chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid1, intermediate_mapped_oid3, root_cross_inhibit_mapping, root2}.slice(),
            policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
            requireExplicitPolicy: true,
            valid: false
        )
    }.slice();
    foreach (var (_, useAny) in new bool[]{false, true}.slice()) {
        ж<global::go.crypto.x509_package.Certificate> intermediateΔ1 = default!;
        if (useAny){
            intermediateΔ1 = intermediate_mapped_any;
        } else {
            intermediateΔ1 = intermediate_mapped;
        }
        var extraTests = new TestPoliciesValid_testCase[]{ // OID3 is mapped to {OID1, OID2}, which means OID1 and OID2 (or both) are
 // acceptable for OID3.

            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ),
            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid1, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ),
            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid2, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ), // If the intermediate's policies were anyPolicy, OID3 at the leaf, despite
 // being mapped, is still acceptable as OID3 at the root. Despite the OID3
 // having expected_policy_set = {OID1, OID2}, it can match the anyPolicy
 // node instead.
 //
 // If the intermediate's policies listed OIDs explicitly, OID3 at the leaf
 // is not acceptable as OID3 at the root. OID3 has expected_polciy_set =
 // {OID1, OID2} and no other node allows OID3.

            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid3, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID3}.slice(),
                requireExplicitPolicy: true,
                valid: useAny
            ), // If the intermediate's policies were anyPolicy, OID1 at the leaf is no
 // longer acceptable as OID1 at the root because policies only match
 // anyPolicy when they match no other policy.
 //
 // If the intermediate's policies listed OIDs explicitly, OID1 at the leaf
 // is acceptable as OID1 at the root because it will match both OID1 and
 // OID3 (mapped) policies.

            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid1, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID1}.slice(),
                requireExplicitPolicy: true,
                valid: !useAny
            ), // All pairs of OID4 and OID5 are mapped together, so either can stand for
 // the other.

            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid4, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID4}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ),
            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid4, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID5}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ),
            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid5, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID4}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ),
            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid5, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID5}.slice(),
                requireExplicitPolicy: true,
                valid: true
            ),
            new(
                chain: new ж<global::go.crypto.x509_package.Certificate>[]{leaf_oid4, intermediateΔ1, root}.slice(),
                policies: new global::go.crypto.x509_package.OID[]{testOID4, testOID5}.slice(),
                requireExplicitPolicy: true,
                valid: true
            )
        }.slice();
        tests = appendꓸꓸꓸ(tests, extraTests);
    }
    foreach (var (i, vᴛ1) in tests) {
        ref var tc = ref heap(new TestPoliciesValid_testCase(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ2) => {
            var valid = policiesValid(tcʗ1.chain, new VerifyOptions(
                CertificatePolicies: tcʗ1.policies,
                requireExplicitPolicy: tcʗ1.requireExplicitPolicy,
                inhibitPolicyMapping: tcʗ1.inhibitPolicyMapping,
                inhibitAnyPolicy: tcʗ1.inhibitAnyPolicy
            ));
            if (valid != tcʗ1.valid) {
                tΔ2.Errorf("policiesValid: got %t, want %t"u8, valid, tcʗ1.valid);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509NoValidChainsBuiltˢ2 = "x509: no valid chains built: all candidate chains have invalid policies"u8;
internal static readonly object unexpectedSuccessInvalidˢ = (@string)"unexpected success, invalid policy shouldn't be bypassed by passing VerifyOptions.KeyUsages with ExtKeyUsageAny"u8;

public static void TestInvalidPolicyWithAnyKeyUsage(ж<testing.T> Ꮡt) {
    ж<global::go.crypto.x509_package.Certificate> loadTestCert(ж<testing.T> tΔ1, @string path) {
        var (b, errΔ1) = os.ReadFile(path);
        if (errΔ1 != default!) {
            tΔ1.Fatal(errΔ1);
        }
        var (p, _) = pem.Decode(b);
        (var c, errΔ1) = ParseCertificate((~p).Bytes);
        if (errΔ1 != default!) {
            tΔ1.Fatal(errΔ1);
        }
        return c;
    }
    var testOID3 = mustNewOIDFromInts(new uint64[]{1, 2, 840, 113554, 4, 1, 72585, 2, 3}.slice());
    var (root, intermediate, leaf) = (loadTestCert(Ꮡt, testdataPolicyRootPemˢ), loadTestCert(Ꮡt, testdataPolicyˢ6), loadTestCert(Ꮡt, testdataPolicyLeafPemˢ));
    @string expectedErr = x509NoValidChainsBuiltˢ2;
    var (roots, intermediates) = (NewCertPool(), NewCertPool());
    roots.AddCert(root);
    intermediates.AddCert(intermediate);
    var (_, err) = leaf.Verify(new VerifyOptions(
        Roots: roots,
        Intermediates: intermediates,
        KeyUsages: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageAny}.slice(),
        CertificatePolicies: new global::go.crypto.x509_package.OID[]{testOID3}.slice()
    ));
    if (err == default!){
        Ꮡt.Fatal(unexpectedSuccessInvalidˢ);
    } else 
    if (err.Error() != expectedErr) {
        Ꮡt.Fatalf("unexpected error, got %q, want %q"u8, err, expectedErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string certificateSignedByˢ = "certificate signed by unknown authority"u8;

public static void TestCertificateChainSignedByECDSA(ж<testing.T> Ꮡt) {
    var (caKey, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var root = Ꮡ(new Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "X"u8),
        NotBefore: time.Now().Add(-time.ΔHour),
        NotAfter: time.Now().Add((time.Duration)(31536000000000000L)),
        IsCA: true,
        KeyUsage: (global::go.crypto.x509_package.KeyUsage)(KeyUsageCertSign | KeyUsageCRLSign),
        BasicConstraintsValid: true
    ));
    (var caDER, err) = CreateCertificate(rand.Reader, root, root, caKey.of(ecdsa.PrivateKey.ᏑPublicKey), caKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (root, err) = ParseCertificate(caDER);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (leafKey, _) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    var leaf = Ꮡ(new Certificate(
        SerialNumber: big.NewInt(42),
        Subject: new pkix.Name(CommonName: "leaf"u8),
        NotBefore: time.Now().Add((time.Duration)(-600000000000L)),
        NotAfter: time.Now().Add((time.Duration)(86400000000000L)),
        KeyUsage: KeyUsageDigitalSignature,
        ExtKeyUsage: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
        BasicConstraintsValid: true
    ));
    (var leafDER, err) = CreateCertificate(rand.Reader, leaf, root, leafKey.of(ecdsa.PrivateKey.ᏑPublicKey), caKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (leaf, err) = ParseCertificate(leafDER);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var inter, err) = ParseCertificate(dsaSelfSignedCNX(Ꮡt));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var inters = NewCertPool();
    inters.AddCert(root);
    inters.AddCert(inter);
    @string wantErr = certificateSignedByˢ;
    (_, err) = leaf.Verify(new VerifyOptions(Intermediates: inters, Roots: NewCertPool()));
    if (!strings.Contains(err.Error(), wantErr)) {
        Ꮡt.Errorf("got %v, want %q"u8, err, wantErr);
    }
}

internal partial struct dsaSelfSignedCNX_dsaParams /*dyn*/ {
    public ж<bigꓸInt> P, Q, G;
}

// dsaSelfSignedCNX produces DER-encoded
// certificate with the properties:
//
//	Subject=Issuer=CN=X
//	DSA SPKI
//	Matching inner/outer signature OIDs
//	Dummy ECDSA signature
internal static slice<byte> dsaSelfSignedCNX(ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    ref var @params = ref heap(new dsa.Parameters(), out var Ꮡparams);
    {
        var errΔ1 = dsa.GenerateParameters(Ꮡparams, rand.Reader, dsa.L1024N160); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    ref var dsaPriv = ref heap(new dsa.PrivateKey(), out var ᏑdsaPriv);
    dsaPriv.Parameters = @params;
    {
        var errΔ2 = dsa.GenerateKey(ᏑdsaPriv, rand.Reader); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    var dsaPub = ᏑdsaPriv.of(dsa.PrivateKey.ᏑPublicKey);
    var (paramDER, err) = asn1.Marshal(new dsaSelfSignedCNX_dsaParams((~dsaPub).P, (~dsaPub).Q, (~dsaPub).G));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var yDER, err) = asn1.Marshal((~dsaPub).Y.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var spki = new publicKeyInfo(
        Algorithm: new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyDSA,
            Parameters: new asn1.RawValue(FullBytes: paramDER)
        ),
        PublicKey: new asn1.BitString(Bytes: yDER, BitLength: 8 * builtin.len(yDER))
    );
    var rdn = new pkix.Name(CommonName: "X"u8).ToRDNSequence();
    (var b, err) = asn1.Marshal(rdn);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var rawName = new asn1.RawValue(FullBytes: b);
    var algoIdent = new pkix.AlgorithmIdentifier(Algorithm: oidSignatureDSAWithSHA256);
    var tbs = new tbsCertificate(
        Version: 0,
        SerialNumber: big.NewInt(1002),
        SignatureAlgorithm: algoIdent,
        Issuer: rawName,
        Validity: new validity(NotBefore: time.Now().Add(-time.ΔHour), NotAfter: time.Now().Add((time.Duration)(86400000000000L))),
        Subject: rawName,
        PublicKey: spki
    );
    var c = new certificate(
        TBSCertificate: tbs,
        SignatureAlgorithm: algoIdent,
        SignatureValue: new asn1.BitString(Bytes: new byte[]{0}.slice(), BitLength: 8)
    );
    (var dsaDER, err) = asn1.Marshal(c);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return dsaDER;
}

} // end x509_internal_test_package
