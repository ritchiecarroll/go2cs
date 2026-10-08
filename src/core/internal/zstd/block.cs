// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using io = io_package;

partial class zstd_package {

// debug can be set in the source to print debug info using println.
internal const bool debug = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string extraneousDataAfterNoˢ = "extraneous data after no sequences"u8;

// compressedBlock decompresses a compressed block, storing the decompressed
// data in r.buffer. The blockSize argument is the compressed size.
// RFC 3.1.1.3.
internal static error compressedBlock(this ж<Reader> Ꮡr, nint blockSize) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (builtin.len(r.compressedBuf) >= blockSize){
        r.compressedBuf = r.compressedBuf.slice(0, blockSize);
    } else {
        // We know that blockSize <= 128K,
        // so this won't allocate an enormous amount.
        nint need = blockSize - builtin.len(r.compressedBuf);
        r.compressedBuf = appendꓸꓸꓸ(r.compressedBuf, makeꓸꓸꓸ<byte>(need));
    }
    {
        var (_, errΔ1) = io.ReadFull(r.r, r.compressedBuf); if (errΔ1 != default!) {
            return r.wrapNonEOFError(0, errΔ1);
        }
    }
    var data = ((block)r.compressedBuf);
    nint off = 0;
    r.buffer = r.buffer[..0];
    var (litoff, litbuf, err) = Ꮡr.readLiterals(data, off, r.literals[..0]);
    if (err != default!) {
        return err;
    }
    r.literals = litbuf;
    off = litoff;
    (var seqCount, off, err) = Ꮡr.initSeqs(data, off);
    if (err != default!) {
        return err;
    }
    if (seqCount == 0) {
        // No sequences, just literals.
        if (off < builtin.len(data)) {
            return r.makeError(off, extraneousDataAfterNoˢ);
        }
        r.buffer = appendꓸꓸꓸ(r.buffer, litbuf);
        return default!;
    }
    return Ꮡr.execSeqs(data, off, litbuf, seqCount);
}

partial struct seqCode /*num:nint*/;

internal static seqCode seqLiteral => /* iota */ 0;
internal static seqCode seqOffset => 1;
internal static seqCode seqMatch => 2;

// seqCodeInfoData is the information needed to set up seqTables and
// seqTableBits for a particular kind of sequence code.
partial struct seqCodeInfoData {
    internal slice<fseBaselineEntry> predefTable; // predefined FSE
    internal nint predefTableBits;               // number of bits in predefTable
    internal nint maxSym;               // max symbol value in FSE
    internal nint maxBits;               // max bits for FSE
    // toBaseline converts from an FSE table to an FSE baseline table.
    internal Func<ж<Reader>, nint, slice<fseEntry>, slice<fseBaselineEntry>, error> toBaseline;
}

