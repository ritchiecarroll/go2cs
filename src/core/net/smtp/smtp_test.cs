// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using tls = crypto.tls_package;
using x509 = crypto.x509_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using net = net_package;
using textproto = go.net.textproto_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using go.net;
using static go.net.smtp_package;

partial class smtp_internal_test_package {

internal partial struct authTest {
    internal global::go.net.smtp_package.ΔAuth auth;
    internal slice<@string> challenges;
    internal @string name;
    internal slice<@string> responses;
}

internal static slice<authTest> authTests = new authTest[]{
    new(PlainAuth(""u8, "user"u8, "pass"u8, "testserver"u8), new @string[]{}.slice(), "PLAIN"u8, new @string[]{"\x00user\x00pass"u8}.slice()),
    new(PlainAuth("foo"u8, "bar"u8, "baz"u8, "testserver"u8), new @string[]{}.slice(), "PLAIN"u8, new @string[]{((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72, 0x00, 0x62, 0x61, 0x7a}))}.slice()),
    new(CRAMMD5Auth("user"u8, "pass"u8), new @string[]{"<123456.1322876914@testserver>"u8}.slice(), "CRAM-MD5"u8, new @string[]{""u8, "user 287eb355114cf5c471c26a875f1ca4ae"u8}.slice())
}.slice();

public static void TestAuth(ж<testing.T> Ꮡt) {
testLoop:
    foreach (var (i, test) in authTests) {
        var (name, resp, err) = test.auth.Start(Ꮡ(new ServerInfo("testserver"u8, true, default!)));
        if (name != test.name) {
            Ꮡt.Errorf("#%d got name %s, expected %s"u8, i, name, test.name);
        }
        if (!bytes.Equal(resp, slice<byte>(test.responses[0]))) {
            Ꮡt.Errorf("#%d got response %s, expected %s"u8, i, resp, test.responses[0]);
        }
        if (err != default!) {
            Ꮡt.Errorf("#%d error: %s"u8, i, err);
        }
        foreach (var (j, _) in test.challenges) {
            var challenge = slice<byte>(test.challenges[j]);
            var expected = slice<byte>(test.responses[j + 1]);
            var (respΔ1, errΔ1) = test.auth.Next(challenge, true);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("#%d error: %s"u8, i, errΔ1);
                goto continue_testLoop;
            }
            if (!bytes.Equal(respΔ1, expected)) {
                Ꮡt.Errorf("#%d got %s, expected %s"u8, i, respΔ1, expected);
                goto continue_testLoop;
            }
        }
continue_testLoop:;
    }
break_testLoop:;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string bazˢ = "baz"u8;

internal partial struct TestAuthPlain_tests /*dyn*/ {
    internal @string authName;
    internal ж<global::go.net.smtp_package.ServerInfo> server;
    internal @string err;
}

