// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using io = io_package;
using net = net_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.crypto.tls_package;
using time = time_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object roundUpBrokenˢ = (@string)"roundUp broken"u8;

public static partial void TestRoundUp(ж<testing.T> t) {
    if (roundUp(0, 16) != 0 || roundUp(1, 16) != 16 || roundUp(15, 16) != 16 || roundUp(16, 16) != 16 || roundUp(17, 16) != 32) {
        t.Error(roundUpBrokenˢ);
    }
}

// will be initialized with {0, 255, 255, ..., 255}
internal static array<byte> padding255Bad = new byte[]{}.array(256);

// will be initialized with {255, 255, 255, ..., 255}
internal static array<byte> padding255Good = new byte[]{255}.array(256);

partial struct paddingTestsᴛ1 /*dyn*/ {
    internal slice<byte> @in;
    internal bool good;
    internal nint expectedLen;
}
internal static slice<paddingTestsᴛ1> paddingTests = slice<paddingTestsᴛ1>([
    new(slice<byte>([1, 2, 3, 4, 0]), true, 4),
    new(slice<byte>([1, 2, 3, 4, 0, 1]), false, 0),
    new(slice<byte>([1, 2, 3, 4, 99, 99]), false, 0),
    new(slice<byte>([1, 2, 3, 4, 1, 1]), true, 4),
    new(slice<byte>([1, 2, 3, 2, 2, 2]), true, 3),
    new(slice<byte>([1, 2, 3, 3, 3, 3]), true, 2),
    new(slice<byte>([1, 2, 3, 4, 3, 3]), false, 0),
    new(slice<byte>([1, 4, 4, 4, 4, 4]), true, 1),
    new(slice<byte>([5, 5, 5, 5, 5, 5]), true, 0),
    new(slice<byte>([6, 6, 6, 6, 6, 6]), false, 0),
    new(padding255Bad[..], false, 0),
    new(padding255Good[..], true, 0)
]);

public static void TestRemovePadding(ж<testing.T> t) {
    for (nint i = 1; i < len(padding255Bad); i++) {
        padding255Bad[i] = 255;
        padding255Good[i] = 255;
    }
    foreach (var (i, test) in paddingTests) {
        var (paddingLen, good) = extractPadding(test.@in);
        var expectedGood = (byte)255;
        if (!test.good) {
            expectedGood = 0;
        }
        if (good != expectedGood) {
            t.Errorf("#%d: wrong validity, want:%d got:%d", i, expectedGood, good);
        }
        if (good == 255 && len(test.@in) - paddingLen != test.expectedLen) {
            t.Errorf("#%d: got %d, want %d", i, len(test.@in) - paddingLen, test.expectedLen);
        }
    }
}

internal static @string certExampleCom = @"308201713082011ba003020102021005a75ddf21014d5f417083b7a010ba2e300d06092a864886f70d01010b050030123110300e060355040a130741636d6520436f301e170d3136303831373231343135335a170d3137303831373231343135335a30123110300e060355040a130741636d6520436f305c300d06092a864886f70d0101010500034b003048024100b37f0fdd67e715bf532046ac34acbd8fdc4dabe2b598588f3f58b1f12e6219a16cbfe54d2b4b665396013589262360b6721efa27d546854f17cc9aeec6751db10203010001a34d304b300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff0402300030160603551d11040f300d820b6578616d706c652e636f6d300d06092a864886f70d01010b050003410059fc487866d3d855503c8e064ca32aac5e9babcece89ec597f8b2b24c17867f4a5d3b4ece06e795bfc5448ccbd2ffca1b3433171ebf3557a4737b020565350a0";

internal static @string certWildcardExampleCom = @"308201743082011ea003020102021100a7aa6297c9416a4633af8bec2958c607300d06092a864886f70d01010b050030123110300e060355040a130741636d6520436f301e170d3136303831373231343231395a170d3137303831373231343231395a30123110300e060355040a130741636d6520436f305c300d06092a864886f70d0101010500034b003048024100b105afc859a711ee864114e7d2d46c2dcbe392d3506249f6c2285b0eb342cc4bf2d803677c61c0abde443f084745c1a6d62080e5664ef2cc8f50ad8a0ab8870b0203010001a34f304d300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff0402300030180603551d110411300f820d2a2e6578616d706c652e636f6d300d06092a864886f70d01010b0500034100af26088584d266e3f6566360cf862c7fecc441484b098b107439543144a2b93f20781988281e108c6d7656934e56950e1e5f2bcf38796b814ccb729445856c34";

