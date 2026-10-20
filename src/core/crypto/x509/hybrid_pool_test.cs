// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using Δtls = go.crypto.tls_package;
using Δx509 = go.crypto.x509_package;
using pkix = go.crypto.x509.pkix_package;
using testenv = go.@internal.testenv_package;
using big = math.big_package;
using runtime = runtime_package;
using testing = testing_package;
using time = time_package;
using crypto = crypto_package;
using go.@internal;
using go.crypto;
using go.crypto.x509;
using io = io_package;
using math;
using static go.crypto.x509_internal_test_package;

partial class x509_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string googleCom443ˢ = "google.com:443"u8;
internal static readonly object windowsRootPoolAppearsToˢ = (@string)"windows root pool appears to be in an uninitialized state (missing root that chains to google.com)"u8;

public static void TestHybridPool(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    if (!(runtime.GOOS == "windows"u8 || runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8)) {
        Ꮡt.Skipf("platform verifier not available on %s"u8, runtime.GOOS);
    }
    if (!testenv.HasExternalNetwork()) {
        Ꮡt.Skip();
    }
    if (runtime.GOOS == "windows"u8) {
        // NOTE(#51599): on the Windows builders we sometimes see that the state
        // of the root pool is not fully initialized, causing an expected
        // platform verification to fail. In part this is because Windows
        // dynamically populates roots into its local trust store at time of
        // use. We can attempt to prime the pool by attempting TLS connections
        // to google.com until it works, suggesting the pool has been properly
        // updated. If after we hit the deadline, the pool has _still_ not been
        // populated with the expected root, it's unlikely we are ever going to
        // get into a good state, and so we just fail the test. #52108 suggests
        // a better possible long term solution.
        var deadline = time.Now().Add((time.Duration)(10000000000L));
        var nextSleep = 10 * time.Millisecond;
        for (nint i = 0; ᐧ ; i++) {
            var (cΔ1, errΔ1) = Δtls.Dial(tcpˢ, googleCom443ˢ, nil);
            if (errΔ1 == default!) {
                cΔ1.Close();
                break;
            }
            nextSleep = nextSleep * ((time.Duration)(int64)i);
            if (time.Until(deadline) < nextSleep) {
                Ꮡt.Fatal(windowsRootPoolAppearsToˢ);
            }
            time.Sleep(nextSleep);
        }
    }

    // Get the google.com chain, which should be valid on all platforms we
    // are testing
    var (c, err) = Δtls.Dial(tcpˢ, googleCom443ˢ, Ꮡ(new Δtls.Config(InsecureSkipVerify: true)));
    if (err != default!) {
        Ꮡt.Fatalf("tls connection failed: %s"u8, err);
    }
    var googChain = c.ConnectionState().PeerCertificates;

    var rootTmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "Go test root"u8),
        IsCA: true,
        BasicConstraintsValid: true,
        NotBefore: time.Now().Add(-time.ΔHour),
        NotAfter: time.Now().Add((time.Duration)(36000000000000L))
    ));
    (var k, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        Ꮡt.Fatalf("failed to generate test key: %s"u8, err);
    }
    (var rootDER, err) = Δx509.CreateCertificate(rand.Reader, rootTmpl, rootTmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("failed to create test cert: %s"u8, err);
    }
    (var root, err) = Δx509.ParseCertificate(rootDER);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse test cert: %s"u8, err);
    }

    (var pool, err) = Δx509.SystemCertPool();
    if (err != default!) {
        Ꮡt.Fatalf("SystemCertPool failed: %s"u8, err);
    }
    var opts = new Δx509.VerifyOptions(Roots: pool);

    (_, err) = googChain[0].Verify(opts);
    if (err != default!) {
        Ꮡt.Fatalf("verification failed for google.com chain (system only pool): %s"u8, err);
    }

    pool.AddCert(root);

    (_, err) = googChain[0].Verify(opts);
    if (err != default!) {
        Ꮡt.Fatalf("verification failed for google.com chain (hybrid pool): %s"u8, err);
    }

    var certTmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        NotBefore: time.Now().Add(-time.ΔHour),
        NotAfter: time.Now().Add((time.Duration)(36000000000000L)),
        DNSNames: new @string[]{"example.com"u8}.slice()
    ));
    (var certDER, err) = Δx509.CreateCertificate(rand.Reader, certTmpl, rootTmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("failed to create test cert: %s"u8, err);
    }
    (var cert, err) = Δx509.ParseCertificate(certDER);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse test cert: %s"u8, err);
    }

    (_, err) = cert.Verify(opts);
    if (err != default!) {
        Ꮡt.Fatalf("verification failed for custom chain (hybrid pool): %s"u8, err);
    }
}

} // end x509_test_package