// seqCodeInfo is the seqCodeInfoData for each kind of sequence code.
internal static ж<array<seqCodeInfoData>> ᏑseqCodeInfo = new StandardBox<array<seqCodeInfoData>>(default(array<seqCodeInfoData>));
internal static ref array<seqCodeInfoData> seqCodeInfo => ref ᏑseqCodeInfo.Value;
internal static void initᴛseqCodeInfo() { seqCodeInfo = new golib.SparseArray<seqCodeInfoData>{
    [(int)seqLiteral] = new(
        predefTable: predefinedLiteralTable[..],
        predefTableBits: 6,
        maxSym: 35,
        maxBits: 9,
        toBaseline: ((Func<ж<Reader>, nint, slice<fseEntry>, slice<fseBaselineEntry>, error>)(makeLiteralBaselineFSE))
    ),
    [(int)seqOffset] = new(
        predefTable: predefinedOffsetTable[..],
        predefTableBits: 5,
        maxSym: 31,
        maxBits: 8,
        toBaseline: ((Func<ж<Reader>, nint, slice<fseEntry>, slice<fseBaselineEntry>, error>)(makeOffsetBaselineFSE))
    ),
    [(int)seqMatch] = new(
        predefTable: predefinedMatchTable[..],
        predefTableBits: 6,
        maxSym: 52,
        maxBits: 9,
        toBaseline: ((Func<ж<Reader>, nint, slice<fseEntry>, slice<fseBaselineEntry>, error>)(makeMatchBaselineFSE))
    )
}.array(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidSymbolCompressionˢ = "invalid symbol compression mode"u8;

// initSeqs reads the Sequences_Section_Header and sets up the FSE
// tables used to read the sequence codes. It returns the number of
// sequences and the new offset. RFC 3.1.1.3.2.1.
internal static (nint, nint, error) initSeqs(this ж<Reader> Ꮡr, block data, nint off) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (off >= builtin.len(data)) {
        return (0, 0, r.makeEOFError(off));
    }
    var seqHdr = data[off];
    off++;
    if (seqHdr == 0) {
        return (0, off, default!);
    }
    nint seqCount = default!;
    if (seqHdr < 128){
        seqCount = (nint)seqHdr;
    } else 
    if (seqHdr < 255){
        if (off >= builtin.len(data)) {
            return (0, 0, r.makeEOFError(off));
        }
        seqCount = ((((nint)seqHdr - 128) << (int)(8))) + (nint)data[off];
        off++;
    } else {
        if (off + 1 >= builtin.len(data)) {
            return (0, 0, r.makeEOFError(off));
        }
        seqCount = (nint)data[off] + (((nint)data[off + 1] << (int)(8))) + 0x7f00;
        off += 2;
    }
    // Read the Symbol_Compression_Modes byte.
    if (off >= builtin.len(data)) {
        return (0, 0, r.makeEOFError(off));
    }
    var symMode = data[off];
    if ((byte)(symMode & 3) != 0) {
        return (0, 0, r.makeError(off, invalidSymbolCompressionˢ));
    }
    off++;
    // Set up the FSE tables used to decode the sequence codes.
    error err = default!;
    (off, err) = Ꮡr.setSeqTable(data, off, seqLiteral, (byte)(((symMode >> (int)(6))) & 3));
    if (err != default!) {
        return (0, 0, err);
    }
    (off, err) = Ꮡr.setSeqTable(data, off, seqOffset, (byte)(((symMode >> (int)(4))) & 3));
    if (err != default!) {
        return (0, 0, err);
    }
    (off, err) = Ꮡr.setSeqTable(data, off, seqMatch, (byte)(((symMode >> (int)(2))) & 3));
    if (err != default!) {
        return (0, 0, err);
    }
    return (seqCount, off, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingRepeatSequenceFseˢ = "missing repeat sequence FSE table"u8;

// setSeqTable uses the Compression_Mode in mode to set up r.seqTables and
// r.seqTableBits for kind. We store these in the Reader because one of
// the modes simply reuses the value from the last block in the frame.
internal static (nint, error) setSeqTable(this ж<Reader> Ꮡr, block data, nint off, seqCode kind, byte mode) {
    ref var r = ref Ꮡr.DerefOrNull();

    var info = ᏑseqCodeInfo.at<seqCodeInfoData>(kind);
    switch (mode) {
    case 0: {
        r.seqTables[kind] = info.Value.predefTable;
        r.seqTableBits[kind] = (uint8)(~info).predefTableBits;
        return (off, default!);
    }
    case 1: {
        if (off >= builtin.len(data)) {
            // Predefined_Mode
            // RLE_Mode
            return (0, r.makeEOFError(off));
        }
        var rle = data[off];
        off++;
        var entry = new fseEntry[]{ // Build a simple baseline table that always returns rle.

            new(
                sym: rle,
                bits: 0,
                @base: 0
            )
        }.slice();
        if (cap(r.seqTableBuffers[kind]) == 0) {
            r.seqTableBuffers[kind] = new slice<fseBaselineEntry>(((nint)1).Lsh((int64)((~info).maxBits)));
        }
        r.seqTableBuffers[kind] = r.seqTableBuffers[kind][..1];
        {
            var err = (~info).toBaseline(Ꮡr, off, entry, r.seqTableBuffers[kind]); if (err != default!) {
                return (0, err);
            }
        }
        r.seqTables[kind] = r.seqTableBuffers[kind];
        r.seqTableBits[kind] = 0;
        return (off, default!);
    }
    case 2: {
        if (cap(r.fseScratch) < ((nint)1).Lsh((int64)((~info).maxBits))) {
            // FSE_Compressed_Mode
            r.fseScratch = new slice<fseEntry>(((nint)1).Lsh((int64)((~info).maxBits)));
        }
        r.fseScratch = r.fseScratch.slice(0, ((nint)1).Lsh((int64)((~info).maxBits)));
        var (tableBits, roff, err) = Ꮡr.readFSE(data, off, (~info).maxSym, (~info).maxBits, r.fseScratch);
        if (err != default!) {
            return (0, err);
        }
        r.fseScratch = r.fseScratch.slice(0, ((nint)1).Lsh((int64)(tableBits)));
        if (cap(r.seqTableBuffers[kind]) == 0) {
            r.seqTableBuffers[kind] = new slice<fseBaselineEntry>(((nint)1).Lsh((int64)((~info).maxBits)));
        }
        r.seqTableBuffers[kind] = r.seqTableBuffers[kind].slice(0, ((nint)1).Lsh((int64)(tableBits)));
        {
            var errΔ1 = (~info).toBaseline(Ꮡr, roff, r.fseScratch, r.seqTableBuffers[kind]); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        r.seqTables[kind] = r.seqTableBuffers[kind];
        r.seqTableBits[kind] = (uint8)tableBits;
        return (roff, default!);
    }
    case 3: {
        if (builtin.len(r.seqTables[kind]) == 0) {
            // Repeat_Mode
            return (0, r.makeError(off, missingRepeatSequenceFseˢ));
        }
        return (off, default!);
    }}

    throw panic("unreachable");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string uncompressedSizeTooBigˢ = "uncompressed size too big"u8;
internal static readonly @string literalByteOverflowˢ = "literal byte overflow"u8;
internal static readonly @string extraneousDataAfterˢ = "extraneous data after sequences"u8;

// execSeqs reads and executes the sequences. RFC 3.1.1.3.2.1.2.
internal static error execSeqs(this ж<Reader> Ꮡr, block data, nint off, slice<byte> litbuf, nint seqCount) {
    ref var r = ref Ꮡr.DerefOrNull();

    // Set up the initial states for the sequence code readers.
    ref var rbr = ref heap<reverseBitReader>(out var Ꮡrbr);
    (rbr, var err) = Ꮡr.makeReverseBitReader(data, builtin.len(data) - 1, off);
    if (err != default!) {
        return err;
    }
    (var literalState, err) = rbr.val(r.seqTableBits[seqLiteral]);
    if (err != default!) {
        return err;
    }
    (var offsetState, err) = rbr.val(r.seqTableBits[seqOffset]);
    if (err != default!) {
        return err;
    }
    (var matchState, err) = rbr.val(r.seqTableBits[seqMatch]);
    if (err != default!) {
        return err;
    }
    // Read and perform all the sequences. RFC 3.1.1.4.
    nint seq = 0;
    while (seq < seqCount) {
        if (builtin.len(r.buffer) + builtin.len(litbuf) > (128 << (int)(10))) {
            return rbr.makeError(uncompressedSizeTooBigˢ);
        }
        var ptoffset = Ꮡ(r.seqTables[seqOffset], offsetState);
        var ptmatch = Ꮡ(r.seqTables[seqMatch], matchState);
        var ptliteral = Ꮡ(r.seqTables[seqLiteral], literalState);
        var (add, errΔ1) = rbr.val((~ptoffset).basebits);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        var offset = (~ptoffset).baseline + add;
        (add, errΔ1) = rbr.val((~ptmatch).basebits);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        var match = (~ptmatch).baseline + add;
        (add, errΔ1) = rbr.val((~ptliteral).basebits);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        var literal = (~ptliteral).baseline + add;
        // Handle repeat offsets. RFC 3.1.1.5.
        // See the comment in makeOffsetBaselineFSE.
        if ((~ptoffset).basebits > 1){
            r.repeatedOffset3 = r.repeatedOffset2;
            r.repeatedOffset2 = r.repeatedOffset1;
            r.repeatedOffset1 = offset;
        } else {
            if (literal == 0) {
                offset++;
            }
            switch (offset) {
            case 1: {
                offset = r.repeatedOffset1;
                break;
            }
            case 2: {
                offset = r.repeatedOffset2;
                r.repeatedOffset2 = r.repeatedOffset1;
                r.repeatedOffset1 = offset;
                break;
            }
            case 3: {
                offset = r.repeatedOffset3;
                r.repeatedOffset3 = r.repeatedOffset2;
                r.repeatedOffset2 = r.repeatedOffset1;
                r.repeatedOffset1 = offset;
                break;
            }
            case 4: {
                offset = r.repeatedOffset1 - 1;
                r.repeatedOffset3 = r.repeatedOffset2;
                r.repeatedOffset2 = r.repeatedOffset1;
                r.repeatedOffset1 = offset;
                break;
            }}

        }
        seq++;
        if (seq < seqCount) {
            // Update the states.
            (add, errΔ1) = rbr.val((~ptliteral).bits);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            literalState = (uint32)(~ptliteral).@base + add;
            (add, errΔ1) = rbr.val((~ptmatch).bits);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            matchState = (uint32)(~ptmatch).@base + add;
            (add, errΔ1) = rbr.val((~ptoffset).bits);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            offsetState = (uint32)(~ptoffset).@base + add;
        }
        // The next sequence is now in literal, offset, match.
        if (debug) {
            println((@string)"literal"u8, literal, (@string)"offset"u8, offset, (@string)"match"u8, match);
        }
        // Copy literal bytes from litbuf.
        if (literal > (uint32)builtin.len(litbuf)) {
            return rbr.makeError(literalByteOverflowˢ);
        }
        if (literal > 0) {
            r.buffer = appendꓸꓸꓸ(r.buffer, litbuf.slice(0, (nint)(literal)));
            litbuf = litbuf.slice((nint)(literal));
        }
        if (match > 0) {
            {
                var errΔ2 = r.copyFromWindow(Ꮡrbr, offset, match); if (errΔ2 != default!) {
                    return errΔ2;
                }
            }
        }
    }
    r.buffer = appendꓸꓸꓸ(r.buffer, litbuf);
    if (rbr.cnt != 0) {
        return r.makeError(off, extraneousDataAfterˢ);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidZeroOffsetˢ = "invalid zero offset"u8;
internal static readonly @string offsetPastWindowˢ = "offset past window"u8;

// Copy match bytes from the decoded output, or the window, at offset.
internal static error copyFromWindow(this ref Reader r, ж<reverseBitReader> Ꮡrbr, uint32 offset, uint32 match) {
    ref var rbr = ref Ꮡrbr.DerefOrNull();

    if (offset == 0) {
        return rbr.makeError(invalidZeroOffsetˢ);
    }
    // Offset may point into the buffer or the window and
    // match may extend past the end of the initial buffer.
    // |--r.window--|--r.buffer--|
    //        |<-----offset------|
    //        |------match----------->|
    var bufferOffset = (uint32)0;
    var lenBlock = (uint32)builtin.len(r.buffer);
    if (lenBlock < offset){
        var lenWindow = r.window.len();
        var copy = offset - lenBlock;
        if (copy > lenWindow) {
            return rbr.makeError(offsetPastWindowˢ);
        }
        var windowOffset = lenWindow - copy;
        if (copy > match) {
            copy = match;
        }
        r.buffer = r.window.appendTo(r.buffer, windowOffset, windowOffset + copy);
        match -= copy;
    } else {
        bufferOffset = lenBlock - offset;
    }
    // We are being asked to copy data that we are adding to the
    // buffer in the same copy.
    while (match > 0) {
        var copy = (uint32)builtin.len(r.buffer) - bufferOffset;
        if (copy > match) {
            copy = match;
        }
        r.buffer = appendꓸꓸꓸ(r.buffer, r.buffer.slice((nint)(bufferOffset), (nint)(bufferOffset + copy)));
        match -= copy;
    }
    return default!;
}

} // end zstd_package
