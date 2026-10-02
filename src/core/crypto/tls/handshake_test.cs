// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using ed25519 = go.crypto.ed25519_package;
using Δx509 = go.crypto.x509_package;
using hex = encoding.hex_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using os = os_package;
using exec = go.os.exec_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using ecdsa = go.crypto.ecdsa_package;
using encoding;
using fs = go.io.fs_package;
using go.crypto;
using go.os;
using hash = hash_package;
using rsa = go.crypto.rsa_package;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

// TLS reference tests run a connection against a reference implementation
// (OpenSSL) of TLS and record the bytes of the resulting connection. The Go
// code, during a test, is configured with deterministic randomness and so the
// reference test can be reproduced exactly in the future.
//
// In order to save everyone who wishes to run the tests from needing the
// reference implementation installed, the reference connections are saved in
// files in the testdata directory. Thus running the tests involves nothing
// external, but creating and updating them requires the reference
// implementation.
//
// Tests can be updated by running them with the -update flag. This will cause
// the test files for failing tests to be regenerated. Since the reference
// implementation will always generate fresh random numbers, large parts of the
// reference connection will always change.
internal static ж<bool> update = flag.Bool("update"u8, false, "update golden files on failure"u8);
internal static ж<@string> keyFile = flag.String("keylog"u8, ""u8, "destination file for KeyLogWriter"u8);
internal static ж<bool> bogoMode = flag.Bool("bogo-mode"u8, false, "Enabled bogo shim mode, ignore everything else"u8);
internal static ж<@string> bogoFilter = flag.String("bogo-filter"u8, ""u8, "BoGo test filter"u8);
internal static ж<@string> bogoLocalDir = flag.String("bogo-local-dir"u8, ""u8, "Local BoGo to use, instead of fetching from source"u8);

