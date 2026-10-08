// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using io = io_package;

partial class cipher_package {

// The Stream* objects are so simple that all their members are public. Users
// can create them themselves.

// StreamReader wraps a [Stream] into an [io.Reader]. It calls XORKeyStream
// to process each slice of data which passes through.
partial struct StreamReader {
    public Stream S;
    public io.Reader R;
}

public static (nint n, error err) Read(this StreamReader r, slice<byte> dst) {
    nint n = default!;
    error err = default!;

    (n, err) = r.R.Read(dst);
    r.S.XORKeyStream(dst.slice(0, n), dst.slice(0, n));
    return (n, err);
}

// StreamWriter wraps a [Stream] into an io.Writer. It calls XORKeyStream
// to process each slice of data which passes through. If any [StreamWriter.Write]
// call returns short then the StreamWriter is out of sync and must be discarded.
// A StreamWriter has no internal buffering; [StreamWriter.Close] does not need
// to be called to flush write data.
partial struct StreamWriter {
    public Stream S;
    public io.Writer W;
    public error Err; // unused
}

public static (nint n, error err) Write(this StreamWriter w, slice<byte> src) {
    nint n = default!;
    error err = default!;

    var c = new slice<byte>(len(src));
    w.S.XORKeyStream(c, src);
    (n, err) = w.W.Write(c);
    if (n != len(src) && err == default!) {
        // should never happen
        err = io.ErrShortWrite;
    }
    return (n, err);
}

// Close closes the underlying Writer and returns its Close return value, if the Writer
// is also an io.Closer. Otherwise it returns nil.
public static error Close(this StreamWriter w) {
    {
        var (c, ok) = w.W._<io.Closer>(ᐧ); if (ok) {
            return c.Close();
        }
    }
    return default!;
}

} // end cipher_package
