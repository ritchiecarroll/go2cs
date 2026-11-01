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
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using template = html.template_package;
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
using time = time_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using encoding;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.io;
using go.os;
using html;
using path;
using static go.crypto.tls_package;
using vendor.golang.org.x.crypto;

partial class tls_internal_test_package {

internal static ref readonly @string boringsslModVer => ref boringsslModVerᵇ;
private static readonly @string boringsslModVerᵇ = "v0.0.0-20250620172916-f51d8b099832"u8;

internal static ж<@string> port = flag.String("port", "", "");
internal static ж<bool> server = flag.Bool("server", false, "");

internal static ж<bool> isHandshakerSupported = flag.Bool("is-handshaker-supported", false, "");

internal static ж<@string> keyfile = flag.String("key-file", "", "");
internal static ж<@string> certfile = flag.String("cert-file", "", "");
internal static ж<base64Flag> ocspResponse = flagBase64("ocsp-response", "");
internal static ж<intSlice> signingPrefs = flagIntSlice("signing-prefs", "");

internal static ж<@string> trustCert = flag.String("trust-cert", "", "");

internal static ж<nint> minVersion = flag.Int("min-version", VersionSSL30, "");
internal static ж<nint> maxVersion = flag.Int("max-version", VersionTLS13, "");
internal static ж<nint> expectVersion = flag.Int("expect-version", 0, "");

internal static ж<bool> noTLS1 = flag.Bool("no-tls1", false, "");
internal static ж<bool> noTLS11 = flag.Bool("no-tls11", false, "");
internal static ж<bool> noTLS12 = flag.Bool("no-tls12", false, "");
internal static ж<bool> noTLS13 = flag.Bool("no-tls13", false, "");

internal static ж<bool> requireAnyClientCertificate = flag.Bool("require-any-client-certificate", false, "");

internal static ж<bool> shimWritesFirst = flag.Bool("shim-writes-first", false, "");

internal static ж<nint> resumeCount = flag.Int("resume-count", 0, "");

internal static ж<intSlice> curves = flagIntSlice("curves", "");
internal static ж<@string> expectedCurve = flag.String("expect-curve-id", "", "");

internal static ж<intSlice> verifyPrefs = flagIntSlice("verify-prefs", "");
internal static ж<@string> expectedSigAlg = flag.String("expect-peer-signature-algorithm", "", "");
internal static ж<intSlice> expectedPeerSigAlg = flagIntSlice("expect-peer-verify-pref", "");

internal static ж<uint64> shimID = flag.Uint64("shim-id", 0, "");
internal static ж<bool> _ᴛ1ʗ = flag.Bool("ipv6", false, "");

internal static ж<base64Flag> echConfigList = flagBase64("ech-config-list", "");
internal static ж<bool> expectECHAccepted = flag.Bool("expect-ech-accept", false, "");
internal static ж<bool> expectHRR = flag.Bool("expect-hrr", false, "");
internal static ж<bool> expectNoHRR = flag.Bool("expect-no-hrr", false, "");
internal static ж<@string> expectedECHRetryConfigs = flag.String("expect-ech-retry-configs", "", "");
internal static ж<bool> expectNoECHRetryConfigs = flag.Bool("expect-no-ech-retry-configs", false, "");
internal static ж<bool> onInitialExpectECHAccepted = flag.Bool("on-initial-expect-ech-accept", false, "");
internal static ж<bool> _ᴛ2ʗ = flag.Bool("expect-no-ech-name-override", false, "");
internal static ж<@string> _ᴛ3ʗ = flag.String("expect-ech-name-override", "", "");
internal static ж<bool> _ᴛ4ʗ = flag.Bool("reverify-on-resume", false, "");
internal static ж<base64Flag> onResumeECHConfigList = flagBase64("on-resume-ech-config-list", "");
internal static ж<bool> _ᴛ5ʗ = flag.Bool("on-resume-expect-reject-early-data", false, "");
internal static ж<bool> onResumeExpectECHAccepted = flag.Bool("on-resume-expect-ech-accept", false, "");
internal static ж<bool> _ᴛ6ʗ = flag.Bool("on-resume-expect-no-ech-name-override", false, "");
internal static ж<@string> expectedServerName = flag.String("expect-server-name", "", "");
internal static ж<stringSlice> echServerConfig = flagStringSlice("ech-server-config", "");
internal static ж<stringSlice> echServerKey = flagStringSlice("ech-server-key", "");
internal static ж<stringSlice> echServerRetryConfig = flagStringSlice("ech-is-retry-config", "");

internal static ж<bool> expectSessionMiss = flag.Bool("expect-session-miss", false, "");

internal static ж<bool> _ᴛ7ʗ = flag.Bool("enable-early-data", false, "");
internal static ж<bool> _ᴛ8ʗ = flag.Bool("on-resume-expect-accept-early-data", false, "");
internal static ж<bool> _ᴛ9ʗ = flag.Bool("expect-ticket-supports-early-data", false, "");
internal static ж<bool> _ᴛ10ʗ = flag.Bool("on-resume-shim-writes-first", false, "");

internal static ж<@string> advertiseALPN = flag.String("advertise-alpn", "", "");
internal static ж<@string> expectALPN = flag.String("expect-alpn", "", "");
internal static ж<bool> rejectALPN = flag.Bool("reject-alpn", false, "");
internal static ж<bool> declineALPN = flag.Bool("decline-alpn", false, "");
internal static ж<@string> expectAdvertisedALPN = flag.String("expect-advertised-alpn", "", "");
internal static ж<@string> selectALPN = flag.String("select-alpn", "", "");

internal static ж<@string> hostName = flag.String("host-name", "", "");

internal static ж<bool> verifyPeer = flag.Bool("verify-peer", false, "");
internal static ж<bool> _ᴛ11ʗ = flag.Bool("use-custom-verify-callback", false, "");

internal static ж<bool> waitForDebugger = flag.Bool("wait-for-debugger", false, "");

internal partial struct stringSlice /*[]@string*/;

internal static ж<stringSlice> flagStringSlice(@string name, @string usage) {
    var f = @new<stringSlice>();
    flag.Var(new tls_internal_test_package.stringSliceжValue(f), name, usage);
    return f;
}

internal static @string String(this ref stringSlice saf) {
    return strings.Join(saf, ",");
}

internal static error Set(this ref stringSlice saf, @string s) {
    saf = append(saf, s);
    return default;
}

internal partial struct intSlice /*[]int64*/;

internal static ж<intSlice> flagIntSlice(@string name, @string usage) {
    var f = @new<intSlice>();
    flag.Var(new tls_internal_test_package.intSliceжValue(f), name, usage);
    return f;
}

internal static @string String(this ref intSlice sf) {
    return strings.Join(strings.Split(fmt.Sprint(sf), " "), ",");
}

internal static error Set(this ref intSlice sf, @string s) {
    var (i, err) = strconv.ParseInt(s, 10, 64);
    if (err != nil) {
        return err;
    }
    sf = append(sf, i);
    return default;
}

internal partial struct base64Flag /*[]byte*/;

internal static ж<base64Flag> flagBase64(@string name, @string usage) {
    var f = @new<base64Flag>();
    flag.Var(new tls_internal_test_package.base64FlagжValue(f), name, usage);
    return f;
}

internal static @string String(this ref base64Flag f) {
    return base64.StdEncoding.EncodeToString(f);
}

internal static error Set(this ref base64Flag f, @string s) {
    if (f != nil) {
        return fmt.Errorf("multiple base64 values not supported"u8);
    }
    var (b, err) = base64.StdEncoding.DecodeString(s);
    if (err != nil) {
        return err;
    }
    f = b;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object echServerConfigEchServerˢ = (@string)"-ech-server-config, -ech-server-key, and -ech-is-retry-config mismatch"u8;
internal static ref readonly @string tcpˢ => ref tcpˢᵇ;
private static readonly @string tcpˢᵇ = "tcp"u8;
internal static ref readonly @string localhostˢ => ref localhostˢᵇ;
private static readonly @string localhostˢᵇ = "localhost"u8;
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
            fmt.Println("No");
            return;
        }

