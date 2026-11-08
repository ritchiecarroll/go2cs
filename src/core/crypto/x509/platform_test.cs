// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using pem = go.encoding.pem_package;
using big = go.math.big_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using crypto = crypto_package;
using go.crypto;
using go.encoding;
using go.math;
using io = io_package;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

// In order to run this test suite locally, you need to insert the test root, at
// the path below, into your trust store. This root is constrained such that it
// should not be dangerous to local developers to trust, but care should be
// taken when inserting it into the trust store not to give it increased
// permissions.
//
// On macOS the certificate can be further constrained to only be valid for
// 'SSL' in the certificate properties pane of the 'Keychain Access' program.
//
// On Windows the certificate can also be constrained to only server
// authentication in the properties pane of the certificate in the
// "Certificates" snap-in of mmc.exe.
internal static ref readonly @string rootCertPath => ref rootCertPathᵇ;
private static readonly @string rootCertPathᵇ = "platform_root_cert.pem"u8;
internal static ref readonly @string rootKeyPath => ref rootKeyPathᵇ;
private static readonly @string rootKeyPathᵇ = "platform_root_key.pem"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object onlyTestedOnWindowsAndˢ = (@string)"only tested on windows and darwin"u8;

internal partial struct TestPlatformVerifier_tests /*dyn*/ {
    internal @string name;
    internal ж<global::go.crypto.x509_package.Certificate> cert;
    internal bool selfSigned;
    internal @string dnsName;
    internal time.Time time;
    internal slice<global::go.crypto.x509_package.ExtKeyUsage> eku;
    internal @string expectedErr;
    internal @string windowsErr;
    internal @string macosErr;
}

public static void TestPlatformVerifier(ж<testing.T> t) {
    if (runtime.GOOS != "windows"u8 && runtime.GOOS != "darwin"u8) {
        t.Skip(onlyTestedOnWindowsAndˢ);
    }

    var (der, err) = os.ReadFile(rootCertPath);
    if (err != nil) {
        t.Fatalf("failed to read test root: %s", err);
    }
    var (b, _) = pem.Decode(der);
    (var testRoot, err) = ParseCertificate(b.Bytes);
    if (err != nil) {
        t.Fatalf("failed to parse test root: %s", err);
    }

    (der, err) = os.ReadFile(rootKeyPath);
    if (err != nil) {
        t.Fatalf("failed to read test key: %s", err);
    }
    (b, _) = pem.Decode(der);
    (var testRootKey, err) = ParseECPrivateKey(b.Bytes);
    if (err != nil) {
        t.Fatalf("failed to parse test key: %s", err);
    }

    var (_, errΔ1) = testRoot.Verify(new VerifyOptions()); if (errΔ1 != nil) {
        t.Skipf("test root is not in trust store, skipping (err: %q)", errΔ1);
    }

    var now = time.Now();

    var tests = slice<TestPlatformVerifier_tests>([
        new(
            name: "valid",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            ))
        ),
        new(
            name: "valid (with name)",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            dnsName: "valid.testing.golang.invalid"
        ),
        new(
            name: "valid (with time)",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            time: now.Add((time.Duration)(1800000000000L))
        ),
        new(
            name: "valid (with eku)",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            eku: [ExtKeyUsageServerAuth]
        ),
        new(
            name: "wrong name",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            dnsName: "invalid.testing.golang.invalid",
            expectedErr: "x509: certificate is valid for valid.testing.golang.invalid, not invalid.testing.golang.invalid"
        ),
        new(
            name: "expired (future)",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            time: now.Add((time.Duration)(7200000000000L)),
            expectedErr: "x509: certificate has expired or is not yet valid"
        ),
        new(
            name: "expired (past)",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            time: now.Add((time.Duration)(7200000000000L)),
            expectedErr: "x509: certificate has expired or is not yet valid"
        ),
        new(
            name: "self-signed",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            selfSigned: true,
            macosErr: "x509: “valid.testing.golang.invalid” certificate is not trusted",
            windowsErr: "x509: certificate signed by unknown authority"
        ),
        new(
            name: "non-specified KU",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageServerAuth]
            )),
            eku: [ExtKeyUsageEmailProtection],
            expectedErr: "x509: certificate specifies an incompatible key usage"
        ),
        new(
            name: "non-nested KU",
            cert: Ꮡ(new Certificate(
                SerialNumber: big.NewInt(1),
                DNSNames: ["valid.testing.golang.invalid"],
                NotBefore: now.Add(-time.ΔHour),
                NotAfter: now.Add(time.ΔHour),
                ExtKeyUsage: [ExtKeyUsageEmailProtection]
            )),
            macosErr: "x509: “valid.testing.golang.invalid” certificate is not permitted for this usage",
            windowsErr: "x509: certificate specifies an incompatible key usage"
        )
    ]);

    (var leafKey, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != nil) {
        t.Fatalf("ecdsa.GenerateKey failed: %s", err);
    }

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestPlatformVerifier_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var leafKeyʗ1 = leafKey;
        var tcʗ1 = tc;
        var testRootʗ1 = testRoot;
        var testRootKeyʗ1 = testRootKey;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var parent = testRootʗ1;
            if (tcʗ1.selfSigned) {
                parent = tcʗ1.cert;
            }
            var (certDER, errΔ2) = CreateCertificate(rand.Reader, tcʗ1.cert, parent, leafKeyʗ1.Public(), testRootKeyʗ1.OrTypedNil());
            if (errΔ2 != nil) {
                tΔ1.Fatalf("CreateCertificate failed: %s", errΔ2);
            }
            (var cert, errΔ2) = ParseCertificate(certDER);
            if (errΔ2 != nil) {
                tΔ1.Fatalf("ParseCertificate failed: %s", errΔ2);
            }

            global::go.crypto.x509_package.VerifyOptions opts = default;
            if (tcʗ1.dnsName != ""u8) {
                opts.DNSName = tcʗ1.dnsName;
            }
            if (!tcʗ1.time.IsZero()) {
                opts.CurrentTime = tcʗ1.time;
            }
            if (builtin.len(tcʗ1.eku) > 0) {
                opts.KeyUsages = tcʗ1.eku;
            }

            @string expectedErr = tcʗ1.expectedErr;
            if (runtime.GOOS == "darwin"u8 && tcʗ1.macosErr != ""u8) {
                expectedErr = tcʗ1.macosErr;
            } else if (runtime.GOOS == "windows"u8 && tcʗ1.windowsErr != ""u8) {
                expectedErr = tcʗ1.windowsErr;
            }

            (_, errΔ2) = cert.Verify(opts);
            if (errΔ2 != nil && expectedErr == ""u8) {
                tΔ1.Errorf("unexpected verification error: %s", errΔ2);
            } else if (errΔ2 != nil && !strings.HasPrefix(errΔ2.Error(), expectedErr)) {
                tΔ1.Errorf("unexpected verification error: got %q, want %q", errΔ2.Error(), expectedErr);
            } else if (errΔ2 == nil && expectedErr != ""u8) {
                tΔ1.Errorf("unexpected verification success: want %q", expectedErr);
            }
        });
    }
}

} // end x509_internal_test_package
