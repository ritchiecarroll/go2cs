// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using pkix = go.crypto.x509.pkix_package;
using base64 = encoding.base64_package;
using hex = encoding.hex_package;
using pem = encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using byteorder = go.@internal.byteorder_package;
using io = io_package;
using big = go.math.big_package;
using net = net_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using container;
using crypto = crypto_package;
using encoding;
using fs = go.io.fs_package;
using go.@internal;
using go.crypto;
using go.crypto.tls.@internal;
using go.crypto.x509;
using go.math;
using go.os;
using list = container.list_package;
using path;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

// Note: see comment in handshake_test.go for details of how the reference
// tests work.

// opensslInputEvent enumerates possible inputs that can be sent to an `openssl
// s_client` process.
internal partial struct opensslInputEvent /*num:nint*/;

// opensslRenegotiate causes OpenSSL to request a renegotiation of the
// connection.
internal static opensslInputEvent opensslRenegotiate => /* iota */ 0;

// opensslSendBanner causes OpenSSL to send the contents of
// opensslSentinel on the connection.
internal static opensslInputEvent opensslSendSentinel => 1;

// opensslKeyUpdate causes OpenSSL to send a key update message to the
// client and request one back.
internal static opensslInputEvent opensslKeyUpdate => 2;

internal static ref readonly @string opensslSentinel => ref opensslSentinelᵇ;
private static readonly @string opensslSentinelᵇ = "SENTINEL\n"u8;

internal partial struct opensslInput /*chan opensslInputEvent*/;

internal static (nint n, error err) Read(this opensslInput i, slice<byte> buf) {
    foreach (var @event in i) {
        switch (@event) {
        case var ᴛ when ᴛ == opensslRenegotiate: {
            return (copy(buf, slice<byte>("R\n"u8)), default);
        }
        case var ᴛ when ᴛ == opensslKeyUpdate: {
            return (copy(buf, slice<byte>("K\n"u8)), default);
        }
        case var ᴛ when ᴛ == opensslSendSentinel: {
            return (copy(buf, slice<byte>(opensslSentinel)), default);
        }
        default: {
            throw panic("unknown event");
            break;
        }}
    }

    return (0, io.EOF);
}

// opensslOutputSink is an io.Writer that receives the stdout and stderr from an
// `openssl` process and sends a value to handshakeComplete or readKeyUpdate
// when certain messages are seen.
internal partial struct opensslOutputSink {
    internal channel<EmptyStruct> handshakeComplete;
    internal channel<EmptyStruct> readKeyUpdate;
    internal slice<byte> all;
    internal slice<byte> line;
}

internal static ж<opensslOutputSink> newOpensslOutputSink() {
    return Ꮡ(new opensslOutputSink(new channel<EmptyStruct>(0), new channel<EmptyStruct>(0), default, default));
}

// opensslEndOfHandshake is a message that the “openssl s_server” tool will
// print when a handshake completes if run with “-state”.
internal static ref readonly @string opensslEndOfHandshake => ref opensslEndOfHandshakeᵇ;
private static readonly @string opensslEndOfHandshakeᵇ = "SSL_accept:SSLv3/TLS write finished"u8;

// opensslReadKeyUpdate is a message that the “openssl s_server” tool will
// print when a KeyUpdate message is received if run with “-state”.
internal static ref readonly @string opensslReadKeyUpdate => ref opensslReadKeyUpdateᵇ;
private static readonly @string opensslReadKeyUpdateᵇ = "SSL_accept:TLSv1.3 read client key update"u8;

internal static (nint n, error err) Write(this ref opensslOutputSink o, slice<byte> data) {
    o.line = appendꓸꓸꓸ(o.line, data);
    o.all = appendꓸꓸꓸ(o.all, data);

    for (;;) {
        var (line, next, ok) = bytes.Cut(o.line, slice<byte>("\n"u8));
        if (!ok) {
            break;
        }

        if (bytes.Equal(slice<byte>(opensslEndOfHandshake), line)) {
            o.handshakeComplete.ᐸꟷ(new EmptyStruct());
        }
        if (bytes.Equal(slice<byte>(opensslReadKeyUpdate), line)) {
            o.readKeyUpdate.ᐸꟷ(new EmptyStruct());
        }
        o.line = next;
    }

    return (len(data), default);
}

internal static @string String(this ref opensslOutputSink o) {
    return (@string)o.all;
}

// clientTest represents a test of the TLS client handshake against a reference
// implementation.
internal partial struct clientTest {
    // name is a freeform string identifying the test and the file in which
    // the expected results will be stored.
    internal @string name;
    // args, if not empty, contains a series of arguments for the
    // command to run for the reference server.
    internal slice<@string> args;
    // config, if not nil, contains a custom Config to use for this test.
    internal ж<global::go.crypto.tls_package.Config> config;
    // cert, if not empty, contains a DER-encoded certificate for the
    // reference server.
    internal slice<byte> cert;
    // key, if not nil, contains either a *rsa.PrivateKey, ed25519.PrivateKey or
    // *ecdsa.PrivateKey which is the private key for the reference server.
    internal any key;
    // extensions, if not nil, contains a list of extension data to be returned
    // from the ServerHello. The data should be in standard TLS format with
    // a 2-byte uint16 type, 2-byte data length, followed by the extension data.
    internal slice<slice<byte>> extensions;
    // validate, if not nil, is a function that will be called with the
    // ConnectionState of the resulting connection. It returns a non-nil
    // error if the ConnectionState is unacceptable.
    internal Func<global::go.crypto.tls_package.ΔConnectionState, error> validate;
    // numRenegotiations is the number of times that the connection will be
    // renegotiated.
    internal nint numRenegotiations;
    // renegotiationExpectedToFail, if not zero, is the number of the
    // renegotiation attempt that is expected to fail.
    internal nint renegotiationExpectedToFail;
    // checkRenegotiationError, if not nil, is called with any error
    // arising from renegotiation. It can map expected errors to nil to
    // ignore them.
    internal Func<nint, error, error> checkRenegotiationError;
    // sendKeyUpdate will cause the server to send a KeyUpdate message.
    internal bool sendKeyUpdate;
}

internal static slice<@string> serverCommand = slice<@string>(["openssl", "s_server", "-no_ticket", "-num_tickets", "0"]);

