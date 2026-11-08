// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using fmt = fmt_package;
using log = log_package;
using smtp = go.net.smtp_package;
using go.net;
using io = io_package;
using static go.net.smtp_internal_test_package;

partial class smtp_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mailExampleCom25ˢ => ref mailExampleCom25ˢᵇ;
private static readonly @string mailExampleCom25ˢᵇ = "mail.example.com:25"u8;
internal static ref readonly @string senderExampleOrgˢ => ref senderExampleOrgˢᵇ;
private static readonly @string senderExampleOrgˢᵇ = "sender@example.org"u8;
internal static ref readonly @string recipientExampleNetˢ => ref recipientExampleNetˢᵇ;
private static readonly @string recipientExampleNetˢᵇ = "recipient@example.net"u8;

public static void Example() {
    // Connect to the remote SMTP server.
    var (c, err) = smtp.Dial(mailExampleCom25ˢ);
    if (err != nil) {
        log.Fatal(err);
    }

    // Set the sender and recipient first
    var errΔ1 = c.Mail(senderExampleOrgˢ); if (errΔ1 != nil) {
        log.Fatal(errΔ1);
    }
    var errΔ2 = c.Rcpt(recipientExampleNetˢ); if (errΔ2 != nil) {
        log.Fatal(errΔ2);
    }

    // Send the email body.
    (var wc, err) = c.Data();
    if (err != nil) {
        log.Fatal(err);
    }
    (_, err) = fmt.Fprintf(wc, "This is the email body"u8);
    if (err != nil) {
        log.Fatal(err);
    }
    err = wc.Close();
    if (err != nil) {
        log.Fatal(err);
    }

    // Send the QUIT command and close the connection.
    err = c.Quit();
    if (err != nil) {
        log.Fatal(err);
    }
}

// variables to make ExamplePlainAuth compile, without adding
// unnecessary noise there.
internal static @string from = "gopher@example.net";
internal static slice<byte> msg = slice<byte>("dummy message"u8);
internal static slice<@string> recipients = ["foo@example.com"];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mailExampleComˢ => ref mailExampleComˢᵇ;
private static readonly @string mailExampleComˢᵇ = "mail.example.com"u8;
internal static ref readonly @string userExampleComˢ => ref userExampleComˢᵇ;
private static readonly @string userExampleComˢᵇ = "user@example.com"u8;
internal static ref readonly @string passwordˢ => ref passwordˢᵇ;
private static readonly @string passwordˢᵇ = "password"u8;

public static void ExamplePlainAuth() {
    // hostname is used by PlainAuth to validate the TLS certificate.
    @string hostname = mailExampleComˢ;
    var auth = smtp.PlainAuth(""u8, userExampleComˢ, passwordˢ, hostname);

    var err = smtp.SendMail(hostname + ":25"u8, auth, from, recipients, msg);
    if (err != nil) {
        log.Fatal(err);
    }
}

public static void ExampleSendMail() {
    // Set up authentication information.
    var auth = smtp.PlainAuth(""u8, userExampleComˢ, passwordˢ, mailExampleComˢ);

    // Connect to the server, authenticate, set the sender and recipient,
    // and send the email all in one step.
    var to = slice<@string>(["recipient@example.net"]);
    var msg = slice<byte>((@string)("To: recipient@example.net\r\n"u8 + "Subject: discount Gophers!\r\n"u8 + "\r\n"u8 + "This is the email body.\r\n"u8));
    var err = smtp.SendMail(mailExampleCom25ˢ, auth, senderExampleOrgˢ, to, msg);
    if (err != nil) {
        log.Fatal(err);
    }
}

} // end smtp_test_package
