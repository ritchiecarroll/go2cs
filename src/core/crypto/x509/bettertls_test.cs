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
public static void TestBetterTLS(ж<testing.T> Ꮡt) {
    testenv.SkipIfShortAndSlow(new x509_test_package.testing_TжTB(Ꮡt));

    ref var data = ref heap<betterTLS>(out var Ꮡdata);
    (data, var roots) = betterTLSTestData(Ꮡt);

    foreach (var (_, suite) in new @string[]{"pathbuilding"u8, "nameconstraints"u8}.slice()) {
        var rootsʗ1 = roots;
        Ꮡt.Run(suite, (ж<testing.T> tΔ1) => {
            runTestSuite(tΔ1, suite, Ꮡdata, rootsʗ1);
        });
    }
}

internal static void runTestSuite(ж<testing.T> Ꮡt, @string suiteName, ж<betterTLS> Ꮡdata, ж<global::go.crypto.x509_package.CertPool> Ꮡroots) {
    ref var data = ref Ꮡdata.DerefOrNull();

    var (suite, exists) = data.Suites[suiteName, ꟷ];
    if (!exists) {
        Ꮡt.Fatalf("missing %s suite"u8, suiteName);
    }

    Ꮡt.Logf(
        "running %s test suite with %d test cases"u8,
        suiteName, builtin.len(suite.TestCases));

    foreach (var (_, vᴛ1) in suite.TestCases) {
        var tc = vᴛ1;

        Ꮡt.Logf("testing %s test case %d"u8, suiteName, tc.ID);

        var (certsDER, err) = tc.Certs();
        if (err != default!) {
            Ꮡt.Fatalf(
                "failed to decode certificates for test case %d: %v"u8,
                tc.ID, err);
        }

        if (builtin.len(certsDER) == 0) {
            Ꮡt.Fatalf("test case %d has no certificates"u8, tc.ID);
        }

        (var eeCert, err) = ParseCertificate(certsDER[0]);
        if (err != default!) {
            // Several constraint test cases contain invalid end-entity
            // certificate extensions that we reject ahead of verification
            // time. We consider this a pass and skip further processing.
            //
            // For example, a SAN with a uniformResourceIdentifier general name
            // containing the value `"http://foo.bar, DNS:test.localhost"`, or
            // an iPAddress general name of the wrong length.
            if (suiteName == "nameconstraints"u8 && tc.Expected == expectedReject) {
                Ꮡt.Logf(
                    "skipping expected reject test case %d "u8 + "- end entity certificate parse error: %v"u8,
                    tc.ID, err);
                continue;
            }
            Ꮡt.Fatalf(
                "failed to parse end entity certificate for test case %d: %v"u8,
                tc.ID, err);
        }

        var intermediates = NewCertPool();
        foreach (var (i, certDER) in certsDER[1..]) {
            var (cert, errΔ1) = ParseCertificate(certDER);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf(
                    "failed to parse intermediate certificate %d for test case %d: %v"u8,
                    i + 1, tc.ID, errΔ1);
            }
            intermediates.AddCert(cert);
        }

        (_, err) = eeCert.Verify(new VerifyOptions(
            Roots: Ꮡroots,
            Intermediates: intermediates,
            DNSName: tc.Hostname,
            KeyUsages: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice()
        ));

        var exprᴛ1 = tc.Expected;
        if (exprᴛ1 == expectedAccept) {
            if (err != default!) {
                Ꮡt.Errorf(
                    "test case %d failed: expected success, got error: %v"u8,
                    tc.ID, err);
            }
        }
        else if (exprᴛ1 == expectedReject) {
            if (err == default!) {
                Ꮡt.Errorf(
                    "test case %d failed: expected failure, but verification succeeded"u8,
                    tc.ID);
            }
        }
        else { /* default: */
            Ꮡt.Fatalf(
                "test case %d failed: unknown expected result: %s"u8,
                tc.ID, tc.Expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testsJsonˢ = "tests.json"u8;
internal static readonly @string runˢ = "run"u8;
internal static readonly @string testSuitesCmdBettertlsˢ = "./test-suites/cmd/bettertls"u8;
internal static readonly @string exportTestsˢ = "export-tests"u8;
internal static readonly @string outˢ = "--out"u8;
internal static readonly object runningBettertlsExportˢ = (@string)"running bettertls export-tests command"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bettertlsModuleᶜ = "github.com/Netflix/bettertls"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bettertlsVersionᶜ = "v0.0.0-20250909192348-e1e99e353074"u8;

internal static (betterTLS, ж<global::go.crypto.x509_package.CertPool>) betterTLSTestData(ж<testing.T> Ꮡt) {
    @string bettertlsModule = bettertlsModuleᶜ;
    @string bettertlsVersion = bettertlsVersionᶜ;

    @string bettertlsDir = cryptotest.FetchModule(Ꮡt, bettertlsModule, bettertlsVersion);

    @string tempDir = Ꮡt.TempDir();
    @string testsJSONPath = filepath.Join(tempDir, testsJsonˢ);

    var cmd = testenv.Command(new x509_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new x509_test_package.testing_TжTB(Ꮡt)),
        runˢ, testSuitesCmdBettertlsˢ,
        exportTestsˢ,
        outˢ, testsJSONPath);
    cmd.Value.Dir = bettertlsDir;
    Ꮡt.Log(runningBettertlsExportˢ);
    {
        var (@out, errΔ1) = cmd.CombinedOutput(); if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to run bettertls export-tests: %v\n%s"u8, errΔ1, @out);
        }
    }

    var (jsonData, err) = os.ReadFile(testsJSONPath);
    if (err != default!) {
        Ꮡt.Fatalf("failed to read exported tests.json: %v"u8, err);
    }

    Ꮡt.Logf("successfully loaded tests.json at %s"u8, testsJSONPath);

    ref var data = ref heap(new betterTLS(), out var Ꮡdata);
    {
        var errΔ2 = json.Unmarshal(jsonData, Ꮡdata); if (errΔ2 != default!) {
            Ꮡt.Fatalf("failed to unmarshal JSON data: %v"u8, errΔ2);
        }
    }

    Ꮡt.Logf("testing betterTLS revision: %s"u8, data.Revision);
    Ꮡt.Logf("number of test suites: %d"u8, builtin.len(data.Suites));

    (var rootDER, err) = data.RootCert();
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode trust root: %v"u8, err);
    }

    (var rootCert, err) = ParseCertificate(rootDER);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse trust root certificate: %v"u8, err);
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
    foreach (var (i, cert) in test.Certificates) {
        var (decoded, err) = base64.StdEncoding.DecodeString(cert);
        if (err != default!) {
            return (default!, err);
        }
        certs[i] = decoded;
    }
    return (certs, default!);
}

public partial struct expectedResult /*@string*/;

internal static readonly expectedResult expectedAccept = "ACCEPT"u8;
internal static readonly expectedResult expectedReject = "REJECT"u8;

} // end x509_internal_test_package
