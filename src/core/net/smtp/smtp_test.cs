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

internal static slice<authTest> authTests = slice<authTest>([
    new(PlainAuth("", "user", "pass", "testserver"), [], "PLAIN", ["\x00user\x00pass"]),
    new(PlainAuth("foo", "bar", "baz", "testserver"), [], "PLAIN", [((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72, 0x00, 0x62, 0x61, 0x7a}))]),
    new(CRAMMD5Auth("user", "pass"), ["<123456.1322876914@testserver>"], "CRAM-MD5", ["", "user 287eb355114cf5c471c26a875f1ca4ae"])
]);

public static void TestAuth(ж<testing.T> t) {
testLoop:
    foreach (var (i, test) in range(authTests)) {
        var (name, resp, err) = test.auth.Start(Ꮡ(new ServerInfo("testserver", true, default)));
        if (name != test.name) {
            t.Errorf("#%d got name %s, expected %s", i, name, test.name);
        }
        if (!bytes.Equal(resp, slice<byte>(test.responses[0]))) {
            t.Errorf("#%d got response %s, expected %s", i, resp, test.responses[0]);
        }
        if (err != nil) {
            t.Errorf("#%d error: %s", i, err);
        }
        foreach (var j in range(len(test.challenges))) {
            var challenge = slice<byte>(test.challenges[j]);
            var expected = slice<byte>(test.responses[j + 1]);
            var (respΔ1, errΔ1) = test.auth.Next(challenge, true);
            if (errΔ1 != nil) {
                t.Errorf("#%d error: %s", i, errΔ1);
                goto continue_testLoop;
            }
            if (!bytes.Equal(respΔ1, expected)) {
                t.Errorf("#%d got %s, expected %s", i, respΔ1, expected);
                goto continue_testLoop;
            }
        }
continue_testLoop:;
    }
break_testLoop:;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;

internal partial struct TestAuthPlain_tests /*dyn*/ {
    internal @string authName;
    internal ж<global::go.net.smtp_package.ServerInfo> server;
    internal @string err;
}

public static void TestAuthPlain(ж<testing.T> t) {
    var tests = slice<TestAuthPlain_tests>([
        new(
            authName: "servername",
            server: Ꮡ(new ServerInfo(Name: "servername", TLS: true))
        ),
        new(
            // OK to use PlainAuth on localhost without TLS
            authName: "localhost",
            server: Ꮡ(new ServerInfo(Name: "localhost", TLS: false))
        ),
        new(
            // NOT OK on non-localhost, even if server says PLAIN is OK.
            // (We don't know that the server is the real server.)
            authName: "servername",
            server: Ꮡ(new ServerInfo(Name: "servername", Auth: ["PLAIN"])),
            err: "unencrypted connection"
        ),
        new(
            authName: "servername",
            server: Ꮡ(new ServerInfo(Name: "servername", Auth: ["CRAM-MD5"])),
            err: "unencrypted connection"
        ),
        new(
            authName: "servername",
            server: Ꮡ(new ServerInfo(Name: "attacker", TLS: true)),
            err: "wrong host name"
        )
    ]);
    foreach (var (i, tt) in range(tests)) {
        var auth = PlainAuth(fooˢ, barˢ, bazˢ, tt.authName);
        var (_, _, err) = auth.Start(tt.server);
        @string got = ""u8;
        if (err != nil) {
            got = err.Error();
        }
        if (got != tt.err) {
            t.Errorf("%d. got error = %q; want %q", i, got, tt.err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fakeHostˢ => ref fakeHostˢᵇ;
private static readonly @string fakeHostˢᵇ = "fake.host"u8;
internal static ref readonly @string authFooauthQuitˢ => ref authFooauthQuitˢᵇ;
private static readonly @string authFooauthQuitˢᵇ = "AUTH FOOAUTH\r\n*\r\nQUIT\r\n"u8;

internal partial struct TestClientAuthTrimSpace_fake /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
    /*embed*/ public io_package.Writer Writer;
}

// Issue 17794: don't send a trailing space on AUTH command when there's no password.
public static void TestClientAuthTrimSpace(ж<testing.T> t) {
    @string server = "220 hello world\r\n"u8 + "200 some more"u8;
    ref var wrote = ref heap(new strings.Builder(), out var Ꮡwrote);
    faker fake = new(nil);
    fake.ReadWriter = new TestClientAuthTrimSpace_fake(
        new smtp_test_package.strings_ReaderжReader(strings.NewReader(server)),
        new smtp_test_package.strings_BuilderжWriter(Ꮡwrote)
    );
    var (c, err) = NewClient(fake, fakeHostˢ);
    if (err != nil) {
        t.Fatalf("NewClient: %v", err);
    }
    c.tls = true;
    c.didHello = true;
    c.Auth(new toServerEmptyAuth());
    c.Close();
    @string got = wrote.String();
    @string want = authFooauthQuitˢ; if (got != want) {
        t.Errorf("wrote %q; want %q", got, want);
    }
}

// toServerEmptyAuth is an implementation of Auth that only implements
// the Start method, and returns "FOOAUTH", nil, nil. Notably, it returns
// zero bytes for "toServer" so we can test that we don't send spaces at
// the end of the line. See TestClientAuthTrimSpace.
internal partial struct toServerEmptyAuth {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooauthˢ => ref fooauthˢᵇ;
private static readonly @string fooauthˢᵇ = "FOOAUTH"u8;

internal static (@string proto, slice<byte> toServer, error err) Start(this toServerEmptyAuth _, ж<global::go.net.smtp_package.ServerInfo> server) {
    return (fooauthˢ, default, default);
}

internal static (slice<byte> toServer, error err) Next(this toServerEmptyAuth _, slice<byte> fromServer, bool more) {
    throw panic("unexpected call");
}

internal partial struct faker {
    /*embed*/ public io_package.ReadWriter ReadWriter;
}

internal static error Close(this faker f) { return default; }
internal static netꓸAddr LocalAddr(this faker f) { return default; }
internal static netꓸAddr RemoteAddr(this faker f) { return default; }
internal static error SetDeadline(this faker f, time.Time _) { return default; }
internal static error SetReadDeadline(this faker f, time.Time _) { return default; }
internal static error SetWriteDeadline(this faker f, time.Time _) { return default; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aUtHˢ => ref aUtHˢᵇ;
private static readonly @string aUtHˢᵇ = "aUtH"u8;
internal static ref readonly @string dsnˢ => ref dsnˢᵇ;
private static readonly @string dsnˢᵇ = "DSN"u8;
internal static ref readonly @string userGmailComˢ => ref userGmailComˢᵇ;
private static readonly @string userGmailComˢᵇ = "user@gmail.com"u8;
internal static ref readonly @string user1GmailComˢ => ref user1GmailComˢᵇ;
private static readonly @string user1GmailComˢᵇ = "user1@gmail.com"u8;
internal static ref readonly @string user2GmailComDataAnotherˢ => ref user2GmailComDataAnotherˢᵇ;
private static readonly @string user2GmailComDataAnotherˢᵇ = "user2@gmail.com>\r\nDATA\r\nAnother injected message body\r\n.\r\nQUIT\r\n"u8;
internal static ref readonly @string user2GmailComˢ => ref user2GmailComˢᵇ;
private static readonly @string user2GmailComˢᵇ = "user2@gmail.com"u8;
internal static ref readonly @string smtpGoogleComˢ => ref smtpGoogleComˢᵇ;
private static readonly @string smtpGoogleComˢᵇ = "smtp.google.com"u8;
internal static ref readonly @string userˢ => ref userˢᵇ;
private static readonly @string userˢᵇ = "user"u8;
internal static ref readonly @string passˢ => ref passˢᵇ;
private static readonly @string passˢᵇ = "pass"u8;
internal static ref readonly @string golangNutsGooglegroupsˢ => ref golangNutsGooglegroupsˢᵇ;
private static readonly @string golangNutsGooglegroupsˢᵇ = "golang-nuts@googlegroups.com>\r\nDATA\r\nInjected message body\r\n.\r\nQUIT\r\n"u8;
internal static ref readonly @string userGmailComDataAnotherˢ => ref userGmailComDataAnotherˢᵇ;
private static readonly @string userGmailComDataAnotherˢᵇ = "user@gmail.com>\r\nDATA\r\nAnother injected message body\r\n.\r\nQUIT\r\n"u8;
internal static ref readonly @string golangNutsGooglegroupsˢ2 => ref golangNutsGooglegroupsˢ2ᵇ;
private static readonly @string golangNutsGooglegroupsˢ2ᵇ = "golang-nuts@googlegroups.com"u8;
internal static ref readonly @string fromUserGmailComToGolangˢ => ref fromUserGmailComToGolangˢᵇ;
private static readonly @string fromUserGmailComToGolangˢᵇ = """
From: user@gmail.com
To: golang-nuts@googlegroups.com
Subject: Hooray for Go

Line 1
.Leading dot line .
Goodbye.
"""u8.Raw();

public static void TestBasic(ж<testing.T> t) {
    @string server = strings.Join(strings.Split(basicServer, "\n"), "\r\n");
    @string client = strings.Join(strings.Split(basicClient, "\n"), "\r\n");

    ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
    var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
    faker fake = new(nil);
    fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
    var c = Ꮡ(new Client(Text: textproto.NewConn(fake), localName: "localhost"));

    var errΔ1 = c.helo(); if (errΔ1 != nil) {
        t.Fatalf("HELO failed: %s", errΔ1);
    }
    var errΔ2 = c.ehlo(); if (errΔ2 == nil) {
        t.Fatalf("Expected first EHLO to fail");
    }
    var errΔ3 = c.ehlo(); if (errΔ3 != nil) {
        t.Fatalf("Second EHLO failed: %s", errΔ3);
    }

    c.didHello = true;
    {
        var (ok, args) = c.Extension(aUtHˢ); if (!ok || args != "LOGIN PLAIN"u8) {
            t.Fatalf("Expected AUTH supported");
        }
    }
    {
        var (ok, _) = c.Extension(dsnˢ); if (ok) {
            t.Fatalf("Shouldn't support DSN");
        }
    }

    var errΔ4 = c.Mail(userGmailComˢ); if (errΔ4 == nil) {
        t.Fatalf("MAIL should require authentication");
    }

    var errΔ5 = c.Verify(user1GmailComˢ); if (errΔ5 == nil) {
        t.Fatalf("First VRFY: expected no verification");
    }
    var errΔ6 = c.Verify(user2GmailComDataAnotherˢ); if (errΔ6 == nil) {
        t.Fatalf("VRFY should have failed due to a message injection attempt");
    }
    var errΔ7 = c.Verify(user2GmailComˢ); if (errΔ7 != nil) {
        t.Fatalf("Second VRFY: expected verification, got %s", errΔ7);
    }

    // fake TLS so authentication won't complain
    c.tls = true;
    c.serverName = smtpGoogleComˢ;
    var errΔ8 = c.Auth(PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ)); if (errΔ8 != nil) {
        t.Fatalf("AUTH failed: %s", errΔ8);
    }

    var errΔ9 = c.Rcpt(golangNutsGooglegroupsˢ); if (errΔ9 == nil) {
        t.Fatalf("RCPT should have failed due to a message injection attempt");
    }
    var errΔ10 = c.Mail(userGmailComDataAnotherˢ); if (errΔ10 == nil) {
        t.Fatalf("MAIL should have failed due to a message injection attempt");
    }
    var errΔ11 = c.Mail(userGmailComˢ); if (errΔ11 != nil) {
        t.Fatalf("MAIL failed: %s", errΔ11);
    }
    var errΔ12 = c.Rcpt(golangNutsGooglegroupsˢ2); if (errΔ12 != nil) {
        t.Fatalf("RCPT failed: %s", errΔ12);
    }
    @string msg = fromUserGmailComToGolangˢ;
    var (w, err) = c.Data();
    if (err != nil) {
        t.Fatalf("DATA failed: %s", err);
    }
    var (_, errΔ13) = w.Write(slice<byte>(msg)); if (errΔ13 != nil) {
        t.Fatalf("Data write failed: %s", errΔ13);
    }
    var errΔ14 = w.Close(); if (errΔ14 != nil) {
        t.Fatalf("Bad data response: %s", errΔ14);
    }

    var errΔ15 = c.Quit(); if (errΔ15 != nil) {
        t.Fatalf("QUIT failed: %s", errΔ15);
    }

    bcmdbuf.Flush();
    @string actualcmds = cmdbuf.String();
    if (client != actualcmds) {
        t.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
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

"""u8.Raw();

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

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string eh502Eh221Okˢ => ref eh502Eh221Okˢᵇ;
private static readonly @string eh502Eh221Okˢᵇ = """
502 EH?
502 EH?
221 OK

"""u8.Raw();
internal static ref readonly @string ehloLocalhostHeloˢ => ref ehloLocalhostHeloˢᵇ;
private static readonly @string ehloLocalhostHeloˢᵇ = """
EHLO localhost
HELO localhost
QUIT

"""u8.Raw();
internal static ref readonly @string localhostˢ => ref localhostˢᵇ;
private static readonly @string localhostˢᵇ = "localhost"u8;
internal static readonly object expectedEhloToFailˢ = (@string)"expected EHLO to fail"u8;

public static void TestHELOFailed(ж<testing.T> t) {
    @string serverLines = eh502Eh221Okˢ;
    @string clientLines = ehloLocalhostHeloˢ;

    @string server = strings.Join(strings.Split(serverLines, "\n"), "\r\n");
    @string client = strings.Join(strings.Split(clientLines, "\n"), "\r\n");
    ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
    var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
    faker fake = new(nil);
    fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
    var c = Ꮡ(new Client(Text: textproto.NewConn(fake), localName: "localhost"));

    {
        var err = c.Hello(localhostˢ); if (err == nil) {
            t.Fatal(expectedEhloToFailˢ);
        }
    }
    {
        var err = c.Quit(); if (err != nil) {
            t.Errorf("QUIT failed: %s", err);
        }
    }
    bcmdbuf.Flush();
    @string actual = cmdbuf.String();
    if (client != actual) {
        t.Errorf("Got:\n%s\nWant:\n%s", actual, client);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string heloˢ => ref heloˢᵇ;
private static readonly @string heloˢᵇ = "helo"u8;
internal static ref readonly @string ehloˢ => ref ehloˢᵇ;
private static readonly @string ehloˢᵇ = "ehlo"u8;
internal static ref readonly @string ehlo8bitmimeˢ => ref ehlo8bitmimeˢᵇ;
private static readonly @string ehlo8bitmimeˢᵇ = "ehlo 8bitmime"u8;
internal static ref readonly @string ehloSmtputf8ˢ => ref ehloSmtputf8ˢᵇ;
private static readonly @string ehloSmtputf8ˢᵇ = "ehlo smtputf8"u8;
internal static ref readonly @string userGmailComˢ2 => ref userGmailComˢ2ᵇ;
private static readonly @string userGmailComˢ2ᵇ = "user+📧@gmail.com"u8;
internal static ref readonly @string ehlo8bitmimeSmtputf8ˢ => ref ehlo8bitmimeSmtputf8ˢᵇ;
private static readonly @string ehlo8bitmimeSmtputf8ˢᵇ = "ehlo 8bitmime smtputf8"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicServerᶜ => ref basicServerᶜᵇ;
private static readonly @string basicServerᶜᵇ = """
250 mx.google.com at your service
250 Sender OK
221 Goodbye

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicClientᶜ => ref basicClientᶜᵇ;
private static readonly @string basicClientᶜᵇ = """
HELO localhost
MAIL FROM:<user@gmail.com>
QUIT

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicServerᶜ1 => ref basicServerᶜ1ᵇ;
private static readonly @string basicServerᶜ1ᵇ = """
250-mx.google.com at your service
250 SIZE 35651584
250 Sender OK
221 Goodbye

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicClientᶜ1 => ref basicClientᶜ1ᵇ;
private static readonly @string basicClientᶜ1ᵇ = """
EHLO localhost
MAIL FROM:<user@gmail.com>
QUIT

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicServerᶜ2 => ref basicServerᶜ2ᵇ;
private static readonly @string basicServerᶜ2ᵇ = """
250-mx.google.com at your service
250-SIZE 35651584
250 8BITMIME
250 Sender OK
221 Goodbye

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicClientᶜ2 => ref basicClientᶜ2ᵇ;
private static readonly @string basicClientᶜ2ᵇ = """
EHLO localhost
MAIL FROM:<user@gmail.com> BODY=8BITMIME
QUIT

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicServerᶜ3 => ref basicServerᶜ3ᵇ;
private static readonly @string basicServerᶜ3ᵇ = """
250-mx.google.com at your service
250-SIZE 35651584
250 SMTPUTF8
250 Sender OK
221 Goodbye

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicClientᶜ3 => ref basicClientᶜ3ᵇ;
private static readonly @string basicClientᶜ3ᵇ = """
EHLO localhost
MAIL FROM:<user+📧@gmail.com> SMTPUTF8
QUIT

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicServerᶜ4 => ref basicServerᶜ4ᵇ;
private static readonly @string basicServerᶜ4ᵇ = """
250-mx.google.com at your service
250-SIZE 35651584
250-8BITMIME
250 SMTPUTF8
250 Sender OK
221 Goodbye
	
"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string basicClientᶜ4 => ref basicClientᶜ4ᵇ;
private static readonly @string basicClientᶜ4ᵇ = """
EHLO localhost
MAIL FROM:<user+📧@gmail.com> BODY=8BITMIME SMTPUTF8
QUIT

"""u8.Raw();

public static void TestExtensions(ж<testing.T> t) {
    (ж<global::go.net.smtp_package.Client> c, ж<bufio.Writer> bcmdbuf, ж<strings.Builder> cmdbuf) fake(@string server) {
        ж<global::go.net.smtp_package.Client> c = default;
        ж<bufio.Writer> bcmdbuf = default;
        ж<strings.Builder> cmdbuf = default;
        server = strings.Join(strings.Split(server, "\n"), "\r\n");

        cmdbuf = Ꮡ(new strings.Builder());
        bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(cmdbuf));
        faker fakeΔ1 = new(nil);
        fakeΔ1.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        c = Ꮡ(new Client(Text: textproto.NewConn(fakeΔ1), localName: "localhost"));

        return (c, bcmdbuf, cmdbuf);
    }

    var fakeʗ1 = fake;
    t.Run(heloˢ, (ж<testing.T> tΔ1) => {
        @string basicServer = basicServerᶜ;

        @string basicClient = basicClientᶜ;

        var (c, bcmdbuf, cmdbuf) = fakeʗ1(basicServer);

        {
            var err = c.helo(); if (err != nil) {
                tΔ1.Fatalf("HELO failed: %s", err);
            }
        }
        c.didHello = true;
        {
            var err = c.Mail(userGmailComˢ); if (err != nil) {
                tΔ1.Fatalf("MAIL FROM failed: %s", err);
            }
        }
        {
            var err = c.Quit(); if (err != nil) {
                tΔ1.Fatalf("QUIT failed: %s", err);
            }
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"), "\r\n");
        if (client != actualcmds) {
            tΔ1.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
        }
    });

    var fakeʗ2 = fake;
    t.Run(ehloˢ, (ж<testing.T> tΔ2) => {
        @string basicServer = basicServerᶜ1;

        @string basicClient = basicClientᶜ1;

        var (c, bcmdbuf, cmdbuf) = fakeʗ2(basicServer);

        {
            var err = c.Hello(localhostˢ); if (err != nil) {
                tΔ2.Fatalf("EHLO failed: %s", err);
            }
        }
        {
            var (ok, _) = c.Extension("8BITMIME"); if (ok) {
                tΔ2.Fatalf("Shouldn't support 8BITMIME");
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (ok) {
                tΔ2.Fatalf("Shouldn't support SMTPUTF8");
            }
        }
        {
            var err = c.Mail(userGmailComˢ); if (err != nil) {
                tΔ2.Fatalf("MAIL FROM failed: %s", err);
            }
        }
        {
            var err = c.Quit(); if (err != nil) {
                tΔ2.Fatalf("QUIT failed: %s", err);
            }
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"), "\r\n");
        if (client != actualcmds) {
            tΔ2.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
        }
    });

    var fakeʗ3 = fake;
    t.Run(ehlo8bitmimeˢ, (ж<testing.T> tΔ3) => {
        @string basicServer = basicServerᶜ2;

        @string basicClient = basicClientᶜ2;

        var (c, bcmdbuf, cmdbuf) = fakeʗ3(basicServer);

        {
            var err = c.Hello(localhostˢ); if (err != nil) {
                tΔ3.Fatalf("EHLO failed: %s", err);
            }
        }
        {
            var (ok, _) = c.Extension("8BITMIME"); if (!ok) {
                tΔ3.Fatalf("Should support 8BITMIME");
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (ok) {
                tΔ3.Fatalf("Shouldn't support SMTPUTF8");
            }
        }
        {
            var err = c.Mail(userGmailComˢ); if (err != nil) {
                tΔ3.Fatalf("MAIL FROM failed: %s", err);
            }
        }
        {
            var err = c.Quit(); if (err != nil) {
                tΔ3.Fatalf("QUIT failed: %s", err);
            }
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"), "\r\n");
        if (client != actualcmds) {
            tΔ3.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
        }
    });

    var fakeʗ4 = fake;
    t.Run(ehloSmtputf8ˢ, (ж<testing.T> tΔ4) => {
        @string basicServer = basicServerᶜ3;

        @string basicClient = basicClientᶜ3;

        var (c, bcmdbuf, cmdbuf) = fakeʗ4(basicServer);

        {
            var err = c.Hello(localhostˢ); if (err != nil) {
                tΔ4.Fatalf("EHLO failed: %s", err);
            }
        }
        {
            var (ok, _) = c.Extension("8BITMIME"); if (ok) {
                tΔ4.Fatalf("Shouldn't support 8BITMIME");
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (!ok) {
                tΔ4.Fatalf("Should support SMTPUTF8");
            }
        }
        {
            var err = c.Mail(userGmailComˢ2); if (err != nil) {
                tΔ4.Fatalf("MAIL FROM failed: %s", err);
            }
        }
        {
            var err = c.Quit(); if (err != nil) {
                tΔ4.Fatalf("QUIT failed: %s", err);
            }
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"), "\r\n");
        if (client != actualcmds) {
            tΔ4.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
        }
    });

    var fakeʗ5 = fake;
    t.Run(ehlo8bitmimeSmtputf8ˢ, (ж<testing.T> tΔ5) => {
        @string basicServer = basicServerᶜ4;

        @string basicClient = basicClientᶜ4;

        var (c, bcmdbuf, cmdbuf) = fakeʗ5(basicServer);

        {
            var err = c.Hello(localhostˢ); if (err != nil) {
                tΔ5.Fatalf("EHLO failed: %s", err);
            }
        }
        c.didHello = true;
        {
            var (ok, _) = c.Extension("8BITMIME"); if (!ok) {
                tΔ5.Fatalf("Should support 8BITMIME");
            }
        }
        {
            var (ok, _) = c.Extension(smtputf8ˢ); if (!ok) {
                tΔ5.Fatalf("Should support SMTPUTF8");
            }
        }
        {
            var err = c.Mail(userGmailComˢ2); if (err != nil) {
                tΔ5.Fatalf("MAIL FROM failed: %s", err);
            }
        }
        {
            var err = c.Quit(); if (err != nil) {
                tΔ5.Fatalf("QUIT failed: %s", err);
            }
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        @string client = strings.Join(strings.Split(basicClient, "\n"), "\r\n");
        if (client != actualcmds) {
            tΔ5.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
        }
    });
}

public static void TestNewClient(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string server = strings.Join(strings.Split(newClientServer, "\n"), "\r\n");
        @string client = strings.Join(strings.Split(newClientClient, "\n"), "\r\n");

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
        if (err != nil) {
            t.Fatalf("NewClient: %v\n(after %v)", err, @out());
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            var (ok, args) = c.Extension(aUtHˢ); if (!ok || args != "LOGIN PLAIN"u8) {
                t.Fatalf("Expected AUTH supported");
            }
        }
        {
            var (ok, _) = c.Extension(dsnˢ); if (ok) {
                t.Fatalf("Shouldn't support DSN");
            }
        }
        var errΔ1 = c.Quit(); if (errΔ1 != nil) {
            t.Fatalf("QUIT failed: %s", errΔ1);
        }

        @string actualcmds = @out();
        if (client != actualcmds) {
            t.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
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

"""u8.Raw();

internal static @string newClientClient = """
EHLO localhost
QUIT

"""u8.Raw();

public static void TestNewClient2(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string server = strings.Join(strings.Split(newClient2Server, "\n"), "\r\n");
        @string client = strings.Join(strings.Split(newClient2Client, "\n"), "\r\n");

        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        faker fake = new(nil);
        fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        var (c, err) = NewClient(fake, fakeHostˢ);
        if (err != nil) {
            t.Fatalf("NewClient: %v", err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var (ok, _) = c.Extension(dsnˢ); if (ok) {
            t.Fatalf("Shouldn't support DSN");
        }
        var errΔ1 = c.Quit(); if (errΔ1 != nil) {
            t.Fatalf("QUIT failed: %s", errΔ1);
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        if (client != actualcmds) {
            t.Fatalf("Got:\n%s\nExpected:\n%s", actualcmds, client);
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

"""u8.Raw();

internal static @string newClient2Client = """
EHLO localhost
HELO localhost
QUIT

"""u8.Raw();

public static partial void TestNewClientWithTLS(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (cert, err) = tls.X509KeyPair(localhostCert, localhostKey);
        if (err != nil) {
            t.Fatalf("loadcert: %v", err);
        }

        ref var config = ref heap<tls.Config>(new tls.Config(Certificates: [cert]), out var Ꮡconfig);

        (var ln, err) = tls.Listen(tcpˢ, "127.0.0.1:0", Ꮡconfig);
        if (err != nil) {
            (ln, err) = tls.Listen(tcpˢ, "[::1]:0", Ꮡconfig);
            if (err != nil) {
                t.Fatalf("server: listen: %v", err);
            }
        }

        var lnʗ1 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (connΔ1, errΔ1) = lnʗ1.Accept();
                if (errΔ1 != nil) {
                    t.Errorf("server: accept: %v", errΔ1);
                    return;
                }
                var connʗ1 = connΔ1;
                defer(() => connʗ1.Close(), ref ᒐ);

                (_, errΔ1) = connΔ1.Write(slice<byte>("220 SIGNS\r\n"u8));
                if (errΔ1 != nil) {
                    t.Errorf("server: write: %v", errΔ1);
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        config.InsecureSkipVerify = true;
        (var conn, err) = tls.Dial(tcpˢ, ln.Addr().String(), Ꮡconfig);
        if (err != nil) {
            t.Fatalf("client: dial: %v", err);
        }
        var connʗ2 = conn;
        defer(() => connʗ2.Close(), ref ᒐ);

        (var client, err) = NewClient(new tls.ConnжConn(conn), ln.Addr().String());
        if (err != nil) {
            t.Fatalf("smtp: newclient: %v", err);
        }
        if (!client.tls) {
            t.Errorf("client.tls Got: %t Expected: %t", client.tls, true);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string customhostˢ => ref customhostˢᵇ;
private static readonly @string customhostˢᵇ = "customhost"u8;
internal static ref readonly @string hostinjectionDataˢ => ref hostinjectionDataˢᵇ;
private static readonly @string hostinjectionDataˢᵇ = "hostinjection>\n\rDATA\r\nInjected message body\r\n.\r\nQUIT\r\n"u8;
internal static ref readonly @string testExampleComˢ => ref testExampleComˢᵇ;
private static readonly @string testExampleComˢᵇ = "test@example.com"u8;
internal static ref readonly @string featureˢ => ref featureˢᵇ;
private static readonly @string featureˢᵇ = "feature"u8;

public static void TestHello(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (len(helloServer) != len(helloClient)) {
            t.Fatalf("Hello server and client size mismatch");
        }

        for (nint i = 0; i < len(helloServer); i++) {
            @string server = strings.Join(strings.Split(baseHelloServer + helloServer[i], "\n"), "\r\n");
            @string client = strings.Join(strings.Split(baseHelloClient + helloClient[i], "\n"), "\r\n");
            ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
            var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
            faker fake = new(nil);
            fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
            var (c, err) = NewClient(fake, fakeHostˢ);
            if (err != nil) {
                t.Fatalf("NewClient: %v", err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            c.localName = customhostˢ;
            err = default;

            switch (i) {
            case 0: {
                err = c.Hello(hostinjectionDataˢ);
                if (err == nil) {
                    t.Errorf("Expected Hello to be rejected due to a message injection attempt");
                }
                err = c.Hello(customhostˢ);
                break;
            }
            case 1: {
                err = c.StartTLS(nil);
                if (err.Error() == @"502 ""Not implemented"""u8) {
                    err = default;
                }
                break;
            }
            case 2: {
                err = c.Verify(testExampleComˢ);
                break;
            }
            case 3: {
                c.tls = true;
                c.serverName = smtpGoogleComˢ;
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
                    t.Errorf("Expected FEATURE not to be supported");
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
                if (err != nil) {
                    err = c.Hello(customhostˢ);
                    if (err != nil) {
                        t.Errorf("Want error, got none");
                    }
                }
                break;
            }
            case 9: {
                err = c.Noop();
                break;
            }
            default: {
                t.Fatalf("Unhandled command");
                break;
            }}

            if (err != nil) {
                t.Errorf("Command %d failed: %v", i, err);
            }

            bcmdbuf.Flush();
            @string actualcmds = cmdbuf.String();
            if (client != actualcmds) {
                t.Errorf("Got:\n%s\nExpected:\n%s", actualcmds, client);
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

"""u8.Raw();

internal static slice<@string> helloServer = [
    "",
    "502 Not implemented\n",
    "250 User is valid\n",
    "235 Accepted\n",
    "250 Sender ok\n",
    "",
    "250 Reset ok\n",
    "221 Goodbye\n",
    "250 Sender ok\n",
    "250 ok\n"
];

internal static @string baseHelloClient = """
EHLO customhost
HELO customhost

"""u8.Raw();

internal static slice<@string> helloClient = [
    "",
    "STARTTLS\n",
    "VRFY test@example.com\n",
    "AUTH PLAIN AHVzZXIAcGFzcw==\n",
    "MAIL FROM:<test@example.com>\n",
    "",
    "RSET\n",
    "QUIT\n",
    "VRFY test@example.com\n",
    "NOOP\n"
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fromTestExampleComToˢ => ref fromTestExampleComToˢᵇ;
private static readonly @string fromTestExampleComToˢᵇ = """
From: test@example.com
To: other@example.com
Subject: SendMail test

SendMail is working for me.

"""u8.Raw();

public static partial void TestSendMail(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string server = strings.Join(strings.Split(sendMailServer, "\n"), "\r\n");
        @string client = strings.Join(strings.Split(sendMailClient, "\n"), "\r\n");
        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        var (l, err) = net.Listen(tcpˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatalf("Unable to create listener: %v", err);
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
                if (errΔ1 != nil) {
                    t.Errorf("Accept error: %v", errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);

                var tc = textproto.NewConn(new smtp_test_package.net_ConnᴠReadWriteCloser(conn));
                for (nint i = 0; i < len(data) && data[i] != ""; i++) {
                    tc.of(textproto.Conn.ᏑWriter).PrintfLine("%s", data[i]);
                    while (len(data[i]) >= 4 && data[i][3] == '-') {
                        i++;
                        tc.of(textproto.Conn.ᏑWriter).PrintfLine("%s", data[i]);
                    }
                    if (data[i] == "221 Goodbye") {
                        return;
                    }
                    var read = false;
                    while (!read || data[i] == "354 Go ahead") {
                        var (msg, errΔ2) = tc.of(textproto.Conn.ᏑReader).ReadLine();
                        bcmdbufʗ1.Write(slice<byte>(msg + "\r\n"));
                        read = true;
                        if (errΔ2 != nil) {
                            t.Errorf("Read error: %v", errΔ2);
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
        }, strings.Split(server, "\r\n"));

        err = SendMail(l.Addr().String(), default, testExampleComˢ, ["other@example.com>\n\rDATA\r\nInjected message body\r\n.\r\nQUIT\r\n"], slice<byte>(strings.Replace(fromTestExampleComToˢ, "\n", "\r\n", -1)));
        if (err == nil) {
            t.Errorf("Expected SendMail to be rejected due to a message injection attempt");
        }

        err = SendMail(l.Addr().String(), default, testExampleComˢ, ["other@example.com"], slice<byte>(strings.Replace(fromTestExampleComToˢ, "\n", "\r\n", -1)));

        if (err != nil) {
            t.Errorf("%v", err);
        }

        ᐸꟷ(done);
        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        if (client != actualcmds) {
            t.Errorf("Got:\n%s\nExpected:\n%s", actualcmds, client);
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

"""u8.Raw();

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

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "220 hello world"u8;
internal static ref readonly @string mxGoogleComAtYourServiceˢ => ref mxGoogleComAtYourServiceˢᵇ;
private static readonly @string mxGoogleComAtYourServiceˢᵇ = "250 mx.google.com at your service"u8;
internal static readonly object sendMailServerDoesnTˢ = (@string)"SendMail: Server doesn't support AUTH, expected to get an error, but got none "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantMsgᶜ => ref wantMsgᶜᵇ;
private static readonly @string wantMsgᶜᵇ = "EHLO localhost"u8;

public static partial void TestSendMailWithAuth(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (l, err) = net.Listen(tcpˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatalf("Unable to create listener: %v", err);
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
                if (errΔ1 != nil) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("Accept: %v"u8, errΔ1));
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);

                var tc = textproto.NewConn(new smtp_test_package.net_ConnᴠReadWriteCloser(conn));
                tc.of(textproto.Conn.ᏑWriter).PrintfLine(helloWorldˢ);
                (var msg, errΔ1) = tc.of(textproto.Conn.ᏑReader).ReadLine();
                if (errΔ1 != nil) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("ReadLine error: %v"u8, errΔ1));
                    return;
                }
                @string wantMsg = wantMsgᶜ;
                if (msg != wantMsg) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("unexpected response %q; want %q"u8, msg, wantMsg));
                    return;
                }
                errΔ1 = tc.of(textproto.Conn.ᏑWriter).PrintfLine(mxGoogleComAtYourServiceˢ);
                if (errΔ1 != nil) {
                    errChʗ1.ᐸꟷ(fmt.Errorf("PrintfLine: %v"u8, errΔ1));
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        err = SendMail(l.Addr().String(), PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ), testExampleComˢ, ["other@example.com"], slice<byte>(strings.Replace(fromTestExampleComToˢ, "\n", "\r\n", -1)));
        if (err == nil) {
            t.Error(sendMailServerDoesnTˢ);
        }
        if (err.Error() != "smtp: server doesn't support AUTH"u8) {
            t.Errorf("Expected: smtp: server doesn't support AUTH, got: %s", err);
        }
        err = ᐸꟷ(errCh);
        if (err != nil) {
            t.Fatalf("server error: %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object authExpectedErrorGotNoneˢ = (@string)"Auth: expected error; got none"u8;
internal static readonly object invalidCredentialsˢ = (@string)@"535 ""Invalid credentials\nplease see www.example.com"""u8;

public static void TestAuthFailed(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string server = strings.Join(strings.Split(authFailedServer, "\n"), "\r\n");
        @string client = strings.Join(strings.Split(authFailedClient, "\n"), "\r\n");
        ref var cmdbuf = ref heap(new strings.Builder(), out var Ꮡcmdbuf);
        var bcmdbuf = bufio.NewWriter(new smtp_test_package.strings_BuilderжWriter(Ꮡcmdbuf));
        faker fake = new(nil);
        fake.ReadWriter = new smtp_test_package.bufio_ReadWriterжReadWriter(bufio.NewReadWriter(bufio.NewReader(new smtp_test_package.strings_ReaderжReader(strings.NewReader(server))), bcmdbuf));
        var (c, err) = NewClient(fake, fakeHostˢ);
        if (err != nil) {
            t.Fatalf("NewClient: %v", err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        c.tls = true;
        c.serverName = smtpGoogleComˢ;
        err = c.Auth(PlainAuth(""u8, userˢ, passˢ, smtpGoogleComˢ));

        if (err == nil) {
            t.Error(authExpectedErrorGotNoneˢ);
        } else if (err.Error() != @"535 ""Invalid credentials\nplease see www.example.com"""u8) {
            t.Errorf("Auth: got error: %v, want: %s", err, invalidCredentialsˢ);
        }

        bcmdbuf.Flush();
        @string actualcmds = cmdbuf.String();
        if (client != actualcmds) {
            t.Errorf("Got:\n%s\nExpected:\n%s", actualcmds, client);
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

"""u8.Raw();

internal static @string authFailedClient = """
EHLO localhost
AUTH PLAIN AHVzZXIAcGFzcw==
*
QUIT

"""u8.Raw();

public static partial void TestTLSClient(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "freebsd"u8 || runtime.GOOS == "js"u8 || runtime.GOOS == "wasip1"u8) {
            testenv.SkipFlaky(new smtp_test_package.testing_TжTB(t), 19229);
        }
        var ln = newLocalListener(t);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(0);
        var errcʗ1 = errc;
        var lnʗ2 = ln;
        goǃ(() => {
            errcʗ1.ᐸꟷ(sendMail(lnʗ2.Addr().String()));
        });
        var (conn, err) = ln.Accept();
        if (err != nil) {
            t.Fatalf("failed to accept connection: %v", err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var errΔ1 = serverHandle(conn, t); if (errΔ1 != nil) {
            t.Fatalf("failed to handle connection: %v", errΔ1);
        }
        var errΔ2 = ᐸꟷ(errc); if (errΔ2 != nil) {
            t.Fatalf("client error: %v", errΔ2);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTLSConnState(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(t);
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
                if (err != nil) {
                    t.Errorf("Server accept: %v", err);
                    return;
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                var errΔ1 = serverHandle(c, t); if (errΔ1 != nil) {
                    t.Errorf("server error: %v", errΔ1);
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
                if (err != nil) {
                    t.Errorf("Client dial: %v", err);
                    return;
                }
                var cʗ2 = c;
                defer(() => cʗ2.Quit(), ref ᒐ);
                var cfg = Ꮡ(new tls.Config(ServerName: "example.com"));
                testHookStartTLS(cfg); // set the RootCAs
                var errΔ1 = c.StartTLS(cfg); if (errΔ1 != nil) {
                    t.Errorf("StartTLS: %v", errΔ1);
                    return;
                }
                ref var cs = ref heap<tlsꓸConnectionState>(out var Ꮡcs);
                (cs, var ok) = c.TLSConnectionState();
                if (!ok) {
                    t.Errorf("TLSConnectionState returned ok == false; want true");
                    return;
                }
                if (cs.Version == 0 || !cs.HandshakeComplete) {
                    t.Errorf("ConnectionState = %#v; expect non-zero Version and HandshakeComplete", cs);
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
internal static ref readonly @string tcp6ˢ => ref tcp6ˢᵇ;
private static readonly @string tcp6ˢᵇ = "tcp6"u8;

internal static net.Listener newLocalListener(ж<testing.T> t) {
    var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0");
    if (err != nil) {
        (ln, err) = net.Listen(tcp6ˢ, "[::1]:0");
    }
    if (err != nil) {
        t.Fatal(err);
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
internal static ref readonly @string esmtpServiceReadyˢ => ref esmtpServiceReadyˢᵇ;
private static readonly @string esmtpServiceReadyˢᵇ = "220 127.0.0.1 ESMTP service ready"u8;
internal static ref readonly @string esmtpOffersAWarmHugOfˢ => ref esmtpOffersAWarmHugOfˢᵇ;
private static readonly @string esmtpOffersAWarmHugOfˢᵇ = "250-127.0.0.1 ESMTP offers a warm hug of welcome"u8;
internal static ref readonly @string starttlsˢ2 => ref starttlsˢ2ᵇ;
private static readonly @string starttlsˢ2ᵇ = "250-STARTTLS"u8;
internal static ref readonly @string goAheadˢ => ref goAheadˢᵇ;
private static readonly @string goAheadˢᵇ = "220 Go ahead"u8;

// smtp server, finely tailored to deal with our own client only!
internal static error serverHandle(net.Conn c, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var recvʗ1 = new smtpSender(c);
        var send = (@string p1) => recvʗ1.send(p1);
        send(esmtpServiceReadyˢ);
        var s = bufio.NewScanner(new smtp_test_package.net_ConnᴠReader(c));
        while (s.Scan()) {
            var exprᴛ1 = s.Text();
            switch (ᐧ) {
            case {} when exprᴛ1 == "EHLO localhost"u8: {
                send(esmtpOffersAWarmHugOfˢ);
                send(starttlsˢ2);
                send("250 Ok");
                break;
            }
            case {} when exprᴛ1 == "STARTTLS"u8: {
                send(goAheadˢ);
                var (keypair, err) = tls.X509KeyPair(localhostCert, localhostKey);
                if (err != nil) {
                    return err;
                }
                var config = Ꮡ(new tls.Config(Certificates: [keypair]));
                c = new tls.ConnжConn(tls.Server(c, config));
                defer(() => c.Close(), ref ᒐ);
                return serverHandleTLS(c, t);
            }
            default: {
                t.Fatalf("unrecognized command: %q", s.Text());
                break;
            }}
        }
        return s.Err();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sendTheMailDataEndWithˢ => ref sendTheMailDataEndWithˢᵇ;
private static readonly @string sendTheMailDataEndWithˢᵇ = "354 send the mail data, end with ."u8;
internal static ref readonly @string serviceClosingˢ => ref serviceClosingˢᵇ;
private static readonly @string serviceClosingˢᵇ = "221 127.0.0.1 Service closing transmission channel"u8;

internal static error serverHandleTLS(net.Conn c, ж<testing.T> t) {
    var recvʗ1 = new smtpSender(c);
    var send = (@string p1) => recvʗ1.send(p1);
    var s = bufio.NewScanner(new smtp_test_package.net_ConnᴠReader(c));
    while (s.Scan()) {
        var exprᴛ1 = s.Text();
        switch (ᐧ) {
        case {} when exprᴛ1 == "EHLO localhost"u8: {
            send("250 Ok");
            break;
        }
        case {} when exprᴛ1 == "MAIL FROM:<joe1@example.com>"u8: {
            send("250 Ok");
            break;
        }
        case {} when exprᴛ1 == "RCPT TO:<joe2@example.com>"u8: {
            send("250 Ok");
            break;
        }
        case {} when exprᴛ1 == "DATA"u8: {
            send(sendTheMailDataEndWithˢ);
            send("250 Ok");
            break;
        }
        case {} when exprᴛ1 == "Subject: test"u8: {
            break;
        }
        case {} when exprᴛ1 == ""u8: {
            break;
        }
        case {} when exprᴛ1 == "howdy!"u8: {
            break;
        }
        case {} when exprᴛ1 == "."u8: {
            break;
        }
        case {} when exprᴛ1 == "QUIT"u8: {
            send(serviceClosingˢ);
            return default;
        }
        default: {
            t.Fatalf("unrecognized command during TLS: %q", s.Text());
            break;
        }}
    }
    return s.Err();
}

[GoInit] internal static void init() {
    var testRootCAs = x509.NewCertPool();
    testRootCAs.AppendCertsFromPEM(localhostCert);
    var testRootCAsʗ1 = testRootCAs;
    testHookStartTLS = (ж<tls.Config> config) => {
        config.RootCAs = testRootCAsʗ1;
        config.Time = () => time.Unix(0, 0);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string joe1ExampleComˢ => ref joe1ExampleComˢᵇ;
private static readonly @string joe1ExampleComˢᵇ = "joe1@example.com"u8;

internal static error sendMail(@string hostPort) {
    @string from = joe1ExampleComˢ;
    var to = slice<@string>(["joe2@example.com"]);
    return SendMail(hostPort, default, from, to, slice<byte>("Subject: test\n\nhowdy!"u8));
}

// localhostCert is a PEM-encoded TLS cert generated from src/crypto/tls:
//
//	go run generate_cert.go --rsa-bits 2048 --host 127.0.0.1,::1,example.com \
//		--ca --start-date "Jan 1 00:00:00 1970" --duration=1000000h
//
// The actual expiry time of this cert should not matter since we set
// tls.Config.Time to be fixed in our tests.
internal static slice<byte> localhostCert = slice<byte>("""

-----BEGIN CERTIFICATE-----
MIIDOjCCAiKgAwIBAgIRAM1/4MS0P4BXstjv50eeEsswDQYJKoZIhvcNAQELBQAw
EjEQMA4GA1UEChMHQWNtZSBDbzAgFw03MDAxMDEwMDAwMDBaGA8yMDg0MDEyOTE2
MDAwMFowEjEQMA4GA1UEChMHQWNtZSBDbzCCASIwDQYJKoZIhvcNAQEBBQADggEP
ADCCAQoCggEBAN5KVxPqz+h6hHC3QBg7ZwCZUql4Mbz7LvrYg+1CCRJnbWdK2MTP
s0Hi3CKzAEE6H52rPO1kqdcIo2D1Pw2PC7/TB6w8ASLumJQaZfBlbaZesbBfrtIu
iEtSKs/Iwxp57mn9RbjUkQgu3nSzjrgbFPrktz6lJ4LfC6azN62klkCTfspCDTjU
Sk58dlygIweYkIiWHAh5f+KvKT1aeheNMkLEx1KZ+Vz+Y/oEnEKjRxBcnUIwzIrZ
/fXbvRq8Fa9nLuDO8F0JDcM1Zg9gzPvFmdFy8fifC3H/uflcLVJp4ImtEWEoVPvt
OQLAwkulknsXACBVsCu/JgDU7Yda6Lk2Qq0CAwEAAaOBiDCBhTAOBgNVHQ8BAf8E
BAMCAqQwEwYDVR0lBAwwCgYIKwYBBQUHAwEwDwYDVR0TAQH/BAUwAwEB/zAdBgNV
HQ4EFgQU3YcFHBnqY6c03/Ydoy94fa59+F8wLgYDVR0RBCcwJYILZXhhbXBsZS5j
b22HBH8AAAGHEAAAAAAAAAAAAAAAAAAAAAEwDQYJKoZIhvcNAQELBQADggEBANI7
DKO8ub7SOwesjcnt4fCfHumink2ixo2nxW/DpnNWBaAhA529HCAa7BgAFzQi/ES1
ALEFEr0Phad4KA+9qrQXIJsMV/GTPPsTVuluU9Uhq6V2M8YelQuoMDbnjZDWcdZV
0arpMdVT8vU4eOE7XWlo83gA08+1mX4WbEI5XaHDeKE4ogifCGamroOTzJidfMg/
tz01iclVt7Fkri6PYcUS+8ySYrc2XH+h1P2xZCNP8VhAsrpnqQqGS85TTSUkOgZt
ITQpEVnLIDwZSX0zYrN5z8gChVhzzMR8XmsOpMUBJL5qcpWrqy/ZswmsMvjVXmeN
zQLoXduc3BgLtaXv7O0=
-----END CERTIFICATE-----
"""u8.Raw());

// localhostKey is the private key for localhostCert.
internal static slice<byte> localhostKey = slice<byte>(testingKey("""

-----BEGIN RSA TESTING KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDeSlcT6s/oeoRw
t0AYO2cAmVKpeDG8+y762IPtQgkSZ21nStjEz7NB4twiswBBOh+dqzztZKnXCKNg
9T8Njwu/0wesPAEi7piUGmXwZW2mXrGwX67SLohLUirPyMMaee5p/UW41JEILt50
s464GxT65Lc+pSeC3wumszetpJZAk37KQg041EpOfHZcoCMHmJCIlhwIeX/iryk9
WnoXjTJCxMdSmflc/mP6BJxCo0cQXJ1CMMyK2f31270avBWvZy7gzvBdCQ3DNWYP
YMz7xZnRcvH4nwtx/7n5XC1SaeCJrRFhKFT77TkCwMJLpZJ7FwAgVbArvyYA1O2H
Wui5NkKtAgMBAAECggEAG4ZS//lcYyoAikB2pEl+uJlDng5vAjqMF62FsHQz0V6T
Mm4XJ0+cn7TqkzVc+7apwYk5kx+a1DCSomfbtd8XklocIhyP+3ZV2EjohHrat/YT
xIYkjIwMfl8fQ/lVB0s/1UnyPy+7AatkCklNi8h2sZZuhkhG+zKJK8wXQd4WaMpf
lcIaDijMdu0UTxUO+rISbjVpfL6HswTDUan6LhhxSa9F3zesLqgClKZIqzR8HCtM
83QwK+kiW00D3pVZT4qHfFouoPrszP/qm/17wjxBmk83rKKsF2AnmipBaHR+MHou
tCarJV35//h6z6m0VnAdrYhREif34s8H0pYbKng8oQKBgQD2lCbQu7W/FDu+D4u2
F9wXdjZGplwUHaldfMUsMvawMSt86JYg9yVCHPFnWLhCBYT9Q1B1biqXu5YHwvyi
F/SCVDBaN1pLAkNF5i3McgA2Zw9TbFwinJFpZSa5hSdBiZgpaFZj0KhJqA2ayaSQ
wbTt1aN2oix1wdd9VU7cb5v6GQKBgQDmyJ5JUee6Vc6r/iucO4JkTijzwfPkSqOc
zC7YmcWAE8oTWZf5ozM4vtuUhAyrfiHBaT8uUbyb3+E6MqRrZJmaAPEk9ALOvmZC
vSZD5htzzUsLi7bR7e9PJjXoT+3V1EB3VyHnMv6LCbx/vSs/XI8VrahlDoJAW4rP
UgGE703HtQKBgQCeIaLG6CqFMQejOrsBe0m1biUep9+TMvaDstmMH97eXZojD9H/
sB+fx4n1GguIo5uHBB1cQdtk1XNA5QY5OZ2f2zfrE2Z/hiL4d8ZVP6LtQKiuemaX
98q1SZ5NCZyERiZkH7qPZqgWHIUlCD3Wa7OJdyHOmfBjUH3Ord/WNGlWOQKBgQCv
RLVRoa6HSRuIa6PbJybD3sgjN61uN3FCZ588SKxBtMXHJEfTAyqncet5Q0AMDeK8
7J1bJCBFkSWP+V39YY119Dkvg1GOifNHxDcHYf5/V+4iep0Bmd4hEjfmkq1hs6yx
9a5907CVD3Pk31m06SqRoC0/cmFhVyR4hyM4PjWn8QKBgFz97Xe4VlllQ4v1lY3g
1LXoF3oVBAcIiDOfnJuJKKUNJuQPfp7Z2/gisX/8RDPO+iBqKesUQxTKC2v6MOue
YMR7L8AAn1wBFU5dioARmfBcVWBOpMZIHzHUqsnTqGzuIPTfnaZWxz13PbBxEiGS
+NeMNAdZn3grwXTdcD3VBVHs
-----END RSA TESTING KEY-----
"""u8.Raw()));

internal static @string testingKey(@string s) { return strings.ReplaceAll(s, "TESTING KEY", "PRIVATE KEY"); }

} // end smtp_internal_test_package
