// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using strings = strings_package;
using static tar_package;

partial class tar_package {

// Format represents the tar archive format.
//
// The original tar format was introduced in Unix V7.
// Since then, there have been multiple competing formats attempting to
// standardize or extend the V7 format to overcome its limitations.
// The most common formats are the USTAR, PAX, and GNU formats,
// each with their own advantages and limitations.
//
// The following table captures the capabilities of each format:
//
//	                  |  USTAR |       PAX |       GNU
//	------------------+--------+-----------+----------
//	Name              |   256B | unlimited | unlimited
//	Linkname          |   100B | unlimited | unlimited
//	Size              | uint33 | unlimited |    uint89
//	Mode              | uint21 |    uint21 |    uint57
//	Uid/Gid           | uint21 | unlimited |    uint57
//	Uname/Gname       |    32B | unlimited |       32B
//	ModTime           | uint33 | unlimited |     int89
//	AccessTime        |    n/a | unlimited |     int89
//	ChangeTime        |    n/a | unlimited |     int89
//	Devmajor/Devminor | uint21 |    uint21 |    uint57
//	------------------+--------+-----------+----------
//	string encoding   |  ASCII |     UTF-8 |    binary
//	sub-second times  |     no |       yes |        no
//	sparse files      |     no |       yes |       yes
//
// The table's upper portion shows the [Header] fields, where each format reports
// the maximum number of bytes allowed for each string field and
// the integer type used to store each numeric field
// (where timestamps are stored as the number of seconds since the Unix epoch).
//
// The table's lower portion shows specialized features of each format,
// such as supported string encodings, support for sub-second timestamps,
// or support for sparse files.
//
// The Writer currently provides no support for sparse files.
partial struct Format /*num:nint*/;

// Constants to identify various tar formats.
// Deliberately hide the meaning of constants from public API.
internal static Format _ᴛ1ʗ => /* (1 << iota) / 4 */ 0; // Sequence of 0, 0, 1, 2, 4, 8, etc...

// FormatUnknown indicates that the format is unknown.
public static Format FormatUnknown => 0;

// The format of the original Unix V7 tar tool prior to standardization.
internal static Format formatV7 => 1;

// FormatUSTAR represents the USTAR header format defined in POSIX.1-1988.
//
// While this format is compatible with most tar readers,
// the format has several limitations making it unsuitable for some usages.
// Most notably, it cannot support sparse files, files larger than 8GiB,
// filenames larger than 256 characters, and non-ASCII filenames.
//
// Reference:
//	http://pubs.opengroup.org/onlinepubs/9699919799/utilities/pax.html#tag_20_92_13_06
public static Format FormatUSTAR => 2;

// FormatPAX represents the PAX header format defined in POSIX.1-2001.
//
// PAX extends USTAR by writing a special file with Typeflag TypeXHeader
// preceding the original header. This file contains a set of key-value
// records, which are used to overcome USTAR's shortcomings, in addition to
// providing the ability to have sub-second resolution for timestamps.
//
// Some newer formats add their own extensions to PAX by defining their
// own keys and assigning certain semantic meaning to the associated values.
// For example, sparse file support in PAX is implemented using keys
// defined by the GNU manual (e.g., "GNU.sparse.map").
//
// Reference:
//	http://pubs.opengroup.org/onlinepubs/009695399/utilities/pax.html
public static Format FormatPAX => 4;

// FormatGNU represents the GNU header format.
//
// The GNU header format is older than the USTAR and PAX standards and
// is not compatible with them. The GNU format supports
// arbitrary file sizes, filenames of arbitrary encoding and length,
// sparse files, and other features.
//
// It is recommended that PAX be chosen over GNU unless the target
// application can only parse GNU formatted archives.
//
// Reference:
//	https://www.gnu.org/software/tar/manual/html_node/Standard.html
public static Format FormatGNU => 8;

// Schily's tar format, which is incompatible with USTAR.
// This does not cover STAR extensions to the PAX format; these fall under
// the PAX format.
internal static Format formatSTAR => 16;

internal static Format formatMax => 32;

} partial class tar_package {

[GoMethodBody("has")] internal static bool hasᴍ(Format f, Format f2) { return (Format)(f & f2) != 0; }
[GoMethodBody("mayBe")] internal static void mayBeᴍ(ref Format f, Format f2) { f |= (Format)(f2); }
[GoMethodBody("mayOnlyBe")] internal static void mayOnlyBeᴍ(ref Format f, Format f2) { f &= (Format)(f2); }
[GoMethodBody("mustNotBe")] internal static void mustNotBeᴍ(ref Format f, Format f2) { f &= ~(Format)(f2); }

} partial class tar_package {

internal static map<Format, @string> formatNames = new map<Format, @string>{
    [formatV7] = "V7", [FormatUSTAR] = "USTAR", [FormatPAX] = "PAX", [FormatGNU] = "GNU", [formatSTAR] = "STAR"
};

} partial class tar_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownˢᴍFormat => ref unknownˢᵇᴍFormat;
private static readonly @string unknownˢᵇᴍFormat = "<unknown>"u8;