internal static void runTestAndUpdateIfNeeded(ж<testing.T> Ꮡt, @string name, Action<ж<testing.T>, bool> run, bool wait) {
    ref var t = ref Ꮡt.DerefOrNull();

    // FIPS mode is non-deterministic and so isn't suited for testing against static test transcripts.
    skipFIPS(Ꮡt);
    var success = Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
        if (!update.Value && !wait) {
            tΔ1.Parallel();
        }
        run(tΔ1, false);
    });
    if (!success && update.Value) {
        Ꮡt.Run(name + "#update"u8, (ж<testing.T> tΔ2) => {
            run(tΔ2, true);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string opensslˢ = "openssl"u8;
internal static readonly @string versionˢ = "version"u8;
internal static readonly @string openSSL111ˢ = "OpenSSL 1.1.1"u8;
internal static readonly @string versionOfOpenSSLDoesNotˢ = "version of OpenSSL does not appear to be suitable for updating test data"u8;

// checkOpenSSLVersion ensures that the version of OpenSSL looks reasonable
// before updating the test data.
internal static error checkOpenSSLVersion() {
    if (!update.Value) {
        return default!;
    }
    var openssl = exec.Command(opensslˢ, versionˢ);
    var (output, err) = openssl.CombinedOutput();
    if (err != default!) {
        return err;
    }
    @string version = ((@string)output);
    if (strings.HasPrefix(version, openSSL111ˢ)) {
        return default!;
    }
    println((@string)"***********************************************"u8);
    println((@string)""u8);
    println((@string)"You need to build OpenSSL 1.1.1 from source in order"u8);
    println((@string)"to update the test data."u8);
    println((@string)""u8);
    println((@string)"Configure it with:"u8);
    println((@string)"./Configure enable-weak-ssl-ciphers no-shared"u8);
    println((@string)"and then add the apps/ directory at the front of your PATH."u8);
    println((@string)"***********************************************"u8);
    return errors.New(versionOfOpenSSLDoesNotˢ);
}

// recordingConn is a net.Conn that records the traffic that passes through it.
// WriteTo can be used to produce output that can be later be loaded with
// ParseTestData.
[GoType] internal partial struct recordingConn {
    [GoEmbedded] public net_package.Conn Conn;
    public partial ref sync_package.Mutex Mutex { get; }
    internal slice<slice<byte>> flows;
    internal bool reading;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *recordingConn's method set; see the pointer-only satisfaction record.
internal static error Close(this recordingConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *recordingConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this recordingConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *recordingConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this recordingConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *recordingConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this recordingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *recordingConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this recordingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *recordingConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this recordingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint n, error err) Read(this ж<recordingConn> Ꮡr, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        {
            (n, err) = r.Conn.Read(b); if (n == 0) {
                goto ᒐdone;
            }
        }
        b = b[..(int)(n)];
        Ꮡr.of(recordingConn.ᏑMutex).Lock();
        defer(Ꮡr.of(recordingConn.ᏑMutex).Unlock, ref ᒐ);
        {
            nint l = len(r.flows); if (l == 0 || !r.reading){
                var buf = new slice<byte>(len(b));
                copy(buf, b);
                r.flows = append(r.flows, buf);
            } else {
                r.flows[l - 1] = appendꓸꓸꓸ(r.flows[l - 1], b[..(int)(n)]);
            }
        }
        r.reading = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static (nint n, error err) Write(this ж<recordingConn> Ꮡr, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        {
            (n, err) = r.Conn.Write(b); if (n == 0) {
                goto ᒐdone;
            }
        }
        b = b[..(int)(n)];
        Ꮡr.of(recordingConn.ᏑMutex).Lock();
        defer(Ꮡr.of(recordingConn.ᏑMutex).Unlock, ref ᒐ);
        {
            nint l = len(r.flows); if (l == 0 || r.reading){
                var buf = new slice<byte>(len(b));
                copy(buf, b);
                r.flows = append(r.flows, buf);
            } else {
                r.flows[l - 1] = appendꓸꓸꓸ(r.flows[l - 1], b[..(int)(n)]);
            }
        }
        r.reading = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

// WriteTo writes Go source code to w that contains the recorded traffic.
[GoRecv] internal static (int64, error) WriteTo(this ref recordingConn r, io.Writer w) {
    // TLS always starts with a client to server flow.
    var clientToServer = true;
    int64 written = default!;
    foreach (var (i, flow) in r.flows) {
        @string source = clientˢ;
        @string dest = serverˢ;
        if (!clientToServer) {
            (source, dest) = (dest, source);
        }
        var (n, err) = fmt.Fprintf(w, ">>> Flow %d (%s to %s)\n"u8, i + 1, source, dest);
        written += (int64)n;
        if (err != default!) {
            return (written, err);
        }
        var dumper = hex.Dumper(w);
        (n, err) = dumper.Write(flow);
        written += (int64)n;
        if (err != default!) {
            return (written, err);
        }
        err = dumper.Close();
        if (err != default!) {
            return (written, err);
        }
        clientToServer = !clientToServer;
    }
    return (written, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidTestDataˢ = "invalid test data"u8;

internal static (slice<slice<byte>> flows, error err) parseTestData(io.Reader r) {
    slice<slice<byte>> flows = default!;

    slice<byte> currentFlow = default!;
    var scanner = bufio.NewScanner(r);
    while (scanner.Scan()) {
        @string line = scanner.Text();
        // If the line starts with ">>> " then it marks the beginning
        // of a new flow.
        if (strings.HasPrefix(line, ">>> "u8)) {
            if (len(currentFlow) > 0 || len(flows) > 0) {
                flows = append(flows, currentFlow);
                currentFlow = default!;
            }
            continue;
        }
        // Otherwise the line is a line of hex dump that looks like:
        // 00000170  fc f5 06 bf (...)  |.....X{&?......!|
        // (Some bytes have been omitted from the middle section.)
        var (_, after, ok) = strings.Cut(line, " "u8);
        if (!ok) {
            return (default!, errors.New(invalidTestDataˢ));
        }
        line = after;
        (var before, _, ok) = strings.Cut(line, "|"u8);
        if (!ok) {
            return (default!, errors.New(invalidTestDataˢ));
        }
        line = before;
        var hexBytes = strings.Fields(line);
        foreach (var (_, hexByte) in hexBytes) {
            var (val, errΔ1) = strconv.ParseUint(hexByte, 16, 8);
            if (errΔ1 != default!) {
                return (default!, errors.New("invalid hex byte in test data: "u8 + errΔ1.Error()));
            }
            currentFlow = append(currentFlow, (byte)val);
        }
    }
    if (len(currentFlow) > 0) {
        flows = append(flows, currentFlow);
    }
    return (flows, default!);
}

// replayingConn is a net.Conn that replays flows recorded by recordingConn.
[GoType] internal partial struct replayingConn {
    internal testing.TB t;
    public partial ref sync_package.Mutex Mutex { get; }
    internal slice<slice<byte>> flows;
    internal bool reading;
}

internal static net.Conn _ᴛ11ʗ = new tls_internal_test_package.replayingConnжConn(((ж<replayingConn>)nil));

internal static (nint n, error err) Read(this ж<replayingConn> Ꮡr, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        Ꮡr.of(replayingConn.ᏑMutex).Lock();
        defer(Ꮡr.of(replayingConn.ᏑMutex).Unlock, ref ᒐ);
        if (!r.reading) {
            r.t.Errorf("expected write, got read"u8);
            (n, err) = (0, fmt.Errorf("recording expected write, got read"u8)); goto ᒐdone;
        }
        n = copy(b, r.flows[0]);
        r.flows[0] = r.flows[0][(int)(n)..];
        if (len(r.flows[0]) == 0) {
            r.flows = r.flows[1..];
            if (len(r.flows) == 0){
                (n, err) = (n, io.EOF); goto ᒐdone;
            } else {
                r.reading = false;
            }
        }
        (n, err) = (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static (nint n, error err) Write(this ж<replayingConn> Ꮡr, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        Ꮡr.of(replayingConn.ᏑMutex).Lock();
        defer(Ꮡr.of(replayingConn.ᏑMutex).Unlock, ref ᒐ);
        if (r.reading) {
            r.t.Errorf("expected read, got write"u8);
            (n, err) = (0, fmt.Errorf("recording expected read, got write"u8)); goto ᒐdone;
        }
        if (!bytes.HasPrefix(r.flows[0], b)) {
            r.t.Errorf("write mismatch: expected %x, got %x"u8, r.flows[0], b);
            (n, err) = (0, fmt.Errorf("write mismatch"u8)); goto ᒐdone;
        }
        r.flows[0] = r.flows[0][(int)(len(b))..];
        if (len(r.flows[0]) == 0) {
            r.flows = r.flows[1..];
            r.reading = true;
        }
        (n, err) = (len(b), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static error Close(this ж<replayingConn> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        Ꮡr.of(replayingConn.ᏑMutex).Lock();
        ᒐd1 = true;
        if (len(r.flows) > 0) {
            r.t.Errorf("closed with unfinished flows"u8);
            return fmt.Errorf("unexpected close"u8);
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡr.of(replayingConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static netꓸAddr LocalAddr(this ref replayingConn r) {
    return default!;
}

[GoRecv] internal static netꓸAddr RemoteAddr(this ref replayingConn r) {
    return default!;
}

[GoRecv] internal static error SetDeadline(this ref replayingConn r, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetReadDeadline(this ref replayingConn r, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetWriteDeadline(this ref replayingConn r, time.Time t) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goTlsTestˢ = "go-tls-test"u8;

// tempFile creates a temp file containing contents and returns its path.
internal static @string tempFile(@string contents) {
    var (@file, err) = os.CreateTemp(""u8, goTlsTestˢ);
    if (err != default!) {
        throw panic("failed to create temp file: " + err.Error());
    }
    @string path = @file.Name();
    @file.WriteString(contents);
    @file.Close();
    return path;
}

// localListener is set up by TestMain and used by localPipe to create Conn
// pairs like net.Pipe, but connected by an actual buffered TCP connection.

[GoType("dyn")] partial struct localListenerᴛ1 {
    internal sync.Mutex mu;
    internal netꓸAddr addr;
    internal channel<net.Conn> ch;
}
internal static ж<localListenerᴛ1> ᏑlocalListener = new StandardBox<localListenerᴛ1>(default(localListenerᴛ1));
internal static ref localListenerᴛ1 localListener => ref ᏑlocalListener.Value;

internal static UntypedInt localFlakes => 0; // change to 1 or 2 to exercise localServer/localPipe handling of mismatches

internal static void localServer(net.Listener l) {
    for (nint n = 0; ᐧ ; n++) {
        var (c, err) = l.Accept();
        if (err != default!) {
            return;
        }
        if (localFlakes == 1 && n % 2 == 0) {
            c.Close();
            continue;
        }
        localListener.ch.ᐸꟷ(c);
    }
}

internal static Func<error, bool> isConnRefused = (error err) => false;

internal static (net.Conn, net.Conn) localPipe(testing.TB t) {
    GoFrame ᒐ = default;
    try {
        ᏑlocalListener.of(localListenerᴛ1.Ꮡmu).Lock();
        defer(ᏑlocalListener.of(localListenerᴛ1.Ꮡmu).Unlock, ref ᒐ);
        var addr = localListener.addr;
        error err = default!;
Dialing:
        for (nint i = 0; i < 5; i++) {
            // We expect a rare mismatch, but probably not 5 in a row.
            var tooSlow = time_package.NewTimer(1 * time_package.ΔSecond);
            var tooSlowʗ1 = tooSlow;
            defer(() => tooSlowʗ1.Stop(), ref ᒐ);
            ref var c1 = ref heap<net.Conn>(out var Ꮡc1);
            (c1, err) = net.Dial(addr.Network(), addr.String());
            if (err != default!) {
                if (runtime.GOOS == "dragonfly"u8 && (isConnRefused(err) || os.IsTimeout(err))) {
                    // golang.org/issue/29583: Dragonfly sometimes returns a spurious
                    // ECONNREFUSED or ETIMEDOUT.
                    ᐸꟷ((~tooSlow).C);
                    continue;
                }
                t.Fatalf("localPipe: %v"u8, err);
            }
            if (localFlakes == 2 && i == 0) {
                c1.Close();
                continue;
            }
            while (ᐧ) {
                var selᴛ6 = (~tooSlow).C;
                var selᴛ7 = localListener.ch;
                switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ), ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
                case 0 when selᴛ6.ꟷᐳ(out _): {
                    t.Logf("localPipe: timeout waiting for %v"u8, c1.LocalAddr());
                    c1.Close();
                    goto continue_Dialing;
                    break;
                }
                case 1 when selᴛ7.ꟷᐳ(out var c2): {
                    if (c2.RemoteAddr().String() == c1.LocalAddr().String()) {
                        t.Cleanup(() => {
                            Ꮡc1.ValueSlot.Close();
                        });
                        var c2ʗ1 = c2;
                        t.Cleanup(() => {
                            c2ʗ1.Close();
                        });
                        return (c1, c2);
                    }
                    t.Logf("localPipe: unexpected connection: %v != %v"u8, c2.RemoteAddr(), c1.LocalAddr());
                    c2.Close();
                    break;
                }}
            }
continue_Dialing:;
        }
break_Dialing:;
        t.Fatalf("localPipe: failed to connect: %v"u8, err);
        throw panic("unreachable");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// zeroSource is an io.Reader that returns an unlimited number of zero bytes.
[GoType] internal partial struct zeroSource {
}

internal static (nint n, error err) Read(this zeroSource _, slice<byte> b) {
    clear(b);
    return (len(b), default!);
}

internal static slice<uint16> allCipherSuites() {
    var ids = new slice<uint16>(len(ΔcipherSuites));
    foreach (var (i, suite) in ΔcipherSuites) {
        ids[i] = suite.Value.id;
    }
    return ids;
}

internal static ж<global::go.crypto.tls_package.Config> testConfig;

public static void TestMain(ж<testing.M> Ꮡm) {
    flag.Usage = () => {
        fmt.Fprintf(flag.CommandLine.Output(), "Usage of %s:\n"u8, os.Args);
        flag.PrintDefaults();
        if (bogoMode.Value) {
            os.Exit(89);
        }
    };
    flag.Parse();
    if (bogoMode.Value) {
        bogoShim();
        os.Exit(0);
    }
    os.Exit(runMain(Ꮡm));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcp6ˢ = "tcp6"u8;

internal static nint runMain(ж<testing.M> Ꮡm) {
    GoFrame ᒐ = default;
    try {
        // Cipher suites preferences change based on the architecture. Force them to
        // the version without AES acceleration for test consistency.
        hasAESGCMHardwareSupport = false;
        // Set up localPipe.
        var (l, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            (l, err) = net.Listen(tcp6ˢ, "[::1]:0"u8);
        }
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "Failed to open local listener: %v"u8, err);
            os.Exit(1);
        }
        localListener.ch = new channel<net.Conn>(0);
        localListener.addr = l.Addr();
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        goǃ(localServer, l);
        {
            var errΔ1 = checkOpenSSLVersion(); if (errΔ1 != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: %v"u8, errΔ1);
                os.Exit(1);
            }
        }
        testConfig = Ꮡ(new Config(
            Time: () => time_package.Unix(0, 0),
            Rand: new zeroSource(nil),
            Certificates: new slice<global::go.crypto.tls_package.Certificate>(2),
            InsecureSkipVerify: true,
            CipherSuites: allCipherSuites(),
            CurvePreferences: new global::go.crypto.tls_package.CurveID[]{X25519, CurveP256, CurveP384, CurveP521}.slice(),
            MinVersion: VersionTLS10,
            MaxVersion: VersionTLS13
        ));
        (~testConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testRSACertificate}.slice();
        (~testConfig).Certificates[0].PrivateKey = testRSAPrivateKey.OrTypedNil();
        (~testConfig).Certificates[1].ΔCertificate = new slice<byte>[]{testSNICertificate}.slice();
        (~testConfig).Certificates[1].PrivateKey = testRSAPrivateKey.OrTypedNil();
        testConfig.BuildNameToCertificate();
        if (keyFile.Value != ""u8) {
            var (f, errΔ2) = os.OpenFile(keyFile.Value, (nint)((nint)(nint)(os.O_APPEND | os.O_CREATE) | os.O_WRONLY), 420);
            if (errΔ2 != default!) {
                throw panic("failed to open -keylog file: " + errΔ2.Error());
            }
            testConfig.Value.KeyLogWriter = new os.FileжWriter(f);
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
        }
        return Ꮡm.Run();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (global::go.crypto.tls_package.ΔConnectionState serverState, global::go.crypto.tls_package.ΔConnectionState clientState, error err) testHandshake(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.Config> ᏑclientConfig, ж<global::go.crypto.tls_package.Config> ᏑserverConfig) {
    global::go.crypto.tls_package.ΔConnectionState serverState = default!;
    global::go.crypto.tls_package.ΔConnectionState clientState = default!;
    error err = default!;

    @string sentinel = "SENTINEL\n"u8;
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var errChan = new channel<error>(1);
    var cʗ1 = c;
    var errChanʗ1 = errChan;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var cli = Client(cʗ1, ᏑclientConfig);
            var errΔ1 = cli.Handshake();
            if (errΔ1 != default!) {
                errChanʗ1.ᐸꟷ(fmt.Errorf("client: %v"u8, errΔ1));
                cʗ1.Close();
                return;
            }
            var errChanʗ2 = errChanʗ1;
            defer(() => {
                errChanʗ2.ᐸꟷ(default!);
            }, ref ᒐ);
            clientState = cli.ConnectionState();
            (var buf, errΔ1) = io.ReadAll(new tls_test_package.tls_ConnжReader(cli));
            if (errΔ1 != default!) {
                Ꮡt.Errorf("failed to call cli.Read: %v"u8, errΔ1);
            }
            {
                @string got = ((@string)buf); if (got != sentinel) {
                    Ꮡt.Errorf("read %q from TLS connection, but expected %q"u8, got, sentinel);
                }
            }
            // We discard the error because after ReadAll returns the server must
            // have already closed the connection. Sending data (the closeNotify
            // alert) can cause a reset, that will make Close return an error.
            cli.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var server = Server(s, ᏑserverConfig);
    err = server.Handshake();
    if (err == default!){
        serverState = server.ConnectionState();
        {
            var (_, errΔ2) = io.WriteString(new tls_test_package.tls_ConnжWriter(server), sentinel); if (errΔ2 != default!) {
                Ꮡt.Errorf("failed to call server.Write: %v"u8, errΔ2);
            }
        }
        {
            var errΔ3 = server.Close(); if (errΔ3 != default!) {
                Ꮡt.Errorf("failed to call server.Close: %v"u8, errΔ3);
            }
        }
    } else {
        err = fmt.Errorf("server: %v"u8, err);
        s.Close();
    }
    err = errors.Join(err, ᐸꟷ(errChan));
    return (serverState, clientState, err);
}

internal static slice<byte> fromHex(@string s) {
    var (b, _) = hex.DecodeString(s);
    return b;
}

// testTime is 2016-10-20T17:32:09.000Z, which is within the validity period of
// [testRSACertificate], [testRSACertificateIssuer], [testRSA2048Certificate],
// [testRSA2048CertificateIssuer], and [testECDSACertificate].
internal static Func<time.Time> testTime = () => time_package.Unix(1476984729, 0);

internal static slice<byte> testRSACertificate = fromHex("3082024b308201b4a003020102020900e8f09d3fe25beaa6300d06092a864886f70d01010b0500301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f74301e170d3136303130313030303030305a170d3235303130313030303030305a301a310b3009060355040a1302476f310b300906035504031302476f30819f300d06092a864886f70d010101050003818d0030818902818100db467d932e12270648bc062821ab7ec4b6a25dfe1e5245887a3647a5080d92425bc281c0be97799840fb4f6d14fd2b138bc2a52e67d8d4099ed62238b74a0b74732bc234f1d193e596d9747bf3589f6c613cc0b041d4d92b2b2423775b1c3bbd755dce2054cfa163871d1e24c4f31d1a508baab61443ed97a77562f414c852d70203010001a38193308190300e0603551d0f0101ff0404030205a0301d0603551d250416301406082b0601050507030106082b06010505070302300c0603551d130101ff0402300030190603551d0e041204109f91161f43433e49a6de6db680d79f60301b0603551d230414301280104813494d137e1631bba301d5acab6e7b30190603551d1104123010820e6578616d706c652e676f6c616e67300d06092a864886f70d01010b0500038181009d30cc402b5b50a061cbbae55358e1ed8328a9581aa938a495a1ac315a1a84663d43d32dd90bf297dfd320643892243a00bccf9c7db74020015faad3166109a276fd13c3cce10c5ceeb18782f16c04ed73bbb343778d0c1cf10fa1d8408361c94c722b9daedb4606064df4c1b33ec0d1bd42d4dbfe3d1360845c21d33be9fae7"u8);

internal static slice<byte> testRSACertificateIssuer = fromHex("3082021930820182a003020102020900ca5e4e811a965964300d06092a864886f70d01010b0500301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f74301e170d3136303130313030303030305a170d3235303130313030303030305a301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f7430819f300d06092a864886f70d010101050003818d0030818902818100d667b378bb22f34143b6cd2008236abefaf2852adf3ab05e01329e2c14834f5105df3f3073f99dab5442d45ee5f8f57b0111c8cb682fbb719a86944eebfffef3406206d898b8c1b1887797c9c5006547bb8f00e694b7a063f10839f269f2c34fff7a1f4b21fbcd6bfdfb13ac792d1d11f277b5c5b48600992203059f2a8f8cc50203010001a35d305b300e0603551d0f0101ff040403020204301d0603551d250416301406082b0601050507030106082b06010505070302300f0603551d130101ff040530030101ff30190603551d0e041204104813494d137e1631bba301d5acab6e7b300d06092a864886f70d01010b050003818100c1154b4bab5266221f293766ae4138899bd4c5e36b13cee670ceeaa4cbdf4f6679017e2fe649765af545749fe4249418a56bd38a04b81e261f5ce86b8d5c65413156a50d12449554748c59a30c515bc36a59d38bddf51173e899820b282e40aa78c806526fd184fb6b4cf186ec728edffa585440d2b3225325f7ab580e87dd76"u8);

internal static slice<byte> testRSA2048Certificate = fromHex("30820316308201fea003020102020900e8f09d3fe25beaa6300d06092a864886f70d01010b0500301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f74301e170d3136303130313030303030305a170d3338303130313030303030305a301a310b3009060355040a1302476f310b300906035504031302476f30820122300d06092a864886f70d01010105000382010f003082010a0282010100e0ac47db9ba1b7f98a996c62dc1d248d4ee570544136fe4e911e22fccc0fe2b20982f3c4cdd8f4065c5068c873ca0a768b80dc915edc66541a5f26cdea44e56e411221e2f9927bf4e009fee76dbe0e118dcc13392efd6f42d8eb2fd5bc8f63ac77800c84d3be90c20c321273254b9137ef61f825dad1ec2c5e75aa4be6d3104899bd5ac400da7ab942b4227a3870ae5bb97870aa09a1082fb8e78b944cd7fd1b0c6fb1cce03b5430b12ef9ce2d95e01821766e998df0cc99202a57cf030577bd2dc0ec85a49f203511bb6f0e9f43398ead0958f8d7534c61e81daf4501faaa68d9cbc725b58401900fa48a3e2333b15c88cf0c5cc8f33fb9464f9d5f5768b8f10203010001a35a3058300e0603551d0f0101ff0404030205a0301d0603551d250416301406082b0601050507030106082b06010505070302300c0603551d130101ff0402300030190603551d1104123010820e6578616d706c652e676f6c616e67300d06092a864886f70d01010b050003820101009e83f835e2da08204ee6f8bdca793cf83c7aec175349c1642dfbe9f4d0dcfb1aedb4d0122e16c2ad92e63dd31cce10ca5dd04be48cded0fdc8fea49e891d9d93e778a67d54b619ac167ce7bb0f6000ca00c5677d09df3eb10080134ba32bfe4132d33954dc479cb266288d53d3f43af9c78c0ca59d396498bdc56d4966dc6b7e49081f7f2ae1d704bb9f9effed93c57d3b738da02edff3999e3f1a5dce2b093951947d233d9c6b6a12b4b1611826aa02544980089eebbcf22a1a96bd35a3ddf638578989334a93d5081fab442b4383ba6213b7cdd74110582244a2abd937828b311d8dd69178756db7874293b9810c5c2e833f91d49d283a62caaf359141997f"u8);

internal static slice<byte> testRSA2048CertificateIssuer = fromHex("308203223082020aa003020102020900ca5e4e811a965964300d06092a864886f70d01010b0500301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f74301e170d3136303130313030303030305a170d3235303130313030303030305a301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f7430820122300d06092a864886f70d01010105000382010f003082010a0282010100b308c1720c7054abe66e1be6f8a11246808215a810e8936e47601f7ec1afeb02ad69a5000959d4e08ebc4455ef90b39616f380b8ff2e76f29942d7e009cf010824fe56f69140ac39b761595255ec2aa35155ca2eea884f57b25f8a52f41f56f65b0197cb6c637f9adfa97d8ac27565449f64e67f8b918646ffd630601b0badd8d38aea421fe413ee94f10ea5874c2fd6d8c1b9febaa5ca0ce759993a232c9c48e52230bbf58777b0c30e07e9e0914133730d844b9887b950d5a17c779ac69de2d9c65d26f1ea46c7dd7ac636af6d77df7c9218f78c7b5f08b025867f343ac66cd43a657ac44bfd7e9d07e95a22ff9a0babf72dcffc66eba0a1d90731f67e3bbd0203010001a361305f300e0603551d0f0101ff040403020204301d0603551d250416301406082b0601050507030106082b06010505070302300f0603551d130101ff040530030101ff301d0603551d0e0416041460145a6ce2e8a15b1b68db9a4752ce8684d6ba2d300d06092a864886f70d01010b050003820101001d342fe0b50a25d57a8b13bc14d0abb1eea7431ee752aa423e1306654183e44e9d48bbf592cd32ce77310fdc4e8bbcd724fc43d2723f454bfe605ff90d38d8c6fe60b36c6f4d2d7e4e79bceeb2484f0565274b0d0c4a8562370677624a4c133e332a9e63d4b47544c14e4908ee8685dd0760ae6f4ab089ede2b0cdc595ecefbee7d8be80d57b2d4e4510b6ceda54d1a5980540214191d81cc89a983da43d4043f8efe97a2e231c5153bded520acce87ec8c64a3408f0eb4c742c4a877e8b5b7b7f72497734a41a95994a7a103262ea6d598d03fd5cb0579ed4702424da8893334c58215bc655d49656aedcd02d18676f45d6b9469ae04b89abe9b358391cce99"u8);

internal static ж<ж<rsa.PrivateKey>> ᏑtestRSA2048PrivateKey = new StandardBox<ж<rsa.PrivateKey>>(Δx509.ParsePKCS1PrivateKey(fromHex("308204a40201000282010100e0ac47db9ba1b7f98a996c62dc1d248d4ee570544136fe4e911e22fccc0fe2b20982f3c4cdd8f4065c5068c873ca0a768b80dc915edc66541a5f26cdea44e56e411221e2f9927bf4e009fee76dbe0e118dcc13392efd6f42d8eb2fd5bc8f63ac77800c84d3be90c20c321273254b9137ef61f825dad1ec2c5e75aa4be6d3104899bd5ac400da7ab942b4227a3870ae5bb97870aa09a1082fb8e78b944cd7fd1b0c6fb1cce03b5430b12ef9ce2d95e01821766e998df0cc99202a57cf030577bd2dc0ec85a49f203511bb6f0e9f43398ead0958f8d7534c61e81daf4501faaa68d9cbc725b58401900fa48a3e2333b15c88cf0c5cc8f33fb9464f9d5f5768b8f10203010001028201007aac96efca229b199e1bf79a63256677e1c455792bc2a348b2e409a68ea57dda486740430d4290bb885c3f5a741eb567d4f41f7b2098a726f4df4f88cf899edc7c9b31f584dffedece15a7212642c7dbbdd8d806392a183e1fc30af36169c9bab9e528f0bdcd27ad4c8b6a97849da6452c6809de61848db80c3ba3289e785042cdfd46fbfee5f78adcba2927fcd8cbe9dcaa97190457eaa45d77adbe0db820aff0c8511d837ab5b307bad5f85afd2cc70d9659ec58045d97ced1eb7950670ac559449c0305fddefda1bac88d36629a177f65abad182c6470830b39e7f6dbdef4df813ccaef01d5a42d37213b2b9647e2ff56a63e6b6a4b6e8a1567bbfd77042102818100eb66f205e8507c78f7167dbef3ddf02fde6a67bd15152609e9296576e28c79678177145ae98e0a2fee58fdb3d626fb6beae3e0ae0b76bc47d16fcdeb16f0caca8a0902779979382609705ae84514de480c2fb2ddda3049347cc1bde9f1a359747079ef3dce020a3c186c90e63bc20b5489a40d768b1c1c35c679edc5662e18c702818100f454ffff95b126b55cb13b68a3841600fc0bc69ff4064f7ceb122495fa972fdb05ca2fa1c6e2e84432f81c96875ab12226e8ce92ba808c4f6325f27ce058791f05db96e623687d3cfc198e748a07521a8c7ee9e7e8faf95b0985be82b867a49f7d5d50fac3881d2c39dedfdbca3ebe847b859c9864cf7a543e4688f5a60118870281806cee737ac65950704daeebbb8c701c709a54d4f28baa00b33f6137a1bf0e5033d4963d2620c3e8f4eb2fe51eee2f95d3079c31e1784e96ac093fdaa33a376d3032961ebd27990fa192669abab715041385082196461c6813d0d37ac5a25afbcf452937cb7ae438c63c6b28d651bae6b1550c446aa1cefd42e9388d0df6cdc80b02818100cac172c33504923bb494fad8e5c0a9c5dd63244bfe63f238969632b82700a95cd71c2694d887d9f92656d0da75ae640a1441e392cda3f94bb3da7cb4f6335527d2639c809467946e34423cfe26c0d6786398ba20922d1b1a59f79bd5bc937d8040b75c890c13fb298548977a3c05ff71cf535c54f66b5a77684a7e4363a3cb2702818100a4d782f35d5a07f9c1f8f9c378564b220387d1e481cc856b631de7637d8bb77c851db070122050ac230dc6e45edf4523471c717c1cb86a36b2fd3358fae349d51be54d71d7dbeaa6af668323e2b51933f0b8488aa12723e0f32207068b4aa64ed54bcef4acbbbe35b92802faba7ed45ae52bef8313d9ef4393ccc5cf868ddbf8"u8)).Item1);
internal static ref ж<rsa.PrivateKey> testRSA2048PrivateKey => ref ᏑtestRSA2048PrivateKey.ValueSlot;
internal static error _ᴛ12ʗ;

// testRSAPSSCertificate has signatureAlgorithm rsassaPss, but subjectPublicKeyInfo
// algorithm rsaEncryption, for use with the rsa_pss_rsae_* SignatureSchemes.
// See also TestRSAPSSKeyError. testRSAPSSCertificate is self-signed.
internal static slice<byte> testRSAPSSCertificate = fromHex("308202583082018da003020102021100f29926eb87ea8a0db9fcc247347c11b0304106092a864886f70d01010a3034a00f300d06096086480165030402010500a11c301a06092a864886f70d010108300d06096086480165030402010500a20302012030123110300e060355040a130741636d6520436f301e170d3137313132333136313631305a170d3138313132333136313631305a30123110300e060355040a130741636d6520436f30819f300d06092a864886f70d010101050003818d0030818902818100db467d932e12270648bc062821ab7ec4b6a25dfe1e5245887a3647a5080d92425bc281c0be97799840fb4f6d14fd2b138bc2a52e67d8d4099ed62238b74a0b74732bc234f1d193e596d9747bf3589f6c613cc0b041d4d92b2b2423775b1c3bbd755dce2054cfa163871d1e24c4f31d1a508baab61443ed97a77562f414c852d70203010001a3463044300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff04023000300f0603551d110408300687047f000001304106092a864886f70d01010a3034a00f300d06096086480165030402010500a11c301a06092a864886f70d010108300d06096086480165030402010500a20302012003818100cdac4ef2ce5f8d79881042707f7cbf1b5a8a00ef19154b40151771006cd41626e5496d56da0c1a139fd84695593cb67f87765e18aa03ea067522dd78d2a589b8c92364e12838ce346c6e067b51f1a7e6f4b37ffab13f1411896679d18e880e0ba09e302ac067efca460288e9538122692297ad8093d4f7dd701424d7700a46a1"u8);

internal static slice<byte> testECDSACertificate = fromHex("3082020030820162020900b8bf2d47a0d2ebf4300906072a8648ce3d04013045310b3009060355040613024155311330110603550408130a536f6d652d53746174653121301f060355040a1318496e7465726e6574205769646769747320507479204c7464301e170d3132313132323135303633325a170d3232313132303135303633325a3045310b3009060355040613024155311330110603550408130a536f6d652d53746174653121301f060355040a1318496e7465726e6574205769646769747320507479204c746430819b301006072a8648ce3d020106052b81040023038186000400c4a1edbe98f90b4873367ec316561122f23d53c33b4d213dcd6b75e6f6b0dc9adf26c1bcb287f072327cb3642f1c90bcea6823107efee325c0483a69e0286dd33700ef0462dd0da09c706283d881d36431aa9e9731bd96b068c09b23de76643f1a5c7fe9120e5858b65f70dd9bd8ead5d7f5d5ccb9b69f30665b669a20e227e5bffe3b300906072a8648ce3d040103818c0030818802420188a24febe245c5487d1bacf5ed989dae4770c05e1bb62fbdf1b64db76140d311a2ceee0b7e927eff769dc33b7ea53fcefa10e259ec472d7cacda4e970e15a06fd00242014dfcbe67139c2d050ebd3fa38c25c13313830d9406bbd4377af6ec7ac9862eddd711697f857c56defb31782be4c7780daecbbe9e4e3624317b6a0f399512078f2a"u8);

internal static slice<byte> testEd25519Certificate = fromHex("3082012e3081e1a00302010202100f431c425793941de987e4f1ad15005d300506032b657030123110300e060355040a130741636d6520436f301e170d3139303531363231333830315a170d3230303531353231333830315a30123110300e060355040a130741636d6520436f302a300506032b65700321003fe2152ee6e3ef3f4e854a7577a3649eede0bf842ccc92268ffa6f3483aaec8fa34d304b300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff0402300030160603551d11040f300d820b6578616d706c652e636f6d300506032b65700341006344ed9cc4be5324539fd2108d9fe82108909539e50dc155ff2c16b71dfcab7d4dd4e09313d0a942e0b66bfe5d6748d79f50bc6ccd4b03837cf20858cdaccf0c"u8);

internal static slice<byte> testSNICertificate = fromHex("0441883421114c81480804c430820237308201a0a003020102020900e8f09d3fe25beaa6300d06092a864886f70d01010b0500301f310b3009060355040a1302476f3110300e06035504031307476f20526f6f74301e170d3136303130313030303030305a170d3235303130313030303030305a3023310b3009060355040a1302476f311430120603550403130b736e69746573742e636f6d30819f300d06092a864886f70d010101050003818d0030818902818100db467d932e12270648bc062821ab7ec4b6a25dfe1e5245887a3647a5080d92425bc281c0be97799840fb4f6d14fd2b138bc2a52e67d8d4099ed62238b74a0b74732bc234f1d193e596d9747bf3589f6c613cc0b041d4d92b2b2423775b1c3bbd755dce2054cfa163871d1e24c4f31d1a508baab61443ed97a77562f414c852d70203010001a3773075300e0603551d0f0101ff0404030205a0301d0603551d250416301406082b0601050507030106082b06010505070302300c0603551d130101ff0402300030190603551d0e041204109f91161f43433e49a6de6db680d79f60301b0603551d230414301280104813494d137e1631bba301d5acab6e7b300d06092a864886f70d01010b0500038181007beeecff0230dbb2e7a334af65430b7116e09f327c3bbf918107fc9c66cb497493207ae9b4dbb045cb63d605ec1b5dd485bb69124d68fa298dc776699b47632fd6d73cab57042acb26f083c4087459bc5a3bb3ca4d878d7fe31016b7bc9a627438666566e3389bfaeebe6becc9a0093ceed18d0f9ac79d56f3a73f18188988ed"u8);

internal static slice<byte> testP256Certificate = fromHex("308201693082010ea00302010202105012dc24e1124ade4f3e153326ff27bf300a06082a8648ce3d04030230123110300e060355040a130741636d6520436f301e170d3137303533313232343934375a170d3138303533313232343934375a30123110300e060355040a130741636d6520436f3059301306072a8648ce3d020106082a8648ce3d03010703420004c02c61c9b16283bbcc14956d886d79b358aa614596975f78cece787146abf74c2d5dc578c0992b4f3c631373479ebf3892efe53d21c4f4f1cc9a11c3536b7f75a3463044300e0603551d0f0101ff0404030205a030130603551d25040c300a06082b06010505070301300c0603551d130101ff04023000300f0603551d1104083006820474657374300a06082a8648ce3d0403020349003046022100963712d6226c7b2bef41512d47e1434131aaca3ba585d666c924df71ac0448b3022100f4d05c725064741aef125f243cdbccaa2a5d485927831f221c43023bd5ae471a"u8);

internal static ж<rsa.PrivateKey> testRSAPrivateKey = Δx509.ParsePKCS1PrivateKey(fromHex("3082025b02010002818100db467d932e12270648bc062821ab7ec4b6a25dfe1e5245887a3647a5080d92425bc281c0be97799840fb4f6d14fd2b138bc2a52e67d8d4099ed62238b74a0b74732bc234f1d193e596d9747bf3589f6c613cc0b041d4d92b2b2423775b1c3bbd755dce2054cfa163871d1e24c4f31d1a508baab61443ed97a77562f414c852d702030100010281800b07fbcf48b50f1388db34b016298b8217f2092a7c9a04f77db6775a3d1279b62ee9951f7e371e9de33f015aea80660760b3951dc589a9f925ed7de13e8f520e1ccbc7498ce78e7fab6d59582c2386cc07ed688212a576ff37833bd5943483b5554d15a0b9b4010ed9bf09f207e7e9805f649240ed6c1256ed75ab7cd56d9671024100fded810da442775f5923debae4ac758390a032a16598d62f059bb2e781a9c2f41bfa015c209f966513fe3bf5a58717cbdb385100de914f88d649b7d15309fa49024100dd10978c623463a1802c52f012cfa72ff5d901f25a2292446552c2568b1840e49a312e127217c2186615aae4fb6602a4f6ebf3f3d160f3b3ad04c592f65ae41f02400c69062ca781841a09de41ed7a6d9f54adc5d693a2c6847949d9e1358555c9ac6a8d9e71653ac77beb2d3abaf7bb1183aa14278956575dbebf525d0482fd72d90240560fe1900ba36dae3022115fd952f2399fb28e2975a1c3e3d0b679660bdcb356cc189d611cfdd6d87cd5aea45aa30a2082e8b51e94c2f3dd5d5c6036a8a615ed0240143993d80ece56f877cb80048335701eb0e608cc0c1ca8c2227b52edf8f1ac99c562f2541b5ce81f0515af1c5b4770dba53383964b4b725ff46fdec3d08907df"u8)).Item1;
internal static error _ᴛ13ʗ;

internal static ж<ж<ecdsa.PrivateKey>> ᏑtestECDSAPrivateKey = new StandardBox<ж<ecdsa.PrivateKey>>(Δx509.ParseECPrivateKey(fromHex("3081dc0201010442019883e909ad0ac9ea3d33f9eae661f1785206970f8ca9a91672f1eedca7a8ef12bd6561bb246dda5df4b4d5e7e3a92649bc5d83a0bf92972e00e62067d0c7bd99d7a00706052b81040023a18189038186000400c4a1edbe98f90b4873367ec316561122f23d53c33b4d213dcd6b75e6f6b0dc9adf26c1bcb287f072327cb3642f1c90bcea6823107efee325c0483a69e0286dd33700ef0462dd0da09c706283d881d36431aa9e9731bd96b068c09b23de76643f1a5c7fe9120e5858b65f70dd9bd8ead5d7f5d5ccb9b69f30665b669a20e227e5bffe3b"u8)).Item1);
internal static ref ж<ecdsa.PrivateKey> testECDSAPrivateKey => ref ᏑtestECDSAPrivateKey.ValueSlot;
internal static error _ᴛ14ʗ;

internal static ж<ecdsa.PrivateKey> testP256PrivateKey = Δx509.ParseECPrivateKey(fromHex("30770201010420012f3b52bc54c36ba3577ad45034e2e8efe1e6999851284cb848725cfe029991a00a06082a8648ce3d030107a14403420004c02c61c9b16283bbcc14956d886d79b358aa614596975f78cece787146abf74c2d5dc578c0992b4f3c631373479ebf3892efe53d21c4f4f1cc9a11c3536b7f75"u8)).Item1;
internal static error _ᴛ15ʗ;

internal static ed25519.PrivateKey testEd25519PrivateKey = ((ed25519.PrivateKey)fromHex("3a884965e76b3f55e5faf9615458a92354894234de3ec9f684d46d55cebf3dc63fe2152ee6e3ef3f4e854a7577a3649eede0bf842ccc92268ffa6f3483aaec8f"u8));

internal static readonly @string clientCertificatePEM = """

-----BEGIN CERTIFICATE-----
MIIB7zCCAVigAwIBAgIQXBnBiWWDVW/cC8m5k5/pvDANBgkqhkiG9w0BAQsFADAS
MRAwDgYDVQQKEwdBY21lIENvMB4XDTE2MDgxNzIxNTIzMVoXDTE3MDgxNzIxNTIz
MVowEjEQMA4GA1UEChMHQWNtZSBDbzCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkC
gYEAum+qhr3Pv5/y71yUYHhv6BPy0ZZvzdkybiI3zkH5yl0prOEn2mGi7oHLEMff
NFiVhuk9GeZcJ3NgyI14AvQdpJgJoxlwaTwlYmYqqyIjxXuFOE8uCXMyp70+m63K
hAfmDzr/d8WdQYUAirab7rCkPy1MTOZCPrtRyN1IVPQMjkcCAwEAAaNGMEQwDgYD
VR0PAQH/BAQDAgWgMBMGA1UdJQQMMAoGCCsGAQUFBwMBMAwGA1UdEwEB/wQCMAAw
DwYDVR0RBAgwBocEfwAAATANBgkqhkiG9w0BAQsFAAOBgQBGq0Si+yhU+Fpn+GKU
8ZqyGJ7ysd4dfm92lam6512oFmyc9wnTN+RLKzZ8Aa1B0jLYw9KT+RBrjpW5LBeK
o0RIvFkTgxYEiKSBXCUNmAysEbEoVr4dzWFihAm/1oDGRY2CLLTYg5vbySK3KhIR
e/oCO8HJ/+rJnahJ05XX1Q7lNQ==
-----END CERTIFICATE-----
"""u8;

internal static @string clientKeyPEM = testingKey("""

-----BEGIN RSA TESTING KEY-----
MIICXQIBAAKBgQC6b6qGvc+/n/LvXJRgeG/oE/LRlm/N2TJuIjfOQfnKXSms4Sfa
YaLugcsQx980WJWG6T0Z5lwnc2DIjXgC9B2kmAmjGXBpPCViZiqrIiPFe4U4Ty4J
czKnvT6brcqEB+YPOv93xZ1BhQCKtpvusKQ/LUxM5kI+u1HI3UhU9AyORwIDAQAB
AoGAEJZ03q4uuMb7b26WSQsOMeDsftdatT747LGgs3pNRkMJvTb/O7/qJjxoG+Mc
qeSj0TAZXp+PXXc3ikCECAc+R8rVMfWdmp903XgO/qYtmZGCorxAHEmR80SrfMXv
PJnznLQWc8U9nphQErR+tTESg7xWEzmFcPKwnZd1xg8ERYkCQQDTGtrFczlB2b/Z
9TjNMqUlMnTLIk/a/rPE2fLLmAYhK5sHnJdvDURaH2mF4nso0EGtENnTsh6LATnY
dkrxXGm9AkEA4hXHG2q3MnhgK1Z5hjv+Fnqd+8bcbII9WW4flFs15EKoMgS1w/PJ
zbsySaSy5IVS8XeShmT9+3lrleed4sy+UwJBAJOOAbxhfXP5r4+5R6ql66jES75w
jUCVJzJA5ORJrn8g64u2eGK28z/LFQbv9wXgCwfc72R468BdawFSLa/m2EECQGbZ
rWiFla26IVXV0xcD98VWJsTBZMlgPnSOqoMdM1kSEd4fUmlAYI/dFzV1XYSkOmVr
FhdZnklmpVDeu27P4c0CQQCuCOup0FlJSBpWY1TTfun/KMBkBatMz0VMA3d7FKIU
csPezl677Yjo8u1r/KzeI6zLg87Z8E6r6ZWNc9wBSZK6
-----END RSA TESTING KEY-----
"""u8);

internal static readonly @string clientECDSACertificatePEM = """

-----BEGIN CERTIFICATE-----
MIIB/DCCAV4CCQCaMIRsJjXZFzAJBgcqhkjOPQQBMEUxCzAJBgNVBAYTAkFVMRMw
EQYDVQQIEwpTb21lLVN0YXRlMSEwHwYDVQQKExhJbnRlcm5ldCBXaWRnaXRzIFB0
eSBMdGQwHhcNMTIxMTE0MTMyNTUzWhcNMjIxMTEyMTMyNTUzWjBBMQswCQYDVQQG
EwJBVTEMMAoGA1UECBMDTlNXMRAwDgYDVQQHEwdQeXJtb250MRIwEAYDVQQDEwlK
b2VsIFNpbmcwgZswEAYHKoZIzj0CAQYFK4EEACMDgYYABACVjJF1FMBexFe01MNv
ja5oHt1vzobhfm6ySD6B5U7ixohLZNz1MLvT/2XMW/TdtWo+PtAd3kfDdq0Z9kUs
jLzYHQFMH3CQRnZIi4+DzEpcj0B22uCJ7B0rxE4wdihBsmKo+1vx+U56jb0JuK7q
ixgnTy5w/hOWusPTQBbNZU6sER7m8TAJBgcqhkjOPQQBA4GMADCBiAJCAOAUxGBg
C3JosDJdYUoCdFzCgbkWqD8pyDbHgf9stlvZcPE4O1BIKJTLCRpS8V3ujfK58PDa
2RU6+b0DeoeiIzXsAkIBo9SKeDUcSpoj0gq+KxAxnZxfvuiRs9oa9V2jI/Umi0Vw
jWVim34BmT0Y9hCaOGGbLlfk+syxis7iI6CH8OFnUes=
-----END CERTIFICATE-----
"""u8;

internal static @string clientECDSAKeyPEM = testingKey("""

-----BEGIN EC PARAMETERS-----
BgUrgQQAIw==
-----END EC PARAMETERS-----
-----BEGIN EC TESTING KEY-----
MIHcAgEBBEIBkJN9X4IqZIguiEVKMqeBUP5xtRsEv4HJEtOpOGLELwO53SD78Ew8
k+wLWoqizS3NpQyMtrU8JFdWfj+C57UNkOugBwYFK4EEACOhgYkDgYYABACVjJF1
FMBexFe01MNvja5oHt1vzobhfm6ySD6B5U7ixohLZNz1MLvT/2XMW/TdtWo+PtAd
3kfDdq0Z9kUsjLzYHQFMH3CQRnZIi4+DzEpcj0B22uCJ7B0rxE4wdihBsmKo+1vx
+U56jb0JuK7qixgnTy5w/hOWusPTQBbNZU6sER7m8Q==
-----END EC TESTING KEY-----
"""u8);

internal static readonly @string clientEd25519CertificatePEM = """

-----BEGIN CERTIFICATE-----
MIIBLjCB4aADAgECAhAX0YGTviqMISAQJRXoNCNPMAUGAytlcDASMRAwDgYDVQQK
EwdBY21lIENvMB4XDTE5MDUxNjIxNTQyNloXDTIwMDUxNTIxNTQyNlowEjEQMA4G
A1UEChMHQWNtZSBDbzAqMAUGAytlcAMhAAvgtWC14nkwPb7jHuBQsQTIbcd4bGkv
xRStmmNveRKRo00wSzAOBgNVHQ8BAf8EBAMCBaAwEwYDVR0lBAwwCgYIKwYBBQUH
AwIwDAYDVR0TAQH/BAIwADAWBgNVHREEDzANggtleGFtcGxlLmNvbTAFBgMrZXAD
QQD8GRcqlKUx+inILn9boF2KTjRAOdazENwZ/qAicbP1j6FYDc308YUkv+Y9FN/f
7Q7hF9gRomDQijcjKsJGqjoI
-----END CERTIFICATE-----
"""u8;

internal static @string clientEd25519KeyPEM = testingKey("""

-----BEGIN TESTING KEY-----
MC4CAQAwBQYDK2VwBCIEINifzf07d9qx3d44e0FSbV4mC/xQxT644RRbpgNpin7I
-----END TESTING KEY-----
"""u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorFromˢ2 = (@string)"expected error from incomplete handshake, got nil"u8;

public static void TestServerHelloTrailingMessage(ж<testing.T> Ꮡt) {
    // In TLS 1.3 the change cipher spec message is optional. If a CCS message
    // is not sent, after reading the ServerHello, the read traffic secret is
    // set, and all following messages must be encrypted. If the server sends
    // additional unencrypted messages in a record with the ServerHello, the
    // client must either fail or ignore the additional messages.
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var sʗ1 = s;
    goǃ(() => {
        var ctx = context.Background();
        var srv = Server(sʗ1, testConfig);
        var (clientHello, _, err) = srv.readClientHello(ctx);
        if (err != default!) {
            testFatal(Ꮡt, err);
        }
        ref var hs = ref heap<global::go.crypto.tls_package.serverHandshakeStateTLS13>(out var Ꮡhs);
        hs = new serverHandshakeStateTLS13(
            c: srv,
            ctx: ctx,
            clientHello: clientHello
        );
        {
            var errΔ1 = Ꮡhs.processClientHello(); if (errΔ1 != default!) {
                testFatal(Ꮡt, errΔ1);
            }
        }
        {
            var errΔ2 = transcriptMsg(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(hs.clientHello), new tls_test_package.hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ2 != default!) {
                testFatal(Ꮡt, errΔ2);
            }
        }
        (var record, err) = concatHandshakeMessages(new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(hs.hello), new global::go.crypto.tls_package.encryptedExtensionsMsgжhandshakeMessage(Ꮡ(new encryptedExtensionsMsg(alpnProtocol: "h2"u8))));
        if (err != default!) {
            testFatal(Ꮡt, err);
        }
        {
            var (_, errΔ3) = sʗ1.Write(record); if (errΔ3 != default!) {
                testFatal(Ꮡt, errΔ3);
            }
        }
        srv.Close();
    });
    var cli = Client(c, testConfig);
    @string expectedErr = tlsHandshakeBufferNotˢ;
    {
        var err = cli.Handshake(); if (err == default!){
            Ꮡt.Fatal(expectedErrorFromˢ2);
        } else 
        if (err.Error() != expectedErr) {
            Ꮡt.Fatalf("expected error %q, got %q"u8, expectedErr, err.Error());
        }
    }
}

public static void TestClientHelloTrailingMessage(ж<testing.T> Ꮡt) {
    // Same as TestServerHelloTrailingMessage but for the client side.
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var cʗ1 = c;
    goǃ(() => {
        var cli = Client(cʗ1, testConfig);
        var (hello, _, _, err) = cli.makeClientHello();
        if (err != default!) {
            testFatal(Ꮡt, err);
        }
        (var record, err) = concatHandshakeMessages(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(hello), new global::go.crypto.tls_package.certificateMsgTLS13жhandshakeMessage(Ꮡ(new certificateMsgTLS13(nil))));
        if (err != default!) {
            testFatal(Ꮡt, err);
        }
        {
            var (_, errΔ1) = cʗ1.Write(record); if (errΔ1 != default!) {
                testFatal(Ꮡt, errΔ1);
            }
        }
        cli.Close();
    });
    var srv = Server(s, testConfig);
    @string expectedErr = tlsHandshakeBufferNotˢ;
    {
        var err = srv.Handshake(); if (err == default!){
            Ꮡt.Fatal(expectedErrorFromˢ2);
        } else 
        if (err.Error() != expectedErr) {
            Ꮡt.Fatalf("expected error %q, got %q"u8, expectedErr, err.Error());
        }
    }
}

public static void TestDoubleClientHelloHRR(ж<testing.T> Ꮡt) {
    // If a client sends two ClientHello messages in a single record, and the
    // server sends a HRR after reading the first ClientHello, the server must
    // either fail or ignore the trailing ClientHello.
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var cʗ1 = c;
    goǃ(() => {
        var cli = Client(cʗ1, testConfig);
        var (hello, _, _, err) = cli.makeClientHello();
        if (err != default!) {
            testFatal(Ꮡt, err);
        }
        hello.Value.keyShares = default!;
        (var record, err) = concatHandshakeMessages(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(hello), new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(hello));
        if (err != default!) {
            testFatal(Ꮡt, err);
        }
        {
            var (_, errΔ1) = cʗ1.Write(record); if (errΔ1 != default!) {
                testFatal(Ꮡt, errΔ1);
            }
        }
        cli.Close();
    });
    var srv = Server(s, testConfig);
    @string expectedErr = tlsHandshakeBufferNotˢ2;
    {
        var err = srv.Handshake(); if (err == default!){
            Ꮡt.Fatal(expectedErrorFromˢ2);
        } else 
        if (err.Error() != expectedErr) {
            Ꮡt.Fatalf("expected error %q, got %q"u8, expectedErr, err.Error());
        }
    }
}

// concatHandshakeMessages marshals and concatenates the given handshake
// messages into a single record.
internal static (slice<byte>, error) concatHandshakeMessages(params Span<global::go.crypto.tls_package.handshakeMessage> msgsʗp) {
    var msgs = msgsʗp.sslice();

    slice<byte> marshalled = default!;
    foreach (var (_, msg) in msgs) {
        var (data, err) = msg.marshal();
        if (err != default!) {
            return (default!, err);
        }
        marshalled = appendꓸꓸꓸ(marshalled, data);
    }
    nint m = len(marshalled);
    var outBuf = new slice<byte>(recordHeaderLen);
    outBuf[0] = (byte)recordTypeHandshake;
    nint vers = VersionTLS12;
    outBuf[1] = (byte)((vers >> (int)(8)));
    outBuf[2] = (byte)vers;
    outBuf[3] = (byte)((m >> (int)(8)));
    outBuf[4] = (byte)m;
    outBuf = appendꓸꓸꓸ(outBuf, marshalled);
    return (outBuf, default!);
}

} // end tls_internal_test_package
