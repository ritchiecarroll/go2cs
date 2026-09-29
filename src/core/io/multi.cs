// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using ꓸꓸꓸReader = Span<io_package.Reader>;
using ꓸꓸꓸWriter = Span<io_package.Writer>;

partial class io_package {

[GoType] partial struct eofReader {
}

internal static (nint, error) Read(this eofReader _Δp0, slice<byte> _Δp1) {
    return (0, EOF);
}

[GoType] partial struct multiReader {
    internal slice<Reader> readers;
}

[GoRecv] internal static (nint n, error err) Read(this ref multiReader mr, slice<byte> p) {
    nint n = default!;
    error err = default!;

    while (len(mr.readers) > 0) {
        // Optimization to flatten nested multiReaders (Issue 13558).
        if (len(mr.readers) == 1) {
            {
                var (r, ok) = mr.readers[0]._<ж<multiReader>>(ᐧ); if (ok) {
                    mr.readers = r.Value.readers;
                    continue;
                }
            }
        }
        (n, err) = mr.readers[0].Read(p);
        if (AreEqual(err, EOF)) {
            // Use eofReader instead of nil to avoid nil panic
            // after performing flatten (Issue 18232).
            mr.readers[0] = new eofReader(nil); // permit earlier GC
            mr.readers = mr.readers[1..];
        }
        if (n > 0 || !AreEqual(err, EOF)) {
            if (AreEqual(err, EOF) && len(mr.readers) > 0) {
                // Don't return EOF yet. More readers remain.
                err = default!;
            }
            return (n, err);
        }
    }
    return (0, EOF);
}

[GoRecv] internal static (int64 sum, error err) WriteTo(this ref multiReader mr, Writer w) {
    return mr.writeToWithBuffer(w, new slice<byte>(1024 * 32));
}

[GoRecv] internal static (int64 sum, error err) writeToWithBuffer(this ref multiReader mr, Writer w, slice<byte> buf) {
    int64 sum = default!;
    error err = default!;

    foreach (var (i, r) in mr.readers) {
        int64 n = default!;
        {
            var (subMr, ok) = r._<ж<multiReader>>(ᐧ); if (ok){
                // reuse buffer with nested multiReaders
                (n, err) = subMr.writeToWithBuffer(w, buf);
            } else {
                (n, err) = copyBuffer(w, r, buf);
            }
        }
        sum += n;
        if (err != default!) {
            mr.readers = mr.readers.slice(i); // permit resume / retry after error
            return (sum, err);
        }
        mr.readers[i] = default!; // permit early GC
    }
    mr.readers = default!;
    return (sum, default!);
}

internal static WriterTo _ᴛ2ʗ = new multiReaderжWriterTo(((ж<multiReader>)nil));

// MultiReader returns a Reader that's the logical concatenation of
// the provided input readers. They're read sequentially. Once all
// inputs have returned EOF, Read will return EOF.  If any of the readers
// return a non-nil, non-EOF error, Read will return that error.
public static Reader MultiReader(params ꓸꓸꓸReader readersʗp) {
    var readers = readersʗp.sslice();

    var r = new slice<Reader>(len(readers));
    copy(r, readers);
    return new multiReaderжReader(Ꮡ(new multiReader(r)));
}

[GoType] partial struct multiWriter {
    internal slice<Writer> writers;
}

[GoRecv] internal static (nint n, error err) Write(this ref multiWriter t, slice<byte> p) {
    nint n = default!;
    error err = default!;

    foreach (var (_, w) in t.writers) {
        (n, err) = w.Write(p);
        if (err != default!) {
            return (n, err);
        }
        if (n != len(p)) {
            err = ErrShortWrite;
            return (n, err);
        }
    }
    return (len(p), default!);
}

internal static StringWriter _ᴛ3ʗ = new multiWriterжStringWriter(((ж<multiWriter>)nil));

[GoRecv] internal static (nint n, error err) WriteString(this ref multiWriter t, @string s) {
    nint n = default!;
    error err = default!;

    slice<byte> p = default!;             // lazily initialized if/when needed
    foreach (var (_, w) in t.writers) {
        {
            var (sw, ok) = w._<StringWriter>(ᐧ); if (ok){
                (n, err) = sw.WriteString(s);
            } else {
                if (p == default!) {
                    p = slice<byte>(s);
                }
                (n, err) = w.Write(p);
            }
        }
        if (err != default!) {
            return (n, err);
        }
        if (n != len(s)) {
            err = ErrShortWrite;
            return (n, err);
        }
    }
    return (len(s), default!);
}

// MultiWriter creates a writer that duplicates its writes to all the
// provided writers, similar to the Unix tee(1) command.
//
// Each write is written to each listed writer, one at a time.
// If a listed writer returns an error, that overall write operation
// stops and returns the error; it does not continue down the list.
public static Writer MultiWriter(params ꓸꓸꓸWriter writersʗp) {
    var writers = writersʗp.sslice();

    var allWriters = new slice<Writer>(0, len(writers));
    foreach (var (_, w) in writers) {
        {
            var (mw, ok) = w._<ж<multiWriter>>(ᐧ); if (ok){
                allWriters = appendꓸꓸꓸ(allWriters, (~mw).writers);
            } else {
                allWriters = append(allWriters, w);
            }
        }
    }
    return new multiWriterжWriter(Ꮡ(new multiWriter(allWriters)));
}

} // end io_package
