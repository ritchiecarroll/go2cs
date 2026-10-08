// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package smtp implements the Simple Mail Transfer Protocol as defined in RFC 5321.
// It also implements the following extensions:
//
//	8BITMIME  RFC 1652
//	AUTH      RFC 2554
//	STARTTLS  RFC 3207
//
// Additional extensions may be handled by clients.
//
// The smtp package is frozen and is not accepting new features.
// Some external packages provide more functionality. See:
//
//	https://godoc.org/?q=smtp
namespace go.net;

using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using textproto = go.net.textproto_package;
using strings = strings_package;
using crypto;
using encoding;
using go.net;
using time = time_package;
using ꓸꓸꓸany = Span<any>;

partial class smtp_package {

// A Client represents a client connection to an SMTP server.
partial struct Client {
    // Text is the textproto.Conn used by the Client. It is exported to allow for
    // clients to add extensions.
    public ж<textproto.Conn> Text;
    // keep a reference to the connection so it can be used to create a TLS
    // connection later
    internal net.Conn conn;
    // whether the Client is using TLS
    internal bool tls;
    internal @string serverName;
    // map of supported extensions
    internal map<@string, @string> ext;
    // supported auth mechanisms
    internal slice<@string> auth;
    internal @string localName; // the name to use in HELO/EHLO
    internal bool didHello;   // whether we've said HELO/EHLO
    internal error helloError;  // the error from the hello
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;

// Dial returns a new [Client] connected to an SMTP server at addr.
// The addr must include a port, as in "mail.example.com:smtp".
public static (ж<Client>, error) Dial(@string addr) {
    var (conn, err) = net.Dial(tcpˢ, addr);
    if (err != default!) {
        return (default!, err);
    }
    var (host, _, _) = net.SplitHostPort(addr);
    return NewClient(conn, host);
}

// NewClient returns a new [Client] using an existing connection and host as a
// server name to be used when authenticating.
public static (ж<Client>, error) NewClient(net.Conn conn, @string host) {
    var text = textproto.NewConn(new net_ConnᴠReadWriteCloser(conn));
    var (_, _, err) = text.of(textproto.Conn.ᏑReader).ReadResponse(220);
    if (err != default!) {
        text.Close();
        return (default!, err);
    }
    var c = Ꮡ(new Client(Text: text, conn: conn, serverName: host, localName: "localhost"u8));
    (_, c.Value.tls) = conn._<ж<tls.Conn>>(ᐧ);
    return (c, default!);
}

// Close closes the connection.
public static error Close(this ref Client c) {
    return c.Text.Close();
}

// hello runs a hello exchange if needed.
internal static error hello(this ж<Client> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!c.didHello) {
        c.didHello = true;
        var err = Ꮡc.ehlo();
        if (err != default!) {
            c.helloError = Ꮡc.helo();
        }
    }
    return c.helloError;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string smtpHelloCalledAfterˢ = "smtp: Hello called after other methods"u8;

// Hello sends a HELO or EHLO to the server as the given host name.
// Calling this method is only necessary if the client needs control
// over the host name used. The client will introduce itself as "localhost"
// automatically otherwise. If Hello is called, it must be called before
// any of the other methods.
public static error Hello(this ж<Client> Ꮡc, @string localName) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var err = validateLine(localName); if (err != default!) {
            return err;
        }
    }
    if (c.didHello) {
        return errors.New(smtpHelloCalledAfterˢ);
    }
    c.localName = localName;
    return Ꮡc.hello();
}

