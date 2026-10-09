// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using json = go.encoding.json_package;
using os = os_package;
using filepath = path.filepath_package;
using slices = slices_package;
using testing = testing_package;
using go.encoding;
using path;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

internal static map<@string, global::go.crypto.x509_package.OID> nistTestPolicies;
internal static void initᴛnistTestPolicies() { nistTestPolicies = new map<@string, global::go.crypto.x509_package.OID>{
    ["anyPolicy"u8] = anyPolicyOID,
    ["NIST-test-policy-1"u8] = mustNewOIDFromInts(new uint64[]{2, 16, 840, 1, 101, 3, 2, 1, 48, 1}.slice()),
    ["NIST-test-policy-2"u8] = mustNewOIDFromInts(new uint64[]{2, 16, 840, 1, 101, 3, 2, 1, 48, 2}.slice()),
    ["NIST-test-policy-3"u8] = mustNewOIDFromInts(new uint64[]{2, 16, 840, 1, 101, 3, 2, 1, 48, 3}.slice()),
    ["NIST-test-policy-6"u8] = mustNewOIDFromInts(new uint64[]{2, 16, 840, 1, 101, 3, 2, 1, 48, 6}.slice())
}; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataNistPkitsCertsˢ = "testdata/nist-pkits/certs"u8;
internal static readonly @string testdataNistPkitsVectorsˢ = "testdata/nist-pkits/vectors.json"u8;
internal static readonly object expectedPathValidationToˢ = (@string)"Expected path validation to fail"u8;

internal partial struct TestNISTPKITSPolicy_testcases /*dyn*/ {
    public @string Name;
    public slice<@string> CertPath;
    public slice<@string> InitialPolicySet;
    public bool InitialPolicyMappingInhibit;
    public bool InitialExplicitPolicy;
    public bool InitialAnyPolicyInhibit;
    public bool ShouldValidate;
    public bool Skipped;
}

public static void TestNISTPKITSPolicy(ж<testing.T> Ꮡt) {
    // This test runs a subset of the NIST PKI path validation test suite that
    // focuses of policy validation, rather than the entire suite. Since the
    // suite assumes you are only validating the path, rather than building
    // _and_ validating the path, we take the path as given and run
    // policiesValid on it.
    @string certDir = testdataNistPkitsCertsˢ;
    ref var testcases = ref heap<slice<TestNISTPKITSPolicy_testcases>>(out var Ꮡtestcases);
    var (b, err) = os.ReadFile(testdataNistPkitsVectorsˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = json.Unmarshal(b, Ꮡtestcases); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var policyTests = new map<@string, bool>{
        ["4.8.1 All Certificates Same Policy Test1 (Subpart 1)"u8] = true,
        ["4.8.1 All Certificates Same Policy Test1 (Subpart 2)"u8] = true,
        ["4.8.1 All Certificates Same Policy Test1 (Subpart 3)"u8] = true,
        ["4.8.1 All Certificates Same Policy Test1 (Subpart 4)"u8] = true,
        ["4.8.2 All Certificates No Policies Test2 (Subpart 1)"u8] = true,
        ["4.8.2 All Certificates No Policies Test2 (Subpart 2)"u8] = true,
        ["4.8.3 Different Policies Test3 (Subpart 1)"u8] = true,
        ["4.8.3 Different Policies Test3 (Subpart 2)"u8] = true,
        ["4.8.3 Different Policies Test3 (Subpart 3)"u8] = true,
        ["4.8.4 Different Policies Test4"u8] = true,
        ["4.8.5 Different Policies Test5"u8] = true,
        ["4.8.6 Overlapping Policies Test6 (Subpart 1)"u8] = true,
        ["4.8.6 Overlapping Policies Test6 (Subpart 2)"u8] = true,
        ["4.8.6 Overlapping Policies Test6 (Subpart 3)"u8] = true,
        ["4.8.7 Different Policies Test7"u8] = true,
        ["4.8.8 Different Policies Test8"u8] = true,
        ["4.8.9 Different Policies Test9"u8] = true,
        ["4.8.10 All Certificates Same Policies Test10 (Subpart 1)"u8] = true,
        ["4.8.10 All Certificates Same Policies Test10 (Subpart 2)"u8] = true,
        ["4.8.10 All Certificates Same Policies Test10 (Subpart 3)"u8] = true,
        ["4.8.11 All Certificates AnyPolicy Test11 (Subpart 1)"u8] = true,
        ["4.8.11 All Certificates AnyPolicy Test11 (Subpart 2)"u8] = true,
        ["4.8.12 Different Policies Test12"u8] = true,
        ["4.8.13 All Certificates Same Policies Test13 (Subpart 1)"u8] = true,
        ["4.8.13 All Certificates Same Policies Test13 (Subpart 2)"u8] = true,
        ["4.8.13 All Certificates Same Policies Test13 (Subpart 3)"u8] = true,
        ["4.8.14 AnyPolicy Test14 (Subpart 1)"u8] = true,
        ["4.8.14 AnyPolicy Test14 (Subpart 2)"u8] = true,
        ["4.8.15 User Notice Qualifier Test15"u8] = true,
        ["4.8.16 User Notice Qualifier Test16"u8] = true,
        ["4.8.17 User Notice Qualifier Test17"u8] = true,
        ["4.8.18 User Notice Qualifier Test18 (Subpart 1)"u8] = true,
        ["4.8.18 User Notice Qualifier Test18 (Subpart 2)"u8] = true,
        ["4.8.19 User Notice Qualifier Test19"u8] = true,
        ["4.8.20 CPS Pointer Qualifier Test20"u8] = true,
        ["4.9.1 Valid RequireExplicitPolicy Test1"u8] = true,
        ["4.9.2 Valid RequireExplicitPolicy Test2"u8] = true,
        ["4.9.3 Invalid RequireExplicitPolicy Test3"u8] = true,
        ["4.9.4 Valid RequireExplicitPolicy Test4"u8] = true,
        ["4.9.5 Invalid RequireExplicitPolicy Test5"u8] = true,
        ["4.9.6 Valid Self-Issued requireExplicitPolicy Test6"u8] = true,
        ["4.9.7 Invalid Self-Issued requireExplicitPolicy Test7"u8] = true,
        ["4.9.8 Invalid Self-Issued requireExplicitPolicy Test8"u8] = true,
        ["4.10.1.1 Valid Policy Mapping Test1 (Subpart 1)"u8] = true,
        ["4.10.1.2 Valid Policy Mapping Test1 (Subpart 2)"u8] = true,
        ["4.10.1.3 Valid Policy Mapping Test1 (Subpart 3)"u8] = true,
        ["4.10.2 Invalid Policy Mapping Test2 (Subpart 1)"u8] = true,
        ["4.10.2 Invalid Policy Mapping Test2 (Subpart 2)"u8] = true,
        ["4.10.3 Valid Policy Mapping Test3 (Subpart 1)"u8] = true,
        ["4.10.3 Valid Policy Mapping Test3 (Subpart 2)"u8] = true,
        ["4.10.4 Invalid Policy Mapping Test4"u8] = true,
        ["4.10.5 Valid Policy Mapping Test5 (Subpart 1)"u8] = true,
        ["4.10.5 Valid Policy Mapping Test5 (Subpart 2)"u8] = true,
        ["4.10.6 Valid Policy Mapping Test6 (Subpart 1)"u8] = true,
        ["4.10.6 Valid Policy Mapping Test6 (Subpart 2)"u8] = true,
        ["4.10.7 Invalid Mapping From anyPolicy Test7"u8] = true,
        ["4.10.8 Invalid Mapping To anyPolicy Test8"u8] = true,
        ["4.10.9 Valid Policy Mapping Test9"u8] = true,
        ["4.10.10 Invalid Policy Mapping Test10"u8] = true,
        ["4.10.11 Valid Policy Mapping Test11"u8] = true,
        ["4.10.12 Valid Policy Mapping Test12 (Subpart 1)"u8] = true,
        ["4.10.12 Valid Policy Mapping Test12 (Subpart 2)"u8] = true,
        ["4.10.13 Valid Policy Mapping Test13 (Subpart 1)"u8] = true,
        ["4.10.13 Valid Policy Mapping Test13 (Subpart 2)"u8] = true,
        ["4.10.13 Valid Policy Mapping Test13 (Subpart 3)"u8] = true,
        ["4.10.14 Valid Policy Mapping Test14"u8] = true,
        ["4.11.1 Invalid inhibitPolicyMapping Test1"u8] = true,
        ["4.11.2 Valid inhibitPolicyMapping Test2"u8] = true,
        ["4.11.3 Invalid inhibitPolicyMapping Test3"u8] = true,
        ["4.11.4 Valid inhibitPolicyMapping Test4"u8] = true,
        ["4.11.5 Invalid inhibitPolicyMapping Test5"u8] = true,
        ["4.11.6 Invalid inhibitPolicyMapping Test6"u8] = true,
        ["4.11.7 Valid Self-Issued inhibitPolicyMapping Test7"u8] = true,
        ["4.11.8 Invalid Self-Issued inhibitPolicyMapping Test8"u8] = true,
        ["4.11.9 Invalid Self-Issued inhibitPolicyMapping Test9"u8] = true,
        ["4.11.10 Invalid Self-Issued inhibitPolicyMapping Test10"u8] = true,
        ["4.11.11 Invalid Self-Issued inhibitPolicyMapping Test11"u8] = true,
        ["4.12.1 Invalid inhibitAnyPolicy Test1"u8] = true,
        ["4.12.2 Valid inhibitAnyPolicy Test2"u8] = true,
        ["4.12.3 inhibitAnyPolicy Test3 (Subpart 1)"u8] = true,
        ["4.12.3 inhibitAnyPolicy Test3 (Subpart 2)"u8] = true,
        ["4.12.4 Invalid inhibitAnyPolicy Test4"u8] = true,
        ["4.12.5 Invalid inhibitAnyPolicy Test5"u8] = true,
        ["4.12.6 Invalid inhibitAnyPolicy Test6"u8] = true,
        ["4.12.7 Valid Self-Issued inhibitAnyPolicy Test7"u8] = true,
        ["4.12.8 Invalid Self-Issued inhibitAnyPolicy Test8"u8] = true,
        ["4.12.9 Valid Self-Issued inhibitAnyPolicy Test9"u8] = true,
        ["4.12.10 Invalid Self-Issued inhibitAnyPolicy Test10"u8] = true
    };
    foreach (var (_, vᴛ1) in testcases) {
        ref var tc = ref heap(new TestNISTPKITSPolicy_testcases(), out var Ꮡtc);
        tc = vᴛ1;

        if (!policyTests[tc.Name]) {
            continue;
        }
        var errʗ1 = err;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.Name, (ж<testing.T> tΔ1) => {
            slice<ж<global::go.crypto.x509_package.Certificate>> chain = default!;
            foreach (var (_, c) in tcʗ1.CertPath) {
                var (certDER, errΔ2) = os.ReadFile(filepath.Join(certDir, c));
                if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
                (var cert, errΔ2) = ParseCertificate(certDER);
                if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
                chain = append(chain, cert);
            }
            slices.Reverse<slice<ж<global::go.crypto.x509_package.Certificate>>, ж<global::go.crypto.x509_package.Certificate>>(chain);
            slice<global::go.crypto.x509_package.OID> initialPolicies = default!;
            foreach (var (_, pstr) in tcʗ1.InitialPolicySet) {
                var (policy, ok) = nistTestPolicies[pstr, ꟷ];
                if (!ok) {
                    tΔ1.Fatalf("unknown test policy: %s"u8, pstr);
                }
                initialPolicies = append(initialPolicies, policy);
            }
            var valid = policiesValid(chain, new VerifyOptions(
                CertificatePolicies: initialPolicies,
                inhibitPolicyMapping: tcʗ1.InitialPolicyMappingInhibit,
                requireExplicitPolicy: tcʗ1.InitialExplicitPolicy,
                inhibitAnyPolicy: tcʗ1.InitialAnyPolicyInhibit
            ));
            if (!valid) {
                if (!tcʗ1.ShouldValidate) {
                    return;
                }
                tΔ1.Fatalf("Failed to validate: %s"u8, errʗ1);
            }
            if (!tcʗ1.ShouldValidate) {
                tΔ1.Fatal(expectedPathValidationToˢ);
            }
        });
    }
}

} // end x509_internal_test_package