// connFromCommand starts the reference server process, connects to it and
// returns a recordingConn for the connection. The stdin return value is an
// opensslInput for the stdin of the child process. It must be closed before
// Waiting for child.
internal static (ж<recordingConn> conn, ж<exec.Cmd> child, opensslInput stdin, ж<opensslOutputSink> stdout, error err) connFromCommand(this ж<clientTest> Ꮡtest) {
    ж<recordingConn> conn = default;
    ж<exec.Cmd> child = default;
    opensslInput stdin = default;
    ж<opensslOutputSink> stdout = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();
        var commandᴷ = new stackAppend<@string>();

        var cert = testRSACertificate;
        if (len(test.cert) > 0) {
            cert = test.cert;
        }
        @string certPath = tempFile((@string)cert);
        defer(os.Remove, certPath, ref ᒐ);

        any key = testRSAPrivateKey.OrTypedNil();
        if (test.key != nil) {
            key = test.key;
        }
        (var derBytes, err) = Δx509.MarshalPKCS8PrivateKey(key);
        if (err != nil) {
            throw panic(err);
        }

        ref var pemOut = ref heap(new bytes.Buffer(), out var ᏑpemOut);
        pem.Encode(new tls_test_package.bytes_BufferжWriter(ᏑpemOut), Ꮡ(new pem.Block(Type: "PRIVATE KEY", Bytes: derBytes)));

        @string keyPath = tempFile(ᏑpemOut.String());
        defer(os.Remove, keyPath, ref ᒐ);

        slice<@string> command = default;
        command = appendꓸꓸꓸ(command, serverCommand);
        command = appendꓸꓸꓸ(command, test.args);
        command = append(commandᴷ, command, (@string)"-cert", certPath, (@string)"-certform", (@string)"DER", (@string)"-key", keyPath);
        // serverPort contains the port that OpenSSL will listen on. OpenSSL
        // can't take "0" as an argument here so we have to pick a number and
        // hope that it's not in use on the machine. Since this only occurs
        // when -update is given and thus when there's a human watching the
        // test, this isn't too bad.
        const nint serverPort = 24323;
        command = append(command, (@string)"-accept", strconv.Itoa(serverPort));

        if (len(test.extensions) > 0) {
            ref var serverInfo = ref heap(new bytes.Buffer(), out var ᏑserverInfo);
            foreach (var (_, ext) in test.extensions) {
                pem.Encode(new tls_test_package.bytes_BufferжWriter(ᏑserverInfo), Ꮡ(new pem.Block(
                    Type: fmt.Sprintf("SERVERINFO FOR EXTENSION %d"u8, byteorder.BEUint16(ext)),
                    Bytes: ext
                )));
            }
            @string serverInfoPath = tempFile(ᏑserverInfo.String());
            defer(os.Remove, serverInfoPath, ref ᒐ);
            command = append(command, (@string)"-serverinfo", serverInfoPath);
        }

        if (test.numRenegotiations > 0 || test.sendKeyUpdate) {
            var found = false;
            foreach (var (_, flag) in command[1..]) {
                if (flag == "-state"u8) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                throw panic("-state flag missing to OpenSSL, you need this if testing renegotiation or KeyUpdate");
            }
        }

        var cmd = exec.Command(command[0], command[1..].ꓸꓸꓸ);
        stdin = (opensslInput)new channel<opensslInputEvent>(0);
        cmd.Stdin = stdin;
        var @out = newOpensslOutputSink();
        cmd.Stdout = new tls_internal_test_package.opensslOutputSinkжWriter(@out);
        cmd.Stderr = new tls_internal_test_package.opensslOutputSinkжWriter(@out);
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
                (conn, child, stdin, stdout, err) = (default, default, default, default, errΔ1); goto ᒐdone;
            }
        }

        // OpenSSL does print an "ACCEPT" banner, but it does so *before*
        // opening the listening socket, so we can't use that to wait until it
        // has started listening. Thus we are forced to poll until we get a
        // connection.
        net.Conn tcpConn = default;
        for (nuint i = (nuint)0; i < 5; i++) {
            var (ᴛ1, ᴛ2) = net.DialTCP(tcpˢ, nil, Ꮡ(new net.TCPAddr(
                IP: net.IPv4(127, 0, 0, 1),
                Port: serverPort
            )));
            (tcpConn, err) = (new net.TCPConnжConn(ᴛ1), ᴛ2);
            if (err == nil) {
                break;
            }
            time_package.Sleep(((time.Duration)((int64)1 << (int)(i))) * 5 * time_package.Millisecond);
        }
        if (err != nil) {
            close<opensslInputEvent>(stdin);
            cmd.Process.Kill();
            err = fmt.Errorf("error connecting to the OpenSSL server: %v (%v)\n\n%s"u8, err, cmd.Wait(), @out.OrTypedNil());
            (conn, child, stdin, stdout, err) = (default, default, default, default, err); goto ᒐdone;
        }

        var record = Ꮡ(new recordingConn(
            Conn: tcpConn
        ));

        (conn, child, stdin, stdout, err) = (record, cmd, stdin, @out, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (conn, child, stdin, stdout, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;

internal static @string dataPath(this ref clientTest test) {
    return filepath.Join(testdataˢ, "Client-" + test.name);
}

internal static (slice<slice<byte>> flows, error err) loadData(this ж<clientTest> Ꮡtest) {
    slice<slice<byte>> flows = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        (var @in, err) = os.Open(test.dataPath());
        if (err != nil) {
            (flows, err) = (default, err); goto ᒐdone;
        }
        var inʗ1 = @in;
        defer(() => inʗ1.Close(), ref ᒐ);
        (flows, err) = parseTestData(new tls_test_package.os_FileжReader(@in));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (flows, err);
}

internal static partial void run(this ж<clientTest> Ꮡtest, ж<testing.T> t, bool write) {
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        net.Conn clientConn = default;
        ж<recordingConn> recordingConn = default;
        ж<exec.Cmd> childProcess = default;
        opensslInput stdin = default;
        ж<opensslOutputSink> stdout = default;

        if (write) {
            error err = default;
            (recordingConn, childProcess, stdin, stdout, err) = Ꮡtest.connFromCommand();
            if (err != nil) {
                t.Fatalf("Failed to start subcommand: %s", err);
            }
            clientConn = new tls_internal_test_package.recordingConnжConn(recordingConn);
            var stdoutʗ1 = stdout;
            defer(() => {
                if (t.Failed()) {
                    t.Logf("OpenSSL output:\n\n%s", stdoutʗ1.all);
                }
            }, ref ᒐ);
        } else {
            var (flows, err) = Ꮡtest.loadData();
            if (err != nil) {
                t.Fatalf("failed to load data from %s: %v", test.dataPath(), err);
            }
            clientConn = new tls_internal_test_package.replayingConnжConn(Ꮡ(new replayingConn(t: new tls_test_package.testing_TжTB(t), flows: flows, reading: false)));
        }

        var config = test.config;
        if (config == nil) {
            config = testConfig;
        }
        var client = Client(clientConn, config);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        {
            var (_, err) = client.Write(slice<byte>("hello\n"u8)); if (err != nil) {
                t.Errorf("Client.Write failed: %s", err);
                return;
            }
        }

        for (nint iᴛ1 = 1; iᴛ1 <= test.numRenegotiations; iᴛ1++) {
            var i = iᴛ1;
            // The initial handshake will generate a
            // handshakeComplete signal which needs to be quashed.
            if (i == 1 && write) {
                ᐸꟷ(stdout.handshakeComplete);
            }

            // OpenSSL will try to interleave application data and
            // a renegotiation if we send both concurrently.
            // Therefore: ask OpensSSL to start a renegotiation, run
            // a goroutine to call client.Read and thus process the
            // renegotiation request, watch for OpenSSL's stdout to
            // indicate that the handshake is complete and,
            // finally, have OpenSSL write something to cause
            // client.Read to complete.
            if (write) {
                stdin.ᐸꟷ(opensslRenegotiate);
            }

            var signalChan = new channel<EmptyStruct>(0);

            var clientʗ2 = client;
            var signalChanʗ1 = signalChan;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => close(ᴛ1), signalChanʗ1, ref ᒐ);

                    var buf = new slice<byte>(256);
                    var (n, err) = clientʗ2.Read(buf);

                    if (Ꮡtest.Value.checkRenegotiationError != nil) {
                        var newErr = Ꮡtest.Value.checkRenegotiationError(i, err);
                        if (err != nil && newErr == nil) {
                            return;
                        }
                        err = newErr;
                    }

                    if (err != nil) {
                        t.Errorf("Client.Read failed after renegotiation #%d: %s", i, err);
                        return;
                    }

                    buf = buf.slice(0, n);
                    if (!bytes.Equal(slice<byte>(opensslSentinel), buf)) {
                        t.Errorf("Client.Read returned %q, but wanted %q", (@string)buf, opensslSentinel);
                    }

                    {
                        nint expected = i + 1; if (clientʗ2.handshakes != expected) {
                            t.Errorf("client should have recorded %d handshakes, but believes that %d have occurred", expected, clientʗ2.handshakes);
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });

            if (write && test.renegotiationExpectedToFail != i) {
                ᐸꟷ(stdout.handshakeComplete);
                stdin.ᐸꟷ(opensslSendSentinel);
            }
            ᐸꟷ(signalChan);
        }

        if (test.sendKeyUpdate) {
            if (write) {
                ᐸꟷ(stdout.handshakeComplete);
                stdin.ᐸꟷ(opensslKeyUpdate);
            }

            var doneRead = new channel<EmptyStruct>(0);

            var clientʗ3 = client;
            var doneReadʗ1 = doneRead;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => close(ᴛ1), doneReadʗ1, ref ᒐ);

                    var buf = new slice<byte>(256);
                    var (n, err) = clientʗ3.Read(buf);

                    if (err != nil) {
                        t.Errorf("Client.Read failed after KeyUpdate: %s", err);
                        return;
                    }

                    buf = buf.slice(0, n);
                    if (!bytes.Equal(slice<byte>(opensslSentinel), buf)) {
                        t.Errorf("Client.Read returned %q, but wanted %q", (@string)buf, opensslSentinel);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });

            if (write) {
                // There's no real reason to wait for the client KeyUpdate to
                // send data with the new server keys, except that s_server
                // drops writes if they are sent at the wrong time.
                ᐸꟷ(stdout.readKeyUpdate);
                stdin.ᐸꟷ(opensslSendSentinel);
            }
            ᐸꟷ(doneRead);

            {
                var (_, err) = client.Write(slice<byte>("hello again\n"u8)); if (err != nil) {
                    t.Errorf("Client.Write failed: %s", err);
                    return;
                }
            }
        }

        if (test.validate != nil) {
            {
                var err = test.validate(client.ConnectionState()); if (err != nil) {
                    t.Errorf("validate callback returned error: %s", err);
                }
            }
        }

        // If the server sent us an alert after our last flight, give it a
        // chance to arrive.
        if (write && test.renegotiationExpectedToFail == 0) {
            {
                var err = peekError(new global::go.crypto.tls_package.ConnжConn(client)); if (err != nil) {
                    t.Errorf("final Read returned an error: %s", err);
                }
            }
        }

        if (write) {
            client.Close();
            @string path = test.dataPath();
            var (@out, err) = os.OpenFile(path, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 420);
            if (err != nil) {
                t.Fatalf("Failed to create output file: %s", err);
            }
            var outʗ1 = @out;
            defer(() => outʗ1.Close(), ref ᒐ);
            (~recordingConn).Conn.Close();
            close<opensslInputEvent>(stdin);
            childProcess.Process.Kill();
            childProcess.Wait();
            if (len(recordingConn.flows) < 3) {
                t.Fatalf("Client connection didn't work");
            }
            recordingConn.WriteTo(new os.FileжWriter(@out));
            t.Logf("Wrote %s\n", path);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedlyReadDataˢ => ref unexpectedlyReadDataˢᵇ;
private static readonly @string unexpectedlyReadDataˢᵇ = "unexpectedly read data"u8;

// peekError does a read with a short timeout to check if the next read would
// cause an error, for example if there is an alert waiting on the wire.
internal static error peekError(net.Conn conn) {
    conn.SetReadDeadline(time_package.Now().Add(100 * time_package.Millisecond));
    {
        var (n, err) = conn.Read(new slice<byte>(1)); if (n != 0) {
            return errors.New(unexpectedlyReadDataˢ);
        } else if (err != nil) {
            {
                var (netErr, ok) = err._<netꓸError>(ᐧ); if (!ok || !netErr.Timeout()) {
                    return err;
                }
            }
        }
    }
    return default;
}

internal static void runClientTestForVersion(ж<testing.T> t, ж<clientTest> Ꮡtemplate, @string version, @string option) {
    ref var template = ref Ꮡtemplate.DerefOrNull();

    // Make a deep copy of the template before going parallel.
    ref var test = ref heap<clientTest>(template, out var Ꮡtest);
    if (template.config != nil) {
        test.config = template.config.Clone();
    }
    test.name = version + "-"u8 + test.name;
    test.args = appendꓸꓸꓸ(slice<@string>([option]), test.args);

    runTestAndUpdateIfNeeded(t, version, Ꮡtest.run, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlSv10ˢ => ref tlSv10ˢᵇ;
private static readonly @string tlSv10ˢᵇ = "TLSv10"u8;
internal static ref readonly @string tls1ˢ => ref tls1ˢᵇ;
private static readonly @string tls1ˢᵇ = "-tls1"u8;

internal static void runClientTestTLS10(ж<testing.T> t, ж<clientTest> template) {
    runClientTestForVersion(t, template, tlSv10ˢ, tls1ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlSv11ˢ => ref tlSv11ˢᵇ;
private static readonly @string tlSv11ˢᵇ = "TLSv11"u8;
internal static ref readonly @string tls11ˢ2 => ref tls11ˢ2ᵇ;
private static readonly @string tls11ˢ2ᵇ = "-tls1_1"u8;

internal static void runClientTestTLS11(ж<testing.T> t, ж<clientTest> template) {
    runClientTestForVersion(t, template, tlSv11ˢ, tls11ˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlSv12ˢ => ref tlSv12ˢᵇ;
private static readonly @string tlSv12ˢᵇ = "TLSv12"u8;
internal static ref readonly @string tls12ˢ2 => ref tls12ˢ2ᵇ;
private static readonly @string tls12ˢ2ᵇ = "-tls1_2"u8;

internal static void runClientTestTLS12(ж<testing.T> t, ж<clientTest> template) {
    runClientTestForVersion(t, template, tlSv12ˢ, tls12ˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlSv13ˢ => ref tlSv13ˢᵇ;
private static readonly @string tlSv13ˢᵇ = "TLSv13"u8;
internal static ref readonly @string tls13ˢ2 => ref tls13ˢ2ᵇ;
private static readonly @string tls13ˢ2ᵇ = "-tls1_3"u8;

internal static void runClientTestTLS13(ж<testing.T> t, ж<clientTest> template) {
    runClientTestForVersion(t, template, tlSv13ˢ, tls13ˢ2);
}

public static void TestHandshakeClientRSARC4(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "RSA-RC4",
        args: slice<@string>(["-cipher", "RC4-SHA"])
    ));
    runClientTestTLS10(t, test);
    runClientTestTLS11(t, test);
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientRSAAES128GCM(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "AES128-GCM-SHA256",
        args: slice<@string>(["-cipher", "AES128-GCM-SHA256"])
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientRSAAES256GCM(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "AES256-GCM-SHA384",
        args: slice<@string>(["-cipher", "AES256-GCM-SHA384"])
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientECDHERSAAES(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-RSA-AES",
        args: slice<@string>(["-cipher", "ECDHE-RSA-AES128-SHA"])
    ));
    runClientTestTLS10(t, test);
    runClientTestTLS11(t, test);
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientECDHEECDSAAES(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-AES128-SHA"]),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS10(t, test);
    runClientTestTLS11(t, test);
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientECDHEECDSAAESGCM(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES-GCM",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-AES128-GCM-SHA256"]),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientAES256GCMSHA384(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES256-GCM-SHA384",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-AES256-GCM-SHA384"]),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientAES128CBCSHA256(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "AES128-SHA256",
        args: slice<@string>(["-cipher", "AES128-SHA256"])
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientECDHERSAAES128CBCSHA256(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-RSA-AES128-SHA256",
        args: slice<@string>(["-cipher", "ECDHE-RSA-AES128-SHA256"])
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientECDHEECDSAAES128CBCSHA256(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES128-SHA256",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-AES128-SHA256"]),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientX25519(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.CurvePreferences = slice<global::go.crypto.tls_package.CurveID>([X25519]);

    var test = Ꮡ(new clientTest(
        name: "X25519-ECDHE",
        args: slice<@string>(["-cipher", "ECDHE-RSA-AES128-GCM-SHA256", "-curves", "X25519"]),
        config: config
    ));

    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientP256(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.CurvePreferences = slice<global::go.crypto.tls_package.CurveID>([CurveP256]);

    var test = Ꮡ(new clientTest(
        name: "P256-ECDHE",
        args: slice<@string>(["-cipher", "ECDHE-RSA-AES128-GCM-SHA256", "-curves", "P-256"]),
        config: config
    ));

    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string expectedˢ => ref expectedˢᵇ;
private static readonly @string expectedˢᵇ = "expected HelloRetryRequest"u8;

public static void TestHandshakeClientHelloRetryRequest(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.CurvePreferences = slice<global::go.crypto.tls_package.CurveID>([X25519, CurveP256]);

    var test = Ꮡ(new clientTest(
        name: "HelloRetryRequest",
        args: slice<@string>(["-cipher", "ECDHE-RSA-AES128-GCM-SHA256", "-curves", "P-256"]),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState cs) => {
            if (!cs.HelloRetryRequest) {
                return errors.New(expectedˢ);
            }
            return default;
        }
    ));

    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientECDHERSAChaCha20(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.CipherSuites = slice<uint16>([TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256]);

    var test = Ꮡ(new clientTest(
        name: "ECDHE-RSA-CHACHA20-POLY1305",
        args: slice<@string>(["-cipher", "ECDHE-RSA-CHACHA20-POLY1305"]),
        config: config
    ));

    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientECDHEECDSAChaCha20(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.CipherSuites = slice<uint16>([TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256]);

    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-CHACHA20-POLY1305",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-CHACHA20-POLY1305"]),
        config: config,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));

    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientAES128SHA256(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "AES128-SHA256",
        args: slice<@string>(["-ciphersuites", "TLS_AES_128_GCM_SHA256"])
    ));
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientAES256SHA384(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "AES256-SHA384",
        args: slice<@string>(["-ciphersuites", "TLS_AES_256_GCM_SHA384"])
    ));
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientCHACHA20SHA256(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "CHACHA20-SHA256",
        args: slice<@string>(["-ciphersuites", "TLS_CHACHA20_POLY1305_SHA256"])
    ));
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientECDSATLS13(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ECDSA",
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientEd25519(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "Ed25519",
        cert: testEd25519Certificate,
        key: testEd25519PrivateKey
    ));
    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);

    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientEd25519CertificatePEM), slice<byte>(clientEd25519KeyPEM));
    config.Certificates = slice<global::go.crypto.tls_package.Certificate>([cert]);

    test = Ꮡ(new clientTest(
        name: "ClientCert-Ed25519",
        args: slice<@string>(["-Verify", "1"]),
        config: config
    ));

    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientCertRSA(ж<testing.T> t) {
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientCertificatePEM), slice<byte>(clientKeyPEM));
    config.Certificates = slice<global::go.crypto.tls_package.Certificate>([cert]);

    var test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-RSA",
        args: slice<@string>(["-cipher", "AES128", "-Verify", "1"]),
        config: config
    ));

    runClientTestTLS10(t, test);
    runClientTestTLS12(t, test);

    test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-ECDSA",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-AES128-SHA", "-Verify", "1"]),
        config: config,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));

    runClientTestTLS10(t, test);
    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);

    test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-AES256-GCM-SHA384",
        args: slice<@string>(["-cipher", "ECDHE-RSA-AES256-GCM-SHA384", "-Verify", "1"]),
        config: config,
        cert: testRSACertificate,
        key: testRSAPrivateKey.OrTypedNil()
    ));

    runClientTestTLS12(t, test);
}

public static void TestHandshakeClientCertECDSA(ж<testing.T> t) {
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientECDSACertificatePEM), slice<byte>(clientECDSAKeyPEM));
    config.Certificates = slice<global::go.crypto.tls_package.Certificate>([cert]);

    var test = Ꮡ(new clientTest(
        name: "ClientCert-ECDSA-RSA",
        args: slice<@string>(["-cipher", "AES128", "-Verify", "1"]),
        config: config
    ));

    runClientTestTLS10(t, test);
    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);

    test = Ꮡ(new clientTest(
        name: "ClientCert-ECDSA-ECDSA",
        args: slice<@string>(["-cipher", "ECDHE-ECDSA-AES128-SHA", "-Verify", "1"]),
        config: config,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));

    runClientTestTLS10(t, test);
    runClientTestTLS12(t, test);
}

// TestHandshakeClientCertRSAPSS tests rsa_pss_rsae_sha256 signatures from both
// client and server certificates. It also serves from both sides a certificate
// signed itself with RSA-PSS, mostly to check that crypto/x509 chain validation
// works.
public static void TestHandshakeClientCertRSAPSS(ж<testing.T> t) {
    var (cert, err) = Δx509.ParseCertificate(testRSAPSSCertificate);
    if (err != nil) {
        throw panic(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(cert);

    var config = testConfig.Clone();
    // Use GetClientCertificate to bypass the client certificate selection logic.
    config.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> _) => (Ꮡ(new Certificate(
            ΔCertificate: slice<slice<byte>>([testRSAPSSCertificate]),
            PrivateKey: testRSAPrivateKey.OrTypedNil()
        )), default);
    config.RootCAs = rootCAs;

    var test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-RSAPSS",
        args: slice<@string>(["-cipher", "AES128", "-Verify", "1", "-client_sigalgs",
            "rsa_pss_rsae_sha256", "-sigalgs", "rsa_pss_rsae_sha256"]),
        config: config,
        cert: testRSAPSSCertificate,
        key: testRSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);
}

public static void TestHandshakeClientCertRSAPKCS1v15(ж<testing.T> t) {
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientCertificatePEM), slice<byte>(clientKeyPEM));
    config.Certificates = slice<global::go.crypto.tls_package.Certificate>([cert]);

    var test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-RSAPKCS1v15",
        args: slice<@string>(["-cipher", "AES128", "-Verify", "1", "-client_sigalgs",
            "rsa_pkcs1_sha256", "-sigalgs", "rsa_pkcs1_sha256"]),
        config: config
    ));

    runClientTestTLS12(t, test);
}

public static void TestClientKeyUpdate(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "KeyUpdate",
        args: slice<@string>(["-state"]),
        sendKeyUpdate: true
    ));
    runClientTestTLS13(t, test);
}

