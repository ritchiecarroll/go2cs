// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using flate = go.compress.flate_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using hash = hash_package;
using adler32 = go.hash.adler32_package;
using io = io_package;
using encoding;
using go.compress;
using go.hash;

partial class zlib_package {

// These constants are copied from the flate package, so that code that imports
// "compress/zlib" does not also have to import "compress/flate".
public static UntypedInt NoCompression => /* flate.NoCompression */ 0;

public static UntypedInt BestSpeed => /* flate.BestSpeed */ 1;

public static UntypedInt BestCompression => /* flate.BestCompression */ 9;

public static UntypedInt DefaultCompression => /* flate.DefaultCompression */ -1;

public static UntypedInt HuffmanOnly => /* flate.HuffmanOnly */ -2;

// A Writer takes data written to it and writes the compressed
// form of that data to an underlying writer (see NewWriter).
partial struct Writer {
    internal io.Writer w;
    internal nint level;
    internal slice<byte> dict;
    internal ж<flate.Writer> compressor;
    internal hash.Hash32 digest;
    internal error err;
    internal array<byte> scratch = new(4);
    internal bool wroteHeader;
}

// NewWriter creates a new Writer.
// Writes to the returned Writer are compressed and written to w.
//
// It is the caller's responsibility to call Close on the Writer when done.
// Writes may be buffered and not flushed until Close.
public static ж<Writer> NewWriter(io.Writer w) {
    var (z, _) = NewWriterLevelDict(w, DefaultCompression, default!);
    return z;
}

// NewWriterLevel is like NewWriter but specifies the compression level instead
// of assuming DefaultCompression.
//
// The compression level can be DefaultCompression, NoCompression, HuffmanOnly
// or any integer value between BestSpeed and BestCompression inclusive.
// The error returned will be nil if the level is valid.
public static (ж<Writer>, error) NewWriterLevel(io.Writer w, nint level) {
    return NewWriterLevelDict(w, level, default!);
}

// NewWriterLevelDict is like NewWriterLevel but specifies a dictionary to
// compress with.
//
// The dictionary may be nil. If not, its contents should not be modified until
// the Writer is closed.
public static (ж<Writer>, error) NewWriterLevelDict(io.Writer w, nint level, slice<byte> dict) {
    if (level < HuffmanOnly || level > BestCompression) {
        return (default!, fmt.Errorf("zlib: invalid compression level: %d"u8, level));
    }
    return (Ꮡ(new Writer(
        w: w,
        level: level,
        dict: dict
    )), default!);
}

// Reset clears the state of the Writer z such that it is equivalent to its
// initial state from NewWriterLevel or NewWriterLevelDict, but instead writing
// to w.
public static void Reset(this ref Writer z, io.Writer w) {
    z.w = w;
    // z.level and z.dict left unchanged.
    if (z.compressor != nil) {
        z.compressor.Reset(w);
    }
    if (z.digest != default!) {
        z.digest.Reset();
    }
    z.err = default!;
    z.scratch = new byte[]{}.array(4);
    z.wroteHeader = false;
}

// writeHeader writes the ZLIB header.
internal static error /*err*/ writeHeader(this ref Writer z) {
    error err = default!;

    z.wroteHeader = true;
    // ZLIB has a two-byte header (as documented in RFC 1950).
    // The first four bits is the CINFO (compression info), which is 7 for the default deflate window size.
    // The next four bits is the CM (compression method), which is 8 for deflate.
    z.scratch[0] = 0x78;
    // The next two bits is the FLEVEL (compression level). The four values are:
    // 0=fastest, 1=fast, 2=default, 3=best.
    // The next bit, FDICT, is set if a dictionary is given.
    // The final five FCHECK bits form a mod-31 checksum.
    var exprᴛ1 = z.level;
    if (exprᴛ1 == -2 || exprᴛ1 == 0 || exprᴛ1 == 1) {
        z.scratch[1] = (byte)(0 << (int)(6));
    }
    else if (exprᴛ1 is 2 or 3 or 4 or 5) {
        z.scratch[1] = (byte)(1 << (int)(6));
    }
    else if (exprᴛ1 == 6 || exprᴛ1 == -1) {
        z.scratch[1] = (byte)(2 << (int)(6));
    }
    else if (exprᴛ1 is 7 or 8 or 9) {
        z.scratch[1] = (byte)(3 << (int)(6));
    }
    else { /* default: */
        throw panic("unreachable");
    }

    if (z.dict != default!) {
        z.scratch[1] |= (byte)((byte)(1 << (int)(5)));
    }
    z.scratch[1] += (uint8)(31 - binary.BigEndian.Uint16(z.scratch[..2]) % 31);
    {
        (_, err) = z.w.Write(z.scratch[0..2]); if (err != default!) {
            return err;
        }
    }
    if (z.dict != default!) {
        // The next four bytes are the Adler-32 checksum of the dictionary.
        binary.BigEndian.PutUint32(z.scratch[..], adler32.Checksum(z.dict));
        {
            (_, err) = z.w.Write(z.scratch[0..4]); if (err != default!) {
                return err;
            }
        }
    }
    if (z.compressor == nil) {
        // Initialize deflater unless the Writer is being reused
        // after a Reset call.
        (z.compressor, err) = flate.NewWriterDict(z.w, z.level, z.dict);
        if (err != default!) {
            return err;
        }
        z.digest = adler32.New();
    }
    return default!;
}

// Write writes a compressed form of p to the underlying io.Writer. The
// compressed bytes are not necessarily flushed until the Writer is closed or
// explicitly flushed.
public static (nint n, error err) Write(this ref Writer z, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (!z.wroteHeader) {
        z.err = z.writeHeader();
    }
    if (z.err != default!) {
        return (0, z.err);
    }
    if (len(p) == 0) {
        return (0, default!);
    }
    (n, err) = z.compressor.Write(p);
    if (err != default!) {
        z.err = err;
        return (n, err);
    }
    z.digest.Write(p);
    return (n, err);
}

// Flush flushes the Writer to its underlying io.Writer.
public static error Flush(this ref Writer z) {
    if (!z.wroteHeader) {
        z.err = z.writeHeader();
    }
    if (z.err != default!) {
        return z.err;
    }
    z.err = z.compressor.Flush();
    return z.err;
}

// Close closes the Writer, flushing any unwritten data to the underlying
// io.Writer, but does not close the underlying io.Writer.
public static error Close(this ref Writer z) {
    if (!z.wroteHeader) {
        z.err = z.writeHeader();
    }
    if (z.err != default!) {
        return z.err;
    }
    z.err = z.compressor.Close();
    if (z.err != default!) {
        return z.err;
    }
    var checksum = z.digest.Sum32();
    // ZLIB (RFC 1950) is big-endian, unlike GZIP (RFC 1952).
    binary.BigEndian.PutUint32(z.scratch[..], checksum);
    (_, z.err) = z.w.Write(z.scratch[0..4]);
    return z.err;
}

} // end zlib_package
