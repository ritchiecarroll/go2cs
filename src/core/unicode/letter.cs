// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unicode provides data and functions to test some properties of
// Unicode code points.
namespace go;

partial class unicode_package {

public static UntypedInt MaxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.
public static UntypedInt ReplacementChar => /* '\uFFFD' */ 65533; // Represents invalid code points.
public static UntypedInt MaxASCII => /* '\u007F' */ 127; // maximum ASCII value.
public static UntypedInt MaxLatin1 => /* '\u00FF' */ 255; // maximum Latin-1 value.

// RangeTable defines a set of Unicode code points by listing the ranges of
// code points within the set. The ranges are listed in two slices
// to save space: a slice of 16-bit ranges and a slice of 32-bit ranges.
// The two slices must be in sorted order and non-overlapping.
// Also, R32 should contain only values >= 0x10000 (1<<16).
[GoType] partial struct RangeTable {
    public slice<Range16> R16;
    public slice<Range32> R32;
    public nint LatinOffset; // number of entries in R16 with Hi <= MaxLatin1
}

// Range16 represents of a range of 16-bit Unicode code points. The range runs from Lo to Hi
// inclusive and has the specified stride.
[GoType] partial struct Range16 {
    public uint16 Lo;
    public uint16 Hi;
    public uint16 Stride;
}

// Range32 represents of a range of Unicode code points and is used when one or
// more of the values will not fit in 16 bits. The range runs from Lo to Hi
// inclusive and has the specified stride. Lo and Hi must always be >= 1<<16.
[GoType] partial struct Range32 {
    public uint32 Lo;
    public uint32 Hi;
    public uint32 Stride;
}

// CaseRange represents a range of Unicode code points for simple (one
// code point to one code point) case conversion.
// The range runs from Lo to Hi inclusive, with a fixed stride of 1. Deltas
// are the number to add to the code point to reach the code point for a
// different case for that character. They may be negative. If zero, it
// means the character is in the corresponding case. There is a special
// case representing sequences of alternating corresponding Upper and Lower
// pairs. It appears with a fixed Delta of
//
//	{UpperLower, UpperLower, UpperLower}
//
// The constant UpperLower has an otherwise impossible delta value.
[GoType] partial struct CaseRange {
    public uint32 Lo;
    public uint32 Hi;
    public d Delta;
}

[GoType("[]CaseRange")] partial struct SpecialCase;

// BUG(r): There is no mechanism for full case folding, that is, for
// characters that involve multiple runes in the input or output.

// Indices into the Delta arrays inside CaseRanges for case mapping.
public static UntypedInt UpperCase => iota;

public static UntypedInt LowerCase => 1;

public static UntypedInt TitleCase => 2;

public static UntypedInt MaxCase => 3;

[GoType("[3]rune")] /* [MaxCase]rune */
public partial struct d; // to make the CaseRanges text shorter

// If the Delta field of a [CaseRange] is UpperLower, it means
// this CaseRange represents a sequence of the form (say)
// [Upper] [Lower] [Upper] [Lower].
public static UntypedInt UpperLower => /* MaxRune + 1 */ 1114112; // (Cannot be a valid delta.)

// linearMax is the maximum size table for linear search for non-Latin1 rune.
// Derived by running 'go test -calibrate'.
internal static UntypedInt linearMax => 18;

// is16 reports whether r is in the sorted slice of 16-bit ranges.
internal static bool is16(slice<Range16> ranges, uint16 r) {
    if (len(ranges) <= linearMax || r <= MaxLatin1) {
        foreach (var (i, _) in ranges) {
            var range_ = Ꮡ(ranges, i);
            if (r < (~range_).Lo) {
                return false;
            }
            if (r <= (~range_).Hi) {
                return (~range_).Stride == 1 || (uint16)((uint16)(r - (~range_).Lo) % (~range_).Stride) == 0;
            }
        }
        return false;
    }
    // binary search over ranges
    nint lo = 0;
    nint hi = len(ranges);
    while (lo < hi) {
        nint m = (nint)(((nuint)(lo + hi) >> (int)(1)));
        var range_ = Ꮡ(ranges, m);
        if ((~range_).Lo <= r && r <= (~range_).Hi) {
            return (~range_).Stride == 1 || (uint16)((uint16)(r - (~range_).Lo) % (~range_).Stride) == 0;
        }
        if (r < (~range_).Lo){
            hi = m;
        } else {
            lo = m + 1;
        }
    }
    return false;
}

// is32 reports whether r is in the sorted slice of 32-bit ranges.
internal static bool is32(slice<Range32> ranges, uint32 r) {
    if (len(ranges) <= linearMax) {
        foreach (var (i, _) in ranges) {
            var range_ = Ꮡ(ranges, i);
            if (r < (~range_).Lo) {
                return false;
            }
            if (r <= (~range_).Hi) {
                return (~range_).Stride == 1 || (r - (~range_).Lo) % (~range_).Stride == 0;
            }
        }
        return false;
    }
    // binary search over ranges
    nint lo = 0;
    nint hi = len(ranges);
    while (lo < hi) {
        nint m = (nint)(((nuint)(lo + hi) >> (int)(1)));
        var range_ = ranges[m];
        if (range_.Lo <= r && r <= range_.Hi) {
            return range_.Stride == 1 || (r - range_.Lo) % range_.Stride == 0;
        }
        if (r < range_.Lo){
            hi = m;
        } else {
            lo = m + 1;
        }
    }
    return false;
}

// Is reports whether the rune is in the specified table of ranges.
public static bool Is(ж<RangeTable> ᏑrangeTab, rune r) {
    ref var rangeTab = ref ᏑrangeTab.DerefOrNull();

    var r16 = rangeTab.R16;
    // Compare as uint32 to correctly handle negative runes.
    if (len(r16) > 0 && (uint32)r <= (uint32)r16[len(r16) - 1].Hi) {
        return is16(r16, (uint16)r);
    }
    var r32 = rangeTab.R32;
    if (len(r32) > 0 && r >= (rune)r32[0].Lo) {
        return is32(r32, (uint32)r);
    }
    return false;
}

internal static bool isExcludingLatin(ref RangeTable rangeTab, rune r) {
    var r16 = rangeTab.R16;
    // Compare as uint32 to correctly handle negative runes.
    {
        nint off = rangeTab.LatinOffset; if (len(r16) > off && (uint32)r <= (uint32)r16[len(r16) - 1].Hi) {
            return is16(r16.slice(off), (uint16)r);
        }
    }
    var r32 = rangeTab.R32;
    if (len(r32) > 0 && r >= (rune)r32[0].Lo) {
        return is32(r32, (uint32)r);
    }
    return false;
}

// IsUpper reports whether the rune is an upper case letter.
public static bool IsUpper(rune r) {
    // See comment in IsGraphic.
    if ((uint32)r <= MaxLatin1) {
        return (uint8)(properties[(uint8)r] & (uint8)pLmask) == pLu;
    }
    return isExcludingLatin(ref (Upper).DerefOrNull(), r);
}

// IsLower reports whether the rune is a lower case letter.
public static bool IsLower(rune r) {
    // See comment in IsGraphic.
    if ((uint32)r <= MaxLatin1) {
        return (uint8)(properties[(uint8)r] & (uint8)pLmask) == pLl;
    }
    return isExcludingLatin(ref (Lower).DerefOrNull(), r);
}

// IsTitle reports whether the rune is a title case letter.
public static bool IsTitle(rune r) {
    if (r <= MaxLatin1) {
        return false;
    }
    return isExcludingLatin(ref (Title).DerefOrNull(), r);
}

// lookupCaseRange returns the CaseRange mapping for rune r or nil if no
// mapping exists for r.
internal static ж<CaseRange> lookupCaseRange(rune r, slice<CaseRange> caseRange) {
    // binary search over ranges
    nint lo = 0;
    nint hi = len(caseRange);
    while (lo < hi) {
        nint m = (nint)(((nuint)(lo + hi) >> (int)(1)));
        var cr = Ꮡ(caseRange, m);
        if ((rune)(~cr).Lo <= r && r <= (rune)(~cr).Hi) {
            return cr;
        }
        if (r < (rune)(~cr).Lo){
            hi = m;
        } else {
            lo = m + 1;
        }
    }
    return default!;
}

// convertCase converts r to _case using CaseRange cr.
internal static rune convertCase(nint _case, rune r, ref CaseRange cr) {
    var delta = cr.Delta[_case];
    if (delta > MaxRune) {
        // In an Upper-Lower sequence, which always starts with
        // an UpperCase letter, the real deltas always look like:
        //	{0, 1, 0}    UpperCase (Lower is next)
        //	{-1, 0, -1}  LowerCase (Upper, Title are previous)
        // The characters at even offsets from the beginning of the
        // sequence are upper case; the ones at odd offsets are lower.
        // The correct mapping can be done by clearing or setting the low
        // bit in the sequence offset.
        // The constants UpperCase and TitleCase are even while LowerCase
        // is odd so we take the low bit from _case.
        return (rune)cr.Lo + ((rune)((rune)((r - (rune)cr.Lo) & ~1) | (rune)((nint)(_case & 1))));
    }
    return r + delta;
}

// to maps the rune using the specified case mapping.
// It additionally reports whether caseRange contained a mapping for r.
internal static (rune mappedRune, bool foundMapping) to(nint _case, rune r, slice<CaseRange> caseRange) {
    if (_case < 0 || MaxCase <= _case) {
        return (ReplacementChar, false); // as reasonable an error as any
    }
    {
        var cr = lookupCaseRange(r, caseRange); if (cr != nil) {
            return (convertCase(_case, r, ref (cr).DerefOrNull()), true);
        }
    }
    return (r, false);
}

// To maps the rune to the specified case: [UpperCase], [LowerCase], or [TitleCase].
public static rune To(nint _case, rune r) {
    (r, _) = to(_case, r, CaseRanges);
    return r;
}

// ToUpper maps the rune to upper case.
public static rune ToUpper(rune r) {
    if (r <= MaxASCII) {
        if ((rune)'a' <= r && r <= (rune)'z') {
            r -= (rune)'a' - (rune)'A';
        }
        return r;
    }
    return To(UpperCase, r);
}

// ToLower maps the rune to lower case.
public static rune ToLower(rune r) {
    if (r <= MaxASCII) {
        if ((rune)'A' <= r && r <= (rune)'Z') {
            r += (rune)'a' - (rune)'A';
        }
        return r;
    }
    return To(LowerCase, r);
}

// ToTitle maps the rune to title case.
public static rune ToTitle(rune r) {
    if (r <= MaxASCII) {
        if ((rune)'a' <= r && r <= (rune)'z') {
            // title case is upper case for ASCII
            r -= (rune)'a' - (rune)'A';
        }
        return r;
    }
    return To(TitleCase, r);
}

// ToUpper maps the rune to upper case giving priority to the special mapping.
public static rune ToUpper(this SpecialCase special, rune r) {
    var (r1, hadMapping) = to(UpperCase, r, ((slice<CaseRange>)special));
    if (r1 == r && !hadMapping) {
        r1 = ToUpper(r);
    }
    return r1;
}

// ToTitle maps the rune to title case giving priority to the special mapping.
public static rune ToTitle(this SpecialCase special, rune r) {
    var (r1, hadMapping) = to(TitleCase, r, ((slice<CaseRange>)special));
    if (r1 == r && !hadMapping) {
        r1 = ToTitle(r);
    }
    return r1;
}

// ToLower maps the rune to lower case giving priority to the special mapping.
public static rune ToLower(this SpecialCase special, rune r) {
    var (r1, hadMapping) = to(LowerCase, r, ((slice<CaseRange>)special));
    if (r1 == r && !hadMapping) {
        r1 = ToLower(r);
    }
    return r1;
}

// caseOrbit is defined in tables.go as []foldPair. Right now all the
// entries fit in uint16, so use uint16. If that changes, compilation
// will fail (the constants in the composite literal will not fit in uint16)
// and the types here can change to uint32.
[GoType] partial struct foldPair {
    public uint16 From;
    public uint16 To;
}

// SimpleFold iterates over Unicode code points equivalent under
// the Unicode-defined simple case folding. Among the code points
// equivalent to rune (including rune itself), SimpleFold returns the
// smallest rune > r if one exists, or else the smallest rune >= 0.
// If r is not a valid Unicode code point, SimpleFold(r) returns r.
//
// For example:
//
//	SimpleFold('A') = 'a'
//	SimpleFold('a') = 'A'
//
//	SimpleFold('K') = 'k'
//	SimpleFold('k') = '\u212A' (Kelvin symbol, K)
//	SimpleFold('\u212A') = 'K'
//
//	SimpleFold('1') = '1'
//
//	SimpleFold(-2) = -2
public static rune SimpleFold(rune r) {
    if (r < 0 || r > MaxRune) {
        return r;
    }
    if ((nint)r < len(asciiFold)) {
        return (rune)asciiFold[r];
    }
    // Consult caseOrbit table for special cases.
    nint lo = 0;
    nint hi = len(caseOrbit);
    while (lo < hi) {
        nint m = (nint)(((nuint)(lo + hi) >> (int)(1)));
        if ((rune)caseOrbit[m].From < r){
            lo = m + 1;
        } else {
            hi = m;
        }
    }
    if (lo < len(caseOrbit) && (rune)caseOrbit[lo].From == r) {
        return (rune)caseOrbit[lo].To;
    }
    // No folding specified. This is a one- or two-element
    // equivalence class containing rune and ToLower(rune)
    // and ToUpper(rune) if they are different from rune.
    {
        var cr = lookupCaseRange(r, CaseRanges); if (cr != nil) {
            {
                var l = convertCase(LowerCase, r, ref (cr).DerefOrNull()); if (l != r) {
                    return l;
                }
            }
            return convertCase(UpperCase, r, ref (cr).DerefOrNull());
        }
    }
    return r;
}

} // end unicode_package