public static void TestResumption(ж<testing.T> t) {
    t.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testResumption(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testResumption(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ2 = (@string)"skipping in -short mode"u8;
internal static ref readonly @string resumeˢ => ref resumeˢᵇ;
private static readonly @string resumeˢᵇ = "Resume"u8;
internal static readonly object ticketDidnTChangeAfterˢ = (@string)"ticket didn't change after resumption"u8;
internal static ref readonly @string resumeWithOldTicketˢ => ref resumeWithOldTicketˢᵇ;
private static readonly @string resumeWithOldTicketˢᵇ = "ResumeWithOldTicket"u8;
internal static readonly object oldFirstTicketMatchesTheˢ = (@string)"old first ticket matches the fresh one"u8;
internal static ref readonly @string resumeWithExpiredTicketˢ => ref resumeWithExpiredTicketˢᵇ;
private static readonly @string resumeWithExpiredTicketˢᵇ = "ResumeWithExpiredTicket"u8;
internal static readonly object expiredFirstTicketˢ = (@string)"expired first ticket matches the fresh one"u8;
internal static ref readonly @string invalidSessionTicketKeyˢ => ref invalidSessionTicketKeyˢᵇ;
private static readonly @string invalidSessionTicketKeyˢᵇ = "InvalidSessionTicketKey"u8;
internal static ref readonly @string keyChangeˢ => ref keyChangeˢᵇ;
private static readonly @string keyChangeˢᵇ = "KeyChange"u8;
internal static readonly object newTicketWasnTIncludedˢ = (@string)"new ticket wasn't included while resuming"u8;
internal static ref readonly @string keyChangeFinishˢ => ref keyChangeFinishˢᵇ;
private static readonly @string keyChangeFinishˢᵇ = "KeyChangeFinish"u8;
internal static ref readonly @string oldSessionTicketˢ => ref oldSessionTicketˢᵇ;
private static readonly @string oldSessionTicketˢᵇ = "OldSessionTicket"u8;
internal static ref readonly @string expiredSessionTicketˢ => ref expiredSessionTicketˢᵇ;
private static readonly @string expiredSessionTicketˢᵇ = "ExpiredSessionTicket"u8;
internal static readonly object newTicketWasnTProvidedˢ = (@string)"new ticket wasn't provided after old ticket expired"u8;
internal static ref readonly @string getFreshSessionTicketˢ => ref getFreshSessionTicketˢᵇ;
private static readonly @string getFreshSessionTicketˢᵇ = "GetFreshSessionTicket"u8;
internal static ref readonly @string freshConfigˢ => ref freshConfigˢᵇ;
private static readonly @string freshConfigˢᵇ = "FreshConfig"u8;
internal static ref readonly @string differentCipherSuiteˢ => ref differentCipherSuiteˢᵇ;
private static readonly @string differentCipherSuiteˢᵇ = "DifferentCipherSuite"u8;
internal static ref readonly @string withoutSessionTicketˢ => ref withoutSessionTicketˢᵇ;
private static readonly @string withoutSessionTicketˢᵇ = "WithoutSessionTicket"u8;
internal static ref readonly @string initialHandshakeˢ => ref initialHandshakeˢᵇ;
private static readonly @string initialHandshakeˢᵇ = "InitialHandshake"u8;
internal static ref readonly @string withHelloRetryRequestˢ => ref withHelloRetryRequestˢᵇ;
private static readonly @string withHelloRetryRequestˢᵇ = "WithHelloRetryRequest"u8;
internal static ref readonly @string withClientCertificatesˢ => ref withClientCertificatesˢᵇ;
private static readonly @string withClientCertificatesˢᵇ = "WithClientCertificates"u8;
internal static ref readonly @string fetchTicketToCorruptˢ => ref fetchTicketToCorruptˢᵇ;
private static readonly @string fetchTicketToCorruptˢᵇ = "FetchTicketToCorrupt"u8;
internal static ref readonly @string afterHandshakeFailureˢ => ref afterHandshakeFailureˢᵇ;
private static readonly @string afterHandshakeFailureˢᵇ = "AfterHandshakeFailure"u8;
internal static ref readonly @string withoutSessionCacheˢ => ref withoutSessionCacheˢᵇ;
private static readonly @string withoutSessionCacheˢᵇ = "WithoutSessionCache"u8;
internal static ref readonly @string beforeSerializingCacheˢ => ref beforeSerializingCacheˢᵇ;
private static readonly @string beforeSerializingCacheˢᵇ = "BeforeSerializingCache"u8;
internal static ref readonly @string withSerializingCacheˢ => ref withSerializingCacheˢᵇ;
private static readonly @string withSerializingCacheˢᵇ = "WithSerializingCache"u8;

internal static partial void testResumption(ж<testing.T> t, uint16 version) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ2);
    }

    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    var testCertificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]), PrivateKey: testRSA2048PrivateKey.OrTypedNil())]);
    ref var serverConfig = ref heap<ж<global::go.crypto.tls_package.Config>>(Ꮡ(new Config(
        MaxVersion: version,
        CipherSuites: slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384]),
        Certificates: testCertificates,
        Time: testTime
    )), out var ᏑserverConfig);

    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != nil) {
        throw panic(err);
    }

    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);

    var clientConfig = Ꮡ(new Config(
        MaxVersion: version,
        CipherSuites: slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256]),
        ClientSessionCache: NewLRUClientSessionCache(32),
        RootCAs: rootCAs,
        ServerName: "example.golang",
        Time: testTime
    ));

    var clientConfigʗ1 = clientConfig;
    void testResumeState(@string test, bool didResume) {
        t.Helper();
        var (_, hs, errΔ1) = testHandshake(t, clientConfigʗ1, ᏑserverConfig.ValueSlot);
        if (errΔ1 != nil) {
            t.Fatalf("%s: handshake failed: %s", test, errΔ1);
        }
        if (hs.DidResume != didResume) {
            t.Fatalf("%s resumed: %v, expected: %v", test, hs.DidResume, didResume);
        }
        if (didResume && (hs.PeerCertificates == nil || hs.VerifiedChains == nil)) {
            t.Fatalf("expected non-nil certificates after resumption. Got peerCertificates: %#v, verifiedCertificates: %#v", hs.PeerCertificates, hs.VerifiedChains);
        }
        {
            @string got = hs.ServerName;
            @string want = clientConfigʗ1.ServerName; if (got != want) {
                t.Errorf("%s: server name %s, want %s", test, got, want);
            }
        }
    }

    var clientConfigʗ2 = clientConfig;
    slice<byte> getTicket() => (~clientConfigʗ2.ClientSessionCache._<ж<global::go.crypto.tls_package.lruSessionCache>>().q.Front()).Value._<ж<global::go.crypto.tls_package.lruSessionCacheEntry>>().state.session.ticket;
    var clientConfigʗ3 = clientConfig;
    void deleteTicket() {
        @string ticketKey = (~clientConfigʗ3.ClientSessionCache._<ж<global::go.crypto.tls_package.lruSessionCache>>().q.Front()).Value._<ж<global::go.crypto.tls_package.lruSessionCacheEntry>>().sessionKey;
        clientConfigʗ3.ClientSessionCache.Put(ticketKey, nil);
    }
    var clientConfigʗ4 = clientConfig;
    void corruptTicket() {
        (~clientConfigʗ4.ClientSessionCache._<ж<global::go.crypto.tls_package.lruSessionCache>>().q.Front()).Value._<ж<global::go.crypto.tls_package.lruSessionCacheEntry>>().state.session.secret[0] ^= (byte)(0xff);
    }
    array<byte> randomKey() {
        array<byte> k = new(32);
        {
            var (_, errΔ2) = io.ReadFull(ᏑserverConfig.ValueSlot.rand(), k[..]); if (errΔ2 != nil) {
                t.Fatalf("Failed to read new SessionTicketKey: %s", errΔ2);
            }
        }
        return k.Clone();
    }

    testResumeState(handshakeˢ, false);
    var ticket = getTicket();
    testResumeState(resumeˢ, true);
    if (bytes.Equal(ticket, getTicket())) {
        t.Fatal(ticketDidnTChangeAfterˢ);
    }

    // An old session ticket is replaced with a ticket encrypted with a fresh key.
    ticket = getTicket();
    serverConfig.Time = () => testTime().Add((time.Duration)(86460000000000L));
    testResumeState(resumeWithOldTicketˢ, true);
    if (bytes.Equal(ticket, getTicket())) {
        t.Fatal(oldFirstTicketMatchesTheˢ);
    }

    // Once the session master secret is expired, a full handshake should occur.
    ticket = getTicket();
    serverConfig.Time = () => testTime().Add((time.Duration)(691260000000000L));
    testResumeState(resumeWithExpiredTicketˢ, false);
    if (bytes.Equal(ticket, getTicket())) {
        t.Fatal(expiredFirstTicketˢ);
    }

    serverConfig.Time = testTime; // reset the time back
    var key1 = randomKey();
    serverConfig.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{key1.Clone()}.slice(), 32));

    testResumeState(invalidSessionTicketKeyˢ, false);
    testResumeState("ResumeAfterInvalidSessionTicketKey", true);

    var key2 = randomKey();
    serverConfig.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{key2.Clone(), key1.Clone()}.slice(), 32));
    ticket = getTicket();
    testResumeState(keyChangeˢ, true);
    if (bytes.Equal(ticket, getTicket())) {
        t.Fatal(newTicketWasnTIncludedˢ);
    }
    testResumeState(keyChangeFinishˢ, true);

    // Age the session ticket a bit, but not yet expired.
    serverConfig.Time = () => testTime().Add((time.Duration)(86460000000000L));
    testResumeState(oldSessionTicketˢ, true);
    ticket = getTicket();
    // Expire the session ticket, which would force a full handshake.
    serverConfig.Time = () => testTime().Add((time.Duration)(691320000000000L));
    testResumeState(expiredSessionTicketˢ, false);
    if (bytes.Equal(ticket, getTicket())) {
        t.Fatal(newTicketWasnTProvidedˢ);
    }

    // Age the session ticket a bit at a time, but don't expire it.
    var d = 0 * time_package.ΔHour;
    serverConfig.Time = () => testTime().Add(d);
    deleteTicket();
    testResumeState(getFreshSessionTicketˢ, false);
    for (nint i = 0; i < 13; i++) {
        d += (time.Duration)(43200000000000L);
        testResumeState(oldSessionTicketˢ, true);
    }
    // Expire it (now a little more than 7 days) and make sure a full
    // handshake occurs for TLS 1.2. Resumption should still occur for
    // TLS 1.3 since the client should be using a fresh ticket sent over
    // by the server.
    d += (time.Duration)(43260000000000L);
    if (version == VersionTLS13) {
        testResumeState(expiredSessionTicketˢ, true);
    } else {
        testResumeState(expiredSessionTicketˢ, false);
    }
    if (bytes.Equal(ticket, getTicket())) {
        t.Fatal(newTicketWasnTProvidedˢ);
    }

    // Reset serverConfig to ensure that calling SetSessionTicketKeys
    // before the serverConfig is used works.
    serverConfig = Ꮡ(new Config(
        MaxVersion: version,
        CipherSuites: slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384]),
        Certificates: testCertificates,
        Time: testTime
    ));
    serverConfig.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{key2.Clone()}.slice(), 32));

    testResumeState(freshConfigˢ, true);

    // In TLS 1.3, cross-cipher suite resumption is allowed as long as the KDF
    // hash matches. Also, Config.CipherSuites does not apply to TLS 1.3.
    if (version != VersionTLS13) {
        clientConfig.CipherSuites = slice<uint16>([TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384]);
        testResumeState(differentCipherSuiteˢ, false);
        testResumeState("DifferentCipherSuiteRecovers", true);
    }

    deleteTicket();
    testResumeState(withoutSessionTicketˢ, false);

    // In TLS 1.3, HelloRetryRequest is sent after incorrect key share.
    // See https://www.rfc-editor.org/rfc/rfc8446#page-14.
    if (version == VersionTLS13) {
        deleteTicket();
        serverConfig = Ꮡ(new Config(
            // Use a different curve than the client to force a HelloRetryRequest.
            CurvePreferences: slice<global::go.crypto.tls_package.CurveID>([CurveP521, CurveP384, CurveP256]),
            MaxVersion: version,
            Certificates: testCertificates,
            Time: testTime
        ));
        testResumeState(initialHandshakeˢ, false);
        testResumeState(withHelloRetryRequestˢ, true);

        // Reset serverConfig back.
        serverConfig = Ꮡ(new Config(
            MaxVersion: version,
            CipherSuites: slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384]),
            Certificates: testCertificates,
            Time: testTime
        ));
    }

    // Session resumption should work when using client certificates
    deleteTicket();
    serverConfig.ClientCAs = rootCAs;
    serverConfig.ClientAuth = RequireAndVerifyClientCert;
    clientConfig.Certificates = serverConfig.Certificates;
    testResumeState(initialHandshakeˢ, false);
    testResumeState(withClientCertificatesˢ, true);
    serverConfig.ClientAuth = NoClientCert;

    // Tickets should be removed from the session cache on TLS handshake
    // failure, and the client should recover from a corrupted PSK
    testResumeState(fetchTicketToCorruptˢ, false);
    corruptTicket();
    (_, _, err) = testHandshake(t, clientConfig, serverConfig);
    if (err == nil) {
        t.Fatalf("handshake did not fail with a corrupted client secret");
    }
    testResumeState(afterHandshakeFailureˢ, false);

    clientConfig.ClientSessionCache = default;
    testResumeState(withoutSessionCacheˢ, false);

    clientConfig.ClientSessionCache = new tls_internal_test_package.serializingClientCacheжClientSessionCache(Ꮡ(new serializingClientCache(t: t)));
    testResumeState(beforeSerializingCacheˢ, false);
    testResumeState(withSerializingCacheˢ, true);
}

internal partial struct serializingClientCache {
    internal ж<testing.T> t;
    internal slice<byte> ticket, state;
}

internal static (ж<global::go.crypto.tls_package.ClientSessionState> session, bool ok) Get(this ref serializingClientCache c, @string sessionKey) {
    if (c.ticket == nil) {
        return (default, false);
    }
    var (state, err) = ParseSessionState(c.state);
    if (err != nil) {
        c.t.Error(err);
        return (default, false);
    }
    (var cs, err) = NewResumptionState(c.ticket, state);
    if (err != nil) {
        c.t.Error(err);
        return (default, false);
    }
    return (cs, true);
}

internal static void Put(this ref serializingClientCache c, @string sessionKey, ж<global::go.crypto.tls_package.ClientSessionState> cs) {
    if (cs == nil) {
        (c.ticket, c.state) = (default, default);
        return;
    }
    var (ticket, state, err) = cs.ResumptionState();
    if (err != nil) {
        c.t.Error(err);
        return;
    }
    (var stateBytes, err) = state.Bytes();
    if (err != nil) {
        c.t.Error(err);
        return;
    }
    (c.ticket, c.state) = (ticket, stateBytes);
}

public static void TestLRUClientSessionCache(ж<testing.T> t) {
    // Initialize cache of capacity 4.
    var cache = NewLRUClientSessionCache(4);
    var cs = new slice<global::go.crypto.tls_package.ClientSessionState>(6);
    var keys = slice<@string>(["0", "1", "2", "3", "4", "5", "6"]);

    // Add 4 entries to the cache and look them up.
    for (nint i = 0; i < 4; i++) {
        cache.Put(keys[i], Ꮡ(cs, i));
    }
    for (nint i = 0; i < 4; i++) {
        {
            var (s, ok) = cache.Get(keys[i]); if (!ok || s != Ꮡ(cs, i)) {
                t.Fatalf("session cache failed lookup for added key: %s", keys[i]);
            }
        }
    }

    // Add 2 more entries to the cache. First 2 should be evicted.
    for (nint i = 4; i < 6; i++) {
        cache.Put(keys[i], Ꮡ(cs, i));
    }
    for (nint i = 0; i < 2; i++) {
        {
            var (s, ok) = cache.Get(keys[i]); if (ok || s != nil) {
                t.Fatalf("session cache should have evicted key: %s", keys[i]);
            }
        }
    }

    // Touch entry 2. LRU should evict 3 next.
    cache.Get(keys[2]);
    cache.Put(keys[0], Ꮡ(cs, 0));
    {
        var (s, ok) = cache.Get(keys[3]); if (ok || s != nil) {
            t.Fatalf("session cache should have evicted key 3");
        }
    }

    // Update entry 0 in place.
    cache.Put(keys[0], Ꮡ(cs, 3));
    {
        var (s, ok) = cache.Get(keys[0]); if (!ok || s != Ꮡ(cs, 3)) {
            t.Fatalf("session cache failed update for key 0");
        }
    }

    // Calling Put with a nil entry deletes the key.
    cache.Put(keys[0], nil);
    {
        var (_, ok) = cache.Get(keys[0]); if (ok) {
            t.Fatalf("session cache failed to delete key 0");
        }
    }

    // Delete entry 2. LRU should keep 4 and 5
    cache.Put(keys[2], nil);
    {
        var (_, ok) = cache.Get(keys[2]); if (ok) {
            t.Fatalf("session cache failed to delete key 4");
        }
    }
    for (nint i = 4; i < 6; i++) {
        {
            var (s, ok) = cache.Get(keys[i]); if (!ok || s != Ꮡ(cs, i)) {
                t.Fatalf("session cache should not have deleted key: %s", keys[i]);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string clientˢ => ref clientˢᵇ;
private static readonly @string clientˢᵇ = "client"u8;
internal static ref readonly @string serverˢ => ref serverˢᵇ;
private static readonly @string serverˢᵇ = "server"u8;

public static partial void TestKeyLogTLS12(ж<testing.T> t) {
    ref var serverBuf = ref heap(new bytes.Buffer(), out var ᏑserverBuf);
    ref var clientBuf = ref heap(new bytes.Buffer(), out var ᏑclientBuf);

    var clientConfig = testConfig.Clone();
    clientConfig.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑclientBuf);
    clientConfig.MaxVersion = VersionTLS12;

    var serverConfig = testConfig.Clone();
    serverConfig.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑserverBuf);
    serverConfig.MaxVersion = VersionTLS12;

    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var done = new channel<bool>(0);

    var doneʗ1 = done;
    var sʗ1 = s;
    var serverConfigʗ1 = serverConfig;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);

            {
                var err = Server(sʗ1, serverConfigʗ1).Handshake(); if (err != nil) {
                    t.Errorf("server: %s", err);
                    return;
                }
            }
            sʗ1.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    {
        var err = Client(c, clientConfig).Handshake(); if (err != nil) {
            t.Fatalf("client: %s", err);
        }
    }

    c.Close();
    ᐸꟷ(done);

    void checkKeylogLine(@string side, @string loggedLine) {
        if (len(loggedLine) == 0) {
            t.Fatalf("%s: no keylog line was produced", side);
        }
/* "CLIENT_RANDOM" */
/* space */
/* hex client nonce */
/* space */
/* hex master secret */
        const nint expectedLen = /* 13 +
	1 +
	32*2 +
	1 +
	48*2 +
	1 */ 176; /* new line */
        if (len(loggedLine) != expectedLen) {
            t.Fatalf("%s: keylog line has incorrect length (want %d, got %d): %q", side, (nint)(expectedLen), len(loggedLine), loggedLine);
        }
        if (!strings.HasPrefix(loggedLine, "CLIENT_RANDOM "u8 + strings.Repeat("0", 64) + " "u8)) {
            t.Fatalf("%s: keylog line has incorrect structure or nonce: %q", side, loggedLine);
        }
    }

    checkKeylogLine(clientˢ, ᏑclientBuf.String());
    checkKeylogLine(serverˢ, ᏑserverBuf.String());
}

public static partial void TestKeyLogTLS13(ж<testing.T> t) {
    ref var serverBuf = ref heap(new bytes.Buffer(), out var ᏑserverBuf);
    ref var clientBuf = ref heap(new bytes.Buffer(), out var ᏑclientBuf);

    var clientConfig = testConfig.Clone();
    clientConfig.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑclientBuf);

    var serverConfig = testConfig.Clone();
    serverConfig.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑserverBuf);

    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var done = new channel<bool>(0);

    var doneʗ1 = done;
    var sʗ1 = s;
    var serverConfigʗ1 = serverConfig;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);

            {
                var err = Server(sʗ1, serverConfigʗ1).Handshake(); if (err != nil) {
                    t.Errorf("server: %s", err);
                    return;
                }
            }
            sʗ1.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    {
        var err = Client(c, clientConfig).Handshake(); if (err != nil) {
            t.Fatalf("client: %s", err);
        }
    }

    c.Close();
    ᐸꟷ(done);

    void checkKeylogLines(@string side, @string loggedLines) {
        loggedLines = strings.TrimSpace(loggedLines);
        var lines = strings.Split(loggedLines, "\n");
        if (len(lines) != 4) {
            t.Errorf("Expected the %s to log 4 lines, got %d", side, len(lines));
        }
    }

    checkKeylogLines(clientˢ, ᏑclientBuf.String());
    checkKeylogLines(serverˢ, ᏑserverBuf.String());
}

