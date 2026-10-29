// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using io = io_package;

partial class quotedprintable_package {

internal static UntypedInt lineMaxLen => 76;

// A Writer is a quoted-printable writer that implements [io.WriteCloser].
partial struct Writer {
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
public static (nint n, error err) Write(this ref Writer w, slice<byte> p) {
    nint n = default;

    foreach (var (i, b) in p) {
        switch (ᐧ) {
        // Simple writes are done in batch.
        case {} when b >= (byte)'!' && b <= (byte)'~' && b != (byte)'=': {
            continue;
            break;
        }
        case {} when isWhitespace(b) || !w.Binary && (b == (byte)'\n' || b == (byte)'\r'): {
            continue;
            break;
        }}

        if (i > n) {
            {
                var errΔ1 = w.write(p.slice(n, i)); if (errΔ1 != nil) {
                    return (n, errΔ1);
                }
            }
            n = i;
        }

        {
            var errΔ2 = w.encode(b); if (errΔ2 != nil) {
                return (n, errΔ2);
            }
        }
        n++;
    }

    if (n == len(p)) {
        return (n, default);
    }

    {
        var errΔ3 = w.write(p.slice(n)); if (errΔ3 != nil) {
            return (n, errΔ3);
        }
    }

    return (len(p), default);
}

// Close closes the [Writer], flushing any unwritten data to the underlying
// [io.Writer], but does not close the underlying io.Writer.
public static error Close(this ref Writer w) {
    {
        var err = w.checkLastByte(); if (err != nil) {
            return err;
        }
    }

    return w.flush();
}

// write limits text encoded in quoted-printable to 76 characters per line.
internal static error write(this ref Writer w, slice<byte> p) {
    foreach (var (_, b) in p) {
        if (b == '\n' || b == '\r') {
            // If the previous byte was \r, the CRLF has already been inserted.
            if (w.cr && b == '\n') {
                w.cr = false;
                continue;
            }

            if (b == '\r') {
                w.cr = true;
            }

            {
                var err = w.checkLastByte(); if (err != nil) {
                    return err;
                }
            }
            {
                var err = w.insertCRLF(); if (err != nil) {
                    return err;
                }
            }
            continue;
        }

        if (w.i == (nint)(lineMaxLen - 1)) {
            {
                var err = w.insertSoftLineBreak(); if (err != nil) {
                    return err;
                }
            }
        }

        w.line[w.i] = b;
        w.i++;
        w.cr = false;
    }

    return default;
}

internal static error encode(this ref Writer w, byte b) {
    if ((nint)(lineMaxLen - 1) - w.i < 3) {
        {
            var err = w.insertSoftLineBreak(); if (err != nil) {
                return err;
            }
        }
    }

    w.line[w.i] = (byte)'=';
    w.line[w.i + 1] = upperhex[(b >> 4)];
    w.line[w.i + 2] = upperhex[(byte)(b & 0x0f)];
    w.i += 3;

    return default;
}

internal static readonly @string upperhex = "0123456789ABCDEF"u8;

// checkLastByte encodes the last buffered byte if it is a space or a tab.
internal static error checkLastByte(this ref Writer w) {
    if (w.i == 0) {
        return default;
    }

    var b = w.line[w.i - 1];
    if (isWhitespace(b)) {
        w.i--;
        {
            var err = w.encode(b); if (err != nil) {
                return err;
            }
        }
    }

    return default;
}

internal static error insertSoftLineBreak(this ref Writer w) {
    w.line[w.i] = (byte)'=';
    w.i++;

    return w.insertCRLF();
}

internal static error insertCRLF(this ref Writer w) {
    w.line[w.i] = (byte)'\r';
    w.line[w.i + 1] = (byte)'\n';
    w.i += 2;

    return w.flush();
}

internal static error flush(this ref Writer w) {
    {
        var (_, err) = w.w.Write(w.line.slice(0, w.i)); if (err != nil) {
            return err;
        }
    }

    w.i = 0;
    return default;
}

internal static bool isWhitespace(byte b) {
    return b == ' ' || b == '\t';
}

} // end quotedprintable_package