internal static @string certFooExampleCom = @"308201753082011fa00302010202101bbdb6070b0aeffc49008cde74deef29300d06092a864886f70d01010b050030123110300e060355040a130741636d6520436f301e170d3136303831373231343234345a170d3137303831373231343234345a30123110300e060355040a130741636d6520436f305c300d06092a864886f70d0101010500034b003048024100f00ac69d8ca2829f26216c7b50f1d4bbabad58d447706476cd89a2f3e1859943748aa42c15eedc93ac7c49e40d3b05ed645cb6b81c4efba60d961f44211a54eb0203010001a351304f300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff04023000301a0603551d1104133011820f666f6f2e6578616d706c652e636f6d300d06092a864886f70d01010b0500034100a0957fca6d1e0f1ef4b247348c7a8ca092c29c9c0ecc1898ea6b8065d23af6d922a410dd2335a0ea15edd1394cef9f62c9e876a21e35250a0b4fe1ddceba0f36";

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleComˢ => ref exampleComˢᵇ;
private static readonly @string exampleComˢᵇ = "example.com"u8;
internal static ref readonly @string barExampleComˢ => ref barExampleComˢᵇ;
private static readonly @string barExampleComˢᵇ = "bar.example.com"u8;
internal static ref readonly @string fooExampleComˢ => ref fooExampleComˢᵇ;
private static readonly @string fooExampleComˢᵇ = "foo.example.com"u8;
internal static ref readonly @string fooBarExampleComˢ => ref fooBarExampleComˢᵇ;
private static readonly @string fooBarExampleComˢᵇ = "foo.bar.example.com"u8;

public static void TestCertificateSelection(ж<testing.T> t) {
    ref var config = ref heap<global::go.crypto.tls_package.Config>(new Config(
        Certificates: slice<global::go.crypto.tls_package.Certificate>([
            new(
                ΔCertificate: slice<slice<byte>>([fromHex(certExampleCom)])
            ),
            new(
                ΔCertificate: slice<slice<byte>>([fromHex(certWildcardExampleCom)])
            ),
            new(
                ΔCertificate: slice<slice<byte>>([fromHex(certFooExampleCom)])
            )
        ])
    ), out var Ꮡconfig);

    config.BuildNameToCertificate();

    nint pointerToIndex(ж<global::go.crypto.tls_package.Certificate> c) {
        foreach (var i in range(Ꮡconfig.Value.Certificates)) {
            if (c == Ꮡ(Ꮡconfig.Value.Certificates, i)) {
                return i;
            }
        }
        return -1;
    }

    ж<global::go.crypto.tls_package.Certificate> certificateForName(@string name) {
        var clientHello = Ꮡ(new ClientHelloInfo(
            ServerName: name
        ));
        {
            var (cert, err) = Ꮡconfig.Value.getCertificate(clientHello); if (err != nil) {
                t.Errorf("unable to get certificate for name '%s': %s", name, err);
                return default;
            } else {
                return cert;
            }
        }
    }

    {
        nint n = pointerToIndex(certificateForName(exampleComˢ)); if (n != 0) {
            t.Errorf("example.com returned certificate %d, not 0", n);
        }
    }
    {
        nint n = pointerToIndex(certificateForName(barExampleComˢ)); if (n != 1) {
            t.Errorf("bar.example.com returned certificate %d, not 1", n);
        }
    }
    {
        nint n = pointerToIndex(certificateForName(fooExampleComˢ)); if (n != 2) {
            t.Errorf("foo.example.com returned certificate %d, not 2", n);
        }
    }
    {
        nint n = pointerToIndex(certificateForName(fooBarExampleComˢ)); if (n != 0) {
            t.Errorf("foo.bar.example.com returned certificate %d, not 0", n);
        }
    }
}

