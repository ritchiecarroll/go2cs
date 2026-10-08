// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package iotest implements Readers and Writers useful mainly for testing.
namespace go.testing;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;

partial class iotest_package {

// OneByteReader returns a Reader that implements
// each non-empty Read by reading one byte from r.
public static io.Reader OneByteReader(io.Reader r) {
    return new oneByteReaderжReader(Ꮡ(new oneByteReader(r)));
}

partial struct oneByteReader {
    internal io.Reader r;
}

internal static (nint, error) Read(this ref oneByteReader r, slice<byte> p) {
    if (len(p) == 0) {
        return (0, default!);
    }
    return r.r.Read(p[0..1]);
}

// HalfReader returns a Reader that implements Read
// by reading half as many requested bytes from r.
public static io.Reader HalfReader(io.Reader r) {
    return new halfReaderжReader(Ꮡ(new halfReader(r)));
}

partial struct halfReader {
    internal io.Reader r;
}

internal static (nint, error) Read(this ref halfReader r, slice<byte> p) {
    return r.r.Read(p.slice(0, (len(p) + 1) / 2));
}

// DataErrReader changes the way errors are handled by a Reader. Normally, a
// Reader returns an error (typically EOF) from the first Read call after the
// last piece of data is read. DataErrReader wraps a Reader and changes its
// behavior so the final error is returned along with the final data, instead
// of in the first call after the final data.
public static io.Reader DataErrReader(io.Reader r) {
    return new dataErrReaderжReader(Ꮡ(new dataErrReader(r, default!, new slice<byte>(1024))));
}

partial struct dataErrReader {
    internal io.Reader r;
    internal slice<byte> unread;
    internal slice<byte> data;
}

internal static (nint n, error err) Read(this ref dataErrReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    // loop because first call needs two reads:
    // one to get data and a second to look for an error.
    while (ᐧ) {
        if (len(r.unread) == 0) {
            var (n1, err1) = r.r.Read(r.data);
            r.unread = r.data.slice(0, n1);
            err = err1;
        }
        if (n > 0 || err != default!) {
            break;
        }
        n = copy(p, r.unread);
        r.unread = r.unread.slice(n);
    }
    return (n, err);
}

// ErrTimeout is a fake timeout error.
public static error ErrTimeout = errors.New("timeout"u8);

// TimeoutReader returns [ErrTimeout] on the second read
// with no data. Subsequent calls to read succeed.
public static io.Reader TimeoutReader(io.Reader r) {
    return new timeoutReaderжReader(Ꮡ(new timeoutReader(r, 0)));
}

partial struct timeoutReader {
    internal io.Reader r;
    internal nint count;
}

internal static (nint, error) Read(this ref timeoutReader r, slice<byte> p) {
    r.count++;
    if (r.count == 2) {
        return (0, ErrTimeout);
    }
    return r.r.Read(p);
}

// ErrReader returns an [io.Reader] that returns 0, err from all Read calls.
public static io.Reader ErrReader(error err) {
    return new errReaderжReader(Ꮡ(new errReader(err: err)));
}

partial struct errReader {
    internal error err;
}

internal static (nint, error) Read(this ref errReader r, slice<byte> p) {
    return (0, r.err);
}

partial struct smallByteReader {
    internal io.Reader r;
    internal nint off;
    internal nint n;
}

internal static (nint, error) Read(this ref smallByteReader r, slice<byte> p) {
    if (len(p) == 0) {
        return (0, default!);
    }
    r.n = r.n % 3 + 1;
    nint n = r.n;
    if (n > len(p)) {
        n = len(p);
    }
    (n, var err) = r.r.Read(p.slice(0, n));
    if (err != default! && !AreEqual(err, io.EOF)) {
        err = fmt.Errorf("Read(%d bytes at offset %d): %v"u8, n, r.off, err);
    }
    r.off += n;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "nil"u8;
internal static readonly @string nilOrEofˢ = "nil or EOF"u8;

// TestReader tests that reading from r returns the expected file content.
// It does reads of different sizes, until EOF.
// If r implements [io.ReaderAt] or [io.Seeker], TestReader also checks
// that those operations behave as they should.
//
// If TestReader finds any misbehaviors, it returns an error reporting them.
// The error text may span multiple lines.
public static error TestReader(io.Reader r, slice<byte> content) {
    if (len(content) > 0) {
        var (nΔ1, errΔ1) = r.Read(default!);
        if (nΔ1 != 0 || errΔ1 != default!) {
            return fmt.Errorf("Read(0) = %d, %v, want 0, nil"u8, nΔ1, errΔ1);
        }
    }
    var (data, err) = io.ReadAll(new smallByteReaderжReader(Ꮡ(new smallByteReader(r: r))));
    if (err != default!) {
        return err;
    }
    if (!bytes.Equal(data, content)) {
        return fmt.Errorf("ReadAll(small amounts) = %q\n\twant %q"u8, data, content);
    }
    (var n, err) = r.Read(new slice<byte>(10));
    if (n != 0 || !AreEqual(err, io.EOF)) {
        return fmt.Errorf("Read(10) at EOF = %v, %v, want 0, EOF"u8, n, err);
    }
    {
        var (rΔ1, ok) = r._<io.ReadSeeker>(ᐧ); if (ok) {
            // Seek(0, 1) should report the current file position (EOF).
            {
                var (off, errΔ2) = rΔ1.Seek(0, 1); if (off != (int64)len(content) || errΔ2 != default!) {
                    return fmt.Errorf("Seek(0, 1) from EOF = %d, %v, want %d, nil"u8, off, errΔ2, len(content));
                }
            }
            // Seek backward partway through file, in two steps.
            // If middle == 0, len(content) == 0, can't use the -1 and +1 seeks.
            nint middle = len(content) - len(content) / 3;
            if (middle > 0) {
                {
                    var (off, errΔ3) = rΔ1.Seek(-1, 1); if (off != (int64)(len(content) - 1) || errΔ3 != default!) {
                        return fmt.Errorf("Seek(-1, 1) from EOF = %d, %v, want %d, nil"u8, -off, errΔ3, len(content) - 1);
                    }
                }
                {
                    var (off, errΔ4) = rΔ1.Seek((int64)(-len(content) / 3), 1); if (off != (int64)(middle - 1) || errΔ4 != default!) {
                        return fmt.Errorf("Seek(%d, 1) from %d = %d, %v, want %d, nil"u8, -len(content) / 3, len(content) - 1, off, errΔ4, middle - 1);
                    }
                }
                {
                    var (off, errΔ5) = rΔ1.Seek(+1, 1); if (off != (int64)middle || errΔ5 != default!) {
                        return fmt.Errorf("Seek(+1, 1) from %d = %d, %v, want %d, nil"u8, middle - 1, off, errΔ5, middle);
                    }
                }
            }
            // Seek(0, 1) should report the current file position (middle).
            {
                var (off, errΔ6) = rΔ1.Seek(0, 1); if (off != (int64)middle || errΔ6 != default!) {
                    return fmt.Errorf("Seek(0, 1) from %d = %d, %v, want %d, nil"u8, middle, off, errΔ6, middle);
                }
            }
            // Reading forward should return the last part of the file.
            var (dataΔ1, errΔ7) = io.ReadAll(new smallByteReaderжReader(Ꮡ(new smallByteReader(r: rΔ1))));
            if (errΔ7 != default!) {
                return fmt.Errorf("ReadAll from offset %d: %v"u8, middle, errΔ7);
            }
            if (!bytes.Equal(dataΔ1, content.slice(middle))) {
                return fmt.Errorf("ReadAll from offset %d = %q\n\twant %q"u8, middle, dataΔ1, content.slice(middle));
            }
            // Seek relative to end of file, but start elsewhere.
            {
                var (off, errΔ8) = rΔ1.Seek((int64)(middle / 2), 0); if (off != (int64)(middle / 2) || errΔ8 != default!) {
                    return fmt.Errorf("Seek(%d, 0) from EOF = %d, %v, want %d, nil"u8, middle / 2, off, errΔ8, middle / 2);
                }
            }
            {
                var (off, errΔ9) = rΔ1.Seek((int64)(-len(content) / 3), 2); if (off != (int64)middle || errΔ9 != default!) {
                    return fmt.Errorf("Seek(%d, 2) from %d = %d, %v, want %d, nil"u8, -len(content) / 3, middle / 2, off, errΔ9, middle);
                }
            }
            // Reading forward should return the last part of the file (again).
            (dataΔ1, errΔ7) = io.ReadAll(new smallByteReaderжReader(Ꮡ(new smallByteReader(r: rΔ1))));
            if (errΔ7 != default!) {
                return fmt.Errorf("ReadAll from offset %d: %v"u8, middle, errΔ7);
            }
            if (!bytes.Equal(dataΔ1, content.slice(middle))) {
                return fmt.Errorf("ReadAll from offset %d = %q\n\twant %q"u8, middle, dataΔ1, content.slice(middle));
            }
            // Absolute seek & read forward.
            {
                var (off, errΔ10) = rΔ1.Seek((int64)(middle / 2), 0); if (off != (int64)(middle / 2) || errΔ10 != default!) {
                    return fmt.Errorf("Seek(%d, 0) from EOF = %d, %v, want %d, nil"u8, middle / 2, off, errΔ10, middle / 2);
                }
            }
            (dataΔ1, errΔ7) = io.ReadAll(rΔ1);
            if (errΔ7 != default!) {
                return fmt.Errorf("ReadAll from offset %d: %v"u8, middle / 2, errΔ7);
            }
            if (!bytes.Equal(dataΔ1, content.slice(middle / 2))) {
                return fmt.Errorf("ReadAll from offset %d = %q\n\twant %q"u8, middle / 2, dataΔ1, content.slice(middle / 2));
            }
        }
    }
    {
        var (rΔ2, ok) = r._<io.ReaderAt>(ᐧ); if (ok) {
            var dataΔ2 = new slice<byte>(len(content), len(content) + 1);
            foreach (var (i, _) in dataΔ2) {
                dataΔ2[i] = 0xfe;
            }
            var (nΔ2, errΔ11) = rΔ2.ReadAt(dataΔ2, 0);
            if (nΔ2 != len(dataΔ2) || errΔ11 != default! && !AreEqual(errΔ11, io.EOF)) {
                return fmt.Errorf("ReadAt(%d, 0) = %v, %v, want %d, nil or EOF"u8, len(dataΔ2), nΔ2, errΔ11, len(dataΔ2));
            }
            if (!bytes.Equal(dataΔ2, content)) {
                return fmt.Errorf("ReadAt(%d, 0) = %q\n\twant %q"u8, len(dataΔ2), dataΔ2, content);
            }
            (nΔ2, errΔ11) = rΔ2.ReadAt(dataΔ2[..1], (int64)len(dataΔ2));
            if (nΔ2 != 0 || !AreEqual(errΔ11, io.EOF)) {
                return fmt.Errorf("ReadAt(1, %d) = %v, %v, want 0, EOF"u8, len(dataΔ2), nΔ2, errΔ11);
            }
            foreach (var (i, _) in dataΔ2) {
                dataΔ2[i] = 0xfe;
            }
            (nΔ2, errΔ11) = rΔ2.ReadAt(dataΔ2.slice(0, cap(dataΔ2)), 0);
            if (nΔ2 != len(dataΔ2) || !AreEqual(errΔ11, io.EOF)) {
                return fmt.Errorf("ReadAt(%d, 0) = %v, %v, want %d, EOF"u8, cap(dataΔ2), nΔ2, errΔ11, len(dataΔ2));
            }
            if (!bytes.Equal(dataΔ2, content)) {
                return fmt.Errorf("ReadAt(%d, 0) = %q\n\twant %q"u8, len(dataΔ2), dataΔ2, content);
            }
            foreach (var (i, _) in dataΔ2) {
                dataΔ2[i] = 0xfe;
            }
            foreach (var (i, _) in dataΔ2) {
                (nΔ2, errΔ11) = rΔ2.ReadAt(dataΔ2.slice(i, i + 1), (int64)i);
                if (nΔ2 != 1 || errΔ11 != default! && (i != len(dataΔ2) - 1 || !AreEqual(errΔ11, io.EOF))) {
                    @string want = nilˢ;
                    if (i == len(dataΔ2) - 1) {
                        want = nilOrEofˢ;
                    }
                    return fmt.Errorf("ReadAt(1, %d) = %v, %v, want 1, %s"u8, i, nΔ2, errΔ11, want);
                }
                if (dataΔ2[i] != content[i]) {
                    return fmt.Errorf("ReadAt(1, %d) = %q want %q"u8, i, dataΔ2.slice(i, i + 1), content.slice(i, i + 1));
                }
            }
        }
    }
    return default!;
}

} // end iotest_package
