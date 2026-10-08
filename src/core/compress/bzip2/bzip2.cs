// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bzip2 implements bzip2 decompression.
namespace go.compress;

using io = io_package;

partial class bzip2_package {

partial struct StructuralError /*@string*/;

// There's no RFC for bzip2. I used the Wikipedia page for reference and a lot
// of guessing: https://en.wikipedia.org/wiki/Bzip2
// The source code to pyflate was useful for debugging:
// http://www.paul.sladen.org/projects/pyflate
public static @string Error(this StructuralError s) {
    return "bzip2 data invalid: "u8 + ((@string)s);
}

// A reader decompresses bzip2 compressed data.
partial struct reader {
    internal bitReader br;
    internal uint32 fileCRC;
    internal uint32 blockCRC;
    internal uint32 wantBlockCRC;
    internal bool setupDone; // true if we have parsed the bzip2 header.
    internal bool eof;
    internal nint blockSize;      // blockSize in bytes, i.e. 900 * 1000.
    internal array<nuint> c = new(256); // the ``C'' array for the inverse BWT.
    internal slice<uint32> tt; // mirrors the ``tt'' array in the bzip2 source and contains the P array in the upper 24 bits.
    internal uint32 tPos;    // Index of the next output byte in tt.
    internal slice<uint32> preRLE; // contains the RLE data still to be processed.
    internal nint preRLEUsed;     // number of entries of preRLE used.
    internal nint lastByte;     // the last byte value seen.
    internal nuint byteRepeats;    // the number of repeats of lastByte seen.
    internal nuint repeats;    // the number of copies of lastByte to output.
}

// NewReader returns an io.Reader which decompresses bzip2 data from r.
// If r does not also implement [io.ByteReader],
// the decompressor may read more data than necessary from r.
public static io.Reader NewReader(io.Reader r) {
    var bz2 = @new<reader>();
    bz2.Value.br = newBitReader(r);
    return new readerжReader(bz2);
}

internal static UntypedInt bzip2FileMagic => 0x425a; // "BZ"

internal static UntypedInt bzip2BlockMagic => 0x314159265359;

internal static UntypedInt bzip2FinalMagic => 0x177245385090;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badMagicValueˢ = "bad magic value"u8;
internal static readonly @string nonHuffmanEntropyˢ = "non-Huffman entropy encoding"u8;
internal static readonly @string invalidCompressionLevelˢ = "invalid compression level"u8;

// setup parses the bzip2 header.
internal static error setup(this ж<reader> Ꮡbz2, bool needMagic) {
    ref var bz2 = ref Ꮡbz2.DerefOrNull();

    var br = Ꮡbz2.of(reader.Ꮡbr);
    if (needMagic) {
        nint magic = br.ReadBits(16);
        if (magic != bzip2FileMagic) {
            return ((StructuralError)(@string)badMagicValueˢ);
        }
    }
    nint t = br.ReadBits(8);
    if (t != (rune)'h') {
        return ((StructuralError)(@string)nonHuffmanEntropyˢ);
    }
    nint level = br.ReadBits(8);
    if (level < (rune)'1' || level > (rune)'9') {
        return ((StructuralError)(@string)invalidCompressionLevelˢ);
    }
    bz2.fileCRC = 0;
    bz2.blockSize = 100 * 1000 * (level - (rune)'0');
    if (bz2.blockSize > len(bz2.tt)) {
        bz2.tt = new slice<uint32>(bz2.blockSize);
    }
    return default!;
}

internal static (nint n, error err) Read(this ж<reader> Ꮡbz2, slice<byte> buf) {
    nint n = default!;
    error err = default!;

    ref var bz2 = ref Ꮡbz2.DerefOrNull();
    if (bz2.eof) {
        return (0, io.EOF);
    }
    if (!bz2.setupDone) {
        err = Ꮡbz2.setup(true);
        var brErrΔ1 = bz2.br.Err();
        if (brErrΔ1 != default!) {
            err = brErrΔ1;
        }
        if (err != default!) {
            return (0, err);
        }
        bz2.setupDone = true;
    }
    (n, err) = Ꮡbz2.read(buf);
    var brErr = bz2.br.Err();
    if (brErr != default!) {
        err = brErr;
    }
    return (n, err);
}

internal static nint readFromBlock(this ref reader bz2, slice<byte> buf) {
    // bzip2 is a block based compressor, except that it has a run-length
    // preprocessing step. The block based nature means that we can
    // preallocate fixed-size buffers and reuse them. However, the RLE
    // preprocessing would require allocating huge buffers to store the
    // maximum expansion. Thus we process blocks all at once, except for
    // the RLE which we decompress as required.
    nint n = 0;
    while ((bz2.repeats > 0 || bz2.preRLEUsed < len(bz2.preRLE)) && n < len(buf)) {
        // We have RLE data pending.
        // The run-length encoding works like this:
        // Any sequence of four equal bytes is followed by a length
        // byte which contains the number of repeats of that byte to
        // include. (The number of repeats can be zero.) Because we are
        // decompressing on-demand our state is kept in the reader
        // object.
        if (bz2.repeats > 0) {
            buf[n] = (byte)bz2.lastByte;
            n++;
            bz2.repeats--;
            if (bz2.repeats == 0) {
                bz2.lastByte = -1;
            }
            continue;
        }
        bz2.tPos = bz2.preRLE[bz2.tPos];
        var b = (byte)bz2.tPos;
        bz2.tPos >>= (int)(8);
        bz2.preRLEUsed++;
        if (bz2.byteRepeats == 3) {
            bz2.repeats = (nuint)b;
            bz2.byteRepeats = 0;
            continue;
        }
        if (bz2.lastByte == (nint)b){
            bz2.byteRepeats++;
        } else {
            bz2.byteRepeats = 0;
        }
        bz2.lastByte = (nint)b;
        buf[n] = b;
        n++;
    }
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blockChecksumMismatchˢ = "block checksum mismatch"u8;
internal static readonly @string badMagicValueFoundˢ = "bad magic value found"u8;
internal static readonly @string fileChecksumMismatchˢ = "file checksum mismatch"u8;
internal static readonly @string badMagicValueInˢ = "bad magic value in continuation file"u8;

internal static (nint, error) read(this ж<reader> Ꮡbz2, slice<byte> buf) {
    ref var bz2 = ref Ꮡbz2.DerefOrNull();

    while (ᐧ) {
        nint n = bz2.readFromBlock(buf);
        if (n > 0 || len(buf) == 0) {
            bz2.blockCRC = updateCRC(bz2.blockCRC, buf.slice(0, n));
            return (n, default!);
        }
        // End of block. Check CRC.
        if (bz2.blockCRC != bz2.wantBlockCRC) {
            bz2.br.err = ((StructuralError)(@string)blockChecksumMismatchˢ);
            return (0, bz2.br.err);
        }
        // Find next block.
        var br = Ꮡbz2.of(reader.Ꮡbr);
        var exprᴛ1 = br.ReadBits64(48);
        if (exprᴛ1 == bzip2BlockMagic) {
            var err = Ꮡbz2.readBlock();
            if (err != default!) {
                // Start of block.
                return (0, err);
            }
        }
        else if (exprᴛ1 == bzip2FinalMagic) {
            var wantFileCRC = (uint32)br.ReadBits64(32);
            if ((~br).err != default!) {
                // Check end-of-file CRC.
                return (0, (~br).err);
            }
            if (bz2.fileCRC != wantFileCRC) {
                br.Value.err = ((StructuralError)(@string)fileChecksumMismatchˢ);
                return (0, (~br).err);
            }
            if ((~br).bits % 8 != 0) {
                // Skip ahead to byte boundary.
                // Is there a file concatenated to this one?
                // It would start with BZ.
                br.ReadBits((~br).bits % 8);
            }
            var (b, err) = (~br).r.ReadByte();
            if (AreEqual(err, io.EOF)) {
                br.Value.err = io.EOF;
                bz2.eof = true;
                return (0, io.EOF);
            }
            if (err != default!) {
                br.Value.err = err;
                return (0, err);
            }
            (var z, err) = (~br).r.ReadByte();
            if (err != default!) {
                if (AreEqual(err, io.EOF)) {
                    err = io.ErrUnexpectedEOF;
                }
                br.Value.err = err;
                return (0, err);
            }
            if (b != (rune)'B' || z != (rune)'Z') {
                return (0, ((StructuralError)(@string)badMagicValueInˢ));
            }
            {
                var errΔ1 = Ꮡbz2.setup(false); if (errΔ1 != default!) {
                    return (0, errΔ1);
                }
            }
        }
        else { /* default: */
            return (0, ((StructuralError)(@string)badMagicValueFoundˢ));
        }

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string deprecatedRandomizedˢ = "deprecated randomized files"u8;
internal static readonly @string noSymbolsInInputˢ = "no symbols in input"u8;
internal static readonly @string invalidNumberOfHuffmanˢ = "invalid number of Huffman trees"u8;
internal static readonly @string treeIndexTooLargeˢ = "tree index too large"u8;
internal static readonly @string huffmanLengthOutOfRangeˢ = "Huffman length out of range"u8;
internal static readonly @string noTreeSelectorsGivenˢ = "no tree selectors given"u8;
internal static readonly @string treeSelectorOutOfRangeˢ = "tree selector out of range"u8;
internal static readonly @string insufficientSelectorˢ = "insufficient selector indices for number of symbols"u8;
internal static readonly @string repeatCountTooLargeˢ = "repeat count too large"u8;
internal static readonly @string repeatsPastEndOfBlockˢ = "repeats past end of block"u8;
internal static readonly @string dataExceedsBlockSizeˢ = "data exceeds block size"u8;
internal static readonly @string origPtrOutOfBoundsˢ = "origPtr out of bounds"u8;

// readBlock reads a bzip2 block. The magic number should already have been consumed.
internal static error /*err*/ readBlock(this ж<reader> Ꮡbz2) {
    error err = default!;

    ref var bz2 = ref Ꮡbz2.DerefOrNull();
    var br = Ꮡbz2.of(reader.Ꮡbr);
    bz2.wantBlockCRC = (uint32)br.ReadBits64(32); // skip checksum. TODO: check it if we can figure out what it is.
    bz2.blockCRC = 0;
    bz2.fileCRC = (uint32)(((uint32)((bz2.fileCRC << (int)(1)) | (bz2.fileCRC >> (int)(31)))) ^ bz2.wantBlockCRC);
    nint randomized = br.ReadBits(1);
    if (randomized != 0) {
        return ((StructuralError)(@string)deprecatedRandomizedˢ);
    }
    nuint origPtr = (nuint)br.ReadBits(24);
    // If not every byte value is used in the block (i.e., it's text) then
    // the symbol set is reduced. The symbols used are stored as a
    // two-level, 16x16 bitmap.
    nint symbolRangeUsedBitmap = br.ReadBits(16);
    var symbolPresent = new slice<bool>(256);
    nint numSymbols = 0;
    for (nuint symRange = (nuint)0; symRange < 16; symRange++) {
        if ((nint)(symbolRangeUsedBitmap & (((nint)1).Lsh((15 - symRange)))) != 0) {
            nint bits = br.ReadBits(16);
            for (nuint symbol = (nuint)0; symbol < 16; symbol++) {
                if ((nint)(bits & (((nint)1).Lsh((15 - symbol)))) != 0) {
                    symbolPresent[16 * symRange + symbol] = true;
                    numSymbols++;
                }
            }
        }
    }
    if (numSymbols == 0) {
        // There must be an EOF symbol.
        return ((StructuralError)(@string)noSymbolsInInputˢ);
    }
    // A block uses between two and six different Huffman trees.
    nint numHuffmanTrees = br.ReadBits(3);
    if (numHuffmanTrees < 2 || numHuffmanTrees > 6) {
        return ((StructuralError)(@string)invalidNumberOfHuffmanˢ);
    }
    // The Huffman tree can switch every 50 symbols so there's a list of
    // tree indexes telling us which tree to use for each 50 symbol block.
    nint numSelectors = br.ReadBits(15);
    var treeIndexes = new slice<uint8>(numSelectors);
    // The tree indexes are move-to-front transformed and stored as unary
    // numbers.
    var mtfTreeDecoder = newMTFDecoderWithRange(numHuffmanTrees);
    foreach (var (i, _) in treeIndexes) {
        nint c = 0;
        while (ᐧ) {
            nint inc = br.ReadBits(1);
            if (inc == 0) {
                break;
            }
            c++;
        }
        if (c >= numHuffmanTrees) {
            return ((StructuralError)(@string)treeIndexTooLargeˢ);
        }
        treeIndexes[i] = mtfTreeDecoder.Decode(c);
    }
    // The list of symbols for the move-to-front transform is taken from
    // the previously decoded symbol bitmap.
    var symbols = new slice<byte>(numSymbols);
    nint nextSymbol = 0;
    for (nint i = 0; i < 256; i++) {
        if (symbolPresent[i]) {
            symbols[nextSymbol] = (byte)i;
            nextSymbol++;
        }
    }
    var mtf = newMTFDecoder(symbols);
    numSymbols += 2; // to account for RUNA and RUNB symbols
    var huffmanTrees = new slice<huffmanTree>(numHuffmanTrees);
    // Now we decode the arrays of code-lengths for each tree.
    var lengths = new slice<uint8>(numSymbols);
    foreach (var (i, _) in huffmanTrees) {
        // The code lengths are delta encoded from a 5-bit base value.
        nint length = br.ReadBits(5);
        foreach (var (j, _) in lengths) {
            while (ᐧ) {
                if (length < 1 || length > 20) {
                    return ((StructuralError)(@string)huffmanLengthOutOfRangeˢ);
                }
                if (!br.ReadBit()) {
                    break;
                }
                if (br.ReadBit()){
                    length--;
                } else {
                    length++;
                }
            }
            lengths[j] = (uint8)length;
        }
        (huffmanTrees[i], err) = newHuffmanTree(lengths);
        if (err != default!) {
            return err;
        }
    }
    nint selectorIndex = 1; // the next tree index to use
    if (len(treeIndexes) == 0) {
        return ((StructuralError)(@string)noTreeSelectorsGivenˢ);
    }
    if ((nint)treeIndexes[0] >= len(huffmanTrees)) {
        return ((StructuralError)(@string)treeSelectorOutOfRangeˢ);
    }
    var currentHuffmanTree = huffmanTrees[treeIndexes[0]];
    nint bufIndex = 0; // indexes bz2.buf, the output buffer.
    // The output of the move-to-front transform is run-length encoded and
    // we merge the decoding into the Huffman parsing loop. These two
    // variables accumulate the repeat count. See the Wikipedia page for
    // details.
    nint repeat = 0;
    nint repeatPower = 0;
    // The `C' array (used by the inverse BWT) needs to be zero initialized.
    clear(bz2.c[..]);
    nint decoded = 0; // counts the number of symbols decoded by the current tree.
    while (ᐧ) {
        if (decoded == 50) {
            if (selectorIndex >= numSelectors) {
                return ((StructuralError)(@string)insufficientSelectorˢ);
            }
            if ((nint)treeIndexes[selectorIndex] >= len(huffmanTrees)) {
                return ((StructuralError)(@string)treeSelectorOutOfRangeˢ);
            }
            currentHuffmanTree = huffmanTrees[treeIndexes[selectorIndex]];
            selectorIndex++;
            decoded = 0;
        }
        var v = currentHuffmanTree.Decode(br);
        decoded++;
        if (v < 2) {
            // This is either the RUNA or RUNB symbol.
            if (repeat == 0) {
                repeatPower = 1;
            }
            repeat += repeatPower.Lsh((uint64)(v));
            repeatPower <<= (int)(1);
            // This limit of 2 million comes from the bzip2 source
            // code. It prevents repeat from overflowing.
            if (repeat > 2 * 1024 * 1024) {
                return ((StructuralError)(@string)repeatCountTooLargeˢ);
            }
            continue;
        }
        if (repeat > 0) {
            // We have decoded a complete run-length so we need to
            // replicate the last output symbol.
            if (repeat > bz2.blockSize - bufIndex) {
                return ((StructuralError)(@string)repeatsPastEndOfBlockˢ);
            }
            for (nint i = 0; i < repeat; i++) {
                var bΔ1 = mtf.First();
                bz2.tt[bufIndex] = (uint32)bΔ1;
                bz2.c[bΔ1]++;
                bufIndex++;
            }
            repeat = 0;
        }
        if ((nint)v == numSymbols - 1) {
            // This is the EOF symbol. Because it's always at the
            // end of the move-to-front list, and never gets moved
            // to the front, it has this unique value.
            break;
        }
        // Since two metasymbols (RUNA and RUNB) have values 0 and 1,
        // one would expect |v-2| to be passed to the MTF decoder.
        // However, the front of the MTF list is never referenced as 0,
        // it's always referenced with a run-length of 1. Thus 0
        // doesn't need to be encoded and we have |v-1| in the next
        // line.
        var b = mtf.Decode((nint)((uint16)(v - 1)));
        if (bufIndex >= bz2.blockSize) {
            return ((StructuralError)(@string)dataExceedsBlockSizeˢ);
        }
        bz2.tt[bufIndex] = (uint32)b;
        bz2.c[b]++;
        bufIndex++;
    }
    if (origPtr >= (nuint)bufIndex) {
        return ((StructuralError)(@string)origPtrOutOfBoundsˢ);
    }
    // We have completed the entropy decoding. Now we can perform the
    // inverse BWT and setup the RLE buffer.
    bz2.preRLE = bz2.tt.slice(0, bufIndex);
    bz2.preRLEUsed = 0;
    bz2.tPos = inverseBWT(bz2.preRLE, origPtr, bz2.c[..]);
    bz2.lastByte = -1;
    bz2.byteRepeats = 0;
    bz2.repeats = 0;
    return default!;
}

// inverseBWT implements the inverse Burrows-Wheeler transform as described in
// http://www.hpl.hp.com/techreports/Compaq-DEC/SRC-RR-124.pdf, section 4.2.
// In that document, origPtr is called “I” and c is the “C” array after the
// first pass over the data. It's an argument here because we merge the first
// pass with the Huffman decoding.
//
// This also implements the “single array” method from the bzip2 source code
// which leaves the output, still shuffled, in the bottom 8 bits of tt with the
// index of the next byte in the top 24-bits. The index of the first byte is
// returned.
internal static uint32 inverseBWT(slice<uint32> tt, nuint origPtr, slice<nuint> c) {
    nuint sum = (nuint)0;
    for (nint i = 0; i < 256; i++) {
        sum += c[i];
        c[i] = sum - c[i];
    }
    foreach (var (i, _) in tt) {
        var b = (uint32)(tt[i] & 0xff);
        tt[c[b]] |= (uint32)(((uint32)i << (int)(8)));
        c[b]++;
    }
    return (tt[origPtr] >> (int)(8));
}

// This is a standard CRC32 like in hash/crc32 except that all the shifts are reversed,
// causing the bits in the input to be processed in the reverse of the usual order.
internal static array<uint32> crctab = new(256);

[GoInit] internal static void init() {
    const uint32 poly = 0x04C11DB7;
    foreach (var (i, _) in crctab) {
        var crc = ((uint32)i << (int)(24));
        for (nint j = 0; j < 8; j++) {
            if ((uint32)(crc & 0x80000000U) != 0){
                crc = (uint32)(((crc << (int)(1))) ^ poly);
            } else {
                crc <<= (int)(1);
            }
        }
        crctab[i] = crc;
    }
}

// updateCRC updates the crc value to incorporate the data in b.
// The initial value is 0.
internal static uint32 updateCRC(uint32 val, slice<byte> b) {
    var crc = ~val;
    foreach (var (_, v) in b) {
        crc = (uint32)(crctab[(byte)((byte)((crc >> (int)(24))) ^ v)] ^ ((crc << (int)(8))));
    }
    return ~crc;
}

} // end bzip2_package
