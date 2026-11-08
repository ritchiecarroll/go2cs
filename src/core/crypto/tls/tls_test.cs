// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using ecdh = go.crypto.ecdh_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using fips140 = go.crypto.fips140_package;
using boring = go.crypto.@internal.boring_package;
using rand = go.crypto.rand_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using json = encoding.json_package;
using pem = encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using math = math_package;
using big = go.math.big_package;
using net = net_package;
using os = os_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.tls.@internal;
using go.crypto.x509;
using go.math;
using hash = hash_package;
using static go.crypto.tls_package;
using vendor.golang.org.x.crypto;

partial class tls_internal_test_package {

internal static @string rsaCertPEM = """
-----BEGIN CERTIFICATE-----
MIIB0zCCAX2gAwIBAgIJAI/M7BYjwB+uMA0GCSqGSIb3DQEBBQUAMEUxCzAJBgNV
BAYTAkFVMRMwEQYDVQQIDApTb21lLVN0YXRlMSEwHwYDVQQKDBhJbnRlcm5ldCBX
aWRnaXRzIFB0eSBMdGQwHhcNMTIwOTEyMjE1MjAyWhcNMTUwOTEyMjE1MjAyWjBF
MQswCQYDVQQGEwJBVTETMBEGA1UECAwKU29tZS1TdGF0ZTEhMB8GA1UECgwYSW50
ZXJuZXQgV2lkZ2l0cyBQdHkgTHRkMFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBANLJ
hPHhITqQbPklG3ibCVxwGMRfp/v4XqhfdQHdcVfHap6NQ5Wok/4xIA+ui35/MmNa
rtNuC+BdZ1tMuVCPFZcCAwEAAaNQME4wHQYDVR0OBBYEFJvKs8RfJaXTH08W+SGv
zQyKn0H8MB8GA1UdIwQYMBaAFJvKs8RfJaXTH08W+SGvzQyKn0H8MAwGA1UdEwQF
MAMBAf8wDQYJKoZIhvcNAQEFBQADQQBJlffJHybjDGxRMqaRmDhX0+6v02TUKZsW
r5QuVbpQhH6u+0UgcW0jp9QwpxoPTLTWGXEWBBBurxFwiCBhkQ+V
-----END CERTIFICATE-----

"""u8.Raw();

internal static @string rsaKeyPEM = testingKey("""
-----BEGIN RSA TESTING KEY-----
MIIBOwIBAAJBANLJhPHhITqQbPklG3ibCVxwGMRfp/v4XqhfdQHdcVfHap6NQ5Wo
k/4xIA+ui35/MmNartNuC+BdZ1tMuVCPFZcCAwEAAQJAEJ2N+zsR0Xn8/Q6twa4G
6OB1M1WO+k+ztnX/1SvNeWu8D6GImtupLTYgjZcHufykj09jiHmjHx8u8ZZB/o1N
MQIhAPW+eyZo7ay3lMz1V01WVjNKK9QSn1MJlb06h/LuYv9FAiEA25WPedKgVyCW
SmUwbPw8fnTcpqDWE3yTO3vKcebqMSsCIBF3UmVue8YU3jybC3NxuXq3wNm34R8T
xVLHwDXh/6NJAiEAl2oHGGLz64BuAfjKrqwz7qMYr9HCLIe/YsoWq/olzScCIQDi
D2lWusoe2/nEqfDVVWGWlyJ7yOmqaVm/iNUN9B2N2g==
-----END RSA TESTING KEY-----

"""u8.Raw());

// keyPEM is the same as rsaKeyPEM, but declares itself as just
// "PRIVATE KEY", not "RSA PRIVATE KEY".  https://golang.org/issue/4477
internal static @string keyPEM = testingKey("""
-----BEGIN TESTING KEY-----
MIIBOwIBAAJBANLJhPHhITqQbPklG3ibCVxwGMRfp/v4XqhfdQHdcVfHap6NQ5Wo
k/4xIA+ui35/MmNartNuC+BdZ1tMuVCPFZcCAwEAAQJAEJ2N+zsR0Xn8/Q6twa4G
6OB1M1WO+k+ztnX/1SvNeWu8D6GImtupLTYgjZcHufykj09jiHmjHx8u8ZZB/o1N
MQIhAPW+eyZo7ay3lMz1V01WVjNKK9QSn1MJlb06h/LuYv9FAiEA25WPedKgVyCW
SmUwbPw8fnTcpqDWE3yTO3vKcebqMSsCIBF3UmVue8YU3jybC3NxuXq3wNm34R8T
xVLHwDXh/6NJAiEAl2oHGGLz64BuAfjKrqwz7qMYr9HCLIe/YsoWq/olzScCIQDi
D2lWusoe2/nEqfDVVWGWlyJ7yOmqaVm/iNUN9B2N2g==
-----END TESTING KEY-----

"""u8.Raw());

internal static @string ecdsaCertPEM = """
-----BEGIN CERTIFICATE-----
MIIB/jCCAWICCQDscdUxw16XFDAJBgcqhkjOPQQBMEUxCzAJBgNVBAYTAkFVMRMw
EQYDVQQIEwpTb21lLVN0YXRlMSEwHwYDVQQKExhJbnRlcm5ldCBXaWRnaXRzIFB0
eSBMdGQwHhcNMTIxMTE0MTI0MDQ4WhcNMTUxMTE0MTI0MDQ4WjBFMQswCQYDVQQG
EwJBVTETMBEGA1UECBMKU29tZS1TdGF0ZTEhMB8GA1UEChMYSW50ZXJuZXQgV2lk
Z2l0cyBQdHkgTHRkMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQBY9+my9OoeSUR
lDQdV/x8LsOuLilthhiS1Tz4aGDHIPwC1mlvnf7fg5lecYpMCrLLhauAc1UJXcgl
01xoLuzgtAEAgv2P/jgytzRSpUYvgLBt1UA0leLYBy6mQQbrNEuqT3INapKIcUv8
XxYP0xMEUksLPq6Ca+CRSqTtrd/23uTnapkwCQYHKoZIzj0EAQOBigAwgYYCQXJo
A7Sl2nLVf+4Iu/tAX/IF4MavARKC4PPHK3zfuGfPR3oCCcsAoz3kAzOeijvd0iXb
H5jBImIxPL4WxQNiBTexAkF8D1EtpYuWdlVQ80/h/f4pBcGiXPqX5h2PQSQY7hP1
+jwM1FGS4fREIOvlBYr/SzzQRtwrvrzGYxDEDbsC0ZGRnA==
-----END CERTIFICATE-----

"""u8.Raw();

internal static @string ecdsaKeyPEM = testingKey("""
-----BEGIN EC PARAMETERS-----
BgUrgQQAIw==
-----END EC PARAMETERS-----
-----BEGIN EC TESTING KEY-----
MIHcAgEBBEIBrsoKp0oqcv6/JovJJDoDVSGWdirrkgCWxrprGlzB9o0X8fV675X0
NwuBenXFfeZvVcwluO7/Q9wkYoPd/t3jGImgBwYFK4EEACOhgYkDgYYABAFj36bL
06h5JRGUNB1X/Hwuw64uKW2GGJLVPPhoYMcg/ALWaW+d/t+DmV5xikwKssuFq4Bz
VQldyCXTXGgu7OC0AQCC/Y/+ODK3NFKlRi+AsG3VQDSV4tgHLqZBBus0S6pPcg1q
kohxS/xfFg/TEwRSSws+roJr4JFKpO2t3/be5OdqmQ==
-----END EC TESTING KEY-----

"""u8.Raw());

partial struct keyPairTestsᴛ1 /*dyn*/ {
    internal @string algo;
    internal @string cert;
    internal @string key;
}
internal static slice<keyPairTestsᴛ1> keyPairTests = [
    new("ECDSA", ecdsaCertPEM, ecdsaKeyPEM),
    new("RSA", rsaCertPEM, rsaKeyPEM),
    new("RSA-untyped", rsaCertPEM, keyPEM) // golang.org/issue/4477
];

public static void TestX509KeyPair(ж<testing.T> t) {
    t.Parallel();
    slice<byte> pem = default;
    foreach (var (_, test) in range(keyPairTests)) {
        pem = slice<byte>(test.cert + test.key);
        {
            var (_, err) = X509KeyPair(pem, pem); if (err != nil) {
                t.Errorf("Failed to load %s cert followed by %s key: %s", test.algo, test.algo, err);
            }
        }
        pem = slice<byte>(test.key + test.cert);
        {
            var (_, err) = X509KeyPair(pem, pem); if (err != nil) {
                t.Errorf("Failed to load %s key followed by %s cert: %s", test.algo, test.algo, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string beenSwitchedˢ => ref beenSwitchedˢᵇ;
private static readonly @string beenSwitchedˢᵇ = "been switched"u8;
internal static ref readonly @string certificateˢ => ref certificateˢᵇ;
private static readonly @string certificateˢᵇ = "certificate"u8;
internal static ref readonly @string nonsenseˢ => ref nonsenseˢᵇ;
private static readonly @string nonsenseˢᵇ = "NONSENSE"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string nonsensePEMᶜ => ref nonsensePEMᶜᵇ;
private static readonly @string nonsensePEMᶜᵇ = """

-----BEGIN NONSENSE-----
Zm9vZm9vZm9v
-----END NONSENSE-----

"""u8.Raw();

public static void TestX509KeyPairErrors(ж<testing.T> t) {
    var (_, err) = X509KeyPair(slice<byte>(rsaKeyPEM), slice<byte>(rsaCertPEM));
    if (err == nil) {
        t.Fatalf("X509KeyPair didn't return an error when arguments were switched");
    }
    {
        @string subStr = beenSwitchedˢ; if (!strings.Contains(err.Error(), subStr)) {
            t.Fatalf("Expected %q in the error when switching arguments to X509KeyPair, but the error was %q", subStr, err);
        }
    }

    (_, err) = X509KeyPair(slice<byte>(rsaCertPEM), slice<byte>(rsaCertPEM));
    if (err == nil) {
        t.Fatalf("X509KeyPair didn't return an error when both arguments were certificates");
    }
    {
        @string subStr = certificateˢ; if (!strings.Contains(err.Error(), subStr)) {
            t.Fatalf("Expected %q in the error when both arguments to X509KeyPair were certificates, but the error was %q", subStr, err);
        }
    }

    @string nonsensePEM = nonsensePEMᶜ;

    (_, err) = X509KeyPair(slice<byte>(nonsensePEM), slice<byte>(nonsensePEM));
    if (err == nil) {
        t.Fatalf("X509KeyPair didn't return an error when both arguments were nonsense");
    }
    {
        @string subStr = nonsenseˢ; if (!strings.Contains(err.Error(), subStr)) {
            t.Fatalf("Expected %q in the error when both arguments to X509KeyPair were nonsense, but the error was %q", subStr, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object loadOfRsaCertificateˢ = (@string)"Load of RSA certificate succeeded with ECDSA private key"u8;
internal static readonly object loadOfEcdsaCertificateˢ = (@string)"Load of ECDSA certificate succeeded with RSA private key"u8;

public static void TestX509MixedKeyPair(ж<testing.T> t) {
    {
        var (_, err) = X509KeyPair(slice<byte>(rsaCertPEM), slice<byte>(ecdsaKeyPEM)); if (err == nil) {
            t.Error(loadOfRsaCertificateˢ);
        }
    }
    {
        var (_, err) = X509KeyPair(slice<byte>(ecdsaCertPEM), slice<byte>(rsaKeyPEM)); if (err == nil) {
            t.Error(loadOfEcdsaCertificateˢ);
        }
    }
}

internal static net.Listener newLocalListener(testing.TB t) {
    t.Helper();
    var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0");
    if (err != nil) {
        (ln, err) = net.Listen(tcp6ˢ, "[::1]:0");
    }
    if (err != nil) {
        t.Fatal(err);
    }
    return ln;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fips140tlsˢ => ref fips140tlsˢᵇ;
private static readonly @string fips140tlsˢᵇ = "fips140tls"u8;

internal static void runWithFIPSEnabled(ж<testing.T> t, Action<ж<testing.T>> testFunc) {
    GoFrame ᒐ = default;
    try {
        var originalFIPS = fips140tls.Required();
        defer(() => {
            if (originalFIPS) {
                fips140tls.Force();
            } else {
                fips140tls.TestingOnlyAbandon();
            }
        }, ref ᒐ);

        fips140tls.Force();
        t.Run(fips140tlsˢ, testFunc);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noFips140tlsˢ => ref noFips140tlsˢᵇ;
private static readonly @string noFips140tlsˢᵇ = "no-fips140tls"u8;
internal static readonly object canTRunNoFips140tlsTestsˢ = (@string)"can't run no-fips140tls tests in fips140=only mode"u8;

internal static void runWithFIPSDisabled(ж<testing.T> t, Action<ж<testing.T>> testFunc) {
    GoFrame ᒐ = default;
    try {
        if (fips140.Enforced()) {
            t.Run(noFips140tlsˢ, (ж<testing.T> tΔ1) => {
                tΔ1.Skip(canTRunNoFips140tlsTestsˢ);
            });
            return;
        }

        var originalFIPS = fips140tls.Required();
        defer(() => {
            if (originalFIPS) {
                fips140tls.Force();
            } else {
                fips140tls.TestingOnlyAbandon();
            }
        }, ref ᒐ);

        fips140tls.TestingOnlyAbandon();
        t.Run(noFips140tlsˢ, testFunc);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInFipsModeˢ = (@string)"skipping test in FIPS mode"u8;

internal static partial void skipFIPS(ж<testing.T> t) {
    if (fips140tls.Required()) {
        t.Skip(skippingTestInFipsModeˢ);
    }
}

public static partial void TestDialTimeout(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }

    ref var timeout = ref heap<time.Duration>(100 * time_package.Microsecond, out var Ꮡtimeout);
    while (!t.Failed()) {
        var acceptc = new channel<net.Conn>(0);
        var listener = newLocalListener(new tls_test_package.testing_TжTB(t));
        var acceptcʗ1 = acceptc;
        var listenerʗ1 = listener;
        goǃ(() => {
            for (;;) {
                var (conn, err) = listenerʗ1.Accept();
                if (err != nil) {
                    close(acceptcʗ1);
                    return;
                }
                acceptcʗ1.ᐸꟷ(conn);
            }
        });

        @string addr = listener.Addr().String();
        var dialer = Ꮡ(new net.Dialer(
            Timeout: timeout
        ));
        {
            var (conn, err) = DialWithDialer(dialer, tcpˢ, addr, nil); if (err == nil) {
                conn.Close();
                t.Errorf("DialWithTimeout unexpectedly completed successfully");
            } else if (!isTimeoutError(err)) {
                t.Errorf("resulting error not a timeout: %v\nType %T: %#v", err, err, err);
            }
        }

        listener.Close();

        // We're looking for a timeout during the handshake, so check that the
        // Listener actually accepted the connection to initiate it. (If the server
        // takes too long to accept the connection, we might cancel before the
        // underlying net.Conn is ever dialed — without ever attempting a
        // handshake.)
        var (lconn, ok) = ᐸꟷ(acceptc, ꟷ);
        if (ok) {
            // The Listener accepted a connection, so assume that it was from our
            // Dial: we triggered the timeout at the point where we wanted it!
            t.Logf("Listener accepted a connection from %s", lconn.RemoteAddr());
            lconn.Close();
        }
        // Close any spurious extra connections from the listener. (This is
        // possible if there are, for example, stray Dial calls from other tests.)
        foreach (var extraConn in acceptc) {
            t.Logf("spurious extra connection from %s", extraConn.RemoteAddr());
            extraConn.Close();
        }
        if (ok) {
            break;
        }

        t.Logf("with timeout %v, DialWithDialer returned before listener accepted any connections; retrying", timeout);
        timeout *= 2;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeShouldHaveTimedOutˢ = (@string)"Write should have timed out"u8;
internal static readonly object writeWhichPreviouslyˢ = (@string)"Write which previously failed should still time out"u8;
internal static readonly object writeTimedOutButˢ = (@string)"Write timed out but incorrectly classified the error as Temporary"u8;
internal static readonly object writeTimedOutButDidNotˢ = (@string)"Write timed out but did not classify the error as a Timeout"u8;

public static partial void TestDeadlineOnWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(skippingInShortModeˢ);
        }

        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var srvCh = new channel<ж<global::go.crypto.tls_package.Conn>>(1);

        var lnʗ2 = ln;
        var srvChʗ1 = srvCh;
        goǃ(() => {
            var (sconn, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != nil) {
                srvChʗ1.ᐸꟷ(default);
                return;
            }
            var srvΔ1 = Server(sconn, testConfig.Clone());
            var errΔ2 = srvΔ1.Handshake(); if (errΔ2 != nil) {
                srvChʗ1.ᐸꟷ(default);
                return;
            }
            srvChʗ1.ᐸꟷ(srvΔ1);
        });

        var clientConfig = testConfig.Clone();
        clientConfig.MaxVersion = VersionTLS12;
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        var srv = ᐸꟷ(srvCh);
        if (srv == nil) {
            t.Error(err);
        }

        // Make sure the client/server is setup correctly and is able to do a typical Write/Read
        var buf = new slice<byte>(6);
        var (_, errΔ3) = srv.Write(slice<byte>("foobar"u8)); if (errΔ3 != nil) {
            t.Errorf("Write err: %v", errΔ3);
        }
        var (n, errΔ4) = conn.Read(buf); if (n != 6 || errΔ4 != nil || ((sstring)buf) != "foobar"u8) {
            t.Errorf("Read = %d, %v, data %q; want 6, nil, foobar", n, errΔ4, buf);
        }

        // Set a deadline which should cause Write to timeout
        err = srv.SetDeadline(time_package.Now()); if (err != nil) {
            t.Fatalf("SetDeadline(time.Now()) err: %v", err);
        }
        (_, err) = srv.Write(slice<byte>("should fail"u8)); if (err == nil) {
            t.Fatal(writeShouldHaveTimedOutˢ);
        }

        // Clear deadline and make sure it still times out
        err = srv.SetDeadline(new time_package.Time()); if (err != nil) {
            t.Fatalf("SetDeadline(time.Time{}) err: %v", err);
        }
        (_, err) = srv.Write(slice<byte>("This connection is permanently broken"u8)); if (err == nil) {
            t.Fatal(writeWhichPreviouslyˢ);
        }

        // Verify the error
        var ne = err._<netꓸError>(); if (ne.Temporary() != false) {
            t.Error(writeTimedOutButˢ);
        }
        if (!isTimeoutError(err)) {
            t.Error(writeTimedOutButDidNotˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal delegate (nint, error) readerFunc(slice<byte> _Δp0);

internal static (nint, error) Read(this readerFunc f, slice<byte> b) { return f(b); }

// TestDialer tests that tls.Dialer.DialContext can abort in the middle of a handshake.
// (The other cases are all handled by the existing dial tests in this package, which
// all also flow through the same code shared code paths)
public static partial void TestDialer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var unblockServer = new channel<EmptyStruct>(0); // close-only
        defer(ᴛ1 => close(ᴛ1), unblockServer, ref ᒐ);
        var lnʗ2 = ln;
        var unblockServerʗ1 = unblockServer;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (conn, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != nil) {
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                ᐸꟷ(unblockServerʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var (ctx, cancel) = context.WithCancel(context.Background());
            var cancelʗ1 = cancel;
        var d = new Dialer(Config: Ꮡ(new Config(
            Rand: new tls_internal_test_package.readerFuncᴠReader(new readerFunc((slice<byte> b) => {
                // By the time crypto/tls wants randomness, that means it has a TCP
                // connection, so we're past the Dialer's dial and now blocked
                // in a handshake. Cancel our context and see if we get unstuck.
                // (Our TCP listener above never reads or writes, so the Handshake
                // would otherwise be stuck forever)
                cancelʗ1();
                return (len(b), default);
            })),
            ServerName: "foo"
        ))
        );
        var (_, err) = d.DialContext(ctx, tcpˢ, ln.Addr().String());
        if (!AreEqual(err, context.Canceled)) {
            t.Errorf("err = %v; want context.Canceled", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool isTimeoutError(error err) {
    var (ne, ok) = err._<netꓸError>(ᐧ); if (ok) {
        return ne.Timeout();
    }
    return false;
}

// tests that Conn.Read returns (non-zero, io.EOF) instead of
// (non-zero, nil) when a Close (alertCloseNotify) is sitting right
// behind the application data in the buffer.
public static void TestConnReadNonzeroAndEOF(ж<testing.T> t) {
    // This test is racy: it assumes that after a write to a
    // localhost TCP connection, the peer TCP connection can
    // immediately read it. Because it's racy, we skip this test
    // in short mode, and then retry it several times with an
    // increasing sleep in between our final write (via srv.Close
    // below) and the following read.
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    error err = default;
    for (var delay = time_package.Millisecond; delay <= 64 * time_package.Millisecond; delay *= 2) {
        err = testConnReadNonzeroAndEOF(t, delay); if (err == nil) {
            return;
        }
    }
    t.Error(err);
}

internal static partial error testConnReadNonzeroAndEOF(ж<testing.T> t, time.Duration delay) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var srvCh = new channel<ж<global::go.crypto.tls_package.Conn>>(1);
        ref var serr = ref heap<error>(out var Ꮡserr);
        var lnʗ2 = ln;
        var srvChʗ1 = srvCh;
        goǃ(() => {
            var (sconn, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != nil) {
                Ꮡserr.ValueSlot = errΔ1;
                srvChʗ1.ᐸꟷ(default);
                return;
            }
            var serverConfig = testConfig.Clone();
            var srvΔ1 = Server(sconn, serverConfig);
            var errΔ2 = srvΔ1.Handshake(); if (errΔ2 != nil) {
                Ꮡserr.ValueSlot = fmt.Errorf("handshake: %v"u8, errΔ2);
                srvChʗ1.ᐸꟷ(default);
                return;
            }
            srvChʗ1.ᐸꟷ(srvΔ1);
        });

        var clientConfig = testConfig.Clone();
        // In TLS 1.3, alerts are encrypted and disguised as application data, so
        // the opportunistic peek won't work.
        clientConfig.MaxVersion = VersionTLS12;
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        var srv = ᐸꟷ(srvCh);
        if (srv == nil) {
            return serr;
        }

        var buf = new slice<byte>(6);

        srv.Write(slice<byte>("foobar"u8));
        (var n, err) = conn.Read(buf);
        if (n != 6 || err != nil || ((sstring)buf) != "foobar"u8) {
            return fmt.Errorf("Read = %d, %v, data %q; want 6, nil, foobar"u8, n, err, buf);
        }

        srv.Write(slice<byte>("abcdef"u8));
        srv.Close();
        time_package.Sleep(delay);
        (n, err) = conn.Read(buf);
        if (n != 6 || ((sstring)buf) != "abcdef"u8) {
            return fmt.Errorf("Read = %d, buf= %q; want 6, abcdef"u8, n, buf);
        }
        if (!AreEqual(err, io.EOF)) {
            return fmt.Errorf("Second Read error = %v; want io.EOF"u8, err);
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientAndServerChannelˢ = (@string)"client and server channel bindings differ"u8;
internal static readonly object tlsUniqueIsEmptyOrZeroˢ = (@string)"tls-unique is empty or zero"u8;
internal static readonly object secondSessionDidNotUseˢ = (@string)"second session did not use resumption"u8;
internal static readonly object clientAndServerChannelˢ2 = (@string)"client and server channel bindings differ when session resumption is used"u8;
internal static readonly object resumptionTlsUniqueIsˢ = (@string)"resumption tls-unique is empty or zero"u8;

public static partial void TestTLSUniqueMatches(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var serverTLSUniques = new channel<slice<byte>>(0);
        var parentDone = new channel<EmptyStruct>(0);
        var childDone = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), parentDone, ref ᒐ);
        var childDoneʗ1 = childDone;
        var lnʗ2 = ln;
        var parentDoneʗ1 = parentDone;
        var serverTLSUniquesʗ1 = serverTLSUniques;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), childDoneʗ1, ref ᒐ);
                for (nint i = 0; i < 2; i++) {
                    var (sconn, errΔ1) = lnʗ2.Accept();
                    if (errΔ1 != nil) {
                        t.Error(errΔ1);
                        return;
                    }
                    var serverConfig = testConfig.Clone();
                    serverConfig.MaxVersion = VersionTLS12; // TLSUnique is not defined in TLS 1.3
                    var srv = Server(sconn, serverConfig);
                    var errΔ2 = srv.Handshake(); if (errΔ2 != nil) {
                        t.Error(errΔ2);
                        return;
                    }
                    var selᴛ8 = parentDoneʗ1;
                    var selᴛ9 = serverTLSUniquesʗ1.ᐸꟷ(srv.ConnectionState().TLSUnique, ꓸꓸꓸ);
                    switch (select(ᐸꟷ(selᴛ8, ꓸꓸꓸ), selᴛ9)) {
                    case 0 when selᴛ8.ꟷᐳ(out _): {
                        return;
                    }
                    case 1: {
                        break;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var clientConfig = testConfig.Clone();
        clientConfig.ClientSessionCache = NewLRUClientSessionCache(1);
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != nil) {
            t.Fatal(err);
        }

        slice<byte> serverTLSUniquesValue = default;
        var selᴛ10 = childDone;
        var selᴛ11 = serverTLSUniques;
        switch (select(ᐸꟷ(selᴛ10, ꓸꓸꓸ), ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ10.ꟷᐳ(out _): {
            return;
        }
        case 1 when selᴛ11.ꟷᐳ(out serverTLSUniquesValue): {
            break;
        }}

        if (!bytes.Equal(conn.ConnectionState().TLSUnique, serverTLSUniquesValue)) {
            t.Error(clientAndServerChannelˢ);
        }
        if (serverTLSUniquesValue == nil || bytes.Equal(serverTLSUniquesValue, new slice<byte>(12))) {
            t.Error(tlsUniqueIsEmptyOrZeroˢ);
        }
        conn.Close();

        (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        if (!conn.ConnectionState().DidResume) {
            t.Error(secondSessionDidNotUseˢ);
        }

        var selᴛ12 = childDone;
        var selᴛ13 = serverTLSUniques;
        switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
        case 0 when selᴛ12.ꟷᐳ(out _): {
            return;
        }
        case 1 when selᴛ13.ꟷᐳ(out serverTLSUniquesValue): {
            break;
        }}

        if (!bytes.Equal(conn.ConnectionState().TLSUnique, serverTLSUniquesValue)) {
            t.Error(clientAndServerChannelˢ2);
        }
        if (serverTLSUniquesValue == nil || bytes.Equal(serverTLSUniquesValue, new slice<byte>(12))) {
            t.Error(resumptionTlsUniqueIsˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wwwGoogleComHttpsˢ => ref wwwGoogleComHttpsˢᵇ;
private static readonly @string wwwGoogleComHttpsˢᵇ = "www.google.com:https"u8;
internal static ref readonly @string wwwGoogleComˢ => ref wwwGoogleComˢᵇ;
private static readonly @string wwwGoogleComˢᵇ = "www.google.com"u8;
internal static ref readonly @string wwwYahooComˢ => ref wwwYahooComˢᵇ;
private static readonly @string wwwYahooComˢᵇ = "www.yahoo.com"u8;

public static void TestVerifyHostname(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new tls_test_package.testing_TжTB(t));

    var (c, err) = Dial(tcpˢ, wwwGoogleComHttpsˢ, nil);
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ1 = c.VerifyHostname(wwwGoogleComˢ); if (errΔ1 != nil) {
        t.Fatalf("verify www.google.com: %v", errΔ1);
    }
    var errΔ2 = c.VerifyHostname(wwwYahooComˢ); if (errΔ2 == nil) {
        t.Fatalf("verify www.yahoo.com succeeded");
    }

    (c, err) = Dial(tcpˢ, wwwGoogleComHttpsˢ, Ꮡ(new Config(InsecureSkipVerify: true)));
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ3 = c.VerifyHostname(wwwGoogleComˢ); if (errΔ3 == nil) {
        t.Fatalf("verify www.google.com succeeded with InsecureSkipVerify=true");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string yahooCom443ˢ => ref yahooCom443ˢᵇ;
private static readonly @string yahooCom443ˢᵇ = "yahoo.com:443"u8;
internal static readonly object dialErrorˢ = (@string)"Dial error:"u8;
internal static readonly object secondDialErrorˢ = (@string)"second Dial error:"u8;
internal static readonly object noConnectionUsedSessionˢ = (@string)"no connection used session resumption"u8;

public static void TestRealResumption(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new tls_test_package.testing_TжTB(t));

    var config = Ꮡ(new Config(
        ServerName: "yahoo.com",
        ClientSessionCache: NewLRUClientSessionCache(0)
    ));

    foreach (var _ᴛ1 in range(10)) {
        var (conn, err) = Dial(tcpˢ, yahooCom443ˢ, config);
        if (err != nil) {
            t.Log(dialErrorˢ, err);
            continue;
        }
        // Do a read to consume the NewSessionTicket messages.
        fmt.Fprintf(new tls_test_package.tls_ConnжWriter(conn), "GET / HTTP/1.1\r\nHost: yahoo.com\r\nConnection: close\r\n\r\n"u8);
        conn.Read(new slice<byte>(4096));
        conn.Close();

        (conn, err) = Dial(tcpˢ, yahooCom443ˢ, config);
        if (err != nil) {
            t.Log(secondDialErrorˢ, err);
            continue;
        }
        var state = conn.ConnectionState();
        conn.Close();

        if (state.DidResume) {
            return;
        }
    }

    t.Fatal(noConnectionUsedSessionˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string connClosedForTestˢ => ref connClosedForTestˢᵇ;
private static readonly @string connClosedForTestˢᵇ = "conn closed for test"u8;

public static partial void TestConnCloseBreakingWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var srvCh = new channel<ж<global::go.crypto.tls_package.Conn>>(1);
        ref var serr = ref heap<error>(out var Ꮡserr);
        ref var sconn = ref heap<net.Conn>(out var Ꮡsconn);
        var lnʗ2 = ln;
        var srvChʗ1 = srvCh;
        goǃ(() => {
            error errΔ1 = default;
            (Ꮡsconn.ValueSlot, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != nil) {
                Ꮡserr.ValueSlot = errΔ1;
                srvChʗ1.ᐸꟷ(default);
                return;
            }
            var serverConfig = testConfig.Clone();
            var srvΔ1 = Server(Ꮡsconn.ValueSlot, serverConfig);
            var errΔ2 = srvΔ1.Handshake(); if (errΔ2 != nil) {
                Ꮡserr.ValueSlot = fmt.Errorf("handshake: %v"u8, errΔ2);
                srvChʗ1.ᐸꟷ(default);
                return;
            }
            srvChʗ1.ᐸꟷ(srvΔ1);
        });

        var (cconn, err) = net.Dial(tcpˢ, ln.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        var cconnʗ1 = cconn;
        defer(() => cconnʗ1.Close(), ref ᒐ);

        var conn = Ꮡ(new changeImplConn(
            Conn: cconn
        ));

        var clientConfig = testConfig.Clone();
        var tconn = Client(new tls_internal_test_package.changeImplConnжConn(conn), clientConfig);
        var errΔ3 = tconn.Handshake(); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }

        var srv = ᐸꟷ(srvCh);
        if (srv == nil) {
            t.Fatal(serr);
        }
        defer(() => Ꮡsconn.ValueSlot.Close(), ref ᒐ);

        var connClosed = new channel<EmptyStruct>(0);
        var connClosedʗ1 = connClosed;
        conn.closeFunc = error () => {
            close(connClosedʗ1);
            return default;
        };

        var inWrite = new channel<bool>(1);
        error errConnClosed = errors.New(connClosedForTestˢ);
        var connClosedʗ2 = connClosed;
        var errConnClosedʗ1 = errConnClosed;
        var inWriteʗ1 = inWrite;
        conn.writeFunc = (slice<byte> p) => {
            inWriteʗ1.ᐸꟷ(true);
            ᐸꟷ(connClosedʗ2);
            return (0, errConnClosedʗ1);
        };

        var closeReturned = new channel<bool>(1);
        var closeReturnedʗ1 = closeReturned;
        var inWriteʗ2 = inWrite;
        var tconnʗ1 = tconn;
        goǃ(() => {
            ᐸꟷ(inWriteʗ2);
            tconnʗ1.Close(); // test that this doesn't block forever.
            closeReturnedʗ1.ᐸꟷ(true);
        });

        (_, err) = tconn.Write(slice<byte>("foo"u8));
        if (!AreEqual(err, errConnClosed)) {
            t.Errorf("Write error = %v; want errConnClosed", err);
        }

        ᐸꟷ(closeReturned);
        var errΔ5 = tconn.Close(); if (!AreEqual(errΔ5, net.ErrClosed)) {
            t.Errorf("Close error = %v; want net.ErrClosed", errΔ5);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deadlockˢ = (@string)"deadlock"u8;

public static partial void TestConnCloseWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var clientDoneChan = new channel<EmptyStruct>(0);

        var clientDoneChanʗ1 = clientDoneChan;
        var lnʗ2 = ln;
        error serverCloseWrite() {
            GoFrame ᒐ = default;
            try {
                var (sconn, err) = lnʗ2.Accept();
                if (err != nil) {
                    return fmt.Errorf("accept: %v"u8, err);
                }
                var sconnʗ1 = sconn;
                defer(() => sconnʗ1.Close(), ref ᒐ);

                var serverConfig = testConfig.Clone();
                var srv = Server(sconn, serverConfig);
                var errΔ1 = srv.Handshake(); if (errΔ1 != nil) {
                    return fmt.Errorf("handshake: %v"u8, errΔ1);
                }
                var srvʗ1 = srv;
                defer(() => srvʗ1.Close(), ref ᒐ);

                (var data, err) = io.ReadAll(new tls_test_package.tls_ConnжReader(srv));
                if (err != nil) {
                    return err;
                }
                if (len(data) > 0) {
                    return fmt.Errorf("Read data = %q; want nothing"u8, data);
                }

                var errΔ2 = srv.CloseWrite(); if (errΔ2 != nil) {
                    return fmt.Errorf("server CloseWrite: %v"u8, errΔ2);
                }

                // Wait for clientCloseWrite to finish, so we know we
                // tested the CloseWrite before we defer the
                // sconn.Close above, which would also cause the
                // client to unblock like CloseWrite.
                ᐸꟷ(clientDoneChanʗ1);
                return default;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        }

        var clientDoneChanʗ2 = clientDoneChan;
        var lnʗ3 = ln;
        error clientCloseWrite() {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), clientDoneChanʗ2, ref ᒐ);

                var clientConfig = testConfig.Clone();
                var (conn, err) = Dial(tcpˢ, lnʗ3.Addr().String(), clientConfig);
                if (err != nil) {
                    return err;
                }
                var errΔ1 = conn.Handshake(); if (errΔ1 != nil) {
                    return errΔ1;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);

                var errΔ2 = conn.CloseWrite(); if (errΔ2 != nil) {
                    return fmt.Errorf("client CloseWrite: %v"u8, errΔ2);
                }

                var (_, errΔ3) = conn.Write([0]); if (!AreEqual(errΔ3, errShutdown)) {
                    return fmt.Errorf("CloseWrite error = %v; want errShutdown"u8, errΔ3);
                }

                (var data, err) = io.ReadAll(new tls_test_package.tls_ConnжReader(conn));
                if (err != nil) {
                    return err;
                }
                if (len(data) > 0) {
                    return fmt.Errorf("Read data = %q; want nothing"u8, data);
                }
                return default;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        }

        var errChan = new channel<error>(2);

        var errChanʗ1 = errChan;
        var serverCloseWriteʗ1 = serverCloseWrite;
        goǃ(() => {
            errChanʗ1.ᐸꟷ(serverCloseWriteʗ1());
        });
        var clientCloseWriteʗ1 = clientCloseWrite;
        var errChanʗ2 = errChan;
        goǃ(() => {
            errChanʗ2.ᐸꟷ(clientCloseWriteʗ1());
        });

        for (nint i = 0; i < 2; i++) {
            var selᴛ14 = errChan;
            var selᴛ15 = time_package.After((time.Duration)(10000000000L));
            switch (select(ᐸꟷ(selᴛ14, ꓸꓸꓸ), ᐸꟷ(selᴛ15, ꓸꓸꓸ))) {
            case 0 when selᴛ14.ꟷᐳ(out var err): {
                if (err != nil) {
                    t.Fatal(err);
                }
                break;
            }
            case 1 when selᴛ15.ꟷᐳ(out _): {
                t.Fatal(deadlockˢ);
                break;
            }}
        }

        // Also test CloseWrite being called before the handshake is
        // finished:
        {
            var ln2 = newLocalListener(new tls_test_package.testing_TжTB(t));
            var ln2ʗ1 = ln2;
            defer(() => ln2ʗ1.Close(), ref ᒐ);

            var (netConn, err) = net.Dial(tcpˢ, ln2.Addr().String());
            if (err != nil) {
                t.Fatal(err);
            }
            var netConnʗ1 = netConn;
            defer(() => netConnʗ1.Close(), ref ᒐ);
            var conn = Client(netConn, testConfig.Clone());

            var errΔ1 = conn.CloseWrite(); if (!AreEqual(errΔ1, errEarlyCloseWrite)) {
                t.Errorf("CloseWrite error = %v; want errEarlyCloseWrite", errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedLackOfErrorˢ => ref unexpectedLackOfErrorˢᵇ;
private static readonly @string unexpectedLackOfErrorˢᵇ = "unexpected lack of error from server"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedᶜ => ref expectedᶜᵇ;
private static readonly @string expectedᶜᵇ = "too many ignored"u8;

public static partial void TestWarningAlertFlood(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(t));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var lnʗ2 = ln;
        error server() {
            GoFrame ᒐ = default;
            try {
                var (sconn, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != nil) {
                    return fmt.Errorf("accept: %v"u8, errΔ1);
                }
                var sconnʗ1 = sconn;
                defer(() => sconnʗ1.Close(), ref ᒐ);

                var serverConfig = testConfig.Clone();
                var srv = Server(sconn, serverConfig);
                var errΔ2 = srv.Handshake(); if (errΔ2 != nil) {
                    return fmt.Errorf("handshake: %v"u8, errΔ2);
                }
                var srvʗ1 = srv;
                defer(() => srvʗ1.Close(), ref ᒐ);

                (_, errΔ1) = io.ReadAll(new tls_test_package.tls_ConnжReader(srv));
                if (errΔ1 == nil) {
                    return errors.New(unexpectedLackOfErrorˢ);
                }
                @string expected = expectedᶜ;
                @string str = errΔ1.Error(); if (!strings.Contains(str, expected)) {
                    return fmt.Errorf("expected error containing %q, but saw: %s"u8, expected, str);
                }

                return default;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        }

        var errChan = new channel<error>(1);
        var errChanʗ1 = errChan;
        var serverʗ1 = server;
        goǃ(() => {
            errChanʗ1.ᐸꟷ(serverʗ1());
        });

        var clientConfig = testConfig.Clone();
        clientConfig.MaxVersion = VersionTLS12; // there are no warning alerts in TLS 1.3
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var errΔ3 = conn.Handshake(); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }

        for (nint i = 0; i < (nint)(maxUselessRecords + 1); i++) {
            conn.sendAlert(alertNoRenegotiation);
        }

        var errΔ4 = ᐸꟷ(errChan); if (errΔ4 != nil) {
            t.Fatal(errΔ4);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCloneFuncFields(ж<testing.T> t) {
    UntypedInt expectedCount = 10;
    nint called = 0;

    ref var c1 = ref heap<global::go.crypto.tls_package.Config>(new Config(
        Time: () => {
            called |= (nint)((1 << 0));
            return new time_package.Time();
        },
        GetCertificate: (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => {
            called |= (nint)((1 << 1));
            return (default, default);
        },
        GetClientCertificate: (ж<global::go.crypto.tls_package.CertificateRequestInfo> _) => {
            called |= (nint)((1 << 2));
            return (default, default);
        },
        GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => {
            called |= (nint)((1 << 3));
            return (default, default);
        },
        VerifyPeerCertificate: (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> verifiedChains) => {
            called |= (nint)((1 << 4));
            return default;
        },
        VerifyConnection: (global::go.crypto.tls_package.ΔConnectionState _) => {
            called |= (nint)((1 << 5));
            return default;
        },
        UnwrapSession: (slice<byte> identity, global::go.crypto.tls_package.ΔConnectionState cs) => {
            called |= (nint)((1 << 6));
            return (default, default);
        },
        WrapSession: (global::go.crypto.tls_package.ΔConnectionState cs, ж<global::go.crypto.tls_package.SessionState> ss) => {
            called |= (nint)((1 << 7));
            return (default, default);
        },
        EncryptedClientHelloRejectionVerify: (global::go.crypto.tls_package.ΔConnectionState _) => {
            called |= (nint)((1 << 8));
            return default;
        },
        GetEncryptedClientHelloKeys: (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => {
            called |= (nint)((1 << 9));
            return (default, default);
        }
    ), out var Ꮡc1);

    var c2 = Ꮡc1.Clone();

    c2.Time();
    c2.GetCertificate(nil);
    c2.GetClientCertificate(nil);
    c2.GetConfigForClient(nil);
    c2.VerifyPeerCertificate(default, default);
    c2.VerifyConnection(new ΔConnectionState());
    c2.UnwrapSession(default, new ΔConnectionState());
    c2.WrapSession(new ΔConnectionState(), nil);
    c2.EncryptedClientHelloRejectionVerify(new ΔConnectionState());
    c2.GetEncryptedClientHelloKeys(nil);

    if (called != (nint)((1 << (int)(expectedCount)) - 1)) {
        t.Fatalf("expected %d calls but saw calls %b", (nint)(expectedCount), called);
    }
}

public static void TestCloneNonFuncFields(ж<testing.T> t) {
    ref var c1 = ref heap(new global::go.crypto.tls_package.Config(), out var Ꮡc1);
    var v = reflect.ValueOf(Ꮡc1).Elem();

    var typ = v.Type();
    for (nint i = 0; i < typ.NumField(); i++) {
        var f = v.Field(i);
        // testing/quick can't handle functions or interfaces and so
        // isn't used here.
        @string fn = typ.Field(i).Name;
        var exprᴛ1 = fn;
        switch (ᐧ) {
        case {} when exprᴛ1 == "Rand"u8: {
            f.Set(reflect.ValueOf((io.Reader)new tls_test_package.os_FileжReader(os.Stdin)));
            break;
        }
        case {} when exprᴛ1 == "Time"u8 || exprᴛ1 == "GetCertificate"u8 || exprᴛ1 == "GetConfigForClient"u8 || exprᴛ1 == "VerifyPeerCertificate"u8 || exprᴛ1 == "VerifyConnection"u8 || exprᴛ1 == "GetClientCertificate"u8 || exprᴛ1 == "WrapSession"u8 || exprᴛ1 == "UnwrapSession"u8 || exprᴛ1 == "EncryptedClientHelloRejectionVerify"u8 || exprᴛ1 == "GetEncryptedClientHelloKeys"u8: {
            // DeepEqual can't compare functions. If you add a
            // function field to this list, you must also change
            // TestCloneFuncFields to ensure that the func field is
            // cloned.
            break;
        }
        case {} when exprᴛ1 == "Certificates"u8: {
            f.Set(reflect.ValueOf(slice<global::go.crypto.tls_package.Certificate>([
                new(ΔCertificate: [[(byte)'b']])
            ])));
            break;
        }
        case {} when exprᴛ1 == "NameToCertificate"u8: {
            f.Set(reflect.ValueOf(new map<@string, ж<global::go.crypto.tls_package.Certificate>>{["a"] = default}));
            break;
        }
        case {} when exprᴛ1 == "RootCAs"u8 || exprᴛ1 == "ClientCAs"u8: {
            f.Set(reflect.ValueOf(Δx509.NewCertPool().OrTypedNil()));
            break;
        }
        case {} when exprᴛ1 == "ClientSessionCache"u8: {
            f.Set(reflect.ValueOf(NewLRUClientSessionCache(10)));
            break;
        }
        case {} when exprᴛ1 == "KeyLogWriter"u8: {
            f.Set(reflect.ValueOf((io.Writer)new os.FileжWriter(os.Stdout)));
            break;
        }
        case {} when exprᴛ1 == "NextProtos"u8: {
            f.Set(reflect.ValueOf(slice<@string>(["a", "b"])));
            break;
        }
        case {} when exprᴛ1 == "ServerName"u8: {
            f.Set(reflect.ValueOf("b"));
            break;
        }
        case {} when exprᴛ1 == "ClientAuth"u8: {
            f.Set(reflect.ValueOf(VerifyClientCertIfGiven));
            break;
        }
        case {} when exprᴛ1 == "InsecureSkipVerify"u8 || exprᴛ1 == "SessionTicketsDisabled"u8 || exprᴛ1 == "DynamicRecordSizingDisabled"u8 || exprᴛ1 == "PreferServerCipherSuites"u8: {
            f.Set(reflect.ValueOf(true));
            break;
        }
        case {} when exprᴛ1 == "MinVersion"u8 || exprᴛ1 == "MaxVersion"u8: {
            f.Set(reflect.ValueOf((uint16)VersionTLS12));
            break;
        }
        case {} when exprᴛ1 == "SessionTicketKey"u8: {
            f.Set(reflect.ValueOf(new byte[]{}.array(32)));
            break;
        }
        case {} when exprᴛ1 == "CipherSuites"u8: {
            f.Set(reflect.ValueOf(slice<uint16>([1, 2])));
            break;
        }
        case {} when exprᴛ1 == "CurvePreferences"u8: {
            f.Set(reflect.ValueOf(slice<global::go.crypto.tls_package.CurveID>([CurveP256])));
            break;
        }
        case {} when exprᴛ1 == "Renegotiation"u8: {
            f.Set(reflect.ValueOf(RenegotiateOnceAsClient));
            break;
        }
        case {} when exprᴛ1 == "EncryptedClientHelloConfigList"u8: {
            f.Set(reflect.ValueOf(slice<byte>([(byte)'x'])));
            break;
        }
        case {} when exprᴛ1 == "EncryptedClientHelloKeys"u8: {
            f.Set(reflect.ValueOf(slice<global::go.crypto.tls_package.EncryptedClientHelloKey>([
                new(Config: [1], PrivateKey: [1])
            ])));
            break;
        }
        case {} when exprᴛ1 == "mutex"u8 || exprᴛ1 == "autoSessionTicketKeys"u8 || exprᴛ1 == "sessionTicketKeys"u8: {
            continue; // these are unexported fields that are handled separately
            break;
        }
        default: {
            t.Errorf("all fields must be accounted for, but saw unknown field %q", fn);
            break;
        }}
    }
    // Set the unexported fields related to session ticket keys, which are copied with Clone().
    c1.autoSessionTicketKeys = slice<global::go.crypto.tls_package.ticketKey>([c1.ticketKeyFromBytes(c1.SessionTicketKey)]);
    c1.sessionTicketKeys = slice<global::go.crypto.tls_package.ticketKey>([c1.ticketKeyFromBytes(c1.SessionTicketKey)]);

    var c2 = Ꮡc1.Clone();
    if (!reflect.DeepEqual(Ꮡc1, c2.OrTypedNil())) {
        t.Errorf("clone failed to copy a field");
    }
}

public static void TestCloneNilConfig(ж<testing.T> t) {
    ж<global::go.crypto.tls_package.Config> config = default;
    var cc = config.Clone(); if (cc != nil) {
        t.Fatalf("Clone with nil should return nil, got: %+v", cc.OrTypedNil());
    }
}

// changeImplConn is a net.Conn which can change its Write and Close
// methods.
internal partial struct changeImplConn {
    /*embed*/ public net_package.Conn Conn;
    internal Func<slice<byte>, (nint, error)> writeFunc;
    internal Func<error> closeFunc;
}

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *changeImplConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this changeImplConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *changeImplConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this changeImplConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *changeImplConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this changeImplConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *changeImplConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this changeImplConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *changeImplConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this changeImplConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *changeImplConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this changeImplConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint n, error err) Write(this ref changeImplConn w, slice<byte> p) {
    if (w.writeFunc != nil) {
        return w.writeFunc(p);
    }
    return w.Conn.Write(p);
}

internal static error Close(this ref changeImplConn w) {
    if (w.closeFunc != nil) {
        return w.closeFunc();
    }
    return w.Conn.Close();
}

internal static partial void throughput(ж<testing.B> Ꮡb, uint16 version, int64 totalBytes, bool dynamicRecordSizingDisabled) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var ln = newLocalListener(new tls_test_package.testing_BжTB(Ꮡb));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        nint N = b.N;

        // Less than 64KB because Windows appears to use a TCP rwin < 64KB.
        // See Issue #15899.
        const nint bufsize = /* 32 << 10 */ 32768;

        var lnʗ2 = ln;
        goǃ(() => {
            var bufΔ1 = new slice<byte>(bufsize);
            for (nint i = 0; i < N; i++) {
                var (sconn, err) = lnʗ2.Accept();
                if (err != nil) {
                    // panic rather than synchronize to avoid benchmark overhead
                    // (cannot call b.Fatal in goroutine)
                    throw panic(fmt.Errorf("accept: %v"u8, err));
                }
                var serverConfig = testConfig.Clone();
                serverConfig.CipherSuites = default; // the defaults may prefer faster ciphers
                serverConfig.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
                var srv = Server(sconn, serverConfig);
                var errΔ1 = srv.Handshake(); if (errΔ1 != nil) {
                    throw panic(fmt.Errorf("handshake: %v"u8, errΔ1));
                }
                var (_, errΔ2) = io.CopyBuffer(new tls_test_package.tls_ConnжWriter(srv), new tls_test_package.tls_ConnжReader(srv), bufΔ1); if (errΔ2 != nil) {
                    throw panic(fmt.Errorf("copy buffer: %v"u8, errΔ2));
                }
            }
        });

        b.SetBytes(totalBytes);
        var clientConfig = testConfig.Clone();
        clientConfig.CipherSuites = default; // the defaults may prefer faster ciphers
        clientConfig.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
        clientConfig.MaxVersion = version;

        var buf = new slice<byte>(bufsize);
        nint chunks = (nint)math.Ceil((float64)totalBytes / (float64)len(buf));
        for (nint i = 0; i < N; i++) {
            var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
            if (err != nil) {
                Ꮡb.Fatal(err);
            }
            for (nint j = 0; j < chunks; j++) {
                var (_, errΔ1) = conn.Write(buf);
                if (errΔ1 != nil) {
                    Ꮡb.Fatal(errΔ1);
                }
                (_, errΔ1) = io.ReadFull(new tls_test_package.tls_ConnжReader(conn), buf);
                if (errΔ1 != nil) {
                    Ꮡb.Fatal(errΔ1);
                }
            }
            conn.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkThroughput(ж<testing.B> b) {
    foreach (var (_, mode) in range(slice<@string>(["Max", "Dynamic"]))) {
        for (nint sizeᴛ1 = 1; sizeᴛ1 <= 64; sizeᴛ1 <<= 1) {
            var size = sizeᴛ1;
            @string name = fmt.Sprintf("%sPacket/%dMB"u8, mode, size);
            b.Run(name, (ж<testing.B> bΔ1) => {
                bΔ1.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ2) => {
                    throughput(bΔ2, VersionTLS12, (int64)(size << 20), mode == "Max"u8);
                });
                bΔ1.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ3) => {
                    throughput(bΔ3, VersionTLS13, (int64)(size << 20), mode == "Max"u8);
                });
            });
        }
    }
}

internal partial struct slowConn {
    /*embed*/ public net_package.Conn Conn;
    internal nint bps;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static error Close(this slowConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this slowConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this slowConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this slowConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this slowConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this slowConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *slowConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this slowConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint, error) Write(this ref slowConn c, slice<byte> p) {
    if (c.bps == 0) {
        throw panic("too slow");
    }
    var t0 = time_package.Now();
    nint wrote = 0;
    while (wrote < len(p)) {
        time_package.Sleep(100 * time_package.Microsecond);
        nint allowed = (nint)(time_package.Since(t0).Seconds() * (float64)c.bps) / 8;
        if (allowed > len(p)) {
            allowed = len(p);
        }
        if (wrote < allowed) {
            var (n, err) = c.Conn.Write(p.slice(wrote, allowed));
            wrote += n;
            if (err != nil) {
                return (wrote, err);
            }
        }
    }
    return (len(p), default);
}

internal static partial void latency(ж<testing.B> Ꮡb, uint16 version, nint bps, bool dynamicRecordSizingDisabled) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var ln = newLocalListener(new tls_test_package.testing_BжTB(Ꮡb));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        nint N = b.N;

        var lnʗ2 = ln;
        goǃ(() => {
            for (nint i = 0; i < N; i++) {
                var (sconn, err) = lnʗ2.Accept();
                if (err != nil) {
                    // panic rather than synchronize to avoid benchmark overhead
                    // (cannot call b.Fatal in goroutine)
                    throw panic(fmt.Errorf("accept: %v"u8, err));
                }
                var serverConfig = testConfig.Clone();
                serverConfig.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
                var srv = Server(new tls_internal_test_package.slowConnжConn(Ꮡ(new slowConn(sconn, bps))), serverConfig);
                var errΔ1 = srv.Handshake(); if (errΔ1 != nil) {
                    throw panic(fmt.Errorf("handshake: %v"u8, errΔ1));
                }
                io.Copy(new tls_test_package.tls_ConnжWriter(srv), new tls_test_package.tls_ConnжReader(srv));
            }
        });

        var clientConfig = testConfig.Clone();
        clientConfig.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
        clientConfig.MaxVersion = version;

        var buf = new slice<byte>(16384);
        var peek = new slice<byte>(1);

        for (nint i = 0; i < N; i++) {
            var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
            if (err != nil) {
                Ꮡb.Fatal(err);
            }
            // make sure we're connected and previous connection has stopped
            var (_, errΔ1) = conn.Write(buf[..1]); if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
            var (_, errΔ2) = io.ReadFull(new tls_test_package.tls_ConnжReader(conn), peek); if (errΔ2 != nil) {
                Ꮡb.Fatal(errΔ2);
            }
            var (_, errΔ3) = conn.Write(buf); if (errΔ3 != nil) {
                Ꮡb.Fatal(errΔ3);
            }
            (_, err) = io.ReadFull(new tls_test_package.tls_ConnжReader(conn), peek); if (err != nil) {
                Ꮡb.Fatal(err);
            }
            conn.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkLatency(ж<testing.B> b) {
    foreach (var (_, mode) in range(slice<@string>(["Max", "Dynamic"]))) {
        foreach (var (_, kbps) in range(slice<nint>([200, 500, 1000, 2000, 5000]))) {
            @string name = fmt.Sprintf("%sPacket/%dkbps"u8, mode, kbps);
            b.Run(name, (ж<testing.B> bΔ1) => {
                bΔ1.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ2) => {
                    latency(bΔ2, VersionTLS12, kbps * 1000, mode == "Max"u8);
                });
                bΔ1.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ3) => {
                    latency(bΔ3, VersionTLS13, kbps * 1000, mode == "Max"u8);
                });
            });
        }
    }
}

public static void TestConnectionStateMarshal(ж<testing.T> t) {
    var cs = Ꮡ(new ΔConnectionState());
    var (_, err) = json.Marshal(cs.OrTypedNil());
    if (err != nil) {
        t.Errorf("json.Marshal failed on ConnectionState: %v", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string clientˢ2 => ref clientˢ2ᵇ;
private static readonly @string clientˢ2ᵇ = "Client"u8;
internal static ref readonly @string serverˢ2 => ref serverˢ2ᵇ;
private static readonly @string serverˢ2ᵇ = "Server"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string alpnProtocolᶜ => ref alpnProtocolᶜᵇ;
private static readonly @string alpnProtocolᶜᵇ = "golang"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string serverNameᶜ => ref serverNameᶜᵇ;
private static readonly @string serverNameᶜᵇ = "example.golang"u8;

public static void TestConnectionState(ж<testing.T> t) {
    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != nil) {
        throw panic(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);

    @string alpnProtocol = alpnProtocolᶜ;
    @string serverName = serverNameᶜ;
    slice<slice<byte>> scts = slice<slice<byte>>([slice<byte>("dummy sct 1"u8), slice<byte>("dummy sct 2"u8)]);
    slice<byte> ocsp = slice<byte>("dummy ocsp"u8);

    var ocspʗ1 = ocsp;
    var sctsʗ1 = scts;
    void checkConnectionState(ж<testing.T> tΔ1, global::go.crypto.tls_package.ΔConnectionState cs, uint16 version, bool isClient) {
        if (cs.Version != version) {
            tΔ1.Errorf("got Version %x, expected %x", cs.Version, version);
        }

        if (!cs.HandshakeComplete) {
            tΔ1.Errorf("got HandshakeComplete %v, expected true", cs.HandshakeComplete);
        }

        if (cs.DidResume) {
            tΔ1.Errorf("got DidResume %v, expected false", cs.DidResume);
        }

        if (cs.CipherSuite == 0) {
            tΔ1.Errorf("got zero CipherSuite");
        }

        if (cs.CurveID == 0) {
            tΔ1.Errorf("got zero CurveID");
        }

        if (cs.NegotiatedProtocol != alpnProtocol) {
            tΔ1.Errorf("got ALPN protocol %q, expected %q", cs.NegotiatedProtocol, alpnProtocol);
        }

        if (!cs.NegotiatedProtocolIsMutual) {
            tΔ1.Errorf("got NegotiatedProtocolIsMutual %v, expected true", cs.NegotiatedProtocolIsMutual);
        }

        if (cs.ServerName != serverName) {
            tΔ1.Errorf("got ServerName %q, expected %q", cs.ServerName, serverName);
        }

        if (len(cs.PeerCertificates) != 1) {
            tΔ1.Errorf("got %d PeerCertificates, expected %d", len(cs.PeerCertificates), (nint)1);
        } else if (!bytes.Equal(cs.PeerCertificates[0].Raw, testRSA2048Certificate)) {
            tΔ1.Errorf("got PeerCertificates %x, expected %x", cs.PeerCertificates[0].Raw, testRSA2048Certificate);
        }

        if (len(cs.VerifiedChains) != 1) {
            tΔ1.Errorf("got %d long verified chain, expected %d", len(cs.VerifiedChains), (nint)1);
        } else if (len(cs.VerifiedChains[0]) != 2) {
            tΔ1.Errorf("got %d verified chain, expected %d", len(cs.VerifiedChains[0]), (nint)2);
        } else if (!bytes.Equal(cs.VerifiedChains[0][0].Raw, testRSA2048Certificate)) {
            tΔ1.Errorf("got verified chain[0][0] %x, expected %x", cs.VerifiedChains[0][0].Raw, testRSA2048Certificate);
        } else if (!bytes.Equal(cs.VerifiedChains[0][1].Raw, testRSA2048CertificateIssuer)) {
            tΔ1.Errorf("got verified chain[0][1] %x, expected %x", cs.VerifiedChains[0][1].Raw, testRSA2048CertificateIssuer);
        }

        // Only TLS 1.3 supports OCSP and SCTs on client certs.
        if (isClient || version == VersionTLS13) {
            if (len(cs.SignedCertificateTimestamps) != 2) {
                tΔ1.Errorf("got %d SCTs, expected %d", len(cs.SignedCertificateTimestamps), (nint)2);
            } else if (!bytes.Equal(cs.SignedCertificateTimestamps[0], sctsʗ1[0])) {
                tΔ1.Errorf("got SCTs %x, expected %x", cs.SignedCertificateTimestamps[0], sctsʗ1[0]);
            } else if (!bytes.Equal(cs.SignedCertificateTimestamps[1], sctsʗ1[1])) {
                tΔ1.Errorf("got SCTs %x, expected %x", cs.SignedCertificateTimestamps[1], sctsʗ1[1]);
            }
            if (!bytes.Equal(cs.OCSPResponse, ocspʗ1)) {
                tΔ1.Errorf("got OCSP %x, expected %x", cs.OCSPResponse, ocspʗ1);
            }
        } else {
            if (cs.SignedCertificateTimestamps != nil) {
                tΔ1.Errorf("got %d SCTs, expected nil", len(cs.SignedCertificateTimestamps));
            }
            if (cs.OCSPResponse != nil) {
                tΔ1.Errorf("got OCSP %x, expected nil", cs.OCSPResponse);
            }
        }

        if (version == VersionTLS13) {
            if (cs.TLSUnique != nil) {
                tΔ1.Errorf("got TLSUnique %x, expected nil", cs.TLSUnique);
            }
        } else {
            if (cs.TLSUnique == nil) {
                tΔ1.Errorf("got nil TLSUnique");
            }
        }
    }

    void compareConnectionStates(ж<testing.T> tΔ2, global::go.crypto.tls_package.ΔConnectionState cs1, global::go.crypto.tls_package.ΔConnectionState cs2) {
        if (cs1.Version != cs2.Version) {
            tΔ2.Errorf("Version mismatch: %x != %x", cs1.Version, cs2.Version);
        }
        if (cs1.HandshakeComplete != cs2.HandshakeComplete) {
            tΔ2.Errorf("HandshakeComplete mismatch: %v != %v", cs1.HandshakeComplete, cs2.HandshakeComplete);
        }
        // DidResume is expected to be different.
        if (cs1.CipherSuite != cs2.CipherSuite) {
            tΔ2.Errorf("CipherSuite mismatch: %x != %x", cs1.CipherSuite, cs2.CipherSuite);
        }
        if (cs1.CurveID != cs2.CurveID) {
            tΔ2.Errorf("CurveID mismatch: %s != %s", cs1.CurveID, cs2.CurveID);
        }
        if (cs1.NegotiatedProtocol != cs2.NegotiatedProtocol) {
            tΔ2.Errorf("NegotiatedProtocol mismatch: %q != %q", cs1.NegotiatedProtocol, cs2.NegotiatedProtocol);
        }
        if (cs1.NegotiatedProtocolIsMutual != cs2.NegotiatedProtocolIsMutual) {
            tΔ2.Errorf("NegotiatedProtocolIsMutual mismatch: %v != %v", cs1.NegotiatedProtocolIsMutual, cs2.NegotiatedProtocolIsMutual);
        }
        if (cs1.ServerName != cs2.ServerName) {
            tΔ2.Errorf("ServerName mismatch: %q != %q", cs1.ServerName, cs2.ServerName);
        }
        if (!reflect.DeepEqual(cs1.PeerCertificates, cs2.PeerCertificates)) {
            tΔ2.Errorf("PeerCertificates mismatch");
        }
        if (!reflect.DeepEqual(cs1.VerifiedChains, cs2.VerifiedChains)) {
            tΔ2.Errorf("VerifiedChains mismatch");
        }
        if (!reflect.DeepEqual(cs1.SignedCertificateTimestamps, cs2.SignedCertificateTimestamps)) {
            tΔ2.Errorf("SignedCertificateTimestamps mismatch: %x != %x", cs1.SignedCertificateTimestamps, cs2.SignedCertificateTimestamps);
        }
        if (!bytes.Equal(cs1.OCSPResponse, cs2.OCSPResponse)) {
            tΔ2.Errorf("OCSPResponse mismatch: %x != %x", cs1.OCSPResponse, cs2.OCSPResponse);
        }
        // TLSUnique is expected to be different.
    }

    foreach (var (_, vᴛ1) in range(slice<uint16>([VersionTLS10, VersionTLS12, VersionTLS13]))) {
        ref var v = ref heap(new uint16(), out var Ꮡv);
        v = vᴛ1;

        if (!isFIPSVersion(v) && fips140tls.Required()) {
            t.Skipf("skipping test in FIPS 140-3 mode for non-FIPS version %x", v);
        }
        @string name = default;
        switch (v) {
        case var ᴛ when ᴛ == VersionTLS10: {
            name = tlSv10ˢ;
            break;
        }
        case var ᴛ when ᴛ == VersionTLS12: {
            name = tlSv12ˢ;
            break;
        }
        case var ᴛ when ᴛ == VersionTLS13: {
            name = tlSv13ˢ;
            break;
        }}
        var checkConnectionStateʗ1 = checkConnectionState;
        var compareConnectionStatesʗ1 = compareConnectionStates;
        var ocspʗ2 = ocsp;
        var rootCAsʗ1 = rootCAs;
        var sctsʗ2 = scts;
        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ3) => {
            var config = Ꮡ(new Config(
                Time: testTime,
                Certificates: new slice<global::go.crypto.tls_package.Certificate>(1),
                MinVersion: vʗ1,
                MaxVersion: vʗ1,
                RootCAs: rootCAsʗ1,
                ClientCAs: rootCAsʗ1,
                ClientAuth: RequireAndVerifyClientCert,
                NextProtos: [alpnProtocol],
                ServerName: serverName,
                ClientSessionCache: NewLRUClientSessionCache(1)
            ));
            config.Certificates[0].ΔCertificate = [testRSA2048Certificate];
            config.Certificates[0].PrivateKey = testRSA2048PrivateKey.OrTypedNil();
            config.Certificates[0].SignedCertificateTimestamps = sctsʗ2;
            config.Certificates[0].OCSPStaple = ocspʗ2;

            ref var ss = ref heap<global::go.crypto.tls_package.ΔConnectionState>(out var Ꮡss);
            ref var cs = ref heap<global::go.crypto.tls_package.ΔConnectionState>(out var Ꮡcs);
            (ss, cs, var errΔ1) = testHandshake(tΔ3, config, config);
            if (errΔ1 != nil) {
                tΔ3.Fatalf("handshake failed: %v", errΔ1);
            }

            var checkConnectionStateʗ2 = checkConnectionStateʗ1;
            var csʗ1 = cs;
            var vʗ2 = vʗ1;
            tΔ3.Run(clientˢ2, (ж<testing.T> tΔ4) => {
                checkConnectionStateʗ2(tΔ4, csʗ1, vʗ2, true);
            });
            var checkConnectionStateʗ3 = checkConnectionStateʗ1;
            var ssʗ1 = ss;
            var vʗ3 = vʗ1;
            tΔ3.Run(serverˢ2, (ж<testing.T> tΔ5) => {
                checkConnectionStateʗ3(tΔ5, ssʗ1, vʗ3, false);
            });

            var compareConnectionStatesʗ2 = compareConnectionStatesʗ1;
            var configʗ1 = config;
            var csʗ2 = cs;
            var ssʗ2 = ss;
            tΔ3.Run(resumeˢ, (ж<testing.T> tΔ6) => {
                // TODO: test changing parameters between original and resumed
                // connection when the protocol allows it.

                ref var ss1 = ref heap<global::go.crypto.tls_package.ΔConnectionState>(out var Ꮡss1);
                ref var cs1 = ref heap<global::go.crypto.tls_package.ΔConnectionState>(out var Ꮡcs1);
                (ss1, cs1, var errΔ2) = testHandshake(tΔ6, configʗ1, configʗ1);
                if (errΔ2 != nil) {
                    tΔ6.Fatalf("handshake failed: %v", errΔ2);
                }

                if (!cs1.DidResume || !ss1.DidResume) {
                    tΔ6.Errorf("DidResume is false");
                }

                var compareConnectionStatesʗ3 = compareConnectionStatesʗ2;
                var csʗ3 = csʗ2;
                var cs1ʗ1 = cs1;
                tΔ6.Run(clientˢ2, (ж<testing.T> tΔ7) => {
                    compareConnectionStatesʗ3(tΔ7, csʗ3, cs1ʗ1);
                });
                var compareConnectionStatesʗ4 = compareConnectionStatesʗ2;
                var ssʗ3 = ssʗ2;
                var ss1ʗ1 = ss1;
                tΔ6.Run(serverˢ2, (ж<testing.T> tΔ8) => {
                    compareConnectionStatesʗ4(tΔ8, ssʗ3, ss1ʗ1);
                });
            });
        });
    }
}

// Issue 28744: Ensure that we don't modify memory
// that Config doesn't own such as Certificates.
public static void TestBuildNameToCertificate_doesntModifyCertificates(ж<testing.T> t) {
    var c0 = new Certificate(
        ΔCertificate: [testRSACertificate],
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    );
    var c1 = new Certificate(
        ΔCertificate: [testSNICertificate],
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    );
    var config = testConfig.Clone();
    config.Certificates = [c0, c1];

    config.BuildNameToCertificate();
    var got = config.Certificates;
    var want = slice<global::go.crypto.tls_package.Certificate>([c0, c1]);
    if (!reflect.DeepEqual(got, want)) {
        t.Fatalf("Certificates were mutated by BuildNameToCertificate\nGot: %#v\nWant: %#v\n", got, want);
    }
}

internal static @string testingKey(@string s) { return strings.ReplaceAll(s, "TESTING KEY", "PRIVATE KEY"); }

internal partial struct TestClientHelloInfo_SupportsCertificate_tests /*dyn*/ {
    internal ж<global::go.crypto.tls_package.Certificate> c;
    internal ж<global::go.crypto.tls_package.ClientHelloInfo> chi;
    internal @string wantErr;
}

public static void TestClientHelloInfo_SupportsCertificate(ж<testing.T> t) {
    skipFIPS(t); // Test certificates not FIPS compatible.

    var rsaCert = Ꮡ(new Certificate(
        ΔCertificate: [testRSACertificate],
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    ));
    var pkcs1Cert = Ꮡ(new Certificate(
        ΔCertificate: [testRSACertificate],
        PrivateKey: testRSAPrivateKey.OrTypedNil(),
        SupportedSignatureAlgorithms: [PKCS1WithSHA1, PKCS1WithSHA256]
    ));
    var ecdsaCert = Ꮡ(new Certificate(
        // ECDSA P-256 certificate
        ΔCertificate: [testP256Certificate],
        PrivateKey: testP256PrivateKey.OrTypedNil()
    ));
    var ed25519Cert = Ꮡ(new Certificate(
        ΔCertificate: [testEd25519Certificate],
        PrivateKey: testEd25519PrivateKey
    ));

    var tests = slice<TestClientHelloInfo_SupportsCertificate_tests>([
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            ServerName: "example.golang",
            SignatureSchemes: [PSSWithSHA256],
            SupportedVersions: [VersionTLS13]
        )), ""),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            SignatureSchemes: [PSSWithSHA256, ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS13, VersionTLS12]
        )), ""),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            ServerName: "example.com",
            SignatureSchemes: [PSSWithSHA256],
            SupportedVersions: [VersionTLS13]
        )), "not valid for requested server name"),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            SignatureSchemes: [ECDSAWithP384AndSHA384],
            SupportedVersions: [VersionTLS13]
        )), "signature algorithms"),
        new(pkcs1Cert, Ꮡ(new ClientHelloInfo(
            SignatureSchemes: [PSSWithSHA256, ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS13]
        )), "signature algorithms"),

        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256],
            SignatureSchemes: [PKCS1WithSHA1],
            SupportedVersions: [VersionTLS13, VersionTLS12]
        )), "signature algorithms"),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256],
            SignatureSchemes: [PKCS1WithSHA1],
            SupportedVersions: [VersionTLS13, VersionTLS12],
            config: Ꮡ(new Config(
                CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256],
                MaxVersion: VersionTLS12
            ))
        )), ""), // Check that mutual version selection works.

        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS12]
        )), ""),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [ECDSAWithP384AndSHA384],
            SupportedVersions: [VersionTLS12]
        )), ""), // TLS 1.2 does not restrict curves based on the SignatureScheme.
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: default,
            SupportedVersions: [VersionTLS12]
        )), ""), // TLS 1.2 comes with default signature schemes.
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS12]
        )), "cipher suite"),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS12],
            config: Ꮡ(new Config(
                CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256]
            ))
        )), "cipher suite"),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP384],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS12]
        )), "certificate curve"),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [1],
            SignatureSchemes: [ECDSAWithP256AndSHA256],
            SupportedVersions: [VersionTLS12]
        )), "only incompatible point formats"),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [PSSWithSHA256],
            SupportedVersions: [VersionTLS12]
        )), "signature algorithms"),

        new(ed25519Cert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256], // only relevant for ECDHE support
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [Ed25519],
            SupportedVersions: [VersionTLS12]
        )), ""),
        new(ed25519Cert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [CurveP256], // only relevant for ECDHE support
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [Ed25519],
            SupportedVersions: [VersionTLS10],
            config: Ꮡ(new Config(MinVersion: VersionTLS10))
        )), "doesn't support Ed25519"),
        new(ed25519Cert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256],
            SupportedCurves: [],
            SupportedPoints: [pointFormatUncompressed],
            SignatureSchemes: [Ed25519],
            SupportedVersions: [VersionTLS12]
        )), "doesn't support ECDHE"),

        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA],
            SupportedCurves: [CurveP256], // only relevant for ECDHE support
            SupportedPoints: [pointFormatUncompressed],
            SupportedVersions: [VersionTLS10],
            config: Ꮡ(new Config(MinVersion: VersionTLS10))
        )), ""),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256],
            SupportedVersions: [VersionTLS12],
            config: Ꮡ(new Config(
                CipherSuites: [TLS_RSA_WITH_AES_128_GCM_SHA256]
            ))
        )), "") // static RSA fallback
    ]);
    foreach (var (i, tt) in range(tests)) {
        var err = tt.chi.SupportsCertificate(tt.c);
        switch (ᐧ) {
        case {} when tt.wantErr == ""u8 && err != nil: {
            t.Errorf("%d: unexpected error: %v", i, err);
            break;
        }
        case {} when tt.wantErr != ""u8 && err == nil: {
            t.Errorf("%d: unexpected success", i);
            break;
        }
        case {} when tt.wantErr != ""u8 && !strings.Contains(err.Error(), tt.wantErr): {
            t.Errorf("%d: got error %q, expected %q", i, err, tt.wantErr);
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rc4ˢ => ref rc4ˢᵇ;
private static readonly @string rc4ˢᵇ = "RC4"u8;
internal static ref readonly @string cbcSha256ˢ => ref cbcSha256ˢᵇ;
private static readonly @string cbcSha256ˢᵇ = "CBC_SHA256"u8;
internal static ref readonly @string aesˢ => ref aesˢᵇ;
private static readonly @string aesˢᵇ = "AES"u8;
internal static ref readonly @string aes128ˢ => ref aes128ˢᵇ;
private static readonly @string aes128ˢᵇ = "AES_128"u8;
internal static ref readonly @string aes256ˢ => ref aes256ˢᵇ;
private static readonly @string aes256ˢᵇ = "AES_256"u8;
internal static readonly object preferenceOrderIsNotˢ = (@string)"preference order is not sorted according to the rules"u8;

public static void TestCipherSuites(ж<testing.T> t) {
    uint16 lastID = default;
    foreach (var (_, c) in range(CipherSuites())) {
        if (lastID > c.ID) {
            t.Errorf("CipherSuites are not ordered by ID: got %#04x after %#04x", c.ID, lastID);
        } else {
            lastID = c.ID;
        }

        if (c.Insecure) {
            t.Errorf("%#04x: Insecure CipherSuite returned by CipherSuites()", c.ID);
        }
    }
    lastID = 0;
    foreach (var (_, c) in range(InsecureCipherSuites())) {
        if (lastID > c.ID) {
            t.Errorf("InsecureCipherSuites are not ordered by ID: got %#04x after %#04x", c.ID, lastID);
        } else {
            lastID = c.ID;
        }

        if (!c.Insecure) {
            t.Errorf("%#04x: not Insecure CipherSuite returned by InsecureCipherSuites()", c.ID);
        }
    }

    ж<global::go.crypto.tls_package.CipherSuite> CipherSuiteByID(uint16 id) {
        foreach (var (_, c) in range(CipherSuites())) {
            if (c.ID == id) {
                return c;
            }
        }
        foreach (var (_, c) in range(InsecureCipherSuites())) {
            if (c.ID == id) {
                return c;
            }
        }
        return default;
    }

    foreach (var (_, c) in range(ΔcipherSuites)) {
        var cc = CipherSuiteByID(c.id);
        if (cc == nil) {
            t.Errorf("%#04x: no CipherSuite entry", c.id);
            continue;
        }

        var tls12Only = (c.flags & (nint)suiteTLS12) != 0; if (tls12Only && len(cc.SupportedVersions) != 1) {
            t.Errorf("%#04x: suite is TLS 1.2 only, but SupportedVersions is %v", c.id, cc.SupportedVersions);
        } else if (!tls12Only && len(cc.SupportedVersions) != 3) {
            t.Errorf("%#04x: suite TLS 1.0-1.2, but SupportedVersions is %v", c.id, cc.SupportedVersions);
        }

        if (cc.Insecure) {
            if (slices.Contains(defaultCipherSuites(false), c.id)) {
                t.Errorf("%#04x: insecure suite in default list", c.id);
            }
        } else {
            if (!slices.Contains(defaultCipherSuites(false), c.id)) {
                t.Errorf("%#04x: secure suite not in default list", c.id);
            }
        }

        @string got = CipherSuiteName(c.id); if (got != cc.Name) {
            t.Errorf("%#04x: unexpected CipherSuiteName: got %q, expected %q", c.id, got, cc.Name);
        }
    }
    foreach (var (_, c) in range(cipherSuitesTLS13)) {
        var cc = CipherSuiteByID(c.id);
        if (cc == nil) {
            t.Errorf("%#04x: no CipherSuite entry", c.id);
            continue;
        }

        if (cc.Insecure) {
            t.Errorf("%#04x: Insecure %v, expected false", c.id, cc.Insecure);
        }
        if (len(cc.SupportedVersions) != 1 || cc.SupportedVersions[0] != VersionTLS13) {
            t.Errorf("%#04x: suite is TLS 1.3 only, but SupportedVersions is %v", c.id, cc.SupportedVersions);
        }

        @string got = CipherSuiteName(c.id); if (got != cc.Name) {
            t.Errorf("%#04x: unexpected CipherSuiteName: got %q, expected %q", c.id, got, cc.Name);
        }
    }

    {
        @string got = CipherSuiteName(0xabc); if (got != "0x0ABC"u8) {
            t.Errorf("unexpected fallback CipherSuiteName: got %q, expected 0x0ABC", got);
        }
    }

    if (len(cipherSuitesPreferenceOrder) != len(ΔcipherSuites)) {
        t.Errorf("cipherSuitesPreferenceOrder is not the same size as cipherSuites");
    }
    if (len(cipherSuitesPreferenceOrderNoAES) != len(cipherSuitesPreferenceOrder)) {
        t.Errorf("cipherSuitesPreferenceOrderNoAES is not the same size as cipherSuitesPreferenceOrder");
    }

    // Check that disabled suites are marked insecure.
    foreach (var (_, badSuites) in range(slice<map<uint16, bool>>([disabledCipherSuites, rsaKexCiphers]))) {
        foreach (var (id, _) in badSuites) {
            var c = CipherSuiteByID(id);
            if (c == nil) {
                t.Errorf("%#04x: no CipherSuite entry", id);
                continue;
            }
            if (!c.Insecure) {
                t.Errorf("%#04x: disabled by default but not marked insecure", id);
            }
        }
    }

    foreach (var (i, prefOrder) in range(slice<slice<uint16>>([cipherSuitesPreferenceOrder, cipherSuitesPreferenceOrderNoAES]))) {
        // Check that insecure and HTTP/2 bad cipher suites are at the end of
        // the preference lists.
        bool sawInsecure = default;
        bool sawBad = default;
        foreach (var (_, id) in range(prefOrder)) {
            var c = CipherSuiteByID(id);
            if (c == nil) {
                t.Errorf("%#04x: no CipherSuite entry", id);
                continue;
            }

            if (c.Insecure) {
                sawInsecure = true;
            } else if (sawInsecure) {
                t.Errorf("%#04x: secure suite after insecure one(s)", id);
            }

            if (http2isBadCipher(id)) {
                sawBad = true;
            } else if (sawBad) {
                t.Errorf("%#04x: non-bad suite after bad HTTP/2 one(s)", id);
            }
        }

        // Check that the list is sorted according to the documented criteria.
        var isBetter = nint (uint16 a, uint16 b) => {
            var (aSuite, bSuite) = (cipherSuiteByID(a), cipherSuiteByID(b));
            @string aName = CipherSuiteName(a);
            @string bName = CipherSuiteName(b);
            // * < RC4
            if (!strings.Contains(aName, rc4ˢ) && strings.Contains(bName, rc4ˢ)) {
                return -1;
            } else if (strings.Contains(aName, rc4ˢ) && !strings.Contains(bName, rc4ˢ)) {
                return +1;
            }
            // * < CBC_SHA256
            if (!strings.Contains(aName, cbcSha256ˢ) && strings.Contains(bName, cbcSha256ˢ)) {
                return -1;
            } else if (strings.Contains(aName, cbcSha256ˢ) && !strings.Contains(bName, cbcSha256ˢ)) {
                return +1;
            }
            // * < 3DES
            if (!strings.Contains(aName, "3DES") && strings.Contains(bName, "3DES")) {
                return -1;
            } else if (strings.Contains(aName, "3DES") && !strings.Contains(bName, "3DES")) {
                return +1;
            }
            // ECDHE < *
            if ((aSuite.flags & (nint)suiteECDHE) != 0 && (bSuite.flags & (nint)suiteECDHE) == 0) {
                return -1;
            } else if ((aSuite.flags & (nint)suiteECDHE) == 0 && (bSuite.flags & (nint)suiteECDHE) != 0) {
                return +1;
            }
            // AEAD < CBC
            if (aSuite.aead != nil && bSuite.aead == nil) {
                return -1;
            } else if (aSuite.aead == nil && bSuite.aead != nil) {
                return +1;
            }
            // AES < ChaCha20
            if (strings.Contains(aName, aesˢ) && strings.Contains(bName, chacha20ˢ)) {
                // negative for cipherSuitesPreferenceOrder
                if (i == 0) {
                    return -1;
                } else {
                    return +1;
                }
            } else if (strings.Contains(aName, chacha20ˢ) && strings.Contains(bName, aesˢ)) {
                // negative for cipherSuitesPreferenceOrderNoAES
                if (i != 0) {
                    return -1;
                } else {
                    return +1;
                }
            }
            // AES-128 < AES-256
            if (strings.Contains(aName, aes128ˢ) && strings.Contains(bName, aes256ˢ)) {
                return -1;
            } else if (strings.Contains(aName, aes256ˢ) && strings.Contains(bName, aes128ˢ)) {
                return +1;
            }
            // ECDSA < RSA
            if ((aSuite.flags & (nint)suiteECSign) != 0 && (bSuite.flags & (nint)suiteECSign) == 0) {
                return -1;
            } else if ((aSuite.flags & (nint)suiteECSign) == 0 && (bSuite.flags & (nint)suiteECSign) != 0) {
                return +1;
            }
            t.Fatalf("two ciphersuites are equal by all criteria: %v and %v", aName, bName);
            throw panic("unreachable");
        };
        if (!slices.IsSortedFunc(prefOrder, isBetter)) {
            t.Error(preferenceOrderIsNotˢ);
        }
    }
}

public static void TestVersionName(ж<testing.T> t) {
    {
        @string got = VersionName(VersionTLS13);
        @string exp = tls13ˢ; if (got != exp) {
            t.Errorf("unexpected VersionName: got %q, expected %q", got, exp);
        }
    }
    {
        @string got = VersionName(0x12a);
        @string exp = "0x012A"; if (got != exp) {
            t.Errorf("unexpected fallback VersionName: got %q, expected %q", got, exp);
        }
    }
}

// http2isBadCipher is copied from net/http.
// TODO: if it ends up exposed somewhere, use that instead.
internal static bool http2isBadCipher(uint16 cipher) {
    switch (cipher) {
    case TLS_RSA_WITH_RC4_128_SHA or TLS_RSA_WITH_3DES_EDE_CBC_SHA or TLS_RSA_WITH_AES_128_CBC_SHA or TLS_RSA_WITH_AES_256_CBC_SHA or TLS_RSA_WITH_AES_128_CBC_SHA256 or TLS_RSA_WITH_AES_128_GCM_SHA256 or TLS_RSA_WITH_AES_256_GCM_SHA384 or TLS_ECDHE_ECDSA_WITH_RC4_128_SHA or TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA or TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA or TLS_ECDHE_RSA_WITH_RC4_128_SHA or TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA or TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA or TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA or TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 or TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256: {
        return true;
    }
    default: {
        return false;
    }}
}

internal partial struct brokenSigner {
    /*embed*/ public crypto_package.Signer Signer;
}

internal static (slice<byte> signature, error err) Sign(this brokenSigner s, io.Reader rand, slice<byte> digest, crypto.SignerOpts opts) {
    // Replace opts with opts.HashFunc(), so rsa.PSSOptions are discarded.
    return s.Signer.Sign(rand, digest, opts.HashFunc());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedBrokenˢ = (@string)"expected broken certificate to cause connection to fail"u8;

// TestPKCS1OnlyCert uses a client certificate with a broken crypto.Signer that
// always makes PKCS #1 v1.5 signatures, so can't be used with RSA-PSS.
public static void TestPKCS1OnlyCert(ж<testing.T> t) {
    var clientConfig = testConfig.Clone();
    clientConfig.Certificates = [new(
        ΔCertificate: [testRSACertificate],
        PrivateKey: new brokenSigner(new tls_test_package.rsa_PrivateKeyжSigner(testRSAPrivateKey))
    )
    ];
    var serverConfig = testConfig.Clone();
    serverConfig.MaxVersion = VersionTLS12; // TLS 1.3 doesn't support PKCS #1 v1.5
    serverConfig.ClientAuth = RequireAnyClientCert;

    // If RSA-PSS is selected, the handshake should fail.
    {
        var (_, _, err) = testHandshake(t, clientConfig, serverConfig); if (err == nil) {
            t.Fatal(expectedBrokenˢ);
        }
    }

    clientConfig.Certificates[0].SupportedSignatureAlgorithms = [PKCS1WithSHA1, PKCS1WithSHA256];

    // But if the certificate restricts supported algorithms, RSA-PSS should not
    // be selected, and the handshake should succeed.
    {
        var (_, _, err) = testHandshake(t, clientConfig, serverConfig); if (err != nil) {
            t.Error(err);
        }
    }
}

public static void TestVerifyCertificates(ж<testing.T> t) {
    skipFIPS(t); // Test certificates not FIPS compatible.

    // See https://go.dev/issue/31641.
    t.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testVerifyCertificates(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testVerifyCertificates(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object verifyConnectionDidNotˢ = (@string)"VerifyConnection did not get called on the server"u8;
internal static readonly object verifyConnectionDidNotˢ2 = (@string)"VerifyConnection did not get called on the client"u8;
internal static readonly object expectedResumptionˢ = (@string)"expected resumption"u8;
internal static readonly object verifyPeerCertificatesˢ = (@string)"VerifyPeerCertificates got called on the server on resumption"u8;
internal static readonly object verifyPeerCertificatesˢ2 = (@string)"VerifyPeerCertificates got called on the client on resumption"u8;
internal static readonly object verifyConnectionDidNotˢ3 = (@string)"VerifyConnection did not get called on the server on resumption"u8;
internal static readonly object verifyConnectionDidNotˢ4 = (@string)"VerifyConnection did not get called on the client on resumption"u8;

internal partial struct testVerifyCertificates_tests /*dyn*/ {
    internal @string name;
    public bool InsecureSkipVerify;
    public global::go.crypto.tls_package.ClientAuthType ClientAuth;
    public bool ClientCertificates;
}

internal static void testVerifyCertificates(ж<testing.T> t, uint16 version) {
    var tests = slice<testVerifyCertificates_tests>([
        new(
            name: "defaults"
        ),
        new(
            name: "InsecureSkipVerify",
            InsecureSkipVerify: true
        ),
        new(
            name: "RequestClientCert with no certs",
            ClientAuth: RequestClientCert
        ),
        new(
            name: "RequestClientCert with certs",
            ClientAuth: RequestClientCert,
            ClientCertificates: true
        ),
        new(
            name: "RequireAnyClientCert",
            ClientAuth: RequireAnyClientCert,
            ClientCertificates: true
        ),
        new(
            name: "VerifyClientCertIfGiven with no certs",
            ClientAuth: VerifyClientCertIfGiven
        ),
        new(
            name: "VerifyClientCertIfGiven with certs",
            ClientAuth: VerifyClientCertIfGiven,
            ClientCertificates: true
        ),
        new(
            name: "RequireAndVerifyClientCert",
            ClientAuth: RequireAndVerifyClientCert,
            ClientCertificates: true
        )
    ]);

    var (issuer, err) = Δx509.ParseCertificate(testRSACertificateIssuer);
    if (err != nil) {
        t.Fatal(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new testVerifyCertificates_tests(), out var Ꮡtest);
        test = vᴛ1;

        var rootCAsʗ1 = rootCAs;
        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();

            bool serverVerifyConnection = default;
            bool clientVerifyConnection = default;
            bool serverVerifyPeerCertificates = default;
            bool clientVerifyPeerCertificates = default;

            var clientConfig = testConfig.Clone();
            clientConfig.Time = testTime;
            clientConfig.MaxVersion = version;
            clientConfig.MinVersion = version;
            clientConfig.RootCAs = rootCAsʗ1;
            clientConfig.ServerName = exampleGolangˢ;
            clientConfig.ClientSessionCache = NewLRUClientSessionCache(1);
            var serverConfig = clientConfig.Clone();
            serverConfig.ClientCAs = rootCAsʗ1;

            clientConfig.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState csΔ1) => {
                clientVerifyConnection = true;
                return default;
            };
            clientConfig.VerifyPeerCertificate = error (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> verifiedChains) => {
                clientVerifyPeerCertificates = true;
                return default;
            };
            serverConfig.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState csΔ2) => {
                serverVerifyConnection = true;
                return default;
            };
            serverConfig.VerifyPeerCertificate = error (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> verifiedChains) => {
                serverVerifyPeerCertificates = true;
                return default;
            };

            clientConfig.InsecureSkipVerify = testʗ1.InsecureSkipVerify;
            serverConfig.ClientAuth = testʗ1.ClientAuth;
            if (!testʗ1.ClientCertificates) {
                clientConfig.Certificates = default;
            }

            var (_, _, errΔ1) = testHandshake(tΔ1, clientConfig, serverConfig); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }

            var want = serverConfig.ClientAuth != NoClientCert;
            if (serverVerifyPeerCertificates != want) {
                tΔ1.Errorf("VerifyPeerCertificates on the server: got %v, want %v",
                    serverVerifyPeerCertificates, want);
            }
            if (!clientVerifyPeerCertificates) {
                tΔ1.Errorf("VerifyPeerCertificates not called on the client");
            }
            if (!serverVerifyConnection) {
                tΔ1.Error(verifyConnectionDidNotˢ);
            }
            if (!clientVerifyConnection) {
                tΔ1.Error(verifyConnectionDidNotˢ2);
            }

            (serverVerifyPeerCertificates, clientVerifyPeerCertificates) = (false, false);
            (serverVerifyConnection, clientVerifyConnection) = (false, false);
            var (cs, _, errΔ2) = testHandshake(tΔ1, clientConfig, serverConfig);
            if (errΔ2 != nil) {
                tΔ1.Fatal(errΔ2);
            }
            if (!cs.DidResume) {
                tΔ1.Error(expectedResumptionˢ);
            }

            if (serverVerifyPeerCertificates) {
                tΔ1.Error(verifyPeerCertificatesˢ);
            }
            if (clientVerifyPeerCertificates) {
                tΔ1.Error(verifyPeerCertificatesˢ2);
            }
            if (!serverVerifyConnection) {
                tΔ1.Error(verifyConnectionDidNotˢ3);
            }
            if (!clientVerifyConnection) {
                tΔ1.Error(verifyConnectionDidNotˢ4);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mlKemNotSupportedInˢ = (@string)"ML-KEM not supported in BoringCrypto FIPS mode"u8;
internal static ref readonly @string tlsmlkem0ˢ => ref tlsmlkem0ˢᵇ;
private static readonly @string tlsmlkem0ˢᵇ = "tlsmlkem=0"u8;
internal static ref readonly @string tlssecpmlkem0ˢ => ref tlssecpmlkem0ˢᵇ;
private static readonly @string tlssecpmlkem0ˢᵇ = "tlssecpmlkem=0"u8;
internal static readonly object x25519NotSupportedInFipsˢ = (@string)"X25519 not supported in FIPS mode"u8;
internal static readonly object serverDidNotUseHrrˢ = (@string)"server did not use HRR"u8;
internal static readonly object clientDidNotUseHrrˢ = (@string)"client did not use HRR"u8;
internal static readonly object serverUsedHrrˢ = (@string)"server used HRR"u8;
internal static readonly object clientUsedHrrˢ = (@string)"client used HRR"u8;

internal partial struct TestHandshakeMLKEM_type /*dyn*/ {
    internal @string name;
    internal Action<ж<global::go.crypto.tls_package.Config>> clientConfig;
    internal Action<ж<global::go.crypto.tls_package.Config>> serverConfig;
    internal Action<ж<testing.T>> preparation;
    internal slice<global::go.crypto.tls_package.CurveID> expectClient;
    internal global::go.crypto.tls_package.CurveID expectSelected;
    internal bool expectHRR;
}

public static void TestHandshakeMLKEM(ж<testing.T> t) {
    if (boring.Enabled && fips140tls.Required()) {
        t.Skip(mlKemNotSupportedInˢ);
    }
    var defaultWithPQ = slice<global::go.crypto.tls_package.CurveID>([X25519MLKEM768, SecP256r1MLKEM768, SecP384r1MLKEM1024,
        X25519, CurveP256, CurveP384, CurveP521]);
    var defaultWithoutPQ = slice<global::go.crypto.tls_package.CurveID>([X25519, CurveP256, CurveP384, CurveP521]);
    slice<TestHandshakeMLKEM_type> tests = slice<TestHandshakeMLKEM_type>([
        new(
            name: "Default",
            expectClient: defaultWithPQ,
            expectSelected: X25519MLKEM768
        ),
        new(
            name: "ClientCurvePreferences",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [X25519];
            },
            expectClient: [X25519],
            expectSelected: X25519
        ),
        new(
            name: "ServerCurvePreferencesX25519",
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [X25519];
            },
            expectClient: defaultWithPQ,
            expectSelected: X25519
        ),
        new(
            name: "ServerCurvePreferencesHRR",
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [CurveP256];
            },
            expectClient: defaultWithPQ,
            expectSelected: CurveP256,
            expectHRR: true
        ),
        new(
            name: "SecP256r1MLKEM768-Only",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [SecP256r1MLKEM768];
            },
            expectClient: [SecP256r1MLKEM768],
            expectSelected: SecP256r1MLKEM768
        ),
        new(
            name: "SecP256r1MLKEM768-HRR",
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [SecP256r1MLKEM768, CurveP256];
            },
            expectClient: defaultWithPQ,
            expectSelected: SecP256r1MLKEM768,
            expectHRR: true
        ),
        new(
            name: "SecP384r1MLKEM1024",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [SecP384r1MLKEM1024, CurveP384];
            },
            expectClient: [SecP384r1MLKEM1024, CurveP384],
            expectSelected: SecP384r1MLKEM1024
        ),
        new(
            name: "CurveP256NoHRR",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [SecP256r1MLKEM768, CurveP256];
            },
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [CurveP256];
            },
            expectClient: [SecP256r1MLKEM768, CurveP256],
            expectSelected: CurveP256
        ),
        new(
            name: "ClientMLKEMOnly",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [X25519MLKEM768];
            },
            expectClient: [X25519MLKEM768],
            expectSelected: X25519MLKEM768
        ),
        new(
            name: "ClientSortedCurvePreferences",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.CurvePreferences = [CurveP256, X25519MLKEM768];
            },
            expectClient: [X25519MLKEM768, CurveP256],
            expectSelected: X25519MLKEM768
        ),
        new(
            name: "ClientTLSv12",
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.MaxVersion = VersionTLS12;
            },
            expectClient: defaultWithoutPQ,
            expectSelected: X25519
        ),
        new(
            name: "ServerTLSv12",
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.MaxVersion = VersionTLS12;
            },
            expectClient: defaultWithPQ,
            expectSelected: X25519
        ),
        new(
            name: "GODEBUG tlsmlkem=0",
            preparation: (ж<testing.T> tΔ1) => {
                tΔ1.Setenv(godebugˢ, tlsmlkem0ˢ);
            },
            expectClient: defaultWithoutPQ,
            expectSelected: X25519
        ),
        new(
            name: "GODEBUG tlssecpmlkem=0",
            preparation: (ж<testing.T> tΔ2) => {
                tΔ2.Setenv(godebugˢ, tlssecpmlkem0ˢ);
            },
            expectClient: [X25519MLKEM768, X25519, CurveP256, CurveP384, CurveP521],
            expectSelected: X25519MLKEM768
        )
    ]);

    var baseConfig = testConfig.Clone();
    baseConfig.CurvePreferences = default;
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestHandshakeMLKEM_type(), out var Ꮡtest);
        test = vᴛ1;

        var baseConfigʗ1 = baseConfig;
        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ3) => {
            if (fips140tls.Required() && testʗ1.expectSelected == X25519) {
                tΔ3.Skip(x25519NotSupportedInFipsˢ);
            }
            if (testʗ1.preparation != nil) {
                testʗ1.preparation(tΔ3);
            } else {
                tΔ3.Parallel();
            }
            var serverConfig = baseConfigʗ1.Clone();
            if (testʗ1.serverConfig != nil) {
                testʗ1.serverConfig(serverConfig);
            }
            var testʗ2 = testʗ1;
            serverConfig.GetConfigForClient = (ж<global::go.crypto.tls_package.Config>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> hello) => {
                var expectClient = slices.Clone<slice<global::go.crypto.tls_package.CurveID>, global::go.crypto.tls_package.CurveID>(testʗ2.expectClient);
                expectClient = slices.DeleteFunc(expectClient, (global::go.crypto.tls_package.CurveID c) => fips140tls.Required() && c == X25519);
                if (!slices.Equal<slice<global::go.crypto.tls_package.CurveID>, global::go.crypto.tls_package.CurveID>(hello.SupportedCurves, expectClient)) {
                    tΔ3.Errorf("got client curves %v, expected %v", hello.SupportedCurves, expectClient);
                }
                return (default, default);
            };
            var clientConfig = baseConfigʗ1.Clone();
            if (testʗ1.clientConfig != nil) {
                testʗ1.clientConfig(clientConfig);
            }
            var (ss, cs, err) = testHandshake(tΔ3, clientConfig, serverConfig);
            if (err != nil) {
                tΔ3.Fatal(err);
            }
            if (ss.CurveID != testʗ1.expectSelected) {
                tΔ3.Errorf("server selected curve %v, expected %v", ss.CurveID, testʗ1.expectSelected);
            }
            if (cs.CurveID != testʗ1.expectSelected) {
                tΔ3.Errorf("client selected curve %v, expected %v", cs.CurveID, testʗ1.expectSelected);
            }
            if (testʗ1.expectHRR) {
                if (!ss.HelloRetryRequest) {
                    tΔ3.Error(serverDidNotUseHrrˢ);
                }
                if (!cs.HelloRetryRequest) {
                    tΔ3.Error(clientDidNotUseHrrˢ);
                }
            } else {
                if (ss.HelloRetryRequest) {
                    tΔ3.Error(serverUsedHrrˢ);
                }
                if (cs.HelloRetryRequest) {
                    tΔ3.Error(clientUsedHrrˢ);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string x509keypairleaf0ˢ => ref x509keypairleaf0ˢᵇ;
private static readonly @string x509keypairleaf0ˢᵇ = "x509keypairleaf=0"u8;
internal static readonly object leafShouldNotBePopulatedˢ = (@string)"Leaf should not be populated"u8;
internal static ref readonly @string x509keypairleaf1ˢ => ref x509keypairleaf1ˢᵇ;
private static readonly @string x509keypairleaf1ˢᵇ = "x509keypairleaf=1"u8;
internal static readonly object leafShouldBePopulatedˢ = (@string)"Leaf should be populated"u8;
internal static ref readonly @string godebugUnsetˢ => ref godebugUnsetˢᵇ;
private static readonly @string godebugUnsetˢᵇ = "GODEBUG unset"u8;

public static void TestX509KeyPairPopulateCertificate(ж<testing.T> t) {
    var (key, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    (var keyDER, err) = Δx509.MarshalPKCS8PrivateKey(key.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    var keyPEM = pem.EncodeToMemory(Ꮡ(new pem.Block(Type: "PRIVATE KEY", Bytes: keyDER)));
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test")
    ));
    (var certDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, key.Public(), key.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    var certPEM = pem.EncodeToMemory(Ꮡ(new pem.Block(Type: "CERTIFICATE", Bytes: certDER)));

    var certPEMʗ1 = certPEM;
    var keyPEMʗ1 = keyPEM;
    t.Run(x509keypairleaf0ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Setenv(godebugˢ, x509keypairleaf0ˢ);
        var (cert, errΔ1) = X509KeyPair(certPEMʗ1, keyPEMʗ1);
        if (errΔ1 != nil) {
            tΔ1.Fatal(errΔ1);
        }
        if (cert.Leaf != nil) {
            tΔ1.Fatal(leafShouldNotBePopulatedˢ);
        }
    });
    var certPEMʗ2 = certPEM;
    var keyPEMʗ2 = keyPEM;
    t.Run(x509keypairleaf1ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Setenv(godebugˢ, x509keypairleaf1ˢ);
        var (cert, errΔ2) = X509KeyPair(certPEMʗ2, keyPEMʗ2);
        if (errΔ2 != nil) {
            tΔ2.Fatal(errΔ2);
        }
        if (cert.Leaf == nil) {
            tΔ2.Fatal(leafShouldBePopulatedˢ);
        }
    });
    var certPEMʗ3 = certPEM;
    var keyPEMʗ3 = keyPEM;
    t.Run(godebugUnsetˢ, (ж<testing.T> tΔ3) => {
        var (cert, errΔ3) = X509KeyPair(certPEMʗ3, keyPEMʗ3);
        if (errΔ3 != nil) {
            tΔ3.Fatal(errΔ3);
        }
        if (cert.Leaf == nil) {
            tΔ3.Fatal(leafShouldBePopulatedˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlsHandshakeMessageOfˢ => ref tlsHandshakeMessageOfˢᵇ;
private static readonly @string tlsHandshakeMessageOfˢᵇ = "tls: handshake message of length 131071 bytes exceeds maximum of 65536 bytes"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

public static partial void TestEarlyLargeCertMsg(ж<testing.T> t) {
    var (client, server) = localPipe(new tls_test_package.testing_TжTB(t));

    var clientʗ1 = client;
    goǃ(() => {
        var (_, errΔ1) = clientʗ1.Write([(byte)recordTypeHandshake, 3, 4, 0, 4, typeCertificate, 1, 255, 255]); if (errΔ1 != nil) {
            t.Log(errΔ1);
        }
    });

    @string expectedErr = tlsHandshakeMessageOfˢ;
    var servConn = Server(server, testConfig);
    var err = servConn.Handshake();
    if (err == nil) {
        t.Fatal(unexpectedSuccessˢ);
    }
    if (err.Error() != expectedErr) {
        t.Fatalf("unexpected error: got %q, want %q", err, expectedErr);
    }
}

public static void TestLargeCertMsg(ж<testing.T> t) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test"),
        ExtraExtensions: slice<pkix.Extension>([
            new(
                Id: new asn1.ObjectIdentifier(new nint[]{1, 2, 3}.slice()),
                // Ballast to inflate the certificate beyond the
                // regular handshake record size.
                Value: new slice<byte>(65536)
            )
        ])
    ));
    (var cert, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }

    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    clientConfig.InsecureSkipVerify = true;
    serverConfig.Certificates = [
        new(
            ΔCertificate: [cert],
            PrivateKey: k.OrTypedNil()
        )
    ];
    var (_, _, errΔ1) = testHandshake(t, clientConfig, serverConfig); if (errΔ1 != nil) {
        t.Fatalf("unexpected failure: %s", errΔ1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string secretExampleˢ => ref secretExampleˢᵇ;
private static readonly @string secretExampleˢᵇ = "secret.example"u8;
internal static ref readonly @string publicExampleˢ => ref publicExampleˢᵇ;
private static readonly @string publicExampleˢᵇ = "public.example"u8;
internal static readonly object serverConnectionStateˢ = (@string)"server ConnectionState shows ECH not accepted"u8;
internal static readonly object clientConnectionStateˢ = (@string)"client ConnectionState shows ECH not accepted"u8;
internal static readonly object unexpectNumberOfˢ = (@string)"unexpect number of certificate chains"u8;
internal static readonly object unexpectNumberOfˢ2 = (@string)"unexpect number of certificates"u8;
internal static readonly object unexpectedCertificateˢ = (@string)"unexpected certificate"u8;
internal static ref readonly @string randomExampleˢ => ref randomExampleˢᵇ;
private static readonly @string randomExampleˢᵇ = "random.example"u8;

public static void TestECH(ж<testing.T> t) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        DNSNames: ["public.example"],
        NotBefore: time_package.Now().Add(-time_package.ΔHour),
        NotAfter: time_package.Now().Add(time_package.ΔHour)
    ));
    (var publicCertDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    (var publicCert, err) = Δx509.ParseCertificate(publicCertDER);
    if (err != nil) {
        t.Fatal(err);
    }
    tmpl.DNSNames[0] = secretExampleˢ;
    (var secretCertDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    (var secretCert, err) = Δx509.ParseCertificate(secretCertDER);
    if (err != nil) {
        t.Fatal(err);
    }

    slice<byte> marshalECHConfig(uint8 id, slice<byte> pubKey, @string publicName, uint8 maxNameLen) {
        var builderΔ1 = cryptobyte.NewBuilder(default);
        builderΔ1.AddUint16(extensionEncryptedClientHello);
        var pubKeyʗ1 = pubKey;
        builderΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ2) => {
            builderΔ2.AddUint8(id);
            builderΔ2.AddUint16(0x0020 /* DHKEM(X25519, HKDF-SHA256) */);
            var pubKeyʗ2 = pubKeyʗ1;
            builderΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ3) => {
                builderΔ3.AddBytes(pubKeyʗ2);
            });
            builderΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ4) => {
                builderΔ4.AddUint16(0x0001 /* HKDF-SHA256 */);
                builderΔ4.AddUint16(0x0001 /* AES-128-GCM */);
            });
            builderΔ2.AddUint8(maxNameLen);
            builderΔ2.AddUint8LengthPrefixed((ж<cryptobyte.Builder> builderΔ5) => {
                builderΔ5.AddBytes(slice<byte>(publicName));
            });
            builderΔ2.AddUint16(0); // extensions
        });

        return builderΔ1.BytesOrPanic();
    }

    (var echKey, err) = ecdh.X25519().GenerateKey(go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }

    var echConfig = marshalECHConfig(123, echKey.PublicKey().Bytes(), publicExampleˢ, 32);

    var builder = cryptobyte.NewBuilder(default);
    var echConfigʗ1 = echConfig;
    builder.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ6) => {
        builderΔ6.AddBytes(echConfigʗ1);
    });
    var echConfigList = builder.BytesOrPanic();

    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    clientConfig.InsecureSkipVerify = false;
    clientConfig.Rand = go.crypto.rand_package.Reader;
    clientConfig.Time = default;
    clientConfig.MinVersion = VersionTLS13;
    clientConfig.ServerName = secretExampleˢ;
    clientConfig.RootCAs = Δx509.NewCertPool();
    clientConfig.RootCAs.AddCert(secretCert);
    clientConfig.RootCAs.AddCert(publicCert);
    clientConfig.EncryptedClientHelloConfigList = echConfigList;
    clientConfig.ClientSessionCache = NewLRUClientSessionCache(2);
    serverConfig.InsecureSkipVerify = false;
    serverConfig.Rand = go.crypto.rand_package.Reader;
    serverConfig.Time = default;
    serverConfig.MinVersion = VersionTLS13;
    serverConfig.ServerName = publicExampleˢ;
    serverConfig.Certificates = [
        new(ΔCertificate: [publicCertDER], PrivateKey: k.OrTypedNil()),
        new(ΔCertificate: [secretCertDER], PrivateKey: k.OrTypedNil())
    ];
    serverConfig.EncryptedClientHelloKeys = slice<global::go.crypto.tls_package.EncryptedClientHelloKey>([
        new(Config: echConfig, PrivateKey: echKey.Bytes(), SendAsRetry: true)
    ]);

    var clientConfigʗ1 = clientConfig;
    var secretCertʗ1 = secretCert;
    var serverConfigʗ1 = serverConfig;
    void check() {
        var (ss, cs, errΔ1) = testHandshake(t, clientConfigʗ1, serverConfigʗ1);
        if (errΔ1 != nil) {
            t.Fatalf("unexpected failure: %s", errΔ1);
        }
        if (!ss.ECHAccepted) {
            t.Fatal(serverConnectionStateˢ);
        }
        if (!cs.ECHAccepted) {
            t.Fatal(clientConnectionStateˢ);
        }
        if (cs.ServerName != "secret.example"u8 || ss.ServerName != "secret.example"u8) {
            t.Fatalf("unexpected ConnectionState.ServerName, want %q, got server:%q, client: %q", secretExampleˢ, ss.ServerName, cs.ServerName);
        }
        if (len(cs.VerifiedChains) != 1) {
            t.Fatal(unexpectNumberOfˢ);
        }
        if (len(cs.VerifiedChains[0]) != 1) {
            t.Fatal(unexpectNumberOfˢ2);
        }
        if (!cs.VerifiedChains[0][0].Equal(secretCertʗ1)) {
            t.Fatal(unexpectedCertificateˢ);
        }
    }

    check();

    var echConfigʗ2 = echConfig;
    var echKeyʗ1 = echKey;
    serverConfig.GetEncryptedClientHelloKeys = (slice<global::go.crypto.tls_package.EncryptedClientHelloKey>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => (slice<global::go.crypto.tls_package.EncryptedClientHelloKey>([new(Config: echConfigʗ2, PrivateKey: echKeyʗ1.Bytes(), SendAsRetry: true)]), default);
    (var randKey, err) = ecdh.X25519().GenerateKey(go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    var randConfig = marshalECHConfig(32, randKey.PublicKey().Bytes(), randomExampleˢ, 32);
    serverConfig.EncryptedClientHelloKeys = slice<global::go.crypto.tls_package.EncryptedClientHelloKey>([
        new(Config: randConfig, PrivateKey: randKey.Bytes(), SendAsRetry: true)
    ]);

    check();
}

public static void TestMessageSigner(ж<testing.T> t) {
    t.Run(tlSv10ˢ, (ж<testing.T> tΔ1) => {
        testMessageSigner(tΔ1, VersionTLS10);
    });
    t.Run(tlSv12ˢ, (ж<testing.T> tΔ2) => {
        testMessageSigner(tΔ2, VersionTLS12);
    });
    t.Run(tlSv13ˢ, (ж<testing.T> tΔ3) => {
        testMessageSigner(tΔ3, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedFailureForTls101ˢ = (@string)"expected failure for TLS 1.0/1.1"u8;

internal static void testMessageSigner(ж<testing.T> t, uint16 version) {
    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    serverConfig.ClientAuth = RequireAnyClientCert;
    clientConfig.MinVersion = version;
    clientConfig.MaxVersion = version;
    serverConfig.MinVersion = version;
    serverConfig.MaxVersion = version;
    clientConfig.Certificates = [new(
        ΔCertificate: [testRSACertificate],
        PrivateKey: new messageOnlySigner(new tls_test_package.rsa_PrivateKeyжSigner(testRSAPrivateKey))
    )
    ];
    serverConfig.Certificates = [new(
        ΔCertificate: [testRSACertificate],
        PrivateKey: new messageOnlySigner(new tls_test_package.rsa_PrivateKeyжSigner(testRSAPrivateKey))
    )
    ];

    var (_, _, err) = testHandshake(t, clientConfig, serverConfig);
    if (version < VersionTLS12) {
        if (err == nil) {
            t.Fatal(expectedFailureForTls101ˢ);
        }
    } else {
        if (err != nil) {
            t.Fatalf("unexpected failure: %s", err);
        }
    }

    clientConfig.Certificates = [new(
        ΔCertificate: [testECDSACertificate],
        PrivateKey: new messageOnlySigner(new tls_test_package.ecdsa_PrivateKeyжSigner(testECDSAPrivateKey))
    )
    ];
    serverConfig.Certificates = [new(
        ΔCertificate: [testECDSACertificate],
        PrivateKey: new messageOnlySigner(new tls_test_package.ecdsa_PrivateKeyжSigner(testECDSAPrivateKey))
    )
    ];

    (_, _, err) = testHandshake(t, clientConfig, serverConfig);
    if (version < VersionTLS12) {
        if (err == nil) {
            t.Fatal(expectedFailureForTls101ˢ);
        }
    } else {
        if (err != nil) {
            t.Fatalf("unexpected failure: %s", err);
        }
    }
}

internal partial struct messageOnlySigner {
    /*embed*/ public crypto_package.Signer Signer;
}

internal static cryptoꓸPublicKey Public(this messageOnlySigner s) {
    return s.Signer.Public();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string messageOnlySignerSignˢ => ref messageOnlySignerSignˢᵇ;
private static readonly @string messageOnlySignerSignˢᵇ = "messageOnlySigner: Sign called"u8;

internal static (slice<byte> signature, error err) Sign(this messageOnlySigner s, io.Reader rand, slice<byte> msg, crypto.SignerOpts opts) {
    return (default, errors.New(messageOnlySignerSignˢ));
}

internal static (slice<byte> signature, error err) SignMessage(this messageOnlySigner s, io.Reader rand, slice<byte> msg, crypto.SignerOpts opts) {
    var h = opts.HashFunc().New();
    h.Write(msg);
    var digest = h.Sum(default);
    return s.Signer.Sign(rand, digest, opts);
}

} // end tls_internal_test_package
