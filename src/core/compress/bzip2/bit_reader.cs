// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bufio = bufio_package;
using io = io_package;

partial class bzip2_package {

// bitReader wraps an io.Reader and provides the ability to read values,
// bit-by-bit, from it. Its Read* methods don't return the usual error
// because the error handling was verbose. Instead, any error is kept and can
// be checked afterwards.
partial struct bitReader {
    internal io.ByteReader r;
    internal uint64 n;
    internal nuint bits;
    internal error err;
}

// newBitReader returns a new bitReader reading from r. If r is not
// already an io.ByteReader, it will be converted via a bufio.Reader.
internal static bitReader newBitReader(io.Reader r) {
    var (byter, ok) = r._<io.ByteReader>(ᐧ);
    if (!ok) {
        byter = new bufio_ReaderжByteReader(bufio.NewReader(r));
    }
    return new bitReader(r: byter);
}

// ReadBits64 reads the given number of bits and returns them in the
// least-significant part of a uint64. In the event of an error, it returns 0
// and the error can be obtained by calling bitReader.Err().
internal static uint64 /*n*/ ReadBits64(this ref bitReader br, nuint bits) {
    uint64 n = default!;

    while (bits > br.bits) {
        var (b, err) = br.r.ReadByte();
        if (AreEqual(err, io.EOF)) {
            err = io.ErrUnexpectedEOF;
        }
        if (err != default!) {
            br.err = err;
            return 0;
        }
        br.n <<= (int)(8);
        br.n |= (uint64)((uint64)b);
        br.bits += 8;
    }
    // br.n looks like this (assuming that br.bits = 14 and bits = 6):
    // Bit: 111111
    //      5432109876543210
    //
    //         (6 bits, the desired output)
    //        |-----|
    //        V     V
    //      0101101101001110
    //        ^            ^
    //        |------------|
    //           br.bits (num valid bits)
    //
    // The next line right shifts the desired bits into the
    // least-significant places and masks off anything above.
    n = (uint64)((br.n.Rsh((br.bits - bits))) & ((((uint64)1).Lsh(bits)) - 1));
    br.bits -= bits;
    return n;
}

internal static nint /*n*/ ReadBits(this ref bitReader br, nuint bits) {
    var n64 = br.ReadBits64(bits);
    return (nint)n64;
}

internal static bool ReadBit(this ref bitReader br) {
    nint n = br.ReadBits(1);
    return n != 0;
}

internal static error Err(this ref bitReader br) {
    return br.err;
}

} // end bzip2_package
