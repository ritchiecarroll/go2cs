// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using io = io_package;
using net = net_package;
using testing = testing_package;
using static go.crypto.tls_package;
using time = time_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object roundUpBrokenˢ = (@string)"roundUp broken"u8;

public static void TestRoundUp(ж<testing.T> Ꮡt) {
    if (roundUp(0, 16) != 0 || roundUp(1, 16) != 16 || roundUp(15, 16) != 16 || roundUp(16, 16) != 16 || roundUp(17, 16) != 32) {
        Ꮡt.Error(roundUpBrokenˢ);
    }
}

// will be initialized with {0, 255, 255, ..., 255}
internal static array<byte> padding255Bad = new byte[]{}.array(256);

// will be initialized with {255, 255, 255, ..., 255}
internal static array<byte> padding255Good = new byte[]{255}.array(256);


[GoType("dyn")] partial struct paddingTestsᴛ1 {
    internal slice<byte> @in;
    internal bool good;
    internal nint expectedLen;
}
internal static slice<paddingTestsᴛ1> paddingTests = new paddingTestsᴛ1[]{
    new(new byte[]{1, 2, 3, 4, 0}.slice(), true, 4),
    new(new byte[]{1, 2, 3, 4, 0, 1}.slice(), false, 0),
    new(new byte[]{1, 2, 3, 4, 99, 99}.slice(), false, 0),
    new(new byte[]{1, 2, 3, 4, 1, 1}.slice(), true, 4),
    new(new byte[]{1, 2, 3, 2, 2, 2}.slice(), true, 3),
    new(new byte[]{1, 2, 3, 3, 3, 3}.slice(), true, 2),
    new(new byte[]{1, 2, 3, 4, 3, 3}.slice(), false, 0),
    new(new byte[]{1, 4, 4, 4, 4, 4}.slice(), true, 1),
    new(new byte[]{5, 5, 5, 5, 5, 5}.slice(), true, 0),
    new(new byte[]{6, 6, 6, 6, 6, 6}.slice(), false, 0),
    new(padding255Bad[..], false, 0),
    new(padding255Good[..], true, 0)
}.slice();

public static void TestRemovePadding(ж<testing.T> Ꮡt) {
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
            Ꮡt.Errorf("#%d: wrong validity, want:%d got:%d"u8, i, expectedGood, good);
        }
        if (good == 255 && len(test.@in) - paddingLen != test.expectedLen) {
            Ꮡt.Errorf("#%d: got %d, want %d"u8, i, len(test.@in) - paddingLen, test.expectedLen);
        }
    }
}

internal static @string certExampleCom = @"308201713082011ba003020102021005a75ddf21014d5f417083b7a010ba2e300d06092a864886f70d01010b050030123110300e060355040a130741636d6520436f301e170d3136303831373231343135335a170d3137303831373231343135335a30123110300e060355040a130741636d6520436f305c300d06092a864886f70d0101010500034b003048024100b37f0fdd67e715bf532046ac34acbd8fdc4dabe2b598588f3f58b1f12e6219a16cbfe54d2b4b665396013589262360b6721efa27d546854f17cc9aeec6751db10203010001a34d304b300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff0402300030160603551d11040f300d820b6578616d706c652e636f6d300d06092a864886f70d01010b050003410059fc487866d3d855503c8e064ca32aac5e9babcece89ec597f8b2b24c17867f4a5d3b4ece06e795bfc5448ccbd2ffca1b3433171ebf3557a4737b020565350a0"u8;

internal static @string certWildcardExampleCom = @"308201743082011ea003020102021100a7aa6297c9416a4633af8bec2958c607300d06092a864886f70d01010b050030123110300e060355040a130741636d6520436f301e170d3136303831373231343231395a170d3137303831373231343231395a30123110300e060355040a130741636d6520436f305c300d06092a864886f70d0101010500034b003048024100b105afc859a711ee864114e7d2d46c2dcbe392d3506249f6c2285b0eb342cc4bf2d803677c61c0abde443f084745c1a6d62080e5664ef2cc8f50ad8a0ab8870b0203010001a34f304d300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff0402300030180603551d110411300f820d2a2e6578616d706c652e636f6d300d06092a864886f70d01010b0500034100af26088584d266e3f6566360cf862c7fecc441484b098b107439543144a2b93f20781988281e108c6d7656934e56950e1e5f2bcf38796b814ccb729445856c34"u8;

