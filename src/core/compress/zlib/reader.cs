// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package zlib implements reading and writing of zlib format compressed data,
as specified in RFC 1950.

The implementation provides filters that uncompress during reading
and compress during writing.  For example, to write compressed data
to a buffer:

	var b bytes.Buffer
	w := zlib.NewWriter(&b)
	w.Write([]byte("hello, world\n"))
	w.Close()

and to read that data back:

	r, err := zlib.NewReader(&b)
	io.Copy(os.Stdout, r)
	r.Close()
*/
namespace go.compress;

using bufio = bufio_package;
using flate = go.compress.flate_package;
using binary = encoding.binary_package;
using errors = errors_package;
using hash = hash_package;
using adler32 = go.hash.adler32_package;
using io = io_package;
using encoding;
using go.compress;
using go.hash;

partial class zlib_package {

internal static UntypedInt zlibDeflate => 8;
internal static UntypedInt zlibMaxWindow => 7;

public static error ErrChecksum = errors.New("zlib: invalid checksum"u8);
public static error ErrDictionary = errors.New("zlib: invalid dictionary"u8);
public static error ErrHeader = errors.New("zlib: invalid header"u8);

[GoType] partial struct reader {
    internal flate.Reader r;
    internal io.ReadCloser decompressor;
    internal hash.Hash32 digest;
    internal error err;
    internal array<byte> scratch = new(4);
}

// Resetter resets a ReadCloser returned by [NewReader] or [NewReaderDict]
// to switch to a new underlying Reader. This permits reusing a ReadCloser
// instead of allocating a new one.
[GoType] partial interface Resetter {
    // Reset discards any buffered data and resets the Resetter as if it was
    // newly initialized with the given reader.
    error Reset(io.Reader r, slice<byte> dict);
}

// NewReader creates a new ReadCloser.
// Reads from the returned ReadCloser read and decompress data from r.
// If r does not implement [io.ByteReader], the decompressor may read more
// data than necessary from r.
// It is the caller's responsibility to call Close on the ReadCloser when done.
//
// The [io.ReadCloser] returned by NewReader also implements [Resetter].
public static (io.ReadCloser, error) NewReader(io.Reader r) {
    return NewReaderDict(r, default!);
}

// NewReaderDict is like [NewReader] but uses a preset dictionary.
// NewReaderDict ignores the dictionary if the compressed data does not refer to it.
// If the compressed data refers to a different dictionary, NewReaderDict returns [ErrDictionary].
//
// The ReadCloser returned by NewReaderDict also implements [Resetter].
public static (io.ReadCloser, error) NewReaderDict(io.Reader r, slice<byte> dict) {
    var z = @new<reader>();
    var err = z.Reset(r, dict);
    if (err != default!) {
        return (default!, err);
    }
    return (new readerжReadCloser(z), default!);
}

[GoRecv] internal static (nint, error) Read(this ref reader z, slice<byte> p) {
    if (z.err != default!) {
        return (0, z.err);
    }
    nint n = default!;
    (n, z.err) = z.decompressor.Read(p);
    z.digest.Write(p.slice(0, n));
    if (!AreEqual(z.err, io.EOF)) {
        // In the normal case we return here.
        return (n, z.err);
    }
    // Finished file; check checksum.
    {
        var (_, err) = io.ReadFull(new flate_ReaderᴠReader(z.r), z.scratch[0..4]); if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            z.err = err;
            return (n, z.err);
        }
    }
    // ZLIB (RFC 1950) is big-endian, unlike GZIP (RFC 1952).
    var checksum = binary.BigEndian.Uint32(z.scratch[..4]);
    if (checksum != z.digest.Sum32()) {
        z.err = ErrChecksum;
        return (n, z.err);
    }
    return (n, io.EOF);
}

// Calling Close does not close the wrapped [io.Reader] originally passed to [NewReader].
// In order for the ZLIB checksum to be verified, the reader must be
// fully consumed until the [io.EOF].
[GoRecv] internal static error Close(this ref reader z) {
    if (z.err != default! && !AreEqual(z.err, io.EOF)) {
        return z.err;
    }
    z.err = z.decompressor.Close();
    return z.err;
}

[GoRecv] internal static error Reset(this ref reader z, io.Reader r, slice<byte> dict) {
    z = new reader(decompressor: z.decompressor);
    {
        var (fr, ok) = r._<flate.Reader>(ᐧ); if (ok){
            z.r = fr;
        } else {
            z.r = new bufio_ReaderжReader(bufio.NewReader(r));
        }
    }
    // Read the header (RFC 1950 section 2.2.).
    (_, z.err) = io.ReadFull(z.r, z.scratch[0..2]);
    if (z.err != default!) {
        if (AreEqual(z.err, io.EOF)) {
            z.err = io.ErrUnexpectedEOF;
        }
        return z.err;
    }
    var h = binary.BigEndian.Uint16(z.scratch[..2]);
    if (((byte)(z.scratch[0] & 0x0f) != zlibDeflate) || ((byte)((z.scratch[0] >> (int)(4))) > zlibMaxWindow) || ((uint16)(h % 31) != 0)) {
        z.err = ErrHeader;
        return z.err;
    }
    var haveDict = (byte)(z.scratch[1] & 0x20) != 0;
    if (haveDict) {
        (_, z.err) = io.ReadFull(z.r, z.scratch[0..4]);
        if (z.err != default!) {
            if (AreEqual(z.err, io.EOF)) {
                z.err = io.ErrUnexpectedEOF;
            }
            return z.err;
        }
        var checksum = binary.BigEndian.Uint32(z.scratch[..4]);
        if (checksum != adler32.Checksum(dict)) {
            z.err = ErrDictionary;
            return z.err;
        }
    }
    if (z.decompressor == default!){
        if (haveDict){
            z.decompressor = flate.NewReaderDict(z.r, dict);
        } else {
            z.decompressor = flate.NewReader(z.r);
        }
    } else {
        z.decompressor._<flate.Resetter>().Reset(z.r, dict);
    }
    z.digest = adler32.New();
    return default!;
}

} // end zlib_package