public static void TestAuthPlain(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestAuthPlain_tests[]{
        new(
            authName: "servername"u8,
            server: Ꮡ(new ServerInfo(Name: "servername"u8, TLS: true))
        ),
        new(
            authName: "localhost"u8, // OK to use PlainAuth on localhost without TLS

            server: Ꮡ(new ServerInfo(Name: "localhost"u8, TLS: false))
        ),
        new(
            authName: "servername"u8, // NOT OK on non-localhost, even if server says PLAIN is OK.
 // (We don't know that the server is the real server.)

            server: Ꮡ(new ServerInfo(Name: "servername"u8, Auth: new @string[]{"PLAIN"u8}.slice())),
            err: "unencrypted connection"u8
        ),
        new(
            authName: "servername"u8,
            server: Ꮡ(new ServerInfo(Name: "servername"u8, Auth: new @string[]{"CRAM-MD5"u8}.slice())),
            err: "unencrypted connection"u8
        ),
        new(
            authName: "servername"u8,
            server: Ꮡ(new ServerInfo(Name: "attacker"u8, TLS: true)),
            err: "wrong host name"u8
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        var auth = PlainAuth(fooˢ, barˢ, bazˢ, tt.authName);
        var (_, _, err) = auth.Start(tt.server);
        @string got = ""u8;
        if (err != default!) {
            got = err.Error();
        }
        if (got != tt.err) {
            Ꮡt.Errorf("%d. got error = %q; want %q"u8, i, got, tt.err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakeHostˢ = "fake.host"u8;
internal static readonly @string authFooauthQuitˢ = "AUTH FOOAUTH\r\n*\r\nQUIT\r\n"u8;

internal partial struct TestClientAuthTrimSpace_fake /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
    /*embed*/ public io_package.Writer Writer;
}

// Issue 17794: don't send a trailing space on AUTH command when there's no password.
public static void TestClientAuthTrimSpace(ж<testing.T> Ꮡt) {
    @string server = "220 hello world\r\n"u8 + "200 some more"u8;
    ref var wrote = ref heap(new strings.Builder(), out var Ꮡwrote);
    faker fake = new(nil);
    fake.ReadWriter = new TestClientAuthTrimSpace_fake(
        new smtp_test_package.strings_ReaderжReader(strings.NewReader(server)),
        new smtp_test_package.strings_BuilderжWriter(Ꮡwrote)
    );
    var (c, err) = NewClient(fake, fakeHostˢ);
    if (err != default!) {
        Ꮡt.Fatalf("NewClient: %v"u8, err);
    }
    c.Value.tls = true;
    c.Value.didHello = true;
    c.Auth(new toServerEmptyAuth(nil));
    c.Close();
    {
        @string got = wrote.String();
        @string want = authFooauthQuitˢ; if (got != want) {
            Ꮡt.Errorf("wrote %q; want %q"u8, got, want);
        }
    }
}

// toServerEmptyAuth is an implementation of Auth that only implements
// the Start method, and returns "FOOAUTH", nil, nil. Notably, it returns
// zero bytes for "toServer" so we can test that we don't send spaces at
// the end of the line. See TestClientAuthTrimSpace.
internal partial struct toServerEmptyAuth {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooauthˢ = "FOOAUTH"u8;

internal static (@string proto, slice<byte> toServer, error err) Start(this toServerEmptyAuth _, ж<global::go.net.smtp_package.ServerInfo> Ꮡserver) {
    return (fooauthˢ, default!, default!);
}

internal static (slice<byte> toServer, error err) Next(this toServerEmptyAuth _, slice<byte> fromServer, bool more) {
    throw panic("unexpected call");
}

internal partial struct faker {
    /*embed*/ public io_package.ReadWriter ReadWriter;
}

internal static error Close(this faker f) {
    return default!;
}

internal static netꓸAddr LocalAddr(this faker f) {
    return default!;
}

internal static netꓸAddr RemoteAddr(this faker f) {
    return default!;
}

internal static error SetDeadline(this faker f, time.Time _) {
    return default!;
}

internal static error SetReadDeadline(this faker f, time.Time _) {
    return default!;
}

internal static error SetWriteDeadline(this faker f, time.Time _) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aUtHˢ = "aUtH"u8;
internal static readonly @string dsnˢ = "DSN"u8;
internal static readonly @string userGmailComˢ = "user@gmail.com"u8;
internal static readonly @string user1GmailComˢ = "user1@gmail.com"u8;
internal static readonly @string user2GmailComDataAnotherˢ = "user2@gmail.com>\r\nDATA\r\nAnother injected message body\r\n.\r\nQUIT\r\n"u8;
internal static readonly @string user2GmailComˢ = "user2@gmail.com"u8;
internal static readonly @string smtpGoogleComˢ = "smtp.google.com"u8;
internal static readonly @string userˢ = "user"u8;
internal static readonly @string passˢ = "pass"u8;
internal static readonly @string golangNutsGooglegroupsˢ = "golang-nuts@googlegroups.com>\r\nDATA\r\nInjected message body\r\n.\r\nQUIT\r\n"u8;
internal static readonly @string userGmailComDataAnotherˢ = "user@gmail.com>\r\nDATA\r\nAnother injected message body\r\n.\r\nQUIT\r\n"u8;
internal static readonly @string golangNutsGooglegroupsˢ2 = "golang-nuts@googlegroups.com"u8;
internal static readonly @string fromUserGmailComToGolangˢ = """
From: user@gmail.com
To: golang-nuts@googlegroups.com
Subject: Hooray for Go

Line 1
.Leading dot line .
Goodbye.
"""u8;

public static void TestBasic(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string server = strings.Join(strings.Split(basicServer, "\n"u8), "\r\n"u8);
    @string client = strings.Join(strings.Split(basicClient, "\n"u8), "\r\n"u8);
    ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
    var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
    faker fake = new(nil);
    fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
    var c = Ꮡ(new Client(Text: textproto.NewConn(fake), localName: "localhost"u8));
    {
        var errΔ1 = c.helo(); if (errΔ1 != default!) {
            Ꮡt.Fatalf("HELO failed: %s"u8, errΔ1);
        }
    }
    {
        var errΔ2 = c.ehlo(); if (errΔ2 == default!) {
            Ꮡt.Fatalf("Expected first EHLO to fail"u8);
        }
    }
    {
        var errΔ3 = c.ehlo(); if (errΔ3 != default!) {
            Ꮡt.Fatalf("Second EHLO failed: %s"u8, errΔ3);
        }
    }
    c.Value.didHello = true;
    {
        var (ok, args) = c.Extension(aUtHˢ); if (!ok || args != "LOGIN PLAIN"u8) {
            Ꮡt.Fatalf("Expected AUTH supported"u8);
        }
    }
    {
        var (ok, _) = c.Extension(dsnˢ); if (ok) {
            Ꮡt.Fatalf("Shouldn't support DSN"u8);
        }
    }
    {
        var errΔ4 = c.Mail(userGmailComˢ); if (errΔ4 == default!) {
            Ꮡt.Fatalf("MAIL should require authentication"u8);
        }
    }
    {
        var errΔ5 = c.Verify(user1GmailComˢ); if (errΔ5 == default!) {
            Ꮡt.Fatalf("First VRFY: expected no verification"u8);
        }
    }
    {
        var errΔ6 = c.Verify(user2GmailComDataAnotherˢ); if (errΔ6 == default!) {
            Ꮡt.Fatalf("VRFY should have failed due to a message injection attempt"u8);
        }
    }
    {
        var errΔ7 = c.Verify(user2GmailComˢ); if (errΔ7 != default!) {
            Ꮡt.Fatalf("Second VRFY: expected verification, got %s"u8, errΔ7);
        }
    }
    // fake TLS so authentication won't complain
    c.Value.tls = true;
    c.Value.serverName = smtpGoogleComˢ;
    {
        var errΔ8 = c.Auth(PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ)); if (errΔ8 != default!) {
            Ꮡt.Fatalf("AUTH failed: %s"u8, errΔ8);
        }
    }
    {
        var errΔ9 = c.Rcpt(golangNutsGooglegroupsˢ); if (errΔ9 == default!) {
            Ꮡt.Fatalf("RCPT should have failed due to a message injection attempt"u8);
        }
    }
    {
        var errΔ10 = c.Mail(userGmailComDataAnotherˢ); if (errΔ10 == default!) {
            Ꮡt.Fatalf("MAIL should have failed due to a message injection attempt"u8);
        }
    }
    {
        var errΔ11 = c.Mail(userGmailComˢ); if (errΔ11 != default!) {
            Ꮡt.Fatalf("MAIL failed: %s"u8, errΔ11);
        }
    }
    {
        var errΔ12 = c.Rcpt(golangNutsGooglegroupsˢ2); if (errΔ12 != default!) {
            Ꮡt.Fatalf("RCPT failed: %s"u8, errΔ12);
        }
    }
    @string msg = fromUserGmailComToGolangˢ;
    var (w, err) = c.Data();
    if (err != default!) {
        Ꮡt.Fatalf("DATA failed: %s"u8, err);
    }
    {
        var (_, errΔ13) = w.Write(slice<byte>(msg)); if (errΔ13 != default!) {
            Ꮡt.Fatalf("Data write failed: %s"u8, errΔ13);
        }
    }
    {
        var errΔ14 = w.Close(); if (errΔ14 != default!) {
            Ꮡt.Fatalf("Bad data response: %s"u8, errΔ14);
        }
    }
    {
        var errΔ15 = c.Quit(); if (errΔ15 != default!) {
            Ꮡt.Fatalf("QUIT failed: %s"u8, errΔ15);
        }
    }
    bcmdbuf.Flush();
    @string actualcmds = cmdbuf.String();
    if (client != actualcmds) {
        Ꮡt.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
    }
}

internal static @string basicServer = """
250 mx.google.com at your service
502 Unrecognized command.
250-mx.google.com at your service
250-SIZE 35651584
250-AUTH LOGIN PLAIN
250 8BITMIME
530 Authentication required
252 Send some mail, I'll try my best
250 User is valid
235 Accepted
250 Sender OK
250 Receiver OK
354 Go ahead
250 Data OK
221 OK

"""u8;

internal static @string basicClient = """
HELO localhost
EHLO localhost
EHLO localhost
MAIL FROM:<user@gmail.com> BODY=8BITMIME
VRFY user1@gmail.com
VRFY user2@gmail.com
AUTH PLAIN AHVzZXIAcGFzcw==
MAIL FROM:<user@gmail.com> BODY=8BITMIME
RCPT TO:<golang-nuts@googlegroups.com>
DATA
From: user@gmail.com
To: golang-nuts@googlegroups.com
Subject: Hooray for Go

Line 1
..Leading dot line .
Goodbye.
.
QUIT

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eh502Eh221Okˢ = """
502 EH?
502 EH?
221 OK

"""u8;
internal static readonly @string ehloLocalhostHeloˢ = """
EHLO localhost
HELO localhost
QUIT

"""u8;
internal static readonly @string localhostˢ = "localhost"u8;
internal static readonly object expectedEhloToFailˢ = (@string)"expected EHLO to fail"u8;

public static void TestHELOFailed(ж<testing.T> Ꮡt) {
    @string serverLines = eh502Eh221Okˢ;
    @string clientLines = ehloLocalhostHeloˢ;
    @string server = strings.Join(strings.Split(serverLines, "\n"u8), "\r\n"u8);
    @string client = strings.Join(strings.Split(clientLines, "\n"u8), "\r\n"u8);
    ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
    var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
    faker fake = new(nil);
    fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
    var c = Ꮡ(new Client(Text: textproto.NewConn(fake), localName: "localhost"u8));
    {
        var err = c.Hello(localhostˢ); if (err == default!) {
            Ꮡt.Fatal(expectedEhloToFailˢ);
        }
    }
    {
        var err = c.Quit(); if (err != default!) {
            Ꮡt.Errorf("QUIT failed: %s"u8, err);
        }
    }
    bcmdbuf.Flush();
    @string actual = cmdbuf.String();
    if (client != actual) {
        Ꮡt.Errorf("Got:\n%s\nWant:\n%s"u8, actual, client);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heloˢ = "helo"u8;
internal static readonly @string ehloˢ = "ehlo"u8;
internal static readonly @string ehlo8bitmimeˢ = "ehlo 8bitmime"u8;
internal static readonly @string ehloSmtputf8ˢ = "ehlo smtputf8"u8;
internal static readonly @string userGmailComˢ2 = "user+📧@gmail.com"u8;
internal static readonly @string ehlo8bitmimeSmtputf8ˢ = "ehlo 8bitmime smtputf8"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicServerᶜ = """
250 mx.google.com at your service
250 Sender OK
221 Goodbye

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicClientᶜ = """
HELO localhost
MAIL FROM:<user@gmail.com>
QUIT

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicServerᶜ1 = """
250-mx.google.com at your service
250 SIZE 35651584
250 Sender OK
221 Goodbye

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicClientᶜ1 = """
EHLO localhost
MAIL FROM:<user@gmail.com>
QUIT

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicServerᶜ2 = """
250-mx.google.com at your service
250-SIZE 35651584
250 8BITMIME
250 Sender OK
221 Goodbye

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicClientᶜ2 = """
EHLO localhost
MAIL FROM:<user@gmail.com> BODY=8BITMIME
QUIT

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicServerᶜ3 = """
250-mx.google.com at your service
250-SIZE 35651584
250 SMTPUTF8
250 Sender OK
221 Goodbye

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicClientᶜ3 = """
EHLO localhost
MAIL FROM:<user+📧@gmail.com> SMTPUTF8
QUIT

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicServerᶜ4 = """
250-mx.google.com at your service
250-SIZE 35651584
250-8BITMIME
250 SMTPUTF8
250 Sender OK
221 Goodbye
	
"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string basicClientᶜ4 = """
EHLO localhost
MAIL FROM:<user+📧@gmail.com> BODY=8BITMIME SMTPUTF8
QUIT

"""u8;

public static void TestExtensions(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    (ж<global::go.net.smtp_package.Client> c, ж<bufio.Writer> bcmdbuf, ж<strings.Builder> cmdbuf) fake(@string server) {
        ж<global::go.net.smtp_package.Client> c = default!;
        ж<bufio.Writer> bcmdbuf = default!;
        ж<strings.Builder> cmdbuf = default!;
        server = strings.Join(strings.Split(server, "\n"u8), "\r\n"u8);
        cmdbuf = Ꮡ(new strings.Builder(nil));
        bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(cmdbuf));
        faker fakeΔ1 = new(nil);
        fakeΔ1.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        c = Ꮡ(new Client(Text: textproto.NewConn(fakeΔ1), localName: "localhost"u8));
        return (c, bcmdbuf, cmdbuf);
    }
    var fakeʗ1 = fake;
    Ꮡt.Run(heloˢ, (ж<testing.T> tΔ1) => {
        @string basicServer = basicServerᶜ;
        @string basicClient = basicClientᶜ;
        var (c, bcmdbuf, cmdbuf) = fakeʗ1(basicServer);
        {
            var err = c.helo(); if (err != default!) {
                tΔ1.Fatalf("HELO failed: %s"u8, err);
            }
        }
        c.Value.didHello = true;
        {
            var err = c.Mail(userGmailComˢ); if (err != default!) {
                tΔ1.Fatalf("MAIL FROM failed: %s"u8, err);
            }
        }
        {
            var err = c.Quit(); if (err != default!) {
                tΔ1.Fatalf("QUIT failed: %s"u8, err);
            }
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"u8), "\r\n"u8);
        if (client != actualcmds) {
            tΔ1.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    });
    var fakeʗ2 = fake;
    Ꮡt.Run(ehloˢ, (ж<testing.T> tΔ2) => {
        @string basicServer = basicServerᶜ1;
        @string basicClient = basicClientᶜ1;
        var (c, bcmdbuf, cmdbuf) = fakeʗ2(basicServer);
        {
            var err = c.Hello(localhostˢ); if (err != default!) {
                tΔ2.Fatalf("EHLO failed: %s"u8, err);
            }
        }
        {
            var (ok, _) = c.Extension("8BITMIME"u8); if (ok) {
                tΔ2.Fatalf("Shouldn't support 8BITMIME"u8);
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (ok) {
                tΔ2.Fatalf("Shouldn't support SMTPUTF8"u8);
            }
        }
        {
            var err = c.Mail(userGmailComˢ); if (err != default!) {
                tΔ2.Fatalf("MAIL FROM failed: %s"u8, err);
            }
        }
        {
            var err = c.Quit(); if (err != default!) {
                tΔ2.Fatalf("QUIT failed: %s"u8, err);
            }
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"u8), "\r\n"u8);
        if (client != actualcmds) {
            tΔ2.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    });
    var fakeʗ3 = fake;
    Ꮡt.Run(ehlo8bitmimeˢ, (ж<testing.T> tΔ3) => {
        @string basicServer = basicServerᶜ2;
        @string basicClient = basicClientᶜ2;
        var (c, bcmdbuf, cmdbuf) = fakeʗ3(basicServer);
        {
            var err = c.Hello(localhostˢ); if (err != default!) {
                tΔ3.Fatalf("EHLO failed: %s"u8, err);
            }
        }
        {
            var (ok, _) = c.Extension("8BITMIME"u8); if (!ok) {
                tΔ3.Fatalf("Should support 8BITMIME"u8);
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (ok) {
                tΔ3.Fatalf("Shouldn't support SMTPUTF8"u8);
            }
        }
        {
            var err = c.Mail(userGmailComˢ); if (err != default!) {
                tΔ3.Fatalf("MAIL FROM failed: %s"u8, err);
            }
        }
        {
            var err = c.Quit(); if (err != default!) {
                tΔ3.Fatalf("QUIT failed: %s"u8, err);
            }
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"u8), "\r\n"u8);
        if (client != actualcmds) {
            tΔ3.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    });
    var fakeʗ4 = fake;
    Ꮡt.Run(ehloSmtputf8ˢ, (ж<testing.T> tΔ4) => {
        @string basicServer = basicServerᶜ3;
        @string basicClient = basicClientᶜ3;
        var (c, bcmdbuf, cmdbuf) = fakeʗ4(basicServer);
        {
            var err = c.Hello(localhostˢ); if (err != default!) {
                tΔ4.Fatalf("EHLO failed: %s"u8, err);
            }
        }
        {
            var (ok, _) = c.Extension("8BITMIME"u8); if (ok) {
                tΔ4.Fatalf("Shouldn't support 8BITMIME"u8);
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (!ok) {
                tΔ4.Fatalf("Should support SMTPUTF8"u8);
            }
        }
        {
            var err = c.Mail(userGmailComˢ2); if (err != default!) {
                tΔ4.Fatalf("MAIL FROM failed: %s"u8, err);
            }
        }
        {
            var err = c.Quit(); if (err != default!) {
                tΔ4.Fatalf("QUIT failed: %s"u8, err);
            }
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"u8), "\r\n"u8);
        if (client != actualcmds) {
            tΔ4.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    });
    var fakeʗ5 = fake;
    Ꮡt.Run(ehlo8bitmimeSmtputf8ˢ, (ж<testing.T> tΔ5) => {
        @string basicServer = basicServerᶜ4;
        @string basicClient = basicClientᶜ4;
        var (c, bcmdbuf, cmdbuf) = fakeʗ5(basicServer);
        {
            var err = c.Hello(localhostˢ); if (err != default!) {
                tΔ5.Fatalf("EHLO failed: %s"u8, err);
            }
        }
        c.Value.didHello = true;
        {
            var (ok, _) = c.Extension("8BITMIME"u8); if (!ok) {
                tΔ5.Fatalf("Should support 8BITMIME"u8);
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (!ok) {
                tΔ5.Fatalf("Should support SMTPUTF8"u8);
            }
        }
        {
            var err = c.Mail(userGmailComˢ2); if (err != default!) {
                tΔ5.Fatalf("MAIL FROM failed: %s"u8, err);
            }
        }
        {
            var err = c.Quit(); if (err != default!) {
                tΔ5.Fatalf("QUIT failed: %s"u8, err);
            }
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"u8), "\r\n"u8);
        if (client != actualcmds) {
            tΔ5.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    });
}

public static void TestNewClient(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string server = strings.Join(strings.Split(newClientServer, "\n"u8), "\r\n"u8);
        @string client = strings.Join(strings.Split(newClientClient, "\n"u8), "\r\n"u8);
        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        var bcmdbufʗ1 = bcmdbuf;
        @string @out() {
            bcmdbufʗ1.Flush();
            return Ꮡcmdbuf.Value.String();
        }
        faker fake = new(nil);
        fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        var (c, err) = NewClient(fake, fakeHostˢ);
        if (err != default!) {
            Ꮡt.Fatalf("NewClient: %v\n(after %v)"u8, err, @out());
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            var (ok, args) = c.Extension(aUtHˢ); if (!ok || args != "LOGIN PLAIN"u8) {
                Ꮡt.Fatalf("Expected AUTH supported"u8);
            }
        }
        {
            var (ok, _) = c.Extension(dsnˢ); if (ok) {
                Ꮡt.Fatalf("Shouldn't support DSN"u8);
            }
        }
        {
            var errΔ1 = c.Quit(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("QUIT failed: %s"u8, errΔ1);
            }
        }
        @string actualcmds = @out();
        if (client != actualcmds) {
            Ꮡt.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string newClientServer = """
220 hello world
250-mx.google.com at your service
250-SIZE 35651584
250-AUTH LOGIN PLAIN
250 8BITMIME
221 OK

"""u8;

internal static @string newClientClient = """
EHLO localhost
QUIT

"""u8;

public static void TestNewClient2(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string server = strings.Join(strings.Split(newClient2Server, "\n"u8), "\r\n"u8);
        @string client = strings.Join(strings.Split(newClient2Client, "\n"u8), "\r\n"u8);
        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        faker fake = new(nil);
        fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        var (c, err) = NewClient(fake, fakeHostˢ);
        if (err != default!) {
            Ꮡt.Fatalf("NewClient: %v"u8, err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            var (ok, _) = c.Extension(dsnˢ); if (ok) {
                Ꮡt.Fatalf("Shouldn't support DSN"u8);
            }
        }
        {
            var errΔ1 = c.Quit(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("QUIT failed: %s"u8, errΔ1);
            }
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        if (client != actualcmds) {
            Ꮡt.Fatalf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string newClient2Server = """
220 hello world
502 EH?
250-mx.google.com at your service
250-SIZE 35651584
250-AUTH LOGIN PLAIN
250 8BITMIME
221 OK

"""u8;

internal static @string newClient2Client = """
EHLO localhost
HELO localhost
QUIT

"""u8;

public static partial void TestNewClientWithTLS(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var (cert, err) = tls.X509KeyPair(localhostCert, localhostKey);
        if (err != default!) {
            Ꮡt.Fatalf("loadcert: %v"u8, err);
        }
        ref var config = ref heap<tls.Config>(out var Ꮡconfig);
        config = new tls.Config(Certificates: new tls.Certificate[]{cert}.slice());
        (var ln, err) = tls.Listen(tcpˢ, "127.0.0.1:0"u8, Ꮡconfig);
        if (err != default!) {
            (ln, err) = tls.Listen(tcpˢ, "[::1]:0"u8, Ꮡconfig);
            if (err != default!) {
                Ꮡt.Fatalf("server: listen: %v"u8, err);
            }
        }
        var lnʗ1 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (connΔ1, errΔ1) = lnʗ1.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("server: accept: %v"u8, errΔ1);
                    return;
                }
                var connʗ1 = connΔ1;
                defer(() => connʗ1.Close(), ref ᒐ);
                (_, errΔ1) = connΔ1.Write(slice<byte>("220 SIGNS\r\n"u8));
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("server: write: %v"u8, errΔ1);
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        config.InsecureSkipVerify = true;
        (var conn, err) = tls.Dial(tcpˢ, ln.Addr().String(), Ꮡconfig);
        if (err != default!) {
            Ꮡt.Fatalf("client: dial: %v"u8, err);
        }
        var connʗ2 = conn;
        defer(() => connʗ2.Close(), ref ᒐ);
        (var client, err) = NewClient(new tls.ConnжConn(conn), ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("smtp: newclient: %v"u8, err);
        }
        if (!(~client).tls) {
            Ꮡt.Errorf("client.tls Got: %t Expected: %t"u8, (~client).tls, true);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string customhostˢ = "customhost"u8;
internal static readonly @string hostinjectionDataˢ = "hostinjection>\n\rDATA\r\nInjected message body\r\n.\r\nQUIT\r\n"u8;
internal static readonly @string testExampleComˢ = "test@example.com"u8;
internal static readonly @string featureˢ = "feature"u8;

public static void TestHello(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (len(helloServer) != len(helloClient)) {
            Ꮡt.Fatalf("Hello server and client size mismatch"u8);
        }
        for (nint i = 0; i < len(helloServer); i++) {
            @string server = strings.Join(strings.Split(baseHelloServer + helloServer[i], "\n"u8), "\r\n"u8);
            @string client = strings.Join(strings.Split(baseHelloClient + helloClient[i], "\n"u8), "\r\n"u8);
            ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
            var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
            faker fake = new(nil);
            fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
            var (c, err) = NewClient(fake, fakeHostˢ);
            if (err != default!) {
                Ꮡt.Fatalf("NewClient: %v"u8, err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            c.Value.localName = customhostˢ;
            err = default!;
            switch (i) {
            case 0: {
                err = c.Hello(hostinjectionDataˢ);
                if (err == default!) {
                    Ꮡt.Errorf("Expected Hello to be rejected due to a message injection attempt"u8);
                }
                err = c.Hello(customhostˢ);
                break;
            }
            case 1: {
                err = c.StartTLS(nil);
                if (err.Error() == "502 Not implemented"u8) {
                    err = default!;
                }
                break;
            }
            case 2: {
                err = c.Verify(testExampleComˢ);
                break;
            }
            case 3: {
                c.Value.tls = true;
                c.Value.serverName = smtpGoogleComˢ;
                err = c.Auth(PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ));
                break;
            }
            case 4: {
                err = c.Mail(testExampleComˢ);
                break;
            }
            case 5: {
                var (ok, _) = c.Extension(featureˢ);
                if (ok) {
                    Ꮡt.Errorf("Expected FEATURE not to be supported"u8);
                }
                break;
            }
            case 6: {
                err = c.Reset();
                break;
            }
            case 7: {
                err = c.Quit();
                break;
            }
            case 8: {
                err = c.Verify(testExampleComˢ);
                if (err != default!) {
                    err = c.Hello(customhostˢ);
                    if (err != default!) {
                        Ꮡt.Errorf("Want error, got none"u8);
                    }
                }
                break;
            }
            case 9: {
                err = c.Noop();
                break;
            }
            default: {
                Ꮡt.Fatalf("Unhandled command"u8);
                break;
            }}

            if (err != default!) {
                Ꮡt.Errorf("Command %d failed: %v"u8, i, err);
            }
            bcmdbuf.Flush();
            @string actualcmds = cmdbuf.String();
            if (client != actualcmds) {
                Ꮡt.Errorf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string baseHelloServer = """
220 hello world
502 EH?
250-mx.google.com at your service
250 FEATURE

"""u8;

internal static slice<@string> helloServer = new @string[]{
    ""u8,
    "502 Not implemented\n"u8,
    "250 User is valid\n"u8,
    "235 Accepted\n"u8,
    "250 Sender ok\n"u8,
    ""u8,
    "250 Reset ok\n"u8,
    "221 Goodbye\n"u8,
    "250 Sender ok\n"u8,
    "250 ok\n"u8
}.slice();

internal static @string baseHelloClient = """
EHLO customhost
HELO customhost

"""u8;

internal static slice<@string> helloClient = new @string[]{
    ""u8,
    "STARTTLS\n"u8,
    "VRFY test@example.com\n"u8,
    "AUTH PLAIN AHVzZXIAcGFzcw==\n"u8,
    "MAIL FROM:<test@example.com>\n"u8,
    ""u8,
    "RSET\n"u8,
    "QUIT\n"u8,
    "VRFY test@example.com\n"u8,
    "NOOP\n"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fromTestExampleComToˢ = """
From: test@example.com
To: other@example.com
Subject: SendMail test

SendMail is working for me.

"""u8;

public static partial void TestSendMail(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string server = strings.Join(strings.Split(sendMailServer, "\n"u8), "\r\n"u8);
        @string client = strings.Join(strings.Split(sendMailClient, "\n"u8), "\r\n"u8);
        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        var (l, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatalf("Unable to create listener: %v"u8, err);
        }
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        // prevent data race on bcmdbuf
        channel<EmptyStruct> done = new channel<EmptyStruct>(0);
        var bcmdbufʗ1 = bcmdbuf;
        var doneʗ1 = done;
        var lʗ2 = l;
        goǃ((slice<@string> data) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);
                var (conn, errΔ1) = lʗ2.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("Accept error: %v"u8, errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                var tc = textproto.NewConn(new smtp_test_package.net_ConnᴠReadWriteCloser(conn));
                for (nint i = 0; i < len(data) && data[i] != ""; i++) {
                    tc.of(textproto.Conn.ᏑWriter).PrintfLine("%s"u8, data[i]);
                    while (len(data[i]) >= 4 && data[i][3] == (rune)'-') {
                        i++;
                        tc.of(textproto.Conn.ᏑWriter).PrintfLine("%s"u8, data[i]);
                    }
                    if (data[i] == "221 Goodbye") {
                        return;
                    }
                    var read = false;
                    while (!read || data[i] == "354 Go ahead") {
                        var (msg, errΔ2) = tc.of(textproto.Conn.ᏑReader).ReadLine();
                        bcmdbufʗ1.Write(slice<byte>(msg + "\r\n"));
                        read = true;
                        if (errΔ2 != default!) {
                            Ꮡt.Errorf("Read error: %v"u8, errΔ2);
                            return;
                        }
                        if (data[i] == "354 Go ahead" && msg == "."u8) {
                            break;
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, strings.Split(server, "\r\n"u8));
        err = SendMail(l.Addr().String(), default!, testExampleComˢ, new @string[]{"other@example.com>\n\rDATA\r\nInjected message body\r\n.\r\nQUIT\r\n"u8}.slice(), slice<byte>(strings.Replace(fromTestExampleComToˢ, "\n"u8, "\r\n"u8, -1)));
        if (err == default!) {
            Ꮡt.Errorf("Expected SendMail to be rejected due to a message injection attempt"u8);
        }
        err = SendMail(l.Addr().String(), default!, testExampleComˢ, new @string[]{"other@example.com"u8}.slice(), slice<byte>(strings.Replace(fromTestExampleComToˢ, "\n"u8, "\r\n"u8, -1)));
        if (err != default!) {
            Ꮡt.Errorf("%v"u8, err);
        }
        ᐸꟷ(done);
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        if (client != actualcmds) {
            Ꮡt.Errorf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string sendMailServer = """
220 hello world
502 EH?
250 mx.google.com at your service
250 Sender ok
250 Receiver ok
354 Go ahead
250 Data ok
221 Goodbye

"""u8;

internal static @string sendMailClient = """
EHLO localhost
HELO localhost
MAIL FROM:<test@example.com>
RCPT TO:<other@example.com>
DATA
From: test@example.com
To: other@example.com
Subject: SendMail test

SendMail is working for me.
.
QUIT

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ = "220 hello world"u8;
internal static readonly @string mxGoogleComAtYourServiceˢ = "250 mx.google.com at your service"u8;
internal static readonly object sendMailServerDoesnTˢ = (@string)"SendMail: Server doesn't support AUTH, expected to get an error, but got none "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantMsgᶜ = "EHLO localhost"u8;

public static partial void TestSendMailWithAuth(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var (l, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatalf("Unable to create listener: %v"u8, err);
        }
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        var errCh = new channel<error>(0);
        var errChʗ1 = errCh;
        var lʗ2 = l;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), errChʗ1, ref ᒐ);
                var (conn, errΔ1) = lʗ2.Accept();
                if (errΔ1 != default!) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("Accept: %v"u8, errΔ1));
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                var tc = textproto.NewConn(new smtp_test_package.net_ConnᴠReadWriteCloser(conn));
                tc.of(textproto.Conn.ᏑWriter).PrintfLine(helloWorldˢ);
                (var msg, errΔ1) = tc.of(textproto.Conn.ᏑReader).ReadLine();
                if (errΔ1 != default!) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("ReadLine error: %v"u8, errΔ1));
                    return;
                }
                @string wantMsg = wantMsgᶜ;
                if (msg != wantMsg) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("unexpected response %q; want %q"u8, msg, wantMsg));
                    return;
                }
                errΔ1 = tc.of(textproto.Conn.ᏑWriter).PrintfLine(mxGoogleComAtYourServiceˢ);
                if (errΔ1 != default!) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("PrintfLine: %v"u8, errΔ1));
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        err = SendMail(l.Addr().String(), PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ), testExampleComˢ, new @string[]{"other@example.com"u8}.slice(), slice<byte>(strings.Replace(fromTestExampleComToˢ, "\n"u8, "\r\n"u8, -1)));
        if (err == default!) {
            Ꮡt.Error(sendMailServerDoesnTˢ);
        }
        if (err.Error() != "smtp: server doesn't support AUTH"u8) {
            Ꮡt.Errorf("Expected: smtp: server doesn't support AUTH, got: %s"u8, err);
        }
        err = ᐸꟷ(errCh);
        if (err != default!) {
            Ꮡt.Fatalf("server error: %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object authExpectedErrorGotNoneˢ = (@string)"Auth: expected error; got none"u8;
internal static readonly object invalidCredentialsPleaseˢ = (@string)"535 Invalid credentials\nplease see www.example.com"u8;

public static void TestAuthFailed(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string server = strings.Join(strings.Split(authFailedServer, "\n"u8), "\r\n"u8);
        @string client = strings.Join(strings.Split(authFailedClient, "\n"u8), "\r\n"u8);
        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        faker fake = new(nil);
        fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        var (c, err) = NewClient(fake, fakeHostˢ);
        if (err != default!) {
            Ꮡt.Fatalf("NewClient: %v"u8, err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        c.Value.tls = true;
        c.Value.serverName = smtpGoogleComˢ;
        err = c.Auth(PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ));
        if (err == default!){
            Ꮡt.Error(authExpectedErrorGotNoneˢ);
        } else 
        if (err.Error() != "535 Invalid credentials\nplease see www.example.com"u8) {
            Ꮡt.Errorf("Auth: got error: %v, want: %s"u8, err, invalidCredentialsPleaseˢ);
        }
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        if (client != actualcmds) {
            Ꮡt.Errorf("Got:\n%s\nExpected:\n%s"u8, actualcmds, client);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string authFailedServer = """
220 hello world
250-mx.google.com at your service
250 AUTH LOGIN PLAIN
535-Invalid credentials
535 please see www.example.com
221 Goodbye

"""u8;

internal static @string authFailedClient = """
EHLO localhost
AUTH PLAIN AHVzZXIAcGFzcw==
*
QUIT

"""u8;

public static partial void TestTLSClient(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "freebsd"u8 || runtime.GOOS == "js"u8 || runtime.GOOS == "wasip1"u8) {
            testenv.SkipFlaky(new smtp_test_package.testing_TжTB(Ꮡt), 19229);
        }
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(0);
        var errcʗ1 = errc;
        var lnʗ2 = ln;
        goǃ(() => {
            errcʗ1.ᐸꟷ(sendMail(lnʗ2.Addr().String()));
        });
        var (conn, err) = ln.Accept();
        if (err != default!) {
            Ꮡt.Fatalf("failed to accept connection: %v"u8, err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = serverHandle(conn, Ꮡt); if (errΔ1 != default!) {
                Ꮡt.Fatalf("failed to handle connection: %v"u8, errΔ1);
            }
        }
        {
            var errΔ2 = ᐸꟷ(errc); if (errΔ2 != default!) {
                Ꮡt.Fatalf("client error: %v"u8, errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTLSConnState(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var clientDone = new channel<bool>(0);
        var serverDone = new channel<bool>(0);
        var lnʗ2 = ln;
        var serverDoneʗ1 = serverDone;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), serverDoneʗ1, ref ᒐ);
                var (c, err) = lnʗ2.Accept();
                if (err != default!) {
                    Ꮡt.Errorf("Server accept: %v"u8, err);
                    return;
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                {
                    var errΔ1 = serverHandle(c, Ꮡt); if (errΔ1 != default!) {
                        Ꮡt.Errorf("server error: %v"u8, errΔ1);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var clientDoneʗ1 = clientDone;
        var lnʗ3 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), clientDoneʗ1, ref ᒐ);
                var (c, err) = Dial(lnʗ3.Addr().String());
                if (err != default!) {
                    Ꮡt.Errorf("Client dial: %v"u8, err);
                    return;
                }
                var cʗ2 = c;
                defer(() => cʗ2.Quit(), ref ᒐ);
                var cfg = Ꮡ(new tls.Config(ServerName: "example.com"u8));
                testHookStartTLS(cfg); // set the RootCAs
                {
                    var errΔ1 = c.StartTLS(cfg); if (errΔ1 != default!) {
                        Ꮡt.Errorf("StartTLS: %v"u8, errΔ1);
                        return;
                    }
                }
                ref var cs = ref heap<tlsꓸConnectionState>(out var Ꮡcs);
                (cs, var ok) = c.TLSConnectionState();
                if (!ok) {
                    Ꮡt.Errorf("TLSConnectionState returned ok == false; want true"u8);
                    return;
                }
                if (cs.Version == 0 || !cs.HandshakeComplete) {
                    Ꮡt.Errorf("ConnectionState = %#v; expect non-zero Version and HandshakeComplete"u8, cs);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        ᐸꟷ(clientDone);
        ᐸꟷ(serverDone);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcp6ˢ = "tcp6"u8;

internal static net.Listener newLocalListener(ж<testing.T> Ꮡt) {
    var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
    if (err != default!) {
        (ln, err) = net.Listen(tcp6ˢ, "[::1]:0"u8);
    }
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return ln;
}

internal partial struct smtpSender {
    internal io.Writer w;
}

internal static void send(this smtpSender s, @string f) {
    s.w.Write(slice<byte>(f + "\r\n"));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string esmtpServiceReadyˢ = "220 127.0.0.1 ESMTP service ready"u8;
internal static readonly @string esmtpOffersAWarmHugOfˢ = "250-127.0.0.1 ESMTP offers a warm hug of welcome"u8;
internal static readonly @string starttlsˢ2 = "250-STARTTLS"u8;
internal static readonly @string goAheadˢ = "220 Go ahead"u8;

// smtp server, finely tailored to deal with our own client only!
internal static error serverHandle(net.Conn c, ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var recvʗ1 = new smtpSender(c);
        var send = (@string p1) => recvʗ1.send(p1);
        send(esmtpServiceReadyˢ);
        var s = bufio.NewScanner(new smtp_test_package.net_ConnᴠReader(c));
        while (s.Scan()) {
            var exprᴛ1 = s.Text();
            if (exprᴛ1 == "EHLO localhost"u8) {
                send(esmtpOffersAWarmHugOfˢ);
                send(starttlsˢ2);
                send("250 Ok"u8);
            }
            else if (exprᴛ1 == "STARTTLS"u8) {
                send(goAheadˢ);
                var (keypair, err) = tls.X509KeyPair(localhostCert, localhostKey);
                if (err != default!) {
                    return err;
                }
                var config = Ꮡ(new tls.Config(Certificates: new tls.Certificate[]{keypair}.slice()));
                c = new tls.ConnжConn(tls.Server(c, config));
                defer(() => c.Close(), ref ᒐ);
                return serverHandleTLS(c, Ꮡt);
            }
            else { /* default: */
                Ꮡt.Fatalf("unrecognized command: %q"u8, s.Text());
            }

        }
        return s.Err();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sendTheMailDataEndWithˢ = "354 send the mail data, end with ."u8;
internal static readonly @string serviceClosingˢ = "221 127.0.0.1 Service closing transmission channel"u8;

internal static error serverHandleTLS(net.Conn c, ж<testing.T> Ꮡt) {
    var recvʗ1 = new smtpSender(c);
    var send = (@string p1) => recvʗ1.send(p1);
    var s = bufio.NewScanner(new smtp_test_package.net_ConnᴠReader(c));
    while (s.Scan()) {
        var exprᴛ1 = s.Text();
        if (exprᴛ1 == "EHLO localhost"u8) {
            send("250 Ok"u8);
        }
        else if (exprᴛ1 == "MAIL FROM:<joe1@example.com>"u8) {
            send("250 Ok"u8);
        }
        else if (exprᴛ1 == "RCPT TO:<joe2@example.com>"u8) {
            send("250 Ok"u8);
        }
        else if (exprᴛ1 == "DATA"u8) {
            send(sendTheMailDataEndWithˢ);
            send("250 Ok"u8);
        }
        else if (exprᴛ1 == "Subject: test"u8) {
        }
        else if (exprᴛ1 == ""u8) {
        }
        else if (exprᴛ1 == "howdy!"u8) {
        }
        else if (exprᴛ1 == "."u8) {
        }
        else if (exprᴛ1 == "QUIT"u8) {
            send(serviceClosingˢ);
            return default!;
        }
        else { /* default: */
            Ꮡt.Fatalf("unrecognized command during TLS: %q"u8, s.Text());
        }

    }
    return s.Err();
}

[GoInit] internal static void init() {
    var testRootCAs = x509.NewCertPool();
    testRootCAs.AppendCertsFromPEM(localhostCert);
    var testRootCAsʗ1 = testRootCAs;
    testHookStartTLS = (ж<tls.Config> config) => {
        config.Value.RootCAs = testRootCAsʗ1;
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string joe1ExampleComˢ = "joe1@example.com"u8;

internal static error sendMail(@string hostPort) {
    @string from = joe1ExampleComˢ;
    var to = new @string[]{"joe2@example.com"u8}.slice();
    return SendMail(hostPort, default!, from, to, slice<byte>("Subject: test\n\nhowdy!"u8));
}

// localhostCert is a PEM-encoded TLS cert generated from src/crypto/tls:
//
//	go run generate_cert.go --rsa-bits 1024 --host 127.0.0.1,::1,example.com \
//		--ca --start-date "Jan 1 00:00:00 1970" --duration=1000000h
internal static slice<byte> localhostCert = slice<byte>("""

-----BEGIN CERTIFICATE-----
MIICFDCCAX2gAwIBAgIRAK0xjnaPuNDSreeXb+z+0u4wDQYJKoZIhvcNAQELBQAw
EjEQMA4GA1UEChMHQWNtZSBDbzAgFw03MDAxMDEwMDAwMDBaGA8yMDg0MDEyOTE2
MDAwMFowEjEQMA4GA1UEChMHQWNtZSBDbzCBnzANBgkqhkiG9w0BAQEFAAOBjQAw
gYkCgYEA0nFbQQuOWsjbGtejcpWz153OlziZM4bVjJ9jYruNw5n2Ry6uYQAffhqa
JOInCmmcVe2siJglsyH9aRh6vKiobBbIUXXUU1ABd56ebAzlt0LobLlx7pZEMy30
LqIi9E6zmL3YvdGzpYlkFRnRrqwEtWYbGBf3znO250S56CCWH2UCAwEAAaNoMGYw
DgYDVR0PAQH/BAQDAgKkMBMGA1UdJQQMMAoGCCsGAQUFBwMBMA8GA1UdEwEB/wQF
MAMBAf8wLgYDVR0RBCcwJYILZXhhbXBsZS5jb22HBH8AAAGHEAAAAAAAAAAAAAAA
AAAAAAEwDQYJKoZIhvcNAQELBQADgYEAbZtDS2dVuBYvb+MnolWnCNqvw1w5Gtgi
NmvQQPOMgM3m+oQSCPRTNGSg25e1Qbo7bgQDv8ZTnq8FgOJ/rbkyERw2JckkHpD4
n4qcK27WkEDBtQFlPihIM8hLIuzWoi/9wygiElTy/tVL3y7fGCvY2/k1KBthtZGF
tN8URjVmyEo=
-----END CERTIFICATE-----
"""u8);

// localhostKey is the private key for localhostCert.
internal static slice<byte> localhostKey = slice<byte>(testingKey("""

-----BEGIN RSA TESTING KEY-----
MIICXgIBAAKBgQDScVtBC45ayNsa16NylbPXnc6XOJkzhtWMn2Niu43DmfZHLq5h
AB9+Gpok4icKaZxV7ayImCWzIf1pGHq8qKhsFshRddRTUAF3np5sDOW3QuhsuXHu
lkQzLfQuoiL0TrOYvdi90bOliWQVGdGurAS1ZhsYF/fOc7bnRLnoIJYfZQIDAQAB
AoGBAMst7OgpKyFV6c3JwyI/jWqxDySL3caU+RuTTBaodKAUx2ZEmNJIlx9eudLA
kucHvoxsM/eRxlxkhdFxdBcwU6J+zqooTnhu/FE3jhrT1lPrbhfGhyKnUrB0KKMM
VY3IQZyiehpxaeXAwoAou6TbWoTpl9t8ImAqAMY8hlULCUqlAkEA+9+Ry5FSYK/m
542LujIcCaIGoG1/Te6Sxr3hsPagKC2rH20rDLqXwEedSFOpSS0vpzlPAzy/6Rbb
PHTJUhNdwwJBANXkA+TkMdbJI5do9/mn//U0LfrCR9NkcoYohxfKz8JuhgRQxzF2
6jpo3q7CdTuuRixLWVfeJzcrAyNrVcBq87cCQFkTCtOMNC7fZnCTPUv+9q1tcJyB
vNjJu3yvoEZeIeuzouX9TJE21/33FaeDdsXbRhQEj23cqR38qFHsF1qAYNMCQQDP
QXLEiJoClkR2orAmqjPLVhR3t2oB3INcnEjLNSq8LHyQEfXyaFfu4U9l5+fRPL2i
jiC0k/9L5dHUsF0XZothAkEA23ddgRs+Id/HxtojqqUT27B8MT/IGNrYsp4DvS/c
qgkeluku4GjxRlDMBuXk94xOBEinUs+p/hwP1Alll80Tpg==
-----END RSA TESTING KEY-----
"""u8));

internal static @string testingKey(@string s) {
    return strings.ReplaceAll(s, "TESTING KEY"u8, "PRIVATE KEY"u8);
}

} // end smtp_internal_test_package
