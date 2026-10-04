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
using hpke = go.crypto.@internal.hpke_package;
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

"""u8;

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

"""u8);

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

"""u8);

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

"""u8;

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

"""u8);

// golang.org/issue/4477

[GoType("dyn")] partial struct keyPairTestsᴛ1 {
    internal @string algo;
    internal @string cert;
    internal @string key;
}
internal static slice<keyPairTestsᴛ1> keyPairTests = new keyPairTestsᴛ1[]{
    new("ECDSA"u8, ecdsaCertPEM, ecdsaKeyPEM),
    new("RSA"u8, rsaCertPEM, rsaKeyPEM),
    new("RSA-untyped"u8, rsaCertPEM, keyPEM)
}.slice();

public static void TestX509KeyPair(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    slice<byte> pem = default!;
    foreach (var (_, test) in keyPairTests) {
        pem = slice<byte>(test.cert + test.key);
        {
            var (_, err) = X509KeyPair(pem, pem); if (err != default!) {
                Ꮡt.Errorf("Failed to load %s cert followed by %s key: %s"u8, test.algo, test.algo, err);
            }
        }
        pem = slice<byte>(test.key + test.cert);
        {
            var (_, err) = X509KeyPair(pem, pem); if (err != default!) {
                Ꮡt.Errorf("Failed to load %s key followed by %s cert: %s"u8, test.algo, test.algo, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beenSwitchedˢ = "been switched"u8;
internal static readonly @string certificateˢ = "certificate"u8;
internal static readonly @string nonsenseˢ = "NONSENSE"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string nonsensePEMᶜ = """

-----BEGIN NONSENSE-----
Zm9vZm9vZm9v
-----END NONSENSE-----