internal static @string certFooExampleCom = @"308201753082011fa00302010202101bbdb6070b0aeffc49008cde74deef29300d06092a864886f70d01010b050030123110300e060355040a130741636d6520436f301e170d3136303831373231343234345a170d3137303831373231343234345a30123110300e060355040a130741636d6520436f305c300d06092a864886f70d0101010500034b003048024100f00ac69d8ca2829f26216c7b50f1d4bbabad58d447706476cd89a2f3e1859943748aa42c15eedc93ac7c49e40d3b05ed645cb6b81c4efba60d961f44211a54eb0203010001a351304f300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff04023000301a0603551d1104133011820f666f6f2e6578616d706c652e636f6d300d06092a864886f70d01010b0500034100a0957fca6d1e0f1ef4b247348c7a8ca092c29c9c0ecc1898ea6b8065d23af6d922a410dd2335a0ea15edd1394cef9f62c9e876a21e35250a0b4fe1ddceba0f36"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleComˢ = "example.com"u8;
internal static readonly @string barExampleComˢ = "bar.example.com"u8;
internal static readonly @string fooExampleComˢ = "foo.example.com"u8;
internal static readonly @string fooBarExampleComˢ = "foo.bar.example.com"u8;

public static void TestCertificateSelection(ж<testing.T> Ꮡt) {
    ref var config = ref heap<global::go.crypto.tls_package.Config>(out var Ꮡconfig);
    config = new Config(
        Certificates: new global::go.crypto.tls_package.Certificate[]{
            new(
                ΔCertificate: new slice<byte>[]{fromHex(certExampleCom)}.slice()
            ),
            new(
                ΔCertificate: new slice<byte>[]{fromHex(certWildcardExampleCom)}.slice()
            ),
            new(
                ΔCertificate: new slice<byte>[]{fromHex(certFooExampleCom)}.slice()
            )
        }.slice()
    );
    config.BuildNameToCertificate();
    nint pointerToIndex(ж<global::go.crypto.tls_package.Certificate> c) {
        foreach (var (i, _) in Ꮡconfig.Value.Certificates) {
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
            var (cert, err) = Ꮡconfig.Value.getCertificate(clientHello); if (err != default!){
                Ꮡt.Errorf("unable to get certificate for name '%s': %s"u8, name, err);
                return default!;
            } else {
                return cert;
            }
        }
    }
    {
        nint n = pointerToIndex(certificateForName(exampleComˢ)); if (n != 0) {
            Ꮡt.Errorf("example.com returned certificate %d, not 0"u8, n);
        }
    }
    {
        nint n = pointerToIndex(certificateForName(barExampleComˢ)); if (n != 1) {
            Ꮡt.Errorf("bar.example.com returned certificate %d, not 1"u8, n);
        }
    }
    {
        nint n = pointerToIndex(certificateForName(fooExampleComˢ)); if (n != 2) {
            Ꮡt.Errorf("foo.example.com returned certificate %d, not 2"u8, n);
        }
    }
    {
        nint n = pointerToIndex(certificateForName(fooBarExampleComˢ)); if (n != 0) {
            Ꮡt.Errorf("foo.bar.example.com returned certificate %d, not 0"u8, n);
        }
    }
}

