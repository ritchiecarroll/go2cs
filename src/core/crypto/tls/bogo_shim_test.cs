// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using Δx509 = go.crypto.x509_package;
using base64 = encoding.base64_package;
using json = encoding.json_package;
using pem = encoding.pem_package;
using flag = flag_package;
using fmt = fmt_package;
using byteorder = go.@internal.byteorder_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using log = log_package;
using net = net_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using encoding;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.os;
using path;
using static go.crypto.tls_package;
using time = time_package;
using vendor.golang.org.x.crypto;

partial class tls_internal_test_package {

internal static ж<@string> port = flag.String("port"u8, ""u8, ""u8);
internal static ж<bool> server = flag.Bool("server"u8, false, ""u8);
internal static ж<bool> isHandshakerSupported = flag.Bool("is-handshaker-supported"u8, false, ""u8);
internal static ж<@string> keyfile = flag.String("key-file"u8, ""u8, ""u8);
internal static ж<@string> certfile = flag.String("cert-file"u8, ""u8, ""u8);
internal static ж<@string> trustCert = flag.String("trust-cert"u8, ""u8, ""u8);
internal static ж<nint> minVersion = flag.Int("min-version"u8, VersionSSL30, ""u8);
internal static ж<nint> maxVersion = flag.Int("max-version"u8, VersionTLS13, ""u8);
internal static ж<nint> expectVersion = flag.Int("expect-version"u8, 0, ""u8);
internal static ж<bool> noTLS1 = flag.Bool("no-tls1"u8, false, ""u8);
internal static ж<bool> noTLS11 = flag.Bool("no-tls11"u8, false, ""u8);
internal static ж<bool> noTLS12 = flag.Bool("no-tls12"u8, false, ""u8);
internal static ж<bool> noTLS13 = flag.Bool("no-tls13"u8, false, ""u8);
internal static ж<bool> requireAnyClientCertificate = flag.Bool("require-any-client-certificate"u8, false, ""u8);
internal static ж<bool> shimWritesFirst = flag.Bool("shim-writes-first"u8, false, ""u8);
internal static ж<nint> resumeCount = flag.Int("resume-count"u8, 0, ""u8);
internal static ж<stringSlice> curves = flagStringSlice("curves"u8, ""u8);
internal static ж<@string> expectedCurve = flag.String("expect-curve-id"u8, ""u8, ""u8);
internal static ж<uint64> shimID = flag.Uint64("shim-id"u8, 0, ""u8);
internal static ж<bool> _ᴛ1ʗ = flag.Bool("ipv6"u8, false, ""u8);
internal static ж<@string> echConfigListB64 = flag.String("ech-config-list"u8, ""u8, ""u8);
internal static ж<bool> expectECHAccepted = flag.Bool("expect-ech-accept"u8, false, ""u8);
internal static ж<bool> expectHRR = flag.Bool("expect-hrr"u8, false, ""u8);
internal static ж<bool> expectNoHRR = flag.Bool("expect-no-hrr"u8, false, ""u8);
internal static ж<@string> expectedECHRetryConfigs = flag.String("expect-ech-retry-configs"u8, ""u8, ""u8);
internal static ж<bool> expectNoECHRetryConfigs = flag.Bool("expect-no-ech-retry-configs"u8, false, ""u8);
internal static ж<bool> onInitialExpectECHAccepted = flag.Bool("on-initial-expect-ech-accept"u8, false, ""u8);
internal static ж<bool> _ᴛ2ʗ = flag.Bool("expect-no-ech-name-override"u8, false, ""u8);
internal static ж<@string> _ᴛ3ʗ = flag.String("expect-ech-name-override"u8, ""u8, ""u8);
internal static ж<bool> _ᴛ4ʗ = flag.Bool("reverify-on-resume"u8, false, ""u8);
internal static ж<@string> onResumeECHConfigListB64 = flag.String("on-resume-ech-config-list"u8, ""u8, ""u8);
internal static ж<bool> _ᴛ5ʗ = flag.Bool("on-resume-expect-reject-early-data"u8, false, ""u8);
internal static ж<bool> onResumeExpectECHAccepted = flag.Bool("on-resume-expect-ech-accept"u8, false, ""u8);
internal static ж<bool> _ᴛ6ʗ = flag.Bool("on-resume-expect-no-ech-name-override"u8, false, ""u8);
internal static ж<@string> expectedServerName = flag.String("expect-server-name"u8, ""u8, ""u8);
internal static ж<stringSlice> echServerConfig = flagStringSlice("ech-server-config"u8, ""u8);
internal static ж<stringSlice> echServerKey = flagStringSlice("ech-server-key"u8, ""u8);
internal static ж<stringSlice> echServerRetryConfig = flagStringSlice("ech-is-retry-config"u8, ""u8);
internal static ж<bool> expectSessionMiss = flag.Bool("expect-session-miss"u8, false, ""u8);
internal static ж<bool> _ᴛ7ʗ = flag.Bool("enable-early-data"u8, false, ""u8);
internal static ж<bool> _ᴛ8ʗ = flag.Bool("on-resume-expect-accept-early-data"u8, false, ""u8);
internal static ж<bool> _ᴛ9ʗ = flag.Bool("expect-ticket-supports-early-data"u8, false, ""u8);
internal static ж<bool> onResumeShimWritesFirst = flag.Bool("on-resume-shim-writes-first"u8, false, ""u8);
internal static ж<@string> advertiseALPN = flag.String("advertise-alpn"u8, ""u8, ""u8);
internal static ж<@string> expectALPN = flag.String("expect-alpn"u8, ""u8, ""u8);
internal static ж<bool> rejectALPN = flag.Bool("reject-alpn"u8, false, ""u8);
internal static ж<bool> declineALPN = flag.Bool("decline-alpn"u8, false, ""u8);
internal static ж<@string> expectAdvertisedALPN = flag.String("expect-advertised-alpn"u8, ""u8, ""u8);
internal static ж<@string> selectALPN = flag.String("select-alpn"u8, ""u8, ""u8);
internal static ж<@string> hostName = flag.String("host-name"u8, ""u8, ""u8);
internal static ж<bool> verifyPeer = flag.Bool("verify-peer"u8, false, ""u8);
internal static ж<bool> _ᴛ10ʗ = flag.Bool("use-custom-verify-callback"u8, false, ""u8);

internal partial struct stringSlice /*[]@string*/;

internal static ж<stringSlice> flagStringSlice(@string name, @string usage) {
    var f = Ꮡ(new stringSlice(new @string[]{}.slice()));
    flag.Var(new tls_internal_test_package.stringSliceжValue(f), name, usage);
    return f;
}

internal static @string String(this ref stringSlice saf) {
    return strings.Join(saf, ","u8);
}

internal static error Set(this ref stringSlice saf, @string s) {
    saf = append(saf, s);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object echServerConfigEchServerˢ = (@string)"-ech-server-config, -ech-server-key, and -ech-is-retry-config mismatch"u8;
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string localhostˢ = "localhost"u8;
internal static readonly object unexpectedAlpnProtocolˢ = (@string)"unexpected ALPN protocol"u8;
internal static readonly object expectedEchToBeAcceptedˢ = (@string)"expected ECH to be accepted, but connection state shows it was not"u8;
internal static readonly object expectedEchToBeAcceptedˢ2 = (@string)"expected ECH to be accepted on resumption, but connection state shows it was not"u8;
internal static readonly object didNotExpectEchButItWasˢ = (@string)"did not expect ECH, but it was accepted"u8;
internal static readonly object expectedHrrButDidNotDoItˢ = (@string)"expected HRR but did not do it"u8;
internal static readonly object expectedNoHrrButDidDoItˢ = (@string)"expected no HRR but did do it"u8;
internal static readonly object unexpectedSessionˢ = (@string)"unexpected session resumption"u8;

internal static void bogoShim() {
    GoFrame ᒐ = default;
    try {
        if (isHandshakerSupported.Value) {
            fmt.Println((@string)"No"u8);
            return;
        }
        var cfg = Ꮡ(new Config(
            ServerName: "test"u8,
            MinVersion: (uint16)(minVersion.Value),
            MaxVersion: (uint16)(maxVersion.Value),
            ClientSessionCache: NewLRUClientSessionCache(0),
            GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> chi) => {
                if (expectAdvertisedALPN.Value != ""u8) {
                    var s = ((cryptobyte.String)slice<byte>(expectAdvertisedALPN.Value));
                    slice<@string> expectedALPNs = default!;
                    while (!s.Empty()) {
                        ref var alpn = ref heap<cryptobyte.String>(out var Ꮡalpn);
                        if (!s.ReadUint8LengthPrefixed(Ꮡalpn)) {
                            return (default!, fmt.Errorf("unexpected error while parsing arguments for -expect-advertised-alpn"u8));
                        }
                        expectedALPNs = append(expectedALPNs, ((@string)(slice<byte>)alpn));
                    }
                    if (!slices.Equal<slice<@string>, @string>((~chi).SupportedProtos, expectedALPNs)) {
                        return (default!, fmt.Errorf("unexpected ALPN: got %q, want %q"u8, (~chi).SupportedProtos, expectedALPNs));
                    }
                }
                return (default!, default!);
            }
        ));
        if (noTLS1.Value){
            cfg.Value.MinVersion = VersionTLS11;
            if (noTLS11.Value) {
                cfg.Value.MinVersion = VersionTLS12;
                if (noTLS12.Value) {
                    cfg.Value.MinVersion = VersionTLS13;
                    if (noTLS13.Value) {
                        log.Fatalf("no supported versions enabled"u8);
                    }
                }
            }
        } else 
        if (noTLS13.Value) {
            cfg.Value.MaxVersion = VersionTLS12;
            if (noTLS12.Value) {
                cfg.Value.MaxVersion = VersionTLS11;
                if (noTLS11.Value) {
                    cfg.Value.MaxVersion = VersionTLS10;
                    if (noTLS1.Value) {
                        log.Fatalf("no supported versions enabled"u8);
                    }
                }
            }
        }
        if (advertiseALPN.Value != ""u8) {
            @string alpns = advertiseALPN.Value;
            while (len(alpns) > 0) {
                nint alpnLen = (nint)alpns[0];
                cfg.Value.NextProtos = append((~cfg).NextProtos, alpns.slice(1, 1 + alpnLen));
                alpns = alpns.slice(alpnLen + 1);
            }
        }
        if (rejectALPN.Value) {
            cfg.Value.NextProtos = new @string[]{"unnegotiableprotocol"u8}.slice();
        }
        if (declineALPN.Value) {
            cfg.Value.NextProtos = new @string[]{}.slice();
        }
        if (selectALPN.Value != ""u8) {
            cfg.Value.NextProtos = new @string[]{selectALPN.Value}.slice();
        }
        if (hostName.Value != ""u8) {
            cfg.Value.ServerName = hostName.Value;
        }
        if (keyfile.Value != ""u8 || certfile.Value != ""u8) {
            var (pair, err) = LoadX509KeyPair(certfile.Value, keyfile.Value);
            if (err != default!) {
                log.Fatalf("load key-file err: %s"u8, err);
            }
            cfg.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{pair}.slice();
        }
        if (trustCert.Value != ""u8) {
            var pool = Δx509.NewCertPool();
            var (certFile, err) = os.ReadFile(trustCert.Value);
            if (err != default!) {
                log.Fatalf("load trust-cert err: %s"u8, err);
            }
            var (block, _) = pem.Decode(certFile);
            (var cert, err) = Δx509.ParseCertificate((~block).Bytes);
            if (err != default!) {
                log.Fatalf("parse trust-cert err: %s"u8, err);
            }
            pool.AddCert(cert);
            cfg.Value.RootCAs = pool;
        }
        if (requireAnyClientCertificate.Value) {
            cfg.Value.ClientAuth = RequireAnyClientCert;
        }
        if (verifyPeer.Value) {
            cfg.Value.ClientAuth = VerifyClientCertIfGiven;
        }
        if (echConfigListB64.Value != ""u8) {
            var (echConfigList, err) = base64.StdEncoding.DecodeString(echConfigListB64.Value);
            if (err != default!) {
                log.Fatalf("parse ech-config-list err: %s"u8, err);
            }
            cfg.Value.EncryptedClientHelloConfigList = echConfigList;
            cfg.Value.MinVersion = VersionTLS13;
        }
        if (len(curves.ValueSlot) != 0) {
            foreach (var (_, curveStr) in curves.ValueSlot) {
                var (id, err) = strconv.Atoi(curveStr);
                if (err != default!) {
                    log.Fatalf("failed to parse curve id %q: %s"u8, curveStr, err);
                }
                cfg.Value.CurvePreferences = append((~cfg).CurvePreferences, ((global::go.crypto.tls_package.CurveID)(uint16)id));
            }
        }
        if (len(echServerConfig.ValueSlot) != 0) {
            if (len(echServerConfig.ValueSlot) != len(echServerKey.ValueSlot) || len(echServerConfig.ValueSlot) != len(echServerRetryConfig.ValueSlot)) {
                log.Fatal(echServerConfigEchServerˢ);
            }
            foreach (var (i, c) in echServerConfig.ValueSlot) {
                var (configBytes, err) = base64.StdEncoding.DecodeString(c);
                if (err != default!) {
                    log.Fatalf("parse ech-server-config err: %s"u8, err);
                }
                (var privBytes, err) = base64.StdEncoding.DecodeString((echServerKey.ValueSlot)[i]);
                if (err != default!) {
                    log.Fatalf("parse ech-server-key err: %s"u8, err);
                }
                cfg.Value.EncryptedClientHelloKeys = append((~cfg).EncryptedClientHelloKeys, new EncryptedClientHelloKey(
                    Config: configBytes,
                    PrivateKey: privBytes,
                    SendAsRetry: (echServerRetryConfig.ValueSlot)[i] == "1"
                ));
            }
        }
        for (nint i = 0; i < resumeCount.Value + 1; i++) {
            if (i > 0 && (onResumeECHConfigListB64.Value != ""u8)) {
                var (echConfigList, errΔ1) = base64.StdEncoding.DecodeString(onResumeECHConfigListB64.Value);
                if (errΔ1 != default!) {
                    log.Fatalf("parse ech-config-list err: %s"u8, errΔ1);
                }
                cfg.Value.EncryptedClientHelloConfigList = echConfigList;
            }
            var (conn, err) = net.Dial(tcpˢ, net.JoinHostPort(localhostˢ, port.Value));
            if (err != default!) {
                log.Fatalf("dial err: %s"u8, err);
            }
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            // Write the shim ID we were passed as a little endian uint64
            var shimIDBytes = new slice<byte>(8);
            byteorder.LEPutUint64(shimIDBytes, shimID.Value);
            {
                var (_, errΔ1) = conn.Write(shimIDBytes); if (errΔ1 != default!) {
                    log.Fatalf("failed to write shim id: %s"u8, errΔ1);
                }
            }
            ж<global::go.crypto.tls_package.Conn> tlsConn = default!;
            if (server.Value){
                tlsConn = Server(conn, cfg);
            } else {
                tlsConn = Client(conn, cfg);
            }
            if (i == 0 && shimWritesFirst.Value) {
                {
                    var (_, errΔ2) = tlsConn.Write(slice<byte>("hello"u8)); if (errΔ2 != default!) {
                        log.Fatalf("write err: %s"u8, errΔ2);
                    }
                }
            }
            while (ᐧ) {
                var buf = new slice<byte>(500);
                nint n = default!;
                (n, err) = tlsConn.Read(buf);
                if (err != default!) {
                    break;
                }
                buf = buf.slice(0, n);
                foreach (var (iΔ1, _) in buf) {
                    buf[iΔ1] ^= (byte)(0xff);
                }
                {
                    (_, err) = tlsConn.Write(buf); if (err != default!) {
                        break;
                    }
                }
            }
            if (err != default! && !AreEqual(err, io.EOF)) {
                var (retryErr, ok) = err._<ж<global::go.crypto.tls_package.ECHRejectionError>>(ᐧ);
                if (!ok) {
                    log.Fatalf("unexpected error type returned: %v"u8, err);
                }
                if (expectNoECHRetryConfigs.Value && len((~retryErr).RetryConfigList) > 0) {
                    log.Fatalf("expected no ECH retry configs, got some"u8);
                }
                if (expectedECHRetryConfigs.Value != ""u8) {
                    var (expectedRetryConfigs, errΔ3) = base64.StdEncoding.DecodeString(expectedECHRetryConfigs.Value);
                    if (errΔ3 != default!) {
                        log.Fatalf("failed to decode expected retry configs: %s"u8, errΔ3);
                    }
                    if (!bytes.Equal((~retryErr).RetryConfigList, expectedRetryConfigs)) {
                        log.Fatalf("unexpected retry list returned: got %x, want %x"u8, (~retryErr).RetryConfigList, expectedRetryConfigs);
                    }
                }
                log.Fatalf("conn error: %s"u8, err);
            }
            var cs = tlsConn.ConnectionState();
            if (cs.HandshakeComplete) {
                if (expectALPN.Value != ""u8 && cs.NegotiatedProtocol != expectALPN.Value) {
                    log.Fatalf("unexpected protocol negotiated: want %q, got %q"u8, expectALPN.Value, cs.NegotiatedProtocol);
                }
                if (selectALPN.Value != ""u8 && cs.NegotiatedProtocol != selectALPN.Value) {
                    log.Fatalf("unexpected protocol negotiated: want %q, got %q"u8, selectALPN.Value, cs.NegotiatedProtocol);
                }
                if (expectVersion.Value != 0 && cs.Version != (uint16)(expectVersion.Value)) {
                    log.Fatalf("expected ssl version %q, got %q"u8, (uint16)(expectVersion.Value), cs.Version);
                }
                if (declineALPN.Value && cs.NegotiatedProtocol != ""u8) {
                    log.Fatal(unexpectedAlpnProtocolˢ);
                }
                if (expectECHAccepted.Value && !cs.ECHAccepted){
                    log.Fatal(expectedEchToBeAcceptedˢ);
                } else 
                if (i == 0 && onInitialExpectECHAccepted.Value && !cs.ECHAccepted){
                    log.Fatal(expectedEchToBeAcceptedˢ);
                } else 
                if (i > 0 && onResumeExpectECHAccepted.Value && !cs.ECHAccepted){
                    log.Fatal(expectedEchToBeAcceptedˢ2);
                } else 
                if (i == 0 && !expectECHAccepted.Value && cs.ECHAccepted) {
                    log.Fatal(didNotExpectEchButItWasˢ);
                }
                if (expectHRR.Value && !cs.testingOnlyDidHRR) {
                    log.Fatal(expectedHrrButDidNotDoItˢ);
                }
                if (expectNoHRR.Value && cs.testingOnlyDidHRR) {
                    log.Fatal(expectedNoHrrButDidDoItˢ);
                }
                if (expectSessionMiss.Value && cs.DidResume) {
                    log.Fatal(unexpectedSessionˢ);
                }
                if (expectedServerName.Value != ""u8 && cs.ServerName != expectedServerName.Value) {
                    log.Fatalf("unexpected server name: got %q, want %q"u8, cs.ServerName, expectedServerName.Value);
                }
            }
            if (expectedCurve.Value != ""u8) {
                var (expectedCurveID, errΔ4) = strconv.Atoi(expectedCurve.Value);
                if (errΔ4 != default!) {
                    log.Fatalf("failed to parse -expect-curve-id: %s"u8, errΔ4);
                }
                if ((~tlsConn).curveID != ((global::go.crypto.tls_package.CurveID)(uint16)expectedCurveID)) {
                    log.Fatalf("unexpected curve id: want %d, got %d"u8, expectedCurveID, (~tlsConn).curveID);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static readonly object windowsNetworkˢ = (@string)"#66913: windows network connections are flakey on builders"u8;
internal static readonly @string bogoConfigJsonˢ = "bogo_config.json"u8;
internal static readonly @string boringsslGooglesourceComˢ = "boringssl.googlesource.com/boringssl.git"u8;
internal static readonly @string resultsJsonˢ = "results.json"u8;
internal static readonly @string sslTestRunnerˢ = "ssl/test/runner"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string boringsslModVerᶜ = "v0.0.0-20241120195446-5cce3fbd23e1"u8;

public static void TestBogoSuite(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    if (testenv.Builder() != ""u8 && runtime.GOOS == "windows"u8) {
        Ꮡt.Skip(windowsNetworkˢ);
    }
    skipFIPS(Ꮡt);
    // In order to make Go test caching work as expected, we stat the
    // bogo_config.json file, so that the Go testing hooks know that it is
    // important for this test and will invalidate a cached test result if the
    // file changes.
    {
        var (_, errΔ1) = os.Stat(bogoConfigJsonˢ); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string bogoDir = default!;
    if (bogoLocalDir.Value != ""u8){
        bogoDir = bogoLocalDir.Value;
    } else {
        @string boringsslModVer = boringsslModVerᶜ;
        bogoDir = cryptotest.FetchModule(Ꮡt, boringsslGooglesourceComˢ, boringsslModVer);
    }
    var (cwd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string resultsFile = filepath.Join(Ꮡt.TempDir(), resultsJsonˢ);
    var args = new @string[]{
        "test"u8,
        "."u8,
        fmt.Sprintf("-shim-config=%s"u8, filepath.Join(cwd, bogoConfigJsonˢ)),
        fmt.Sprintf("-shim-path=%s"u8, os.Args[0]),
        "-shim-extra-flags=-bogo-mode"u8,
        "-allow-unimplemented"u8,
        "-loose-errors"u8, // TODO(roland): this should be removed eventually

        fmt.Sprintf("-json-output=%s"u8, resultsFile)
    }.slice();
    if (bogoFilter.Value != ""u8) {
        args = append(args, fmt.Sprintf("-test=%s"u8, bogoFilter.Value));
    }
    var cmd = testenv.Command(new tls_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new tls_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    var @out = Ꮡ(new strings.Builder(nil));
    cmd.Value.Stderr = new tls_test_package.strings_BuilderжWriter(@out);
    cmd.Value.Dir = filepath.Join(bogoDir, sslTestRunnerˢ);
    err = cmd.Run();
    // NOTE: we don't immediately check the error, because the failure could be either because
    // the runner failed for some unexpected reason, or because a test case failed, and we
    // cannot easily differentiate these cases. We check if the JSON results file was written,
    // which should only happen if the failure was because of a test failure, and use that
    // to determine the failure mode.
    var (resultsJSON, jsonErr) = os.ReadFile(resultsFile);
    if (jsonErr != default!) {
        if (err != default!) {
            Ꮡt.Fatalf("bogo failed: %s\n%s"u8, err, @out.OrTypedNil());
        }
        Ꮡt.Fatalf("failed to read results JSON file: %s"u8, jsonErr);
    }
    ref var results = ref heap(new bogoResults(), out var Ꮡresults);
    {
        var errΔ2 = json.Unmarshal(resultsJSON, Ꮡresults); if (errΔ2 != default!) {
            Ꮡt.Fatalf("failed to parse results JSON: %s"u8, errΔ2);
        }
    }
    // assertResults contains test results we want to make sure
    // are present in the output. They are only checked if -bogo-filter
    // was not passed.
    var assertResults = new map<@string, @string>{
        ["CurveTest-Client-MLKEM-TLS13"u8] = "PASS"u8,
        ["CurveTest-Server-MLKEM-TLS13"u8] = "PASS"u8
    };
    foreach (var (name, vᴛ1) in results.Tests) {
        ref var result = ref heap(new bogoResults_Tests(), out var Ꮡresult);
        result = vᴛ1;

        // This is not really the intended way to do this... but... it works?
        var assertResultsʗ1 = assertResults;
        var resultʗ1 = result;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            if (resultʗ1.Actual == "FAIL"u8 && resultʗ1.IsUnexpected) {
                tΔ1.Fatal(resultʗ1.Error);
            }
            {
                var (expectedResult, ok) = assertResultsʗ1[name, ꟷ]; if (ok && expectedResult != resultʗ1.Actual) {
                    tΔ1.Fatalf("unexpected result: got %s, want %s"u8, resultʗ1.Actual, assertResultsʗ1[name]);
                }
            }
            delete(assertResultsʗ1, name);
            if (resultʗ1.Actual == "SKIP"u8) {
                tΔ1.Skip();
            }
        });
    }
    if (bogoFilter.Value == ""u8) {
        // Anything still in assertResults did not show up in the results, so we should fail
        foreach (var (name, expectedResult) in assertResults) {
            Ꮡt.Run(name, (ж<testing.T> tΔ2) => {
                tΔ2.Fatalf("expected test to run with result %s, but it was not present in the test results"u8, expectedResult);
            });
        }
    }
}

partial struct bogoResults_Tests /*dyn*/ {
    public @string Actual; /*`json:"actual"`*/
    public @string Expected; /*`json:"expected"`*/
    public bool IsUnexpected; /*`json:"is_unexpected"`*/
    public @string Error; /*`json:"error,omitempty"`*/
}

// bogoResults is a copy of boringssl.googlesource.com/boringssl/testresults.Results
internal partial struct bogoResults {
    public nint Version; /*`json:"version"`*/
    public bool Interrupted; /*`json:"interrupted"`*/
    public @string PathDelimiter; /*`json:"path_delimiter"`*/
    public float64 SecondsSinceEpoch; /*`json:"seconds_since_epoch"`*/
    public map<@string, nint> NumFailuresByType; /*`json:"num_failures_by_type"`*/
    public map<@string, bogoResults_Tests> Tests; /*`json:"tests"`*/
}

} // end tls_internal_test_package
