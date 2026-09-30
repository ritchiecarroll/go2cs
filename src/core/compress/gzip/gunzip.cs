// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gzip implements reading and writing of gzip format compressed files,
// as specified in RFC 1952.
namespace go.compress;

using bufio = bufio_package;
using flate = go.compress.flate_package;
using binary = encoding.binary_package;
using errors = errors_package;
using crc32 = hash.crc32_package;
using io = io_package;
using time = time_package;
using encoding;
using go.compress;
using hash;

partial class gzip_package {

internal static UntypedInt gzipID1 => 0x1f;
internal static UntypedInt gzipID2 => 0x8b;
internal static UntypedInt gzipDeflate => 8;
internal static UntypedInt flagText => /* 1 << 0 */ 1;
internal static UntypedInt flagHdrCrc => /* 1 << 1 */ 2;
internal static UntypedInt flagExtra => /* 1 << 2 */ 4;
internal static UntypedInt flagName => /* 1 << 3 */ 8;
internal static UntypedInt flagComment => /* 1 << 4 */ 16;

public static error ErrChecksum = errors.New("gzip: invalid checksum"u8);
public static error ErrHeader = errors.New("gzip: invalid header"u8);

internal static binary.littleEndian le = binary.LittleEndian;

// noEOF converts io.EOF to io.ErrUnexpectedEOF.
internal static error noEOF(error err) {
    if (AreEqual(err, io.EOF)) {
        return io.ErrUnexpectedEOF;
    }
    return err;
}

// The gzip file stores a header giving metadata about the compressed file.
// That header is exposed as the fields of the [Writer] and [Reader] structs.
//
// Strings must be UTF-8 encoded and may only contain Unicode code points
// U+0001 through U+00FF, due to limitations of the GZIP file format.
[GoType] partial struct Header {
    public @string Comment;   // comment
    public slice<byte> Extra; // "extra data"
    public time.Time ModTime; // modification time
    public @string Name;   // file name
    public byte OS;      // operating system type
}

// A Reader is an [io.Reader] that can be read to retrieve
// uncompressed data from a gzip-format compressed file.
//
// In general, a gzip file can be a concatenation of gzip files,
// each with its own header. Reads from the Reader
// return the concatenation of the uncompressed data of each.
// Only the first header is recorded in the Reader fields.
//
// Gzip files store a length and checksum of the uncompressed data.
// The Reader will return an [ErrChecksum] when [Reader.Read]
// reaches the end of the uncompressed data if it does not
// have the expected length or checksum. Clients should treat data
// returned by [Reader.Read] as tentative until they receive the [io.EOF]
// marking the end of the data.
[GoType] partial struct Reader {
    public partial ref Header Header { get; }       // valid after NewReader or Reader.Reset
    internal flate.Reader r;
    internal io.ReadCloser decompressor;
    internal uint32 digest; // CRC-32, IEEE polynomial (section 8)
    internal uint32 size; // Uncompressed size (section 2.3.1)
    internal array<byte> buf = new(512);
    internal error err;
    internal bool multistream;
}

// NewReader creates a new [Reader] reading the given reader.
// If r does not also implement [io.ByteReader],
// the decompressor may read more data than necessary from r.
//
// It is the caller's responsibility to call Close on the [Reader] when done.
//
// The [Reader.Header] fields will be valid in the [Reader] returned.
public static (ж<Reader>, error) NewReader(io.Reader r) {
    var z = @new<Reader>();
    {
        var err = z.Reset(r); if (err != default!) {
            return (default!, err);
        }
    }
    return (z, default!);
}

// Reset discards the [Reader] z's state and makes it equivalent to the
// result of its original state from [NewReader], but reading from r instead.
// This permits reusing a [Reader] rather than allocating a new one.
[GoRecv] public static error Reset(this ref Reader z, io.Reader r) {
    z = new Reader(
        decompressor: z.decompressor,
        multistream: true
    );
    {
        var (rr, ok) = r._<flate.Reader>(ᐧ); if (ok){
            z.r = rr;
        } else {
            z.r = new bufio_ReaderжReader(bufio.NewReader(r));
        }
    }
    (z.Header, z.err) = z.readHeader();
    return z.err;
}

// Multistream controls whether the reader supports multistream files.
//
// If enabled (the default), the [Reader] expects the input to be a sequence
// of individually gzipped data streams, each with its own header and
// trailer, ending at EOF. The effect is that the concatenation of a sequence
// of gzipped files is treated as equivalent to the gzip of the concatenation
// of the sequence. This is standard behavior for gzip readers.
//
// Calling Multistream(false) disables this behavior; disabling the behavior
// can be useful when reading file formats that distinguish individual gzip
// data streams or mix gzip data streams with other data streams.
// In this mode, when the [Reader] reaches the end of the data stream,
// [Reader.Read] returns [io.EOF]. The underlying reader must implement [io.ByteReader]
// in order to be left positioned just after the gzip stream.
// To start the next stream, call z.Reset(r) followed by z.Multistream(false).
// If there is no next stream, z.Reset(r) will return [io.EOF].
[GoRecv] public static void Multistream(this ref Reader z, bool ok) {
    z.multistream = ok;
}

// readString reads a NUL-terminated string from z.r.
// It treats the bytes read as being encoded as ISO 8859-1 (Latin-1) and
// will output a string encoded using UTF-8.
// This method always updates z.digest with the data read.
[GoRecv] internal static (@string, error) readString(this ref Reader z) {
    error err = default!;
    var needConv = false;
    for (nint i = 0; ᐧ ; i++) {
        if (i >= len(z.buf)) {
            return ("", ErrHeader);
        }
        (z.buf[i], err) = z.r.ReadByte();
        if (err != default!) {
            return ("", err);
        }
        if (z.buf[i] > 0x7f) {
            needConv = true;
        }
        if (z.buf[i] == 0) {
            // Digest covers the NUL terminator.
            z.digest = crc32.Update(z.digest, crc32.IEEETable, z.buf.slice(0, i + 1));
            // Strings are ISO 8859-1, Latin-1 (RFC 1952, section 2.3.1).
            if (needConv) {
                var s = new slice<rune>(0, i);
                foreach (var (_, v) in z.buf.slice(0, i)) {
                    s = append(s, (rune)v);
                }
                return (((@string)s), default!);
            }
            return (((@string)(z.buf.slice(0, i))), default!);
        }
    }
}

// readHeader reads the GZIP header according to section 2.3.1.
// This method does not set z.err.
[GoRecv] internal static (Header hdr, error err) readHeader(this ref Reader z) {
    Header hdr = default!;
    error err = default!;

    {
        (_, err) = io.ReadFull(z.r, z.buf[..10]); if (err != default!) {
            // RFC 1952, section 2.2, says the following:
            //	A gzip file consists of a series of "members" (compressed data sets).
            //
            // Other than this, the specification does not clarify whether a
            // "series" is defined as "one or more" or "zero or more". To err on the
            // side of caution, Go interprets this to mean "zero or more".
            // Thus, it is okay to return io.EOF here.
            return (hdr, err);
        }
    }
    if (z.buf[0] != gzipID1 || z.buf[1] != gzipID2 || z.buf[2] != gzipDeflate) {
        return (hdr, ErrHeader);
    }
    var flg = z.buf[3];
    {
        var t = (int64)le.Uint32(z.buf[4..8]); if (t > 0) {
            // Section 2.3.1, the zero value for MTIME means that the
            // modified time is not set.
            hdr.ModTime = time.Unix(t, 0);
        }
    }
    // z.buf[8] is XFL and is currently ignored.
    hdr.OS = z.buf[9];
    z.digest = crc32.ChecksumIEEE(z.buf[..10]);
    if ((byte)(flg & (byte)flagExtra) != 0) {
        {
            (_, err) = io.ReadFull(z.r, z.buf[..2]); if (err != default!) {
                return (hdr, noEOF(err));
            }
        }
        z.digest = crc32.Update(z.digest, crc32.IEEETable, z.buf[..2]);
        var data = new slice<byte>(le.Uint16(z.buf[..2]));
        {
            (_, err) = io.ReadFull(z.r, data); if (err != default!) {
                return (hdr, noEOF(err));
            }
        }
        z.digest = crc32.Update(z.digest, crc32.IEEETable, data);
        hdr.Extra = data;
    }
    @string s = default!;
    if ((byte)(flg & (byte)flagName) != 0) {
        {
            (s, err) = z.readString(); if (err != default!) {
                return (hdr, noEOF(err));
            }
        }
        hdr.Name = s;
    }
    if ((byte)(flg & (byte)flagComment) != 0) {
        {
            (s, err) = z.readString(); if (err != default!) {
                return (hdr, noEOF(err));
            }
        }
        hdr.Comment = s;
    }
    if ((byte)(flg & (byte)flagHdrCrc) != 0) {
        {
            (_, err) = io.ReadFull(z.r, z.buf[..2]); if (err != default!) {
                return (hdr, noEOF(err));
            }
        }
        var digest = le.Uint16(z.buf[..2]);
        if (digest != (uint16)z.digest) {
            return (hdr, ErrHeader);
        }
    }
    z.digest = 0;
    if (z.decompressor == default!){
        z.decompressor = flate.NewReader(z.r);
    } else {
        z.decompressor._<flate.Resetter>().Reset(z.r, default!);
    }
    return (hdr, default!);
}

// Read implements [io.Reader], reading uncompressed bytes from its underlying [Reader].
[GoRecv] public static (nint n, error err) Read(this ref Reader z, slice<byte> p) {
    nint n = default!;

    if (z.err != default!) {
        return (0, z.err);
    }
    while (n == 0) {
        (n, z.err) = z.decompressor.Read(p);
        z.digest = crc32.Update(z.digest, crc32.IEEETable, p.slice(0, n));
        z.size += (uint32)n;
        if (!AreEqual(z.err, io.EOF)) {
            // In the normal case we return here.
            return (n, z.err);
        }
        // Finished file; check checksum and size.
        {
            var (_, errΔ1) = io.ReadFull(z.r, z.buf[..8]); if (errΔ1 != default!) {
                z.err = noEOF(errΔ1);
                return (n, z.err);
            }
        }
        var digest = le.Uint32(z.buf[..4]);
        var size = le.Uint32(z.buf[4..8]);
        if (digest != z.digest || size != z.size) {
            z.err = ErrChecksum;
            return (n, z.err);
        }
        (z.digest, z.size) = (0, 0);
        // File is ok; check if there is another.
        if (!z.multistream) {
            return (n, io.EOF);
        }
        z.err = default!; // Remove io.EOF
        {
            (_, z.err) = z.readHeader(); if (z.err != default!) {
                return (n, z.err);
            }
        }
    }
    return (n, default!);
}

// Close closes the [Reader]. It does not close the underlying [io.Reader].
// In order for the GZIP checksum to be verified, the reader must be
// fully consumed until the [io.EOF].
[GoRecv] public static error Close(this ref Reader z) {
    return z.decompressor.Close();
}

} // end gzip_package
