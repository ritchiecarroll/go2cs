// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This test uses Netflix's BetterTLS test suite to test the crypto/x509
// path building and name constraint validation.
//
// The test data in JSON form is around 31MB, so we fetch the BetterTLS
// go module and use it to generate the JSON data on-the-fly in a tmp dir.
//
// For more information, see:
// https://github.com/netflix/bettertls
// https://netflixtechblog.com/bettertls-c9915cd255c0
namespace go.crypto;

using cryptotest = go.crypto.@internal.cryptotest_package;
using base64 = go.encoding.base64_package;
using json = go.encoding.json_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto.@internal;
using go.encoding;
using go.os;
using path;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

// TestBetterTLS runs the "pathbuilding" and "nameconstraints" suites of
// BetterTLS.
//
// The test cases in the pathbuilding suite are designed to test edge-cases
// for path building and validation. In particular, the ["chain of pain"][0]
// scenario where a validator treats path building as an operation with
// a single possible outcome, instead of many.
//
// The test cases in the nameconstraints suite are designed to test edge-cases
// for name constraint parsing and validation.
//
// [0]: https://medium.com/@sleevi_/path-building-vs-path-verifying-the-chain-of-pain-9fbab861d7d6
public static void TestBetterTLS(ж<testing.T> t) {
    testenv.SkipIfShortAndSlow(new x509_test_package.testing_TжTB(t));

    ref var data = ref heap<betterTLS>(out var Ꮡdata);
    (data, var roots) = betterTLSTestData(t);

    foreach (var (_, suite) in range(slice<@string>(["pathbuilding", "nameconstraints"]))) {
        var rootsʗ1 = roots;
        t.Run(suite, (ж<testing.T> tΔ1) => {
            runTestSuite(tΔ1, suite, Ꮡdata, rootsʗ1);
        });
    }
}

