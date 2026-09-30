// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using strings = strings_package;

partial class tar_package {

[GoType("num:nint")] partial struct Format;

// Constants to identify various tar formats.
internal static Format _ᴛ1ʗ => /* (1 << iota) / 4 */ 0;     // Sequence of 0, 0, 1, 2, 4, 8, etc...

public static Format FormatUnknown => 0;

internal static Format formatV7 => 1;

public static Format FormatUSTAR => 2;

public static Format FormatPAX => 4;

public static Format FormatGNU => 8;

internal static Format formatSTAR => 16;

internal static Format formatMax => 32;

internal static bool has(this Format f, Format f2) {
    return (Format)(f & f2) != 0;
}

[GoRecv] internal static void mayBe(this ref Format f, Format f2) {
    f |= (Format)(f2);
}

[GoRecv] internal static void mayOnlyBe(this ref Format f, Format f2) {
    f &= (Format)(f2);
}

[GoRecv] internal static void mustNotBe(this ref Format f, Format f2) {
    f &= ~(Format)(f2);
}

internal static map<Format, @string> formatNames = new map<Format, @string>{
    [formatV7] = "V7"u8, [FormatUSTAR] = "USTAR"u8, [FormatPAX] = "PAX"u8, [FormatGNU] = "GNU"u8, [formatSTAR] = "STAR"u8
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownˢ = "<unknown>"u8;

public static @string String(this Format f) {
    slice<@string> ss = default!;
    for (Format f2 = ((Format)1); f2 < formatMax; f2 <<= (int)(1)) {
        if (f.has(f2)) {
            ss = append(ss, formatNames[f2]);
        }
    }
    switch (len(ss)) {
    case 0: {
        return unknownˢ;
    }
    case 1: {
        return ss[0];
    }
    default: {
        return "("u8 + strings.Join(ss, " | "u8) + ")"u8;
    }}

}

// Magics used to identify various formats.
internal static readonly @string magicGNU = "ustar "u8;
internal static readonly @string versionGNU = " \x00"u8;

internal static readonly @string magicUSTAR = "ustar\x00"u8;
internal static readonly @string versionUSTAR = "00"u8;

internal static readonly @string trailerSTAR = "tar\x00"u8;

// Size constants from various tar specifications.
internal static UntypedInt blockSize => 512; // Size of each block in a tar stream

internal static UntypedInt nameSize => 100; // Max length of the name field in USTAR format

internal static UntypedInt prefixSize => 155; // Max length of the prefix field in USTAR format

internal static UntypedInt maxSpecialFileSize => /* 1 << 20 */ 1048576;

// blockPadding computes the number of bytes needed to pad offset up to the
// nearest block edge where 0 <= n < blockSize.
internal static int64 /*n*/ blockPadding(int64 offset) {
    return (int64)(-offset & (int64)((blockSize - 1)));
}

internal static block zeroBlock;

[GoType("[512]byte")] /* [blockSize]byte */
partial struct block;

// Convert block to any number of formats.
internal static ж<headerV7> toV7(this ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerV7)(b.Value));
}

internal static ж<headerGNU> toGNU(this ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerGNU)(b.Value));
}

internal static ж<headerSTAR> toSTAR(this ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerSTAR)(b.Value));
}

internal static ж<headerUSTAR> toUSTAR(this ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerUSTAR)(b.Value));
}

[GoRecv] internal static sparseArray toSparse(this ref block b) {
    return ((sparseArray)(b.Value[..]));
}

// getFormat checks that the block is a valid tar header based on the checksum.
// It then attempts to guess the specific format based on magic values.
// If the checksum fails, then FormatUnknown is returned.
internal static Format getFormat(this ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Verify checksum.
    parser p = default!;
    var value = p.parseOctal(Ꮡb.toV7().chksum());
    var (chksum1, chksum2) = b.computeChecksum();
    if (p.err != default! || (value != chksum1 && value != chksum2)) {
        return FormatUnknown;
    }
    // Guess the magic values.
    @string magic = ((@string)Ꮡb.toUSTAR().magic());
    @string version = ((@string)Ꮡb.toUSTAR().version());
    @string trailer = ((@string)Ꮡb.toSTAR().trailer());
    switch (ᐧ) {
    case {} when magic == magicUSTAR && trailer == trailerSTAR: {
        return formatSTAR;
    }
    case {} when magic == magicUSTAR: {
        return (Format)(FormatUSTAR | FormatPAX);
    }
    case {} when magic == magicGNU && version == versionGNU: {
        return FormatGNU;
    }
    default: {
        return formatV7;
    }}

}