// Run with multiple crypto configs to test the logic for computing TLS record overheads.
internal static void runDynamicRecordSizingTest(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.Config> Ꮡconfig) {
    GoFrame ᒐ = default;
    try {
        var (clientConn, serverConn) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var serverConfig = Ꮡconfig.Clone();
        serverConfig.Value.DynamicRecordSizingDisabled = false;
        var tlsConn = Server(serverConn, serverConfig);
        var handshakeDone = new channel<EmptyStruct>(0);
        var recordSizesChan = new channel<slice<nint>>(1);
        var recordSizesChanʗ1 = recordSizesChan;
        defer(() => {
            ᐸꟷ(recordSizesChanʗ1); // wait for the goroutine to exit
        }, ref ᒐ);
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
                var tlsConnΔ1 = Client(clientConnʗ1, Ꮡconfig);
                {
                    var err = tlsConnΔ1.Handshake(); if (err != default!) {
                        Ꮡt.Errorf("Error from client handshake: %v"u8, err);
                        return;
                    }
                }
                close(handshakeDoneʗ1);
                ref var recordHeader = ref heap(new array<byte>(5), out var ᏑrecordHeader);
                slice<byte> record = default!;
                slice<nint> recordSizesΔ1 = default!;
                while (ᐧ) {
                    var (n, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(clientConnʗ1), recordHeader[..]);
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    if (err != default! || n != len(recordHeader)) {
                        Ꮡt.Errorf("io.ReadFull = %d, %v"u8, n, err);
                        return;
                    }
                    nint length = (nint)(((nint)recordHeader[3] << (int)(8)) | (nint)recordHeader[4]);
                    if (len(record) < length) {
                        record = new slice<byte>(length);
                    }
                    (n, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(clientConnʗ1), record[..(int)(length)]);
                    if (err != default! || n != length) {
                        Ꮡt.Errorf("io.ReadFull = %d, %v"u8, n, err);
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
            var err = tlsConn.Handshake(); if (err != default!) {
                Ꮡt.Fatalf("Error from server handshake: %s"u8, err);
            }
        }
        ᐸꟷ(handshakeDone);
        // The server writes these plaintexts in order.
        var plaintext = bytes.Join(new slice<byte>[]{
            bytes.Repeat(slice<byte>("x"u8), recordSizeBoostThreshold),
            bytes.Repeat(slice<byte>("y"u8), maxPlaintext * 2),
            bytes.Repeat(slice<byte>("z"u8), maxPlaintext)
        }.slice(), default!);
        {
            var (_, err) = tlsConn.Write(plaintext); if (err != default!) {
                Ꮡt.Fatalf("Error from server write: %s"u8, err);
            }
        }
        {
            var err = tlsConn.Close(); if (err != default!) {
                Ꮡt.Fatalf("Error from server close: %s"u8, err);
            }
        }
        var recordSizes = ᐸꟷ(recordSizesChan);
        if (recordSizes == default!) {
            Ꮡt.Fatalf("Client encountered an error"u8);
        }
        // Drop the size of the second to last record, which is likely to be
        // truncated, and the last record, which is a close_notify alert.
        recordSizes = recordSizes[..(int)(len(recordSizes) - 2)];
        // recordSizes should contain a series of records smaller than
        // tcpMSSEstimate followed by some larger than maxPlaintext.
        var seenLargeRecord = false;
        foreach (var (i, size) in recordSizes) {
            if (!seenLargeRecord){
                if (size > (i + 1) * (nint)tcpMSSEstimate) {
                    Ꮡt.Fatalf("Record #%d has size %d, which is too large too soon"u8, i, size);
                }
                if (size >= maxPlaintext) {
                    seenLargeRecord = true;
                }
            } else 
            if (size <= maxPlaintext) {
                Ꮡt.Fatalf("Record #%d has size %d but should be full sized"u8, i, size);
            }
        }
        if (!seenLargeRecord) {
            Ꮡt.Fatalf("No large records observed"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDynamicRecordSizingWithStreamCipher(ж<testing.T> Ꮡt) {
    skipFIPS(Ꮡt); // No RC4 in FIPS mode.
    var config = testConfig.Clone();
    config.Value.MaxVersion = VersionTLS12;
    config.Value.CipherSuites = new uint16[]{TLS_RSA_WITH_RC4_128_SHA}.slice();
    runDynamicRecordSizingTest(Ꮡt, config);
}

public static void TestDynamicRecordSizingWithCBC(ж<testing.T> Ꮡt) {
    skipFIPS(Ꮡt); // No CBC cipher suites in defaultCipherSuitesFIPS.
    var config = testConfig.Clone();
    config.Value.MaxVersion = VersionTLS12;
    config.Value.CipherSuites = new uint16[]{TLS_RSA_WITH_AES_256_CBC_SHA}.slice();
    runDynamicRecordSizingTest(Ꮡt, config);
}

public static void TestDynamicRecordSizingWithAEAD(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.MaxVersion = VersionTLS12;
    config.Value.CipherSuites = new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice();
    runDynamicRecordSizingTest(Ꮡt, config);
}

public static void TestDynamicRecordSizingWithTLSv13(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    runDynamicRecordSizingTest(Ꮡt, config);
}

// hairpinConn is a net.Conn that makes a “hairpin” call when closed, back into
// the tls.Conn which is calling it.
[GoType] internal partial struct hairpinConn {
    [GoEmbedded] public net_package.Conn Conn;
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

[GoRecv] internal static error Close(this ref hairpinConn conn) {
    conn.tlsConn.ConnectionState();
    return default!;
}

public static void TestHairpinInClose(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // This tests that the underlying net.Conn can call back into the
        // tls.Conn when being closed without deadlocking.
        var (client, server) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
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
        conn.Value.tlsConn = tlsConn;
        // This call should not deadlock.
        tlsConn.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsReceivedRecordWithˢ = "tls: received record with version 1111 when expecting version 303"u8;

public static void TestRecordBadVersionTLS13(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (client, server) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        var config = testConfig.Clone();
        (config.Value.MinVersion, config.Value.MaxVersion) = (VersionTLS13, VersionTLS13);
        var clientʗ2 = client;
        var configʗ1 = config;
        goǃ(() => {
            var tlsConnΔ1 = Client(clientʗ2, configʗ1);
            {
                var errΔ1 = tlsConnΔ1.Handshake(); if (errΔ1 != default!) {
                    Ꮡt.Errorf("Error from client handshake: %v"u8, errΔ1);
                    return;
                }
            }
            tlsConnΔ1.Value.vers = 0x1111;
            tlsConnΔ1.Write(new byte[]{1}.slice());
        });
        var tlsConn = Server(server, config);
        {
            var errΔ2 = tlsConn.Handshake(); if (errΔ2 != default!) {
                Ꮡt.Errorf("Error from client handshake: %v"u8, errΔ2);
                return;
            }
        }
        @string expectedErr = tlsReceivedRecordWithˢ;
        var (_, err) = tlsConn.Read(new slice<byte>(10));
        if (err.Error() != expectedErr) {
            Ꮡt.Fatalf("unexpected error: got %q, want %q"u8, err, expectedErr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end tls_internal_test_package
