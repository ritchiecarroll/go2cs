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

internal partial struct opensslInputEvent /*num:nint*/;

// Note: see comment in handshake_test.go for details of how the reference
// tests work.
internal static opensslInputEvent opensslRenegotiate => /* iota */ 0;
internal static opensslInputEvent opensslSendSentinel => 1;
internal static opensslInputEvent opensslKeyUpdate => 2;

internal static readonly @string opensslSentinel = "SENTINEL\n"u8;

internal partial struct opensslInput /*chan opensslInputEvent*/;

internal static (nint n, error err) Read(this opensslInput i, slice<byte> buf) {
    foreach (var @event in i) {
        var exprᴛ1 = @event;
        if (exprᴛ1 == opensslRenegotiate) {
            return (copy(buf, slice<byte>("R\n"u8)), default!);
        }
        if (exprᴛ1 == opensslKeyUpdate) {
            return (copy(buf, slice<byte>("K\n"u8)), default!);
        }
        if (exprᴛ1 == opensslSendSentinel) {
            return (copy(buf, slice<byte>(opensslSentinel)), default!);
        }
        { /* default: */
            throw panic("unknown event");
        }

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
    return Ꮡ(new opensslOutputSink(new channel<EmptyStruct>(0), new channel<EmptyStruct>(0), default!, default!));
}

// opensslEndOfHandshake is a message that the “openssl s_server” tool will
// print when a handshake completes if run with “-state”.
internal static readonly @string opensslEndOfHandshake = "SSL_accept:SSLv3/TLS write finished"u8;

// opensslReadKeyUpdate is a message that the “openssl s_server” tool will
// print when a KeyUpdate message is received if run with “-state”.
internal static readonly @string opensslReadKeyUpdate = "SSL_accept:TLSv1.3 read client key update"u8;

internal static (nint n, error err) Write(this ref opensslOutputSink o, slice<byte> data) {
    o.line = appendꓸꓸꓸ(o.line, data);
    o.all = appendꓸꓸꓸ(o.all, data);
    while (ᐧ) {
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
    return (len(data), default!);
}

internal static @string String(this ref opensslOutputSink o) {
    return ((@string)o.all);
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

internal static slice<@string> serverCommand = new @string[]{"openssl"u8, "s_server"u8, "-no_ticket"u8, "-num_tickets"u8, "0"u8}.slice();

// connFromCommand starts the reference server process, connects to it and
// returns a recordingConn for the connection. The stdin return value is an
// opensslInput for the stdin of the child process. It must be closed before
// Waiting for child.
internal static (ж<recordingConn> conn, ж<exec.Cmd> child, opensslInput stdin, ж<opensslOutputSink> stdout, error err) connFromCommand(this ж<clientTest> Ꮡtest) {
    ж<recordingConn> conn = default!;
    ж<exec.Cmd> child = default!;
    opensslInput stdin = default!;
    ж<opensslOutputSink> stdout = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        var cert = testRSACertificate;
        if (len(test.cert) > 0) {
            cert = test.cert;
        }
        @string certPath = tempFile(((@string)cert));
        defer(os.Remove, certPath, ref ᒐ);
        any key = testRSAPrivateKey.OrTypedNil();
        if (test.key != default!) {
            key = test.key;
        }
        (var derBytes, err) = Δx509.MarshalPKCS8PrivateKey(key);
        if (err != default!) {
            throw panic(err);
        }
        ref var pemOut = ref heap(new bytes.Buffer(), out var ᏑpemOut);
        pem.Encode(new tls_test_package.bytes_BufferжWriter(ᏑpemOut), Ꮡ(new pem.Block(Type: "PRIVATE KEY"u8, Bytes: derBytes)));
        @string keyPath = tempFile(ᏑpemOut.String());
        defer(os.Remove, keyPath, ref ᒐ);
        slice<@string> command = default!;
        command = appendꓸꓸꓸ(command, serverCommand);
        command = appendꓸꓸꓸ(command, test.args);
        command = append(command, "-cert"u8, certPath, "-certform", "DER", "-key", keyPath);
        // serverPort contains the port that OpenSSL will listen on. OpenSSL
        // can't take "0" as an argument here so we have to pick a number and
        // hope that it's not in use on the machine. Since this only occurs
        // when -update is given and thus when there's a human watching the
        // test, this isn't too bad.
        const nint serverPort = 24323;
        command = append(command, "-accept"u8, strconv.Itoa(serverPort));
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
            command = append(command, "-serverinfo"u8, serverInfoPath);
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
        stdin = ((opensslInput)new channel<opensslInputEvent>(0));
        cmd.Value.Stdin = stdin;
        var @out = newOpensslOutputSink();
        cmd.Value.Stdout = new tls_internal_test_package.opensslOutputSinkжWriter(@out);
        cmd.Value.Stderr = new tls_internal_test_package.opensslOutputSinkжWriter(@out);
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                (conn, child, stdin, stdout, err) = (default!, default!, default!, default!, errΔ1); goto ᒐdone;
            }
        }
        // OpenSSL does print an "ACCEPT" banner, but it does so *before*
        // opening the listening socket, so we can't use that to wait until it
        // has started listening. Thus we are forced to poll until we get a
        // connection.
        net.Conn tcpConn = default!;
        for (nuint i = (nuint)0; i < 5; i++) {
            var (ᴛ1, ᴛ2) = net.DialTCP(tcpˢ, nil, Ꮡ(new net.TCPAddr(
                IP: net.IPv4(127, 0, 0, 1),
                Port: serverPort
            )));
            (tcpConn, err) = (new net.TCPConnжConn(ᴛ1), ᴛ2);
            if (err == default!) {
                break;
            }
            time_package.Sleep(((time.Duration)((int64)1 << (int)(i))) * 5 * time_package.Millisecond);
        }
        if (err != default!) {
            close<opensslInputEvent>(stdin);
            (~cmd).Process.Kill();
            err = fmt.Errorf("error connecting to the OpenSSL server: %v (%v)\n\n%s"u8, err, cmd.Wait(), @out.OrTypedNil());
            (conn, child, stdin, stdout, err) = (default!, default!, default!, default!, err); goto ᒐdone;
        }
        var record = Ꮡ(new recordingConn(
            Conn: tcpConn
        ));
        (conn, child, stdin, stdout, err) = (record, cmd, stdin, @out, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (conn, child, stdin, stdout, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;

internal static @string dataPath(this ref clientTest test) {
    return filepath.Join(testdataˢ, "Client-" + test.name);
}

internal static (slice<slice<byte>> flows, error err) loadData(this ж<clientTest> Ꮡtest) {
    slice<slice<byte>> flows = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        (var @in, err) = os.Open(test.dataPath());
        if (err != default!) {
            (flows, err) = (default!, err); goto ᒐdone;
        }
        var inʗ1 = @in;
        defer(() => inʗ1.Close(), ref ᒐ);
        (flows, err) = parseTestData(new tls_test_package.os_FileжReader(@in));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (flows, err);
}

internal static partial void run(this ж<clientTest> Ꮡtest, ж<testing.T> Ꮡt, bool write) {
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();
        ref var t = ref Ꮡt.DerefOrNull();

        net.Conn clientConn = default!;
        ж<recordingConn> recordingConn = default!;
        ж<exec.Cmd> childProcess = default!;
        opensslInput stdin = default!;
        ж<opensslOutputSink> stdout = default!;
        if (write){
            error err = default!;
            (recordingConn, childProcess, stdin, stdout, err) = Ꮡtest.connFromCommand();
            if (err != default!) {
                Ꮡt.Fatalf("Failed to start subcommand: %s"u8, err);
            }
            clientConn = new tls_internal_test_package.recordingConnжConn(recordingConn);
            var stdoutʗ1 = stdout;
            defer(() => {
                if (Ꮡt.Failed()) {
                    Ꮡt.Logf("OpenSSL output:\n\n%s"u8, (~stdoutʗ1).all);
                }
            }, ref ᒐ);
        } else {
            var (flows, err) = Ꮡtest.loadData();
            if (err != default!) {
                Ꮡt.Fatalf("failed to load data from %s: %v"u8, test.dataPath(), err);
            }
            clientConn = new tls_internal_test_package.replayingConnжConn(Ꮡ(new replayingConn(t: new tls_test_package.testing_TжTB(Ꮡt), flows: flows, reading: false)));
        }
        var config = test.config;
        if (config == nil) {
            config = testConfig;
        }
        var client = Client(clientConn, config);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        {
            var (_, err) = client.Write(slice<byte>("hello\n"u8)); if (err != default!) {
                Ꮡt.Errorf("Client.Write failed: %s"u8, err);
                return;
            }
        }
        for (nint iᴛ1 = 1; iᴛ1 <= test.numRenegotiations; iᴛ1++) {
            var i = iᴛ1;
            // The initial handshake will generate a
            // handshakeComplete signal which needs to be quashed.
            if (i == 1 && write) {
                ᐸꟷ((~stdout).handshakeComplete);
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
                    if (Ꮡtest.Value.checkRenegotiationError != default!) {
                        var newErr = Ꮡtest.Value.checkRenegotiationError(i, err);
                        if (err != default! && newErr == default!) {
                            return;
                        }
                        err = newErr;
                    }
                    if (err != default!) {
                        Ꮡt.Errorf("Client.Read failed after renegotiation #%d: %s"u8, i, err);
                        return;
                    }
                    buf = buf.slice(0, n);
                    if (!bytes.Equal(slice<byte>(opensslSentinel), buf)) {
                        Ꮡt.Errorf("Client.Read returned %q, but wanted %q"u8, ((@string)buf), opensslSentinel);
                    }
                    {
                        nint expected = i + 1; if ((~clientʗ2).handshakes != expected) {
                            Ꮡt.Errorf("client should have recorded %d handshakes, but believes that %d have occurred"u8, expected, (~clientʗ2).handshakes);
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            if (write && test.renegotiationExpectedToFail != i) {
                ᐸꟷ((~stdout).handshakeComplete);
                stdin.ᐸꟷ(opensslSendSentinel);
            }
            ᐸꟷ(signalChan);
        }
        if (test.sendKeyUpdate) {
            if (write) {
                ᐸꟷ((~stdout).handshakeComplete);
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
                    if (err != default!) {
                        Ꮡt.Errorf("Client.Read failed after KeyUpdate: %s"u8, err);
                        return;
                    }
                    buf = buf.slice(0, n);
                    if (!bytes.Equal(slice<byte>(opensslSentinel), buf)) {
                        Ꮡt.Errorf("Client.Read returned %q, but wanted %q"u8, ((@string)buf), opensslSentinel);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            if (write) {
                // There's no real reason to wait for the client KeyUpdate to
                // send data with the new server keys, except that s_server
                // drops writes if they are sent at the wrong time.
                ᐸꟷ((~stdout).readKeyUpdate);
                stdin.ᐸꟷ(opensslSendSentinel);
            }
            ᐸꟷ(doneRead);
            {
                var (_, err) = client.Write(slice<byte>("hello again\n"u8)); if (err != default!) {
                    Ꮡt.Errorf("Client.Write failed: %s"u8, err);
                    return;
                }
            }
        }
        if (test.validate != default!) {
            {
                var err = test.validate(client.ConnectionState()); if (err != default!) {
                    Ꮡt.Errorf("validate callback returned error: %s"u8, err);
                }
            }
        }
        // If the server sent us an alert after our last flight, give it a
        // chance to arrive.
        if (write && test.renegotiationExpectedToFail == 0) {
            {
                var err = peekError(new global::go.crypto.tls_package.ConnжConn(client)); if (err != default!) {
                    Ꮡt.Errorf("final Read returned an error: %s"u8, err);
                }
            }
        }
        if (write) {
            client.Close();
            @string path = test.dataPath();
            var (@out, err) = os.OpenFile(path, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 420);
            if (err != default!) {
                Ꮡt.Fatalf("Failed to create output file: %s"u8, err);
            }
            var outʗ1 = @out;
            defer(() => outʗ1.Close(), ref ᒐ);
            (~recordingConn).Conn.Close();
            close<opensslInputEvent>(stdin);
            (~childProcess).Process.Kill();
            childProcess.Wait();
            if (len((~recordingConn).flows) < 3) {
                Ꮡt.Fatalf("Client connection didn't work"u8);
            }
            recordingConn.WriteTo(new os.FileжWriter(@out));
            Ꮡt.Logf("Wrote %s\n"u8, path);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedlyReadDataˢ = "unexpectedly read data"u8;

// peekError does a read with a short timeout to check if the next read would
// cause an error, for example if there is an alert waiting on the wire.
internal static error peekError(net.Conn conn) {
    conn.SetReadDeadline(time_package.Now().Add(100 * time_package.Millisecond));
    {
        var (n, err) = conn.Read(new slice<byte>(1)); if (n != 0){
            return errors.New(unexpectedlyReadDataˢ);
        } else 
        if (err != default!) {
            {
                var (netErr, ok) = err._<netꓸError>(ᐧ); if (!ok || !netErr.Timeout()) {
                    return err;
                }
            }
        }
    }
    return default!;
}

internal static void runClientTestForVersion(ж<testing.T> Ꮡt, ж<clientTest> Ꮡtemplate, @string version, @string option) {
    ref var template = ref Ꮡtemplate.DerefOrNull();

    // Make a deep copy of the template before going parallel.
    ref var test = ref heap<clientTest>(out var Ꮡtest);
    test = template;
    if (template.config != nil) {
        test.config = template.config.Clone();
    }
    test.name = version + "-"u8 + test.name;
    test.args = appendꓸꓸꓸ(new @string[]{option}.slice(), test.args);
    runTestAndUpdateIfNeeded(Ꮡt, version, Ꮡtest.run, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlSv10ˢ = "TLSv10"u8;
internal static readonly @string tls1ˢ = "-tls1"u8;

internal static void runClientTestTLS10(ж<testing.T> Ꮡt, ж<clientTest> Ꮡtemplate) {
    runClientTestForVersion(Ꮡt, Ꮡtemplate, tlSv10ˢ, tls1ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlSv11ˢ = "TLSv11"u8;
internal static readonly @string tls11ˢ2 = "-tls1_1"u8;

internal static void runClientTestTLS11(ж<testing.T> Ꮡt, ж<clientTest> Ꮡtemplate) {
    runClientTestForVersion(Ꮡt, Ꮡtemplate, tlSv11ˢ, tls11ˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlSv12ˢ = "TLSv12"u8;
internal static readonly @string tls12ˢ2 = "-tls1_2"u8;

internal static void runClientTestTLS12(ж<testing.T> Ꮡt, ж<clientTest> Ꮡtemplate) {
    runClientTestForVersion(Ꮡt, Ꮡtemplate, tlSv12ˢ, tls12ˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlSv13ˢ = "TLSv13"u8;
internal static readonly @string tls13ˢ2 = "-tls1_3"u8;

internal static void runClientTestTLS13(ж<testing.T> Ꮡt, ж<clientTest> Ꮡtemplate) {
    runClientTestForVersion(Ꮡt, Ꮡtemplate, tlSv13ˢ, tls13ˢ2);
}

public static void TestHandshakeClientRSARC4(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "RSA-RC4"u8,
        args: new @string[]{"-cipher"u8, "RC4-SHA"u8}.slice()
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS11(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientRSAAES128GCM(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "AES128-GCM-SHA256"u8,
        args: new @string[]{"-cipher"u8, "AES128-GCM-SHA256"u8}.slice()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientRSAAES256GCM(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "AES256-GCM-SHA384"u8,
        args: new @string[]{"-cipher"u8, "AES256-GCM-SHA384"u8}.slice()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientECDHERSAAES(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-RSA-AES"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-AES128-SHA"u8}.slice()
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS11(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientECDHEECDSAAES(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-AES128-SHA"u8}.slice(),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS11(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientECDHEECDSAAESGCM(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES-GCM"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-AES128-GCM-SHA256"u8}.slice(),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientAES256GCMSHA384(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES256-GCM-SHA384"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-AES256-GCM-SHA384"u8}.slice(),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientAES128CBCSHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "AES128-SHA256"u8,
        args: new @string[]{"-cipher"u8, "AES128-SHA256"u8}.slice()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientECDHERSAAES128CBCSHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-RSA-AES128-SHA256"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-AES128-SHA256"u8}.slice()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientECDHEECDSAAES128CBCSHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-AES128-SHA256"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-AES128-SHA256"u8}.slice(),
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientX25519(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519}.slice();
    var test = Ꮡ(new clientTest(
        name: "X25519-ECDHE"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-AES128-GCM-SHA256"u8, "-curves"u8, "X25519"u8}.slice(),
        config: config
    ));
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientP256(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice();
    var test = Ꮡ(new clientTest(
        name: "P256-ECDHE"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-AES128-GCM-SHA256"u8, "-curves"u8, "P-256"u8}.slice(),
        config: config
    ));
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedˢ = "expected HelloRetryRequest"u8;

public static void TestHandshakeClientHelloRetryRequest(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519, CurveP256}.slice();
    var test = Ꮡ(new clientTest(
        name: "HelloRetryRequest"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-AES128-GCM-SHA256"u8, "-curves"u8, "P-256"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState cs) => {
            if (!cs.testingOnlyDidHRR) {
                return errors.New(expectedˢ);
            }
            return default!;
        }
    ));
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientECDHERSAChaCha20(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CipherSuites = new uint16[]{TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305}.slice();
    var test = Ꮡ(new clientTest(
        name: "ECDHE-RSA-CHACHA20-POLY1305"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8}.slice(),
        config: config
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientECDHEECDSAChaCha20(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CipherSuites = new uint16[]{TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305}.slice();
    var test = Ꮡ(new clientTest(
        name: "ECDHE-ECDSA-CHACHA20-POLY1305"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-CHACHA20-POLY1305"u8}.slice(),
        config: config,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientAES128SHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "AES128-SHA256"u8,
        args: new @string[]{"-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8}.slice()
    ));
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientAES256SHA384(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "AES256-SHA384"u8,
        args: new @string[]{"-ciphersuites"u8, "TLS_AES_256_GCM_SHA384"u8}.slice()
    ));
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientCHACHA20SHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "CHACHA20-SHA256"u8,
        args: new @string[]{"-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice()
    ));
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientECDSATLS13(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ECDSA"u8,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientEd25519(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "Ed25519"u8,
        cert: testEd25519Certificate,
        key: testEd25519PrivateKey
    ));
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientEd25519CertificatePEM), slice<byte>(clientEd25519KeyPEM));
    config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{cert}.slice();
    test = Ꮡ(new clientTest(
        name: "ClientCert-Ed25519"u8,
        args: new @string[]{"-Verify"u8, "1"u8}.slice(),
        config: config
    ));
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientCertRSA(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientCertificatePEM), slice<byte>(clientKeyPEM));
    config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{cert}.slice();
    var test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-RSA"u8,
        args: new @string[]{"-cipher"u8, "AES128"u8, "-Verify"u8, "1"u8}.slice(),
        config: config
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
    test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-ECDSA"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-AES128-SHA"u8, "-Verify"u8, "1"u8}.slice(),
        config: config,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
    test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-AES256-GCM-SHA384"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-RSA-AES256-GCM-SHA384"u8, "-Verify"u8, "1"u8}.slice(),
        config: config,
        cert: testRSACertificate,
        key: testRSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestHandshakeClientCertECDSA(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientECDSACertificatePEM), slice<byte>(clientECDSAKeyPEM));
    config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{cert}.slice();
    var test = Ꮡ(new clientTest(
        name: "ClientCert-ECDSA-RSA"u8,
        args: new @string[]{"-cipher"u8, "AES128"u8, "-Verify"u8, "1"u8}.slice(),
        config: config
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
    test = Ꮡ(new clientTest(
        name: "ClientCert-ECDSA-ECDSA"u8,
        args: new @string[]{"-cipher"u8, "ECDHE-ECDSA-AES128-SHA"u8, "-Verify"u8, "1"u8}.slice(),
        config: config,
        cert: testECDSACertificate,
        key: testECDSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
}

// TestHandshakeClientCertRSAPSS tests rsa_pss_rsae_sha256 signatures from both
// client and server certificates. It also serves from both sides a certificate
// signed itself with RSA-PSS, mostly to check that crypto/x509 chain validation
// works.
public static void TestHandshakeClientCertRSAPSS(ж<testing.T> Ꮡt) {
    var (cert, err) = Δx509.ParseCertificate(testRSAPSSCertificate);
    if (err != default!) {
        throw panic(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(cert);
    var config = testConfig.Clone();
    // Use GetClientCertificate to bypass the client certificate selection logic.
    config.Value.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> _) => (Ꮡ(new Certificate(
            ΔCertificate: new slice<byte>[]{testRSAPSSCertificate}.slice(),
            PrivateKey: testRSAPrivateKey.OrTypedNil()
        )), default!);
    config.Value.RootCAs = rootCAs;
    var test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-RSAPSS"u8,
        args: new @string[]{"-cipher"u8, "AES128"u8, "-Verify"u8, "1"u8, "-client_sigalgs"u8,
            "rsa_pss_rsae_sha256"u8, "-sigalgs"u8, "rsa_pss_rsae_sha256"u8}.slice(),
        config: config,
        cert: testRSAPSSCertificate,
        key: testRSAPrivateKey.OrTypedNil()
    ));
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
}

public static void TestHandshakeClientCertRSAPKCS1v15(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    var (cert, _) = X509KeyPair(slice<byte>(clientCertificatePEM), slice<byte>(clientKeyPEM));
    config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{cert}.slice();
    var test = Ꮡ(new clientTest(
        name: "ClientCert-RSA-RSAPKCS1v15"u8,
        args: new @string[]{"-cipher"u8, "AES128"u8, "-Verify"u8, "1"u8, "-client_sigalgs"u8,
            "rsa_pkcs1_sha256"u8, "-sigalgs"u8, "rsa_pkcs1_sha256"u8}.slice(),
        config: config
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestClientKeyUpdate(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "KeyUpdate"u8,
        args: new @string[]{"-state"u8}.slice(),
        sendKeyUpdate: true
    ));
    runClientTestTLS13(Ꮡt, test);
}

public static void TestResumption(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testResumption(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testResumption(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ2 = (@string)"skipping in -short mode"u8;
internal static readonly @string resumeˢ = "Resume"u8;
internal static readonly object ticketDidnTChangeAfterˢ = (@string)"ticket didn't change after resumption"u8;
internal static readonly @string resumeWithOldTicketˢ = "ResumeWithOldTicket"u8;
internal static readonly object oldFirstTicketMatchesTheˢ = (@string)"old first ticket matches the fresh one"u8;
internal static readonly @string resumeWithExpiredTicketˢ = "ResumeWithExpiredTicket"u8;
internal static readonly object expiredFirstTicketˢ = (@string)"expired first ticket matches the fresh one"u8;
internal static readonly @string invalidSessionTicketKeyˢ = "InvalidSessionTicketKey"u8;
internal static readonly @string keyChangeˢ = "KeyChange"u8;
internal static readonly object newTicketWasnTIncludedˢ = (@string)"new ticket wasn't included while resuming"u8;
internal static readonly @string keyChangeFinishˢ = "KeyChangeFinish"u8;
internal static readonly @string oldSessionTicketˢ = "OldSessionTicket"u8;
internal static readonly @string expiredSessionTicketˢ = "ExpiredSessionTicket"u8;
internal static readonly object newTicketWasnTProvidedˢ = (@string)"new ticket wasn't provided after old ticket expired"u8;
internal static readonly @string getFreshSessionTicketˢ = "GetFreshSessionTicket"u8;
internal static readonly @string freshConfigˢ = "FreshConfig"u8;
internal static readonly @string differentCipherSuiteˢ = "DifferentCipherSuite"u8;
internal static readonly @string withoutSessionTicketˢ = "WithoutSessionTicket"u8;
internal static readonly @string initialHandshakeˢ = "InitialHandshake"u8;
internal static readonly @string withHelloRetryRequestˢ = "WithHelloRetryRequest"u8;
internal static readonly @string withClientCertificatesˢ = "WithClientCertificates"u8;
internal static readonly @string fetchTicketToCorruptˢ = "FetchTicketToCorrupt"u8;
internal static readonly @string afterHandshakeFailureˢ = "AfterHandshakeFailure"u8;
internal static readonly @string withoutSessionCacheˢ = "WithoutSessionCache"u8;
internal static readonly @string beforeSerializingCacheˢ = "BeforeSerializingCache"u8;
internal static readonly @string withSerializingCacheˢ = "WithSerializingCache"u8;

internal static void testResumption(ж<testing.T> Ꮡt, uint16 version) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ2);
    }
    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    var testCertificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
    ref var serverConfig = ref heap<ж<global::go.crypto.tls_package.Config>>(out var ᏑserverConfig);
    serverConfig = Ꮡ(new Config(
        MaxVersion: version,
        CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384}.slice(),
        Certificates: testCertificates,
        Time: testTime
    ));
    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != default!) {
        throw panic(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);
    var clientConfig = Ꮡ(new Config(
        MaxVersion: version,
        CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        ClientSessionCache: NewLRUClientSessionCache(32),
        RootCAs: rootCAs,
        ServerName: "example.golang"u8,
        Time: testTime
    ));
    var clientConfigʗ1 = clientConfig;
    void testResumeState(@string test, bool didResume) {
        Ꮡt.Helper();
        var (_, hs, errΔ1) = testHandshake(Ꮡt, clientConfigʗ1, ᏑserverConfig.ValueSlot);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("%s: handshake failed: %s"u8, test, errΔ1);
        }
        if (hs.DidResume != didResume) {
            Ꮡt.Fatalf("%s resumed: %v, expected: %v"u8, test, hs.DidResume, didResume);
        }
        if (didResume && (hs.PeerCertificates == default! || hs.VerifiedChains == default!)) {
            Ꮡt.Fatalf("expected non-nil certificates after resumption. Got peerCertificates: %#v, verifiedCertificates: %#v"u8, hs.PeerCertificates, hs.VerifiedChains);
        }
        {
            @string got = hs.ServerName;
            @string want = clientConfigʗ1.Value.ServerName; if (got != want) {
                Ꮡt.Errorf("%s: server name %s, want %s"u8, test, got, want);
            }
        }
    }
    var clientConfigʗ2 = clientConfig;
    slice<byte> getTicket() => (~(~(~(~(~(~clientConfigʗ2).ClientSessionCache._<ж<global::go.crypto.tls_package.lruSessionCache>>()).q.Front()).Value._<ж<global::go.crypto.tls_package.lruSessionCacheEntry>>()).state).session).ticket;
    var clientConfigʗ3 = clientConfig;
    void deleteTicket() {
        @string ticketKey = (~(~(~clientConfigʗ3).ClientSessionCache._<ж<global::go.crypto.tls_package.lruSessionCache>>()).q.Front()).Value._<ж<global::go.crypto.tls_package.lruSessionCacheEntry>>().Value.sessionKey;
        (~clientConfigʗ3).ClientSessionCache.Put(ticketKey, nil);
    }
    var clientConfigʗ4 = clientConfig;
    void corruptTicket() {
        (~(~(~clientConfigʗ4).ClientSessionCache._<ж<global::go.crypto.tls_package.lruSessionCache>>()).q.Front()).Value._<ж<global::go.crypto.tls_package.lruSessionCacheEntry>>().Value.state.Value.session.Value.secret[0] ^= (byte)(0xff);
    }
    array<byte> randomKey() {
        array<byte> k = new(32);
        {
            var (_, errΔ2) = io.ReadFull(ᏑserverConfig.ValueSlot.rand(), k[..]); if (errΔ2 != default!) {
                Ꮡt.Fatalf("Failed to read new SessionTicketKey: %s"u8, errΔ2);
            }
        }
        return k.Clone();
    }
    testResumeState(handshakeˢ, false);
    var ticket = getTicket();
    testResumeState(resumeˢ, true);
    if (bytes.Equal(ticket, getTicket())) {
        Ꮡt.Fatal(ticketDidnTChangeAfterˢ);
    }
    // An old session ticket is replaced with a ticket encrypted with a fresh key.
    ticket = getTicket();
    serverConfig.Value.Time = () => testTime().Add((time.Duration)(86460000000000L));
    testResumeState(resumeWithOldTicketˢ, true);
    if (bytes.Equal(ticket, getTicket())) {
        Ꮡt.Fatal(oldFirstTicketMatchesTheˢ);
    }
    // Once the session master secret is expired, a full handshake should occur.
    ticket = getTicket();
    serverConfig.Value.Time = () => testTime().Add((time.Duration)(691260000000000L));
    testResumeState(resumeWithExpiredTicketˢ, false);
    if (bytes.Equal(ticket, getTicket())) {
        Ꮡt.Fatal(expiredFirstTicketˢ);
    }
    serverConfig.Value.Time = testTime; // reset the time back
    var key1 = randomKey();
    serverConfig.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{key1.Clone()}.slice(), 32));
    testResumeState(invalidSessionTicketKeyˢ, false);
    testResumeState("ResumeAfterInvalidSessionTicketKey"u8, true);
    var key2 = randomKey();
    serverConfig.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{key2.Clone(), key1.Clone()}.slice(), 32));
    ticket = getTicket();
    testResumeState(keyChangeˢ, true);
    if (bytes.Equal(ticket, getTicket())) {
        Ꮡt.Fatal(newTicketWasnTIncludedˢ);
    }
    testResumeState(keyChangeFinishˢ, true);
    // Age the session ticket a bit, but not yet expired.
    serverConfig.Value.Time = () => testTime().Add((time.Duration)(86460000000000L));
    testResumeState(oldSessionTicketˢ, true);
    ticket = getTicket();
    // Expire the session ticket, which would force a full handshake.
    serverConfig.Value.Time = () => testTime().Add((time.Duration)(691320000000000L));
    testResumeState(expiredSessionTicketˢ, false);
    if (bytes.Equal(ticket, getTicket())) {
        Ꮡt.Fatal(newTicketWasnTProvidedˢ);
    }
    // Age the session ticket a bit at a time, but don't expire it.
    var d = 0 * time_package.ΔHour;
    serverConfig.Value.Time = () => testTime().Add(d);
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
    if (version == VersionTLS13){
        testResumeState(expiredSessionTicketˢ, true);
    } else {
        testResumeState(expiredSessionTicketˢ, false);
    }
    if (bytes.Equal(ticket, getTicket())) {
        Ꮡt.Fatal(newTicketWasnTProvidedˢ);
    }
    // Reset serverConfig to ensure that calling SetSessionTicketKeys
    // before the serverConfig is used works.
    serverConfig = Ꮡ(new Config(
        MaxVersion: version,
        CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384}.slice(),
        Certificates: testCertificates,
        Time: testTime
    ));
    serverConfig.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{key2.Clone()}.slice(), 32));
    testResumeState(freshConfigˢ, true);
    // In TLS 1.3, cross-cipher suite resumption is allowed as long as the KDF
    // hash matches. Also, Config.CipherSuites does not apply to TLS 1.3.
    if (version != VersionTLS13) {
        clientConfig.Value.CipherSuites = new uint16[]{TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384}.slice();
        testResumeState(differentCipherSuiteˢ, false);
        testResumeState("DifferentCipherSuiteRecovers"u8, true);
    }
    deleteTicket();
    testResumeState(withoutSessionTicketˢ, false);
    // In TLS 1.3, HelloRetryRequest is sent after incorrect key share.
    // See https://www.rfc-editor.org/rfc/rfc8446#page-14.
    if (version == VersionTLS13) {
        deleteTicket();
        serverConfig = Ꮡ(new Config( // Use a different curve than the client to force a HelloRetryRequest.

            CurvePreferences: new global::go.crypto.tls_package.CurveID[]{CurveP521, CurveP384, CurveP256}.slice(),
            MaxVersion: version,
            Certificates: testCertificates,
            Time: testTime
        ));
        testResumeState(initialHandshakeˢ, false);
        testResumeState(withHelloRetryRequestˢ, true);
        // Reset serverConfig back.
        serverConfig = Ꮡ(new Config(
            MaxVersion: version,
            CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384}.slice(),
            Certificates: testCertificates,
            Time: testTime
        ));
    }
    // Session resumption should work when using client certificates
    deleteTicket();
    serverConfig.Value.ClientCAs = rootCAs;
    serverConfig.Value.ClientAuth = RequireAndVerifyClientCert;
    clientConfig.Value.Certificates = serverConfig.Value.Certificates;
    testResumeState(initialHandshakeˢ, false);
    testResumeState(withClientCertificatesˢ, true);
    serverConfig.Value.ClientAuth = NoClientCert;
    // Tickets should be removed from the session cache on TLS handshake
    // failure, and the client should recover from a corrupted PSK
    testResumeState(fetchTicketToCorruptˢ, false);
    corruptTicket();
    (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err == default!) {
        Ꮡt.Fatalf("handshake did not fail with a corrupted client secret"u8);
    }
    testResumeState(afterHandshakeFailureˢ, false);
    clientConfig.Value.ClientSessionCache = default!;
    testResumeState(withoutSessionCacheˢ, false);
    clientConfig.Value.ClientSessionCache = new tls_internal_test_package.serializingClientCacheжClientSessionCache(Ꮡ(new serializingClientCache(t: Ꮡt)));
    testResumeState(beforeSerializingCacheˢ, false);
    testResumeState(withSerializingCacheˢ, true);
}

internal partial struct serializingClientCache {
    internal ж<testing.T> t;
    internal slice<byte> ticket, state;
}

internal static (ж<global::go.crypto.tls_package.ClientSessionState> session, bool ok) Get(this ref serializingClientCache c, @string sessionKey) {
    if (c.ticket == default!) {
        return (default!, false);
    }
    var (state, err) = ParseSessionState(c.state);
    if (err != default!) {
        c.t.Error(err);
        return (default!, false);
    }
    (var cs, err) = NewResumptionState(c.ticket, state);
    if (err != default!) {
        c.t.Error(err);
        return (default!, false);
    }
    return (cs, true);
}

internal static void Put(this ref serializingClientCache c, @string sessionKey, ж<global::go.crypto.tls_package.ClientSessionState> Ꮡcs) {
    if (Ꮡcs == nil) {
        (c.ticket, c.state) = (default!, default!);
        return;
    }
    var (ticket, state, err) = Ꮡcs.ResumptionState();
    if (err != default!) {
        c.t.Error(err);
        return;
    }
    (var stateBytes, err) = state.Bytes();
    if (err != default!) {
        c.t.Error(err);
        return;
    }
    (c.ticket, c.state) = (ticket, stateBytes);
}

public static void TestLRUClientSessionCache(ж<testing.T> Ꮡt) {
    // Initialize cache of capacity 4.
    var cache = NewLRUClientSessionCache(4);
    var cs = new slice<global::go.crypto.tls_package.ClientSessionState>(6);
    var keys = new @string[]{"0"u8, "1"u8, "2"u8, "3"u8, "4"u8, "5"u8, "6"u8}.slice();
    // Add 4 entries to the cache and look them up.
    for (nint i = 0; i < 4; i++) {
        cache.Put(keys[i], Ꮡ(cs, i));
    }
    for (nint i = 0; i < 4; i++) {
        {
            var (s, ok) = cache.Get(keys[i]); if (!ok || s != Ꮡ(cs, i)) {
                Ꮡt.Fatalf("session cache failed lookup for added key: %s"u8, keys[i]);
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
                Ꮡt.Fatalf("session cache should have evicted key: %s"u8, keys[i]);
            }
        }
    }
    // Touch entry 2. LRU should evict 3 next.
    cache.Get(keys[2]);
    cache.Put(keys[0], Ꮡ(cs, 0));
    {
        var (s, ok) = cache.Get(keys[3]); if (ok || s != nil) {
            Ꮡt.Fatalf("session cache should have evicted key 3"u8);
        }
    }
    // Update entry 0 in place.
    cache.Put(keys[0], Ꮡ(cs, 3));
    {
        var (s, ok) = cache.Get(keys[0]); if (!ok || s != Ꮡ(cs, 3)) {
            Ꮡt.Fatalf("session cache failed update for key 0"u8);
        }
    }
    // Calling Put with a nil entry deletes the key.
    cache.Put(keys[0], nil);
    {
        var (_, ok) = cache.Get(keys[0]); if (ok) {
            Ꮡt.Fatalf("session cache failed to delete key 0"u8);
        }
    }
    // Delete entry 2. LRU should keep 4 and 5
    cache.Put(keys[2], nil);
    {
        var (_, ok) = cache.Get(keys[2]); if (ok) {
            Ꮡt.Fatalf("session cache failed to delete key 4"u8);
        }
    }
    for (nint i = 4; i < 6; i++) {
        {
            var (s, ok) = cache.Get(keys[i]); if (!ok || s != Ꮡ(cs, i)) {
                Ꮡt.Fatalf("session cache should not have deleted key: %s"u8, keys[i]);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string clientˢ = "client"u8;
internal static readonly @string serverˢ = "server"u8;

public static partial void TestKeyLogTLS12(ж<testing.T> Ꮡt) {
    ref var serverBuf = ref heap(new bytes.Buffer(), out var ᏑserverBuf);
    ref var clientBuf = ref heap(new bytes.Buffer(), out var ᏑclientBuf);
    var clientConfig = testConfig.Clone();
    clientConfig.Value.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑclientBuf);
    clientConfig.Value.MaxVersion = VersionTLS12;
    var serverConfig = testConfig.Clone();
    serverConfig.Value.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑserverBuf);
    serverConfig.Value.MaxVersion = VersionTLS12;
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    var sʗ1 = s;
    var serverConfigʗ1 = serverConfig;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);
            {
                var err = Server(sʗ1, serverConfigʗ1).Handshake(); if (err != default!) {
                    Ꮡt.Errorf("server: %s"u8, err);
                    return;
                }
            }
            sʗ1.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    {
        var err = Client(c, clientConfig).Handshake(); if (err != default!) {
            Ꮡt.Fatalf("client: %s"u8, err);
        }
    }
    c.Close();
    ᐸꟷ(done);
    void checkKeylogLine(@string side, @string loggedLine) {
        if (len(loggedLine) == 0) {
            Ꮡt.Fatalf("%s: no keylog line was produced"u8, side);
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
            Ꮡt.Fatalf("%s: keylog line has incorrect length (want %d, got %d): %q"u8, side, (nint)(expectedLen), len(loggedLine), loggedLine);
        }
        if (!strings.HasPrefix(loggedLine, "CLIENT_RANDOM "u8 + strings.Repeat("0"u8, 64) + " "u8)) {
            Ꮡt.Fatalf("%s: keylog line has incorrect structure or nonce: %q"u8, side, loggedLine);
        }
    }
    checkKeylogLine(clientˢ, ᏑclientBuf.String());
    checkKeylogLine(serverˢ, ᏑserverBuf.String());
}

public static partial void TestKeyLogTLS13(ж<testing.T> Ꮡt) {
    ref var serverBuf = ref heap(new bytes.Buffer(), out var ᏑserverBuf);
    ref var clientBuf = ref heap(new bytes.Buffer(), out var ᏑclientBuf);
    var clientConfig = testConfig.Clone();
    clientConfig.Value.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑclientBuf);
    var serverConfig = testConfig.Clone();
    serverConfig.Value.KeyLogWriter = new tls_test_package.bytes_BufferжWriter(ᏑserverBuf);
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    var sʗ1 = s;
    var serverConfigʗ1 = serverConfig;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);
            {
                var err = Server(sʗ1, serverConfigʗ1).Handshake(); if (err != default!) {
                    Ꮡt.Errorf("server: %s"u8, err);
                    return;
                }
            }
            sʗ1.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    {
        var err = Client(c, clientConfig).Handshake(); if (err != default!) {
            Ꮡt.Fatalf("client: %s"u8, err);
        }
    }
    c.Close();
    ᐸꟷ(done);
    void checkKeylogLines(@string side, @string loggedLines) {
        loggedLines = strings.TrimSpace(loggedLines);
        var lines = strings.Split(loggedLines, "\n"u8);
        if (len(lines) != 4) {
            Ꮡt.Errorf("Expected the %s to log 4 lines, got %d"u8, side, len(lines));
        }
    }
    checkKeylogLines(clientˢ, ᏑclientBuf.String());
    checkKeylogLines(serverˢ, ᏑserverBuf.String());
}

public static void TestHandshakeClientALPNMatch(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.NextProtos = new @string[]{"proto2"u8, "proto1"u8}.slice();
    var test = Ꮡ(new clientTest(
        name: "ALPN"u8, // Note that this needs OpenSSL 1.0.2 because that is the first
 // version that supports the -alpn flag.

        args: new @string[]{"-alpn"u8, "proto1,proto2"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            // The server's preferences should override the client.
            if (state.NegotiatedProtocol != "proto1"u8) {
                return fmt.Errorf("Got protocol %q, wanted proto1"u8, state.NegotiatedProtocol);
            }
            return default!;
        }
    ));
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string serverSelectedˢ = "server selected unadvertised ALPN protocol"u8;

public static partial void TestServerSelectingUnconfiguredApplicationProtocol(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This checks that the server can't select an application protocol that the
    // client didn't offer.
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var errChan = new channel<error>(1);
    var cʗ1 = c;
    var errChanʗ1 = errChan;
    goǃ(() => {
        var client = Client(cʗ1, Ꮡ(new Config(
            ServerName: "foo"u8,
            CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
            NextProtos: new @string[]{"http"u8, "something-else"u8}.slice()
        )));
        errChanʗ1.ᐸꟷ(client.Handshake());
    });
    array<byte> header = new(5);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), header[..]); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    nint recordLen = (nint)(((nint)header[3] << (int)(8)) | (nint)header[4]);
    var record = new slice<byte>(recordLen);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), record); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuite: TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
        alpnProtocol: "how-about-this"u8
    ));
    var serverHelloBytes = mustMarshal(Ꮡt, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));
    s.Write(new byte[]{
        (byte)recordTypeHandshake,
        (byte)(((byte)VersionTLS12).Rsh((uint64)(8))),
        (byte)((byte)((byte)VersionTLS12 & 0xff)),
        (byte)((len(serverHelloBytes) >> (int)(8))),
        (byte)len(serverHelloBytes)
    }.slice());
    s.Write(serverHelloBytes);
    s.Close();
    {
        var err = ᐸꟷ(errChan); if (!strings.Contains(err.Error(), serverSelectedˢ)) {
            Ꮡt.Fatalf("Expected error about unconfigured ALPN protocol but got %q"u8, err);
        }
    }
}

// sctsBase64 contains data from `openssl s_client -serverinfo 18 -connect ritter.vg:443`
internal static readonly @string sctsBase64 = "ABIBaQFnAHUApLkJkLQYWBSHuxOizGdwCjw1mAT5G9+443fNDsgN3BAAAAFHl5nuFgAABAMARjBEAiAcS4JdlW5nW9sElUv2zvQyPoZ6ejKrGGB03gjaBZFMLwIgc1Qbbn+hsH0RvObzhS+XZhr3iuQQJY8S9G85D9KeGPAAdgBo9pj4H2SCvjqM7rkoHUz8cVFdZ5PURNEKZ6y7T0/7xAAAAUeX4bVwAAAEAwBHMEUCIDIhFDgG2HIuADBkGuLobU5a4dlCHoJLliWJ1SYT05z6AiEAjxIoZFFPRNWMGGIjskOTMwXzQ1Wh2e7NxXE1kd1J0QsAdgDuS723dc5guuFCaR+r4Z5mow9+X7By2IMAxHuJeqj9ywAAAUhcZIqHAAAEAwBHMEUCICmJ1rBT09LpkbzxtUC+Hi7nXLR0J+2PmwLp+sJMuqK+AiEAr0NkUnEVKVhAkccIFpYDqHOlZaBsuEhWWrYpg2RtKp0="u8;

public static void TestHandshakClientSCTs(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    var (scts, err) = base64.StdEncoding.DecodeString(sctsBase64);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Note that this needs OpenSSL 1.0.2 because that is the first
    // version that supports the -serverinfo flag.
        var sctsʗ1 = scts;
    var test = Ꮡ(new clientTest(
        name: "SCT"u8,
        config: config,
        extensions: new slice<byte>[]{scts}.slice(),
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            var expectedSCTs = new slice<byte>[]{
                sctsʗ1[8..125],
                sctsʗ1[127..245],
                sctsʗ1[247..]
            }.slice();
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
            return default!;
        }
    ));
    runClientTestTLS12(Ꮡt, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedErrorFromˢ = "expected error from renegotiation but got nil"u8;
internal static readonly @string noRenegotiationˢ = "no renegotiation"u8;

// TLS 1.3 moved SCTs to the Certificate extensions and -serverinfo only
// supports ServerHello extensions.
public static void TestRenegotiationRejected(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    var test = Ꮡ(new clientTest(
        name: "RenegotiationRejected"u8,
        args: new @string[]{"-state"u8}.slice(),
        config: config,
        numRenegotiations: 1,
        renegotiationExpectedToFail: 1,
        checkRenegotiationError: error (nint renegotiationNum, error err) => {
            if (err == default!) {
                return errors.New(expectedErrorFromˢ);
            }
            if (!strings.Contains(err.Error(), noRenegotiationˢ)) {
                return fmt.Errorf("expected renegotiation to be rejected but got %q"u8, err);
            }
            return default!;
        }
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestRenegotiateOnce(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.Renegotiation = RenegotiateOnceAsClient;
    var test = Ꮡ(new clientTest(
        name: "RenegotiateOnce"u8,
        args: new @string[]{"-state"u8}.slice(),
        config: config,
        numRenegotiations: 1
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestRenegotiateTwice(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.Renegotiation = RenegotiateFreelyAsClient;
    var test = Ꮡ(new clientTest(
        name: "RenegotiateTwice"u8,
        args: new @string[]{"-state"u8}.slice(),
        config: config,
        numRenegotiations: 2
    ));
    runClientTestTLS12(Ꮡt, test);
}

public static void TestRenegotiateTwiceRejected(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.Renegotiation = RenegotiateOnceAsClient;
    var test = Ꮡ(new clientTest(
        name: "RenegotiateTwiceRejected"u8,
        args: new @string[]{"-state"u8}.slice(),
        config: config,
        numRenegotiations: 2,
        renegotiationExpectedToFail: 2,
        checkRenegotiationError: error (nint renegotiationNum, error err) => {
            if (renegotiationNum == 1) {
                return err;
            }
            if (err == default!) {
                return errors.New(expectedErrorFromˢ);
            }
            if (!strings.Contains(err.Error(), noRenegotiationˢ)) {
                return fmt.Errorf("expected renegotiation to be rejected but got %q"u8, err);
            }
            return default!;
        }
    ));
    runClientTestTLS12(Ꮡt, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;

public static void TestHandshakeClientExportKeyingMaterial(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new clientTest(
        name: "ExportKeyingMaterial"u8,
        config: testConfig.Clone(),
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            {
                var (km, err) = state.ExportKeyingMaterial(testˢ, default!, 42); if (err != default!){
                    return fmt.Errorf("ExportKeyingMaterial failed: %v"u8, err);
                } else 
                if (len(km) != 42) {
                    return fmt.Errorf("Got %d bytes from ExportKeyingMaterial, wanted %d"u8, len(km), (nint)(42));
                }
            }
            return default!;
        }
    ));
    runClientTestTLS10(Ꮡt, test);
    runClientTestTLS12(Ꮡt, test);
    runClientTestTLS13(Ꮡt, test);
}

// Opaque string
// DNS hostname
// Literal IPv4 address
// Literal IPv6 address
// with zone identifier
// as per RFC 5952 we allow the [] style as IPv6 literal

partial struct hostnameInSNITestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<hostnameInSNITestsᴛ1> hostnameInSNITests = new hostnameInSNITestsᴛ1[]{
    new(""u8, ""u8),
    new("localhost"u8, "localhost"u8),
    new("foo, bar, baz and qux"u8, "foo, bar, baz and qux"u8),
    new("golang.org"u8, "golang.org"u8),
    new("golang.org."u8, "golang.org"u8),
    new("1.2.3.4"u8, ""u8),
    new("::1"u8, ""u8),
    new("::1%lo0"u8, ""u8),
    new("[::1]"u8, ""u8),
    new("[::1%lo0]"u8, ""u8)
}.slice();

public static partial void TestHostnameInSNI(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in hostnameInSNITests) {
        ref var tt = ref heap(new hostnameInSNITestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var cʗ1 = c;
        goǃ((@string host) => {
            Client(cʗ1, Ꮡ(new Config(ServerName: host, InsecureSkipVerify: true))).Handshake();
        }, tt.@in);
        array<byte> header = new(5);
        {
            var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), header[..]); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        nint recordLen = (nint)(((nint)header[3] << (int)(8)) | (nint)header[4]);
        var record = new slice<byte>(recordLen);
        {
            var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), record[..]); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        c.Close();
        s.Close();
        ref var m = ref heap(new global::go.crypto.tls_package.clientHelloMsg(), out var Ꮡm);
        if (!Ꮡm.unmarshal(record)) {
            Ꮡt.Errorf("unmarshaling ClientHello for %q failed"u8, tt.@in);
            continue;
        }
        if (tt.@in != tt.@out && m.serverName == tt.@in) {
            Ꮡt.Errorf("prohibited %q found in ClientHello: %x"u8, tt.@in, record);
        }
        if (m.serverName != tt.@out) {
            Ꮡt.Errorf("expected %q not found in ClientHello: %x"u8, tt.@out, record);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unconfiguredCipherˢ = "unconfigured cipher"u8;

public static partial void TestServerSelectingUnconfiguredCipherSuite(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This checks that the server can't select a cipher suite that the
    // client didn't offer. See #13174.
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var errChan = new channel<error>(1);
    var cʗ1 = c;
    var errChanʗ1 = errChan;
    goǃ(() => {
        var client = Client(cʗ1, Ꮡ(new Config(
            ServerName: "foo"u8,
            CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_GCM_SHA256}.slice()
        )));
        errChanʗ1.ᐸꟷ(client.Handshake());
    });
    array<byte> header = new(5);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), header[..]); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    nint recordLen = (nint)(((nint)header[3] << (int)(8)) | (nint)header[4]);
    var record = new slice<byte>(recordLen);
    {
        var (_, err) = io.ReadFull(new tls_test_package.net_ConnᴠReader(s), record); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    // Create a ServerHello that selects a different cipher suite than the
    // sole one that the client offered.
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuite: TLS_RSA_WITH_AES_256_GCM_SHA384
    ));
    var serverHelloBytes = mustMarshal(Ꮡt, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));
    s.Write(new byte[]{
        (byte)recordTypeHandshake,
        (byte)(((byte)VersionTLS12).Rsh((uint64)(8))),
        (byte)((byte)((byte)VersionTLS12 & 0xff)),
        (byte)((len(serverHelloBytes) >> (int)(8))),
        (byte)len(serverHelloBytes)
    }.slice());
    s.Write(serverHelloBytes);
    s.Close();
    {
        var err = ᐸꟷ(errChan); if (!strings.Contains(err.Error(), unconfiguredCipherˢ)) {
            Ꮡt.Fatalf("Expected error about unconfigured cipher suite but got %q"u8, err);
        }
    }
}

public static void TestVerifyConnection(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testVerifyConnection(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testVerifyConnection(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleGolangˢ = "example.golang"u8;
internal static readonly object protocol1ˢ = (@string)"protocol1"u8;

internal partial struct testVerifyConnection_tests /*dyn*/ {
    internal @string name;
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<nint>> configureServer;
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<nint>> configureClient;
}

internal static void testVerifyConnection(ж<testing.T> Ꮡt, uint16 version) {
    ref var t = ref Ꮡt.DerefOrNull();

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
        if (called.Value == 2) {
            // if this is the second time, then it should be a resumption
            wantDidResume = true;
        }
        if (c.DidResume != wantDidResume) {
            return fmt.Errorf("%s: got DidResume %t, want %t"u8, errorType, c.DidResume, wantDidResume);
        }
        return default!;
    }
            var checkFieldsʗ1 = checkFields;

            var checkFieldsʗ3 = checkFields;

            var checkFieldsʗ5 = checkFields;

            var checkFieldsʗ7 = checkFields;

            var checkFieldsʗ9 = checkFields;

            var checkFieldsʗ11 = checkFields;

            var checkFieldsʗ13 = checkFields;

            var checkFieldsʗ15 = checkFields;
    var tests = new testVerifyConnection_tests[]{
        new(
            name: "RequireAndVerifyClientCert"u8,
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Value.ClientAuth = RequireAndVerifyClientCert;
                var checkFieldsʗ2 = checkFieldsʗ1;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
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
                config.Value.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState c) => {
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
                        return default!;
                    }
                    // The SCTs and OCSP Response are dropped on resumption.
                    // See http://golang.org/issue/39075.
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
            name: "InsecureSkipVerify"u8,
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Value.ClientAuth = RequireAnyClientCert;
                config.Value.InsecureSkipVerify = true;
                var checkFieldsʗ6 = checkFieldsʗ5;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("server: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (c.VerifiedChains != default!) {
                        return fmt.Errorf("server: got Verified Chains %v, want nil"u8, c.VerifiedChains);
                    }
                    return checkFieldsʗ6(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Value.InsecureSkipVerify = true;
                var checkFieldsʗ8 = checkFieldsʗ7;
                config.Value.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    {
                        nint l = len(c.PeerCertificates); if (l != 1) {
                            return fmt.Errorf("client: got len(PeerCertificates) = %d, wanted 1"u8, l);
                        }
                    }
                    if (c.VerifiedChains != default!) {
                        return fmt.Errorf("server: got Verified Chains %v, want nil"u8, c.VerifiedChains);
                    }
                    if (c.DidResume) {
                        return default!;
                    }
                    // The SCTs and OCSP Response are dropped on resumption.
                    // See http://golang.org/issue/39075.
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
            name: "NoClientCert"u8,
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Value.ClientAuth = NoClientCert;
                var checkFieldsʗ10 = checkFieldsʗ9;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    return checkFieldsʗ10(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                var checkFieldsʗ12 = checkFieldsʗ11;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    return checkFieldsʗ12(c, called, clientˢ);
                };
            }
        ),
        new(
            name: "RequestClientCert"u8,
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Value.ClientAuth = RequestClientCert;
                var checkFieldsʗ14 = checkFieldsʗ13;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => {
                    called.Value++;
                    return checkFieldsʗ14(c, called, serverˢ);
                };
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<nint> called) => {
                config.Value.Certificates = default!; // clear the client cert
                var checkFieldsʗ16 = checkFieldsʗ15;
                config.Value.VerifyConnection = error (global::go.crypto.tls_package.ΔConnectionState c) => {
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
                        return default!;
                    }
                    // The SCTs and OCSP Response are dropped on resumption.
                    // See http://golang.org/issue/39075.
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
    }.slice();
    foreach (var (_, test) in tests) {
        // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
        var testCertificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
        var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
        if (err != default!) {
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
            NextProtos: new @string[]{"protocol1"u8}.slice()
        ));
        (~serverConfig).Certificates[0].SignedCertificateTimestamps = new slice<byte>[]{slice<byte>("dummy sct 1"u8), slice<byte>("dummy sct 2"u8)}.slice();
        (~serverConfig).Certificates[0].OCSPStaple = slice<byte>("dummy ocsp"u8);
        test.configureServer(serverConfig, ᏑserverCalled);
        var clientConfig = Ꮡ(new Config(
            MaxVersion: version,
            ClientSessionCache: NewLRUClientSessionCache(32),
            RootCAs: rootCAs,
            ServerName: "example.golang"u8,
            Certificates: testCertificates,
            Time: testTime,
            NextProtos: new @string[]{"protocol1"u8}.slice()
        ));
        test.configureClient(clientConfig, ᏑclientCalled);
        var clientConfigʗ1 = clientConfig;
        var serverConfigʗ1 = serverConfig;
        void testHandshakeState(@string name, bool didResume) {
            var (_, hs, errΔ1) = testHandshake(Ꮡt, clientConfigʗ1, serverConfigʗ1);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("%s: handshake failed: %s"u8, name, errΔ1);
            }
            if (hs.DidResume != didResume) {
                Ꮡt.Errorf("%s: resumed: %v, expected: %v"u8, name, hs.DidResume, didResume);
            }
            nint wantCalled = 1;
            if (didResume) {
                wantCalled = 2; // resumption would mean this is the second time it was called in this test
            }
            if (ᏑclientCalled.Value != wantCalled) {
                Ꮡt.Errorf("%s: expected client VerifyConnection called %d times, did %d times"u8, name, wantCalled, ᏑclientCalled.Value);
            }
            if (ᏑserverCalled.Value != wantCalled) {
                Ꮡt.Errorf("%s: expected server VerifyConnection called %d times, did %d times"u8, name, wantCalled, ᏑserverCalled.Value);
            }
        }
        testHandshakeState(fmt.Sprintf("%s-FullHandshake"u8, test.name), false);
        testHandshakeState(fmt.Sprintf("%s-Resumption"u8, test.name), true);
    }
}

public static void TestVerifyPeerCertificate(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testVerifyPeerCertificate(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testVerifyPeerCertificate(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gotLenValidatedChains0ˢ = "got len(validatedChains) = 0, wanted non-zero"u8;

internal partial struct testVerifyPeerCertificate_tests /*dyn*/ {
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<bool>> configureServer;
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<bool>> configureClient;
    internal Action<ж<testing.T>, nint, bool, bool, error, error> validate;
}

internal static partial void testVerifyPeerCertificate(ж<testing.T> Ꮡt, uint16 version) {
    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    ref var err = ref heap<error>(out var Ꮡerr);
    (var issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != default!) {
        throw panic(err);
    }
    var rootCAs = Δx509.NewCertPool();
    rootCAs.AddCert(issuer);
    var sentinelErr = errors.New("TestVerifyPeerCertificate"u8);
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
        return default!;
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
        return default!;
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
    var tests = new testVerifyPeerCertificate_tests[]{
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ2 = verifyPeerCertificateCallbackʗ1;
                config.Value.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ2(called, rawCerts, validatedChains);
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ4 = verifyPeerCertificateCallbackʗ3;
                config.Value.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ4(called, rawCerts, validatedChains);
            },
            validate: (ж<testing.T> tΔ1, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (clientErr != default!) {
                    tΔ1.Errorf("test[%d]: client handshake failed: %v"u8, testNo, clientErr);
                }
                if (serverErr != default!) {
                    tΔ1.Errorf("test[%d]: server handshake failed: %v"u8, testNo, serverErr);
                }
                if (!clientCalled) {
                    tΔ1.Errorf("test[%d]: client did not call callback"u8, testNo);
                }
                if (!serverCalled) {
                    tΔ1.Errorf("test[%d]: server did not call callback"u8, testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var sentinelErrʗ2 = sentinelErrʗ1;
                config.Value.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => sentinelErrʗ2;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.VerifyPeerCertificate = default!;
            },
            validate: (ж<testing.T> tΔ2, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(serverErr, sentinelErrʗ3)) {
                    tΔ2.Errorf("#%d: got server error %v, wanted sentinelErr"u8, testNo, serverErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                var sentinelErrʗ5 = sentinelErrʗ4;
                config.Value.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => sentinelErrʗ5;
            },
            validate: (ж<testing.T> tΔ3, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(clientErr, sentinelErrʗ6)) {
                    tΔ3.Errorf("#%d: got client error %v, wanted sentinelErr"u8, testNo, clientErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = true;
                config.Value.VerifyPeerCertificate = error (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => {
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
                    return default!;
                };
            },
            validate: (ж<testing.T> tΔ4, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (clientErr != default!) {
                    tΔ4.Errorf("test[%d]: client handshake failed: %v"u8, testNo, clientErr);
                }
                if (serverErr != default!) {
                    tΔ4.Errorf("test[%d]: server handshake failed: %v"u8, testNo, serverErr);
                }
                if (!clientCalled) {
                    tΔ4.Errorf("test[%d]: client did not call callback"u8, testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var verifyConnectionCallbackʗ2 = verifyConnectionCallbackʗ1;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => verifyConnectionCallbackʗ2(called, false, c);
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var verifyConnectionCallbackʗ4 = verifyConnectionCallbackʗ3;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => verifyConnectionCallbackʗ4(called, true, c);
            },
            validate: (ж<testing.T> tΔ5, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (clientErr != default!) {
                    tΔ5.Errorf("test[%d]: client handshake failed: %v"u8, testNo, clientErr);
                }
                if (serverErr != default!) {
                    tΔ5.Errorf("test[%d]: server handshake failed: %v"u8, testNo, serverErr);
                }
                if (!clientCalled) {
                    tΔ5.Errorf("test[%d]: client did not call callback"u8, testNo);
                }
                if (!serverCalled) {
                    tΔ5.Errorf("test[%d]: server did not call callback"u8, testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var sentinelErrʗ8 = sentinelErrʗ7;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ8;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                config.Value.VerifyConnection = default!;
            },
            validate: (ж<testing.T> tΔ6, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(serverErr, sentinelErrʗ9)) {
                    tΔ6.Errorf("#%d: got server error %v, wanted sentinelErr"u8, testNo, serverErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                config.Value.VerifyConnection = default!;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var sentinelErrʗ11 = sentinelErrʗ10;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ11;
            },
            validate: (ж<testing.T> tΔ7, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(clientErr, sentinelErrʗ12)) {
                    tΔ7.Errorf("#%d: got client error %v, wanted sentinelErr"u8, testNo, clientErr);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ6 = verifyPeerCertificateCallbackʗ5;
                config.Value.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ6(called, rawCerts, validatedChains);
                var sentinelErrʗ14 = sentinelErrʗ13;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ14;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                config.Value.VerifyPeerCertificate = default!;
                config.Value.VerifyConnection = default!;
            },
            validate: (ж<testing.T> tΔ8, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(serverErr, sentinelErrʗ15)) {
                    tΔ8.Errorf("#%d: got server error %v, wanted sentinelErr"u8, testNo, serverErr);
                }
                if (!serverCalled) {
                    tΔ8.Errorf("test[%d]: server did not call callback"u8, testNo);
                }
            }
        ),
        new(
            configureServer: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                config.Value.VerifyPeerCertificate = default!;
                config.Value.VerifyConnection = default!;
            },
            configureClient: (ж<global::go.crypto.tls_package.Config> config, ж<bool> called) => {
                config.Value.InsecureSkipVerify = false;
                var verifyPeerCertificateCallbackʗ8 = verifyPeerCertificateCallbackʗ7;
                config.Value.VerifyPeerCertificate = (slice<slice<byte>> rawCerts, slice<slice<ж<Δx509.Certificate>>> validatedChains) => verifyPeerCertificateCallbackʗ8(called, rawCerts, validatedChains);
                var sentinelErrʗ17 = sentinelErrʗ16;
                config.Value.VerifyConnection = (global::go.crypto.tls_package.ΔConnectionState c) => sentinelErrʗ17;
            },
            validate: (ж<testing.T> tΔ9, nint testNo, bool clientCalled, bool serverCalled, error clientErr, error serverErr) => {
                if (!AreEqual(clientErr, sentinelErrʗ18)) {
                    tΔ9.Errorf("#%d: got client error %v, wanted sentinelErr"u8, testNo, clientErr);
                }
                if (!clientCalled) {
                    tΔ9.Errorf("test[%d]: client did not call callback"u8, testNo);
                }
            }
        )
    }.slice();
    foreach (var (i, vᴛ1) in tests) {
        ref var test = ref heap(new testVerifyPeerCertificate_tests(), out var Ꮡtest);
        test = vᴛ1;

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var done = new channel<error>(0);
        ref var clientCalled = ref heap(new bool(), out var ᏑclientCalled);
        ref var serverCalled = ref heap(new bool(), out var ᏑserverCalled);
        var doneʗ1 = done;
        var rootCAsʗ1 = rootCAs;
        var sʗ1 = s;
        var testʗ1 = test;
        goǃ(() => {
            var configΔ1 = testConfig.Clone();
            configΔ1.Value.ServerName = exampleGolangˢ;
            configΔ1.Value.ClientAuth = RequireAndVerifyClientCert;
            configΔ1.Value.ClientCAs = rootCAsʗ1;
            configΔ1.Value.Time = testTime;
            configΔ1.Value.MaxVersion = version;
            configΔ1.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
            (~configΔ1).Certificates[0].ΔCertificate = new slice<byte>[]{testRSA2048Certificate}.slice();
            (~configΔ1).Certificates[0].PrivateKey = testRSA2048PrivateKey.OrTypedNil();
            (~configΔ1).Certificates[0].SignedCertificateTimestamps = new slice<byte>[]{slice<byte>("dummy sct 1"u8), slice<byte>("dummy sct 2"u8)}.slice();
            (~configΔ1).Certificates[0].OCSPStaple = slice<byte>("dummy ocsp"u8);
            testʗ1.configureServer(configΔ1, ᏑserverCalled);
            Ꮡerr.ValueSlot = Server(sʗ1, configΔ1).Handshake();
            sʗ1.Close();
            doneʗ1.ᐸꟷ(Ꮡerr.ValueSlot);
        });
        var config = testConfig.Clone();
        config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
        config.Value.ServerName = exampleGolangˢ;
        config.Value.RootCAs = rootCAs;
        config.Value.Time = testTime;
        config.Value.MaxVersion = version;
        test.configureClient(config, ᏑclientCalled);
        var clientErr = Client(c, config).Handshake();
        c.Close();
        var serverErr = ᐸꟷ(done);
        test.validate(Ꮡt, i, clientCalled, serverCalled, clientErr, serverErr);
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
internal static error brokenConnErr = errors.New("too many writes to brokenConn"u8);

internal static (nint, error) Write(this ref brokenConn b, slice<byte> data) {
    if (b.numWrites >= b.breakAfter) {
        return (0, brokenConnErr);
    }
    b.numWrites++;
    return b.Conn.Write(data);
}

public static partial void TestFailedWrite(ж<testing.T> Ꮡt) {
    // Test that a write error during the handshake is returned.
    foreach (var (_, vᴛ1) in new nint[]{0, 1}.slice()) {
        ref var breakAfter = ref heap(new nint(), out var ᏑbreakAfter);
        breakAfter = vᴛ1;

        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
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
            Ꮡt.Errorf("#%d: expected error from brokenConn but got %q"u8, breakAfter, err);
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

public static void TestBuffering(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testBuffering(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testBuffering(tΔ2, VersionTLS13);
    });
}

internal static partial void testBuffering(ж<testing.T> Ꮡt, uint16 version) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var done = new channel<bool>(0);
    var clientWCC = Ꮡ(new writeCountingConn(Conn: c));
    var serverWCC = Ꮡ(new writeCountingConn(Conn: s));
    var doneʗ1 = done;
    var serverWCCʗ1 = serverWCC;
    goǃ(() => {
        var config = testConfig.Clone();
        config.Value.MaxVersion = version;
        Server(new tls_internal_test_package.writeCountingConnжConn(serverWCCʗ1), config).Handshake();
        (~serverWCCʗ1).Conn.Close();
        doneʗ1.ᐸꟷ(true);
    });
    var err = Client(new tls_internal_test_package.writeCountingConnжConn(clientWCC), testConfig).Handshake();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~clientWCC).Conn.Close();
    ᐸꟷ(done);
    nint expectedClient = default!;
    nint expectedServer = default!;
    if (version == VersionTLS13){
        expectedClient = 2;
        expectedServer = 1;
    } else {
        expectedClient = 2;
        expectedServer = 2;
    }
    {
        nint n = clientWCC.Value.numWrites; if (n != expectedClient) {
            Ꮡt.Errorf("expected client handshake to complete with %d writes, but saw %d"u8, expectedClient, n);
        }
    }
    {
        nint n = serverWCC.Value.numWrites; if (n != expectedServer) {
            Ꮡt.Errorf("expected server handshake to complete with %d writes, but saw %d"u8, expectedServer, n);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientUnexpectedlyˢ = (@string)"client unexpectedly returned no error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedErrorᶜ = "remote error: tls: internal error"u8;

public static partial void TestAlertFlushing(ж<testing.T> Ꮡt) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var done = new channel<bool>(0);
    var clientWCC = Ꮡ(new writeCountingConn(Conn: c));
    var serverWCC = Ꮡ(new writeCountingConn(Conn: s));
    var serverConfig = testConfig.Clone();
    // Cause a signature-time error
    ref var brokenKey = ref heap<rsa.PrivateKey>(out var ᏑbrokenKey);
    brokenKey = new rsa.PrivateKey(PublicKey: (~testRSAPrivateKey).PublicKey);
    brokenKey.D = big.NewInt(42);
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: ᏑbrokenKey
    )
    }.slice();
    var doneʗ1 = done;
    var serverConfigʗ1 = serverConfig;
    var serverWCCʗ1 = serverWCC;
    goǃ(() => {
        Server(new tls_internal_test_package.writeCountingConnжConn(serverWCCʗ1), serverConfigʗ1).Handshake();
        (~serverWCCʗ1).Conn.Close();
        doneʗ1.ᐸꟷ(true);
    });
    var err = Client(new tls_internal_test_package.writeCountingConnжConn(clientWCC), testConfig).Handshake();
    if (err == default!) {
        Ꮡt.Fatal(clientUnexpectedlyˢ);
    }
    @string expectedError = expectedErrorᶜ;
    {
        @string e = err.Error(); if (!strings.Contains(e, expectedError)) {
            Ꮡt.Fatalf("expected to find %q in error but error was %q"u8, expectedError, e);
        }
    }
    (~clientWCC).Conn.Close();
    ᐸꟷ(done);
    {
        nint n = serverWCC.Value.numWrites; if (n != 1) {
            Ꮡt.Errorf("expected server handshake to complete with one write, but saw %d"u8, n);
        }
    }
}

public static partial void TestHandshakeRace(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ2);
    }
    Ꮡt.Parallel();
    // This test races a Read and Write to try and complete a handshake in
    // order to provide some evidence that there are no races or deadlocks
    // in the handshake locking.
    for (nint i = 0; i < 32; i++) {
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var sʗ1 = s;
        goǃ(() => {
            var server = Server(sʗ1, testConfig);
            {
                var err = server.Handshake(); if (err != default!) {
                    throw panic(err);
                }
            }
            ref var request = ref heap(new array<byte>(1), out var Ꮡrequest);
            {
                var (n, err) = server.Read(request[..]); if (err != default! || n != 1) {
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
                var (_, err) = io.ReadFull(new tls_test_package.tls_ConnжReader(clientʗ2), reply[..]); if (err != default!) {
                    throw panic(err);
                }
            }
            cʗ1.Close();
            readDoneʗ1.ᐸꟷ(new EmptyStruct());
        });
        if ((nint)(i & 1) == 1){
            startWrite.ᐸꟷ(new EmptyStruct());
            startRead.ᐸꟷ(new EmptyStruct());
        } else {
            startRead.ᐸꟷ(new EmptyStruct());
            startWrite.ᐸꟷ(new EmptyStruct());
        }
        ᐸꟷ(readDone);
    }
}

// Returning a Certificate with no certificate data
// should result in an empty message being sent to the
// server.
// With TLS 1.1, the SignatureSchemes should be
// synthesised from the supported certificate types.
// Returning an error should abort the handshake with
// that error.

partial struct getClientCertificateTestsᴛ1 /*dyn*/ {
    internal Action<ж<global::go.crypto.tls_package.Config>, ж<global::go.crypto.tls_package.Config>> setup;
    internal @string expectedClientError;
    internal Action<ж<testing.T>, nint, ж<global::go.crypto.tls_package.ΔConnectionState>> verify;
}
internal static slice<getClientCertificateTestsᴛ1> getClientCertificateTests;
internal static void initᴛgetClientCertificateTests() { getClientCertificateTests = new getClientCertificateTestsᴛ1[]{
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            serverConfig.Value.ClientCAs = default!;
            clientConfig.Value.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (len((~cri).SignatureSchemes) == 0) {
                    throw panic("empty SignatureSchemes");
                }
                if (len((~cri).AcceptableCAs) != 0) {
                    throw panic("AcceptableCAs should have been empty");
                }
                return (@new<global::go.crypto.tls_package.Certificate>(), default!);
            };
        },
        ""u8,
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
            {
                nint l = len((~cs).PeerCertificates); if (l != 0) {
                    t.Errorf("#%d: expected no certificates but got %d"u8, testNum, l);
                }
            }
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            clientConfig.Value.MaxVersion = VersionTLS11;
            clientConfig.Value.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (len((~cri).SignatureSchemes) == 0) {
                    throw panic("empty SignatureSchemes");
                }
                return (@new<global::go.crypto.tls_package.Certificate>(), default!);
            };
        },
        ""u8,
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
            {
                nint l = len((~cs).PeerCertificates); if (l != 0) {
                    t.Errorf("#%d: expected no certificates but got %d"u8, testNum, l);
                }
            }
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            clientConfig.Value.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => (default!, errors.New("GetClientCertificate"u8));
        },
        "GetClientCertificate"u8,
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) => {
            clientConfig.Value.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> cri) => {
                if (len((~cri).AcceptableCAs) == 0) {
                    throw panic("empty AcceptableCAs");
                }
                var cert = Ꮡ(new Certificate(
                    ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(),
                    PrivateKey: testRSA2048PrivateKey.OrTypedNil()
                ));
                return (cert, default!);
            };
        },
        ""u8,
        (ж<testing.T> t, nint testNum, ж<global::go.crypto.tls_package.ΔConnectionState> cs) => {
            if (len((~cs).VerifiedChains) == 0) {
                t.Errorf("#%d: expected some verified chains, but found none"u8, testNum);
            }
        }
    )
}.slice(); }

public static void TestGetClientCertificate(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        testGetClientCertificate(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        testGetClientCertificate(tΔ2, VersionTLS13);
    });
}

internal partial struct testGetClientCertificate_serverResult /*dyn*/ {
    internal global::go.crypto.tls_package.ΔConnectionState cs;
    internal error err;
}

internal static partial void testGetClientCertificate(ж<testing.T> Ꮡt, uint16 version) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != default!) {
        throw panic(err);
    }
    foreach (var (i, test) in getClientCertificateTests) {
        var serverConfig = testConfig.Clone();
        serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
        serverConfig.Value.ClientAuth = VerifyClientCertIfGiven;
        serverConfig.Value.RootCAs = Δx509.NewCertPool();
        (~serverConfig).RootCAs.AddCert(issuer);
        serverConfig.Value.ClientCAs = serverConfig.Value.RootCAs;
        serverConfig.Value.Time = testTime;
        serverConfig.Value.MaxVersion = version;
        var clientConfig = testConfig.Clone();
        clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
        clientConfig.Value.MaxVersion = version;
        test.setup(clientConfig, serverConfig);
        // TLS 1.1 isn't available for FIPS required
        if (fips140tls.Required() && (~clientConfig).MaxVersion == VersionTLS11) {
            Ꮡt.Logf("skipping test %d for FIPS mode"u8, i);
            continue;
        }
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
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
                if (errΔ1 == default!) {
                    cs = server.ConnectionState();
                }
                doneʗ1.ᐸꟷ(new testGetClientCertificate_serverResult(cs, errΔ1));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var clientErr = Client(c, clientConfig).Handshake();
        c.Close();
        ref var result = ref heap<testGetClientCertificate_serverResult>(out var Ꮡresult);
        result = ᐸꟷ(done);
        if (clientErr != default!){
            if (len(test.expectedClientError) == 0){
                Ꮡt.Errorf("#%d: client error: %v"u8, i, clientErr);
            } else 
            {
                @string got = clientErr.Error(); if (got != test.expectedClientError){
                    Ꮡt.Errorf("#%d: expected client error %q, but got %q"u8, i, test.expectedClientError, got);
                } else {
                    test.verify(Ꮡt, i, Ꮡresult.of(testGetClientCertificate_serverResult.Ꮡcs));
                }
            }
        } else 
        if (len(test.expectedClientError) > 0){
            Ꮡt.Errorf("#%d: expected client error %q, but got no error"u8, i, test.expectedClientError);
        } else 
        {
            var errΔ2 = result.err; if (errΔ2 != default!){
                Ꮡt.Errorf("#%d: server error: %v"u8, i, errΔ2);
            } else {
                test.verify(Ꮡt, i, Ꮡresult.of(testGetClientCertificate_serverResult.Ꮡcs));
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object aRsassaPssCertificateWasˢ = (@string)"A RSASSA-PSS certificate was parsed like a PKCS#1 v1.5 one, and it will be mistakenly used with rsa_pss_rsae_* signature algorithms"u8;

public static void TestRSAPSSKeyError(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
"""u8));
    if (b == nil) {
        Ꮡt.Fatal(failedToDecodeˢ);
    }
    var (cert, err) = Δx509.ParseCertificate((~b).Bytes);
    if (err != default!) {
        return;
    }
    {
        var (_, ok) = (~cert).PublicKey._<ж<rsa.PublicKey>>(ᐧ); if (ok) {
            Ꮡt.Error(aRsassaPssCertificateWasˢ);
        }
    }
}

public static partial void TestCloseClientConnectionOnIdleServer(ж<testing.T> Ꮡt) {
    var (clientConn, serverConn) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
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
    if (err != default!){
        {
            var (errΔ1, ok) = err._<netꓸError>(ᐧ); if (ok && errΔ1.Timeout()) {
                Ꮡt.Errorf("Expected a closed network connection error but got '%s'"u8, errΔ1.Error());
            }
        }
    } else {
        Ꮡt.Errorf("Error expected, but no error returned"u8);
    }
}

internal static error testDowngradeCanary(ж<testing.T> Ꮡt, uint16 clientVersion, uint16 serverVersion) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            testingOnlyForceDowngradeCanary = false;
        }, ref ᒐ);
        testingOnlyForceDowngradeCanary = true;
        var clientConfig = testConfig.Clone();
        clientConfig.Value.MaxVersion = clientVersion;
        var serverConfig = testConfig.Clone();
        serverConfig.Value.MaxVersion = serverVersion;
        var (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTheRestOfTheˢ = (@string)"skipping the rest of the checks in short mode"u8;

public static void TestDowngradeCanary(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS13, VersionTLS12); if (err == default!) {
            Ꮡt.Errorf("downgrade from TLS 1.3 to TLS 1.2 was not detected"u8);
        }
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingTheRestOfTheˢ);
    }
    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS13, VersionTLS11); if (err == default!) {
            Ꮡt.Errorf("downgrade from TLS 1.3 to TLS 1.1 was not detected"u8);
        }
    }
    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS13, VersionTLS10); if (err == default!) {
            Ꮡt.Errorf("downgrade from TLS 1.3 to TLS 1.0 was not detected"u8);
        }
    }
    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS12, VersionTLS11); if (err == default!) {
            Ꮡt.Errorf("downgrade from TLS 1.2 to TLS 1.1 was not detected"u8);
        }
    }
    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS12, VersionTLS10); if (err == default!) {
            Ꮡt.Errorf("downgrade from TLS 1.2 to TLS 1.0 was not detected"u8);
        }
    }
    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS13, VersionTLS13); if (err != default!) {
            Ꮡt.Errorf("server unexpectedly sent downgrade canary for TLS 1.3"u8);
        }
    }
    {
        var err = testDowngradeCanary(Ꮡt, VersionTLS12, VersionTLS12); if (err != default!) {
            Ꮡt.Errorf("client didn't ignore expected TLS 1.2 canary"u8);
        }
    }
    if (!fips140tls.Required()){
        {
            var err = testDowngradeCanary(Ꮡt, VersionTLS11, VersionTLS11); if (err != default!) {
                Ꮡt.Errorf("client unexpectedly reacted to a canary in TLS 1.1"u8);
            }
        }
        {
            var err = testDowngradeCanary(Ꮡt, VersionTLS10, VersionTLS10); if (err != default!) {
                Ꮡt.Errorf("client unexpectedly reacted to a canary in TLS 1.0"u8);
            }
        }
    } else {
        Ꮡt.Logf("skiping TLS 1.1 and TLS 1.0 downgrade canary checks in FIPS mode"u8);
    }
}

public static void TestResumptionKeepsOCSPAndSCT(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testResumptionKeepsOCSPAndSCT(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testResumptionKeepsOCSPAndSCT(tΔ2, VersionTLS13);
    });
}

internal static void testResumptionKeepsOCSPAndSCT(ж<testing.T> Ꮡt, uint16 ver) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Note: using RSA 2048 test certificates because they are compatible with FIPS mode.
    var (issuer, err) = Δx509.ParseCertificate(testRSA2048CertificateIssuer);
    if (err != default!) {
        Ꮡt.Fatalf("failed to parse test issuer"u8);
    }
    var roots = Δx509.NewCertPool();
    roots.AddCert(issuer);
    var clientConfig = Ꮡ(new Config(
        MaxVersion: ver,
        ClientSessionCache: NewLRUClientSessionCache(32),
        ServerName: "example.golang"u8,
        RootCAs: roots,
        Time: testTime
    ));
    var serverConfig = testConfig.Clone();
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
    serverConfig.Value.MaxVersion = ver;
    (~serverConfig).Certificates[0].OCSPStaple = new byte[]{1, 2, 3}.slice();
    (~serverConfig).Certificates[0].SignedCertificateTimestamps = new slice<byte>[]{new byte[]{4, 5, 6}.slice()}.slice();
    (_, var ccs, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    // after a new session we expect to see OCSPResponse and
    // SignedCertificateTimestamps populated as usual
    if (!bytes.Equal(ccs.OCSPResponse, (~serverConfig).Certificates[0].OCSPStaple)) {
        Ꮡt.Errorf("client ConnectionState contained unexpected OCSPResponse: wanted %v, got %v"u8,
            (~serverConfig).Certificates[0].OCSPStaple, ccs.OCSPResponse);
    }
    if (!reflect.DeepEqual(ccs.SignedCertificateTimestamps, (~serverConfig).Certificates[0].SignedCertificateTimestamps)) {
        Ꮡt.Errorf("client ConnectionState contained unexpected SignedCertificateTimestamps: wanted %v, got %v"u8,
            (~serverConfig).Certificates[0].SignedCertificateTimestamps, ccs.SignedCertificateTimestamps);
    }
    // if the server doesn't send any SCTs, repopulate the old SCTs
    var oldSCTs = (~serverConfig).Certificates[0].SignedCertificateTimestamps;
    (~serverConfig).Certificates[0].SignedCertificateTimestamps = default!;
    (_, ccs, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (!ccs.DidResume) {
        Ꮡt.Fatalf("expected session to be resumed"u8);
    }
    // after a resumed session we also expect to see OCSPResponse
    // and SignedCertificateTimestamps populated
    if (!bytes.Equal(ccs.OCSPResponse, (~serverConfig).Certificates[0].OCSPStaple)) {
        Ꮡt.Errorf("client ConnectionState contained unexpected OCSPResponse after resumption: wanted %v, got %v"u8,
            (~serverConfig).Certificates[0].OCSPStaple, ccs.OCSPResponse);
    }
    if (!reflect.DeepEqual(ccs.SignedCertificateTimestamps, oldSCTs)) {
        Ꮡt.Errorf("client ConnectionState contained unexpected SignedCertificateTimestamps after resumption: wanted %v, got %v"u8,
            oldSCTs, ccs.SignedCertificateTimestamps);
    }
    //  Only test overriding the SCTs for TLS 1.2, since in 1.3
    // the server won't send the message containing them
    if (ver == VersionTLS13) {
        return;
    }
    // if the server changes the SCTs it sends, they should override the saved SCTs
    (~serverConfig).Certificates[0].SignedCertificateTimestamps = new slice<byte>[]{new byte[]{7, 8, 9}.slice()}.slice();
    (_, ccs, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (!ccs.DidResume) {
        Ꮡt.Fatalf("expected session to be resumed"u8);
    }
    if (!reflect.DeepEqual(ccs.SignedCertificateTimestamps, (~serverConfig).Certificates[0].SignedCertificateTimestamps)) {
        Ꮡt.Errorf("client ConnectionState contained unexpected SignedCertificateTimestamps after resumption: wanted %v, got %v"u8,
            (~serverConfig).Certificates[0].SignedCertificateTimestamps, ccs.SignedCertificateTimestamps);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clientHandshakeDidNotˢ = (@string)"Client handshake did not error when the context was canceled"u8;
internal static readonly object connCloseDoesNotErrorAsˢ = (@string)"conn.Close does not error as expected when called multiple times on WASM"u8;
internal static readonly object clientConnectionWasNotˢ = (@string)"Client connection was not closed when the context was canceled"u8;

// TestClientHandshakeContextCancellation tests that canceling
// the context given to the client side conn.HandshakeContext
// interrupts the in-progress handshake.
public static partial void TestClientHandshakeContextCancellation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
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
        if (err == default!) {
            Ꮡt.Fatal(clientHandshakeDidNotˢ);
        }
        if (!AreEqual(err, context.Canceled)) {
            Ꮡt.Errorf("Unexpected client handshake error: %v"u8, err);
        }
        if (runtime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(connCloseDoesNotErrorAsˢ);
        }
        err = cli.Close();
        if (err == default!) {
            Ꮡt.Error(clientConnectionWasNotˢ);
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
public static void TestTLS13OnlyClientHelloCipherSuite(ж<testing.T> Ꮡt) {
    var tls13Tests = new TestTLS13OnlyClientHelloCipherSuite_tls13Tests[]{
        new(
            name: "nil"u8,
            ciphers: default!
        ),
        new(
            name: "empty"u8,
            ciphers: new uint16[]{}.slice()
        ),
        new(
            name: "some TLS 1.2 cipher"u8,
            ciphers: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice()
        ),
        new(
            name: "some TLS 1.3 cipher"u8,
            ciphers: new uint16[]{TLS_AES_128_GCM_SHA256}.slice()
        ),
        new(
            name: "some TLS 1.2 and 1.3 ciphers"u8,
            ciphers: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_AES_256_GCM_SHA384}.slice()
        )
    }.slice();
    foreach (var (_, tt) in tls13Tests) {
        ref var ttΔ1 = ref heap<TestTLS13OnlyClientHelloCipherSuite_tls13Tests>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(ttΔ1.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            testTLS13OnlyClientHelloCipherSuite(tΔ1, ttʗ1.ciphers);
        });
    }
}

internal static void testTLS13OnlyClientHelloCipherSuite(ж<testing.T> Ꮡt, slice<uint16> ciphers) {
    var serverConfig = Ꮡ(new Config(
        Certificates: (~testConfig).Certificates,
        GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> chi) => {
            var expectedCiphersuites = defaultCipherSuitesTLS13NoAES;
            if (fips140tls.Required()) {
                expectedCiphersuites = defaultCipherSuitesTLS13FIPS;
            }
            if (len((~chi).CipherSuites) != len(expectedCiphersuites)){
                Ꮡt.Errorf("only TLS 1.3 suites should be advertised, got=%x"u8, (~chi).CipherSuites);
            } else {
                foreach (var (i, _) in expectedCiphersuites) {
                    {
                        var (want, got) = (expectedCiphersuites[i], (~chi).CipherSuites[i]); if (want != got) {
                            Ꮡt.Errorf("cipher at index %d does not match, want=%x, got=%x"u8, i, want, got);
                        }
                    }
                }
            }
            return (default!, default!);
        }
    ));
    var clientConfig = Ꮡ(new Config(
        MinVersion: VersionTLS13, // client only supports TLS 1.3

        CipherSuites: ciphers,
        InsecureSkipVerify: true
    ));
    {
        var (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig); if (err != default!) {
            Ꮡt.Fatalf("handshake failed: %s"u8, err);
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
    return (len(data), default!);
}

// largeRSAKeyCertPEM contains a 8193 bit RSA key
internal static readonly @string largeRSAKeyCertPEM = """
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
"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSentCertificateˢ = "tls: server sent certificate containing RSA key larger than 8192 bits"u8;
internal static readonly @string tlsClientSentCertificateˢ = "tls: client sent certificate containing RSA key larger than 8192 bits"u8;

public static void TestHandshakeRSATooBig(ж<testing.T> Ꮡt) {
    var (testCert, _) = pem.Decode(slice<byte>(largeRSAKeyCertPEM));
    var c = Ꮡ(new Conn(conn: new tls_internal_test_package.discardConnжConn(Ꮡ(new discardConn(nil))), config: testConfig.Clone()));
    @string expectedErr = tlsServerSentCertificateˢ;
    var err = c.verifyServerCertificate(new slice<byte>[]{(~testCert).Bytes}.slice());
    if (err == default! || err.Error() != expectedErr) {
        Ꮡt.Errorf("Conn.verifyServerCertificate unexpected error: want %q, got %q"u8, expectedErr, err);
    }
    expectedErr = tlsClientSentCertificateˢ;
    err = c.processCertsFromClient(new Certificate(ΔCertificate: new slice<byte>[]{(~testCert).Bytes}.slice()));
    if (err == default! || err.Error() != expectedErr) {
        Ꮡt.Errorf("Conn.processCertsFromClient unexpected error: want %q, got %q"u8, expectedErr, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string callbackErrˢ = "callback err"u8;

internal partial struct TestTLS13ECHRejectionCallbacks_type /*dyn*/ {
    internal @string name;
    internal @string expectedErr;
    internal Func<global::go.crypto.tls_package.ΔConnectionState, error> verifyConnection;
    internal Func<slice<slice<byte>>, slice<slice<ж<Δx509.Certificate>>>, error> verifyPeerCertificate;
    internal Func<global::go.crypto.tls_package.ΔConnectionState, error> encryptedClientHelloRejectionVerify;
}

public static void TestTLS13ECHRejectionCallbacks(ж<testing.T> Ꮡt) {
    var (k, err) = ecdsa.GenerateKey(elliptic.P256(), go.crypto.rand_package.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(CommonName: "test"u8),
        DNSNames: new @string[]{"example.golang"u8}.slice(),
        NotBefore: (~testConfig).Time().Add(-time_package.ΔHour),
        NotAfter: (~testConfig).Time().Add(time_package.ΔHour)
    ));
    (var certDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, k.Public(), k.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var cert, err) = Δx509.ParseCertificate(certDER);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{
        new(
            ΔCertificate: new slice<byte>[]{certDER}.slice(),
            PrivateKey: k.OrTypedNil()
        )
    }.slice();
    serverConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.RootCAs = Δx509.NewCertPool();
    (~clientConfig).RootCAs.AddCert(cert);
    clientConfig.Value.MinVersion = VersionTLS13;
    (clientConfig.Value.EncryptedClientHelloConfigList, _) = hex.DecodeString("0041fe0d003d0100200020204bed0a11fc0dde595a9b78d966b0011128eb83f65d3c91c1cc5ac786cd246f000400010001ff0e6578616d706c652e676f6c616e670000"u8);
    clientConfig.Value.ServerName = exampleGolangˢ;
    foreach (var (_, vᴛ1) in new TestTLS13ECHRejectionCallbacks_type[]{
        new(
            name: "no callbacks"u8,
            expectedErr: "tls: server rejected ECH"u8
        ),
        new(
            name: "EncryptedClientHelloRejectionVerify, no err"u8,
            encryptedClientHelloRejectionVerify: (global::go.crypto.tls_package.ΔConnectionState _Δp0) => default!,
            expectedErr: "tls: server rejected ECH"u8
        ),
        new(
            name: "EncryptedClientHelloRejectionVerify, err"u8,
            encryptedClientHelloRejectionVerify: (global::go.crypto.tls_package.ΔConnectionState _Δp0) => errors.New(callbackErrˢ), // testHandshake returns the server side error, so we just need to
 // check alertBadCertificate was sent

            expectedErr: "callback err"u8
        ),
        new(
            name: "VerifyConnection, err"u8,
            verifyConnection: (global::go.crypto.tls_package.ΔConnectionState _Δp0) => errors.New(callbackErrˢ),
            expectedErr: "tls: server rejected ECH"u8
        ),
        new(
            name: "VerifyPeerCertificate, err"u8,
            verifyPeerCertificate: (slice<slice<byte>> _Δp0, slice<slice<ж<Δx509.Certificate>>> _Δp1) => errors.New(callbackErrˢ),
            expectedErr: "tls: server rejected ECH"u8
        )
    }.slice()) {
        ref var tc = ref heap(new TestTLS13ECHRejectionCallbacks_type(), out var Ꮡtc);
        tc = vᴛ1;

        var clientConfigʗ1 = clientConfig;
        var serverConfigʗ1 = serverConfig;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
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
            cConfig.Value.VerifyConnection = tcʗ1.verifyConnection;
            cConfig.Value.VerifyPeerCertificate = tcʗ1.verifyPeerCertificate;
            cConfig.Value.EncryptedClientHelloRejectionVerify = tcʗ1.encryptedClientHelloRejectionVerify;
            var clientErr = Client(c, cConfig).Handshake();
            c.Close();
            if (tcʗ1.expectedErr == ""u8 && clientErr != default!){
                tΔ1.Fatalf("unexpected err: %s"u8, clientErr);
            } else 
            if (clientErr != default! && tcʗ1.expectedErr != clientErr.Error()) {
                tΔ1.Fatalf("unexpected err: got %q, want %q"u8, clientErr, tcʗ1.expectedErr);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string serverTlsClientOfferedˢ = "server: tls: client offered only unsupported versions: [304]\nclient: remote error: tls: protocol version not supported"u8;

public static void TestECHTLS12Server(ж<testing.T> Ꮡt) {
    var (clientConfig, serverConfig) = (testConfig.Clone(), testConfig.Clone());
    serverConfig.Value.MaxVersion = VersionTLS12;
    clientConfig.Value.MinVersion = 0;
    (clientConfig.Value.EncryptedClientHelloConfigList, _) = hex.DecodeString("0041fe0d003d0100200020204bed0a11fc0dde595a9b78d966b0011128eb83f65d3c91c1cc5ac786cd246f000400010001ff0e6578616d706c652e676f6c616e670000"u8);
    @string expectedErr = serverTlsClientOfferedˢ;
    var (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err == default! || err.Error() != expectedErr) {
        Ꮡt.Fatalf("unexpected handshake error: got %q, want %q"u8, err, expectedErr);
    }
}

} // end tls_internal_test_package