public static void TestHandshakeClientALPNMatch(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.NextProtos = slice<@string>(["proto2", "proto1"]);

    var test = Ꮡ(new clientTest(
        name: "ALPN",
        // Note that this needs OpenSSL 1.0.2 because that is the first
        // version that supports the -alpn flag.
        args: slice<@string>(["-alpn", "proto1,proto2"]),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            // The server's preferences should override the client.
            if (state.NegotiatedProtocol != "proto1"u8) {
                return fmt.Errorf("Got protocol %q, wanted proto1"u8, state.NegotiatedProtocol);
            }
            return default;
        }
    ));
    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string serverSelectedˢ => ref serverSelectedˢᵇ;
private static readonly @string serverSelectedˢᵇ = "server selected unadvertised ALPN protocol"u8;

public static partial void TestServerSelectingUnconfiguredApplicationProtocol(ж<testing.T> t) {
    // This checks that the server can't select an application protocol that the
    // client didn't offer.

    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var errChan = new channel<error>(1);

    var cʗ1 = c;
    var errChanʗ1 = errChan;
    goǃ(() => {
        var client = Client(cʗ1, Ꮡ(new Config(
            ServerName: "foo",
            CipherSuites: slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256]),
            NextProtos: slice<@string>(["http", "something-else"])
        )));
        errChanʗ1.ᐸꟷ(client.Handshake());
    });

    array<byte> header = new(5);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), header[..]); if (err != nil) {
            t.Fatal(err);
        }
    }
    nint recordLen = (nint)header[3] << 8 | (nint)header[4];

    var record = new slice<byte>(recordLen);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), record); if (err != nil) {
            t.Fatal(err);
        }
    }

    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuite: TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
        alpnProtocol: "how-about-this"
    ));
    var serverHelloBytes = mustMarshal(t, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));

    s.Write(slice<byte>([
        (byte)recordTypeHandshake,
        (byte)(((byte)VersionTLS12).Rsh((uint64)8)),
        (byte)((byte)((byte)VersionTLS12 & 0xff)),
        (byte)(len(serverHelloBytes) >> 8),
        (byte)len(serverHelloBytes)
    ]));
    s.Write(serverHelloBytes);
    s.Close();

    {
        var err = ᐸꟷ(errChan); if (!strings.Contains(err.Error(), serverSelectedˢ)) {
            t.Fatalf("Expected error about unconfigured ALPN protocol but got %q", err);
        }
    }
}

// sctsBase64 contains data from `openssl s_client -serverinfo 18 -connect ritter.vg:443`
internal static ref readonly @string sctsBase64 => ref sctsBase64ᵇ;
private static readonly @string sctsBase64ᵇ = "ABIBaQFnAHUApLkJkLQYWBSHuxOizGdwCjw1mAT5G9+443fNDsgN3BAAAAFHl5nuFgAABAMARjBEAiAcS4JdlW5nW9sElUv2zvQyPoZ6ejKrGGB03gjaBZFMLwIgc1Qbbn+hsH0RvObzhS+XZhr3iuQQJY8S9G85D9KeGPAAdgBo9pj4H2SCvjqM7rkoHUz8cVFdZ5PURNEKZ6y7T0/7xAAAAUeX4bVwAAAEAwBHMEUCIDIhFDgG2HIuADBkGuLobU5a4dlCHoJLliWJ1SYT05z6AiEAjxIoZFFPRNWMGGIjskOTMwXzQ1Wh2e7NxXE1kd1J0QsAdgDuS723dc5guuFCaR+r4Z5mow9+X7By2IMAxHuJeqj9ywAAAUhcZIqHAAAEAwBHMEUCICmJ1rBT09LpkbzxtUC+Hi7nXLR0J+2PmwLp+sJMuqK+AiEAr0NkUnEVKVhAkccIFpYDqHOlZaBsuEhWWrYpg2RtKp0="u8;

public static void TestHandshakClientSCTs(ж<testing.T> t) {
    var config = testConfig.Clone();

    var (scts, err) = base64.StdEncoding.DecodeString(sctsBase64);
    if (err != nil) {
        t.Fatal(err);
    }

    // Note that this needs OpenSSL 1.0.2 because that is the first
    // version that supports the -serverinfo flag.
        var sctsʗ1 = scts;
    var test = Ꮡ(new clientTest(
        name: "SCT",
        config: config,
        extensions: slice<slice<byte>>([scts]),
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            var expectedSCTs = slice<slice<byte>>([
                sctsʗ1[8..125],
                sctsʗ1[127..245],
                sctsʗ1[247..]
            ]);
            {
                nint n = len(state.SignedCertificateTimestamps); if (n != len(expectedSCTs)) {
                    return fmt.Errorf("Got %d scts, wanted %d"u8, n, len(expectedSCTs));
                }
            }
            foreach (var (i, expected) in expectedSCTs) {
                {
                    var sct = state.SignedCertificateTimestamps[i]; if (!bytes.Equal(sct, expected)) {
                        return fmt.Errorf("SCT #%d contained %x, expected %x"u8, i, sct, expected);
                    }
                }
            }
            return default;
        }
    ));
    runClientTestTLS12(t, test);

    // TLS 1.3 moved SCTs to the Certificate extensions and -serverinfo only
    // supports ServerHello extensions.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string expectedErrorFromˢ => ref expectedErrorFromˢᵇ;
private static readonly @string expectedErrorFromˢᵇ = "expected error from renegotiation but got nil"u8;
internal static ref readonly @string noRenegotiationˢ => ref noRenegotiationˢᵇ;
private static readonly @string noRenegotiationˢᵇ = "no renegotiation"u8;

public static void TestRenegotiationRejected(ж<testing.T> t) {
    var config = testConfig.Clone();
    var test = Ꮡ(new clientTest(
        name: "RenegotiationRejected",
        args: slice<@string>(["-state"]),
        config: config,
        numRenegotiations: 1,
        renegotiationExpectedToFail: 1,
        checkRenegotiationError: error (nint renegotiationNum, error err) => {
            if (err == nil) {
                return errors.New(expectedErrorFromˢ);
            }
            if (!strings.Contains(err.Error(), noRenegotiationˢ)) {
                return fmt.Errorf("expected renegotiation to be rejected but got %q"u8, err);
            }
            return default;
        }
    ));
    runClientTestTLS12(t, test);
}

public static void TestRenegotiateOnce(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.Renegotiation = RenegotiateOnceAsClient;

    var test = Ꮡ(new clientTest(
        name: "RenegotiateOnce",
        args: slice<@string>(["-state"]),
        config: config,
        numRenegotiations: 1
    ));

    runClientTestTLS12(t, test);
}

public static void TestRenegotiateTwice(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.Renegotiation = RenegotiateFreelyAsClient;

    var test = Ꮡ(new clientTest(
        name: "RenegotiateTwice",
        args: slice<@string>(["-state"]),
        config: config,
        numRenegotiations: 2
    ));

    runClientTestTLS12(t, test);
}

public static void TestRenegotiateTwiceRejected(ж<testing.T> t) {
    var config = testConfig.Clone();
    config.Renegotiation = RenegotiateOnceAsClient;

    var test = Ꮡ(new clientTest(
        name: "RenegotiateTwiceRejected",
        args: slice<@string>(["-state"]),
        config: config,
        numRenegotiations: 2,
        renegotiationExpectedToFail: 2,
        checkRenegotiationError: error (nint renegotiationNum, error err) => {
            if (renegotiationNum == 1) {
                return err;
            }

            if (err == nil) {
                return errors.New(expectedErrorFromˢ);
            }
            if (!strings.Contains(err.Error(), noRenegotiationˢ)) {
                return fmt.Errorf("expected renegotiation to be rejected but got %q"u8, err);
            }
            return default;
        }
    ));

    runClientTestTLS12(t, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;

public static void TestHandshakeClientExportKeyingMaterial(ж<testing.T> t) {
    var test = Ꮡ(new clientTest(
        name: "ExportKeyingMaterial",
        config: testConfig.Clone(),
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            {
                var (km, err) = state.ExportKeyingMaterial(testˢ, default, 42); if (err != nil) {
                    return fmt.Errorf("ExportKeyingMaterial failed: %v"u8, err);
                } else if (len(km) != 42) {
                    return fmt.Errorf("Got %d bytes from ExportKeyingMaterial, wanted %d"u8, len(km), (nint)42);
                }
            }
            return default;
        }
    ));
    runClientTestTLS10(t, test);
    runClientTestTLS12(t, test);
    runClientTestTLS13(t, test);
}

partial struct hostnameInSNITestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<hostnameInSNITestsᴛ1> hostnameInSNITests = slice<hostnameInSNITestsᴛ1>([
    // Opaque string
    new("", ""),
    new("localhost", "localhost"),
    new("foo, bar, baz and qux", "foo, bar, baz and qux"),

    // DNS hostname
    new("golang.org", "golang.org"),
    new("golang.org.", "golang.org"),

    // Literal IPv4 address
    new("1.2.3.4", ""),

    // Literal IPv6 address
    new("::1", ""),
    new("::1%lo0", ""), // with zone identifier
    new("[::1]", ""),   // as per RFC 5952 we allow the [] style as IPv6 literal
    new("[::1%lo0]", "")
]);

public static partial void TestHostnameInSNI(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in hostnameInSNITests) {
        ref var tt = ref heap(new hostnameInSNITestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));

        var cʗ1 = c;
        goǃ((@string host) => {
            Client(cʗ1, Ꮡ(new Config(ServerName: host, InsecureSkipVerify: true))).Handshake();
        }, tt.@in);

        array<byte> header = new(5);
        {
            var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), header[..]); if (err != nil) {
                t.Fatal(err);
            }
        }
        nint recordLen = (nint)header[3] << 8 | (nint)header[4];

        var record = new slice<byte>(recordLen);
        {
            var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), record[..]); if (err != nil) {
                t.Fatal(err);
            }
        }

        c.Close();
        s.Close();

        ref var m = ref heap(new global::go.crypto.tls_package.clientHelloMsg(), out var Ꮡm);
        if (!Ꮡm.unmarshal(record)) {
            t.Errorf("unmarshaling ClientHello for %q failed", tt.@in);
            continue;
        }
        if (tt.@in != tt.@out && m.serverName == tt.@in) {
            t.Errorf("prohibited %q found in ClientHello: %x", tt.@in, record);
        }
        if (m.serverName != tt.@out) {
            t.Errorf("expected %q not found in ClientHello: %x", tt.@out, record);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unconfiguredCipherˢ => ref unconfiguredCipherˢᵇ;
private static readonly @string unconfiguredCipherˢᵇ = "unconfigured cipher"u8;

public static partial void TestServerSelectingUnconfiguredCipherSuite(ж<testing.T> t) {
    // This checks that the server can't select a cipher suite that the
    // client didn't offer. See #13174.

    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var errChan = new channel<error>(1);

    var cʗ1 = c;
    var errChanʗ1 = errChan;
    goǃ(() => {
        var client = Client(cʗ1, Ꮡ(new Config(
            ServerName: "foo",
            CipherSuites: slice<uint16>([TLS_RSA_WITH_AES_128_GCM_SHA256])
        )));
        errChanʗ1.ᐸꟷ(client.Handshake());
    });

    array<byte> header = new(5);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), header[..]); if (err != nil) {
            t.Fatal(err);
        }
    }
    nint recordLen = (nint)header[3] << 8 | (nint)header[4];

    var record = new slice<byte>(recordLen);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), record); if (err != nil) {
            t.Fatal(err);
        }
    }

    // Create a ServerHello that selects a different cipher suite than the
    // sole one that the client offered.
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuite: TLS_RSA_WITH_AES_256_GCM_SHA384
    ));
    var serverHelloBytes = mustMarshal(t, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));

    s.Write(slice<byte>([
        (byte)recordTypeHandshake,
        (byte)(((byte)VersionTLS12).Rsh((uint64)8)),
        (byte)((byte)((byte)VersionTLS12 & 0xff)),
        (byte)(len(serverHelloBytes) >> 8),
        (byte)len(serverHelloBytes)
    ]));
    s.Write(serverHelloBytes);
    s.Close();

    {
        var err = ᐸꟷ(errChan); if (!strings.Contains(err.Error(), unconfiguredCipherˢ)) {
            t.Fatalf("Expected error about unconfigured cipher suite but got %q", err);
        }
    }
}