"""u8;

public static void TestX509KeyPairErrors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, err) = X509KeyPair(slice<byte>(rsaKeyPEM), slice<byte>(rsaCertPEM));
    if (err == default!) {
        Ꮡt.Fatalf("X509KeyPair didn't return an error when arguments were switched"u8);
    }
    {
        @string subStr = beenSwitchedˢ; if (!strings.Contains(err.Error(), subStr)) {
            Ꮡt.Fatalf("Expected %q in the error when switching arguments to X509KeyPair, but the error was %q"u8, subStr, err);
        }
    }
    (_, err) = X509KeyPair(slice<byte>(rsaCertPEM), slice<byte>(rsaCertPEM));
    if (err == default!) {
        Ꮡt.Fatalf("X509KeyPair didn't return an error when both arguments were certificates"u8);
    }
    {
        @string subStr = certificateˢ; if (!strings.Contains(err.Error(), subStr)) {
            Ꮡt.Fatalf("Expected %q in the error when both arguments to X509KeyPair were certificates, but the error was %q"u8, subStr, err);
        }
    }
    @string nonsensePEM = nonsensePEMᶜ;
    (_, err) = X509KeyPair(slice<byte>(nonsensePEM), slice<byte>(nonsensePEM));
    if (err == default!) {
        Ꮡt.Fatalf("X509KeyPair didn't return an error when both arguments were nonsense"u8);
    }
    {
        @string subStr = nonsenseˢ; if (!strings.Contains(err.Error(), subStr)) {
            Ꮡt.Fatalf("Expected %q in the error when both arguments to X509KeyPair were nonsense, but the error was %q"u8, subStr, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object loadOfRsaCertificateˢ = (@string)"Load of RSA certificate succeeded with ECDSA private key"u8;
internal static readonly object loadOfEcdsaCertificateˢ = (@string)"Load of ECDSA certificate succeeded with RSA private key"u8;

public static void TestX509MixedKeyPair(ж<testing.T> Ꮡt) {
    {
        var (_, err) = X509KeyPair(slice<byte>(rsaCertPEM), slice<byte>(ecdsaKeyPEM)); if (err == default!) {
            Ꮡt.Error(loadOfRsaCertificateˢ);
        }
    }
    {
        var (_, err) = X509KeyPair(slice<byte>(ecdsaCertPEM), slice<byte>(rsaKeyPEM)); if (err == default!) {
            Ꮡt.Error(loadOfEcdsaCertificateˢ);
        }
    }
}

internal static net.Listener newLocalListener(testing.TB t) {
    t.Helper();
    var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
    if (err != default!) {
        (ln, err) = net.Listen(tcp6ˢ, "[::1]:0"u8);
    }
    if (err != default!) {
        t.Fatal(err);
    }
    return ln;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fips140tlsˢ = "fips140tls"u8;

internal static void runWithFIPSEnabled(ж<testing.T> Ꮡt, Action<ж<testing.T>> testFunc) {
    GoFrame ᒐ = default;
    try {
        var originalFIPS = fips140tls.Required();
        defer(() => {
            if (originalFIPS){
                fips140tls.Force();
            } else {
                fips140tls.TestingOnlyAbandon();
            }
        }, ref ᒐ);
        fips140tls.Force();
        Ꮡt.Run(fips140tlsˢ, testFunc);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noFips140tlsˢ = "no-fips140tls"u8;

internal static void runWithFIPSDisabled(ж<testing.T> Ꮡt, Action<ж<testing.T>> testFunc) {
    GoFrame ᒐ = default;
    try {
        var originalFIPS = fips140tls.Required();
        defer(() => {
            if (originalFIPS){
                fips140tls.Force();
            } else {
                fips140tls.TestingOnlyAbandon();
            }
        }, ref ᒐ);
        fips140tls.TestingOnlyAbandon();
        Ꮡt.Run(noFips140tlsˢ, testFunc);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInFipsModeˢ = (@string)"skipping test in FIPS mode"u8;

internal static void skipFIPS(ж<testing.T> Ꮡt) {
    if (fips140tls.Required()) {
        Ꮡt.Skip(skippingTestInFipsModeˢ);
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDialTimeout(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    ref var timeout = ref heap<time.Duration>(out var Ꮡtimeout);
    timeout = 100 * time_package.Microsecond;
    while (!Ꮡt.Failed()) {
        var acceptc = new channel<net.Conn>(0);
        var listener = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
        var acceptcʗ1 = acceptc;
        var listenerʗ1 = listener;
        goǃ(() => {
            while (ᐧ) {
                var (conn, err) = listenerʗ1.Accept();
                if (err != default!) {
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
            var (conn, err) = DialWithDialer(dialer, tcpˢ, addr, nil); if (err == default!){
                conn.Close();
                Ꮡt.Errorf("DialWithTimeout unexpectedly completed successfully"u8);
            } else 
            if (!isTimeoutError(err)) {
                Ꮡt.Errorf("resulting error not a timeout: %v\nType %T: %#v"u8, err, err, err);
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
            Ꮡt.Logf("Listener accepted a connection from %s"u8, lconn.RemoteAddr());
            lconn.Close();
        }
        // Close any spurious extra connections from the listener. (This is
        // possible if there are, for example, stray Dial calls from other tests.)
        foreach (var extraConn in acceptc) {
            Ꮡt.Logf("spurious extra connection from %s"u8, extraConn.RemoteAddr());
            extraConn.Close();
        }
        if (ok) {
            break;
        }
        Ꮡt.Logf("with timeout %v, DialWithDialer returned before listener accepted any connections; retrying"u8, timeout);
        timeout *= 2;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeShouldHaveTimedOutˢ = (@string)"Write should have timed out"u8;
internal static readonly object writeWhichPreviouslyˢ = (@string)"Write which previously failed should still time out"u8;
internal static readonly object writeTimedOutButˢ = (@string)"Write timed out but incorrectly classified the error as Temporary"u8;
internal static readonly object writeTimedOutButDidNotˢ = (@string)"Write timed out but did not classify the error as a Timeout"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDeadlineOnWrite(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var srvCh = new channel<ж<global::go.crypto.tls_package.Conn>>(1);
        var lnʗ2 = ln;
        var srvChʗ1 = srvCh;
        goǃ(() => {
            var (sconn, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != default!) {
                srvChʗ1.ᐸꟷ(default!);
                return;
            }
            var srvΔ1 = Server(sconn, testConfig.Clone());
            {
                var errΔ2 = srvΔ1.Handshake(); if (errΔ2 != default!) {
                    srvChʗ1.ᐸꟷ(default!);
                    return;
                }
            }
            srvChʗ1.ᐸꟷ(srvΔ1);
        });
        var clientConfig = testConfig.Clone();
        clientConfig.Value.MaxVersion = VersionTLS12;
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var srv = ᐸꟷ(srvCh);
        if (srv == nil) {
            Ꮡt.Error(err);
        }
        // Make sure the client/server is setup correctly and is able to do a typical Write/Read
        var buf = new slice<byte>(6);
        {
            var (_, errΔ3) = srv.Write(slice<byte>("foobar"u8)); if (errΔ3 != default!) {
                Ꮡt.Errorf("Write err: %v"u8, errΔ3);
            }
        }
        {
            var (n, errΔ4) = conn.Read(buf); if (n != 6 || errΔ4 != default! || ((sstring)buf) != "foobar"u8) {
                Ꮡt.Errorf("Read = %d, %v, data %q; want 6, nil, foobar"u8, n, errΔ4, buf);
            }
        }
        // Set a deadline which should cause Write to timeout
        {
            err = srv.SetDeadline(time_package.Now()); if (err != default!) {
                Ꮡt.Fatalf("SetDeadline(time.Now()) err: %v"u8, err);
            }
        }
        {
            (_, err) = srv.Write(slice<byte>("should fail"u8)); if (err == default!) {
                Ꮡt.Fatal(writeShouldHaveTimedOutˢ);
            }
        }
        // Clear deadline and make sure it still times out
        {
            err = srv.SetDeadline(new time_package.Time(nil)); if (err != default!) {
                Ꮡt.Fatalf("SetDeadline(time.Time{}) err: %v"u8, err);
            }
        }
        {
            (_, err) = srv.Write(slice<byte>("This connection is permanently broken"u8)); if (err == default!) {
                Ꮡt.Fatal(writeWhichPreviouslyˢ);
            }
        }
        // Verify the error
        {
            var ne = err._<netꓸError>(); if (ne.Temporary() != false) {
                Ꮡt.Error(writeTimedOutButˢ);
            }
        }
        if (!isTimeoutError(err)) {
            Ꮡt.Error(writeTimedOutButDidNotˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal delegate (nint, error) readerFunc(slice<byte> _Δp0);

internal static (nint, error) Read(this readerFunc f, slice<byte> b) {
    return f(b);
}

// TestDialer tests that tls.Dialer.DialContext can abort in the middle of a handshake.
// (The other cases are all handled by the existing dial tests in this package, which
// all also flow through the same code shared code paths)
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDialer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
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
                if (errΔ1 != default!) {
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
                return (len(b), default!);
            })),
            ServerName: "foo"u8
        ))
        );
        var (_, err) = d.DialContext(ctx, tcpˢ, ln.Addr().String());
        if (!AreEqual(err, context.Canceled)) {
            Ꮡt.Errorf("err = %v; want context.Canceled"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool isTimeoutError(error err) {
    {
        var (ne, ok) = err._<netꓸError>(ᐧ); if (ok) {
            return ne.Timeout();
        }
    }
    return false;
}

// tests that Conn.Read returns (non-zero, io.EOF) instead of
// (non-zero, nil) when a Close (alertCloseNotify) is sitting right
// behind the application data in the buffer.
public static void TestConnReadNonzeroAndEOF(ж<testing.T> Ꮡt) {
    // This test is racy: it assumes that after a write to a
    // localhost TCP connection, the peer TCP connection can
    // immediately read it. Because it's racy, we skip this test
    // in short mode, and then retry it several times with an
    // increasing sleep in between our final write (via srv.Close
    // below) and the following read.
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    error err = default!;
    for (var delay = time_package.Millisecond; delay <= 64 * time_package.Millisecond; delay *= 2) {
        {
            err = testConnReadNonzeroAndEOF(Ꮡt, delay); if (err == default!) {
                return;
            }
        }
    }
    Ꮡt.Error(err);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static error testConnReadNonzeroAndEOF(ж<testing.T> Ꮡt, time.Duration delay) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var srvCh = new channel<ж<global::go.crypto.tls_package.Conn>>(1);
        ref var serr = ref heap<error>(out var Ꮡserr);
        var lnʗ2 = ln;
        var srvChʗ1 = srvCh;
        goǃ(() => {
            var (sconn, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != default!) {
                Ꮡserr.ValueSlot = errΔ1;
                srvChʗ1.ᐸꟷ(default!);
                return;
            }
            var serverConfig = testConfig.Clone();
            var srvΔ1 = Server(sconn, serverConfig);
            {
                var errΔ2 = srvΔ1.Handshake(); if (errΔ2 != default!) {
                    Ꮡserr.ValueSlot = fmt.Errorf("handshake: %v"u8, errΔ2);
                    srvChʗ1.ᐸꟷ(default!);
                    return;
                }
            }
            srvChʗ1.ᐸꟷ(srvΔ1);
        });
        var clientConfig = testConfig.Clone();
        // In TLS 1.3, alerts are encrypted and disguised as application data, so
        // the opportunistic peek won't work.
        clientConfig.Value.MaxVersion = VersionTLS12;
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != default!) {
            Ꮡt.Fatal(err);
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
        if (n != 6 || err != default! || ((sstring)buf) != "foobar"u8) {
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
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientAndServerChannelˢ = (@string)"client and server channel bindings differ"u8;
internal static readonly object tlsUniqueIsEmptyOrZeroˢ = (@string)"tls-unique is empty or zero"u8;
internal static readonly object secondSessionDidNotUseˢ = (@string)"second session did not use resumption"u8;
internal static readonly object clientAndServerChannelˢ2 = (@string)"client and server channel bindings differ when session resumption is used"u8;
internal static readonly object resumptionTlsUniqueIsˢ = (@string)"resumption tls-unique is empty or zero"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestTLSUniqueMatches(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
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
                    if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                        return;
                    }
                    var serverConfig = testConfig.Clone();
                    serverConfig.Value.MaxVersion = VersionTLS12; // TLSUnique is not defined in TLS 1.3
                    var srv = Server(sconn, serverConfig);
                    {
                        var errΔ2 = srv.Handshake(); if (errΔ2 != default!) {
                            Ꮡt.Error(errΔ2);
                            return;
                        }
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
        clientConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        slice<byte> serverTLSUniquesValue = default!;
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
            Ꮡt.Error(clientAndServerChannelˢ);
        }
        if (serverTLSUniquesValue == default! || bytes.Equal(serverTLSUniquesValue, new slice<byte>(12))) {
            Ꮡt.Error(tlsUniqueIsEmptyOrZeroˢ);
        }
        conn.Close();
        (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        if (!conn.ConnectionState().DidResume) {
            Ꮡt.Error(secondSessionDidNotUseˢ);
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
            Ꮡt.Error(clientAndServerChannelˢ2);
        }
        if (serverTLSUniquesValue == default! || bytes.Equal(serverTLSUniquesValue, new slice<byte>(12))) {
            Ꮡt.Error(resumptionTlsUniqueIsˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wwwGoogleComHttpsˢ = "www.google.com:https"u8;
internal static readonly @string wwwGoogleComˢ = "www.google.com"u8;
internal static readonly @string wwwYahooComˢ = "www.yahoo.com"u8;

public static void TestVerifyHostname(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new tls_test_package.testing_TжTB(Ꮡt));
    var (c, err) = Dial(tcpˢ, wwwGoogleComHttpsˢ, nil);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = c.VerifyHostname(wwwGoogleComˢ); if (errΔ1 != default!) {
            Ꮡt.Fatalf("verify www.google.com: %v"u8, errΔ1);
        }
    }
    {
        var errΔ2 = c.VerifyHostname(wwwYahooComˢ); if (errΔ2 == default!) {
            Ꮡt.Fatalf("verify www.yahoo.com succeeded"u8);
        }
    }
    (c, err) = Dial(tcpˢ, wwwGoogleComHttpsˢ, Ꮡ(new Config(InsecureSkipVerify: true)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ3 = c.VerifyHostname(wwwGoogleComˢ); if (errΔ3 == default!) {
            Ꮡt.Fatalf("verify www.google.com succeeded with InsecureSkipVerify=true"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string yahooCom443ˢ = "yahoo.com:443"u8;
internal static readonly object dialErrorˢ = (@string)"Dial error:"u8;
internal static readonly object secondDialErrorˢ = (@string)"second Dial error:"u8;
internal static readonly object noConnectionUsedSessionˢ = (@string)"no connection used session resumption"u8;

public static void TestRealResumption(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new tls_test_package.testing_TжTB(Ꮡt));
    var config = Ꮡ(new Config(
        ServerName: "yahoo.com"u8,
        ClientSessionCache: NewLRUClientSessionCache(0)
    ));
    foreach (var _ᴛ1 in range(10)) {
        var (conn, err) = Dial(tcpˢ, yahooCom443ˢ, config);
        if (err != default!) {
            Ꮡt.Log(dialErrorˢ, err);
            continue;
        }
        // Do a read to consume the NewSessionTicket messages.
        fmt.Fprintf(new tls_test_package.tls_ConnжWriter(conn), "GET / HTTP/1.1\r\nHost: yahoo.com\r\nConnection: close\r\n\r\n"u8);
        conn.Read(new slice<byte>(4096));
        conn.Close();
        (conn, err) = Dial(tcpˢ, yahooCom443ˢ, config);
        if (err != default!) {
            Ꮡt.Log(secondDialErrorˢ, err);
            continue;
        }
        var state = conn.ConnectionState();
        conn.Close();
        if (state.DidResume) {
            return;
        }
    }
    Ꮡt.Fatal(noConnectionUsedSessionˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connClosedForTestˢ = "conn closed for test"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestConnCloseBreakingWrite(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var srvCh = new channel<ж<global::go.crypto.tls_package.Conn>>(1);
        ref var serr = ref heap<error>(out var Ꮡserr);
        ref var sconn = ref heap<net.Conn>(out var Ꮡsconn);
        var lnʗ2 = ln;
        var srvChʗ1 = srvCh;
        goǃ(() => {
            error errΔ1 = default!;
            (Ꮡsconn.ValueSlot, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != default!) {
                Ꮡserr.ValueSlot = errΔ1;
                srvChʗ1.ᐸꟷ(default!);
                return;
            }
            var serverConfig = testConfig.Clone();
            var srvΔ1 = Server(Ꮡsconn.ValueSlot, serverConfig);
            {
                var errΔ2 = srvΔ1.Handshake(); if (errΔ2 != default!) {
                    Ꮡserr.ValueSlot = fmt.Errorf("handshake: %v"u8, errΔ2);
                    srvChʗ1.ᐸꟷ(default!);
                    return;
                }
            }
            srvChʗ1.ᐸꟷ(srvΔ1);
        });
        var (cconn, err) = net.Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cconnʗ1 = cconn;
        defer(() => cconnʗ1.Close(), ref ᒐ);
        var conn = Ꮡ(new changeImplConn(
            Conn: cconn
        ));
        var clientConfig = testConfig.Clone();
        var tconn = Client(new tls_internal_test_package.changeImplConnжConn(conn), clientConfig);
        {
            var errΔ3 = tconn.Handshake(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        var srv = ᐸꟷ(srvCh);
        if (srv == nil) {
            Ꮡt.Fatal(serr);
        }
        defer(() => Ꮡsconn.ValueSlot.Close(), ref ᒐ);
        var connClosed = new channel<EmptyStruct>(0);
        var connClosedʗ1 = connClosed;
        conn.Value.closeFunc = error () => {
            close(connClosedʗ1);
            return default!;
        };
        var inWrite = new channel<bool>(1);
        error errConnClosed = errors.New(connClosedForTestˢ);
        var connClosedʗ2 = connClosed;
        var errConnClosedʗ1 = errConnClosed;
        var inWriteʗ1 = inWrite;
        conn.Value.writeFunc = (slice<byte> p) => {
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
            Ꮡt.Errorf("Write error = %v; want errConnClosed"u8, err);
        }
        ᐸꟷ(closeReturned);
        {
            var errΔ5 = tconn.Close(); if (!AreEqual(errΔ5, net.ErrClosed)) {
                Ꮡt.Errorf("Close error = %v; want net.ErrClosed"u8, errΔ5);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deadlockˢ = (@string)"deadlock"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestConnCloseWrite(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var clientDoneChan = new channel<EmptyStruct>(0);
        var clientDoneChanʗ1 = clientDoneChan;
        var lnʗ2 = ln;
        error serverCloseWrite() {
            GoFrame ᒐ = default;
            try {
                var (sconn, err) = lnʗ2.Accept();
                if (err != default!) {
                    return fmt.Errorf("accept: %v"u8, err);
                }
                var sconnʗ1 = sconn;
                defer(() => sconnʗ1.Close(), ref ᒐ);
                var serverConfig = testConfig.Clone();
                var srv = Server(sconn, serverConfig);
                {
                    var errΔ1 = srv.Handshake(); if (errΔ1 != default!) {
                        return fmt.Errorf("handshake: %v"u8, errΔ1);
                    }
                }
                var srvʗ1 = srv;
                defer(() => srvʗ1.Close(), ref ᒐ);
                (var data, err) = io.ReadAll(new tls_test_package.tls_ConnжReader(srv));
                if (err != default!) {
                    return err;
                }
                if (len(data) > 0) {
                    return fmt.Errorf("Read data = %q; want nothing"u8, data);
                }
                {
                    var errΔ2 = srv.CloseWrite(); if (errΔ2 != default!) {
                        return fmt.Errorf("server CloseWrite: %v"u8, errΔ2);
                    }
                }
                // Wait for clientCloseWrite to finish, so we know we
                // tested the CloseWrite before we defer the
                // sconn.Close above, which would also cause the
                // client to unblock like CloseWrite.
                ᐸꟷ(clientDoneChanʗ1);
                return default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
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
                if (err != default!) {
                    return err;
                }
                {
                    var errΔ1 = conn.Handshake(); if (errΔ1 != default!) {
                        return errΔ1;
                    }
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                {
                    var errΔ2 = conn.CloseWrite(); if (errΔ2 != default!) {
                        return fmt.Errorf("client CloseWrite: %v"u8, errΔ2);
                    }
                }
                {
                    var (_, errΔ3) = conn.Write(new byte[]{0}.slice()); if (!AreEqual(errΔ3, errShutdown)) {
                        return fmt.Errorf("CloseWrite error = %v; want errShutdown"u8, errΔ3);
                    }
                }
                (var data, err) = io.ReadAll(new tls_test_package.tls_ConnжReader(conn));
                if (err != default!) {
                    return err;
                }
                if (len(data) > 0) {
                    return fmt.Errorf("Read data = %q; want nothing"u8, data);
                }
                return default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
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
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                break;
            }
            case 1 when selᴛ15.ꟷᐳ(out _): {
                Ꮡt.Fatal(deadlockˢ);
                break;
            }}
        }
        // Also test CloseWrite being called before the handshake is
        // finished:
        {
            var ln2 = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
            var ln2ʗ1 = ln2;
            defer(() => ln2ʗ1.Close(), ref ᒐ);
            var (netConn, err) = net.Dial(tcpˢ, ln2.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var netConnʗ1 = netConn;
            defer(() => netConnʗ1.Close(), ref ᒐ);
            var conn = Client(netConn, testConfig.Clone());
            {
                var errΔ1 = conn.CloseWrite(); if (!AreEqual(errΔ1, errEarlyCloseWrite)) {
                    Ꮡt.Errorf("CloseWrite error = %v; want errEarlyCloseWrite"u8, errΔ1);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedLackOfErrorˢ = "unexpected lack of error from server"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedᶜ = "too many ignored"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestWarningAlertFlood(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new tls_test_package.testing_TжTB(Ꮡt));
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var lnʗ2 = ln;
        error server() {
            GoFrame ᒐ = default;
            try {
                var (sconn, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    return fmt.Errorf("accept: %v"u8, errΔ1);
                }
                var sconnʗ1 = sconn;
                defer(() => sconnʗ1.Close(), ref ᒐ);
                var serverConfig = testConfig.Clone();
                var srv = Server(sconn, serverConfig);
                {
                    var errΔ2 = srv.Handshake(); if (errΔ2 != default!) {
                        return fmt.Errorf("handshake: %v"u8, errΔ2);
                    }
                }
                var srvʗ1 = srv;
                defer(() => srvʗ1.Close(), ref ᒐ);
                (_, errΔ1) = io.ReadAll(new tls_test_package.tls_ConnжReader(srv));
                if (errΔ1 == default!) {
                    return errors.New(unexpectedLackOfErrorˢ);
                }
                @string expected = expectedᶜ;
                {
                    @string str = errΔ1.Error(); if (!strings.Contains(str, expected)) {
                        return fmt.Errorf("expected error containing %q, but saw: %s"u8, expected, str);
                    }
                }
                return default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        var errChan = new channel<error>(1);
        var errChanʗ1 = errChan;
        var serverʗ1 = server;
        goǃ(() => {
            errChanʗ1.ᐸꟷ(serverʗ1());
        });
        var clientConfig = testConfig.Clone();
        clientConfig.Value.MaxVersion = VersionTLS12; // there are no warning alerts in TLS 1.3
        var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        {
            var errΔ3 = conn.Handshake(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        for (nint i = 0; i < (nint)(maxUselessRecords + 1); i++) {
            conn.sendAlert(alertNoRenegotiation);
        }
        {
            var errΔ4 = ᐸꟷ(errChan); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCloneFuncFields(ж<testing.T> Ꮡt) {
    UntypedInt expectedCount = 9;
    nint called = 0;
    ref var c1 = ref heap<global::go.crypto.tls_package.Config>(out var Ꮡc1);
    c1 = new Config(
        Time: () => {
            called |= (nint)((1 << (int)(0)));
            return new time_package.Time(nil);
        },
        GetCertificate: (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => {
            called |= (nint)((1 << (int)(1)));
            return (default!, default!);
        },
        GetClientCertificate: (ж<global::go.crypto.tls_package.CertificateRequestInfo> _) => {
            called |= (nint)((1 << (int)(2)));
            return (default!, default!);
        },
        GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => {
            called |= (nint)((1 << (int)(3)));
            return (default!, default!);
        },
        VerifyPeerCertificate: (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> verifiedChains) => {
            called |= (nint)((1 << (int)(4)));
            return default!;
        },
        VerifyConnection: (global::go.crypto.tls_package.ΔConnectionState _) => {
            called |= (nint)((1 << (int)(5)));
            return default!;
        },
        UnwrapSession: (slice<byte> identity, global::go.crypto.tls_package.ΔConnectionState cs) => {
            called |= (nint)((1 << (int)(6)));
            return (default!, default!);
        },
        WrapSession: (global::go.crypto.tls_package.ΔConnectionState cs, ж<global::go.crypto.tls_package.SessionState> ss) => {
            called |= (nint)((1 << (int)(7)));
            return (default!, default!);
        },
        EncryptedClientHelloRejectionVerify: (global::go.crypto.tls_package.ΔConnectionState _) => {
            called |= (nint)((1 << (int)(8)));
            return default!;
        }
    );
    var c2 = Ꮡc1.Clone();
    (~c2).Time();
    (~c2).GetCertificate(nil);
    (~c2).GetClientCertificate(nil);
    (~c2).GetConfigForClient(nil);
    (~c2).VerifyPeerCertificate(default!, default!);
    (~c2).VerifyConnection(new ΔConnectionState(nil));
    (~c2).UnwrapSession(default!, new ΔConnectionState(nil));
    (~c2).WrapSession(new ΔConnectionState(nil), nil);
    (~c2).EncryptedClientHelloRejectionVerify(new ΔConnectionState(nil));
    if (called != (nint)(((1 << (int)(expectedCount))) - 1)) {
        Ꮡt.Fatalf("expected %d calls but saw calls %b"u8, (nint)(expectedCount), called);
    }
}

public static void TestCloneNonFuncFields(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var c1 = ref heap(new global::go.crypto.tls_package.Config(), out var Ꮡc1);
    var v = reflect.ValueOf(Ꮡc1).Elem();
    var typ = v.Type();
    for (nint i = 0; i < typ.NumField(); i++) {
        var f = v.Field(i);
        // testing/quick can't handle functions or interfaces and so
        // isn't used here.
        {
            @string fn = typ.Field(i).Name;
            var exprᴛ1 = fn;
            if (exprᴛ1 == "Rand"u8) {
                f.Set(reflect.ValueOf(((io.Reader)new tls_test_package.os_FileжReader(os.Stdin))));
            }
            else if (exprᴛ1 == "Time"u8 || exprᴛ1 == "GetCertificate"u8 || exprᴛ1 == "GetConfigForClient"u8 || exprᴛ1 == "VerifyPeerCertificate"u8 || exprᴛ1 == "VerifyConnection"u8 || exprᴛ1 == "GetClientCertificate"u8 || exprᴛ1 == "WrapSession"u8 || exprᴛ1 == "UnwrapSession"u8 || exprᴛ1 == "EncryptedClientHelloRejectionVerify"u8) {
            }
            else if (exprᴛ1 == "Certificates"u8) {
                f.Set(reflect.ValueOf(new global::go.crypto.tls_package.Certificate[]{ // DeepEqual can't compare functions. If you add a
 // function field to this list, you must also change
 // TestCloneFuncFields to ensure that the func field is
 // cloned.

                    new(ΔCertificate: new slice<byte>[]{new byte[]{(rune)'b'}.slice()}.slice())
                }.slice()));
            }
            else if (exprᴛ1 == "NameToCertificate"u8) {
                f.Set(reflect.ValueOf(new map<@string, ж<global::go.crypto.tls_package.Certificate>>{["a"u8] = default!}));
            }
            else if (exprᴛ1 == "RootCAs"u8 || exprᴛ1 == "ClientCAs"u8) {
                f.Set(reflect.ValueOf(Δx509.NewCertPool().OrTypedNil()));
            }
            else if (exprᴛ1 == "ClientSessionCache"u8) {
                f.Set(reflect.ValueOf(NewLRUClientSessionCache(10)));
            }
            else if (exprᴛ1 == "KeyLogWriter"u8) {
                f.Set(reflect.ValueOf(((io.Writer)new os.FileжWriter(os.Stdout))));
            }
            else if (exprᴛ1 == "NextProtos"u8) {
                f.Set(reflect.ValueOf(new @string[]{"a"u8, "b"u8}.slice()));
            }
            else if (exprᴛ1 == "ServerName"u8) {
                f.Set(reflect.ValueOf((@string)"b"u8));
            }
            else if (exprᴛ1 == "ClientAuth"u8) {
                f.Set(reflect.ValueOf(VerifyClientCertIfGiven));
            }
            else if (exprᴛ1 == "InsecureSkipVerify"u8 || exprᴛ1 == "SessionTicketsDisabled"u8 || exprᴛ1 == "DynamicRecordSizingDisabled"u8 || exprᴛ1 == "PreferServerCipherSuites"u8) {
                f.Set(reflect.ValueOf(true));
            }
            else if (exprᴛ1 == "MinVersion"u8 || exprᴛ1 == "MaxVersion"u8) {
                f.Set(reflect.ValueOf((uint16)VersionTLS12));
            }
            else if (exprᴛ1 == "SessionTicketKey"u8) {
                f.Set(reflect.ValueOf(new byte[]{}.array(32)));
            }
            else if (exprᴛ1 == "CipherSuites"u8) {
                f.Set(reflect.ValueOf(new uint16[]{1, 2}.slice()));
            }
            else if (exprᴛ1 == "CurvePreferences"u8) {
                f.Set(reflect.ValueOf(new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice()));
            }
            else if (exprᴛ1 == "Renegotiation"u8) {
                f.Set(reflect.ValueOf(RenegotiateOnceAsClient));
            }
            else if (exprᴛ1 == "EncryptedClientHelloConfigList"u8) {
                f.Set(reflect.ValueOf(new byte[]{(rune)'x'}.slice()));
            }
            else if (exprᴛ1 == "EncryptedClientHelloKeys"u8) {
                f.Set(reflect.ValueOf(new global::go.crypto.tls_package.EncryptedClientHelloKey[]{
                    new(Config: new byte[]{1}.slice(), PrivateKey: new byte[]{1}.slice())
                }.slice()));
            }
            else if (exprᴛ1 == "mutex"u8 || exprᴛ1 == "autoSessionTicketKeys"u8 || exprᴛ1 == "sessionTicketKeys"u8) {
                continue; // these are unexported fields that are handled separately
            }
            else { /* default: */
                Ꮡt.Errorf("all fields must be accounted for, but saw unknown field %q"u8, fn);
            }
        }

    }
    // Set the unexported fields related to session ticket keys, which are copied with Clone().
    c1.autoSessionTicketKeys = new global::go.crypto.tls_package.ticketKey[]{c1.ticketKeyFromBytes(c1.SessionTicketKey)}.slice();
    c1.sessionTicketKeys = new global::go.crypto.tls_package.ticketKey[]{c1.ticketKeyFromBytes(c1.SessionTicketKey)}.slice();
    var c2 = Ꮡc1.Clone();
    if (!reflect.DeepEqual(Ꮡc1, c2.OrTypedNil())) {
        Ꮡt.Errorf("clone failed to copy a field"u8);
    }
}

public static void TestCloneNilConfig(ж<testing.T> Ꮡt) {
    ж<global::go.crypto.tls_package.Config> config = default!;
    {
        var cc = config.Clone(); if (cc != nil) {
            Ꮡt.Fatalf("Clone with nil should return nil, got: %+v"u8, cc.OrTypedNil());
        }
    }
}

// changeImplConn is a net.Conn which can change its Write and Close
// methods.
[GoType] internal partial struct changeImplConn {
    [GoEmbedded] public net_package.Conn Conn;
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

[GoRecv] internal static (nint n, error err) Write(this ref changeImplConn w, slice<byte> p) {
    if (w.writeFunc != default!) {
        return w.writeFunc(p);
    }
    return w.Conn.Write(p);
}

[GoRecv] internal static error Close(this ref changeImplConn w) {
    if (w.closeFunc != default!) {
        return w.closeFunc();
    }
    return w.Conn.Close();
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void throughput(ж<testing.B> Ꮡb, uint16 version, int64 totalBytes, bool dynamicRecordSizingDisabled) {
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
                if (err != default!) {
                    // panic rather than synchronize to avoid benchmark overhead
                    // (cannot call b.Fatal in goroutine)
                    throw panic(fmt.Errorf("accept: %v"u8, err));
                }
                var serverConfig = testConfig.Clone();
                serverConfig.Value.CipherSuites = default!; // the defaults may prefer faster ciphers
                serverConfig.Value.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
                var srv = Server(sconn, serverConfig);
                {
                    var errΔ1 = srv.Handshake(); if (errΔ1 != default!) {
                        throw panic(fmt.Errorf("handshake: %v"u8, errΔ1));
                    }
                }
                {
                    var (_, errΔ2) = io.CopyBuffer(new tls_test_package.tls_ConnжWriter(srv), new tls_test_package.tls_ConnжReader(srv), bufΔ1); if (errΔ2 != default!) {
                        throw panic(fmt.Errorf("copy buffer: %v"u8, errΔ2));
                    }
                }
            }
        });
        b.SetBytes(totalBytes);
        var clientConfig = testConfig.Clone();
        clientConfig.Value.CipherSuites = default!; // the defaults may prefer faster ciphers
        clientConfig.Value.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
        clientConfig.Value.MaxVersion = version;
        var buf = new slice<byte>(bufsize);
        nint chunks = (nint)math.Ceil((float64)totalBytes / (float64)len(buf));
        for (nint i = 0; i < N; i++) {
            var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
            if (err != default!) {
                Ꮡb.Fatal(err);
            }
            for (nint j = 0; j < chunks; j++) {
                var (_, errΔ1) = conn.Write(buf);
                if (errΔ1 != default!) {
                    Ꮡb.Fatal(errΔ1);
                }
                (_, errΔ1) = io.ReadFull(new tls_test_package.tls_ConnжReader(conn), buf);
                if (errΔ1 != default!) {
                    Ꮡb.Fatal(errΔ1);
                }
            }
            conn.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkThroughput(ж<testing.B> Ꮡb) {
    foreach (var (_, mode) in new @string[]{"Max"u8, "Dynamic"u8}.slice()) {
        for (nint sizeᴛ1 = 1; sizeᴛ1 <= 64; sizeᴛ1 <<= (int)(1)) {
            var size = sizeᴛ1;
            @string name = fmt.Sprintf("%sPacket/%dMB"u8, mode, size);
            Ꮡb.Run(name, (ж<testing.B> bΔ1) => {
                bΔ1.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ2) => {
                    throughput(bΔ2, VersionTLS12, (int64)((size << (int)(20))), mode == "Max"u8);
                });
                bΔ1.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ3) => {
                    throughput(bΔ3, VersionTLS13, (int64)((size << (int)(20))), mode == "Max"u8);
                });
            });
        }
    }
}

[GoType] internal partial struct slowConn {
    [GoEmbedded] public net_package.Conn Conn;
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

[GoRecv] internal static (nint, error) Write(this ref slowConn c, slice<byte> p) {
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
            if (err != default!) {
                return (wrote, err);
            }
        }
    }
    return (len(p), default!);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void latency(ж<testing.B> Ꮡb, uint16 version, nint bps, bool dynamicRecordSizingDisabled) {
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
                if (err != default!) {
                    // panic rather than synchronize to avoid benchmark overhead
                    // (cannot call b.Fatal in goroutine)
                    throw panic(fmt.Errorf("accept: %v"u8, err));
                }
                var serverConfig = testConfig.Clone();
                serverConfig.Value.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
                var srv = Server(new tls_internal_test_package.slowConnжConn(Ꮡ(new slowConn(sconn, bps))), serverConfig);
                {
                    var errΔ1 = srv.Handshake(); if (errΔ1 != default!) {
                        throw panic(fmt.Errorf("handshake: %v"u8, errΔ1));
                    }
                }
                io.Copy(new tls_test_package.tls_ConnжWriter(srv), new tls_test_package.tls_ConnжReader(srv));
            }
        });
        var clientConfig = testConfig.Clone();
        clientConfig.Value.DynamicRecordSizingDisabled = dynamicRecordSizingDisabled;
        clientConfig.Value.MaxVersion = version;
        var buf = new slice<byte>(16384);
        var peek = new slice<byte>(1);
        for (nint i = 0; i < N; i++) {
            var (conn, err) = Dial(tcpˢ, ln.Addr().String(), clientConfig);
            if (err != default!) {
                Ꮡb.Fatal(err);
            }
            // make sure we're connected and previous connection has stopped
            {
                var (_, errΔ1) = conn.Write(buf[..1]); if (errΔ1 != default!) {
                    Ꮡb.Fatal(errΔ1);
                }
            }
            {
                var (_, errΔ2) = io.ReadFull(new tls_test_package.tls_ConnжReader(conn), peek); if (errΔ2 != default!) {
                    Ꮡb.Fatal(errΔ2);
                }
            }
            {
                var (_, errΔ3) = conn.Write(buf); if (errΔ3 != default!) {
                    Ꮡb.Fatal(errΔ3);
                }
            }
            {
                (_, err) = io.ReadFull(new tls_test_package.tls_ConnжReader(conn), peek); if (err != default!) {
                    Ꮡb.Fatal(err);
                }
            }
            conn.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkLatency(ж<testing.B> Ꮡb) {
    foreach (var (_, mode) in new @string[]{"Max"u8, "Dynamic"u8}.slice()) {
        foreach (var (_, kbps) in new nint[]{200, 500, 1000, 2000, 5000}.slice()) {
            @string name = fmt.Sprintf("%sPacket/%dkbps"u8, mode, kbps);
            Ꮡb.Run(name, (ж<testing.B> bΔ1) => {
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

public static void TestConnectionStateMarshal(ж<testing.T> Ꮡt) {
    var cs = Ꮡ(new ΔConnectionState(nil));
    var (_, err) = json.Marshal(cs.OrTypedNil());
    if (err != default!) {
        Ꮡt.Errorf("json.Marshal failed on ConnectionState: %v"u8, err);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string alpnProtocolᶜ = "golang"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string serverNameᶜ = "example.golang"u8;

public static void TestConnectionState(ж<testing.T> Ꮡt) {
    skipFIPS(Ꮡt); // Test certificates not FIPS compatible.
    var (issuer, err) = Δx509.ParseCertificate(testRSACertificateIssuer);
    if (err != default!) {
        throw panic(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);
    @string alpnProtocol = alpnProtocolᶜ;
    @string serverName = serverNameᶜ;
    slice<slice<byte>> scts = new slice<byte>[]{slice<byte>("dummy sct 1"u8), slice<byte>("dummy sct 2"u8)}.slice();
    slice<byte> ocsp = slice<byte>("dummy ocsp"u8);
    foreach (var (_, vᴛ1) in new uint16[]{VersionTLS12, VersionTLS13}.slice()) {
        ref var v = ref heap(new uint16(), out var Ꮡv);
        v = vᴛ1;

        @string name = default!;
        var exprᴛ1 = v;
        if (exprᴛ1 == VersionTLS12) {
            name = tlSv12ˢ;
        }
        else if (exprᴛ1 == VersionTLS13) {
            name = tlSv13ˢ;
        }

        var ocspʗ1 = ocsp;
        var rootCAsʗ1 = rootCAs;
        var sctsʗ1 = scts;
        var vʗ1 = v;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var config = Ꮡ(new Config(
                Time: testTime,
                Rand: new zeroSource(nil),
                Certificates: new slice<global::go.crypto.tls_package.Certificate>(1),
                MaxVersion: vʗ1,
                RootCAs: rootCAsʗ1,
                ClientCAs: rootCAsʗ1,
                ClientAuth: RequireAndVerifyClientCert,
                NextProtos: new @string[]{alpnProtocol}.slice(),
                ServerName: serverName
            ));
            (~config).Certificates[0].ΔCertificate = new slice<byte>[]{testRSACertificate}.slice();
            (~config).Certificates[0].PrivateKey = testRSAPrivateKey.OrTypedNil();
            (~config).Certificates[0].SignedCertificateTimestamps = sctsʗ1;
            (~config).Certificates[0].OCSPStaple = ocspʗ1;
            var (ss, cs, errΔ1) = testHandshake(tΔ1, config, config);
            if (errΔ1 != default!) {
                tΔ1.Fatalf("Handshake failed: %v"u8, errΔ1);
            }
            if (ss.Version != vʗ1 || cs.Version != vʗ1) {
                tΔ1.Errorf("Got versions %x (server) and %x (client), expected %x"u8, ss.Version, cs.Version, vʗ1);
            }
            if (!ss.HandshakeComplete || !cs.HandshakeComplete) {
                tΔ1.Errorf("Got HandshakeComplete %v (server) and %v (client), expected true"u8, ss.HandshakeComplete, cs.HandshakeComplete);
            }
            if (ss.DidResume || cs.DidResume) {
                tΔ1.Errorf("Got DidResume %v (server) and %v (client), expected false"u8, ss.DidResume, cs.DidResume);
            }
            if (ss.CipherSuite == 0 || cs.CipherSuite == 0) {
                tΔ1.Errorf("Got invalid cipher suite: %v (server) and %v (client)"u8, ss.CipherSuite, cs.CipherSuite);
            }
            if (ss.NegotiatedProtocol != alpnProtocol || cs.NegotiatedProtocol != alpnProtocol) {
                tΔ1.Errorf("Got negotiated protocol %q (server) and %q (client), expected %q"u8, ss.NegotiatedProtocol, cs.NegotiatedProtocol, alpnProtocol);
            }
            if (!cs.NegotiatedProtocolIsMutual) {
                tΔ1.Errorf("Got false NegotiatedProtocolIsMutual on the client side"u8);
            }
            // NegotiatedProtocolIsMutual on the server side is unspecified.
            if (ss.ServerName != serverName) {
                tΔ1.Errorf("Got server name %q, expected %q"u8, ss.ServerName, serverName);
            }
            if (cs.ServerName != serverName) {
                tΔ1.Errorf("Got server name on client connection %q, expected %q"u8, cs.ServerName, serverName);
            }
            if (len(ss.PeerCertificates) != 1 || len(cs.PeerCertificates) != 1) {
                tΔ1.Errorf("Got %d (server) and %d (client) peer certificates, expected %d"u8, len(ss.PeerCertificates), len(cs.PeerCertificates), (nint)(1));
            }
            if (len(ss.VerifiedChains) != 1 || len(cs.VerifiedChains) != 1){
                tΔ1.Errorf("Got %d (server) and %d (client) verified chains, expected %d"u8, len(ss.VerifiedChains), len(cs.VerifiedChains), (nint)(1));
            } else 
            if (len(ss.VerifiedChains[0]) != 2 || len(cs.VerifiedChains[0]) != 2) {
                tΔ1.Errorf("Got %d (server) and %d (client) long verified chain, expected %d"u8, len(ss.VerifiedChains[0]), len(cs.VerifiedChains[0]), (nint)(2));
            }
            if (len(cs.SignedCertificateTimestamps) != 2) {
                tΔ1.Errorf("Got %d SCTs, expected %d"u8, len(cs.SignedCertificateTimestamps), (nint)(2));
            }
            if (!bytes.Equal(cs.OCSPResponse, ocspʗ1)) {
                tΔ1.Errorf("Got OCSPs %x, expected %x"u8, cs.OCSPResponse, ocspʗ1);
            }
            // Only TLS 1.3 supports OCSP and SCTs on client certs.
            if (vʗ1 == VersionTLS13) {
                if (len(ss.SignedCertificateTimestamps) != 2) {
                    tΔ1.Errorf("Got %d client SCTs, expected %d"u8, len(ss.SignedCertificateTimestamps), (nint)(2));
                }
                if (!bytes.Equal(ss.OCSPResponse, ocspʗ1)) {
                    tΔ1.Errorf("Got client OCSPs %x, expected %x"u8, ss.OCSPResponse, ocspʗ1);
                }
            }
            if (vʗ1 == VersionTLS13){
                if (ss.TLSUnique != default! || cs.TLSUnique != default!) {
                    tΔ1.Errorf("Got TLSUnique %x (server) and %x (client), expected nil in TLS 1.3"u8, ss.TLSUnique, cs.TLSUnique);
                }
            } else {
                if (ss.TLSUnique == default! || cs.TLSUnique == default!) {
                    tΔ1.Errorf("Got TLSUnique %x (server) and %x (client), expected non-nil"u8, ss.TLSUnique, cs.TLSUnique);
                }
            }
        });
    }
}

// Issue 28744: Ensure that we don't modify memory
// that Config doesn't own such as Certificates.
public static void TestBuildNameToCertificate_doesntModifyCertificates(ж<testing.T> Ꮡt) {
    var c0 = new Certificate(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    );
    var c1 = new Certificate(
        ΔCertificate: new slice<byte>[]{testSNICertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    );
    var config = testConfig.Clone();
    config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{c0, c1}.slice();
    config.BuildNameToCertificate();
    var got = config.Value.Certificates;
    var want = new global::go.crypto.tls_package.Certificate[]{c0, c1}.slice();
    if (!reflect.DeepEqual(got, want)) {
        Ꮡt.Fatalf("Certificates were mutated by BuildNameToCertificate\nGot: %#v\nWant: %#v\n"u8, got, want);
    }
}

internal static @string testingKey(@string s) {
    return strings.ReplaceAll(s, "TESTING KEY"u8, "PRIVATE KEY"u8);
}

[GoType("dyn")] internal partial struct TestClientHelloInfo_SupportsCertificate_tests {
    internal ж<global::go.crypto.tls_package.Certificate> c;
    internal ж<global::go.crypto.tls_package.ClientHelloInfo> chi;
    internal @string wantErr;
}

public static void TestClientHelloInfo_SupportsCertificate(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    skipFIPS(Ꮡt); // Test certificates not FIPS compatible.
    var rsaCert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    ));
    var pkcs1Cert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil(),
        SupportedSignatureAlgorithms: new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA1, PKCS1WithSHA256}.slice()
    ));
    var ecdsaCert = Ꮡ(new Certificate( // ECDSA P-256 certificate

        ΔCertificate: new slice<byte>[]{testP256Certificate}.slice(),
        PrivateKey: testP256PrivateKey.OrTypedNil()
    ));
    var ed25519Cert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testEd25519Certificate}.slice(),
        PrivateKey: testEd25519PrivateKey
    ));
    var tests = new TestClientHelloInfo_SupportsCertificate_tests[]{
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            ServerName: "example.golang"u8,
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13}.slice()
        )), ""u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256, ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13, VersionTLS12}.slice()
        )), ""u8),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            ServerName: "example.com"u8,
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13}.slice()
        )), "not valid for requested server name"u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP384AndSHA384}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13}.slice()
        )), "signature algorithms"u8),
        new(pkcs1Cert, Ꮡ(new ClientHelloInfo(
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256, ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13}.slice()
        )), "signature algorithms"u8),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA1}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13, VersionTLS12}.slice()
        )), "signature algorithms"u8),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA1}.slice(),
            SupportedVersions: new uint16[]{VersionTLS13, VersionTLS12}.slice(),
            config: Ꮡ(new Config(
                CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice(),
                MaxVersion: VersionTLS12
            ))
        )), ""u8), // Check that mutual version selection works.

        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), ""u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP384AndSHA384}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), ""u8), // TLS 1.2 does not restrict curves based on the SignatureScheme.

        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: default!,
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), ""u8), // TLS 1.2 comes with default signature schemes.

        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), "cipher suite"u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice(),
            config: Ꮡ(new Config(
                CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice()
            ))
        )), "cipher suite"u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP384}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), "certificate curve"u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{1}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), "doesn't support ECDHE"u8),
        new(ecdsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), "signature algorithms"u8),
        new(ed25519Cert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(), // only relevant for ECDHE support

            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), ""u8),
        new(ed25519Cert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(), // only relevant for ECDHE support

            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(),
            SupportedVersions: new uint16[]{VersionTLS10}.slice(),
            config: Ꮡ(new Config(MinVersion: VersionTLS10))
        )), "doesn't support Ed25519"u8),
        new(ed25519Cert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{}.slice(),
            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SignatureSchemes: new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice()
        )), "doesn't support ECDHE"u8),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA}.slice(),
            SupportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(), // only relevant for ECDHE support

            SupportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
            SupportedVersions: new uint16[]{VersionTLS10}.slice(),
            config: Ꮡ(new Config(MinVersion: VersionTLS10))
        )), ""u8),
        new(rsaCert, Ꮡ(new ClientHelloInfo(
            CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice(),
            SupportedVersions: new uint16[]{VersionTLS12}.slice(),
            config: Ꮡ(new Config(
                CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice()
            ))
        )), ""u8)
    }.slice();
    // static RSA fallback
    foreach (var (i, tt) in tests) {
        var err = tt.chi.SupportsCertificate(tt.c);
        switch (ᐧ) {
        case {} when tt.wantErr == ""u8 && err != default!: {
            Ꮡt.Errorf("%d: unexpected error: %v"u8, i, err);
            break;
        }
        case {} when tt.wantErr != ""u8 && err == default!: {
            Ꮡt.Errorf("%d: unexpected success"u8, i);
            break;
        }
        case {} when tt.wantErr != ""u8 && !strings.Contains(err.Error(), tt.wantErr): {
            Ꮡt.Errorf("%d: got error %q, expected %q"u8, i, err, tt.wantErr);
            break;
        }}

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rc4ˢ = "RC4"u8;
internal static readonly @string cbcSha256ˢ = "CBC_SHA256"u8;
internal static readonly @string aesˢ = "AES"u8;
internal static readonly @string chacha20ˢ = "CHACHA20"u8;
internal static readonly @string aes128ˢ = "AES_128"u8;
internal static readonly @string aes256ˢ = "AES_256"u8;
internal static readonly object preferenceOrderIsNotˢ = (@string)"preference order is not sorted according to the rules"u8;

public static void TestCipherSuites(ж<testing.T> Ꮡt) {
    uint16 lastID = default!;
    foreach (var (_, c) in CipherSuites()) {
        if (lastID > (~c).ID){
            Ꮡt.Errorf("CipherSuites are not ordered by ID: got %#04x after %#04x"u8, (~c).ID, lastID);
        } else {
            lastID = c.Value.ID;
        }
        if ((~c).Insecure) {
            Ꮡt.Errorf("%#04x: Insecure CipherSuite returned by CipherSuites()"u8, (~c).ID);
        }
    }
    lastID = 0;
    foreach (var (_, c) in InsecureCipherSuites()) {
        if (lastID > (~c).ID){
            Ꮡt.Errorf("InsecureCipherSuites are not ordered by ID: got %#04x after %#04x"u8, (~c).ID, lastID);
        } else {
            lastID = c.Value.ID;
        }
        if (!(~c).Insecure) {
            Ꮡt.Errorf("%#04x: not Insecure CipherSuite returned by InsecureCipherSuites()"u8, (~c).ID);
        }
    }
    ж<global::go.crypto.tls_package.CipherSuite> CipherSuiteByID(uint16 id) {
        foreach (var (_, c) in CipherSuites()) {
            if ((~c).ID == id) {
                return c;
            }
        }
        foreach (var (_, c) in InsecureCipherSuites()) {
            if ((~c).ID == id) {
                return c;
            }
        }
        return default!;
    }
    foreach (var (_, c) in ΔcipherSuites) {
        var cc = CipherSuiteByID((~c).id);
        if (cc == nil) {
            Ꮡt.Errorf("%#04x: no CipherSuite entry"u8, (~c).id);
            continue;
        }
        {
            var tls12Only = (nint)((~c).flags & (nint)suiteTLS12) != 0; if (tls12Only && len((~cc).SupportedVersions) != 1){
                Ꮡt.Errorf("%#04x: suite is TLS 1.2 only, but SupportedVersions is %v"u8, (~c).id, (~cc).SupportedVersions);
            } else 
            if (!tls12Only && len((~cc).SupportedVersions) != 3) {
                Ꮡt.Errorf("%#04x: suite TLS 1.0-1.2, but SupportedVersions is %v"u8, (~c).id, (~cc).SupportedVersions);
            }
        }
        if ((~cc).Insecure){
            if (slices.Contains(defaultCipherSuites(), (~c).id)) {
                Ꮡt.Errorf("%#04x: insecure suite in default list"u8, (~c).id);
            }
        } else {
            if (!slices.Contains(defaultCipherSuites(), (~c).id)) {
                Ꮡt.Errorf("%#04x: secure suite not in default list"u8, (~c).id);
            }
        }
        {
            @string got = CipherSuiteName((~c).id); if (got != (~cc).Name) {
                Ꮡt.Errorf("%#04x: unexpected CipherSuiteName: got %q, expected %q"u8, (~c).id, got, (~cc).Name);
            }
        }
    }
    foreach (var (_, c) in cipherSuitesTLS13) {
        var cc = CipherSuiteByID((~c).id);
        if (cc == nil) {
            Ꮡt.Errorf("%#04x: no CipherSuite entry"u8, (~c).id);
            continue;
        }
        if ((~cc).Insecure) {
            Ꮡt.Errorf("%#04x: Insecure %v, expected false"u8, (~c).id, (~cc).Insecure);
        }
        if (len((~cc).SupportedVersions) != 1 || (~cc).SupportedVersions[0] != VersionTLS13) {
            Ꮡt.Errorf("%#04x: suite is TLS 1.3 only, but SupportedVersions is %v"u8, (~c).id, (~cc).SupportedVersions);
        }
        {
            @string got = CipherSuiteName((~c).id); if (got != (~cc).Name) {
                Ꮡt.Errorf("%#04x: unexpected CipherSuiteName: got %q, expected %q"u8, (~c).id, got, (~cc).Name);
            }
        }
    }
    {
        @string got = CipherSuiteName(0xabc); if (got != "0x0ABC"u8) {
            Ꮡt.Errorf("unexpected fallback CipherSuiteName: got %q, expected 0x0ABC"u8, got);
        }
    }
    if (len(cipherSuitesPreferenceOrder) != len(ΔcipherSuites)) {
        Ꮡt.Errorf("cipherSuitesPreferenceOrder is not the same size as cipherSuites"u8);
    }
    if (len(cipherSuitesPreferenceOrderNoAES) != len(cipherSuitesPreferenceOrder)) {
        Ꮡt.Errorf("cipherSuitesPreferenceOrderNoAES is not the same size as cipherSuitesPreferenceOrder"u8);
    }
    // Check that disabled suites are marked insecure.
    foreach (var (_, badSuites) in new map<uint16, bool>[]{disabledCipherSuites, rsaKexCiphers}.slice()) {
        foreach (var (id, _) in badSuites) {
            var c = CipherSuiteByID(id);
            if (c == nil) {
                Ꮡt.Errorf("%#04x: no CipherSuite entry"u8, id);
                continue;
            }
            if (!(~c).Insecure) {
                Ꮡt.Errorf("%#04x: disabled by default but not marked insecure"u8, id);
            }
        }
    }
    foreach (var (i, prefOrder) in new slice<uint16>[]{cipherSuitesPreferenceOrder, cipherSuitesPreferenceOrderNoAES}.slice()) {
        // Check that insecure and HTTP/2 bad cipher suites are at the end of
        // the preference lists.
        bool sawInsecure = default!;
        bool sawBad = default!;
        foreach (var (_, id) in prefOrder) {
            var c = CipherSuiteByID(id);
            if (c == nil) {
                Ꮡt.Errorf("%#04x: no CipherSuite entry"u8, id);
                continue;
            }
            if ((~c).Insecure){
                sawInsecure = true;
            } else 
            if (sawInsecure) {
                Ꮡt.Errorf("%#04x: secure suite after insecure one(s)"u8, id);
            }
            if (http2isBadCipher(id)){
                sawBad = true;
            } else 
            if (sawBad) {
                Ꮡt.Errorf("%#04x: non-bad suite after bad HTTP/2 one(s)"u8, id);
            }
        }
        // Check that the list is sorted according to the documented criteria.
        var isBetter = nint (uint16 a, uint16 b) => {
            var (aSuite, bSuite) = (cipherSuiteByID(a), cipherSuiteByID(b));
            @string aName = CipherSuiteName(a);
            @string bName = CipherSuiteName(b);
            // * < RC4
            if (!strings.Contains(aName, rc4ˢ) && strings.Contains(bName, rc4ˢ)){
                return -1;
            } else 
            if (strings.Contains(aName, rc4ˢ) && !strings.Contains(bName, rc4ˢ)) {
                return +1;
            }
            // * < CBC_SHA256
            if (!strings.Contains(aName, cbcSha256ˢ) && strings.Contains(bName, cbcSha256ˢ)){
                return -1;
            } else 
            if (strings.Contains(aName, cbcSha256ˢ) && !strings.Contains(bName, cbcSha256ˢ)) {
                return +1;
            }
            // * < 3DES
            if (!strings.Contains(aName, "3DES"u8) && strings.Contains(bName, "3DES"u8)){
                return -1;
            } else 
            if (strings.Contains(aName, "3DES"u8) && !strings.Contains(bName, "3DES"u8)) {
                return +1;
            }
            // ECDHE < *
            if ((nint)((~aSuite).flags & (nint)suiteECDHE) != 0 && (nint)((~bSuite).flags & (nint)suiteECDHE) == 0){
                return -1;
            } else 
            if ((nint)((~aSuite).flags & (nint)suiteECDHE) == 0 && (nint)((~bSuite).flags & (nint)suiteECDHE) != 0) {
                return +1;
            }
            // AEAD < CBC
            if ((~aSuite).aead != default! && (~bSuite).aead == default!){
                return -1;
            } else 
            if ((~aSuite).aead == default! && (~bSuite).aead != default!) {
                return +1;
            }
            // AES < ChaCha20
            if (strings.Contains(aName, aesˢ) && strings.Contains(bName, chacha20ˢ)){
                // negative for cipherSuitesPreferenceOrder
                if (i == 0){
                    return -1;
                } else {
                    return +1;
                }
            } else 
            if (strings.Contains(aName, chacha20ˢ) && strings.Contains(bName, aesˢ)) {
                // negative for cipherSuitesPreferenceOrderNoAES
                if (i != 0){
                    return -1;
                } else {
                    return +1;
                }
            }
            // AES-128 < AES-256
            if (strings.Contains(aName, aes128ˢ) && strings.Contains(bName, aes256ˢ)){
                return -1;
            } else 
            if (strings.Contains(aName, aes256ˢ) && strings.Contains(bName, aes128ˢ)) {
                return +1;
            }
            // ECDSA < RSA
            if ((nint)((~aSuite).flags & (nint)suiteECSign) != 0 && (nint)((~bSuite).flags & (nint)suiteECSign) == 0){
                return -1;
            } else 
            if ((nint)((~aSuite).flags & (nint)suiteECSign) == 0 && (nint)((~bSuite).flags & (nint)suiteECSign) != 0) {
                return +1;
            }
            Ꮡt.Fatalf("two ciphersuites are equal by all criteria: %v and %v"u8, aName, bName);
            throw panic("unreachable");
        };
        if (!slices.IsSortedFunc(prefOrder, isBetter)) {
            Ꮡt.Error(preferenceOrderIsNotˢ);
        }
    }
}

public static void TestVersionName(ж<testing.T> Ꮡt) {
    {
        @string got = VersionName(VersionTLS13);
        @string exp = tls13ˢ; if (got != exp) {
            Ꮡt.Errorf("unexpected VersionName: got %q, expected %q"u8, got, exp);
        }
    }
    {
        @string got = VersionName(0x12a);
        @string exp = "0x012A"u8; if (got != exp) {
            Ꮡt.Errorf("unexpected fallback VersionName: got %q, expected %q"u8, got, exp);
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

[GoType] internal partial struct brokenSigner {
    [GoEmbedded] public crypto_package.Signer Signer;
}

internal static (slice<byte> signature, error err) Sign(this brokenSigner s, io.Reader rand, slice<byte> digest, crypto.SignerOpts opts) {
    // Replace opts with opts.HashFunc(), so rsa.PSSOptions are discarded.
    return s.Signer.Sign(rand, digest, opts.HashFunc());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedBrokenˢ = (@string)"expected broken certificate to cause connection to fail"u8;

// TestPKCS1OnlyCert uses a client certificate with a broken crypto.Signer that
// always makes PKCS #1 v1.5 signatures, so can't be used with RSA-PSS.
public static void TestPKCS1OnlyCert(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var clientConfig = testConfig.Clone();
    clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: new brokenSigner(new tls_test_package.rsa_PrivateKeyжSigner(testRSAPrivateKey))
    )
    }.slice();
    var serverConfig = testConfig.Clone();
    serverConfig.Value.MaxVersion = VersionTLS12; // TLS 1.3 doesn't support PKCS #1 v1.5
    serverConfig.Value.ClientAuth = RequireAnyClientCert;
    // If RSA-PSS is selected, the handshake should fail.
    {
        var (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig); if (err == default!) {
            Ꮡt.Fatal(expectedBrokenˢ);
        }
    }
    (~clientConfig).Certificates[0].SupportedSignatureAlgorithms = new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA1, PKCS1WithSHA256}.slice();
    // But if the certificate restricts supported algorithms, RSA-PSS should not
    // be selected, and the handshake should succeed.
    {
        var (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestVerifyCertificates(ж<testing.T> Ꮡt) {
    skipFIPS(Ꮡt); // Test certificates not FIPS compatible.
    // See https://go.dev/issue/31641.
    Ꮡt.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testVerifyCertificates(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
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

[GoType("dyn")] internal partial struct testVerifyCertificates_tests {
    internal @string name;
    public bool InsecureSkipVerify;
    public global::go.crypto.tls_package.ClientAuthType ClientAuth;
    public bool ClientCertificates;
}

internal static void testVerifyCertificates(ж<testing.T> Ꮡt, uint16 version) {
    var tests = new testVerifyCertificates_tests[]{
        new(
            name: "defaults"u8
        ),
        new(
            name: "InsecureSkipVerify"u8,
            InsecureSkipVerify: true
        ),
        new(
            name: "RequestClientCert with no certs"u8,
            ClientAuth: RequestClientCert
        ),
        new(
            name: "RequestClientCert with certs"u8,
            ClientAuth: RequestClientCert,
            ClientCertificates: true
        ),
        new(
            name: "RequireAnyClientCert"u8,
            ClientAuth: RequireAnyClientCert,
            ClientCertificates: true
        ),
        new(
            name: "VerifyClientCertIfGiven with no certs"u8,
            ClientAuth: VerifyClientCertIfGiven
        ),
        new(
            name: "VerifyClientCertIfGiven with certs"u8,
            ClientAuth: VerifyClientCertIfGiven,
            ClientCertificates: true
        ),
        new(
            name: "RequireAndVerifyClientCert"u8,
            ClientAuth: RequireAndVerifyClientCert,
            ClientCertificates: true
        )
    }.slice();
    var (issuer, err) = Δx509.ParseCertificate(testRSACertificateIssuer);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);
    foreach (var (_, test) in tests) {
        ref var testΔ1 = ref heap<testVerifyCertificates_tests>(out var ᏑtestΔ1);
        testΔ1 = test;
        var rootCAsʗ1 = rootCAs;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            bool serverVerifyConnection = default!;
            bool clientVerifyConnection = default!;
            bool serverVerifyPeerCertificates = default!;
            bool clientVerifyPeerCertificates = default!;
            var clientConfig = testConfig.Clone();
            clientConfig.Value.Time = testTime;
            clientConfig.Value.MaxVersion = version;
            clientConfig.Value.MinVersion = version;
            clientConfig.Value.RootCAs = rootCAsʗ1;
            clientConfig.Value.ServerName = exampleGolangˢ;
            clientConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
            var serverConfig = clientConfig.Clone();
            serverConfig.Value.ClientCAs = rootCAsʗ1;
            clientConfig.Value.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState csΔ1) => {
                clientVerifyConnection = true;
                return default!;
            };
            clientConfig.Value.VerifyPeerCertificate = error (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> verifiedChains) => {
                clientVerifyPeerCertificates = true;
                return default!;
            };
            serverConfig.Value.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState csΔ2) => {
                serverVerifyConnection = true;
                return default!;
            };
            serverConfig.Value.VerifyPeerCertificate = error (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> verifiedChains) => {
                serverVerifyPeerCertificates = true;
                return default!;
            };
            clientConfig.Value.InsecureSkipVerify = testʗ1.InsecureSkipVerify;
            serverConfig.Value.ClientAuth = testʗ1.ClientAuth;
            if (!testʗ1.ClientCertificates) {
                clientConfig.Value.Certificates = default!;
            }
            {
                var (_, _, errΔ1) = testHandshake(tΔ1, clientConfig, serverConfig); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            var want = (~serverConfig).ClientAuth != NoClientCert;
            if (serverVerifyPeerCertificates != want) {
                tΔ1.Errorf("VerifyPeerCertificates on the server: got %v, want %v"u8,
                    serverVerifyPeerCertificates, want);
            }
            if (!clientVerifyPeerCertificates) {
                tΔ1.Errorf("VerifyPeerCertificates not called on the client"u8);
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
            if (errΔ2 != default!) {
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
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string tlsmlkem0ˢ = "tlsmlkem=0"u8;
internal static readonly @string clientSupportsˢ = "client supports X25519MLKEM768"u8;
internal static readonly @string clientDoesNotSupportˢ2 = "client does not support X25519MLKEM768"u8;
internal static readonly object serverDidNotUseHrrˢ = (@string)"server did not use HRR"u8;
internal static readonly object clientDidNotUseHrrˢ = (@string)"client did not use HRR"u8;
internal static readonly object serverUsedHrrˢ = (@string)"server used HRR"u8;
internal static readonly object clientUsedHrrˢ = (@string)"client used HRR"u8;

[GoType("dyn")] internal partial struct TestHandshakeMLKEM_type {
    internal @string name;
    internal Action<ж<global::go.crypto.tls_package.Config>> clientConfig;
    internal Action<ж<global::go.crypto.tls_package.Config>> serverConfig;
    internal Action<ж<testing.T>> preparation;
    internal bool expectClientSupport;
    internal bool expectMLKEM;
    internal bool expectHRR;
}

public static void TestHandshakeMLKEM(ж<testing.T> Ꮡt) {
    skipFIPS(Ꮡt); // No X25519MLKEM768 in FIPS
    slice<TestHandshakeMLKEM_type> tests = new TestHandshakeMLKEM_type[]{
        new(
            name: "Default"u8,
            expectClientSupport: true,
            expectMLKEM: true,
            expectHRR: false
        ),
        new(
            name: "ClientCurvePreferences"u8,
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519}.slice();
            },
            expectClientSupport: false
        ),
        new(
            name: "ServerCurvePreferencesX25519"u8,
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519}.slice();
            },
            expectClientSupport: true,
            expectMLKEM: false,
            expectHRR: false
        ),
        new(
            name: "ServerCurvePreferencesHRR"u8,
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice();
            },
            expectClientSupport: true,
            expectMLKEM: false,
            expectHRR: true
        ),
        new(
            name: "ClientMLKEMOnly"u8,
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519MLKEM768}.slice();
            },
            expectClientSupport: true,
            expectMLKEM: true
        ),
        new(
            name: "ClientSortedCurvePreferences"u8,
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{CurveP256, X25519MLKEM768}.slice();
            },
            expectClientSupport: true,
            expectMLKEM: true
        ),
        new(
            name: "ClientTLSv12"u8,
            clientConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.MaxVersion = VersionTLS12;
            },
            expectClientSupport: false
        ),
        new(
            name: "ServerTLSv12"u8,
            serverConfig: (ж<global::go.crypto.tls_package.Config> config) => {
                config.Value.MaxVersion = VersionTLS12;
            },
            expectClientSupport: true,
            expectMLKEM: false
        ),
        new(
            name: "GODEBUG"u8,
            preparation: (ж<testing.T> tΔ1) => {
                tΔ1.Setenv(godebugˢ, tlsmlkem0ˢ);
            },
            expectClientSupport: false
        )
    }.slice();
    var baseConfig = testConfig.Clone();
    baseConfig.Value.CurvePreferences = default!;
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestHandshakeMLKEM_type(), out var Ꮡtest);
        test = vᴛ1;

        var baseConfigʗ1 = baseConfig;
        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ2) => {
            if (testʗ1.preparation != default!){
                testʗ1.preparation(tΔ2);
            } else {
                tΔ2.Parallel();
            }
            var serverConfig = baseConfigʗ1.Clone();
            if (testʗ1.serverConfig != default!) {
                testʗ1.serverConfig(serverConfig);
            }
            var testʗ2 = testʗ1;
            serverConfig.Value.GetConfigForClient = (ж<global::go.crypto.tls_package.Config>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> hello) => {
                if (!testʗ2.expectClientSupport && slices.Contains((~hello).SupportedCurves, X25519MLKEM768)){
                    return (default!, errors.New(clientSupportsˢ));
                } else 
                if (testʗ2.expectClientSupport && !slices.Contains((~hello).SupportedCurves, X25519MLKEM768)) {
                    return (default!, errors.New(clientDoesNotSupportˢ2));
                }
                return (default!, default!);
            };
            var clientConfig = baseConfigʗ1.Clone();
            if (testʗ1.clientConfig != default!) {
                testʗ1.clientConfig(clientConfig);
            }
            var (ss, cs, err) = testHandshake(tΔ2, clientConfig, serverConfig);
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            if (testʗ1.expectMLKEM){
                if (ss.testingOnlyCurveID != X25519MLKEM768) {
                    tΔ2.Errorf("got CurveID %v (server), expected %v"u8, ss.testingOnlyCurveID, X25519MLKEM768);
                }
                if (cs.testingOnlyCurveID != X25519MLKEM768) {
                    tΔ2.Errorf("got CurveID %v (client), expected %v"u8, cs.testingOnlyCurveID, X25519MLKEM768);
                }
            } else {
                if (ss.testingOnlyCurveID == X25519MLKEM768) {
                    tΔ2.Errorf("got CurveID %v (server), expected not X25519MLKEM768"u8, ss.testingOnlyCurveID);
                }
                if (cs.testingOnlyCurveID == X25519MLKEM768) {
                    tΔ2.Errorf("got CurveID %v (client), expected not X25519MLKEM768"u8, cs.testingOnlyCurveID);
                }
            }
            if (testʗ1.expectHRR){
                if (!ss.testingOnlyDidHRR) {
                    tΔ2.Error(serverDidNotUseHrrˢ);
                }
                if (!cs.testingOnlyDidHRR) {
                    tΔ2.Error(clientDidNotUseHrrˢ);
                }
            } else {
                if (ss.testingOnlyDidHRR) {
                    tΔ2.Error(serverUsedHrrˢ);
                }
                if (cs.testingOnlyDidHRR) {
                    tΔ2.Error(clientUsedHrrˢ);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509keypairleaf0ˢ = "x509keypairleaf=0"u8;
internal static readonly object leafShouldNotBePopulatedˢ = (@string)"Leaf should not be populated"u8;
internal static readonly @string x509keypairleaf1ˢ = "x509keypairleaf=1"u8;
internal static readonly object leafShouldBePopulatedˢ = (@string)"Leaf should be populated"u8;
internal static readonly @string godebugUnsetˢ = "GODEBUG unset"u8;

public static void TestX509KeyPairPopulateCertificate(ж<testing.T> Ꮡt) {
    var (key, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var keyDER, err) = Δx509.MarshalPKCS8PrivateKey(key.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var keyPEM = pem.EncodeToMemory(Ꮡ(new pem.Block(Type: "PRIVATE KEY"u8, Bytes: keyDER)));
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test"u8)
    ));
    (var certDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, key.Public(), key.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var certPEM = pem.EncodeToMemory(Ꮡ(new pem.Block(Type: "CERTIFICATE"u8, Bytes: certDER)));
    var certPEMʗ1 = certPEM;
    var keyPEMʗ1 = keyPEM;
    Ꮡt.Run(x509keypairleaf0ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Setenv(godebugˢ, x509keypairleaf0ˢ);
        var (cert, errΔ1) = X509KeyPair(certPEMʗ1, keyPEMʗ1);
        if (errΔ1 != default!) {
            tΔ1.Fatal(errΔ1);
        }
        if (cert.Leaf != nil) {
            tΔ1.Fatal(leafShouldNotBePopulatedˢ);
        }
    });
    var certPEMʗ2 = certPEM;
    var keyPEMʗ2 = keyPEM;
    Ꮡt.Run(x509keypairleaf1ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Setenv(godebugˢ, x509keypairleaf1ˢ);
        var (cert, errΔ2) = X509KeyPair(certPEMʗ2, keyPEMʗ2);
        if (errΔ2 != default!) {
            tΔ2.Fatal(errΔ2);
        }
        if (cert.Leaf == nil) {
            tΔ2.Fatal(leafShouldBePopulatedˢ);
        }
    });
    var certPEMʗ3 = certPEM;
    var keyPEMʗ3 = keyPEM;
    Ꮡt.Run(godebugUnsetˢ, (ж<testing.T> tΔ3) => {
        var (cert, errΔ3) = X509KeyPair(certPEMʗ3, keyPEMʗ3);
        if (errΔ3 != default!) {
            tΔ3.Fatal(errΔ3);
        }
        if (cert.Leaf == nil) {
            tΔ3.Fatal(leafShouldBePopulatedˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsHandshakeMessageOfˢ = "tls: handshake message of length 131071 bytes exceeds maximum of 65536 bytes"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestEarlyLargeCertMsg(ж<testing.T> Ꮡt) {
    var (client, server) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var clientʗ1 = client;
    goǃ(() => {
        {
            var (_, errΔ1) = clientʗ1.Write(new byte[]{(byte)recordTypeHandshake, 3, 4, 0, 4, typeCertificate, 1, 255, 255}.slice()); if (errΔ1 != default!) {
                Ꮡt.Log(errΔ1);
            }
        }
    });
    @string expectedErr = tlsHandshakeMessageOfˢ;
    var servConn = Server(server, testConfig);
    var err = servConn.Handshake();
    if (err == default!) {
        Ꮡt.Fatal(unexpectedSuccessˢ);
    }
    if (err.Error() != expectedErr) {
        Ꮡt.Fatalf("unexpected error: got %q, want %q"u8, err, expectedErr);
    }
}

public static void TestLargeCertMsg(ж<testing.T> Ꮡt) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test"u8),
        ExtraExtensions: new pkix.Extension[]{
            new(
                Id: new asn1.ObjectIdentifier(new nint[]{1, 2, 3}.slice()), // Ballast to inflate the certificate beyond the
 // regular handshake record size.

                Value: new slice<byte>(65536)
            )
        }.slice()
    ));
    (var cert, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    clientConfig.Value.InsecureSkipVerify = true;
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{
        new(
            ΔCertificate: new slice<byte>[]{cert}.slice(),
            PrivateKey: k.OrTypedNil()
        )
    }.slice();
    {
        var (_, _, errΔ1) = testHandshake(Ꮡt, clientConfig, serverConfig); if (errΔ1 != default!) {
            Ꮡt.Fatalf("unexpected failure: %s"u8, errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string secretExampleˢ = "secret.example"u8;
internal static readonly @string publicExampleˢ = "public.example"u8;
internal static readonly object serverConnectionStateˢ = (@string)"server ConnectionState shows ECH not accepted"u8;
internal static readonly object clientConnectionStateˢ = (@string)"client ConnectionState shows ECH not accepted"u8;
internal static readonly object unexpectNumberOfˢ = (@string)"unexpect number of certificate chains"u8;
internal static readonly object unexpectNumberOfˢ2 = (@string)"unexpect number of certificates"u8;
internal static readonly object unexpectedCertificateˢ = (@string)"unexpected certificate"u8;

public static void TestECH(ж<testing.T> Ꮡt) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        DNSNames: new @string[]{"public.example"u8}.slice(),
        NotBefore: time_package.Now().Add(-time_package.ΔHour),
        NotAfter: time_package.Now().Add(time_package.ΔHour)
    ));
    (var publicCertDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var publicCert, err) = Δx509.ParseCertificate(publicCertDER);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    tmpl.Value.DNSNames[0] = secretExampleˢ;
    (var secretCertDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var secretCert, err) = Δx509.ParseCertificate(secretCertDER);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    slice<byte> marshalECHConfig(uint8 id, slice<byte> pubKey, @string publicName, uint8 maxNameLen) {
        var builderΔ1 = cryptobyte.NewBuilder(default!);
        builderΔ1.AddUint16(extensionEncryptedClientHello);
        var pubKeyʗ1 = pubKey;
        builderΔ1.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ2) => {
            builderΔ2.AddUint8(id);
            builderΔ2.AddUint16(hpke.DHKEM_X25519_HKDF_SHA256); // The only DHKEM we support
            var pubKeyʗ2 = pubKeyʗ1;
            builderΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ3) => {
                builderΔ3.AddBytes(pubKeyʗ2);
            });
            builderΔ2.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ4) => {
                foreach (var (_, aeadID) in sortedSupportedAEADs) {
                    builderΔ4.AddUint16(hpke.KDF_HKDF_SHA256); // The only KDF we support
                    builderΔ4.AddUint16(aeadID);
                }
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
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var echConfig = marshalECHConfig(123, echKey.PublicKey().Bytes(), publicExampleˢ, 32);
    var builder = cryptobyte.NewBuilder(default!);
    var echConfigʗ1 = echConfig;
    builder.AddUint16LengthPrefixed((ж<cryptobyte.Builder> builderΔ6) => {
        builderΔ6.AddBytes(echConfigʗ1);
    });
    var echConfigList = builder.BytesOrPanic();
    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    clientConfig.Value.InsecureSkipVerify = false;
    clientConfig.Value.Rand = go.crypto.rand_package.Reader;
    clientConfig.Value.Time = default!;
    clientConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.ServerName = secretExampleˢ;
    clientConfig.Value.RootCAs = Δx509.NewCertPool();
    (~clientConfig).RootCAs.AddCert(secretCert);
    (~clientConfig).RootCAs.AddCert(publicCert);
    clientConfig.Value.EncryptedClientHelloConfigList = echConfigList;
    serverConfig.Value.InsecureSkipVerify = false;
    serverConfig.Value.Rand = go.crypto.rand_package.Reader;
    serverConfig.Value.Time = default!;
    serverConfig.Value.MinVersion = VersionTLS13;
    serverConfig.Value.ServerName = publicExampleˢ;
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{
        new(ΔCertificate: new slice<byte>[]{publicCertDER}.slice(), PrivateKey: k.OrTypedNil()),
        new(ΔCertificate: new slice<byte>[]{secretCertDER}.slice(), PrivateKey: k.OrTypedNil())
    }.slice();
    serverConfig.Value.EncryptedClientHelloKeys = new global::go.crypto.tls_package.EncryptedClientHelloKey[]{
        new(Config: echConfig, PrivateKey: echKey.Bytes(), SendAsRetry: true)
    }.slice();
    (var ss, var cs, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected failure: %s"u8, err);
    }
    if (!ss.ECHAccepted) {
        Ꮡt.Fatal(serverConnectionStateˢ);
    }
    if (!cs.ECHAccepted) {
        Ꮡt.Fatal(clientConnectionStateˢ);
    }
    if (cs.ServerName != "secret.example"u8 || ss.ServerName != "secret.example"u8) {
        Ꮡt.Fatalf("unexpected ConnectionState.ServerName, want %q, got server:%q, client: %q"u8, secretExampleˢ, ss.ServerName, cs.ServerName);
    }
    if (len(cs.VerifiedChains) != 1) {
        Ꮡt.Fatal(unexpectNumberOfˢ);
    }
    if (len(cs.VerifiedChains[0]) != 1) {
        Ꮡt.Fatal(unexpectNumberOfˢ2);
    }
    if (!cs.VerifiedChains[0][0].Equal(secretCert)) {
        Ꮡt.Fatal(unexpectedCertificateˢ);
    }
}

} // end tls_internal_test_package
