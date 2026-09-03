// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package flate implements the DEFLATE compressed data format, described in
// RFC 1951.  The gzip and zlib packages implement access to DEFLATE-based file
// formats.
namespace go.compress;

using bufio = bufio_package;
using io = io_package;
using bits = go.math.bits_package;
using strconv = strconv_package;
using sync = sync_package;
using go.math;

partial class flate_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸbufio() {
    builtin.initPackage(typeof(bufio_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸstrconv() {
    builtin.initPackage(typeof(strconv_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸsync() {
    builtin.initPackage(typeof(sync_package));
}

internal static UntypedInt maxCodeLen => 16; // max length of Huffman code
internal static UntypedInt maxNumLit => 286;
internal static UntypedInt maxNumDist => 30;
internal static UntypedInt numCodes => 19; // number of codes in Huffman meta-code

// Initialize the fixedHuffmanDecoder only once upon first use.
internal static ж<sync.Once> ᏑfixedOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once fixedOnce => ref ᏑfixedOnce.Value;

internal static ж<huffmanDecoder> ᏑfixedHuffmanDecoder = new StandardBox<huffmanDecoder>(new huffmanDecoder());
internal static ref huffmanDecoder fixedHuffmanDecoder => ref ᏑfixedHuffmanDecoder.Value;

[GoType("num:int64")] partial struct CorruptInputError;

public static @string Error(this CorruptInputError e) {
    return "flate: corrupt input before offset "u8 + strconv.FormatInt((int64)e, 10);
}

[GoType("@string")] partial struct InternalError;

public static @string Error(this InternalError e) {
    return "flate: internal error: "u8 + ((@string)e);
}

// A ReadError reports an error encountered while reading input.
//
// Deprecated: No longer returned.
[GoType] partial struct ReadError {
    public int64 Offset; // byte offset where error occurred
    public error Err; // error returned by underlying Read
}

[GoRecv] public static @string Error(this ref ReadError e) {
    return "flate: read error at offset "u8 + strconv.FormatInt(e.Offset, 10) + ": "u8 + e.Err.Error();
}

// A WriteError reports an error encountered while writing output.
//
// Deprecated: No longer returned.
[GoType] partial struct WriteError {
    public int64 Offset; // byte offset where error occurred
    public error Err; // error returned by underlying Write
}

[GoRecv] public static @string Error(this ref WriteError e) {
    return "flate: write error at offset "u8 + strconv.FormatInt(e.Offset, 10) + ": "u8 + e.Err.Error();
}

// Resetter resets a ReadCloser returned by [NewReader] or [NewReaderDict]
// to switch to a new underlying [Reader]. This permits reusing a ReadCloser
// instead of allocating a new one.
[GoType] partial interface Resetter {
    // Reset discards any buffered data and resets the Resetter as if it was
    // newly initialized with the given reader.
    error Reset(io.Reader r, slice<byte> dict);
}

// The data structure for decoding Huffman tables is based on that of
// zlib. There is a lookup table of a fixed bit width (huffmanChunkBits),
// For codes smaller than the table width, there are multiple entries
// (each combination of trailing bits has the same value). For codes
// larger than the table width, the table contains a link to an overflow
// table. The width of each entry in the link table is the maximum code
// size minus the chunk width.
//
// Note that you can do a lookup in the table even without all bits
// filled. Since the extra bits are zero, and the DEFLATE Huffman codes
// have the property that shorter codes come before longer ones, the
// bit length estimate in the result is a lower bound on the actual
// number of bits.
//
// See the following:
//	https://github.com/madler/zlib/raw/master/doc/algorithm.txt
// chunk & 15 is number of bits
// chunk >> 4 is value, including table link
internal static UntypedInt huffmanChunkBits => 9;
internal static UntypedInt huffmanNumChunks => /* 1 << huffmanChunkBits */ 512;
internal static UntypedInt huffmanCountMask => 15;
internal static UntypedInt huffmanValueShift => 4;

[GoType] partial struct huffmanDecoder {
    internal nint min;                     // the minimum code length
    internal array<uint32> chunks = new(huffmanNumChunks); // chunks as described above
    internal slice<slice<uint32>> links;     // overflow links
    internal uint32 linkMask;                   // mask the width of the link table
}

// Initialize Huffman decoding tables from array of code lengths.
// Following this function, h is guaranteed to be initialized into a complete
// tree (i.e., neither over-subscribed nor under-subscribed). The exception is a
// degenerate case where the tree has only a single symbol with length 1. Empty
// trees are permitted.
[GoRecv] internal static bool init(this ref huffmanDecoder h, slice<nint> lengths) {
    // Sanity enables additional runtime tests during Huffman
    // table construction. It's intended to be used during
    // development to supplement the currently ad-hoc unit tests.
    const bool sanity = false;
    if (h.min != 0) {
        h = new huffmanDecoder(nil);
    }
    // Count number of codes of each length,
    // compute min and max length.
    array<nint> count = new(16); /* maxCodeLen */
    nint min = default!;
    nint max = default!;
    foreach (var (_, n) in lengths) {
        if (n == 0) {
            continue;
        }
        if (min == 0 || n < min) {
            min = n;
        }
        if (n > max) {
            max = n;
        }
        count[n]++;
    }
    // Empty tree. The decompressor.huffSym function will fail later if the tree
    // is used. Technically, an empty tree is only valid for the HDIST tree and
    // not the HCLEN and HLIT tree. However, a stream with an empty HCLEN tree
    // is guaranteed to fail since it will attempt to use the tree to decode the
    // codes for the HLIT and HDIST trees. Similarly, an empty HLIT tree is
    // guaranteed to fail later since the compressed data section must be
    // composed of at least one symbol (the end-of-block marker).
    if (max == 0) {
        return true;
    }
    nint code = 0;
    array<nint> nextcode = new(16); /* maxCodeLen */
    for (nint i = min; i <= max; i++) {
        code <<= (int)(1);
        nextcode[i] = code;
        code += count[i];
    }
    // Check that the coding is complete (i.e., that we've
    // assigned all 2-to-the-max possible bit sequences).
    // Exception: To be compatible with zlib, we also need to
    // accept degenerate single-code codings. See also
    // TestDegenerateHuffmanCoding.
    if (code != ((nint)1).Lsh((nuint)max) && !(code == 1 && max == 1)) {
        return false;
    }
    h.min = min;
    if (max > huffmanChunkBits) {
        nint numLinks = ((nint)1).Lsh(((nuint)max - (nuint)huffmanChunkBits));
        h.linkMask = (uint32)(numLinks - 1);
        // create link tables
        nint link = (nextcode[huffmanChunkBits + 1] >> (int)(1));
        h.links = new slice<slice<uint32>>((nint)huffmanNumChunks - link);
        for (nuint j = (nuint)link; j < huffmanNumChunks; j++) {
            nint reverse = (nint)bits.Reverse16((uint16)j);
            reverse >>= (int)((nuint)(16 - huffmanChunkBits));
            nuint off = j - (nuint)link;
            if (sanity && h.chunks[reverse] != 0) {
                throw panic("impossible: overwriting existing chunk");
            }
            h.chunks[reverse] = (uint32)((nuint)((off << (int)(huffmanValueShift)) | (nuint)(huffmanChunkBits + 1)));
            h.links[(nint)(off)] = new slice<uint32>(numLinks);
        }
    }
    foreach (var (i, n) in lengths) {
        if (n == 0) {
            continue;
        }
        nint codeΔ1 = nextcode[n];
        nextcode[n]++;
        var chunk = (uint32)((nint)((i << (int)(huffmanValueShift)) | n));
        nint reverse = (nint)bits.Reverse16((uint16)codeΔ1);
        reverse >>= (int)((nuint)(16 - n));
        if (n <= huffmanChunkBits){
            for (nint off = reverse; off < len(h.chunks); off += ((nint)1).Lsh((nuint)n)) {
                // We should never need to overwrite
                // an existing chunk. Also, 0 is
                // never a valid chunk, because the
                // lower 4 "count" bits should be
                // between 1 and 15.
                if (sanity && h.chunks[off] != 0) {
                    throw panic("impossible: overwriting existing chunk");
                }
                h.chunks[off] = chunk;
            }
        } else {
            nint j = (nint)(reverse & (nint)(huffmanNumChunks - 1));
            if (sanity && (uint32)(h.chunks[j] & (uint32)huffmanCountMask) != huffmanChunkBits + 1) {
                // Longer codes should have been
                // associated with a link table above.
                throw panic("impossible: not an indirect chunk");
            }
            var value = (h.chunks[j] >> (int)(huffmanValueShift));
            var linktab = h.links[(nint)(value)];
            reverse >>= (int)(huffmanChunkBits);
            for (nint off = reverse; off < len(linktab); off += ((nint)1).Lsh((nuint)(n - (nint)huffmanChunkBits))) {
                if (sanity && linktab[off] != 0) {
                    throw panic("impossible: overwriting existing chunk");
                }
                linktab[off] = chunk;
            }
        }
    }
    if (sanity) {
        // Above we've sanity checked that we never overwrote
        // an existing entry. Here we additionally check that
        // we filled the tables completely.
        foreach (var (i, chunk) in h.chunks.ΔRangeSnapshot()) {
            if (chunk == 0) {
                // As an exception, in the degenerate
                // single-code case, we allow odd
                // chunks to be missing.
                if (code == 1 && i % 2 == 1) {
                    continue;
                }
                throw panic("impossible: missing chunk");
            }
        }
        foreach (var (_, linktab) in h.links) {
            foreach (var (_, chunk) in linktab) {
                if (chunk == 0) {
                    throw panic("impossible: missing chunk");
                }
            }
        }
    }
    return true;
}

// The actual read interface needed by [NewReader].
// If the passed in io.Reader does not also have ReadByte,
// the [NewReader] will introduce its own buffering.
[GoType] partial interface Reader :
    io.Reader,
    io.ByteReader
{
}

// Decompress state.
[GoType] partial struct decompressor {
    // Input source.
    internal Reader r;
    internal ж<bufio.Reader> rBuf; // created if provided io.Reader does not implement io.ByteReader
    internal int64 roffset;
    // Input bits, in top of b.
    internal uint32 b;
    internal nuint nb;
    // Huffman decoders for literal/length, distance.
    internal huffmanDecoder h1, h2;
    // Length arrays used to define Huffman codes.
    [GoArrayDims(316)]
    internal ж<array<nint>> bits;
    [GoArrayDims(19)]
    internal ж<array<nint>> codebits;
    // Output history, buffer.
    internal dictDecoder dict;
    // Temporary buffer (avoids repeated allocation).
    internal array<byte> buf = new(4);
    // Next step in the decompression,
    // and decompression state.
    internal Action<ж<decompressor>> step;
    internal nint stepState;
    internal bool final;
    internal error err;
    internal slice<byte> toRead;
    internal ж<huffmanDecoder> hl, hd;
    internal nint copyLen;
    internal nint copyDist;
}

internal static void nextBlock(this ж<decompressor> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    while (f.nb < 1 + 2) {
        {
            f.err = f.moreBits(); if (f.err != default!) {
                return;
            }
        }
    }
    f.final = (uint32)(f.b & 1) == 1;
    f.b >>= (int)(1);
    var typ = (uint32)(f.b & 3);
    f.b >>= (int)(2);
    f.nb -= 1 + 2;
    switch (typ) {
    case 0: {
        f.dataBlock();
        break;
    }
    case 1: {
        f.hl = ᏑfixedHuffmanDecoder;
        f.hd = default!;
        f.huffmanBlock();
        break;
    }
    case 2: {
        {
            f.err = Ꮡf.readHuffman(); if (f.err != default!) {
                // compressed, fixed Huffman tables
                // compressed, dynamic Huffman tables
                break;
            }
        }
        f.hl = Ꮡf.of(decompressor.Ꮡh1);
        f.hd = Ꮡf.of(decompressor.Ꮡh2);
        f.huffmanBlock();
        break;
    }
    default: {
        f.err = ((CorruptInputError)f.roffset);
        break;
    }}

}

// 3 is reserved.
internal static (nint, error) Read(this ж<decompressor> Ꮡf, slice<byte> b) {
    ref var f = ref Ꮡf.DerefOrNull();

    while (ᐧ) {
        if (len(f.toRead) > 0) {
            nint n = copy(b, f.toRead);
            f.toRead = f.toRead[(int)(n)..];
            if (len(f.toRead) == 0) {
                return (n, f.err);
            }
            return (n, default!);
        }
        if (f.err != default!) {
            return (0, f.err);
        }
        f.step(Ꮡf);
        if (f.err != default! && len(f.toRead) == 0) {
            f.toRead = f.dict.readFlush(); // Flush what's left in case of error
        }
    }
}

[GoRecv] internal static error Close(this ref decompressor f) {
    if (AreEqual(f.err, io.EOF)) {
        return default!;
    }
    return f.err;
}

// RFC 1951 section 3.2.7.
// Compression with dynamic Huffman codes
internal static array<nint> codeOrder = new nint[]{16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedLengthCodeˢ = "unexpected length code"u8;

internal static error readHuffman(this ж<decompressor> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    // HLIT[5], HDIST[5], HCLEN[4].
    while (f.nb < 5 + 5 + 4) {
        {
            var err = f.moreBits(); if (err != default!) {
                return err;
            }
        }
    }
    nint nlit = (nint)((uint32)(f.b & 0x1F)) + 257;
    if (nlit > maxNumLit) {
        return ((CorruptInputError)f.roffset);
    }
    f.b >>= (int)(5);
    nint ndist = (nint)((uint32)(f.b & 0x1F)) + 1;
    if (ndist > maxNumDist) {
        return ((CorruptInputError)f.roffset);
    }
    f.b >>= (int)(5);
    nint nclen = (nint)((uint32)(f.b & 0xF)) + 4;
    // numCodes is 19, so nclen is always valid.
    f.b >>= (int)(4);
    f.nb -= 5 + 5 + 4;
    // (HCLEN+4)*3 bits: code lengths in the magic codeOrder order.
    for (nint i = 0; i < nclen; i++) {
        while (f.nb < 3) {
            {
                var err = f.moreBits(); if (err != default!) {
                    return err;
                }
            }
        }
        f.codebits.Value[codeOrder[i]] = (nint)((uint32)(f.b & 0x7));
        f.b >>= (int)(3);
        f.nb -= 3;
    }
    for (nint i = nclen; i < len(codeOrder); i++) {
        f.codebits.Value[codeOrder[i]] = 0;
    }
    if (!f.h1.init((~f.codebits)[0..])) {
        return ((CorruptInputError)f.roffset);
    }
    // HLIT + 257 code lengths, HDIST + 1 code lengths,
    // using the code length Huffman code.
    for ((nint i, nint n) = (0, nlit + ndist); i < n; ) {
        var (x, err) = f.huffSym(Ꮡf.of(decompressor.Ꮡh1));
        if (err != default!) {
            return err;
        }
        if (x < 16) {
            // Actual length.
            f.bits.Value[i] = x;
            i++;
            continue;
        }
        // Repeat previous length or zero.
        nint rep = default!;
        nuint nb = default!;
        nint b = default!;
        switch (x) {
        default: {
            return ((InternalError)(@string)unexpectedLengthCodeˢ);
        }
        case 16: {
            rep = 3;
            nb = 2;
            if (i == 0) {
                return ((CorruptInputError)f.roffset);
            }
            b = f.bits.Value[i - 1];
            break;
        }
        case 17: {
            rep = 3;
            nb = 3;
            b = 0;
            break;
        }
        case 18: {
            rep = 11;
            nb = 7;
            b = 0;
            break;
        }}

        while (f.nb < nb) {
            {
                var errΔ1 = f.moreBits(); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }
        rep += (nint)((uint32)(f.b & (uint32)(((uint32)1).Lsh(nb) - 1)));
        f.b >>= (int)(nb);
        f.nb -= nb;
        if (i + rep > n) {
            return ((CorruptInputError)f.roffset);
        }
        for (nint j = 0; j < rep; j++) {
            f.bits.Value[i] = b;
            i++;
        }
    }
    if (!f.h1.init((~f.bits)[0..(int)(nlit)]) || !f.h2.init((~f.bits)[(int)(nlit)..(int)(nlit + ndist)])) {
        return ((CorruptInputError)f.roffset);
    }
    // As an optimization, we can initialize the min bits to read at a time
    // for the HLIT tree to the length of the EOB marker since we know that
    // every block must terminate with one. This preserves the property that
    // we never read any extra bytes after the end of the DEFLATE stream.
    if (f.h1.min < f.bits.Value[endBlockMarker]) {
        f.h1.min = f.bits.Value[endBlockMarker];
    }
    return default!;
}

// Decode a single Huffman block from f.
// hl and hd are the Huffman states for the lit/length values
// and the distance values, respectively. If hd == nil, using the
// fixed distance encoding associated with fixed Huffman blocks.
[GoRecv] internal static void huffmanBlock(this ref decompressor f) {
    const nint stateInit = iota; // Zero value must be stateInit
    const nint stateDict = 1;
    var exprᴛ1 = f.stepState;
    if (exprᴛ1 == stateInit) {
        goto readLiteral;
    }
    else if (exprᴛ1 == stateDict) {
        goto copyHistory;
    }

readLiteral:
    {
        // Read literal and/or (length, distance) according to RFC section 3.2.3.
        var (v, err) = f.huffSym(f.hl);
        if (err != default!) {
            f.err = err;
            return;
        }
        nuint n = default!;         // number of bits extra
        nint length = default!;
        switch (ᐧ) {
        case {} when v is < 256: {
            f.dict.writeByte((byte)v);
            if (f.dict.availWrite() == 0) {
                f.toRead = f.dict.readFlush();
                f.step = (Action<ж<decompressor>>)(huffmanBlock);
                f.stepState = stateInit;
                return;
            }
            goto readLiteral;
            break;
        }
        case {} when v is 256: {
            f.finishBlock();
            return;
        }
        case {} when v is < 265: {
            length = v - (257 - 3);
            n = 0;
            break;
        }
        case {} when v is < 269: {
            length = v * 2 - (265 * 2 - 11);
            n = 1;
            break;
        }
        case {} when v is < 273: {
            length = v * 4 - (269 * 4 - 19);
            n = 2;
            break;
        }
        case {} when v is < 277: {
            length = v * 8 - (273 * 8 - 35);
            n = 3;
            break;
        }
        case {} when v is < 281: {
            length = v * 16 - (277 * 16 - 67);
            n = 4;
            break;
        }
        case {} when v is < 285: {
            length = v * 32 - (281 * 32 - 131);
            n = 5;
            break;
        }
        case {} when v < maxNumLit: {
            length = 258;
            n = 0;
            break;
        }
        default: {
            f.err = ((CorruptInputError)f.roffset);
            return;
        }}

        // otherwise, reference to older data
        if (n > 0) {
            while (f.nb < n) {
                {
                    err = f.moreBits(); if (err != default!) {
                        f.err = err;
                        return;
                    }
                }
            }
            length += (nint)((uint32)(f.b & (uint32)(((uint32)1).Lsh(n) - 1)));
            f.b >>= (int)(n);
            f.nb -= n;
        }
        nint dist = default!;
        if (f.hd == nil){
            while (f.nb < 5) {
                {
                    err = f.moreBits(); if (err != default!) {
                        f.err = err;
                        return;
                    }
                }
            }
            dist = (nint)bits.Reverse8((uint8)(((uint32)(f.b & 0x1F) << (int)(3))));
            f.b >>= (int)(5);
            f.nb -= 5;
        } else {
            {
                (dist, err) = f.huffSym(f.hd); if (err != default!) {
                    f.err = err;
                    return;
                }
            }
        }
        switch (ᐧ) {
        case {} when dist is < 4: {
            dist++;
            break;
        }
        case {} when dist < maxNumDist: {
            nuint nb = ((nuint)(dist - 2) >> (int)(1));
            nint extra = ((nint)(dist & 1)).Lsh(nb);
            while (f.nb < nb) {
                // have 1 bit in bottom of dist, need nb more.
                {
                    err = f.moreBits(); if (err != default!) {
                        f.err = err;
                        return;
                    }
                }
            }
            extra |= (nint)((nint)((uint32)(f.b & (uint32)(((uint32)1).Lsh(nb) - 1))));
            f.b >>= (int)(nb);
            f.nb -= nb;
            dist = ((nint)1).Lsh((nb + 1)) + 1 + extra;
            break;
        }
        default: {
            f.err = ((CorruptInputError)f.roffset);
            return;
        }}

        // No check on length; encoding can be prescient.
        if (dist > f.dict.histSize()) {
            f.err = ((CorruptInputError)f.roffset);
            return;
        }
        (f.copyLen, f.copyDist) = (length, dist);
        goto copyHistory;
    }
copyHistory:
    {
        // Perform a backwards copy according to RFC section 3.2.3.
        nint cnt = f.dict.tryWriteCopy(f.copyDist, f.copyLen);
        if (cnt == 0) {
            cnt = f.dict.writeCopy(f.copyDist, f.copyLen);
        }
        f.copyLen -= cnt;
        if (f.dict.availWrite() == 0 || f.copyLen > 0) {
            f.toRead = f.dict.readFlush();
            f.step = (Action<ж<decompressor>>)(huffmanBlock); // We need to continue this work
            f.stepState = stateDict;
            return;
        }
        goto readLiteral;
    }
}

// Copy a single uncompressed data block from input to output.
[GoRecv] internal static void dataBlock(this ref decompressor f) {
    // Uncompressed.
    // Discard current half-byte.
    f.nb = 0;
    f.b = 0;
    // Length then ones-complement of length.
    var (nr, err) = io.ReadFull(f.r, f.buf[0..4]);
    f.roffset += (int64)nr;
    if (err != default!) {
        f.err = noEOF(err);
        return;
    }
    nint n = (nint)((nint)f.buf[0] | ((nint)f.buf[1] << (int)(8)));
    nint nn = (nint)((nint)f.buf[2] | ((nint)f.buf[3] << (int)(8)));
    if ((uint16)nn != (uint16)(~n)) {
        f.err = ((CorruptInputError)f.roffset);
        return;
    }
    if (n == 0) {
        f.toRead = f.dict.readFlush();
        f.finishBlock();
        return;
    }
    f.copyLen = n;
    f.copyData();
}

// copyData copies f.copyLen bytes from the underlying reader into f.hist.
// It pauses for reads when f.hist is full.
[GoRecv] internal static void copyData(this ref decompressor f) {
    var buf = f.dict.writeSlice();
    if (len(buf) > f.copyLen) {
        buf = buf[..(int)(f.copyLen)];
    }
    var (cnt, err) = io.ReadFull(f.r, buf);
    f.roffset += (int64)cnt;
    f.copyLen -= cnt;
    f.dict.writeMark(cnt);
    if (err != default!) {
        f.err = noEOF(err);
        return;
    }
    if (f.dict.availWrite() == 0 || f.copyLen > 0) {
        f.toRead = f.dict.readFlush();
        f.step = (Action<ж<decompressor>>)(copyData);
        return;
    }
    f.finishBlock();
}

[GoRecv] internal static void finishBlock(this ref decompressor f) {
    if (f.final) {
        if (f.dict.availRead() > 0) {
            f.toRead = f.dict.readFlush();
        }
        f.err = io.EOF;
    }
    f.step = (Action<ж<decompressor>>)(nextBlock);
}

// noEOF returns err, unless err == io.EOF, in which case it returns io.ErrUnexpectedEOF.
internal static error noEOF(error e) {
    if (AreEqual(e, io.EOF)) {
        return io.ErrUnexpectedEOF;
    }
    return e;
}

[GoRecv] internal static error moreBits(this ref decompressor f) {
    var (c, err) = f.r.ReadByte();
    if (err != default!) {
        return noEOF(err);
    }
    f.roffset++;
    f.b |= (uint32)(((uint32)c).Lsh(f.nb));
    f.nb += 8;
    return default!;
}

// Read the next Huffman-encoded symbol from f according to h.
[GoRecv] internal static (nint, error) huffSym(this ref decompressor f, ж<huffmanDecoder> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    // Since a huffmanDecoder can be empty or be composed of a degenerate tree
    // with single element, huffSym must error on these two edge cases. In both
    // cases, the chunks slice will be 0 for the invalid sequence, leading it
    // satisfy the n == 0 check below.
    nuint n = (nuint)h.min;
    // Optimization. Compiler isn't smart enough to keep f.b,f.nb in registers,
    // but is smart enough to keep local variables in registers, so use nb and b,
    // inline call to moreBits and reassign b,nb back to f on return.
    nuint nb = f.nb;
    var b = f.b;
    while (ᐧ) {
        while (nb < n) {
            var (c, err) = f.r.ReadByte();
            if (err != default!) {
                f.b = b;
                f.nb = nb;
                return (0, noEOF(err));
            }
            f.roffset++;
            b |= (uint32)(((uint32)c << (int)(((nuint)(nb & 31)))));
            nb += 8;
        }
        var chunk = h.chunks[(nint)((uint32)(b & (uint32)((huffmanNumChunks - 1))))];
        n = (nuint)((uint32)(chunk & (uint32)huffmanCountMask));
        if (n > huffmanChunkBits) {
            chunk = h.links[(nint)((chunk >> (int)(huffmanValueShift)))][(nint)((uint32)(((b >> (int)(huffmanChunkBits))) & h.linkMask))];
            n = (nuint)((uint32)(chunk & (uint32)huffmanCountMask));
        }
        if (n <= nb) {
            if (n == 0) {
                f.b = b;
                f.nb = nb;
                f.err = ((CorruptInputError)f.roffset);
                return (0, f.err);
            }
            f.b = (b >> (int)(((nuint)(n & 31))));
            f.nb = nb - n;
            return ((nint)((chunk >> (int)(huffmanValueShift))), default!);
        }
    }
}

[GoRecv] internal static void makeReader(this ref decompressor f, io.Reader r) {
    {
        var (rr, ok) = r._<Reader>(ᐧ); if (ok) {
            f.rBuf = default!;
            f.r = rr;
            return;
        }
    }
    // Reuse rBuf if possible. Invariant: rBuf is always created (and owned) by decompressor.
    if (f.rBuf != nil){
        f.rBuf.Reset(r);
    } else {
        // bufio.NewReader will not return r, as r does not implement flate.Reader, so it is not bufio.Reader.
        f.rBuf = bufio.NewReader(r);
    }
    f.r = new bufio_ReaderжReader(f.rBuf);
}

internal static void fixedHuffmanDecoderInit() {
    ᏑfixedOnce.Do(() => {
        // These come from the RFC section 3.2.6.
        array<nint> bits = new(288);
        for (nint i = 0; i < 144; i++) {
            bits[i] = 8;
        }
        for (nint i = 144; i < 256; i++) {
            bits[i] = 9;
        }
        for (nint i = 256; i < 280; i++) {
            bits[i] = 7;
        }
        for (nint i = 280; i < 288; i++) {
            bits[i] = 8;
        }
        fixedHuffmanDecoder.init(bits[..]);
    });
}

[GoRecv] internal static error Reset(this ref decompressor f, io.Reader r, slice<byte> dict) {
    f = new decompressor(
        rBuf: f.rBuf,
        bits: f.bits,
        codebits: f.codebits,
        dict: f.dict,
        step: (Action<ж<decompressor>>)(nextBlock)
    );
    f.makeReader(r);
    f.dict.init(maxMatchOffset, dict);
    return default!;
}

// NewReader returns a new ReadCloser that can be used
// to read the uncompressed version of r.
// If r does not also implement [io.ByteReader],
// the decompressor may read more data than necessary from r.
// The reader returns [io.EOF] after the final block in the DEFLATE stream has
// been encountered. Any trailing data after the final block is ignored.
//
// The [io.ReadCloser] returned by NewReader also implements [Resetter].
public static io.ReadCloser NewReader(io.Reader r) {
    fixedHuffmanDecoderInit();
    ref var f = ref heap(new decompressor(), out var Ꮡf);
    f.makeReader(r);
    f.bits = Ꮡ(new array<nint>(316));
    f.codebits = Ꮡ(new array<nint>(19));
    f.step = (Action<ж<decompressor>>)(nextBlock);
    f.dict.init(maxMatchOffset, default!);
    return new decompressorжReadCloser(Ꮡf);
}

// NewReaderDict is like [NewReader] but initializes the reader
// with a preset dictionary. The returned [Reader] behaves as if
// the uncompressed data stream started with the given dictionary,
// which has already been read. NewReaderDict is typically used
// to read data compressed by NewWriterDict.
//
// The ReadCloser returned by NewReaderDict also implements [Resetter].
public static io.ReadCloser NewReaderDict(io.Reader r, slice<byte> dict) {
    fixedHuffmanDecoderInit();
    ref var f = ref heap(new decompressor(), out var Ꮡf);
    f.makeReader(r);
    f.bits = Ꮡ(new array<nint>(316));
    f.codebits = Ꮡ(new array<nint>(19));
    f.step = (Action<ж<decompressor>>)(nextBlock);
    f.dict.init(maxMatchOffset, dict);
    return new decompressorжReadCloser(Ꮡf);
}

} // end flate_package