public static void TestVerifyConnection(ж<testing.T> t) {
    t.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testVerifyConnection(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testVerifyConnection(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleGolangˢ => ref exampleGolangˢᵇ;
private static readonly @string exampleGolangˢᵇ = "example.golang"u8;
internal static readonly object protocol1ˢ = (@string)"protocol1"u8;

internal partial struct testVerifyConnection_tests /*dyn*/ {
    internal @string name;
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<nint>> configureServer;
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<nint>> configureClient;
}

internal static void testVerifyConnection(ж<testing.T> t, uint16 version) {
    error checkFields(global::go.crypto.tls_package.ΔConnectionState c, ж<nint> called, @string errorType) {
        if (c.Version != version) {
            return fmt.Errorf("%s: got Version %v, want %v"u8, errorType, c.Version, version);
        }
        if (c.HandshakeComplete) {
            return fmt.Errorf("%s: got HandshakeComplete, want false"u8, errorType);
        }
        if (c.ServerName != "example.golang"u8) {
            return fmt.Errorf("%s: got ServerName %s, want %s"u8, errorType, c.ServerName, exampleGolangˢ);
        }
        if (c.NegotiatedProtocol != "protocol1"u8) {
            return fmt.Errorf("%s: got NegotiatedProtocol %s, want %s"u8, errorType, c.NegotiatedProtocol, protocol1ˢ);
        }
        if (c.CipherSuite == 0) {
            return fmt.Errorf("%s: got CipherSuite 0, want non-zero"u8, errorType);
        }
        var wantDidResume = false;
        if (called.Value == 2) { // if this is the second time, then it should be a resumption
            wantDidResume = true;
        }
        if (c.DidResume != wantDidResume) {
            return fmt.Errorf("%s: got DidResume %t, want %t"u8, errorType, c.DidResume, wantDidResume);
        }
        return default;
    }

            var checkFieldsʗ1 = checkFields;

            var checkFieldsʗ3 = checkFields;

            var checkFieldsʗ5 = checkFields;

            var checkFieldsʗ7 = checkFields;

            var checkFieldsʗ9 = checkFields;

            var checkFieldsʗ11 = checkFields;

            var checkFieldsʗ13 = checkFields;

            var checkFieldsʗ15 = checkFields;
    var tests = slice<testVerifyConnection_tests>([
        new(
            name: "RequireAndVerifyClientCert",
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.ClientAuth = RequireAndVerifyClientCert;
                var checkFieldsʗ2 = checkFieldsʗ1;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("server: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (len(c.VerifiedChains) == 0) {
                        return fmt.Errorf("server: got len(VerifiedChains) = 0, wanted non-zero"u8);
                    }
                    return checkFieldsʗ2(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                var checkFieldsʗ4 = checkFieldsʗ3;
                config.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("client: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (len(c.VerifiedChains) == 0) {
                        return fmt.Errorf("client: got len(VerifiedChains) = 0, wanted non-zero"u8);
                    }
                    if (c.DidResume) {
                        return default;
                        // The SCTs and OCSP Response are dropped on resumption.
                        // See http://golang.org/issue/39075.
                    }
                    if (len(c.OCSPResponse) == 0) {
                        return fmt.Errorf("client: got len(OCSPResponse) = 0, wanted non-zero"u8);
                    }
                    if (len(c.SignedCertificateTimestamps) == 0) {
                        return fmt.Errorf("client: got len(SignedCertificateTimestamps) = 0, wanted non-zero"u8);
                    }
                    return checkFieldsʗ4(c, called, clientˢ);
                };
            }
        ),
        new(
            name: "InsecureSkipVerify",
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.ClientAuth = RequireAnyClientCert;
                config.InsecureSkipVerify = true;
                var checkFieldsʗ6 = checkFieldsʗ5;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("server: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (c.VerifiedChains != nil) {
                        return fmt.Errorf("server: got Verified Chains %v, want nil"u8, c.VerifiedChains);
                    }
                    return checkFieldsʗ6(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.InsecureSkipVerify = true;
                var checkFieldsʗ8 = checkFieldsʗ7;
                config.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("client: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (c.VerifiedChains != nil) {
                        return fmt.Errorf("server: got Verified Chains %v, want nil"u8, c.VerifiedChains);
                    }
                    if (c.DidResume) {
                        return default;
                        // The SCTs and OCSP Response are dropped on resumption.
                        // See http://golang.org/issue/39075.
                    }
                    if (len(c.OCSPResponse) == 0) {
                        return fmt.Errorf("client: got len(OCSPResponse) = 0, wanted non-zero"u8);
                    }
                    if (len(c.SignedCertificateTimestamps) == 0) {
                        return fmt.Errorf("client: got len(SignedCertificateTimestamps) = 0, wanted non-zero"u8);
                    }
                    return checkFieldsʗ8(c, called, clientˢ);
                };
            }
        ),
        new(
            name: "NoClientCert",
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.ClientAuth = NoClientCert;
                var checkFieldsʗ10 = checkFieldsʗ9;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    return checkFieldsʗ10(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                var checkFieldsʗ12 = checkFieldsʗ11;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    return checkFieldsʗ12(c, called, clientˢ);
                };
            }
        ),
        new(
            name: "RequestClientCert",
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.ClientAuth = RequestClientCert;
                var checkFieldsʗ14 = checkFieldsʗ13;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    return checkFieldsʗ14(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Certificates = default; // clear the client cert
                var checkFieldsʗ16 = checkFieldsʗ15;
                config.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("client: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (len(c.VerifiedChains) == 0) {
                        return fmt.Errorf("client: got len(VerifiedChains) = 0, wanted non-zero"u8);
                    }
                    if (c.DidResume) {
                        return default;
                        // The SCTs and OCSP Response are dropped on resumption.
                        // See http://golang.org/issue/39075.
                    }
                    if (len(c.OCSPResponse) == 0) {
                        return fmt.Errorf("client: got len(OCSPResponse) = 0, wanted non-zero"u8);
                    }
                    if (len(c.SignedCertificateTimestamps) == 0) {
                        return fmt.Errorf("client: got len(SignedCertificateTimestamps) = 0, wanted non-zero"u8);
                    }
                    return checkFieldsʗ16(c, called, clientˢ);
                };
            }
        )
    ]);
    foreach (var (_, test) in tests) {
        // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
        var testCertificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]), PrivateKey: testRSA2048PrivateKey.OrTypedNil())]);

        var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
        if (err != nil) {
            throw panic(err);
        }
        var rootCAs = Δx509.NewCertPool();
        rootCAs.AddCert(issuer);

        ref var serverCalled = ref heap(new nint(), out var ᏑserverCalled);
        ref var clientCalled = ref heap(new nint(), out var ᏑclientCalled);

        var serverConfig = Ꮡ(new Config(
            MaxVersion: version,
            Certificates: testCertificates,
            Time: testTime,
            ClientCAs: rootCAs,
            NextProtos: slice<@string>(["protocol1"])
        ));
        serverConfig.Certificates[0].SignedCertificateTimestamps = slice<slice<byte>>([slice<byte>("dummy sct 1"u8), slice<byte>("dummy sct 2"u8)]);
        serverConfig.Certificates[0].OCSPStaple = slice<byte>("dummy ocsp"u8);
        test.configureServer(serverConfig, ᏑserverCalled);

        var clientConfig = Ꮡ(new Config(
            MaxVersion: version,
            ClientSessionCache: NewLRUClientSessionCache(32),
            RootCAs: rootCAs,
            ServerName: "example.golang",
            Certificates: testCertificates,
            Time: testTime,
            NextProtos: slice<@string>(["protocol1"])
        ));
        test.configureClient(clientConfig, ᏑclientCalled);

        var clientConfigʗ1 = clientConfig;
        var serverConfigʗ1 = serverConfig;
        void testHandshakeState(@string name, bool didResume) {
            var (_, hs, errΔ1) = testHandshake(t, clientConfigʗ1, serverConfigʗ1);
            if (errΔ1 != nil) {
                t.Fatalf("%s: handshake failed: %s", name, errΔ1);
            }
            if (hs.DidResume != didResume) {
                t.Errorf("%s: resumed: %v, expected: %v", name, hs.DidResume, didResume);
            }
            nint wantCalled = 1;
            if (didResume) {
                wantCalled = 2; // resumption would mean this is the second time it was called in this test
            }
            if (ᏑclientCalled.Value != wantCalled) {
                t.Errorf("%s: expected client VerifyConnection called %d times, did %d times", name, wantCalled, ᏑclientCalled.Value);
            }
            if (ᏑserverCalled.Value != wantCalled) {
                t.Errorf("%s: expected server VerifyConnection called %d times, did %d times", name, wantCalled, ᏑserverCalled.Value);
            }
        }
        testHandshakeState(fmt.Sprintf("%s-FullHandshake"u8, test.name), false);
        testHandshakeState(fmt.Sprintf("%s-Resumption"u8, test.name), true);
    }
}

