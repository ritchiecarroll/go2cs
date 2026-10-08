// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using io = io_package;

partial class flate_package {

internal static UntypedInt offsetCodeCount => 30;
internal static UntypedInt endBlockMarker => 256;
internal static UntypedInt lengthCodesStart => 257;
internal static UntypedInt codegenCodeCount => 19;
internal static UntypedInt badCode => 255;
internal static UntypedInt bufferFlushSize => 240;
internal static UntypedInt bufferSize => /* bufferFlushSize + 8 */ 248;

/* 257 */
/* 260 */
/* 270 */
/* 280 */
// The number of extra bits needed by length code X - LENGTH_CODES_START.
internal static slice<int8> lengthExtraBits = new int8[]{
    0, 0, 0,
    0, 0, 0, 0, 0, 1, 1, 1, 1, 2,
    2, 2, 2, 3, 3, 3, 3, 4, 4, 4,
    4, 5, 5, 5, 5, 0
}.slice();

// The length indicated by length code X - LENGTH_CODES_START.
internal static slice<uint32> lengthBase = new uint32[]{
    0, 1, 2, 3, 4, 5, 6, 7, 8, 10,
    12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
    64, 80, 96, 112, 128, 160, 192, 224, 255
}.slice();

// offset code word extra bits.
internal static slice<int8> offsetExtraBits = new int8[]{
    0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
    4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
    9, 9, 10, 10, 11, 11, 12, 12, 13, 13
}.slice();

internal static slice<uint32> offsetBase = new uint32[]{
    0x000000, 0x000001, 0x000002, 0x000003, 0x000004,
    0x000006, 0x000008, 0x00000c, 0x000010, 0x000018,
    0x000020, 0x000030, 0x000040, 0x000060, 0x000080,
    0x0000c0, 0x000100, 0x000180, 0x000200, 0x000300,
    0x000400, 0x000600, 0x000800, 0x000c00, 0x001000,
    0x001800, 0x002000, 0x003000, 0x004000, 0x006000
}.slice();

// The odd order in which the codegen code sizes are written.
internal static slice<uint32> codegenOrder = new uint32[]{16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15}.slice();

partial struct huffmanBitWriter {
    // writer is the underlying writer.
    // Do not use it directly; use the write method, which ensures
    // that Write errors are sticky.
    internal io.Writer writer;
    // Data waiting to be written is bytes[0:nbytes]
    // and then the low nbits of bits.  Data is always written
    // sequentially into the bytes array.
    internal uint64 bits;
    internal nuint nbits;
    internal array<byte> bytes = new(bufferSize);
    internal array<int32> codegenFreq = new(codegenCodeCount);
    internal nint nbytes;
    internal slice<int32> literalFreq;
    internal slice<int32> offsetFreq;
    internal slice<uint8> codegen;
    internal ж<huffmanEncoder> literalEncoding;
    internal ж<huffmanEncoder> offsetEncoding;
    internal ж<huffmanEncoder> codegenEncoding;
    internal error err;
}

internal static ж<huffmanBitWriter> newHuffmanBitWriter(io.Writer w) {
    return Ꮡ(new huffmanBitWriter(
        writer: w,
        literalFreq: new slice<int32>(maxNumLit),
        offsetFreq: new slice<int32>(offsetCodeCount),
        codegen: new slice<uint8>(maxNumLit + offsetCodeCount + 1),
        literalEncoding: newHuffmanEncoder(maxNumLit),
        codegenEncoding: newHuffmanEncoder(codegenCodeCount),
        offsetEncoding: newHuffmanEncoder(offsetCodeCount)
    ));
}

internal static void reset(this ref huffmanBitWriter w, io.Writer writer) {
    w.writer = writer;
    (w.bits, w.nbits, w.nbytes, w.err) = (0, 0, 0, default!);
}

internal static void flush(this ref huffmanBitWriter w) {
    if (w.err != default!) {
        w.nbits = 0;
        return;
    }
    nint n = w.nbytes;
    while (w.nbits != 0) {
        w.bytes[n] = (byte)w.bits;
        w.bits >>= (int)(8);
        if (w.nbits > 8){
            // Avoid underflow
            w.nbits -= 8;
        } else {
            w.nbits = 0;
        }
        n++;
    }
    w.bits = 0;
    w.write(w.bytes.slice(0, n));
    w.nbytes = 0;
}

internal static void write(this ref huffmanBitWriter w, slice<byte> b) {
    if (w.err != default!) {
        return;
    }
    (_, w.err) = w.writer.Write(b);
}

internal static void writeBits(this ref huffmanBitWriter w, int32 b, nuint nb) {
    if (w.err != default!) {
        return;
    }
    w.bits |= (uint64)(((uint64)b).Lsh(w.nbits));
    w.nbits += nb;
    if (w.nbits >= 48) {
        var bits = w.bits;
        w.bits >>= (int)(48);
        w.nbits -= 48;
        nint n = w.nbytes;
        var bytes = w.bytes.slice(n, n + 6);
        bytes[0] = (byte)bits;
        bytes[1] = (byte)((bits >> (int)(8)));
        bytes[2] = (byte)((bits >> (int)(16)));
        bytes[3] = (byte)((bits >> (int)(24)));
        bytes[4] = (byte)((bits >> (int)(32)));
        bytes[5] = (byte)((bits >> (int)(40)));
        n += 6;
        if (n >= bufferFlushSize) {
            w.write(w.bytes.slice(0, n));
            n = 0;
        }
        w.nbytes = n;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeBytesWithUnfinishedˢ = "writeBytes with unfinished bits"u8;

internal static void writeBytes(this ref huffmanBitWriter w, slice<byte> bytes) {
    if (w.err != default!) {
        return;
    }
    nint n = w.nbytes;
    if ((nuint)(w.nbits & 7) != 0) {
        w.err = ((InternalError)(@string)writeBytesWithUnfinishedˢ);
        return;
    }
    while (w.nbits != 0) {
        w.bytes[n] = (byte)w.bits;
        w.bits >>= (int)(8);
        w.nbits -= 8;
        n++;
    }
    if (n != 0) {
        w.write(w.bytes.slice(0, n));
    }
    w.nbytes = 0;
    w.write(bytes);
}

// RFC 1951 3.2.7 specifies a special run-length encoding for specifying
// the literal and offset lengths arrays (which are concatenated into a single
// array).  This method generates that run-length encoding.
//
// The result is written into the codegen array, and the frequencies
// of each code is written into the codegenFreq array.
// Codes 0-15 are single byte codes. Codes 16-18 are followed by additional
// information. Code badCode is an end marker
//
//	numLiterals      The number of literals in literalEncoding
//	numOffsets       The number of offsets in offsetEncoding
//	litenc, offenc   The literal and offset encoder to use
internal static void generateCodegen(this ref huffmanBitWriter w, nint numLiterals, nint numOffsets, ж<huffmanEncoder> ᏑlitEnc, ж<huffmanEncoder> ᏑoffEnc) {
    ref var litEnc = ref ᏑlitEnc.DerefOrNull();
    ref var offEnc = ref ᏑoffEnc.DerefOrNull();

    clear(w.codegenFreq[..]);
    // Note that we are using codegen both as a temporary variable for holding
    // a copy of the frequencies, and as the place where we put the result.
    // This is fine because the output is always shorter than the input used
    // so far.
    var codegen = w.codegen; // cache
    // Copy the concatenated code sizes to codegen. Put a marker at the end.
    var cgnl = codegen.slice(0, numLiterals);
    foreach (var (i, _) in cgnl) {
        cgnl[i] = (uint8)litEnc.codes[i].len;
    }
    cgnl = codegen.slice(numLiterals, numLiterals + numOffsets);
    foreach (var (i, _) in cgnl) {
        cgnl[i] = (uint8)offEnc.codes[i].len;
    }
    codegen[numLiterals + numOffsets] = badCode;
    var size = codegen[0];
    nint count = 1;
    nint outIndex = 0;
    for (nint inIndex = 1; size != badCode; inIndex++) {
        // INVARIANT: We have seen "count" copies of size that have not yet
        // had output generated for them.
        var nextSize = codegen[inIndex];
        if (nextSize == size) {
            count++;
            continue;
        }
        // We need to generate codegen indicating "count" of size.
        if (size != 0){
            codegen[outIndex] = size;
            outIndex++;
            w.codegenFreq[size]++;
            count--;
            while (count >= 3) {
                nint n = 6;
                if (n > count) {
                    n = count;
                }
                codegen[outIndex] = 16;
                outIndex++;
                codegen[outIndex] = (uint8)(n - 3);
                outIndex++;
                w.codegenFreq[16]++;
                count -= n;
            }
        } else {
            while (count >= 11) {
                nint n = 138;
                if (n > count) {
                    n = count;
                }
                codegen[outIndex] = 18;
                outIndex++;
                codegen[outIndex] = (uint8)(n - 11);
                outIndex++;
                w.codegenFreq[18]++;
                count -= n;
            }
            if (count >= 3) {
                // count >= 3 && count <= 10
                codegen[outIndex] = 17;
                outIndex++;
                codegen[outIndex] = (uint8)(count - 3);
                outIndex++;
                w.codegenFreq[17]++;
                count = 0;
            }
        }
        count--;
        for (; count >= 0; count--) {
            codegen[outIndex] = size;
            outIndex++;
            w.codegenFreq[size]++;
        }
        // Set up invariant for next time through the loop.
        size = nextSize;
        count = 1;
    }
    // Marker indicating the end of the codegen.
    codegen[outIndex] = badCode;
}

// dynamicSize returns the size of dynamically encoded data in bits.
internal static (nint size, nint numCodegens) dynamicSize(this ref huffmanBitWriter w, ж<huffmanEncoder> ᏑlitEnc, ж<huffmanEncoder> ᏑoffEnc, nint extraBits) {
    nint size = default!;
    nint numCodegens = default!;

    ref var litEnc = ref ᏑlitEnc.DerefOrNull();
    ref var offEnc = ref ᏑoffEnc.DerefOrNull();
    numCodegens = len(w.codegenFreq);
    while (numCodegens > 4 && w.codegenFreq[codegenOrder[numCodegens - 1]] == 0) {
        numCodegens--;
    }
    nint header = 3 + 5 + 5 + 4 + (3 * numCodegens) + w.codegenEncoding.bitLength(w.codegenFreq[..]) + (nint)w.codegenFreq[16] * 2 + (nint)w.codegenFreq[17] * 3 + (nint)w.codegenFreq[18] * 7;
    size = header + litEnc.bitLength(w.literalFreq) + offEnc.bitLength(w.offsetFreq) + extraBits;
    return (size, numCodegens);
}

// fixedSize returns the size of dynamically encoded data in bits.
internal static nint fixedSize(this ref huffmanBitWriter w, nint extraBits) {
    return 3 + fixedLiteralEncoding.bitLength(w.literalFreq) + fixedOffsetEncoding.bitLength(w.offsetFreq) + extraBits;
}

// storedSize calculates the stored size, including header.
// The function returns the size in bits and whether the block
// fits inside a single block.
internal static (nint, bool) storedSize(this ref huffmanBitWriter w, slice<byte> @in) {
    if (@in == default!) {
        return (0, false);
    }
    if (len(@in) <= maxStoreBlockSize) {
        return ((len(@in) + 5) * 8, true);
    }
    return (0, false);
}

internal static void writeCode(this ref huffmanBitWriter w, hcode c) {
    if (w.err != default!) {
        return;
    }
    w.bits |= (uint64)(((uint64)c.code).Lsh(w.nbits));
    w.nbits += (nuint)c.len;
    if (w.nbits >= 48) {
        var bits = w.bits;
        w.bits >>= (int)(48);
        w.nbits -= 48;
        nint n = w.nbytes;
        var bytes = w.bytes.slice(n, n + 6);
        bytes[0] = (byte)bits;
        bytes[1] = (byte)((bits >> (int)(8)));
        bytes[2] = (byte)((bits >> (int)(16)));
        bytes[3] = (byte)((bits >> (int)(24)));
        bytes[4] = (byte)((bits >> (int)(32)));
        bytes[5] = (byte)((bits >> (int)(40)));
        n += 6;
        if (n >= bufferFlushSize) {
            w.write(w.bytes.slice(0, n));
            n = 0;
        }
        w.nbytes = n;
    }
}

// Write the header of a dynamic Huffman block to the output stream.
//
//	numLiterals  The number of literals specified in codegen
//	numOffsets   The number of offsets specified in codegen
//	numCodegens  The number of codegens used in codegen
internal static void writeDynamicHeader(this ref huffmanBitWriter w, nint numLiterals, nint numOffsets, nint numCodegens, bool isEof) {
    if (w.err != default!) {
        return;
    }
    int32 firstBits = 4;
    if (isEof) {
        firstBits = 5;
    }
    w.writeBits(firstBits, 3);
    w.writeBits((int32)(numLiterals - 257), 5);
    w.writeBits((int32)(numOffsets - 1), 5);
    w.writeBits((int32)(numCodegens - 4), 4);
    for (nint iΔ1 = 0; iΔ1 < numCodegens; iΔ1++) {
        nuint value = (nuint)(~w.codegenEncoding).codes[codegenOrder[iΔ1]].len;
        w.writeBits((int32)value, 3);
    }
    nint i = 0;
    while (ᐧ) {
        nint codeWord = (nint)w.codegen[i];
        i++;
        if (codeWord == badCode) {
            break;
        }
        w.writeCode((~w.codegenEncoding).codes[(uint32)codeWord]);
        switch (codeWord) {
        case 16: {
            w.writeBits((int32)w.codegen[i], 2);
            i++;
            break;
        }
        case 17: {
            w.writeBits((int32)w.codegen[i], 3);
            i++;
            break;
        }
        case 18: {
            w.writeBits((int32)w.codegen[i], 7);
            i++;
            break;
        }}

    }
}

internal static void writeStoredHeader(this ref huffmanBitWriter w, nint length, bool isEof) {
    if (w.err != default!) {
        return;
    }
    int32 flag = default!;
    if (isEof) {
        flag = 1;
    }
    w.writeBits(flag, 3);
    w.flush();
    w.writeBits((int32)length, 16);
    w.writeBits((int32)(((uint16)(~(uint16)length))), 16);
}

internal static void writeFixedHeader(this ref huffmanBitWriter w, bool isEof) {
    if (w.err != default!) {
        return;
    }
    // Indicate that we are a fixed Huffman block
    int32 value = 2;
    if (isEof) {
        value = 3;
    }
    w.writeBits(value, 3);
}

// writeBlock will write a block of tokens with the smallest encoding.
// The original input can be supplied, and if the huffman encoded data
// is larger than the original bytes, the data will be written as a
// stored block.
// If the input is nil, the tokens will always be Huffman encoded.
internal static void writeBlock(this ref huffmanBitWriter w, slice<token> tokens, bool eof, slice<byte> input) {
    if (w.err != default!) {
        return;
    }
    tokens = append(tokens, (token)(endBlockMarker));
    var (numLiterals, numOffsets) = w.indexTokens(tokens);
    nint extraBits = default!;
    var (storedSize, storable) = w.storedSize(input);
    if (storable) {
        // We only bother calculating the costs of the extra bits required by
        // the length of offset fields (which will be the same for both fixed
        // and dynamic encoding), if we need to compare those two encodings
        // against stored encoding.
        for (nint lengthCode = lengthCodesStart + 8; lengthCode < numLiterals; lengthCode++) {
            // First eight length codes have extra size = 0.
            extraBits += (nint)w.literalFreq[lengthCode] * (nint)lengthExtraBits[lengthCode - (nint)lengthCodesStart];
        }
        for (nint offsetCode = 4; offsetCode < numOffsets; offsetCode++) {
            // First four offset codes have extra size = 0.
            extraBits += (nint)w.offsetFreq[offsetCode] * (nint)offsetExtraBits[offsetCode];
        }
    }
    // Figure out smallest code.
    // Fixed Huffman baseline.
    ж<huffmanEncoder> literalEncoding = fixedLiteralEncoding;
    ж<huffmanEncoder> offsetEncoding = fixedOffsetEncoding;
    nint size = w.fixedSize(extraBits);
    // Dynamic Huffman?
    nint numCodegens = default!;
    // Generate codegen and codegenFrequencies, which indicates how to encode
    // the literalEncoding and the offsetEncoding.
    w.generateCodegen(numLiterals, numOffsets, w.literalEncoding, w.offsetEncoding);
    w.codegenEncoding.generate(w.codegenFreq[..], 7);
    (var dynamicSize, numCodegens) = w.dynamicSize(w.literalEncoding, w.offsetEncoding, extraBits);
    if (dynamicSize < size) {
        size = dynamicSize;
        literalEncoding = w.literalEncoding;
        offsetEncoding = w.offsetEncoding;
    }
    // Stored bytes?
    if (storable && storedSize < size) {
        w.writeStoredHeader(len(input), eof);
        w.writeBytes(input);
        return;
    }
    // Huffman.
    if (literalEncoding == fixedLiteralEncoding){
        w.writeFixedHeader(eof);
    } else {
        w.writeDynamicHeader(numLiterals, numOffsets, numCodegens, eof);
    }
    // Write the tokens.
    w.writeTokens(tokens, (~literalEncoding).codes, (~offsetEncoding).codes);
}

// writeBlockDynamic encodes a block using a dynamic Huffman table.
// This should be used if the symbols used have a disproportionate
// histogram distribution.
// If input is supplied and the compression savings are below 1/16th of the
// input size the block is stored.
internal static void writeBlockDynamic(this ref huffmanBitWriter w, slice<token> tokens, bool eof, slice<byte> input) {
    if (w.err != default!) {
        return;
    }
    tokens = append(tokens, (token)(endBlockMarker));
    var (numLiterals, numOffsets) = w.indexTokens(tokens);
    // Generate codegen and codegenFrequencies, which indicates how to encode
    // the literalEncoding and the offsetEncoding.
    w.generateCodegen(numLiterals, numOffsets, w.literalEncoding, w.offsetEncoding);
    w.codegenEncoding.generate(w.codegenFreq[..], 7);
    var (size, numCodegens) = w.dynamicSize(w.literalEncoding, w.offsetEncoding, 0);
    // Store bytes, if we don't get a reasonable improvement.
    {
        var (ssize, storable) = w.storedSize(input); if (storable && ssize < (size + (size >> (int)(4)))) {
            w.writeStoredHeader(len(input), eof);
            w.writeBytes(input);
            return;
        }
    }
    // Write Huffman table.
    w.writeDynamicHeader(numLiterals, numOffsets, numCodegens, eof);
    // Write the tokens.
    w.writeTokens(tokens, (~w.literalEncoding).codes, (~w.offsetEncoding).codes);
}

// indexTokens indexes a slice of tokens, and updates
// literalFreq and offsetFreq, and generates literalEncoding
// and offsetEncoding.
// The number of literal and offset tokens is returned.
internal static (nint numLiterals, nint numOffsets) indexTokens(this ref huffmanBitWriter w, slice<token> tokens) {
    nint numLiterals = default!;
    nint numOffsets = default!;

    clear(w.literalFreq);
    clear(w.offsetFreq);
    foreach (var (_, t) in tokens) {
        if (t < matchType) {
            w.literalFreq[t.literal()]++;
            continue;
        }
        var length = t.length();
        var offset = t.offset();
        w.literalFreq[(uint32)lengthCodesStart + lengthCode(length)]++;
        w.offsetFreq[offsetCode(offset)]++;
    }
    // get the number of literals
    numLiterals = len(w.literalFreq);
    while (w.literalFreq[numLiterals - 1] == 0) {
        numLiterals--;
    }
    // get the number of offsets
    numOffsets = len(w.offsetFreq);
    while (numOffsets > 0 && w.offsetFreq[numOffsets - 1] == 0) {
        numOffsets--;
    }
    if (numOffsets == 0) {
        // We haven't found a single match. If we want to go with the dynamic encoding,
        // we should count at least one offset to be sure that the offset huffman tree could be encoded.
        w.offsetFreq[0] = 1;
        numOffsets = 1;
    }
    w.literalEncoding.generate(w.literalFreq, 15);
    w.offsetEncoding.generate(w.offsetFreq, 15);
    return (numLiterals, numOffsets);
}

// writeTokens writes a slice of tokens to the output.
// codes for literal and offset encoding must be supplied.
internal static void writeTokens(this ref huffmanBitWriter w, slice<token> tokens, slice<hcode> leCodes, slice<hcode> oeCodes) {
    if (w.err != default!) {
        return;
    }
    foreach (var (_, t) in tokens) {
        if (t < matchType) {
            w.writeCode(leCodes[t.literal()]);
            continue;
        }
        // Write the length
        var length = t.length();
        var lengthCodeΔ1 = lengthCode(length);
        w.writeCode(leCodes[lengthCodeΔ1 + (uint32)lengthCodesStart]);
        nuint extraLengthBits = (nuint)lengthExtraBits[lengthCodeΔ1];
        if (extraLengthBits > 0) {
            var extraLength = (int32)(length - lengthBase[lengthCodeΔ1]);
            w.writeBits(extraLength, extraLengthBits);
        }
        // Write the offset
        var offset = t.offset();
        var offsetCodeΔ1 = offsetCode(offset);
        w.writeCode(oeCodes[offsetCodeΔ1]);
        nuint extraOffsetBits = (nuint)offsetExtraBits[offsetCodeΔ1];
        if (extraOffsetBits > 0) {
            var extraOffset = (int32)(offset - offsetBase[offsetCodeΔ1]);
            w.writeBits(extraOffset, extraOffsetBits);
        }
    }
}

// huffOffset is a static offset encoder used for huffman only encoding.
// It can be reused since we will not be encoding offset values.
internal static ж<huffmanEncoder> huffOffset;

[GoInit] internal static void init() {
    var offsetFreq = new slice<int32>(offsetCodeCount);
    offsetFreq[0] = 1;
    huffOffset = newHuffmanEncoder(offsetCodeCount);
    huffOffset.generate(offsetFreq, 15);
}

// writeBlockHuff encodes a block of bytes as either
// Huffman encoded literals or uncompressed bytes if the
// results only gains very little from compression.
internal static void writeBlockHuff(this ref huffmanBitWriter w, bool eof, slice<byte> input) {
    if (w.err != default!) {
        return;
    }
    // Clear histogram
    clear(w.literalFreq);
    // Add everything as literals
    histogram(input, w.literalFreq);
    w.literalFreq[endBlockMarker] = 1;
    const nint numLiterals = /* endBlockMarker + 1 */ 257;
    w.offsetFreq[0] = 1;
    const nint numOffsets = 1;
    w.literalEncoding.generate(w.literalFreq, 15);
    // Figure out smallest code.
    // Always use dynamic Huffman or Store
    nint numCodegens = default!;
    // Generate codegen and codegenFrequencies, which indicates how to encode
    // the literalEncoding and the offsetEncoding.
    w.generateCodegen(numLiterals, numOffsets, w.literalEncoding, huffOffset);
    w.codegenEncoding.generate(w.codegenFreq[..], 7);
    (var size, numCodegens) = w.dynamicSize(w.literalEncoding, huffOffset, 0);
    // Store bytes, if we don't get a reasonable improvement.
    {
        var (ssize, storable) = w.storedSize(input); if (storable && ssize < (size + (size >> (int)(4)))) {
            w.writeStoredHeader(len(input), eof);
            w.writeBytes(input);
            return;
        }
    }
    // Huffman.
    w.writeDynamicHeader(numLiterals, numOffsets, numCodegens, eof);
    var encoding = (~w.literalEncoding).codes[..257];
    nint n = w.nbytes;
    foreach (var (_, t) in input) {
        // Bitwriting inlined, ~30% speedup
        var c = encoding[t];
        w.bits |= (uint64)(((uint64)c.code).Lsh(w.nbits));
        w.nbits += (nuint)c.len;
        if (w.nbits < 48) {
            continue;
        }
        // Store 6 bytes
        var bits = w.bits;
        w.bits >>= (int)(48);
        w.nbits -= 48;
        var bytes = w.bytes.slice(n, n + 6);
        bytes[0] = (byte)bits;
        bytes[1] = (byte)((bits >> (int)(8)));
        bytes[2] = (byte)((bits >> (int)(16)));
        bytes[3] = (byte)((bits >> (int)(24)));
        bytes[4] = (byte)((bits >> (int)(32)));
        bytes[5] = (byte)((bits >> (int)(40)));
        n += 6;
        if (n < bufferFlushSize) {
            continue;
        }
        w.write(w.bytes.slice(0, n));
        if (w.err != default!) {
            return; // Return early in the event of write failures
        }
        n = 0;
    }
    w.nbytes = n;
    w.writeCode(encoding[endBlockMarker]);
}

// histogram accumulates a histogram of b in h.
//
// len(h) must be >= 256, and h's elements must be all zeroes.
internal static void histogram(slice<byte> b, slice<int32> h) {
    h = h[..256];
    foreach (var (_, t) in b) {
        h[t]++;
    }
}

} // end flate_package