// setFormat writes the magic values necessary for specified format
// and then updates the checksum accordingly.
internal static void setFormat(this ж<block> Ꮡb, Format format) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Set the magic values.
    switch (ᐧ) {
    case {} when format.has(formatV7): {
        break;
    }
    case {} when format.has(FormatGNU): {
        copy(Ꮡb.toGNU().magic(), // Do nothing.
 magicGNU);
        copy(Ꮡb.toGNU().version(), versionGNU);
        break;
    }
    case {} when format.has(formatSTAR): {
        copy(Ꮡb.toSTAR().magic(), magicUSTAR);
        copy(Ꮡb.toSTAR().version(), versionUSTAR);
        copy(Ꮡb.toSTAR().trailer(), trailerSTAR);
        break;
    }
    case {} when format.has((Format)(FormatUSTAR | FormatPAX)): {
        copy(Ꮡb.toUSTAR().magic(), magicUSTAR);
        copy(Ꮡb.toUSTAR().version(), versionUSTAR);
        break;
    }
    default: {
        throw panic("invalid format");
        break;
    }}

    // Update checksum.
    // This field is special in that it is terminated by a NULL then space.
    formatter f = default!;
    var field = Ꮡb.toV7().chksum();
    var (chksum, _) = b.computeChecksum(); // Possible values are 256..128776
    f.formatOctal(field[..7], chksum); // Never fails since 128776 < 262143
    field[7] = (rune)' ';
}

// computeChecksum computes the checksum for the header block.
// POSIX specifies a sum of the unsigned byte values, but the Sun tar used
// signed byte values.
// We compute and return both.
[GoRecv] internal static (int64 unsigned, int64 signed) computeChecksum(this ref block b) {
    int64 unsigned = default!;
    int64 signed = default!;

    foreach (var (i, vᴛ1) in b.Value) {
        var c = vᴛ1;

        if (148 <= i && i < 156) {
            c = (rune)' '; // Treat the checksum field itself as all spaces.
        }
        unsigned += (int64)c;
        signed += (int64)(int8)c;
    }
    return (unsigned, signed);
}

// reset clears the block with all zeros.
[GoRecv] internal static void reset(this ref block b) {
    b = new block(new byte[512].array());
}

[GoType("[512]byte")] /* [blockSize]byte */
partial struct headerV7;

[GoRecv] internal static slice<byte> name(this ref headerV7 h) {
    return h.Value[0..][..100];
}

[GoRecv] internal static slice<byte> mode(this ref headerV7 h) {
    return h.Value[100..][..8];
}

[GoRecv] internal static slice<byte> uid(this ref headerV7 h) {
    return h.Value[108..][..8];
}

[GoRecv] internal static slice<byte> gid(this ref headerV7 h) {
    return h.Value[116..][..8];
}

[GoRecv] internal static slice<byte> size(this ref headerV7 h) {
    return h.Value[124..][..12];
}

[GoRecv] internal static slice<byte> modTime(this ref headerV7 h) {
    return h.Value[136..][..12];
}

[GoRecv] internal static slice<byte> chksum(this ref headerV7 h) {
    return h.Value[148..][..8];
}

[GoRecv] internal static slice<byte> typeFlag(this ref headerV7 h) {
    return h.Value[156..][..1];
}

[GoRecv] internal static slice<byte> linkName(this ref headerV7 h) {
    return h.Value[157..][..100];
}

[GoType("[512]byte")] /* [blockSize]byte */
partial struct headerGNU;

internal static ж<headerV7> v7(this ж<headerGNU> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return Ꮡ((headerV7)(h.Value));
}

[GoRecv] internal static slice<byte> magic(this ref headerGNU h) {
    return h.Value[257..][..6];
}

[GoRecv] internal static slice<byte> version(this ref headerGNU h) {
    return h.Value[263..][..2];
}

