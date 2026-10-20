// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// go2cs generated this placeholder — func hexdumpWords is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// hexdumper is a Swiss-army knife hex dumper.
//
// To use, optionally set addr and wordBytes, then call write repeatedly,
// followed by close.
partial struct hexdumper {
    // addr is the address to print for the first byte of data.
    internal uintptr addr;
    // addrBytes is the number of bytes of addr to print. If this is 0, it
    // defaults to goarch.PtrSize.
    internal uint8 addrBytes;
    // wordBytes is the number of bytes in a word. If wordBytes is 1, this
    // prints a byte-oriented dump. If it's > 1, this interprets the data as a
    // sequence of words of the given size. If it's 0, it's treated as
    // goarch.PtrSize.
    internal uint8 wordBytes;
    // mark is an optional function that can annotate values in the hex dump.
    //
    // If non-nil, it is called with the address of every complete, aligned word
    // in the hex dump.
    //
    // If it decides to print an annotation, it must first call m.start(), then
    // print the annotation, followed by a new line.
    internal Action<uintptr, hexdumpMarker> mark;
// Below here is state
    internal int8 ready; // 0=need to init state; 1=need to print header; 2=ready
    // dataBuf accumulates a line at a time of data, in case it's split across
    // buffers.
    internal array<byte> dataBuf = new(16);
    internal uint8 dataPos;
    internal uint8 dataSkip; // Skip first n bytes of buf on first line
    // toPos maps from byte offset in data to a visual offset in the printed line.
    internal array<byte> toPos = new(16);
}

partial struct hexdumpMarker {
    internal nint chars;
}

internal static void write(this ref hexdumper h, slice<byte> data) {
    if (h.ready == 0) {
        h.init();
    }

    // Handle leading data
    if (h.dataPos > 0) {
        nint n = copy(h.dataBuf.slice(h.dataPos), data);
        h.dataPos += (uint8)n;
        data = data.slice(n);
        if (h.dataPos < (uint8)len(h.dataBuf)) {
            return;
        }
        h.flushLine(h.dataBuf[..]);
        h.dataPos = 0;
    }

    // Handle full lines in data
    while (len(data) >= len(h.dataBuf)) {
        h.flushLine(data[..(int)(len(h.dataBuf))]);
        data = data[(int)(len(h.dataBuf))..];
    }

    // Handle trailing data
    h.dataPos = (uint8)copy(h.dataBuf[..], data);
}

