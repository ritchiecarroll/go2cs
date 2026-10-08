// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using ꓸꓸꓸany = Span<any>;

partial class textproto_package {

// A Writer implements convenience methods for writing
// requests or responses to a text protocol network connection.
partial struct Writer {
    public ж<bufio.Writer> W;
    internal ж<dotWriter> dot;
}

// NewWriter returns a new [Writer] writing to w.
public static ж<Writer> NewWriter(ж<bufio.Writer> Ꮡw) {
    return Ꮡ(new Writer(W: Ꮡw));
}

internal static slice<byte> crnl = new byte[]{(rune)'\r', (rune)'\n'}.slice();

internal static slice<byte> dotcrnl = new byte[]{(rune)'.', (rune)'\r', (rune)'\n'}.slice();

// PrintfLine writes the formatted output followed by \r\n.
public static error PrintfLine(this ref Writer w, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    w.closeDot();
    fmt.Fprintf(new bufio_WriterжWriter(w.W), format, args.ꓸꓸꓸ);
    w.W.Write(crnl);
    return w.W.Flush();
}

// DotWriter returns a writer that can be used to write a dot-encoding to w.
// It takes care of inserting leading dots when necessary,
// translating line-ending \n into \r\n, and adding the final .\r\n line
// when the DotWriter is closed. The caller should close the
// DotWriter before the next call to a method on w.
//
// See the documentation for the [Reader.DotReader] method for details about dot-encoding.
public static io.WriteCloser DotWriter(this ж<Writer> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    w.closeDot();
    w.dot = Ꮡ(new dotWriter(w: Ꮡw));
    return new dotWriterжWriteCloser(w.dot);
}

internal static void closeDot(this ref Writer w) {
    if (w.dot != nil) {
        w.dot.Close(); // sets w.dot = nil
    }
}

partial struct dotWriter {
    internal ж<Writer> w;
    internal nint state;
}

internal static UntypedInt wstateBegin => iota; // initial state; must be zero
internal static UntypedInt wstateBeginLine => 1; // beginning of line
internal static UntypedInt wstateCR => 2; // wrote \r (possibly at end of line)
internal static UntypedInt wstateData => 3; // writing data in middle of line

internal static (nint n, error err) Write(this ref dotWriter d, slice<byte> b) {
    nint n = default!;
    error err = default!;

    var bw = d.w.Value.W;
    while (n < len(b)) {
        var c = b[n];
        var exprᴛ1 = d.state;
        var matchᴛ1 = false;
        if (exprᴛ1 == wstateBegin || exprᴛ1 == wstateBeginLine) { matchᴛ1 = true;
            d.state = wstateData;
            if (c == (rune)'.') {
                // escape leading dot
                bw.WriteByte((rune)'.');
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == wstateData) {
            if (c == (rune)'\r') {
                d.state = wstateCR;
            }
            if (c == (rune)'\n') {
                bw.WriteByte((rune)'\r');
                d.state = wstateBeginLine;
            }
        }
        else if (exprᴛ1 == wstateCR) { matchᴛ1 = true;
            d.state = wstateData;
            if (c == (rune)'\n') {
                d.state = wstateBeginLine;
            }
        }

        {
            err = bw.WriteByte(c); if (err != default!) {
                break;
            }
        }
        n++;
    }
    return (n, err);
}

internal static error Close(this ж<dotWriter> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    if ((~d.w).dot == Ꮡd) {
        d.w.Value.dot = default!;
    }
    var bw = d.w.Value.W;
    var exprᴛ1 = d.state;
    var matchᴛ1 = false;
    var matchᴛ2 = exprᴛ1 == wstateCR || exprᴛ1 == wstateBeginLine;
    if (!matchᴛ2) { /* default: */
        bw.WriteByte((rune)'\r');
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == wstateCR) {
        bw.WriteByte((rune)'\n');
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == wstateBeginLine) { matchᴛ1 = true;
        bw.Write(dotcrnl);
    }

    return bw.Flush();
}

} // end textproto_package
