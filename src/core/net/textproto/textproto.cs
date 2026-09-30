// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package textproto implements generic support for text-based request/response
// protocols in the style of HTTP, NNTP, and SMTP.
//
// The package provides:
//
// [Error], which represents a numeric error response from
// a server.
//
// [Pipeline], to manage pipelined requests and responses
// in a client.
//
// [Reader], to read numeric response code lines,
// key: value headers, lines wrapped with leading spaces
// on continuation lines, and whole text blocks ending
// with a dot on a line by itself.
//
// [Writer], to write dot-encoded text blocks.
//
// [Conn], a convenient packaging of [Reader], [Writer], and [Pipeline] for use
// with a single network connection.
namespace go.net;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using ꓸꓸꓸany = Span<any>;

partial class textproto_package {

// An Error represents a numeric error response from a server.
[GoType] partial struct ΔError {
    public nint Code;
    public @string Msg;
}

[GoRecv] public static @string Error(this ref ΔError e) {
    return fmt.Sprintf("%03d %s"u8, e.Code, e.Msg);
}

[GoType("@string")] partial struct ProtocolError;

public static @string Error(this ProtocolError p) {
    return ((@string)p);
}

// A Conn represents a textual network protocol connection.
// It consists of a [Reader] and [Writer] to manage I/O
// and a [Pipeline] to sequence concurrent requests on the connection.
// These embedded types carry methods with them;
// see the documentation of those types for details.
[GoType] partial struct Conn {
    public partial ref Reader Reader { get; }
    public partial ref Writer Writer { get; }
    public partial ref Pipeline Pipeline { get; }
    internal io.ReadWriteCloser conn;
}

// NewConn returns a new [Conn] using conn for I/O.
public static ж<Conn> NewConn(io.ReadWriteCloser conn) {
    return Ꮡ(new Conn(
        Reader: new Reader(R: bufio.NewReader(new io_ReadWriteCloserᴠReader(conn))),
        Writer: new Writer(W: bufio.NewWriter(new io_ReadWriteCloserᴠWriter(conn))),
        conn: conn
    ));
}

// Close closes the connection.
[GoRecv] public static error Close(this ref Conn c) {
    return c.conn.Close();
}

// Dial connects to the given address on the given network using [net.Dial]
// and then returns a new [Conn] for the connection.
public static (ж<Conn>, error) Dial(@string network, @string addr) {
    var (c, err) = net.Dial(network, addr);
    if (err != default!) {
        return (default!, err);
    }
    return (NewConn(new net_ConnᴠReadWriteCloser(c)), default!);
}

// Cmd is a convenience method that sends a command after
// waiting its turn in the pipeline. The command text is the
// result of formatting format with args and appending \r\n.
// Cmd returns the id of the command, for use with StartResponse and EndResponse.
//
// For example, a client might run a HELP command that returns a dot-body
// by using:
//
//	id, err := c.Cmd("HELP")
//	if err != nil {
//		return nil, err
//	}
//
//	c.StartResponse(id)
//	defer c.EndResponse(id)
//
//	if _, _, err = c.ReadCodeLine(110); err != nil {
//		return nil, err
//	}
//	text, err := c.ReadDotBytes()
//	if err != nil {
//		return nil, err
//	}
//	return c.ReadCodeLine(250)
public static (nuint id, error err) Cmd(this ж<Conn> Ꮡc, @string format, params ꓸꓸꓸany argsʗp) {
    nuint id = default!;
    error err = default!;
    var args = argsʗp.sslice();

    id = Ꮡc.of(Conn.ᏑPipeline).Next();
    Ꮡc.of(Conn.ᏑPipeline).StartRequest(id);
    err = Ꮡc.of(Conn.ᏑWriter).PrintfLine(format, args.ꓸꓸꓸ);
    Ꮡc.of(Conn.ᏑPipeline).EndRequest(id);
    if (err != default!) {
        return (0, err);
    }
    return (id, default!);
}

// TrimString returns s without leading and trailing ASCII space.
public static @string TrimString(@string s) {
    while (len(s) > 0 && isASCIISpace(s[0])) {
        s = s[1..];
    }
    while (len(s) > 0 && isASCIISpace(s[len(s) - 1])) {
        s = s.slice(0, len(s) - 1);
    }
    return s;
}

// TrimBytes returns b without leading and trailing ASCII space.
public static slice<byte> TrimBytes(slice<byte> b) {
    while (len(b) > 0 && isASCIISpace(b[0])) {
        b = b[1..];
    }
    while (len(b) > 0 && isASCIISpace(b[len(b) - 1])) {
        b = b.slice(0, len(b) - 1);
    }
    return b;
}

internal static bool isASCIISpace(byte b) {
    return b == (rune)' ' || b == (rune)'\t' || b == (rune)'\n' || b == (rune)'\r';
}

internal static bool isASCIILetter(byte b) {
    b |= (byte)(0x20); // make lower case
    return (rune)'a' <= b && b <= (rune)'z';
}

} // end textproto_package