public static void TestVerifyPeerCertificate(ж<testing.T> t) {
    t.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testVerifyPeerCertificate(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testVerifyPeerCertificate(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gotLenValidatedChains0ˢ => ref gotLenValidatedChains0ˢᵇ;
private static readonly @string gotLenValidatedChains0ˢᵇ = "got len(validatedChains) = 0, wanted non-zero"u8;

internal partial struct testVerifyPeerCertificate_tests /*dyn*/ {
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<bool>> configureServer;
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<bool>> configureClient;
    internal Action<ж<testing.T>, nint, bool, bool, error, error> validate;
}

internal static partial void testVerifyPeerCertificate(ж<testing.T> t, uint16 version) {
    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    ref var err = ref heap<error>(out var Ꮡerr);
    (var issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != nil) {
        throw panic(err);
    }

    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);

    var sentinelErr = errors.New("TestVerifyPeerCertificate");

    error verifyPeerCertificateCallback(ж<bool> called, slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) {
        {
            nint l = len(rawCerts); if (l != 1) {
                return fmt.Errorf("got len(rawCerts) = %d, wanted 1"u8, l);
            }
        }
        if (len(validatedChains) == 0) {
            return errors.New(gotLenValidatedChains0ˢ);
        }
        called.Value = true;
        return default;
    }
    error verifyConnectionCallback(ж<bool> called, bool isClient, global::go.crypto.tls_package.ΔConnectionState c) {
        {
            nint l = len(c.PeerCertificates); if (l != 1) {
                return fmt.Errorf("got len(PeerCertificates) = %d, wanted 1"u8, l);
            }
        }
        if (len(c.VerifiedChains) == 0) {
            return fmt.Errorf("got len(VerifiedChains) = 0, wanted non-zero"u8);
        }
        if (isClient && len(c.OCSPResponse) == 0) {
            return fmt.Errorf("got len(OCSPResponse) = 0, wanted non-zero"u8);
        }
        called.Value = true;
        return default;
    }

            var verifyPeerCertificateCallbackʗ1 = verifyPeerCertificateCallback;

            var verifyPeerCertificateCallbackʗ3 = verifyPeerCertificateCallback;


            var sentinelErrʗ1 = sentinelErr;


            var sentinelErrʗ3 = sentinelErr;


            var sentinelErrʗ4 = sentinelErr;

            var sentinelErrʗ6 = sentinelErr;




            var verifyConnectionCallbackʗ1 = verifyConnectionCallback;

            var verifyConnectionCallbackʗ3 = verifyConnectionCallback;


            var sentinelErrʗ7 = sentinelErr;


            var sentinelErrʗ9 = sentinelErr;


            var sentinelErrʗ10 = sentinelErr;

            var sentinelErrʗ12 = sentinelErr;

            var sentinelErrʗ13 = sentinelErr;
            var verifyPeerCertificateCallbackʗ5 = verifyPeerCertificateCallback;


            var sentinelErrʗ15 = sentinelErr;


            var sentinelErrʗ16 = sentinelErr;
            var verifyPeerCertificateCallbackʗ7 = verifyPeerCertificateCallback;

            var sentinelErrʗ18 = sentinelErr;
    var tests = slice<testVerifyPeerCertificate_tests>([
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ2 = verifyPeerCertificateCallbackʗ1;
                config.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ2(called, rawCerts, validatedChains);
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ4 = verifyPeerCertificateCallbackʗ3;
                config.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ4(called, rawCerts, validatedChains);
            },
            validate: (ж<testing.T> tΔ1, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (clientErr != nil) {
                    tΔ1.Errorf("test[%d]: client handshake failed: %v", testNo, clientErr);
                }
                if (serverErr != nil) {
                    tΔ1.Errorf("test[%d]: server handshake failed: %v", testNo, serverErr);
                }
                if (!clientCalled) {
                    tΔ1.Errorf("test[%d]: client did not call callback", testNo);
                }
                if (!serverCalled) {
                    tΔ1.Errorf("test[%d]: server did not call callback", testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var sentinelErrʗ2 = sentinelErrʗ1;
                config.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => sentinelErrʗ2;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.VerifyPeerCertificate = default;
            },
            validate: (ж<testing.T> tΔ2, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(serverErr, sentinelErrʗ3)) {
                    tΔ2.Errorf("#%d: got server error %v, wanted sentinelErr", testNo, serverErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                var sentinelErrʗ5 = sentinelErrʗ4;
                config.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => sentinelErrʗ5;
            },
            validate: (ж<testing.T> tΔ3, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(clientErr, sentinelErrʗ6)) {
                    tΔ3.Errorf("#%d: got client error %v, wanted sentinelErr", testNo, clientErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = true;
                config.VerifyPeerCertificate = error (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => {
                    {
                        nint l = len(rawCerts); if (l != 1) {
                            return fmt.Errorf("got len(rawCerts) = %d, wanted 1"u8, l);
                        }
                    }
                    // With InsecureSkipVerify set, this
                    // callback should still be called but
                    // validatedChains must be empty.
                    {
                        nint l = len(validatedChains); if (l != 0) {
                            return fmt.Errorf("got len(validatedChains) = %d, wanted zero"u8, l);
                        }
                    }
                    called.Value = true;
                    return default;
                };
            },
            validate: (ж<testing.T> tΔ4, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (clientErr != nil) {
                    tΔ4.Errorf("test[%d]: client handshake failed: %v", testNo, clientErr);
                }
                if (serverErr != nil) {
                    tΔ4.Errorf("test[%d]: server handshake failed: %v", testNo, serverErr);
                }
                if (!clientCalled) {
                    tΔ4.Errorf("test[%d]: client did not call callback", testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var verifyConnectionCallbackʗ2 = verifyConnectionCallbackʗ1;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => verifyConnectionCallbackʗ2(called, false, c);
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var verifyConnectionCallbackʗ4 = verifyConnectionCallbackʗ3;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => verifyConnectionCallbackʗ4(called, true, c);
            },
            validate: (ж<testing.T> tΔ5, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (clientErr != nil) {
                    tΔ5.Errorf("test[%d]: client handshake failed: %v", testNo, clientErr);
                }
                if (serverErr != nil) {
                    tΔ5.Errorf("test[%d]: server handshake failed: %v", testNo, serverErr);
                }
                if (!clientCalled) {
                    tΔ5.Errorf("test[%d]: client did not call callback", testNo);
                }
                if (!serverCalled) {
                    tΔ5.Errorf("test[%d]: server did not call callback", testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var sentinelErrʗ8 = sentinelErrʗ7;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ8;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                config.VerifyConnection = default;
            },
            validate: (ж<testing.T> tΔ6, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(serverErr, sentinelErrʗ9)) {
                    tΔ6.Errorf("#%d: got server error %v, wanted sentinelErr", testNo, serverErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                config.VerifyConnection = default;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var sentinelErrʗ11 = sentinelErrʗ10;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ11;
            },
            validate: (ж<testing.T> tΔ7, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(clientErr, sentinelErrʗ12)) {
                    tΔ7.Errorf("#%d: got client error %v, wanted sentinelErr", testNo, clientErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ6 = verifyPeerCertificateCallbackʗ5;
                config.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ6(called, rawCerts, validatedChains);
                var sentinelErrʗ14 = sentinelErrʗ13;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ14;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                config.VerifyPeerCertificate = default;
                config.VerifyConnection = default;
            },
            validate: (ж<testing.T> tΔ8, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(serverErr, sentinelErrʗ15)) {
                    tΔ8.Errorf("#%d: got server error %v, wanted sentinelErr", testNo, serverErr);
                }
                if (!serverCalled) {
                    tΔ8.Errorf("test[%d]: server did not call callback", testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                config.VerifyPeerCertificate = default;
                config.VerifyConnection = default;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ8 = verifyPeerCertificateCallbackʗ7;
                config.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ8(called, rawCerts, validatedChains);
                var sentinelErrʗ17 = sentinelErrʗ16;
                config.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ17;
            },
            validate: (ж<testing.T> tΔ9, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(clientErr, sentinelErrʗ18)) {
                    tΔ9.Errorf("#%d: got client error %v, wanted sentinelErr", testNo, clientErr);
                }
                if (!clientCalled) {
                    tΔ9.Errorf("test[%d]: client did not call callback", testNo);
                }
            }
        )
    ]);

    foreach (var (i, vᴛ1) in tests) {
        ref var test = ref heap(new testVerifyPeerCertificate_tests(), out var Ꮡtest);
        test = vᴛ1;

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
        var done = new channel<error>(0);

        ref var clientCalled = ref heap(new bool(), out var ᏑclientCalled);
        ref var serverCalled = ref heap(new bool(), out var ᏑserverCalled);

        var doneʗ1 = done;
        var rootCAsʗ1 = rootCAs;
        var sʗ1 = s;
        var testʗ1 = test;
        goǃ(() => {
            var configΔ1 = testConfig.Clone();
            configΔ1.ServerName = exampleGolangˢ;
            configΔ1.ClientAuth = RequireAndVerifyClientCert;
            configΔ1.ClientCAs = rootCAsʗ1;
            configΔ1.Time = testTime;
            configΔ1.MaxVersion = version;
            configΔ1.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
            configΔ1.Certificates[0].ΔCertificate = slice<slice<byte>>([testRSA2048Certificate]);
            configΔ1.Certificates[0].PrivateKey = testRSA2048PrivateKey.OrTypedNil();
            configΔ1.Certificates[0].SignedCertificateTimestamps = slice<slice<byte>>([slice<byte>("dummy sct 1"u8), slice<byte>("dummy sct 2"u8)]);
            configΔ1.Certificates[0].OCSPStaple = slice<byte>("dummy ocsp"u8);
            testʗ1.configureServer(configΔ1, ᏑserverCalled);

            Ꮡerr.ValueSlot = Server(sʗ1, configΔ1).Handshake();
            sʗ1.Close();
            doneʗ1.ᐸꟷ(Ꮡerr.ValueSlot);
        });

        var config = testConfig.Clone();
        config.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]), PrivateKey: testRSA2048PrivateKey.OrTypedNil())]);
        config.ServerName = exampleGolangˢ;
        config.RootCAs = rootCAs;
        config.Time = testTime;
        config.MaxVersion = version;
        test.configureClient(config, ᏑclientCalled);
        var clientErr = Client(c, config).Handshake();
        c.Close();
        var serverErr = ᐸꟷ(done);

        test.validate(t, i, clientCalled, serverCalled, clientErr, serverErr);
    }
}

// brokenConn wraps a net.Conn and causes all Writes after a certain number to
// fail with brokenConnErr.
internal partial struct brokenConn {
    /*embed*/ public net_package.Conn Conn;
    // breakAfter is the number of successful writes that will be allowed
    // before all subsequent writes fail.
    internal nint breakAfter;
    // numWrites is the number of writes that have been done.
    internal nint numWrites;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static error Close(this brokenConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this brokenConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this brokenConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this brokenConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this brokenConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this brokenConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *brokenConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this brokenConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

// brokenConnErr is the error that brokenConn returns once exhausted.
internal static error brokenConnErr = errors.New("too many writes to brokenConn");

internal static (nint, error) Write(this ref brokenConn b, slice<byte> data) {
    if (b.numWrites >= b.breakAfter) {
        return (0, brokenConnErr);
    }

    b.numWrites++;
    return b.Conn.Write(data);
}

public static partial void TestFailedWrite(ж<testing.T> t) {
    // Test that a write error during the handshake is returned.
    foreach (var (_, vᴛ1) in slice<nint>([0, 1])) {
        ref var breakAfter = ref heap(new nint(), out var ᏑbreakAfter);
        breakAfter = vᴛ1;

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
        var done = new channel<bool>(0);

        var doneʗ1 = done;
        var sʗ1 = s;
        goǃ(() => {
            Server(sʗ1, testConfig).Handshake();
            sʗ1.Close();
            doneʗ1.ᐸꟷ(true);
        });

        var brokenC = Ꮡ(new brokenConn(Conn: c, breakAfter: breakAfter));
        var err = Client(new tls_internal_test_package.brokenConnжConn(brokenC), testConfig).Handshake();
        if (!AreEqual(err, brokenConnErr)) {
            t.Errorf("#%d: expected error from brokenConn but got %q", breakAfter, err);
        }
        (~brokenC).Conn.Close();

        ᐸꟷ(done);
    }
}

// writeCountingConn wraps a net.Conn and counts the number of Write calls.
internal partial struct writeCountingConn {
    /*embed*/ public net_package.Conn Conn;
    // numWrites is the number of writes that have been done.
    internal nint numWrites;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error Close(this writeCountingConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this writeCountingConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this writeCountingConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this writeCountingConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this writeCountingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this writeCountingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this writeCountingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint, error) Write(this ref writeCountingConn wcc, slice<byte> data) {
    wcc.numWrites++;
    return wcc.Conn.Write(data);
}

public static void TestBuffering(ж<testing.T> t) {
    t.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testBuffering(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testBuffering(tΔ2, VersionTLS13);
    });
}

internal static partial void testBuffering(ж<testing.T> t, uint16 version) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var done = new channel<bool>(0);

    var clientWCC = Ꮡ(new writeCountingConn(Conn: c));
    var serverWCC = Ꮡ(new writeCountingConn(Conn: s));

    var doneʗ1 = done;
    var serverWCCʗ1 = serverWCC;
    goǃ(() => {
        var config = testConfig.Clone();
        config.MaxVersion = version;
        Server(new tls_internal_test_package.writeCountingConnжConn(serverWCCʗ1), config).Handshake();
        (~serverWCCʗ1).Conn.Close();
        doneʗ1.ᐸꟷ(true);
    });

    var err = Client(new tls_internal_test_package.writeCountingConnжConn(clientWCC), testConfig).Handshake();
    if (err != nil) {
        t.Fatal(err);
    }
    (~clientWCC).Conn.Close();
    ᐸꟷ(done);

    nint expectedClient = default;
    nint expectedServer = default;
    if (version == VersionTLS13) {
        expectedClient = 2;
        expectedServer = 1;
    } else {
        expectedClient = 2;
        expectedServer = 2;
    }

    {
        nint n = clientWCC.numWrites; if (n != expectedClient) {
            t.Errorf("expected client handshake to complete with %d writes, but saw %d", expectedClient, n);
        }
    }

    {
        nint n = serverWCC.numWrites; if (n != expectedServer) {
            t.Errorf("expected server handshake to complete with %d writes, but saw %d", expectedServer, n);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientUnexpectedlyˢ = (@string)"client unexpectedly returned no error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedErrorᶜ => ref expectedErrorᶜᵇ;
private static readonly @string expectedErrorᶜᵇ = "remote error: tls: internal error"u8;

public static partial void TestAlertFlushing(ж<testing.T> t) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var done = new channel<bool>(0);

    var clientWCC = Ꮡ(new writeCountingConn(Conn: c));
    var serverWCC = Ꮡ(new writeCountingConn(Conn: s));

    var serverConfig = testConfig.Clone();

    // Cause a signature-time error
    ref var brokenKey = ref heap<rsa.PrivateKey>(new rsa.PrivateKey(PublicKey: testRSAPrivateKey.PublicKey), out var ᏑbrokenKey);
    brokenKey.D = big.NewInt(42);
    serverConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(
        ΔCertificate: slice<slice<byte>>([testRSACertificate]),
        PrivateKey: ᏑbrokenKey
    )
    ]);

    var doneʗ1 = done;
    var serverConfigʗ1 = serverConfig;
    var serverWCCʗ1 = serverWCC;
    goǃ(() => {
        Server(new tls_internal_test_package.writeCountingConnжConn(serverWCCʗ1), serverConfigʗ1).Handshake();
        (~serverWCCʗ1).Conn.Close();
        doneʗ1.ᐸꟷ(true);
    });

    var err = Client(new tls_internal_test_package.writeCountingConnжConn(clientWCC), testConfig).Handshake();
    if (err == nil) {
        t.Fatal(clientUnexpectedlyˢ);
    }

    @string expectedError = expectedErrorᶜ;
    {
        @string e = err.Error(); if (!strings.Contains(e, expectedError)) {
            t.Fatalf("expected to find %q in error but error was %q", expectedError, e);
        }
    }
    (~clientWCC).Conn.Close();
    ᐸꟷ(done);

    {
        nint n = serverWCC.numWrites; if (n != 1) {
            t.Errorf("expected server handshake to complete with one write, but saw %d", n);
        }
    }
}

public static partial void TestHandshakeRace(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ2);
    }
    t.Parallel();
    // This test races a Read and Write to try and complete a handshake in
    // order to provide some evidence that there are no races or deadlocks
    // in the handshake locking.
    for (nint i = 0; i < 32; i++) {
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));

        var sʗ1 = s;
        goǃ(() => {
            var server = Server(sʗ1, testConfig);
            {
                var err = server.Handshake(); if (err != nil) {
                    throw panic(err);
                }
            }

            ref var request = ref heap(new array<byte>(1), out var Ꮡrequest);
            {
                var (n, err) = server.Read(request[..]); if (err != nil || n != 1) {
                    throw panic(err);
                }
            }

            server.Write(request[..]);
            server.Close();
        });

        var startWrite = new channel<EmptyStruct>(0);
        var startRead = new channel<EmptyStruct>(0);
        var readDone = new channel<EmptyStruct>(1);

        var client = Client(c, testConfig);
        var clientʗ1 = client;
        var startWriteʗ1 = startWrite;
        goǃ(() => {
            ᐸꟷ(startWriteʗ1);
            ref var request = ref heap(new array<byte>(1), out var Ꮡrequest);
            clientʗ1.Write(request[..]);
        });

        var cʗ1 = c;
        var clientʗ2 = client;
        var readDoneʗ1 = readDone;
        var startReadʗ1 = startRead;
        goǃ(() => {
            ᐸꟷ(startReadʗ1);
            ref var reply = ref heap(new array<byte>(1), out var Ꮡreply);
            {
                var (_, err) = io.ReadFull(new tls_test_package.tls_ConnжReader(clientʗ2), reply[..]); if (err != nil) {
                    throw panic(err);
                }
            }
            cʗ1.Close();
            readDoneʗ1.ᐸꟷ(new EmptyStruct());
        });

        if ((i & 1) == 1) {
            startWrite.ᐸꟷ(new EmptyStruct());
            startRead.ᐸꟷ(new EmptyStruct());
        } else {
            startRead.ᐸꟷ(new EmptyStruct());
            startWrite.ᐸꟷ(new EmptyStruct());
        }
        ᐸꟷ(readDone);
    }
}

partial struct getClientCertificateTestsᴛ1 /*dyn*/ {
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<global::go.crypto.tls_package.Config>> setup;
    internal @string expectedClientError;
    internal Action<ж<testing.T>, nint, ж<global::go.crypto.tls_package.ΔConnectionState>> verify;
}
internal static slice<getClientCertificateTestsᴛ1> getClientCertificateTests;
internal static void initᴛgetClientCertificateTests() { getClientCertificateTests = slice<getClientCertificateTestsᴛ1>([
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            // Returning a Certificate with no certificate data
            // should result in an empty message being sent to the
            // server.
            serverConfig.ClientCAs = default;
            clientConfig.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (len(cri.SignatureSchemes) == 0) {
                    throw panic("empty SignatureSchemes");
                }
                if (len(cri.AcceptableCAs) != 0) {
                    throw panic("AcceptableCAs should have been empty");
                }
                return (@new<global::go.crypto.tls_package.Certificate>(), default);
            };
        },
        "",
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
            {
                nint l = len(cs.PeerCertificates); if (l != 0) {
                    t.Errorf("#%d: expected no certificates but got %d", testNum, l);
                }
            }
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            // With TLS 1.1, the SignatureSchemes should be
            // synthesised from the supported certificate types.
            clientConfig.MaxVersion = VersionTLS11;
            clientConfig.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (len(cri.SignatureSchemes) == 0) {
                    throw panic("empty SignatureSchemes");
                }
                return (@new<global::go.crypto.tls_package.Certificate>(), default);
            };
        },
        "",
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
            {
                nint l = len(cs.PeerCertificates); if (l != 0) {
                    t.Errorf("#%d: expected no certificates but got %d", testNum, l);
                }
            }
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            // Returning an error should abort the handshake with
            // that error.
            clientConfig.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => (default, errors.New("GetClientCertificate"));
        },
        "GetClientCertificate",
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            clientConfig.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (len(cri.AcceptableCAs) == 0) {
                    throw panic("empty AcceptableCAs");
                }
                var cert = Ꮡ(new Certificate(
                    ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]),
                    PrivateKey: testRSA2048PrivateKey.OrTypedNil()
                ));
                return (cert, default);
            };
        },
        "",
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
            if (len(cs.VerifiedChains) == 0) {
                t.Errorf("#%d: expected some verified chains, but found none", testNum);
            }
        }
    )
]); }

public static void TestGetClientCertificate(ж<testing.T> t) {
    t.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testGetClientCertificate(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testGetClientCertificate(tΔ2, VersionTLS13);
    });
}

internal partial struct testGetClientCertificate_serverResult /*dyn*/ {
    internal global::go.crypto.tls_package.ΔConnectionState cs;
    internal error err;
}