[GoMethodBody("String")] internal static @string Stringᴍ(Format f) {
    var ssᴷ = new stackAppend<@string>();

    slice<@string> ss = default;
    for (Format f2 = (Format)1; f2 < formatMax; f2 <<= 1) {
        if (f.has(f2)) {
            ss = append(ssᴷ, ss, formatNames[f2]);
        }
    }
    switch (len(ss)) {
    case 0: {
        return unknownˢᴍFormat;
    }
    case 1: {
        return ss[0];
    }
    default: {
        return "("u8 + strings.Join(ss, " | ") + ")"u8;
    }}
}

} partial class tar_package {

// Magics used to identify various formats.
internal static ref readonly @string magicGNU => ref magicGNUᵇ;
private static readonly @string magicGNUᵇ = "ustar "u8;
internal static ref readonly @string versionGNU => ref versionGNUᵇ;
private static readonly @string versionGNUᵇ = " \x00"u8;
internal static ref readonly @string magicUSTAR => ref magicUSTARᵇ;
private static readonly @string magicUSTARᵇ = "ustar\x00"u8;
internal static ref readonly @string versionUSTAR => ref versionUSTARᵇ;
private static readonly @string versionUSTARᵇ = "00"u8;
internal static ref readonly @string trailerSTAR => ref trailerSTARᵇ;
private static readonly @string trailerSTARᵇ = "tar\x00"u8;

// Size constants from various tar specifications.
internal static UntypedInt blockSize => 512;  // Size of each block in a tar stream
internal static UntypedInt nameSize => 100;   // Max length of the name field in USTAR format
internal static UntypedInt prefixSize => 155; // Max length of the prefix field in USTAR format

// Max length of a special file (PAX header, GNU long name or link).
// This matches the limit used by libarchive.
internal static UntypedInt maxSpecialFileSize => /* 1 << 20 */ 1048576;

// Maximum number of sparse file entries.
// We should never actually hit this limit
// (every sparse encoding will first be limited by maxSpecialFileSize),
// but this adds an additional layer of defense.
internal static UntypedInt maxSparseFileEntries => /* 1 << 20 */ 1048576;

// blockPadding computes the number of bytes needed to pad offset up to the
// nearest block edge where 0 <= n < blockSize.
internal static int64 /*n*/ blockPadding(int64 offset) {
    return -offset & (int64)(blockSize - 1);
}

internal static block zeroBlock;

/* [blockSize]byte */
partial struct block /*[512]byte*/;

} partial class tar_package {

// Convert block to any number of formats.
[GoMethodBody("toV7")] internal static ж<headerV7> toV7ᴍ(ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerV7)(b.Value));
}

[GoMethodBody("toGNU")] internal static ж<headerGNU> toGNUᴍ(ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerGNU)(b.Value));
}

[GoMethodBody("toSTAR")] internal static ж<headerSTAR> toSTARᴍ(ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerSTAR)(b.Value));
}

[GoMethodBody("toUSTAR")] internal static ж<headerUSTAR> toUSTARᴍ(ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return Ꮡ((headerUSTAR)(b.Value));
}

[GoMethodBody("toSparse")] internal static sparseArray toSparseᴍ(ref block b) { return (sparseArray)(b.Value[..]); }