// cmd is a convenience function that sends a command and returns the response
internal static (nint, @string, error) cmd(this ж<Client> Ꮡc, nint expectCode, @string format, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    try {
        var args = argsʗp.sslice();

        ref var c = ref Ꮡc.DerefOrNull();
        var (id, err) = c.Text.Cmd(format, args.ꓸꓸꓸ);
        if (err != default!) {
            return (0, "", err);
        }
        c.Text.of(textproto.Conn.ᏑPipeline).StartResponse(id);
        defer(Ꮡc.Value.Text.of(textproto.Conn.ᏑPipeline).EndResponse, id, ref ᒐ);
        (var code, var msg, err) = c.Text.of(textproto.Conn.ᏑReader).ReadResponse(expectCode);
        return (code, msg, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heloSˢ = "HELO %s"u8;

// helo sends the HELO greeting to the server. It should be used only when the
// server does not support ehlo.
internal static error helo(this ж<Client> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.ext = default!;
    var (_, _, err) = Ꮡc.cmd(250, heloSˢ, c.localName);
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ehloSˢ = "EHLO %s"u8;
internal static readonly @string authˢ = "AUTH"u8;

// ehlo sends the EHLO (extended hello) greeting to the server. It
// should be the preferred greeting for servers that support it.
internal static error ehlo(this ж<Client> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (_, msg, err) = Ꮡc.cmd(250, ehloSˢ, c.localName);
    if (err != default!) {
        return err;
    }
    var ext = new map<@string, @string>();
    var extList = strings.Split(msg, "\n"u8);
    if (len(extList) > 1) {
        extList = extList[1..];
        foreach (var (_, line) in extList) {
            var (k, v, _) = strings.Cut(line, " "u8);
            ext[k] = v;
        }
    }
    {
        var (mechs, ok) = ext[authˢ, ꟷ]; if (ok) {
            c.auth = strings.Split(mechs, " "u8);
        }
    }
    c.ext = ext;
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string starttlsˢ = "STARTTLS"u8;

// StartTLS sends the STARTTLS command and encrypts all further communication.
// Only servers that advertise the STARTTLS extension support this function.
public static error StartTLS(this ж<Client> Ꮡc, ж<tls.Config> Ꮡconfig) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var errΔ1 = Ꮡc.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = Ꮡc.cmd(220, starttlsˢ);
    if (err != default!) {
        return err;
    }
    c.conn = new tls.ConnжConn(tls.Client(c.conn, Ꮡconfig));
    c.Text = textproto.NewConn(new net_ConnᴠReadWriteCloser(c.conn));
    c.tls = true;
    return Ꮡc.ehlo();
}

// TLSConnectionState returns the client's TLS connection state.
// The return values are their zero values if [Client.StartTLS] did
// not succeed.
public static (tlsꓸConnectionState state, bool ok) TLSConnectionState(this ref Client c) {
    tlsꓸConnectionState state = default!;
    bool ok = default!;

    (var tc, ok) = c.conn._<ж<tls.Conn>>(ᐧ);
    if (!ok) {
        return (state, ok);
    }
    return (tc.ConnectionState(), true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vrfySˢ = "VRFY %s"u8;

// Verify checks the validity of an email address on the server.
// If Verify returns nil, the address is valid. A non-nil return
// does not necessarily indicate an invalid address. Many servers
// will not verify addresses for security reasons.
public static error Verify(this ж<Client> Ꮡc, @string addr) {
    {
        var errΔ1 = validateLine(addr); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = Ꮡc.hello(); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    var (_, _, err) = Ꮡc.cmd(250, vrfySˢ, addr);
    return err;
}

// Auth authenticates a client using the provided authentication mechanism.
// A failed authentication closes the connection.
// Only servers that advertise the AUTH extension support this function.
public static error Auth(this ж<Client> Ꮡc, ΔAuth a) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var errΔ1 = Ꮡc.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var encoding = base64.StdEncoding;
    var (mech, resp, err) = a.Start(Ꮡ(new ServerInfo(c.serverName, c.tls, c.auth)));
    if (err != default!) {
        Ꮡc.Quit();
        return err;
    }
    var resp64 = new slice<byte>(encoding.EncodedLen(len(resp)));
    encoding.Encode(resp64, resp);
    ref var code = ref heap<nint>(out var Ꮡcode);
    ref var msg64 = ref heap<@string>(out var Ꮡmsg64);
    (code, msg64, err) = Ꮡc.cmd(0, "%s"u8, strings.TrimSpace(fmt.Sprintf("AUTH %s %s"u8, mech, resp64)));
    while (err == default!) {
        slice<byte> msg = default!;
        switch (code) {
        case 334: {
            (msg, err) = encoding.DecodeString(msg64);
            break;
        }
        case 235: {
            msg = slice<byte>(msg64);
            break;
        }
        default: {
            err = new textproto.ΔErrorжerror(Ꮡ(new textprotoꓸError( // the last message isn't base64 because it isn't a challenge
Code: code, Msg: msg64)));
            break;
        }}

        if (err == default!) {
            (resp, err) = a.Next(msg, code == 334);
        }
        if (err != default!) {
            // abort the AUTH
            Ꮡc.cmd(501, "*"u8);
            Ꮡc.Quit();
            break;
        }
        if (resp == default!) {
            break;
        }
        resp64 = new slice<byte>(encoding.EncodedLen(len(resp)));
        encoding.Encode(resp64, resp);
        (code, msg64, err) = Ꮡc.cmd(0, "%s"u8, resp64);
    }
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mailFromSˢ = "MAIL FROM:<%s>"u8;
internal static readonly @string smtputf8ˢ = "SMTPUTF8"u8;

// Mail issues a MAIL command to the server using the provided email address.
// If the server supports the 8BITMIME extension, Mail adds the BODY=8BITMIME
// parameter. If the server supports the SMTPUTF8 extension, Mail adds the
// SMTPUTF8 parameter.
// This initiates a mail transaction and is followed by one or more [Client.Rcpt] calls.
public static error Mail(this ж<Client> Ꮡc, @string from) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var errΔ1 = validateLine(from); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = Ꮡc.hello(); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    @string cmdStr = mailFromSˢ;
    if (c.ext != default!) {
        {
            var (_, ok) = c.ext["8BITMIME"u8, ꟷ]; if (ok) {
                cmdStr += " BODY=8BITMIME"u8;
            }
        }
        {
            var (_, ok) = c.ext[smtputf8ˢ, ꟷ]; if (ok) {
                cmdStr += " SMTPUTF8"u8;
            }
        }
    }
    var (_, _, err) = Ꮡc.cmd(250, cmdStr, from);
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rcptToSˢ = "RCPT TO:<%s>"u8;

// Rcpt issues a RCPT command to the server using the provided email address.
// A call to Rcpt must be preceded by a call to [Client.Mail] and may be followed by
// a [Client.Data] call or another Rcpt call.
public static error Rcpt(this ж<Client> Ꮡc, @string to) {
    {
        var errΔ1 = validateLine(to); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = Ꮡc.cmd(25, rcptToSˢ, to);
    return err;
}

partial struct dataCloser {
    internal ж<Client> c;
    /*embed*/ public io_package.WriteCloser WriteCloser;
}

// Go method set entry for the promoted 'WriteCloser.Write()' - provided ONLY by the embedded
// interface field in *dataCloser's method set; see the pointer-only satisfaction record.
internal static (nint, error) Write(this dataCloser recvᴛ, slice<byte> p) => recvᴛ.WriteCloser.Write(p);

internal static error Close(this ref dataCloser d) {
    d.WriteCloser.Close();
    var (_, _, err) = (~d.c).Text.of(textproto.Conn.ᏑReader).ReadResponse(250);
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dataˢ = "DATA"u8;

// Data issues a DATA command to the server and returns a writer that
// can be used to write the mail headers and body. The caller should
// close the writer before calling any more methods on c. A call to
// Data must be preceded by one or more calls to [Client.Rcpt].
public static (io.WriteCloser, error) Data(this ж<Client> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (_, _, err) = Ꮡc.cmd(354, dataˢ);
    if (err != default!) {
        return (default!, err);
    }
    return (new dataCloserжWriteCloser(Ꮡ(new dataCloser(Ꮡc, c.Text.of(textproto.Conn.ᏑWriter).DotWriter()))), default!);
}

internal static Action<ж<tls.Config>> testHookStartTLS;      // nil, except for tests

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string smtpServerDoesnTSupportˢ = "smtp: server doesn't support AUTH"u8;

// SendMail connects to the server at addr, switches to TLS if
// possible, authenticates with the optional mechanism a if possible,
// and then sends an email from address from, to addresses to, with
// message msg.
// The addr must include a port, as in "mail.example.com:smtp".
//
// The addresses in the to parameter are the SMTP RCPT addresses.
//
// The msg parameter should be an RFC 822-style email with headers
// first, a blank line, and then the message body. The lines of msg
// should be CRLF terminated. The msg headers should usually include
// fields such as "From", "To", "Subject", and "Cc".  Sending "Bcc"
// messages is accomplished by including an email address in the to
// parameter but not including it in the msg headers.
//
// The SendMail function and the net/smtp package are low-level
// mechanisms and provide no support for DKIM signing, MIME
// attachments (see the mime/multipart package), or other mail
// functionality. Higher-level packages exist outside of the standard
// library.
public static error SendMail(@string addr, ΔAuth a, @string from, slice<@string> to, slice<byte> msg) {
    GoFrame ᒐ = default;
    try {
        {
            var errΔ1 = validateLine(from); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        foreach (var (_, recp) in to) {
            {
                var errΔ2 = validateLine(recp); if (errΔ2 != default!) {
                    return errΔ2;
                }
            }
        }
        var (c, err) = Dial(addr);
        if (err != default!) {
            return err;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            err = c.hello(); if (err != default!) {
                return err;
            }
        }
        {
            var (ok, _) = c.Extension(starttlsˢ); if (ok) {
                var config = Ꮡ(new tls.Config(ServerName: (~c).serverName));
                if (testHookStartTLS != default!) {
                    testHookStartTLS(config);
                }
                {
                    err = c.StartTLS(config); if (err != default!) {
                        return err;
                    }
                }
            }
        }
        if (a != default! && (~c).ext != default!) {
            {
                var (_, ok) = (~c).ext[authˢ, ꟷ]; if (!ok) {
                    return errors.New(smtpServerDoesnTSupportˢ);
                }
            }
            {
                err = c.Auth(a); if (err != default!) {
                    return err;
                }
            }
        }
        {
            err = c.Mail(from); if (err != default!) {
                return err;
            }
        }
        foreach (var (_, addrΔ1) in to) {
            {
                err = c.Rcpt(addrΔ1); if (err != default!) {
                    return err;
                }
            }
        }
        (var w, err) = c.Data();
        if (err != default!) {
            return err;
        }
        (_, err) = w.Write(msg);
        if (err != default!) {
            return err;
        }
        err = w.Close();
        if (err != default!) {
            return err;
        }
        return c.Quit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Extension reports whether an extension is support by the server.
// The extension name is case-insensitive. If the extension is supported,
// Extension also returns a string that contains any parameters the
// server specifies for the extension.
public static (bool, @string) Extension(this ж<Client> Ꮡc, @string ext) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var err = Ꮡc.hello(); if (err != default!) {
            return (false, "");
        }
    }
    if (c.ext == default!) {
        return (false, "");
    }
    ext = strings.ToUpper(ext);
    var (param, ok) = c.ext[ext, ꟷ];
    return (ok, param);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsetˢ = "RSET"u8;

// Reset sends the RSET command to the server, aborting the current mail
// transaction.
public static error Reset(this ж<Client> Ꮡc) {
    {
        var errΔ1 = Ꮡc.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = Ꮡc.cmd(250, rsetˢ);
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noopˢ = "NOOP"u8;

// Noop sends the NOOP command to the server. It does nothing but check
// that the connection to the server is okay.
public static error Noop(this ж<Client> Ꮡc) {
    {
        var errΔ1 = Ꮡc.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = Ꮡc.cmd(250, noopˢ);
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string quitˢ = "QUIT"u8;

// Quit sends the QUIT command and closes the connection to the server.
public static error Quit(this ж<Client> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.hello(); // ignore error; we're quitting anyhow
    var (_, _, err) = Ꮡc.cmd(221, quitˢ);
    if (err != default!) {
        return err;
    }
    return c.Text.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string smtpALineMustNotContainˢ = "smtp: A line must not contain CR or LF"u8;

// validateLine checks to see if a line has CR or LF as per RFC 5321.
internal static error validateLine(@string line) {
    if (strings.ContainsAny(line, "\n\r"u8)) {
        return errors.New(smtpALineMustNotContainˢ);
    }
    return default!;
}

} // end smtp_package