[GoRecv] internal static slice<byte> userName(this ref headerGNU h) {
    return h.Value[265..][..32];
}

[GoRecv] internal static slice<byte> groupName(this ref headerGNU h) {
    return h.Value[297..][..32];
}

[GoRecv] internal static slice<byte> devMajor(this ref headerGNU h) {
    return h.Value[329..][..8];
}

[GoRecv] internal static slice<byte> devMinor(this ref headerGNU h) {
    return h.Value[337..][..8];
}

[GoRecv] internal static slice<byte> accessTime(this ref headerGNU h) {
    return h.Value[345..][..12];
}

[GoRecv] internal static slice<byte> changeTime(this ref headerGNU h) {
    return h.Value[357..][..12];
}

[GoRecv] internal static sparseArray sparse(this ref headerGNU h) {
    return ((sparseArray)(h.Value[386..][..(int)(24 * 4 + 1)]));
}

[GoRecv] internal static slice<byte> realSize(this ref headerGNU h) {
    return h.Value[483..][..12];
}

[GoType("[512]byte")] /* [blockSize]byte */
partial struct headerSTAR;

internal static ж<headerV7> v7(this ж<headerSTAR> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return Ꮡ((headerV7)(h.Value));
}

[GoRecv] internal static slice<byte> magic(this ref headerSTAR h) {
    return h.Value[257..][..6];
}

[GoRecv] internal static slice<byte> version(this ref headerSTAR h) {
    return h.Value[263..][..2];
}

[GoRecv] internal static slice<byte> userName(this ref headerSTAR h) {
    return h.Value[265..][..32];
}

[GoRecv] internal static slice<byte> groupName(this ref headerSTAR h) {
    return h.Value[297..][..32];
}

[GoRecv] internal static slice<byte> devMajor(this ref headerSTAR h) {
    return h.Value[329..][..8];
}

[GoRecv] internal static slice<byte> devMinor(this ref headerSTAR h) {
    return h.Value[337..][..8];
}

[GoRecv] internal static slice<byte> prefix(this ref headerSTAR h) {
    return h.Value[345..][..131];
}

[GoRecv] internal static slice<byte> accessTime(this ref headerSTAR h) {
    return h.Value[476..][..12];
}

[GoRecv] internal static slice<byte> changeTime(this ref headerSTAR h) {
    return h.Value[488..][..12];
}

[GoRecv] internal static slice<byte> trailer(this ref headerSTAR h) {
    return h.Value[508..][..4];
}

[GoType("[512]byte")] /* [blockSize]byte */
partial struct headerUSTAR;

internal static ж<headerV7> v7(this ж<headerUSTAR> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return Ꮡ((headerV7)(h.Value));
}

[GoRecv] internal static slice<byte> magic(this ref headerUSTAR h) {
    return h.Value[257..][..6];
}

[GoRecv] internal static slice<byte> version(this ref headerUSTAR h) {
    return h.Value[263..][..2];
}

[GoRecv] internal static slice<byte> userName(this ref headerUSTAR h) {
    return h.Value[265..][..32];
}

[GoRecv] internal static slice<byte> groupName(this ref headerUSTAR h) {
    return h.Value[297..][..32];
}

[GoRecv] internal static slice<byte> devMajor(this ref headerUSTAR h) {
    return h.Value[329..][..8];
}

[GoRecv] internal static slice<byte> devMinor(this ref headerUSTAR h) {
    return h.Value[337..][..8];
}

[GoRecv] internal static slice<byte> prefix(this ref headerUSTAR h) {
    return h.Value[345..][..155];
}

[GoType("[]byte")] partial struct sparseArray;

internal static sparseElem entry(this sparseArray s, nint i) {
    return ((sparseElem)(slice<byte>)(s.slice(i * 24)));
}

internal static slice<byte> isExtended(this sparseArray s) {
    return s.slice(24 * s.maxEntries())[..1];
}

internal static nint maxEntries(this sparseArray s) {
    return len(s) / 24;
}

[GoType("[]byte")] partial struct sparseElem;

internal static slice<byte> offset(this sparseElem s) {
    return s[0..][..12];
}

internal static slice<byte> length(this sparseElem s) {
    return s[12..][..12];
}

} // end tar_package