internal static partial void testGetClientCertificate(ж<testing.T> t, uint16 version) {
    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != nil) {
        throw panic(err);
    }

    foreach (var (i, test) in getClientCertificateTests) {
        var serverConfig = testConfig.Clone();
        serverConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]), PrivateKey: testRSA2048PrivateKey.OrTypedNil())]);
        serverConfig.ClientAuth = VerifyClientCertIfGiven;
        serverConfig.RootCAs = Δx509.NewCertPool();
        serverConfig.RootCAs.AddCert(issuer);
        serverConfig.ClientCAs = serverConfig.RootCAs;
        serverConfig.Time = testTime;
        serverConfig.MaxVersion = version;

        var clientConfig = testConfig.Clone();
        clientConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]), PrivateKey: testRSA2048PrivateKey.OrTypedNil())]);
        clientConfig.MaxVersion = version;

        test.setup(clientConfig, serverConfig);

        // TLS 1.1 isn't available for FIPS required
        if (fips140tls.Required() && clientConfig.MaxVersion == VersionTLS11) {
            t.Logf("skipping test %d for FIPS mode", i);
            continue;
        }

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
        var done = new channel<testGetClientCertificate_serverResult>(0);

        var doneʗ1 = done;
        var sʗ1 = s;
        var serverConfigʗ1 = serverConfig;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var sʗ2 = sʗ1;
                defer(() => sʗ2.Close(), ref ᒐ);
                var server = Server(sʗ1, serverConfigʗ1);
                var errΔ1 = server.Handshake();

                ref var cs = ref heap(new global::go.crypto.tls_package.ΔConnectionState(), out var Ꮡcs);
                if (errΔ1 == nil) {
                    cs = server.ConnectionState();
                }
                doneʗ1.ᐸꟷ(new testGetClientCertificate_serverResult(cs, errΔ1));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        var clientErr = Client(c, clientConfig).Handshake();
        c.Close();

        ref var result = ref heap<testGetClientCertificate_serverResult>(ᐸꟷ(done), out var Ꮡresult);

        if (clientErr != nil) {
            if (len(test.expectedClientError) == 0) {
                t.Errorf("#%d: client error: %v", i, clientErr);
            } else {
                @string got = clientErr.Error(); if (got != test.expectedClientError) {
                    t.Errorf("#%d: expected client error %q, but got %q", i, test.expectedClientError, got);
                } else {
                    test.verify(t, i, Ꮡresult.Ꮡcs);
                }
            }
        } else if (len(test.expectedClientError) > 0) {
            t.Errorf("#%d: expected client error %q, but got no error", i, test.expectedClientError);
        } else {
            var errΔ2 = result.err; if (errΔ2 != nil) {
                t.Errorf("#%d: server error: %v", i, errΔ2);
            } else {
                test.verify(t, i, Ꮡresult.Ꮡcs);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object aRsassaPssCertificateWasˢ = (@string)"A RSASSA-PSS certificate was parsed like a PKCS#1 v1.5 one, and it will be mistakenly used with rsa_pss_rsae_* signature algorithms"u8;

public static void TestRSAPSSKeyError(ж<testing.T> t) {
    // crypto/tls does not support the rsa_pss_pss_* SignatureSchemes. If support for
    // public keys with OID RSASSA-PSS is added to crypto/x509, they will be misused with
    // the rsa_pss_rsae_* SignatureSchemes. Assert that RSASSA-PSS certificates don't
    // parse, or that they don't carry *rsa.PublicKey keys.
    var (b, _) = pem.Decode(slice<byte>("""

-----BEGIN CERTIFICATE-----
MIIDZTCCAhygAwIBAgIUCF2x0FyTgZG0CC9QTDjGWkB5vgEwPgYJKoZIhvcNAQEK
MDGgDTALBglghkgBZQMEAgGhGjAYBgkqhkiG9w0BAQgwCwYJYIZIAWUDBAIBogQC
AgDeMBIxEDAOBgNVBAMMB1JTQS1QU1MwHhcNMTgwNjI3MjI0NDM2WhcNMTgwNzI3
MjI0NDM2WjASMRAwDgYDVQQDDAdSU0EtUFNTMIIBIDALBgkqhkiG9w0BAQoDggEP
ADCCAQoCggEBANxDm0f76JdI06YzsjB3AmmjIYkwUEGxePlafmIASFjDZl/elD0Z
/a7xLX468b0qGxLS5al7XCcEprSdsDR6DF5L520+pCbpfLyPOjuOvGmk9KzVX4x5
b05YXYuXdsQ0Kjxcx2i3jjCday6scIhMJVgBZxTEyMj1thPQM14SHzKCd/m6HmCL
QmswpH2yMAAcBRWzRpp/vdH5DeOJEB3aelq7094no731mrLUCHRiZ1htq8BDB3ou
czwqgwspbqZ4dnMXl2MvfySQ5wJUxQwILbiuAKO2lVVPUbFXHE9pgtznNoPvKwQT
JNcX8ee8WIZc2SEGzofjk3NpjR+2ADB2u3sCAwEAAaNTMFEwHQYDVR0OBBYEFNEz
AdyJ2f+fU+vSCS6QzohnOnprMB8GA1UdIwQYMBaAFNEzAdyJ2f+fU+vSCS6Qzohn
OnprMA8GA1UdEwEB/wQFMAMBAf8wPgYJKoZIhvcNAQEKMDGgDTALBglghkgBZQME
AgGhGjAYBgkqhkiG9w0BAQgwCwYJYIZIAWUDBAIBogQCAgDeA4IBAQCjEdrR5aab
sZmCwrMeKidXgfkmWvfuLDE+TCbaqDZp7BMWcMQXT9O0UoUT5kqgKj2ARm2pEW0Z
H3Z1vj3bbds72qcDIJXp+l0fekyLGeCrX/CbgnMZXEP7+/+P416p34ChR1Wz4dU1
KD3gdsUuTKKeMUog3plxlxQDhRQmiL25ygH1LmjLd6dtIt0GVRGr8lj3euVeprqZ
bZ3Uq5eLfsn8oPgfC57gpO6yiN+UURRTlK3bgYvLh4VWB3XXk9UaQZ7Mq1tpXjoD
HYFybkWzibkZp4WRo+Fa28rirH+/wHt0vfeN7UCceURZEx4JaxIIfe4ku7uDRhJi
RwBA9Xk1KBNF
-----END CERTIFICATE-----
"""u8.Raw()));
    if (b == nil) {
        t.Fatal(failedToDecodeˢ);
    }
    var (cert, err) = Δx509.ParseCertificate(b.Bytes);
    if (err != nil) {
        return;
    }
    {
        var (_, ok) = cert.PublicKey._<ж<rsa.PublicKey>>(ᐧ); if (ok) {
            t.Error(aRsassaPssCertificateWasˢ);
        }
    }
}

public static partial void TestCloseClientConnectionOnIdleServer(ж<testing.T> t) {
    var (clientConn, serverConn) = localPipe(new tls_test_package.testing_TжTB(t));
    var client = Client(clientConn, testConfig.Clone());
    var clientʗ1 = client;
    var serverConnʗ1 = serverConn;
    goǃ(() => {
        ref var b = ref heap(new array<byte>(1), out var Ꮡb);
        serverConnʗ1.Read(b[..]);
        clientʗ1.Close();
    });
    client.SetWriteDeadline(time_package.Now().Add(time_package.ΔMinute));
    var err = client.Handshake();
    if (err != nil) {
        {
            var (errΔ1, ok) = err._<netꓸError>(ᐧ); if (ok && errΔ1.Timeout()) {
                t.Errorf("Expected a closed network connection error but got '%s'", errΔ1.Error());
            }
        }
    } else {
        t.Errorf("Error expected, but no error returned");
    }
}

internal static error testDowngradeCanary(ж<testing.T> t, uint16 clientVersion, uint16 serverVersion) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            testingOnlyForceDowngradeCanary = false;
        }, ref ᒐ);
        testingOnlyForceDowngradeCanary = true;

        var clientConfig = testConfig.Clone();
        clientConfig.MaxVersion = clientVersion;
        var serverConfig = testConfig.Clone();
        serverConfig.MaxVersion = serverVersion;
        var (_, _, err) = testHandshake(t, clientConfig, serverConfig);
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTheRestOfTheˢ = (@string)"skipping the rest of the checks in short mode"u8;

public static void TestDowngradeCanary(ж<testing.T> t) {
    {
        var err = testDowngradeCanary(t, VersionTLS13, VersionTLS12); if (err == nil) {
            t.Errorf("downgrade from TLS 1.3 to TLS 1.2 was not detected");
        }
    }
    if (testing.Short()) {
        t.Skip(skippingTheRestOfTheˢ);
    }
    {
        var err = testDowngradeCanary(t, VersionTLS13, VersionTLS11); if (err == nil) {
            t.Errorf("downgrade from TLS 1.3 to TLS 1.1 was not detected");
        }
    }
    {
        var err = testDowngradeCanary(t, VersionTLS13, VersionTLS10); if (err == nil) {
            t.Errorf("downgrade from TLS 1.3 to TLS 1.0 was not detected");
        }
    }
    {
        var err = testDowngradeCanary(t, VersionTLS12, VersionTLS11); if (err == nil) {
            t.Errorf("downgrade from TLS 1.2 to TLS 1.1 was not detected");
        }
    }
    {
        var err = testDowngradeCanary(t, VersionTLS12, VersionTLS10); if (err == nil) {
            t.Errorf("downgrade from TLS 1.2 to TLS 1.0 was not detected");
        }
    }
    {
        var err = testDowngradeCanary(t, VersionTLS13, VersionTLS13); if (err != nil) {
            t.Errorf("server unexpectedly sent downgrade canary for TLS 1.3");
        }
    }
    {
        var err = testDowngradeCanary(t, VersionTLS12, VersionTLS12); if (err != nil) {
            t.Errorf("client didn't ignore expected TLS 1.2 canary");
        }
    }
    if (!fips140tls.Required()) {
        {
            var err = testDowngradeCanary(t, VersionTLS11, VersionTLS11); if (err != nil) {
                t.Errorf("client unexpectedly reacted to a canary in TLS 1.1");
            }
        }
        {
            var err = testDowngradeCanary(t, VersionTLS10, VersionTLS10); if (err != nil) {
                t.Errorf("client unexpectedly reacted to a canary in TLS 1.0");
            }
        }
    } else {
        t.Logf("skiping TLS 1.1 and TLS 1.0 downgrade canary checks in FIPS mode");
    }
}

public static void TestResumptionKeepsOCSPAndSCT(ж<testing.T> t) {
    t.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testResumptionKeepsOCSPAndSCT(tΔ1, VersionTLS12);
    });
    t.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testResumptionKeepsOCSPAndSCT(tΔ2, VersionTLS13);
    });
}

internal static void testResumptionKeepsOCSPAndSCT(ж<testing.T> t, uint16 ver) {
    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != nil) {
        t.Fatalf("failed to parse test issuer");
    }
    var roots = Δx509.NewCertPool();
    roots.AddCert(issuer);
    var clientConfig = Ꮡ(new Config(
        MaxVersion: ver,
        ClientSessionCache: NewLRUClientSessionCache(32),
        ServerName: "example.golang",
        RootCAs: roots,
        Time: testTime
    ));
    var serverConfig = testConfig.Clone();
    serverConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: slice<slice<byte>>([testRSA2048Certificate]), PrivateKey: testRSA2048PrivateKey.OrTypedNil())]);
    serverConfig.MaxVersion = ver;
    serverConfig.Certificates[0].OCSPStaple = slice<byte>([1, 2, 3]);
    serverConfig.Certificates[0].SignedCertificateTimestamps = slice<slice<byte>>([slice<byte>([4, 5, 6])]);

    (_, var ccs, err) = testHandshake(t, clientConfig, serverConfig);
    if (err != nil) {
        t.Fatalf("handshake failed: %s", err);
    }
    // after a new session we expect to see OCSPResponse and
    // SignedCertificateTimestamps populated as usual
    if (!bytes.Equal(ccs.OCSPResponse, serverConfig.Certificates[0].OCSPStaple)) {
        t.Errorf("client ConnectionState contained unexpected OCSPResponse: wanted %v, got %v",
            serverConfig.Certificates[0].OCSPStaple, ccs.OCSPResponse);
    }
    if (!reflect.DeepEqual(ccs.SignedCertificateTimestamps, serverConfig.Certificates[0].SignedCertificateTimestamps)) {
        t.Errorf("client ConnectionState contained unexpected SignedCertificateTimestamps: wanted %v, got %v",
            serverConfig.Certificates[0].SignedCertificateTimestamps, ccs.SignedCertificateTimestamps);
    }

    // if the server doesn't send any SCTs, repopulate the old SCTs
    var oldSCTs = serverConfig.Certificates[0].SignedCertificateTimestamps;
    serverConfig.Certificates[0].SignedCertificateTimestamps = default;
    (_, ccs, err) = testHandshake(t, clientConfig, serverConfig);
    if (err != nil) {
        t.Fatalf("handshake failed: %s", err);
    }
    if (!ccs.DidResume) {
        t.Fatalf("expected session to be resumed");
    }
    // after a resumed session we also expect to see OCSPResponse
    // and SignedCertificateTimestamps populated
    if (!bytes.Equal(ccs.OCSPResponse, serverConfig.Certificates[0].OCSPStaple)) {
        t.Errorf("client ConnectionState contained unexpected OCSPResponse after resumption: wanted %v, got %v",
            serverConfig.Certificates[0].OCSPStaple, ccs.OCSPResponse);
    }
    if (!reflect.DeepEqual(ccs.SignedCertificateTimestamps, oldSCTs)) {
        t.Errorf("client ConnectionState contained unexpected SignedCertificateTimestamps after resumption: wanted %v, got %v",
            oldSCTs, ccs.SignedCertificateTimestamps);
    }

    //  Only test overriding the SCTs for TLS 1.2, since in 1.3
    // the server won't send the message containing them
    if (ver == VersionTLS13) {
        return;
    }

    // if the server changes the SCTs it sends, they should override the saved SCTs
    serverConfig.Certificates[0].SignedCertificateTimestamps = slice<slice<byte>>([slice<byte>([7, 8, 9])]);
    (_, ccs, err) = testHandshake(t, clientConfig, serverConfig);
    if (err != nil) {
        t.Fatalf("handshake failed: %s", err);
    }
    if (!ccs.DidResume) {
        t.Fatalf("expected session to be resumed");
    }
    if (!reflect.DeepEqual(ccs.SignedCertificateTimestamps, serverConfig.Certificates[0].SignedCertificateTimestamps)) {
        t.Errorf("client ConnectionState contained unexpected SignedCertificateTimestamps after resumption: wanted %v, got %v",
            serverConfig.Certificates[0].SignedCertificateTimestamps, ccs.SignedCertificateTimestamps);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientHandshakeDidNotˢ = (@string)"Client handshake did not error when the context was canceled"u8;
internal static readonly object connCloseDoesNotErrorAsˢ = (@string)"conn.Close does not error as expected when called multiple times on GOOS=js or GOOS=wasip1"u8;
internal static readonly object clientConnectionWasNotˢ = (@string)"Client connection was not closed when the context was canceled"u8;

// TestClientHandshakeContextCancellation tests that canceling
// the context given to the client side conn.HandshakeContext
// interrupts the in-progress handshake.
public static partial void TestClientHandshakeContextCancellation(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
        var (ctx, cancel) = context.WithCancel(context.Background());
        var unblockServer = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), unblockServer, ref ᒐ);
        var cancelʗ1 = cancel;
        var sʗ1 = s;
        var unblockServerʗ1 = unblockServer;
        goǃ(() => {
            cancelʗ1();
            ᐸꟷ(unblockServerʗ1);
            _ = sʗ1.Close();
        });
        var cli = Client(c, testConfig);
        // Initiates client side handshake, which will block until the client hello is read
        // by the server, unless the cancellation works.
        var err = cli.HandshakeContext(ctx);
        if (err == nil) {
            t.Fatal(clientHandshakeDidNotˢ);
        }
        if (!AreEqual(err, context.Canceled)) {
            t.Errorf("Unexpected client handshake error: %v", err);
        }
        if (runtime.GOOS == "js"u8 || runtime.GOOS == "wasip1"u8) {
            t.Skip(connCloseDoesNotErrorAsˢ);
        }
        err = cli.Close();
        if (err == nil) {
            t.Error(clientConnectionWasNotˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestTLS13OnlyClientHelloCipherSuite_tls13Tests /*dyn*/ {
    internal @string name;
    internal slice<uint16> ciphers;
}

// TestTLS13OnlyClientHelloCipherSuite tests that when a client states that
// it only supports TLS 1.3, it correctly advertises only TLS 1.3 ciphers.
public static void TestTLS13OnlyClientHelloCipherSuite(ж<testing.T> t) {
    var tls13Tests = slice<TestTLS13OnlyClientHelloCipherSuite_tls13Tests>([
        new(
            name: "nil",
            ciphers: default
        ),
        new(
            name: "empty",
            ciphers: slice<uint16>([])
        ),
        new(
            name: "some TLS 1.2 cipher",
            ciphers: slice<uint16>([TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256])
        ),
        new(
            name: "some TLS 1.3 cipher",
            ciphers: slice<uint16>([TLS_AES_128_GCM_SHA256])
        ),
        new(
            name: "some TLS 1.2 and 1.3 ciphers",
            ciphers: slice<uint16>([TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_AES_256_GCM_SHA384])
        )
    ]);
    foreach (var (_, vᴛ1) in tls13Tests) {
        ref var tt = ref heap(new TestTLS13OnlyClientHelloCipherSuite_tls13Tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            testTLS13OnlyClientHelloCipherSuite(tΔ1, ttʗ1.ciphers);
        });
    }
}

internal static void testTLS13OnlyClientHelloCipherSuite(ж<testing.T> t, slice<uint16> ciphers) {
    var serverConfig = Ꮡ(new Config(
        Certificates: testConfig.Certificates,
        GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> chi) => {
            var expectedCiphersuites = defaultCipherSuitesTLS13NoAES;
            if (fips140tls.Required()) {
                expectedCiphersuites = allowedCipherSuitesTLS13FIPS;
            }
            if (len(chi.CipherSuites) != len(expectedCiphersuites)) {
                t.Errorf("only TLS 1.3 suites should be advertised, got=%x", chi.CipherSuites);
            } else {
                foreach (var i in range(expectedCiphersuites)) {
                    {
                        var (want, got) = (expectedCiphersuites[i], chi.CipherSuites[i]); if (want != got) {
                            t.Errorf("cipher at index %d does not match, want=%x, got=%x", i, want, got);
                        }
                    }
                }
            }
            return (default, default);
        }
    ));
    var clientConfig = Ꮡ(new Config(
        MinVersion: VersionTLS13, // client only supports TLS 1.3
        CipherSuites: ciphers,
        InsecureSkipVerify: true
    ));
    {
        var (_, _, err) = testHandshake(t, clientConfig, serverConfig); if (err != nil) {
            t.Fatalf("handshake failed: %s", err);
        }
    }
}