// getFormat checks that the block is a valid tar header based on the checksum.
// It then attempts to guess the specific format based on magic values.
// If the checksum fails, then FormatUnknown is returned.
[GoMethodBody("getFormat")] internal static Format getFormatᴍ(ж<block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Verify checksum.
    parser p = default;
    var value = p.parseOctal(Ꮡb.toV7().chksum());
    var (chksum1, chksum2) = b.computeChecksum();
    if (p.err != nil || (value != chksum1 && value != chksum2)) {
        return FormatUnknown;
    }

    // Guess the magic values.
    @string magic = (@string)Ꮡb.toUSTAR().magic();
    @string version = (@string)Ꮡb.toUSTAR().version();
    @string trailer = (@string)Ꮡb.toSTAR().trailer();
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
[GoMethodBody("setFormat")] internal static void setFormatᴍ(ж<block> Ꮡb, Format format) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Set the magic values.
    switch (ᐧ) {
    case {} when format.has(formatV7): {
        // Do nothing.
        break;
    }
    case {} when format.has(FormatGNU): {
        copy(Ꮡb.toGNU().magic(), magicGNU);
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
    formatter f = default;
    var field = Ꮡb.toV7().chksum();
    var (chksum, _) = b.computeChecksum(); // Possible values are 256..128776
    f.formatOctal(field[..7], chksum);     // Never fails since 128776 < 262143
    field[7] = (byte)' ';
}

// computeChecksum computes the checksum for the header block.
// POSIX specifies a sum of the unsigned byte values, but the Sun tar used
// signed byte values.
// We compute and return both.
[GoMethodBody("computeChecksum")] internal static (int64 unsigned, int64 signed) computeChecksumᴍ(ref block b) {
    int64 unsigned = default;
    int64 signed = default;

    foreach (var (i, vᴛ1) in range(b.Value)) {
        var c = vᴛ1;

        if (148 <= i && i < 156) {
            c = (byte)' '; // Treat the checksum field itself as all spaces.
        }
        unsigned += (int64)c;
        signed += (int64)(int8)c;
    }
    return (unsigned, signed);
}

// reset clears the block with all zeros.
[GoMethodBody("reset")] internal static void resetᴍ(ref block b) {
    b.Assign(new block(new byte[512].array()));
}

} partial class tar_package {

/* [blockSize]byte */
partial struct headerV7 /*[512]byte*/;

} partial class tar_package {

[GoMethodBody("name")] internal static slice<byte> nameᴍ(ref headerV7 h) { return h.Value[0..][..100]; }
[GoMethodBody("mode")] internal static slice<byte> modeᴍ(ref headerV7 h) { return h.Value[100..][..8]; }
[GoMethodBody("uid")] internal static slice<byte> uidᴍ(ref headerV7 h) { return h.Value[108..][..8]; }
[GoMethodBody("gid")] internal static slice<byte> gidᴍ(ref headerV7 h) { return h.Value[116..][..8]; }
[GoMethodBody("size")] internal static slice<byte> sizeᴍ(ref headerV7 h) { return h.Value[124..][..12]; }
[GoMethodBody("modTime")] internal static slice<byte> modTimeᴍ(ref headerV7 h) { return h.Value[136..][..12]; }
[GoMethodBody("chksum")] internal static slice<byte> chksumᴍ(ref headerV7 h) { return h.Value[148..][..8]; }
[GoMethodBody("typeFlag")] internal static slice<byte> typeFlagᴍ(ref headerV7 h) { return h.Value[156..][..1]; }
[GoMethodBody("linkName")] internal static slice<byte> linkNameᴍ(ref headerV7 h) { return h.Value[157..][..100]; }

} partial class tar_package {

/* [blockSize]byte */
partial struct headerGNU /*[512]byte*/;

} partial class tar_package {

[GoMethodBody("v7")] internal static ж<headerV7> v7ᴍ(ж<headerGNU> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return Ꮡ((headerV7)(h.Value));
}

[GoMethodBody("magic")] internal static slice<byte> magicᴍ(ref headerGNU h) { return h.Value[257..][..6]; }
[GoMethodBody("version")] internal static slice<byte> versionᴍ(ref headerGNU h) { return h.Value[263..][..2]; }
[GoMethodBody("userName")] internal static slice<byte> userNameᴍ(ref headerGNU h) { return h.Value[265..][..32]; }
[GoMethodBody("groupName")] internal static slice<byte> groupNameᴍ(ref headerGNU h) { return h.Value[297..][..32]; }
[GoMethodBody("devMajor")] internal static slice<byte> devMajorᴍ(ref headerGNU h) { return h.Value[329..][..8]; }
[GoMethodBody("devMinor")] internal static slice<byte> devMinorᴍ(ref headerGNU h) { return h.Value[337..][..8]; }
[GoMethodBody("accessTime")] internal static slice<byte> accessTimeᴍ(ref headerGNU h) { return h.Value[345..][..12]; }
[GoMethodBody("changeTime")] internal static slice<byte> changeTimeᴍ(ref headerGNU h) { return h.Value[357..][..12]; }
[GoMethodBody("sparse")] internal static sparseArray sparseᴍ(ref headerGNU h) { return (sparseArray)(h.Value[386..][..(int)(24 * 4 + 1)]); }
[GoMethodBody("realSize")] internal static slice<byte> realSizeᴍ(ref headerGNU h) { return h.Value[483..][..12]; }

} partial class tar_package {

/* [blockSize]byte */
partial struct headerSTAR /*[512]byte*/;

} partial class tar_package {

[GoMethodBody("v7")] internal static ж<headerV7> v7ᴍ(ж<headerSTAR> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return Ꮡ((headerV7)(h.Value));
}

[GoMethodBody("magic")] internal static slice<byte> magicᴍ(ref headerSTAR h) { return h.Value[257..][..6]; }
[GoMethodBody("version")] internal static slice<byte> versionᴍ(ref headerSTAR h) { return h.Value[263..][..2]; }
[GoMethodBody("userName")] internal static slice<byte> userNameᴍ(ref headerSTAR h) { return h.Value[265..][..32]; }
[GoMethodBody("groupName")] internal static slice<byte> groupNameᴍ(ref headerSTAR h) { return h.Value[297..][..32]; }
[GoMethodBody("devMajor")] internal static slice<byte> devMajorᴍ(ref headerSTAR h) { return h.Value[329..][..8]; }
[GoMethodBody("devMinor")] internal static slice<byte> devMinorᴍ(ref headerSTAR h) { return h.Value[337..][..8]; }
[GoMethodBody("prefix")] internal static slice<byte> prefixᴍ(ref headerSTAR h) { return h.Value[345..][..131]; }
[GoMethodBody("accessTime")] internal static slice<byte> accessTimeᴍ(ref headerSTAR h) { return h.Value[476..][..12]; }
[GoMethodBody("changeTime")] internal static slice<byte> changeTimeᴍ(ref headerSTAR h) { return h.Value[488..][..12]; }
[GoMethodBody("trailer")] internal static slice<byte> trailerᴍ(ref headerSTAR h) { return h.Value[508..][..4]; }

} partial class tar_package {

/* [blockSize]byte */
partial struct headerUSTAR /*[512]byte*/;

} partial class tar_package {

[GoMethodBody("v7")] internal static ж<headerV7> v7ᴍ(ж<headerUSTAR> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return Ꮡ((headerV7)(h.Value));
}

[GoMethodBody("magic")] internal static slice<byte> magicᴍ(ref headerUSTAR h) { return h.Value[257..][..6]; }
[GoMethodBody("version")] internal static slice<byte> versionᴍ(ref headerUSTAR h) { return h.Value[263..][..2]; }
[GoMethodBody("userName")] internal static slice<byte> userNameᴍ(ref headerUSTAR h) { return h.Value[265..][..32]; }
[GoMethodBody("groupName")] internal static slice<byte> groupNameᴍ(ref headerUSTAR h) { return h.Value[297..][..32]; }
[GoMethodBody("devMajor")] internal static slice<byte> devMajorᴍ(ref headerUSTAR h) { return h.Value[329..][..8]; }
[GoMethodBody("devMinor")] internal static slice<byte> devMinorᴍ(ref headerUSTAR h) { return h.Value[337..][..8]; }
[GoMethodBody("prefix")] internal static slice<byte> prefixᴍ(ref headerUSTAR h) { return h.Value[345..][..155]; }

} partial class tar_package {

partial struct sparseArray /*[]byte*/;

} partial class tar_package {

[GoMethodBody("entry")] internal static sparseElem entryᴍ(sparseArray s, nint i) { return (sparseElem)(slice<byte>)(s.slice(i * 24)); }
[GoMethodBody("isExtended")] internal static slice<byte> isExtendedᴍ(sparseArray s) { return s.slice(24 * s.maxEntries())[..1]; }
[GoMethodBody("maxEntries")] internal static nint maxEntriesᴍ(sparseArray s) { return len(s) / 24; }

} partial class tar_package {

partial struct sparseElem /*[]byte*/;

} partial class tar_package {

[GoMethodBody("offset")] internal static slice<byte> offsetᴍ(sparseElem s) { return s[0..][..12]; }
[GoMethodBody("length")] internal static slice<byte> lengthᴍ(sparseElem s) { return s[12..][..12]; }

}
