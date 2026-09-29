// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;

partial class flate_package {

public static UntypedInt NoCompression => 0;
public static UntypedInt BestSpeed => 1;
public static UntypedInt BestCompression => 9;
public static UntypedInt DefaultCompression => -1;
public static UntypedInt HuffmanOnly => -2;

internal static UntypedInt logWindowSize => 15;
internal static UntypedInt windowSize => /* 1 << logWindowSize */ 32768;
internal static UntypedInt windowMask => /* windowSize - 1 */ 32767;
internal static UntypedInt baseMatchLength => 3; // The smallest match length per the RFC section 3.2.5
internal static UntypedInt minMatchLength => 4; // The smallest match length that the compressor actually emits
internal static UntypedInt maxMatchLength => 258; // The largest match length
internal static UntypedInt baseMatchOffset => 1; // The smallest match offset
internal static UntypedInt maxMatchOffset => /* 1 << 15 */ 32768; // The largest match offset
internal static UntypedInt maxFlateBlockTokens => /* 1 << 14 */ 16384;
internal static UntypedInt maxStoreBlockSize => 65535;
internal static UntypedInt hashBits => 17; // After 17 performance degrades
internal static UntypedInt hashSize => /* 1 << hashBits */ 131072;
internal static UntypedInt hashMask => /* (1 << hashBits) - 1 */ 131071;
internal static UntypedInt maxHashOffset => /* 1 << 24 */ 16777216;
internal static UntypedInt skipNever => /* math.MaxInt32 */ 2147483647;

[GoType] partial struct compressionLevel {
    internal nint level, good, lazy, nice, chain, fastSkipHashing;
}

// NoCompression.
// BestSpeed uses a custom algorithm; see deflatefast.go.
// For levels 2-3 we don't bother trying with lazy matches.
// Levels 4-9 use increasingly more lazy matching
// and increasingly stringent conditions for "good enough".
internal static slice<compressionLevel> levels = new compressionLevel[]{
    new(0, 0, 0, 0, 0, 0),
    new(1, 0, 0, 0, 0, 0),
    new(2, 4, 0, 16, 8, 5),
    new(3, 4, 0, 32, 32, 6),
    new(4, 4, 4, 16, 16, skipNever),
    new(5, 8, 16, 32, 32, skipNever),
    new(6, 8, 16, 128, 128, skipNever),
    new(7, 8, 32, 128, 256, skipNever),
    new(8, 32, 128, 258, 1024, skipNever),
    new(9, 32, 258, 258, 4096, skipNever)
}.slice();

[GoType] partial struct compressor {
    internal partial ref compressionLevel compressionLevel { get; }
    internal ж<huffmanBitWriter> w;
    internal Action<slice<byte>, slice<uint32>> bulkHasher;
    // compression algorithm
    internal Func<ж<compressor>, slice<byte>, nint> fill; // copy data to window
    internal Action<ж<compressor>> step;        // process window
    internal ж<deflateFast> bestSpeed;               // Encoder for BestSpeed
    // Input hash chains
    // hashHead[hashValue] contains the largest inputIndex with the specified hash value
    // If hashHead[hashValue] is within the current window, then
    // hashPrev[hashHead[hashValue] & windowMask] contains the previous index
    // with the same hash value.
    internal nint chainHead;
    internal array<uint32> hashHead = new(hashSize);
    internal array<uint32> hashPrev = new(windowSize);
    internal nint hashOffset;
    // input window: unprocessed data is window[index:windowEnd]
    internal nint index;
    internal slice<byte> window;
    internal nint windowEnd;
    internal nint blockStart; // window index where current tokens start
    internal bool byteAvailable; // if true, still need to process window[index-1].
    internal bool sync; // requesting flush
    // queued output tokens
    internal slice<token> tokens;
    // deflate state
    internal nint length;
    internal nint offset;
    internal nint maxInsertIndex;
    internal error err;
    // hashMatch must be able to contain hashes for the maximum match length.
    internal array<uint32> hashMatch = new(maxMatchLength - 1);
}

[GoRecv] internal static nint fillDeflate(this ref compressor d, slice<byte> b) {
    if (d.index >= (nint)(2 * windowSize - (minMatchLength + maxMatchLength))) {
        // shift the window by windowSize
        copy(d.window, d.window[(int)(windowSize)..(int)(2 * windowSize)]);
        d.index -= windowSize;
        d.windowEnd -= windowSize;
        if (d.blockStart >= windowSize){
            d.blockStart -= windowSize;
        } else {
            d.blockStart = math.MaxInt32;
        }
        d.hashOffset += windowSize;
        if (d.hashOffset > maxHashOffset) {
            nint delta = d.hashOffset - 1;
            d.hashOffset -= delta;
            d.chainHead -= delta;
            // Iterate over slices instead of arrays to avoid copying
            // the entire table onto the stack (Issue #18625).
            foreach (var (i, v) in d.hashPrev[..]) {
                if ((nint)v > delta){
                    d.hashPrev[i] = (uint32)((nint)v - delta);
                } else {
                    d.hashPrev[i] = 0;
                }
            }
            foreach (var (i, v) in d.hashHead[..]) {
                if ((nint)v > delta){
                    d.hashHead[i] = (uint32)((nint)v - delta);
                } else {
                    d.hashHead[i] = 0;
                }
            }
        }
    }
    nint n = copy(d.window.slice(d.windowEnd), b);
    d.windowEnd += n;
    return n;
}

[GoRecv] internal static error writeBlock(this ref compressor d, slice<token> tokens, nint index) {
    if (index > 0) {
        slice<byte> window = default!;
        if (d.blockStart <= index) {
            window = d.window.slice(d.blockStart, index);
        }
        d.blockStart = index;
        d.w.writeBlock(tokens, false, window);
        return (~d.w).err;
    }
    return default!;
}

// fillWindow will fill the current window with the supplied
// dictionary and calculate all hashes.
// This is much faster than doing a full encode.
// Should only be used after a reset.
[GoRecv] internal static void fillWindow(this ref compressor d, slice<byte> b) {
    // Do not fill window if we are in store-only mode.
    if (d.compressionLevel.level < 2) {
        return;
    }
    if (d.index != 0 || d.windowEnd != 0) {
        throw panic("internal error: fillWindow called with stale data");
    }
    // If we are given too much, cut it.
    if (len(b) > windowSize) {
        b = b.slice(len(b) - (nint)windowSize);
    }
    // Add all to window.
    nint n = copy(d.window, b);
    // Calculate 256 hashes at the time (more L1 cache hits)
    nint loops = (n + 256 - (nint)minMatchLength) / 256;
    for (nint j = 0; j < loops; j++) {
        nint index = j * 256;
        nint end = index + 256 + (nint)minMatchLength - 1;
        if (end > n) {
            end = n;
        }
        var toCheck = d.window.slice(index, end);
        nint dstSize = len(toCheck) - (nint)minMatchLength + 1;
        if (dstSize <= 0) {
            continue;
        }
        var dst = d.hashMatch.slice(0, dstSize);
        d.bulkHasher(toCheck, dst);
        foreach (var (i, val) in dst) {
            nint di = i + index;
            var hh = Ꮡ(d.hashHead, (uint32)(val & (uint32)hashMask));
            // Get previous value with the same hash.
            // Our chain should point to the previous value.
            d.hashPrev[(nint)(di & (nint)windowMask)] = hh.Value;
            // Set the head of the hash chain to us.
            hh.Value = (uint32)(di + d.hashOffset);
        }
    }
    // Update window information.
    d.windowEnd = n;
    d.index = n;
}

// Try to find a match starting at index whose length is greater than prevSize.
// We only look at chainCount possibilities before giving up.
[GoRecv] internal static (nint length, nint offset, bool ok) findMatch(this ref compressor d, nint pos, nint prevHead, nint prevLength, nint lookahead) {
    nint length = default!;
    nint offset = default!;
    bool ok = default!;

    nint minMatchLook = maxMatchLength;
    if (lookahead < minMatchLook) {
        minMatchLook = lookahead;
    }
    var win = d.window.slice(0, pos + minMatchLook);
    // We quit when we get a match that's at least nice long
    nint nice = len(win) - pos;
    if (d.nice < nice) {
        nice = d.nice;
    }
    // If we've got a match that's good enough, only look in 1/4 the chain.
    nint tries = d.chain;
    length = prevLength;
    if (length >= d.good) {
        tries >>= (int)(2);
    }
    var wEnd = win[pos + length];
    var wPos = win.slice(pos);
    nint minIndex = pos - (nint)windowSize;
    for (nint i = prevHead; tries > 0; tries--) {
        if (wEnd == win[i + length]) {
            nint n = matchLen(win.slice(i), wPos, minMatchLook);
            if (n > length && (n > minMatchLength || pos - i <= 4096)) {
                length = n;
                offset = pos - i;
                ok = true;
                if (n >= nice) {
                    // The match is good enough that we don't try to find a better one.
                    break;
                }
                wEnd = win[pos + n];
            }
        }
        if (i == minIndex) {
            // hashPrev[i & windowMask] has already been overwritten, so stop now.
            break;
        }
        i = (nint)d.hashPrev[(nint)(i & (nint)windowMask)] - d.hashOffset;
        if (i < minIndex || i < 0) {
            break;
        }
    }
    return (length, offset, ok);
}

[GoRecv] internal static error writeStoredBlock(this ref compressor d, slice<byte> buf) {
    {
        d.w.writeStoredHeader(len(buf), false); if ((~d.w).err != default!) {
            return (~d.w).err;
        }
    }
    d.w.writeBytes(buf);
    return (~d.w).err;
}

internal static UntypedInt hashmul => 0x1e35a7bd;

// hash4 returns a hash representation of the first 4 bytes
// of the supplied slice.
// The caller must ensure that len(b) >= 4.
internal static uint32 hash4(slice<byte> b) {
    return ((((uint32)((uint32)((uint32)((uint32)b[3] | ((uint32)b[2] << (int)(8))) | ((uint32)b[1] << (int)(16))) | ((uint32)b[0] << (int)(24)))) * (uint32)hashmul) >> (int)((32 - hashBits)));
}

// bulkHash4 will compute hashes using the same
// algorithm as hash4.
internal static void bulkHash4(slice<byte> b, slice<uint32> dst) {
    if (len(b) < minMatchLength) {
        return;
    }
    var hb = (uint32)((uint32)((uint32)((uint32)b[3] | ((uint32)b[2] << (int)(8))) | ((uint32)b[1] << (int)(16))) | ((uint32)b[0] << (int)(24)));
    dst[0] = ((hb * (uint32)hashmul) >> (int)((32 - hashBits)));
    nint end = len(b) - (nint)minMatchLength + 1;
    for (nint i = 1; i < end; i++) {
        hb = (uint32)(((hb << (int)(8))) | (uint32)b[i + 3]);
        dst[i] = ((hb * (uint32)hashmul) >> (int)((32 - hashBits)));
    }
}

// matchLen returns the number of matching bytes in a and b
// up to length 'max'. Both slices must be at least 'max'
// bytes in size.
internal static nint matchLen(slice<byte> a, slice<byte> b, nint max) {
    a = a.slice(0, max);
    b = b.slice(0, len(a));
    foreach (var (i, av) in a) {
        if (b[i] != av) {
            return i;
        }
    }
    return max;
}

// encSpeed will compress and store the currently added data,
// if enough has been accumulated or we at the end of the stream.
// Any error that occurred will be in d.err
[GoRecv] internal static void encSpeed(this ref compressor d) {
    // We only compress if we have maxStoreBlockSize.
    if (d.windowEnd < maxStoreBlockSize) {
        if (!d.sync) {
            return;
        }
        // Handle small sizes.
        if (d.windowEnd < 128) {
            switch (ᐧ) {
            case {} when d.windowEnd is 0: {
                return;
            }
            case {} when d.windowEnd is <= 16: {
                d.err = d.writeStoredBlock(d.window.slice(0, d.windowEnd));
                break;
            }
            default: {
                d.w.writeBlockHuff(false, d.window.slice(0, d.windowEnd));
                d.err = d.w.Value.err;
                break;
            }}

            d.windowEnd = 0;
            d.bestSpeed.reset();
            return;
        }
    }
    // Encode the block.
    d.tokens = d.bestSpeed.encode(d.tokens[..0], d.window.slice(0, d.windowEnd));
    // If we removed less than 1/16th, Huffman compress the block.
    if (len(d.tokens) > d.windowEnd - ((d.windowEnd >> (int)(4)))){
        d.w.writeBlockHuff(false, d.window.slice(0, d.windowEnd));
    } else {
        d.w.writeBlockDynamic(d.tokens, false, d.window.slice(0, d.windowEnd));
    }
    d.err = d.w.Value.err;
    d.windowEnd = 0;
}

[GoRecv] internal static void initDeflate(this ref compressor d) {
    d.window = new slice<byte>(2 * windowSize);
    d.hashOffset = 1;
    d.tokens = new slice<token>(0, maxFlateBlockTokens + 1);
    d.length = minMatchLength - 1;
    d.offset = 0;
    d.byteAvailable = false;
    d.index = 0;
    d.chainHead = -1;
    d.bulkHasher = bulkHash4;
}

[GoRecv] internal static void deflate(this ref compressor d) {
    if (d.windowEnd - d.index < (nint)(minMatchLength + maxMatchLength) && !d.sync) {
        return;
    }
    d.maxInsertIndex = d.windowEnd - (nint)(minMatchLength - 1);
Loop:
    while (ᐧ) {
        if (d.index > d.windowEnd) {
            throw panic("index > windowEnd");
        }
        nint lookahead = d.windowEnd - d.index;
        if (lookahead < (nint)(minMatchLength + maxMatchLength)) {
            if (!d.sync) {
                goto break_Loop;
            }
            if (d.index > d.windowEnd) {
                throw panic("index > windowEnd");
            }
            if (lookahead == 0) {
                // Flush current output block if any.
                if (d.byteAvailable) {
                    // There is still one pending token that needs to be flushed
                    d.tokens = append(d.tokens, literalToken((uint32)d.window[d.index - 1]));
                    d.byteAvailable = false;
                }
                if (len(d.tokens) > 0) {
                    {
                        d.err = d.writeBlock(d.tokens, d.index); if (d.err != default!) {
                            return;
                        }
                    }
                    d.tokens = d.tokens[..0];
                }
                goto break_Loop;
            }
        }
        if (d.index < d.maxInsertIndex) {
            // Update the hash
            var hash = hash4(d.window.slice(d.index, d.index + (nint)minMatchLength));
            var hh = Ꮡ(d.hashHead, (uint32)(hash & (uint32)hashMask));
            d.chainHead = (nint)(hh.Value);
            d.hashPrev[(nint)(d.index & (nint)windowMask)] = (uint32)d.chainHead;
            hh.Value = (uint32)(d.index + d.hashOffset);
        }
        nint prevLength = d.length;
        nint prevOffset = d.offset;
        d.length = minMatchLength - 1;
        d.offset = 0;
        nint minIndex = d.index - (nint)windowSize;
        if (minIndex < 0) {
            minIndex = 0;
        }
        if (d.chainHead - d.hashOffset >= minIndex && (d.fastSkipHashing != skipNever && lookahead > (nint)(minMatchLength - 1) || d.fastSkipHashing == skipNever && lookahead > prevLength && prevLength < d.lazy)) {
            {
                var (newLength, newOffset, ok) = d.findMatch(d.index, d.chainHead - d.hashOffset, minMatchLength - 1, lookahead); if (ok) {
                    d.length = newLength;
                    d.offset = newOffset;
                }
            }
        }
        if (d.fastSkipHashing != skipNever && d.length >= minMatchLength || d.fastSkipHashing == skipNever && prevLength >= minMatchLength && d.length <= prevLength){
            // There was a match at the previous step, and the current match is
            // not better. Output the previous match.
            if (d.fastSkipHashing != skipNever){
                d.tokens = append(d.tokens, matchToken((uint32)(d.length - (nint)baseMatchLength), (uint32)(d.offset - (nint)baseMatchOffset)));
            } else {
                d.tokens = append(d.tokens, matchToken((uint32)(prevLength - (nint)baseMatchLength), (uint32)(prevOffset - (nint)baseMatchOffset)));
            }
            // Insert in the hash table all strings up to the end of the match.
            // index and index-1 are already inserted. If there is not enough
            // lookahead, the last two strings are not inserted into the hash
            // table.
            if (d.length <= d.fastSkipHashing){
                nint newIndex = default!;
                if (d.fastSkipHashing != skipNever){
                    newIndex = d.index + d.length;
                } else {
                    newIndex = d.index + prevLength - 1;
                }
                nint index = d.index;
                for (index++; index < newIndex; index++) {
                    if (index < d.maxInsertIndex) {
                        var hash = hash4(d.window.slice(index, index + (nint)minMatchLength));
                        // Get previous value with the same hash.
                        // Our chain should point to the previous value.
                        var hh = Ꮡ(d.hashHead, (uint32)(hash & (uint32)hashMask));
                        d.hashPrev[(nint)(index & (nint)windowMask)] = hh.Value;
                        // Set the head of the hash chain to us.
                        hh.Value = (uint32)(index + d.hashOffset);
                    }
                }
                d.index = index;
                if (d.fastSkipHashing == skipNever) {
                    d.byteAvailable = false;
                    d.length = minMatchLength - 1;
                }
            } else {
                // For matches this long, we don't bother inserting each individual
                // item into the table.
                d.index += d.length;
            }
            if (len(d.tokens) == maxFlateBlockTokens) {
                // The block includes the current character
                {
                    d.err = d.writeBlock(d.tokens, d.index); if (d.err != default!) {
                        return;
                    }
                }
                d.tokens = d.tokens[..0];
            }
        } else {
            if (d.fastSkipHashing != skipNever || d.byteAvailable) {
                nint i = d.index - 1;
                if (d.fastSkipHashing != skipNever) {
                    i = d.index;
                }
                d.tokens = append(d.tokens, literalToken((uint32)d.window[i]));
                if (len(d.tokens) == maxFlateBlockTokens) {
                    {
                        d.err = d.writeBlock(d.tokens, i + 1); if (d.err != default!) {
                            return;
                        }
                    }
                    d.tokens = d.tokens[..0];
                }
            }
            d.index++;
            if (d.fastSkipHashing == skipNever) {
                d.byteAvailable = true;
            }
        }
continue_Loop:;
    }
break_Loop:;
}

[GoRecv] internal static nint fillStore(this ref compressor d, slice<byte> b) {
    nint n = copy(d.window.slice(d.windowEnd), b);
    d.windowEnd += n;
    return n;
}

[GoRecv] internal static void store(this ref compressor d) {
    if (d.windowEnd > 0 && (d.windowEnd == maxStoreBlockSize || d.sync)) {
        d.err = d.writeStoredBlock(d.window.slice(0, d.windowEnd));
        d.windowEnd = 0;
    }
}

// storeHuff compresses and stores the currently added data
// when the d.window is full or we are at the end of the stream.
// Any error that occurred will be in d.err
[GoRecv] internal static void storeHuff(this ref compressor d) {
    if (d.windowEnd < len(d.window) && !d.sync || d.windowEnd == 0) {
        return;
    }
    d.w.writeBlockHuff(false, d.window.slice(0, d.windowEnd));
    d.err = d.w.Value.err;
    d.windowEnd = 0;
}

internal static (nint n, error err) write(this ж<compressor> Ꮡd, slice<byte> b) {
    nint n = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    if (d.err != default!) {
        return (0, d.err);
    }
    n = len(b);
    while (len(b) > 0) {
        d.step(Ꮡd);
        b = b.slice(d.fill(Ꮡd, b));
        if (d.err != default!) {
            return (0, d.err);
        }
    }
    return (n, default!);
}

internal static error syncFlush(this ж<compressor> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    if (d.err != default!) {
        return d.err;
    }
    d.sync = true;
    d.step(Ꮡd);
    if (d.err == default!) {
        d.w.writeStoredHeader(0, false);
        d.w.flush();
        d.err = d.w.Value.err;
    }
    d.sync = false;
    return d.err;
}

[GoRecv] internal static error /*err*/ init(this ref compressor d, io.Writer w, nint level) {
    d.w = newHuffmanBitWriter(w);
    var matchᴛ1 = false;
    if (level == NoCompression) { matchᴛ1 = true;
        d.window = new slice<byte>(maxStoreBlockSize);
        d.fill = ((Func<ж<compressor>, slice<byte>, nint>)(fillStore));
        d.step = ((Action<ж<compressor>>)(store));
    }
    else if (level == HuffmanOnly) { matchᴛ1 = true;
        d.window = new slice<byte>(maxStoreBlockSize);
        d.fill = ((Func<ж<compressor>, slice<byte>, nint>)(fillStore));
        d.step = ((Action<ж<compressor>>)(storeHuff));
    }
    else if (level == BestSpeed) { matchᴛ1 = true;
        d.compressionLevel = levels[level];
        d.window = new slice<byte>(maxStoreBlockSize);
        d.fill = ((Func<ж<compressor>, slice<byte>, nint>)(fillStore));
        d.step = ((Action<ж<compressor>>)(encSpeed));
        d.bestSpeed = newDeflateFast();
        d.tokens = new slice<token>(maxStoreBlockSize);
    }
    else if (level == DefaultCompression) { matchᴛ1 = true;
        level = 6;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && (2 <= level && level <= 9)) {
        d.compressionLevel = levels[level];
        d.initDeflate();
        d.fill = ((Func<ж<compressor>, slice<byte>, nint>)(fillDeflate));
        d.step = ((Action<ж<compressor>>)(deflate));
    }
    else if (!matchᴛ1) { /* default: */
        return fmt.Errorf("flate: invalid compression level %d: want value in range [-2, 9]"u8, level);
    }

    return default!;
}

[GoRecv] internal static void reset(this ref compressor d, io.Writer w) {
    d.w.reset(w);
    d.sync = false;
    d.err = default!;
    var exprᴛ1 = d.compressionLevel.level;
    if (exprᴛ1 == NoCompression) {
        d.windowEnd = 0;
    }
    else if (exprᴛ1 == BestSpeed) {
        d.windowEnd = 0;
        d.tokens = d.tokens[..0];
        d.bestSpeed.reset();
    }
    else { /* default: */
        d.chainHead = -1;
        clear(d.hashHead[..]);
        clear(d.hashPrev[..]);
        d.hashOffset = 1;
        (d.index, d.windowEnd) = (0, 0);
        (d.blockStart, d.byteAvailable) = (0, false);
        d.tokens = d.tokens[..0];
        d.length = minMatchLength - 1;
        d.offset = 0;
        d.maxInsertIndex = 0;
    }

}

internal static error close(this ж<compressor> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    if (AreEqual(d.err, errWriterClosed)) {
        return default!;
    }
    if (d.err != default!) {
        return d.err;
    }
    d.sync = true;
    d.step(Ꮡd);
    if (d.err != default!) {
        return d.err;
    }
    {
        d.w.writeStoredHeader(0, true); if ((~d.w).err != default!) {
            return (~d.w).err;
        }
    }
    d.w.flush();
    if ((~d.w).err != default!) {
        return (~d.w).err;
    }
    d.err = errWriterClosed;
    return default!;
}

// NewWriter returns a new [Writer] compressing data at the given level.
// Following zlib, levels range from 1 ([BestSpeed]) to 9 ([BestCompression]);
// higher levels typically run slower but compress more. Level 0
// ([NoCompression]) does not attempt any compression; it only adds the
// necessary DEFLATE framing.
// Level -1 ([DefaultCompression]) uses the default compression level.
// Level -2 ([HuffmanOnly]) will use Huffman compression only, giving
// a very fast compression for all types of input, but sacrificing considerable
// compression efficiency.
//
// If level is in the range [-2, 9] then the error returned will be nil.
// Otherwise the error returned will be non-nil.
public static (ж<Writer>, error) NewWriter(io.Writer w, nint level) {
    ref var dw = ref heap(new Writer(), out var Ꮡdw);
    {
        var err = dw.d.init(w, level); if (err != default!) {
            return (default!, err);
        }
    }
    return (Ꮡdw, default!);
}

// NewWriterDict is like [NewWriter] but initializes the new
// [Writer] with a preset dictionary. The returned [Writer] behaves
// as if the dictionary had been written to it without producing
// any compressed output. The compressed data written to w
// can only be decompressed by a [Reader] initialized with the
// same dictionary.
public static (ж<Writer>, error) NewWriterDict(io.Writer w, nint level, slice<byte> dict) {
    var dw = Ꮡ(new dictWriter(w));
    var (zw, err) = NewWriter(new dictWriterжWriter(dw), level);
    if (err != default!) {
        return (default!, err);
    }
    zw.of(Writer.Ꮡd).fillWindow(dict);
    zw.Value.dict = appendꓸꓸꓸ((~zw).dict, dict); // duplicate dictionary for Reset method.
    return (zw, err);
}

[GoType] partial struct dictWriter {
    internal io.Writer w;
}

[GoRecv] internal static (nint n, error err) Write(this ref dictWriter w, slice<byte> b) {
    return w.w.Write(b);
}

internal static error errWriterClosed = errors.New("flate: closed writer"u8);

// A Writer takes data written to it and writes the compressed
// form of that data to an underlying writer (see [NewWriter]).
[GoType] partial struct Writer {
    internal compressor d;
    internal slice<byte> dict;
}

// Write writes data to w, which will eventually write the
// compressed form of data to its underlying writer.
public static (nint n, error err) Write(this ж<Writer> Ꮡw, slice<byte> data) {
    return Ꮡw.of(Writer.Ꮡd).write(data);
}

// Flush flushes any pending data to the underlying writer.
// It is useful mainly in compressed network protocols, to ensure that
// a remote reader has enough data to reconstruct a packet.
// Flush does not return until the data has been written.
// Calling Flush when there is no pending data still causes the [Writer]
// to emit a sync marker of at least 4 bytes.
// If the underlying writer returns an error, Flush returns that error.
//
// In the terminology of the zlib library, Flush is equivalent to Z_SYNC_FLUSH.
public static error Flush(this ж<Writer> Ꮡw) {
    // For more about flushing:
    // https://www.bolet.org/~pornin/deflate-flush.html
    return Ꮡw.of(Writer.Ꮡd).syncFlush();
}

// Close flushes and closes the writer.
public static error Close(this ж<Writer> Ꮡw) {
    return Ꮡw.of(Writer.Ꮡd).close();
}

// Reset discards the writer's state and makes it equivalent to
// the result of [NewWriter] or [NewWriterDict] called with dst
// and w's level and dictionary.
[GoRecv] public static void Reset(this ref Writer w, io.Writer dst) {
    {
        var (dw, ok) = (~w.d.w).writer._<ж<dictWriter>>(ᐧ); if (ok){
            // w was created with NewWriterDict
            dw.Value.w = dst;
            w.d.reset(new dictWriterжWriter(dw));
            w.d.fillWindow(w.dict);
        } else {
            // w was created with NewWriter
            w.d.reset(dst);
        }
    }
}

} // end flate_package