        fmt.Printf("BoGo shim flags: %q"u8, os.Args[1..]);

        // Test with both the default and insecure cipher suites.
        slice<uint16> ciphersuites = default;
        foreach (var (_, s) in appendꓸꓸꓸ(CipherSuites(), InsecureCipherSuites())) {
            ciphersuites = append(ciphersuites, s.ID);
        }

        var cfg = Ꮡ(new Config(
            ServerName: "test",

            MinVersion: (uint16)(minVersion.Value),
            MaxVersion: (uint16)(maxVersion.Value),

            ClientSessionCache: NewLRUClientSessionCache(0),

            CipherSuites: ciphersuites,

            GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> chi) => {
                if (expectAdvertisedALPN.Value != ""u8) {
                    var s = (cryptobyte.String)slice<byte>(expectAdvertisedALPN.Value);

                    slice<@string> expectedALPNs = default;

                    while (!s.Empty()) {
                        ref var alpn = ref heap<cryptobyte.String>(out var Ꮡalpn);
                        if (!s.ReadUint8LengthPrefixed(Ꮡalpn)) {
                            return (default, fmt.Errorf("unexpected error while parsing arguments for -expect-advertised-alpn"u8));
                        }
                        expectedALPNs = append(expectedALPNs, (@string)(slice<byte>)alpn);
                    }

                    if (!slices.Equal<slice<@string>, @string>(chi.SupportedProtos, expectedALPNs)) {
                        return (default, fmt.Errorf("unexpected ALPN: got %q, want %q"u8, chi.SupportedProtos, expectedALPNs));
                    }
                }
                return (default, default);
            }
        ));

        if (noTLS1.Value) {
            cfg.MinVersion = VersionTLS11;
            if (noTLS11.Value) {
                cfg.MinVersion = VersionTLS12;
                if (noTLS12.Value) {
                    cfg.MinVersion = VersionTLS13;
                    if (noTLS13.Value) {
                        log.Fatalf("no supported versions enabled");
                    }
                }
            }
        } else if (noTLS13.Value) {
            cfg.MaxVersion = VersionTLS12;
            if (noTLS12.Value) {
                cfg.MaxVersion = VersionTLS11;
                if (noTLS11.Value) {
                    cfg.MaxVersion = VersionTLS10;
                    if (noTLS1.Value) {
                        log.Fatalf("no supported versions enabled");
                    }
                }
            }
        }

        if (advertiseALPN.Value != ""u8) {
            @string alpns = advertiseALPN.Value;
            while (len(alpns) > 0) {
                nint alpnLen = (nint)alpns[0];
                cfg.NextProtos = append(cfg.NextProtos, alpns.slice(1, 1 + alpnLen));
                alpns = alpns.slice(alpnLen + 1);
            }
        }

        if (rejectALPN.Value) {
            cfg.NextProtos = slice<@string>(["unnegotiableprotocol"]);
        }

        if (declineALPN.Value) {
            cfg.NextProtos = slice<@string>([]);
        }
        if (selectALPN.Value != ""u8) {
            cfg.NextProtos = slice<@string>([selectALPN.Value]);
        }

        if (hostName.Value != ""u8) {
            cfg.ServerName = hostName.Value;
        }

        if (keyfile.Value != ""u8 || certfile.Value != ""u8) {
            ref var pair = ref heap<global::go.crypto.tls_package.Certificate>(out var Ꮡpair);
            (pair, var err) = LoadX509KeyPair(certfile.Value, keyfile.Value);
            if (err != nil) {
                log.Fatalf("load key-file err: %s", err);
            }
            foreach (var (_, id) in signingPrefs.ValueSlot) {
                pair.SupportedSignatureAlgorithms = append(pair.SupportedSignatureAlgorithms, (global::go.crypto.tls_package.SignatureScheme)(uint16)id);
            }
            pair.OCSPStaple = ocspResponse.ValueSlot;
            // Use Get[Client]Certificate to force the use of the certificate, which
            // more closely matches the BoGo expectations (e.g. handshake failure if
            // no client certificates are compatible).
            cfg.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> chi) => {
                if (expectedPeerSigAlg.ValueSlot != nil) {
                    if (len(chi.SignatureSchemes) != len(expectedPeerSigAlg.ValueSlot)) {
                        return (default, fmt.Errorf("unexpected signature algorithms: got %s, want %v"u8, chi.SignatureSchemes, expectedPeerSigAlg.ValueSlot));
                    }
                    foreach (var i in range(expectedPeerSigAlg.ValueSlot)) {
                        if (chi.SignatureSchemes[i] != ((global::go.crypto.tls_package.SignatureScheme)(uint16)(expectedPeerSigAlg.ValueSlot)[i])) {
                            return (default, fmt.Errorf("unexpected signature algorithms: got %s, want %v"u8, chi.SignatureSchemes, expectedPeerSigAlg.ValueSlot));
                        }
                    }
                }
                return (Ꮡpair, default);
            };
            cfg.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (expectedPeerSigAlg.ValueSlot != nil) {
                    if (len(cri.SignatureSchemes) != len(expectedPeerSigAlg.ValueSlot)) {
                        return (default, fmt.Errorf("unexpected signature algorithms: got %s, want %v"u8, cri.SignatureSchemes, expectedPeerSigAlg.ValueSlot));
                    }
                    foreach (var i in range(expectedPeerSigAlg.ValueSlot)) {
                        if (cri.SignatureSchemes[i] != ((global::go.crypto.tls_package.SignatureScheme)(uint16)(expectedPeerSigAlg.ValueSlot)[i])) {
                            return (default, fmt.Errorf("unexpected signature algorithms: got %s, want %v"u8, cri.SignatureSchemes, expectedPeerSigAlg.ValueSlot));
                        }
                    }
                }
                return (Ꮡpair, default);
            };
        }
        if (trustCert.Value != ""u8) {
            var pool = Δx509.NewCertPool();
            var (certFile, err) = os.ReadFile(trustCert.Value);
            if (err != nil) {
                log.Fatalf("load trust-cert err: %s", err);
            }
            var (block, _) = pem.Decode(certFile);
            (var cert, err) = Δx509.ParseCertificate(block.Bytes);
            if (err != nil) {
                log.Fatalf("parse trust-cert err: %s", err);
            }
            pool.AddCert(cert);
            cfg.RootCAs = pool;
        }

        if (requireAnyClientCertificate.Value) {
            cfg.ClientAuth = RequireAnyClientCert;
        }
        if (verifyPeer.Value) {
            cfg.ClientAuth = VerifyClientCertIfGiven;
        }

        if (echConfigList.ValueSlot != nil) {
            cfg.EncryptedClientHelloConfigList = echConfigList.ValueSlot;
            cfg.MinVersion = VersionTLS13;
        }

        if (curves.ValueSlot != nil) {
            foreach (var (_, id) in curves.ValueSlot) {
                cfg.CurvePreferences = append(cfg.CurvePreferences, (global::go.crypto.tls_package.CurveID)(uint16)id);
            }
        }

        if (verifyPrefs.ValueSlot != nil) {
            foreach (var (_, id) in verifyPrefs.ValueSlot) {
                testingOnlySupportedSignatureAlgorithms = append(testingOnlySupportedSignatureAlgorithms, (global::go.crypto.tls_package.SignatureScheme)(uint16)id);
            }
        }

        if (echServerConfig.ValueSlot != nil) {
            if (len(echServerConfig.ValueSlot) != len(echServerKey.ValueSlot) || len(echServerConfig.ValueSlot) != len(echServerRetryConfig.ValueSlot)) {
                log.Fatal(echServerConfigEchServerˢ);
            }

            foreach (var (i, c) in echServerConfig.ValueSlot) {
                var (configBytes, err) = base64.StdEncoding.DecodeString(c);
                if (err != nil) {
                    log.Fatalf("parse ech-server-config err: %s", err);
                }
                (var privBytes, err) = base64.StdEncoding.DecodeString((echServerKey.ValueSlot)[i]);
                if (err != nil) {
                    log.Fatalf("parse ech-server-key err: %s", err);
                }

                cfg.EncryptedClientHelloKeys = append(cfg.EncryptedClientHelloKeys, new EncryptedClientHelloKey(
                    Config: configBytes,
                    PrivateKey: privBytes,
                    SendAsRetry: (echServerRetryConfig.ValueSlot)[i] == "1"
                ));
            }
        }

        for (nint i = 0; i < resumeCount.Value + 1; i++) {
            if (i > 0 && onResumeECHConfigList.ValueSlot != nil) {
                cfg.EncryptedClientHelloConfigList = onResumeECHConfigList.ValueSlot;
            }

            var (conn, err) = net.Dial(tcpˢ, net.JoinHostPort(localhostˢ, port.Value));
            if (err != nil) {
                log.Fatalf("dial err: %s", err);
            }
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);

            // Write the shim ID we were passed as a little endian uint64
            var shimIDBytes = new slice<byte>(8);
            byteorder.LEPutUint64(shimIDBytes, shimID.Value);
            {
                var (_, errΔ1) = conn.Write(shimIDBytes); if (errΔ1 != nil) {
                    log.Fatalf("failed to write shim id: %s", errΔ1);
                }
            }

            ж<global::go.crypto.tls_package.Conn> tlsConn = default;
            if (server.Value) {
                tlsConn = Server(conn, cfg);
            } else {
                tlsConn = Client(conn, cfg);
            }

            if (i == 0 && shimWritesFirst.Value) {
                {
                    var (_, errΔ2) = tlsConn.Write(slice<byte>("hello"u8)); if (errΔ2 != nil) {
                        log.Fatalf("write err: %s", errΔ2);
                    }
                }
            }

            // If we were instructed to wait for a debugger, then send SIGSTOP to ourselves.
            // When the debugger attaches it will continue the process.
            if (waitForDebugger.Value) {
                pauseProcess();
            }

            for (;;) {
                var buf = new slice<byte>(500);
                nint n = default;
                (n, err) = tlsConn.Read(buf);
                if (err != nil) {
                    break;
                }
                buf = buf.slice(0, n);
                foreach (var iΔ1 in range(buf)) {
                    buf[iΔ1] ^= (byte)(0xff);
                }
                {
                    (_, err) = tlsConn.Write(buf); if (err != nil) {
                        break;
                    }
                }
            }
            if (!AreEqual(err, io.EOF)) {
                // Flush the TLS conn and then perform a graceful shutdown of the
                // TCP connection to avoid the runner side hitting an unexpected
                // write error before it has processed the alert we may have
                // generated for the error condition.
                orderlyShutdown(tlsConn);

                var (retryErr, ok) = err._<ж<global::go.crypto.tls_package.ECHRejectionError>>(ᐧ);
                if (!ok) {
                    log.Fatal(err);
                }
                if (expectNoECHRetryConfigs.Value && len(retryErr.RetryConfigList) > 0) {
                    log.Fatalf("expected no ECH retry configs, got some");
                }
                if (expectedECHRetryConfigs.Value != ""u8) {
                    var (expectedRetryConfigs, errΔ3) = base64.StdEncoding.DecodeString(expectedECHRetryConfigs.Value);
                    if (errΔ3 != nil) {
                        log.Fatalf("failed to decode expected retry configs: %s", errΔ3);
                    }
                    if (!bytes.Equal(retryErr.RetryConfigList, expectedRetryConfigs)) {
                        log.Fatalf("unexpected retry list returned: got %x, want %x", retryErr.RetryConfigList, expectedRetryConfigs);
                    }
                }
                log.Fatalf("conn error: %s", err);
            }

            var cs = tlsConn.ConnectionState();
            if (cs.HandshakeComplete) {
                if (expectALPN.Value != ""u8 && cs.NegotiatedProtocol != expectALPN.Value) {
                    log.Fatalf("unexpected protocol negotiated: want %q, got %q", expectALPN.Value, cs.NegotiatedProtocol);
                }

                if (selectALPN.Value != ""u8 && cs.NegotiatedProtocol != selectALPN.Value) {
                    log.Fatalf("unexpected protocol negotiated: want %q, got %q", selectALPN.Value, cs.NegotiatedProtocol);
                }

                if (expectVersion.Value != 0 && cs.Version != (uint16)(expectVersion.Value)) {
                    log.Fatalf("expected ssl version %d, got %d", expectVersion.Value, cs.Version);
                }
                if (declineALPN.Value && cs.NegotiatedProtocol != ""u8) {
                    log.Fatal(unexpectedAlpnProtocolˢ);
                }
                if (expectECHAccepted.Value && !cs.ECHAccepted) {
                    log.Fatal(expectedEchToBeAcceptedˢ);
                } else if (i == 0 && onInitialExpectECHAccepted.Value && !cs.ECHAccepted) {
                    log.Fatal(expectedEchToBeAcceptedˢ);
                } else if (i > 0 && onResumeExpectECHAccepted.Value && !cs.ECHAccepted) {
                    log.Fatal(expectedEchToBeAcceptedˢ2);
                } else if (i == 0 && !expectECHAccepted.Value && cs.ECHAccepted) {
                    log.Fatal(didNotExpectEchButItWasˢ);
                }

                if (expectHRR.Value && !cs.HelloRetryRequest) {
                    log.Fatal(expectedHrrButDidNotDoItˢ);
                }

                if (expectNoHRR.Value && cs.HelloRetryRequest) {
                    log.Fatal(expectedNoHrrButDidDoItˢ);
                }

                if (expectSessionMiss.Value && cs.DidResume) {
                    log.Fatal(unexpectedSessionˢ);
                }

                if (expectedServerName.Value != ""u8 && cs.ServerName != expectedServerName.Value) {
                    log.Fatalf("unexpected server name: got %q, want %q", cs.ServerName, expectedServerName.Value);
                }
            }

            if (expectedCurve.Value != ""u8) {
                var (expectedCurveID, errΔ4) = strconv.Atoi(expectedCurve.Value);
                if (errΔ4 != nil) {
                    log.Fatalf("failed to parse -expect-curve-id: %s", errΔ4);
                }
                if (cs.CurveID != ((global::go.crypto.tls_package.CurveID)(uint16)expectedCurveID)) {
                    log.Fatalf("unexpected curve id: want %d, got %d", expectedCurveID, tlsConn.curveID);
                }
            }

            // TODO: implement testingOnlyPeerSignatureAlgorithm on resumption.
            if (expectedSigAlg.Value != ""u8 && !cs.DidResume) {
                var (expectedSigAlgID, errΔ5) = strconv.Atoi(expectedSigAlg.Value);
                if (errΔ5 != nil) {
                    log.Fatalf("failed to parse -expect-peer-signature-algorithm: %s", errΔ5);
                }
                if (cs.testingOnlyPeerSignatureAlgorithm != ((global::go.crypto.tls_package.SignatureScheme)(uint16)expectedSigAlgID)) {
                    log.Fatalf("unexpected peer signature algorithm: want %s, got %s", (global::go.crypto.tls_package.SignatureScheme)(uint16)expectedSigAlgID, cs.testingOnlyPeerSignatureAlgorithm);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// If the test case produces an error, we don't want to immediately close the
// TCP connection after generating an alert. The runner side may try to write
// additional data to the connection before it reads the alert. If the conn
// has already been torn down, then these writes will produce an unexpected
// broken pipe err and fail the test.
internal static void orderlyShutdown(ж<global::go.crypto.tls_package.Conn> ᏑtlsConn) {
    ref var tlsConn = ref ᏑtlsConn.DerefOrNull();

    // Flush any pending alert data
    tlsConn.flush();

    var netConn = tlsConn.NetConn();
    var tcpConn = netConn._<ж<net.TCPConn>>();
    tcpConn.CloseWrite();

    // Read and discard any data that was sent by the peer.
    var buf = new slice<byte>(maxPlaintext);
    for (;;) {
        var (n, err) = tcpConn.Read(buf);
        if (n == 0 || err != nil) {
            break;
        }
    }

    tcpConn.CloseRead();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static readonly object windowsNetworkˢ = (@string)"#66913: windows network connections are flakey on builders"u8;
internal static ref readonly @string bogoConfigJsonˢ => ref bogoConfigJsonˢᵇ;
private static readonly @string bogoConfigJsonˢᵇ = "bogo_config.json"u8;
internal static ref readonly @string boringsslGooglesourceComˢ => ref boringsslGooglesourceComˢᵇ;
private static readonly @string boringsslGooglesourceComˢᵇ = "boringssl.googlesource.com/boringssl.git"u8;
internal static ref readonly @string resultsJsonˢ => ref resultsJsonˢᵇ;
private static readonly @string resultsJsonˢᵇ = "results.json"u8;
internal static ref readonly @string sslTestRunnerˢ => ref sslTestRunnerˢᵇ;
private static readonly @string sslTestRunnerˢᵇ = "ssl/test/runner"u8;

public static void TestBogoSuite(ж<testing.T> t) {
    var argsᴷ = new stackAppend<@string>();

    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    if (testenv.Builder() != ""u8 && runtime.GOOS == "windows"u8) {
        t.Skip(windowsNetworkˢ);
    }
    skipFIPS(t);

    // In order to make Go test caching work as expected, we stat the
    // bogo_config.json file, so that the Go testing hooks know that it is
    // important for this test and will invalidate a cached test result if the
    // file changes.
    {
        var (_, errΔ1) = os.Stat(bogoConfigJsonˢ); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }

    @string bogoDir = default;
    if (bogoLocalDir.Value != ""u8) {
        ensureLocalBogo(t, bogoLocalDir.Value);
        bogoDir = bogoLocalDir.Value;
    } else {
        bogoDir = cryptotest.FetchModule(t, boringsslGooglesourceComˢ, boringsslModVer);
    }

    var (cwd, err) = os.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }

    @string resultsFile = filepath.Join(t.TempDir(), resultsJsonˢ);

    var args = slice<@string>([
        "test",
        ".",
        fmt.Sprintf("-shim-config=%s"u8, filepath.Join(cwd, bogoConfigJsonˢ)),
        fmt.Sprintf("-shim-path=%s"u8, testenv.Executable(new tls_test_package.testing_TжTB(t))),
        "-shim-extra-flags=-bogo-mode",
        "-allow-unimplemented",
        "-loose-errors", // TODO(roland): this should be removed eventually
        fmt.Sprintf("-json-output=%s"u8, resultsFile)
    ]);
    if (bogoFilter.Value != ""u8) {
        args = append(argsᴷ, args, fmt.Sprintf("-test=%s"u8, bogoFilter.Value));
    }

    var cmd = testenv.Command(new tls_test_package.testing_TжTB(t), testenv.GoToolPath(new tls_test_package.testing_TжTB(t)), args.ꓸꓸꓸ);
    cmd.Dir = filepath.Join(bogoDir, sslTestRunnerˢ);
    (var @out, err) = cmd.CombinedOutput();
    // NOTE: we don't immediately check the error, because the failure could be either because
    // the runner failed for some unexpected reason, or because a test case failed, and we
    // cannot easily differentiate these cases. We check if the JSON results file was written,
    // which should only happen if the failure was because of a test failure, and use that
    // to determine the failure mode.

    var (resultsJSON, jsonErr) = os.ReadFile(resultsFile);
    if (jsonErr != nil) {
        if (err != nil) {
            t.Fatalf("bogo failed: %s\n%s", err, @out);
        }
        t.Fatalf("failed to read results JSON file: %s", jsonErr);
    }

    ref var results = ref heap(new bogoResults(), out var Ꮡresults);
    {
        var errΔ2 = json.Unmarshal(resultsJSON, Ꮡresults); if (errΔ2 != nil) {
            t.Fatalf("failed to parse results JSON: %s", errΔ2);
        }
    }

    if (bogoReport.Value != ""u8) {
        {
            var errΔ3 = generateReport(results, bogoReport.Value); if (errΔ3 != nil) {
                t.Fatalf("failed to generate report: %v", errΔ3);
            }
        }
    }

    // assertResults contains test results we want to make sure
    // are present in the output. They are only checked if -bogo-filter
    // was not passed.
    var assertResults = new map<@string, @string>{
        ["CurveTest-Client-MLKEM-TLS13"] = "PASS",
        ["CurveTest-Server-MLKEM-TLS13"] = "PASS",

        // Various signature algorithm tests checking that we enforce our
        // preferences on the peer.
        ["ClientAuth-Enforced"] = "PASS",
        ["ServerAuth-Enforced"] = "PASS",
        ["ClientAuth-Enforced-TLS13"] = "PASS",
        ["ServerAuth-Enforced-TLS13"] = "PASS",
        ["VerifyPreferences-Advertised"] = "PASS",
        ["VerifyPreferences-Enforced"] = "PASS",
        ["Client-TLS12-NoSign-RSA_PKCS1_MD5_SHA1"] = "PASS",
        ["Server-TLS12-NoSign-RSA_PKCS1_MD5_SHA1"] = "PASS",
        ["Client-TLS13-NoSign-RSA_PKCS1_MD5_SHA1"] = "PASS",
        ["Server-TLS13-NoSign-RSA_PKCS1_MD5_SHA1"] = "PASS"
    };

    foreach (var (name, vᴛ1) in results.Tests) {
        ref var result = ref heap(new bogoResults_Tests(), out var Ꮡresult);
        result = vᴛ1;

        // This is not really the intended way to do this... but... it works?
        var assertResultsʗ1 = assertResults;
        var resultʗ1 = result;
        t.Run(name, (ж<testing.T> tΔ1) => {
            if (resultʗ1.Actual == "FAIL"u8 && resultʗ1.IsUnexpected) {
                tΔ1.Fail();
            }
            if (resultʗ1.Error != ""u8) {
                tΔ1.Log(resultʗ1.Error);
            }
            {
                var (exp, ok) = assertResultsʗ1[name, ꟷ]; if (ok && exp != resultʗ1.Actual) {
                    tΔ1.Errorf("unexpected result: got %s, want %s", resultʗ1.Actual, exp);
                }
            }
            delete(assertResultsʗ1, name);
            if (resultʗ1.Actual == "SKIP"u8) {
                tΔ1.SkipNow();
            }
        });
    }
    if (bogoFilter.Value == ""u8) {
        // Anything still in assertResults did not show up in the results, so we should fail
        foreach (var (name, expectedResult) in assertResults) {
            t.Run(name, (ж<testing.T> tΔ2) => {
                tΔ2.Fatalf("expected test to run with result %s, but it was not present in the test results", expectedResult);
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gitˢ => ref gitˢᵇ;
private static readonly @string gitˢᵇ = "git"u8;
internal static ref readonly @string cloneˢ => ref cloneˢᵇ;
private static readonly @string cloneˢᵇ = "clone"u8;
internal static ref readonly @string noCheckoutˢ => ref noCheckoutˢᵇ;
private static readonly @string noCheckoutˢᵇ = "--no-checkout"u8;
internal static ref readonly @string httpsBoringsslˢ => ref httpsBoringsslˢᵇ;
private static readonly @string httpsBoringsslˢᵇ = "https://boringssl.googlesource.com/boringssl"u8;
internal static ref readonly @string checkoutˢ => ref checkoutˢᵇ;
private static readonly @string checkoutˢᵇ = "checkout"u8;

// ensureLocalBogo fetches BoringSSL to localBogoDir at the correct revision
// (from boringsslModVer) if localBogoDir doesn't already exist.
//
// If localBogoDir does exist, ensureLocalBogo fails the test if it isn't
// a directory.
internal static void ensureLocalBogo(ж<testing.T> t, @string localBogoDir) {
    t.Helper();

    {
        var (stat, err) = os.Stat(localBogoDir); if (err == nil) {
            if (!stat.IsDir()) {
                t.Fatalf("local bogo dir (%q) exists but is not a directory", localBogoDir);
            }

            t.Logf("using local bogo checkout from %q", localBogoDir);
            return;
        } else if (!errors.Is(err, os.ErrNotExist)) {
            t.Fatalf("failed to stat local bogo dir (%q): %v", localBogoDir, err);
        }
    }

    testenv.MustHaveExecPath(new tls_test_package.testing_TжTB(t), gitˢ);

    nint idx = strings.LastIndex(boringsslModVer, "-");
    if (idx == -1 || idx == len(boringsslModVer) - 1) {
        t.Fatalf("invalid boringsslModVer format: %q", boringsslModVer);
    }
    @string commitSHA = boringsslModVer.slice(idx + 1);

    t.Logf("cloning boringssl@%s to %q", commitSHA, localBogoDir);
    var cloneCmd = testenv.Command(new tls_test_package.testing_TжTB(t), gitˢ, cloneˢ, noCheckoutˢ, httpsBoringsslˢ, localBogoDir);
    {
        var err = cloneCmd.Run(); if (err != nil) {
            t.Fatalf("git clone failed: %v", err);
        }
    }

    var checkoutCmd = testenv.Command(new tls_test_package.testing_TжTB(t), gitˢ, checkoutˢ, commitSHA);
    checkoutCmd.Dir = localBogoDir;
    {
        var err = checkoutCmd.Run(); if (err != nil) {
            t.Fatalf("git checkout failed: %v", err);
        }
    }

    t.Logf("using fresh local bogo checkout from %q", localBogoDir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reportˢ => ref reportˢᵇ;
private static readonly @string reportˢᵇ = "report"u8;

internal static error generateReport(bogoResults results, @string outPath) {
    GoFrame ᒐ = default;
    try {
        var data = new reportData(
            Results: results,
            Timestamp: time_package.Unix((int64)results.SecondsSinceEpoch, 0).Format("2006-01-02 15:04:05"),
            Revision: boringsslModVer
        );

        var (ᴛ1, ᴛ2) = template.New(reportˢ).Parse(reportTemplate);
        var tmpl = template.Must(ᴛ1, ᴛ2);
        var (@file, err) = os.Create(outPath);
        if (err != nil) {
            return err;
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);

        return tmpl.Execute(new os.FileжWriter(@file), data);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public partial struct bogoResults_Tests /*dyn*/ {
    public @string Actual; /*`json:"actual"`*/
    public @string Expected; /*`json:"expected"`*/
    public bool IsUnexpected; /*`json:"is_unexpected"`*/
    public @string Error; /*`json:"error,omitempty"`*/
}

// bogoResults is a copy of boringssl.googlesource.com/boringssl/testresults.Results
public partial struct bogoResults {
    public nint Version; /*`json:"version"`*/
    public bool Interrupted; /*`json:"interrupted"`*/
    public @string PathDelimiter; /*`json:"path_delimiter"`*/
    public float64 SecondsSinceEpoch; /*`json:"seconds_since_epoch"`*/
    public map<@string, nint> NumFailuresByType; /*`json:"num_failures_by_type"`*/
    public map<@string, bogoResults_Tests> Tests; /*`json:"tests"`*/
}

internal partial struct reportData {
    public bogoResults Results;
    public map<@string, @string> SkipReasons;
    public @string Timestamp;
    public @string Revision;
}

internal static ref readonly @string reportTemplate => ref reportTemplateᵇ;
private static readonly @string reportTemplateᵇ = """

<!DOCTYPE html>
<html>
<head>
    <title>BoGo Results Report</title>
    <style>
        body { font-family: monospace; margin: 20px; }
        .summary { background: #f5f5f5; padding: 10px; margin-bottom: 20px; }
        .controls { margin-bottom: 10px; }
        .controls input, select { margin-right: 10px; }
        table { width: 100%; border-collapse: collapse; table-layout: fixed; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; vertical-align: top; }
        th { background-color: #f2f2f2; cursor: pointer; }
        .name-col { width: 30%; }
        .status-col { width: 8%; }
        .actual-col { width: 8%; }
        .expected-col { width: 8%; }
        .error-col { width: 26%; }
        .PASS { background-color: #d4edda; }
        .FAIL { background-color: #f8d7da; }
        .SKIP { background-color: #fff3cd; }
        .error {
            font-family: monospace;
            font-size: 0.9em;
            color: #721c24;
            white-space: pre-wrap;
            word-break: break-word;
        }
    </style>
</head>
<body>
<h1>BoGo Results Report</h1>

<div class="summary">
    <strong>Generated:</strong> {{.Timestamp}} | <strong>BoGo Revision:</strong> {{.Revision}}<br>
    {{range $status, $count := .Results.NumFailuresByType}}
    <strong>{{$status}}:</strong> {{$count}} |
    {{end}}
</div>

<div class="controls">
    <input type="text" id="search" placeholder="Search tests..." onkeyup="filterTests()">
    <select id="statusFilter" onchange="filterTests()">
        <option value="">All</option>
        <option value="FAIL">Failed</option>
        <option value="PASS">Passed</option>
        <option value="SKIP">Skipped</option>
    </select>
</div>

<table id="resultsTable">
    <thead>
    <tr>
        <th class="name-col" onclick="sortBy('name')">Test Name</th>
        <th class="status-col" onclick="sortBy('status')">Status</th>
        <th class="actual-col" onclick="sortBy('actual')">Actual</th>
        <th class="expected-col" onclick="sortBy('expected')">Expected</th>
        <th class="error-col">Error</th>
    </tr>
    </thead>
    <tbody>
    {{range $name, $test := .Results.Tests}}
    <tr class="{{$test.Actual}}" data-name="{{$name}}" data-status="{{$test.Actual}}">
        <td>{{$name}}</td>
        <td>{{$test.Actual}}</td>
        <td>{{$test.Actual}}</td>
        <td>{{$test.Expected}}</td>
        <td class="error">{{$test.Error}}</td>
    </tr>
    {{end}}
    </tbody>
</table>

<script>
    function filterTests() {
        const search = document.getElementById('search').value.toLowerCase();
        const status = document.getElementById('statusFilter').value;
        const rows = document.querySelectorAll('#resultsTable tbody tr');

        rows.forEach(row => {
            const name = row.dataset.name.toLowerCase();
            const rowStatus = row.dataset.status;
            const matchesSearch = name.includes(search);
            const matchesStatus = !status || rowStatus === status;

            row.style.display = matchesSearch && matchesStatus ? '' : 'none';
        });
    }

    function sortBy(column) {
        const tbody = document.querySelector('#resultsTable tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));

        rows.sort((a, b) => {
            if (column === 'status') {
                const statusOrder = {'FAIL': 0, 'PASS': 1, 'SKIP': 2};
                const aStatus = a.dataset.status;
                const bStatus = b.dataset.status;
                if (aStatus !== bStatus) {
                    return statusOrder[aStatus] - statusOrder[bStatus];
                }
                return a.dataset.name.localeCompare(b.dataset.name);
            } else {
                return a.dataset.name.localeCompare(b.dataset.name);
            }
        });

        rows.forEach(row => tbody.appendChild(row));
        filterTests();
    }

    sortBy("status");
</script>
</body>
</html>

"""u8.Raw();

} // end tls_internal_test_package