internal static void runTestSuite(ж<testing.T> t, @string suiteName, ж<betterTLS> Ꮡdata, ж<global::go.crypto.x509_package.CertPool> roots) {
    ref var data = ref Ꮡdata.DerefOrNull();

    var (suite, exists) = data.Suites[suiteName, ꟷ];
    if (!exists) {
        t.Fatalf("missing %s suite", suiteName);
    }

    t.Logf(
        "running %s test suite with %d test cases",
        suiteName, builtin.len(suite.TestCases));

    foreach (var (_, vᴛ1) in range(suite.TestCases)) {
        var tc = vᴛ1;

        t.Logf("testing %s test case %d", suiteName, tc.ID);

        var (certsDER, err) = tc.Certs();
        if (err != nil) {
            t.Fatalf(
                "failed to decode certificates for test case %d: %v",
                tc.ID, err);
        }

        if (builtin.len(certsDER) == 0) {
            t.Fatalf("test case %d has no certificates", tc.ID);
        }

        (var eeCert, err) = ParseCertificate(certsDER[0]);
        if (err != nil) {
            // Several constraint test cases contain invalid end-entity
            // certificate extensions that we reject ahead of verification
            // time. We consider this a pass and skip further processing.
            //
            // For example, a SAN with a uniformResourceIdentifier general name
            // containing the value `"http://foo.bar, DNS:test.localhost"`, or
            // an iPAddress general name of the wrong length.
            if (suiteName == "nameconstraints"u8 && tc.Expected == expectedReject) {
                t.Logf(
                    "skipping expected reject test case %d "u8 + "- end entity certificate parse error: %v"u8,
                    tc.ID, err);
                continue;
            }
            t.Fatalf(
                "failed to parse end entity certificate for test case %d: %v",
                tc.ID, err);
        }

        var intermediates = NewCertPool();
        foreach (var (i, certDER) in range(certsDER[1..])) {
            var (cert, errΔ1) = ParseCertificate(certDER);
            if (errΔ1 != nil) {
                t.Fatalf(
                    "failed to parse intermediate certificate %d for test case %d: %v",
                    i + 1, tc.ID, errΔ1);
            }
            intermediates.AddCert(cert);
        }

        (_, err) = eeCert.Verify(new VerifyOptions(
            Roots: roots,
            Intermediates: intermediates,
            DNSName: tc.Hostname,
            KeyUsages: [ExtKeyUsageServerAuth]
        ));

        var exprᴛ1 = tc.Expected;
        switch (ᐧ) {
        case {} when exprᴛ1 == expectedAccept: {
            if (err != nil) {
                t.Errorf(
                    "test case %d failed: expected success, got error: %v",
                    tc.ID, err);
            }
            break;
        }
        case {} when exprᴛ1 == expectedReject: {
            if (err == nil) {
                t.Errorf(
                    "test case %d failed: expected failure, but verification succeeded",
                    tc.ID);
            }
            break;
        }
        default: {
            t.Fatalf(
                "test case %d failed: unknown expected result: %s",
                tc.ID, tc.Expected);
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testsJsonˢ => ref testsJsonˢᵇ;
private static readonly @string testsJsonˢᵇ = "tests.json"u8;
internal static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
internal static ref readonly @string testSuitesCmdBettertlsˢ => ref testSuitesCmdBettertlsˢᵇ;
private static readonly @string testSuitesCmdBettertlsˢᵇ = "./test-suites/cmd/bettertls"u8;
internal static ref readonly @string exportTestsˢ => ref exportTestsˢᵇ;
private static readonly @string exportTestsˢᵇ = "export-tests"u8;
internal static ref readonly @string outˢ => ref outˢᵇ;
private static readonly @string outˢᵇ = "--out"u8;
internal static readonly object runningBettertlsExportˢ = (@string)"running bettertls export-tests command"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bettertlsModuleᶜ => ref bettertlsModuleᶜᵇ;
private static readonly @string bettertlsModuleᶜᵇ = "github.com/Netflix/bettertls"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bettertlsVersionᶜ => ref bettertlsVersionᶜᵇ;
private static readonly @string bettertlsVersionᶜᵇ = "v0.0.0-20250909192348-e1e99e353074"u8;

internal static (betterTLS, ж<global::go.crypto.x509_package.CertPool>) betterTLSTestData(ж<testing.T> t) {
    @string bettertlsModule = bettertlsModuleᶜ;
    @string bettertlsVersion = bettertlsVersionᶜ;

    @string bettertlsDir = cryptotest.FetchModule(t, bettertlsModule, bettertlsVersion);

    @string tempDir = t.TempDir();
    @string testsJSONPath = filepath.Join(tempDir, testsJsonˢ);

    var cmd = testenv.Command(new x509_test_package.testing_TжTB(t), testenv.GoToolPath(new x509_test_package.testing_TжTB(t)),
        runˢ, testSuitesCmdBettertlsˢ,
        exportTestsˢ,
        outˢ, testsJSONPath);
    cmd.Dir = bettertlsDir;
    t.Log(runningBettertlsExportˢ);
    var (@out, errΔ1) = cmd.CombinedOutput(); if (errΔ1 != nil) {
        t.Fatalf("failed to run bettertls export-tests: %v\n%s", errΔ1, @out);
    }

    var (jsonData, err) = os.ReadFile(testsJSONPath);
    if (err != nil) {
        t.Fatalf("failed to read exported tests.json: %v", err);
    }

    t.Logf("successfully loaded tests.json at %s", testsJSONPath);

    ref var data = ref heap(new betterTLS(), out var Ꮡdata);
    var errΔ2 = json.Unmarshal(jsonData, Ꮡdata); if (errΔ2 != nil) {
        t.Fatalf("failed to unmarshal JSON data: %v", errΔ2);
    }

    t.Logf("testing betterTLS revision: %s", data.Revision);
    t.Logf("number of test suites: %d", builtin.len(data.Suites));

    (var rootDER, err) = data.RootCert();
    if (err != nil) {
        t.Fatalf("failed to decode trust root: %v", err);
    }

    (var rootCert, err) = ParseCertificate(rootDER);
    if (err != nil) {
        t.Fatalf("failed to parse trust root certificate: %v", err);
    }

    var roots = NewCertPool();
    roots.AddCert(rootCert);

    return (data, roots);
}

internal partial struct betterTLS {
    public @string Revision; /*`json:"betterTlsRevision"`*/
    public @string Root; /*`json:"trustRoot"`*/
    public map<@string, betterTLSSuite> Suites; /*`json:"suites"`*/
}

internal static (slice<byte>, error) RootCert(this ref betterTLS b) {
    return base64.StdEncoding.DecodeString(b.Root);
}

public partial struct betterTLSSuite {
    public slice<betterTLSTest> TestCases; /*`json:"testCases"`*/
}

public partial struct betterTLSTest {
    public uint32 ID; /*`json:"id"`*/
    public slice<@string> Certificates; /*`json:"certificates"`*/
    public @string Hostname; /*`json:"hostname"`*/
    public expectedResult Expected; /*`json:"expected"`*/
}

public static (slice<slice<byte>>, error) Certs(this ref betterTLSTest test) {
    var certs = new slice<slice<byte>>(builtin.len(test.Certificates));
    foreach (var (i, cert) in range(test.Certificates)) {
        var (decoded, err) = base64.StdEncoding.DecodeString(cert);
        if (err != nil) {
            return (default, err);
        }
        certs[i] = decoded;
    }
    return (certs, default);
}

public partial struct expectedResult /*@string*/;

internal static ref readonly expectedResult expectedAccept => ref expectedAcceptᵇ;
private static readonly expectedResult expectedAcceptᵇ = "ACCEPT"u8;
internal static ref readonly expectedResult expectedReject => ref expectedRejectᵇ;
private static readonly expectedResult expectedRejectᵇ = "REJECT"u8;

} // end x509_internal_test_package
