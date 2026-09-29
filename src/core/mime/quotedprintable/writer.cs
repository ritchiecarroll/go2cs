// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using io = io_package;

partial class quotedprintable_package {

internal static UntypedInt lineMaxLen => 76;

// A Writer is a quoted-printable writer that implements [io.WriteCloser].
[GoType] partial struct Writer {
    // Binary mode treats the writer's input as pure binary and processes end of
    // line bytes as binary data.
    public bool Binary;
    internal io.Writer w;
    internal nint i;
    internal array<byte> line = new(78);
    internal bool cr;
}

// NewWriter returns a new [Writer] that writes to w.
public static ж<Writer> NewWriter(io.Writer w) {
    return Ꮡ(new Writer(w: w));
}

// Write encodes p using quoted-printable encoding and writes it to the
// underlying [io.Writer]. It limits line length to 76 characters. The encoded
// bytes are not necessarily flushed until the [Writer] is closed.
[GoRecv] public static (nint n, error err) Write(this ref Writer w, slice<byte> p) {
    nint n = default!;

    foreach (var (i, b) in p) {
        switch (ᐧ) {
        case {} when b >= (rune)'!' && b <= (rune)'~' && b != (rune)'=': {
            continue;
            break;
        }
        case {} when isWhitespace(b) || !w.Binary && (b == (rune)'\n' || b == (rune)'\r'): {
            continue;
            break;
        }}

        // Simple writes are done in batch.
        if (i > n) {
            {
                var errΔ1 = w.write(p.slice(n, i)); if (errΔ1 != default!) {
                    return (n, errΔ1);
                }
            }
            n = i;
        }
        {
            var errΔ2 = w.encode(b); if (errΔ2 != default!) {
                return (n, errΔ2);
            }
        }
        n++;
    }
    if (n == len(p)) {
        return (n, default!);
    }
    {
        var errΔ3 = w.write(p.slice(n)); if (errΔ3 != default!) {
            return (n, errΔ3);
        }
    }
    return (len(p), default!);
}

// Close closes the [Writer], flushing any unwritten data to the underlying
// [io.Writer], but does not close the underlying io.Writer.
[GoRecv] public static error Close(this ref Writer w) {
    {
        var err = w.checkLastByte(); if (err != default!) {
            return err;
        }
    }
    return w.flush();
}

// write limits text encoded in quoted-printable to 76 characters per line.
[GoRecv] internal static error write(this ref Writer w, slice<byte> p) {
    foreach (var (_, b) in p) {
        if (b == (rune)'\n' || b == (rune)'\r') {
            // If the previous byte was \r, the CRLF has already been inserted.
            if (w.cr && b == (rune)'\n') {
                w.cr = false;
                continue;
            }
            if (b == (rune)'\r') {
                w.cr = true;
            }
            {
                var err = w.checkLastByte(); if (err != default!) {
                    return err;
                }
            }
            {
                var err = w.insertCRLF(); if (err != default!) {
                    return err;
                }
            }
            continue;
        }
        if (w.i == (nint)(lineMaxLen - 1)) {
            {
                var err = w.insertSoftLineBreak(); if (err != default!) {
                    return err;
                }
            }
        }
        w.line[w.i] = b;
        w.i++;
        w.cr = false;
    }
    return default!;
}

[GoRecv] internal static error encode(this ref Writer w, byte b) {
    if ((nint)(lineMaxLen - 1) - w.i < 3) {
        {
            var err = w.insertSoftLineBreak(); if (err != default!) {
                return err;
            }
        }
    }
    w.line[w.i] = (rune)'=';
    w.line[w.i + 1] = upperhex[(b >> (int)(4))];
    w.line[w.i + 2] = upperhex[(byte)(b & 0x0f)];
    w.i += 3;
    return default!;
}

internal static readonly @string upperhex = "0123456789ABCDEF"u8;

// checkLastByte encodes the last buffered byte if it is a space or a tab.
[GoRecv] internal static error checkLastByte(this ref Writer w) {
    if (w.i == 0) {
        return default!;
    }
    var b = w.line[w.i - 1];
    if (isWhitespace(b)) {
        w.i--;
        {
            var err = w.encode(b); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

[GoRecv] internal static error insertSoftLineBreak(this ref Writer w) {
    w.line[w.i] = (rune)'=';
    w.i++;
    return w.insertCRLF();
}

[GoRecv] internal static error insertCRLF(this ref Writer w) {
    w.line[w.i] = (rune)'\r';
    w.line[w.i + 1] = (rune)'\n';
    w.i += 2;
    return w.flush();
}

[GoRecv] internal static error flush(this ref Writer w) {
    {
        var (_, err) = w.w.Write(w.line.slice(0, w.i)); if (err != default!) {
            return err;
        }
    }
    w.i = 0;
    return default!;
}

internal static bool isWhitespace(byte b) {
    return b == (rune)' ' || b == (rune)'\t';
}

} // end quotedprintable_package