// Run with multiple crypto configs to test the logic for computing TLS record overheads.
internal static partial void runDynamicRecordSizingTest(ж<testing.T> t, ж<global::go.crypto.tls_package.Config> config) {
    GoFrame ᒐ = default;
    try {
        var (clientConn, serverConn) = localPipe(new tls_test_package.testing_TжTB(t));

        var serverConfig = config.Clone();
        serverConfig.DynamicRecordSizingDisabled = false;
        var tlsConn = Server(serverConn, serverConfig);

        var handshakeDone = new channel<EmptyStruct>(0);
        var recordSizesChan = new channel<slice<nint>>(1);
        var recordSizesChanʗ1 = recordSizesChan;
        defer(() => {
            ᐸꟷ(recordSizesChanʗ1);
        }, ref ᒐ); // wait for the goroutine to exit
        var clientConnʗ1 = clientConn;
        var handshakeDoneʗ1 = handshakeDone;
        var recordSizesChanʗ2 = recordSizesChan;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                // This goroutine performs a TLS handshake over clientConn and
                // then reads TLS records until EOF. It writes a slice that
                // contains all the record sizes to recordSizesChan.
                defer(ᴛ1 => close(ᴛ1), recordSizesChanʗ2, ref ᒐ);
                var clientConnʗ2 = clientConnʗ1;
                defer(() => clientConnʗ2.Close(), ref ᒐ);

                var tlsConnΔ1 = Client(clientConnʗ1, config);
                {
                    var err = tlsConnΔ1.Handshake(); if (err != nil) {
                        t.Errorf("Error from client handshake: %v", err);
                        return;
                    }
                }
                close(handshakeDoneʗ1);

                ref var recordHeader = ref heap(new array<byte>(5), out var ᏑrecordHeader);
                slice<byte> record = default;
                slice<nint> recordSizesΔ1 = default;

                for (;;) {
                    var (n, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(clientConnʗ1), recordHeader[..]);
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    if (err != nil || n != len(recordHeader)) {
                        t.Errorf("io.ReadFull = %d, %v", n, err);
                        return;
                    }

                    nint length = (nint)recordHeader[3] << 8 | (nint)recordHeader[4];
                    if (len(record) < length) {
                        record = new slice<byte>(length);
                    }

                    (n, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(clientConnʗ1), record.slice(0, length));
                    if (err != nil || n != length) {
                        t.Errorf("io.ReadFull = %d, %v", n, err);
                        return;
                    }

                    recordSizesΔ1 = append(recordSizesΔ1, (nint)recordHeaderLen + length);
                }

                recordSizesChanʗ2.ᐸꟷ(recordSizesΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        {
            var err = tlsConn.Handshake(); if (err != nil) {
                t.Fatalf("Error from server handshake: %s", err);
            }
        }
        ᐸꟷ(handshakeDone);

        // The server writes these plaintexts in order.
        var plaintext = bytes.Join(slice<slice<byte>>([
            bytes.Repeat(slice<byte>("x"u8), recordSizeBoostThreshold),
            bytes.Repeat(slice<byte>("y"u8), maxPlaintext * 2),
            bytes.Repeat(slice<byte>("z"u8), maxPlaintext)
        ]), default);

        {
            var (_, err) = tlsConn.Write(plaintext); if (err != nil) {
                t.Fatalf("Error from server write: %s", err);
            }
        }
        {
            var err = tlsConn.Close(); if (err != nil) {
                t.Fatalf("Error from server close: %s", err);
            }
        }

        var recordSizes = ᐸꟷ(recordSizesChan);
        if (recordSizes == nil) {
            t.Fatalf("Client encountered an error");
        }

        // Drop the size of the second to last record, which is likely to be
        // truncated, and the last record, which is a close_notify alert.
        recordSizes = recordSizes.slice(0, len(recordSizes) - 2);

        // recordSizes should contain a series of records smaller than
        // tcpMSSEstimate followed by some larger than maxPlaintext.
        var seenLargeRecord = false;
        foreach (var (i, size) in recordSizes) {
            if (!seenLargeRecord) {
                if (size > (i + 1) * (nint)tcpMSSEstimate) {
                    t.Fatalf("Record #%d has size %d, which is too large too soon", i, size);
                }
                if (size >= maxPlaintext) {
                    seenLargeRecord = true;
                }
            } else if (size <= maxPlaintext) {
                t.Fatalf("Record #%d has size %d but should be full sized", i, size);
            }
        }

        if (!seenLargeRecord) {
            t.Fatalf("No large records observed");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDynamicRecordSizingWithStreamCipher(ж<testing.T> t) {
    skipFIPS(t); // No RC4 in FIPS mode.

    var config = testConfig.Clone();
    config.MaxVersion = VersionTLS12;
    config.CipherSuites = slice<uint16>([TLS_RSA_WITH_RC4_128_SHA]);
    runDynamicRecordSizingTest(t, config);
}

public static void TestDynamicRecordSizingWithCBC(ж<testing.T> t) {
    skipFIPS(t); // No CBC cipher suites in defaultCipherSuitesFIPS.

    var config = testConfig.Clone();
    config.MaxVersion = VersionTLS12;
    config.CipherSuites = slice<uint16>([TLS_RSA_WITH_AES_256_CBC_SHA]);
    runDynamicRecordSizingTest(t, config);
}

public static void TestDynamicRecordSizingWithAEAD(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.MaxVersion = VersionTLS12;
    config.CipherSuites = slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256]);
    runDynamicRecordSizingTest(t, config);
}

public static void TestDynamicRecordSizingWithTLSv13(ж<testing.T> t) {
    var config = testConfig.Clone();
    runDynamicRecordSizingTest(t, config);
}

// hairpinConn is a net.Conn that makes a “hairpin” call when closed, back into
// the tls.Conn which is calling it.
internal partial struct hairpinConn {
    /*embed*/ public net_package.Conn Conn;
    internal ж<global::go.crypto.tls_package.Conn> tlsConn;
}

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this hairpinConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this hairpinConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this hairpinConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this hairpinConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this hairpinConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this hairpinConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

// Go method set entry for the promoted 'Conn.Write()' - provided ONLY by the embedded
// interface field in *hairpinConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Write(this hairpinConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Write(b);

internal static error Close(this ref hairpinConn conn) {
    conn.tlsConn.ConnectionState();
    return default;
}

public static void TestHairpinInClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // This tests that the underlying net.Conn can call back into the
        // tls.Conn when being closed without deadlocking.
        var (client, server) = localPipe(new tls_test_package.testing_TжTB(t));
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var conn = Ꮡ(new hairpinConn(client, nil));
        var tlsConn = Server(new tls_internal_test_package.hairpinConnжConn(conn), Ꮡ(new Config(
            GetCertificate: (ж<global::go.crypto.tls_package.ClientHelloInfo> _) => {
                throw panic("unreachable");
            }
        )));
        conn.tlsConn = tlsConn;

        // This call should not deadlock.
        tlsConn.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlsReceivedRecordWithˢ => ref tlsReceivedRecordWithˢᵇ;
private static readonly @string tlsReceivedRecordWithˢᵇ = "tls: received record with version 1111 when expecting version 303"u8;

public static partial void TestRecordBadVersionTLS13(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (client, server) = localPipe(new tls_test_package.testing_TжTB(t));
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var config = testConfig.Clone();
        (config.MinVersion, config.MaxVersion) = (VersionTLS13, VersionTLS13);

        var clientʗ2 = client;
        var configʗ1 = config;
        goǃ(() => {
            var tlsConnΔ1 = Client(clientʗ2, configʗ1);
            {
                var errΔ1 = tlsConnΔ1.Handshake(); if (errΔ1 != nil) {
                    t.Errorf("Error from client handshake: %v", errΔ1);
                    return;
                }
            }
            tlsConnΔ1.vers = 0x1111;
            tlsConnΔ1.Write(slice<byte>([1]));
        });

        var tlsConn = Server(server, config);
        {
            var errΔ2 = tlsConn.Handshake(); if (errΔ2 != nil) {
                t.Errorf("Error from client handshake: %v", errΔ2);
                return;
            }
        }

        @string expectedErr = tlsReceivedRecordWithˢ;

        var (_, err) = tlsConn.Read(new slice<byte>(10));
        if (err.Error() != expectedErr) {
            t.Fatalf("unexpected error: got %q, want %q", err, expectedErr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestKeyUpdateSpamPostHandshakeTLS13 tests that TLS 1.3 KeyUpdate messages do
// not count as a state-advancing message after a handshake has been completed.
public static partial void TestKeyUpdateSpamPostHandshakeTLS13(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (client, server) = localPipe(new tls_test_package.testing_TжTB(t));
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var clientʗ2 = client;
        goǃ(() => {
            var c = Client(clientʗ2, testConfig.Clone());
            {
                var errΔ1 = c.Handshake(); if (errΔ1 != nil) {
                    t.Error(errΔ1);
                    return;
                }
            }
            var (ku, err) = (Ꮡ(new keyUpdateMsg())).marshal();
            if (err != nil) {
                t.Error(err);
                return;
            }
            var cs = cipherSuiteTLS13ByID(c.cipherSuite);
            for (nint i = 0; i <= maxUselessRecords; i++) {
                c.writeRecordLocked(recordTypeHandshake, ku);
                c.setWriteTrafficSecret(cs, QUICEncryptionLevelInitial, cs.nextTrafficSecret(c.@out.trafficSecret));
            }
        });
        var s = Server(server, testConfig.Clone());
        {
            var err = s.Handshake(); if (err != nil) {
                t.Fatal(err);
            }
        }
        @string expectedErr = tlsTooManyNonAdvancingˢ;
        {
            var (_, err) = s.Read(new slice<byte>(1)); if (err == nil || err.Error() != expectedErr) {
                t.Fatalf("unexpected error: got %v, want %q", err, expectedErr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end tls_internal_test_package