internal static void close(this ref hexdumper h) {
    if (h.dataPos > 0) {
        h.flushLine(h.dataBuf.slice(0, h.dataPos));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidAddrBytesˢ = "invalid addrBytes"u8;
internal static readonly @string invalidWordBytesˢ = "invalid wordBytes"u8;

internal static void init(this ref hexdumper h) {
    const nint bytesPerLine = /* len(h.dataBuf) */ 16;

    if (h.addrBytes == 0){
        h.addrBytes = goarch.PtrSize;
    } else 
    if (h.addrBytes < 0 || h.addrBytes > goarch.PtrSize) {
        @throw(invalidAddrBytesˢ);
    }

    if (h.wordBytes == 0) {
        h.wordBytes = goarch.PtrSize;
    }
    nint wb = (nint)h.wordBytes;
    if (wb < 0 || wb >= bytesPerLine || (wb & (wb - 1)) != 0) {
        @throw(invalidWordBytesˢ);
    }

    // Construct position mapping.
    foreach (var (i, _) in h.toPos) {
        // First, calculate the "field" within the line, applying byte swizzling.
        nint field = 0;
        if (goarch.BigEndian){
            field = i;
        } else {
            field = i ^ (nint)(wb - 1);
        }
        // Translate this field into a visual offset.
        // "00112233 44556677  8899AABB CCDDEEFF"
        h.toPos[i] = (byte)(field * 2 + field / 4 + field / 8);
    }

    // The first line may need to skip some fields to get to alignment.
    // Round down the starting address.
    var nAddr = (uintptr)(h.addr & ~(uintptr)(uintptr)(bytesPerLine - 1));
    // Skip bytes to get to alignment.
    h.dataPos = (uint8)(h.addr - nAddr);
    h.dataSkip = (uint8)(h.addr - nAddr);
    h.addr = nAddr;

    // We're ready to print the header.
    h.ready = 1;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string addrSepᶜ = ": "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string asciiSepᶜ = "  "u8;

internal static void flushLine(this ref hexdumper h, slice<byte> data) {
    const nint bytesPerLine = /* len(h.dataBuf) */ 16;

    UntypedInt maxAddrChars = /* 2 * goarch.PtrSize */ 16;
    @string addrSep = addrSepᶜ;
    nint dataStart = (nint)((uint8)(2 * h.addrBytes)) + len(addrSep);
    // dataChars uses the same formula to toPos above. We calculate it with the
    // "last field", then add the size of the last field.
    const nint dataChars = /* (bytesPerLine-1)*2 + (bytesPerLine-1)/4 + (bytesPerLine-1)/8 + 2 */ 36;
    @string asciiSep = asciiSepᶜ;
    nint asciiStart = dataStart + dataChars + len(asciiSep);
    const nint asciiChars = /* bytesPerLine */ 16;
    nint nlPos = asciiStart + asciiChars;

    ref var lineBuf = ref heap(new array<byte>(73), out var ᏑlineBuf);
    void clear() {
        foreach (var (i, _) in ᏑlineBuf.Value) {
            ᏑlineBuf.Value[i] = (rune)' ';
        }
    }
    clear();

    if (h.ready == 1) {
        // Print column offsets header.
        foreach (var (offset, pos) in h.toPos.ΔRangeSnapshot()) {
            h.fmtHex(lineBuf.slice(dataStart + (nint)((byte)(pos + 1)))[..1], (uint64)offset);
        }
        // Print ASCII offsets.
        foreach (var offset in range(asciiChars)) {
            h.fmtHex(lineBuf.slice(asciiStart + offset)[..1], (uint64)offset);
        }
        lineBuf[nlPos] = (rune)'\n';
        gwrite(lineBuf.slice(0, nlPos + 1));
        clear();
        h.ready = 2;
    }

    // Format address.
    h.fmtHex(lineBuf.slice(0, (uint8)(2 * h.addrBytes)), (uint64)h.addr);
    copy(lineBuf.slice((uint8)(2 * h.addrBytes)), addrSep);
    // Format data in hex and ASCII.
    foreach (var (offset, b) in data) {
        if (offset < (nint)h.dataSkip) {
            continue;
        }

        var pos = h.toPos[offset];
        h.fmtHex(lineBuf.slice(dataStart + (nint)pos)[..2], (uint64)b);

        copy(lineBuf.slice(dataStart + dataChars), asciiSep);
        var ascii = (uint8)(rune)'.';
        if (b >= (rune)' ' && b <= (rune)'~') {
            ascii = b;
        }
        lineBuf[asciiStart + offset] = ascii;
    }
    // Trim buffer.
    nint end = asciiStart + len(data);
    lineBuf[end] = (rune)'\n';
    var buf = lineBuf.slice(0, end + 1);

    // Print.
    gwrite(buf);

    // Print marks.
    if (h.mark != default!) {
        clear();
        for (nint offset = 0; offset + (nint)h.wordBytes <= len(data); offset += (nint)h.wordBytes) {
            if (offset < (nint)h.dataSkip) {
                continue;
            }
            var addr = h.addr + (uintptr)offset;
            // Find the position of the left edge of this word
            nint caret = dataStart + (nint)min(h.toPos[offset], h.toPos[offset + (nint)h.wordBytes - 1]);
            h.mark(addr, new hexdumpMarker(caret));
        }
    }

    h.addr += (uintptr)bytesPerLine;
    h.dataPos = 0;
    h.dataSkip = 0;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string digᶜ = "0123456789abcdef"u8;

// fmtHex formats v in base 16 into buf. It fills all of buf. If buf is too
// small to represent v, it the output will start with '*'.
internal static void fmtHex(this ref hexdumper h, slice<byte> buf, uint64 v) {
    @string dig = digᶜ;
    nint i = len(buf) - 1;
    for (; i >= 0; i--) {
        buf[i] = dig[v % 16];
        v /= 16;
    }
    if (v != 0) {
        // Indicate that we couldn't fit the whole number.
        buf[0] = (rune)'*';
    }
}

internal static void start(this hexdumpMarker m) {
    array<byte> spaces = new(64);
    foreach (var (i, _) in spaces) {
        spaces[i] = (rune)' ';
    }
    while (m.chars > len(spaces)) {
        gwrite(spaces[..]);
        m.chars -= len(spaces);
    }
    gwrite(spaces.slice(0, m.chars));
    print((@string)"^ "u8);
}

} // end runtime_package