// discardConn wraps a net.Conn but discards all writes, but reports that they happened.
internal partial struct discardConn {
    /*embed*/ public net_package.Conn Conn;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static error Close(this discardConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this discardConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this discardConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this discardConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this discardConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this discardConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *discardConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this discardConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint, error) Write(this ref discardConn dc, slice<byte> data) {
    return (len(data), default);
}

// largeRSAKeyCertPEM contains a 8193 bit RSA key
internal static ref readonly @string largeRSAKeyCertPEM => ref largeRSAKeyCertPEMᵇ;
private static readonly @string largeRSAKeyCertPEMᵇ = """
-----BEGIN CERTIFICATE-----
MIIInjCCBIWgAwIBAgIBAjANBgkqhkiG9w0BAQsFADASMRAwDgYDVQQDEwd0ZXN0
aW5nMB4XDTIzMDYwNzIxMjMzNloXDTIzMDYwNzIzMjMzNlowEjEQMA4GA1UEAxMH
dGVzdGluZzCCBCIwDQYJKoZIhvcNAQEBBQADggQPADCCBAoCggQBAWdHsf6Rh2Ca
n2SQwn4t4OQrOjbLLdGE1pM6TBKKrHUFy62uEL8atNjlcfXIsa4aEu3xNGiqxqur
ZectlkZbm0FkaaQ1Wr9oikDY3KfjuaXdPdO/XC/h8AKNxlDOylyXwUSK/CuYb+1j
gy8yF5QFvVfwW/xwTlHmhUeSkVSQPosfQ6yXNNsmMzkd+ZPWLrfq4R+wiNtwYGu0
WSBcI/M9o8/vrNLnIppoiBJJ13j9CR1ToEAzOFh9wwRWLY10oZhoh1ONN1KQURx4
qedzvvP2DSjZbUccdvl2rBGvZpzfOiFdm1FCnxB0c72Cqx+GTHXBFf8bsa7KHky9
sNO1GUanbq17WoDNgwbY6H51bfShqv0CErxatwWox3we4EcAmFHPVTCYL1oWVMGo
a3Eth91NZj+b/nGhF9lhHKGzXSv9brmLLkfvM1jA6XhNhA7BQ5Vz67lj2j3XfXdh
t/BU5pBXbL4Ut4mIhT1YnKXAjX2/LF5RHQTE8Vwkx5JAEKZyUEGOReD/B+7GOrLp
HduMT9vZAc5aR2k9I8qq1zBAzsL69lyQNAPaDYd1BIAjUety9gAYaSQffCgAgpRO
Gt+DYvxS+7AT/yEd5h74MU2AH7KrAkbXOtlwupiGwhMVTstncDJWXMJqbBhyHPF8
3UmZH0hbL4PYmzSj9LDWQQXI2tv6vrCpfts3Cqhqxz9vRpgY7t1Wu6l/r+KxYYz3
1pcGpPvRmPh0DJm7cPTiXqPnZcPt+ulSaSdlxmd19OnvG5awp0fXhxryZVwuiT8G
VDkhyARrxYrdjlINsZJZbQjO0t8ketXAELJOnbFXXzeCOosyOHkLwsqOO96AVJA8
45ZVL5m95ClGy0RSrjVIkXsxTAMVG6SPAqKwk6vmTdRGuSPS4rhgckPVDHmccmuq
dfnT2YkX+wB2/M3oCgU+s30fAHGkbGZ0pCdNbFYFZLiH0iiMbTDl/0L/z7IdK0nH
GLHVE7apPraKC6xl6rPWsD2iSfrmtIPQa0+rqbIVvKP5JdfJ8J4alI+OxFw/znQe
V0/Rez0j22Fe119LZFFSXhRv+ZSvcq20xDwh00mzcumPWpYuCVPozA18yIhC9tNn
ALHndz0tDseIdy9vC71jQWy9iwri3ueN0DekMMF8JGzI1Z6BAFzgyAx3DkHtwHg7
B7qD0jPG5hJ5+yt323fYgJsuEAYoZ8/jzZ01pkX8bt+UsVN0DGnSGsI2ktnIIk3J
l+8krjmUy6EaW79nITwoOqaeHOIp8m3UkjEcoKOYrzHRKqRy+A09rY+m/cAQaafW
4xp0Zv7qZPLwnu0jsqB4jD8Ll9yPB02ndsoV6U5PeHzTkVhPml19jKUAwFfs7TJg
kXy+/xFhYVUCAwEAATANBgkqhkiG9w0BAQsFAAOCBAIAAQnZY77pMNeypfpba2WK
aDasT7dk2JqP0eukJCVPTN24Zca+xJNPdzuBATm/8SdZK9lddIbjSnWRsKvTnO2r
/rYdlPf3jM5uuJtb8+Uwwe1s+gszelGS9G/lzzq+ehWicRIq2PFcs8o3iQMfENiv
qILJ+xjcrvms5ZPDNahWkfRx3KCg8Q+/at2n5p7XYjMPYiLKHnDC+RE2b1qT20IZ
FhuK/fTWLmKbfYFNNga6GC4qcaZJ7x0pbm4SDTYp0tkhzcHzwKhidfNB5J2vNz6l
Ur6wiYwamFTLqcOwWo7rdvI+sSn05WQBv0QZlzFX+OAu0l7WQ7yU+noOxBhjvHds
14+r9qcQZg2q9kG+evopYZqYXRUNNlZKo9MRBXhfrISulFAc5lRFQIXMXnglvAu+
Ipz2gomEAOcOPNNVldhKAU94GAMJd/KfN0ZP7gX3YvPzuYU6XDhag5RTohXLm18w
5AF+ES3DOQ6ixu3DTf0D+6qrDuK+prdX8ivcdTQVNOQ+MIZeGSc6NWWOTaMGJ3lg
aZIxJUGdo6E7GBGiC1YTjgFKFbHzek1LRTh/LX3vbSudxwaG0HQxwsU9T4DWiMqa
Fkf2KteLEUA6HrR+0XlAZrhwoqAmrJ+8lCFX3V0gE9lpENfVHlFXDGyx10DpTB28
DdjnY3F7EPWNzwf9P3oNT69CKW3Bk6VVr3ROOJtDxVu1ioWo3TaXltQ0VOnap2Pu
sa5wfrpfwBDuAS9JCDg4ttNp2nW3F7tgXC6xPqw5pvGwUppEw9XNrqV8TZrxduuv
rQ3NyZ7KSzIpmFlD3UwV/fGfz3UQmHS6Ng1evrUID9DjfYNfRqSGIGjDfxGtYD+j
Z1gLJZuhjJpNtwBkKRtlNtrCWCJK2hidK/foxwD7kwAPo2I9FjpltxCRywZUs07X
KwXTfBR9v6ij1LV6K58hFS+8ezZyZ05CeVBFkMQdclTOSfuPxlMkQOtjp8QWDj+F
j/MYziT5KBkHvcbrjdRtUJIAi4N7zCsPZtjik918AK1WBNRVqPbrgq/XSEXMfuvs
6JbfK0B76vdBDRtJFC1JsvnIrGbUztxXzyQwFLaR/AjVJqpVlysLWzPKWVX6/+SJ
u1NQOl2E8P6ycyBsuGnO89p0S4F8cMRcI2X1XQsZ7/q0NBrOMaEp5T3SrWo9GiQ3
o2SBdbs3Y6MBPBtTu977Z/0RO63J3M5i2tjUiDfrFy7+VRLKr7qQ7JibohyB8QaR
9tedgjn2f+of7PnP/PEl1cCphUZeHM7QKUMPT8dbqwmKtlYY43EHXcvNOT5IBk3X
9lwJoZk/B2i+ZMRNSP34ztAwtxmasPt6RAWGQpWCn9qmttAHAnMfDqe7F7jVR6rS
u58=
-----END CERTIFICATE-----
"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlsServerSentCertificateˢ => ref tlsServerSentCertificateˢᵇ;
private static readonly @string tlsServerSentCertificateˢᵇ = "tls: server sent certificate containing RSA key larger than 8192 bits"u8;
internal static ref readonly @string tlsClientSentCertificateˢ => ref tlsClientSentCertificateˢᵇ;
private static readonly @string tlsClientSentCertificateˢᵇ = "tls: client sent certificate containing RSA key larger than 8192 bits"u8;

public static void TestHandshakeRSATooBig(ж<testing.T> t) {
    var (testCert, _) = pem.Decode(slice<byte>(largeRSAKeyCertPEM));

    var c = Ꮡ(new Conn(conn: new tls_internal_test_package.discardConnжConn(Ꮡ(new discardConn())), config: testConfig.Clone()));

    @string expectedErr = tlsServerSentCertificateˢ;
    var err = c.verifyServerCertificate(slice<slice<byte>>([testCert.Bytes]));
    if (err == nil || err.Error() != expectedErr) {
        t.Errorf("Conn.verifyServerCertificate unexpected error: want %q, got %q", expectedErr, err);
    }

    expectedErr = tlsClientSentCertificateˢ;
    err = c.processCertsFromClient(new Certificate(ΔCertificate: slice<slice<byte>>([testCert.Bytes])));
    if (err == nil || err.Error() != expectedErr) {
        t.Errorf("Conn.processCertsFromClient unexpected error: want %q, got %q", expectedErr, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string callbackErrˢ => ref callbackErrˢᵇ;
private static readonly @string callbackErrˢᵇ = "callback err"u8;

internal partial struct TestTLS13ECHRejectionCallbacks_type /*dyn*/ {
    internal @string name;
    internal @string expectedErr;
    internal Func<global::go.crypto.tls_package.ΔConnectionState, error> verifyConnection;
    internal Func<slice<slice<byte>>, slice<slice<ж<Δx509.Certificate>>>, error> verifyPeerCertificate;
    internal Func<global::go.crypto.tls_package.ΔConnectionState, error> encryptedClientHelloRejectionVerify;
}

public static void TestTLS13ECHRejectionCallbacks(ж<testing.T> t) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test"),
        DNSNames: slice<@string>(["example.golang"]),
        NotBefore: testConfig.Time().Add(-time_package.ΔHour),
        NotAfter: testConfig.Time().Add(time_package.ΔHour)
    ));
    (var certDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    (var cert, err) = Δx509.ParseCertificate(certDER);
    if (err != nil) {
        t.Fatal(err);
    }

    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    serverConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([
        new(
            ΔCertificate: slice<slice<byte>>([certDER]),
            PrivateKey: k.OrTypedNil()
        )
    ]);
    serverConfig.MinVersion = VersionTLS13;
    clientConfig.RootCAs = Δx509.NewCertPool();
    clientConfig.RootCAs.AddCert(cert);
    clientConfig.MinVersion = VersionTLS13;
    (clientConfig.EncryptedClientHelloConfigList, _) = hex.DecodeString("0041fe0d003d0100200020204bed0a11fc0dde595a9b78d966b0011128eb83f65d3c91c1cc5ac786cd246f000400010001ff0e6578616d706c652e676f6c616e670000");
    clientConfig.ServerName = exampleGolangˢ;

    foreach (var (_, vᴛ1) in slice<TestTLS13ECHRejectionCallbacks_type>([
        new(
            name: "no callbacks",
            expectedErr: "tls: server rejected ECH"
        ),
        new(
            name: "EncryptedClientHelloRejectionVerify, no err",
            encryptedClientHelloRejectionVerify: (global::go.crypto.tls_package.ΔConnectionState _Δp0) => default,
            expectedErr: "tls: server rejected ECH"
        ),
        new(
            name: "EncryptedClientHelloRejectionVerify, err",
            encryptedClientHelloRejectionVerify: (global::go.crypto.tls_package.ΔConnectionState _Δp0) => errors.New(callbackErrˢ),
            // testHandshake returns the server side error, so we just need to
            // check alertBadCertificate was sent
            expectedErr: "callback err"
        ),
        new(
            name: "VerifyConnection, err",
            verifyConnection: (global::go.crypto.tls_package.ΔConnectionState _Δp0) => errors.New(callbackErrˢ),
            expectedErr: "tls: server rejected ECH"
        ),
        new(
            name: "VerifyPeerCertificate, err",
            verifyPeerCertificate: (slice<slice<byte>> _Δp0, slice<slice<ж<Δx509.Certificate>>> _Δp1) => errors.New(callbackErrˢ),
            expectedErr: "tls: server rejected ECH"
        )
    ])) {
        ref var tc = ref heap(new TestTLS13ECHRejectionCallbacks_type(), out var Ꮡtc);
        tc = vᴛ1;

        var clientConfigʗ1 = clientConfig;
        var serverConfigʗ1 = serverConfig;
        var tcʗ1 = tc;
        t.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            var (c, s) = localPipe(new tls_test_package.testing_TжTB(tΔ1));
            var done = new channel<error>(0);

            var doneʗ1 = done;
            var sʗ1 = s;
            var serverConfigʗ2 = serverConfigʗ1;
            goǃ(() => {
                var serverErr = Server(sʗ1, serverConfigʗ2).Handshake();
                sʗ1.Close();
                doneʗ1.ᐸꟷ(serverErr);
            });

            var cConfig = clientConfigʗ1.Clone();
            cConfig.VerifyConnection = tcʗ1.verifyConnection;
            cConfig.VerifyPeerCertificate = tcʗ1.verifyPeerCertificate;
            cConfig.EncryptedClientHelloRejectionVerify = tcʗ1.encryptedClientHelloRejectionVerify;

            var clientErr = Client(c, cConfig).Handshake();
            c.Close();

            if (tcʗ1.expectedErr == ""u8 && clientErr != nil) {
                tΔ1.Fatalf("unexpected err: %s", clientErr);
            } else if (clientErr != nil && tcʗ1.expectedErr != clientErr.Error()) {
                tΔ1.Fatalf("unexpected err: got %q, want %q", clientErr, tcʗ1.expectedErr);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string serverTlsClientOfferedˢ => ref serverTlsClientOfferedˢᵇ;
private static readonly @string serverTlsClientOfferedˢᵇ = "server: tls: client offered only unsupported versions: [304]\nclient: remote error: tls: protocol version not supported"u8;

public static void TestECHTLS12Server(ж<testing.T> t) {
    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());

    serverConfig.MaxVersion = VersionTLS12;
    clientConfig.MinVersion = 0;

    (clientConfig.EncryptedClientHelloConfigList, _) = hex.DecodeString("0041fe0d003d0100200020204bed0a11fc0dde595a9b78d966b0011128eb83f65d3c91c1cc5ac786cd246f000400010001ff0e6578616d706c652e676f6c616e670000");

    @string expectedErr = serverTlsClientOfferedˢ;
    var (_, _, err) = testHandshake(t, clientConfig, serverConfig);
    if (err == nil || err.Error() != expectedErr) {
        t.Fatalf("unexpected handshake error: got %q, want %q", err, expectedErr);
    }
}

} // end tls_internal_test_package
