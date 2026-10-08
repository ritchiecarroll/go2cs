// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using io = io_package;

partial class norm_package {

partial struct normWriter {
    internal reorderBuffer rb;
    internal io.Writer w;
    internal slice<byte> buf;
}

// Write implements the standard write interface.  If the last characters are
// not at a normalization boundary, the bytes will be buffered for the next
// write. The remaining bytes will be written on close.
internal static (nint n, error err) Write(this ж<normWriter> Ꮡw, slice<byte> data) {
    nint n = default!;
    error err = default!;

    ref var w = ref Ꮡw.DerefOrNull();
    // Process data in pieces to keep w.buf size bounded.
    const nint chunk = 4000;
    while (len(data) > 0) {
        // Normalize into w.buf.
        nint m = len(data);
        if (m > chunk) {
            m = chunk;
        }
        w.rb.src = inputBytes(data.slice(0, m));
        w.rb.nsrc = m;
        w.buf = doAppend(Ꮡw.of(normWriter.Ꮡrb), w.buf, 0);
        data = data.slice(m);
        n += m;
        // Write out complete prefix, save remainder.
        // Note that lastBoundary looks back at most 31 runes.
        nint i = lastBoundary(ref nonnil(ref w).rb.f, w.buf);
        if (i == -1) {
            i = 0;
        }
        if (i > 0) {
            {
                (_, err) = w.w.Write(w.buf.slice(0, i)); if (err != default!) {
                    break;
                }
            }
            nint bn = copy(w.buf, w.buf.slice(i));
            w.buf = w.buf.slice(0, bn);
        }
    }
    return (n, err);
}

// Close forces data that remains in the buffer to be written.
internal static error Close(this ref normWriter w) {
    if (len(w.buf) > 0) {
        var (_, err) = w.w.Write(w.buf);
        if (err != default!) {
            return err;
        }
    }
    return default!;
}

// Writer returns a new writer that implements Write(b)
// by writing f(b) to w. The returned writer may use an
// internal buffer to maintain state across Write calls.
// Calling its Close method writes any buffered data to w.
public static io.WriteCloser Writer(this Form f, io.Writer w) {
    var wr = Ꮡ(new normWriter(rb: new reorderBuffer(nil), w: w));
    wr.of(normWriter.Ꮡrb).init(f, default!);
    return new normWriterжWriteCloser(wr);
}

partial struct normReader {
    internal reorderBuffer rb;
    internal io.Reader r;
    internal slice<byte> inbuf;
    internal slice<byte> outbuf;
    internal nint bufStart;
    internal nint lastBoundary;
    internal error err;
}

// Read implements the standard read interface.
internal static (nint, error) Read(this ж<normReader> Ꮡr, slice<byte> p) {
    ref var r = ref Ꮡr.DerefOrNull();

    while (ᐧ) {
        if (r.lastBoundary - r.bufStart > 0) {
            nint nΔ1 = copy(p, r.outbuf.slice(r.bufStart, r.lastBoundary));
            r.bufStart += nΔ1;
            if (r.lastBoundary - r.bufStart > 0) {
                return (nΔ1, default!);
            }
            return (nΔ1, r.err);
        }
        if (r.err != default!) {
            return (0, r.err);
        }
        nint outn = copy(r.outbuf, r.outbuf.slice(r.lastBoundary));
        r.outbuf = r.outbuf.slice(0, outn);
        r.bufStart = 0;
        var (n, err) = r.r.Read(r.inbuf);
        r.rb.src = inputBytes(r.inbuf.slice(0, n));
        (r.rb.nsrc, r.err) = (n, err);
        if (n > 0) {
            r.outbuf = doAppend(Ꮡr.of(normReader.Ꮡrb), r.outbuf, 0);
        }
        if (AreEqual(err, io.EOF)){
            r.lastBoundary = len(r.outbuf);
        } else {
            r.lastBoundary = lastBoundary(ref nonnil(ref r).rb.f, r.outbuf);
            if (r.lastBoundary == -1) {
                r.lastBoundary = 0;
            }
        }
    }
}

// Reader returns a new reader that implements Read
// by reading data from r and returning f(data).
public static io.Reader Reader(this Form f, io.Reader r) {
    const nint chunk = 4000;
    var buf = new slice<byte>(chunk);
    var rr = Ꮡ(new normReader(rb: new reorderBuffer(nil), r: r, inbuf: buf));
    rr.of(normReader.Ꮡrb).init(f, buf);
    return new normReaderжReader(rr);
}

} // end norm_package
