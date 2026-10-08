// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

partial class flate_package {

internal static UntypedInt lengthShift => 22;
internal static UntypedInt offsetMask => /* 1<<lengthShift - 1 */ 4194303;
internal static UntypedInt typeMask => /* 3 << 30 */ 3221225472;
internal static UntypedInt literalType => /* 0 << 30 */ 0;
internal static UntypedInt matchType => /* 1 << 30 */ 1073741824;

// The length code for length X (MIN_MATCH_LENGTH <= X <= MAX_MATCH_LENGTH)
// is lengthCodes[length - MIN_MATCH_LENGTH]
internal static array<uint32> lengthCodes = new uint32[]{
    0, 1, 2, 3, 4, 5, 6, 7, 8, 8,
    9, 9, 10, 10, 11, 11, 12, 12, 12, 12,
    13, 13, 13, 13, 14, 14, 14, 14, 15, 15,
    15, 15, 16, 16, 16, 16, 16, 16, 16, 16,
    17, 17, 17, 17, 17, 17, 17, 17, 18, 18,
    18, 18, 18, 18, 18, 18, 19, 19, 19, 19,
    19, 19, 19, 19, 20, 20, 20, 20, 20, 20,
    20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
    21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
    21, 21, 21, 21, 21, 21, 22, 22, 22, 22,
    22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
    22, 22, 23, 23, 23, 23, 23, 23, 23, 23,
    23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
    24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
    24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
    24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
    25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
    25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
    25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
    25, 25, 26, 26, 26, 26, 26, 26, 26, 26,
    26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
    26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
    26, 26, 26, 26, 27, 27, 27, 27, 27, 27,
    27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
    27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
    27, 27, 27, 27, 27, 28
}.array();

internal static array<uint32> offsetCodes = new uint32[]{
    0, 1, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7,
    8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9,
    10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
    11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
    12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
    12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
    13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
    13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
    14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
    14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
    14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
    14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
    15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
    15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
    15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
    15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15
}.array();

partial struct token /*num:uint32*/;

// Convert a literal into a literal token.
internal static token literalToken(uint32 literal) {
    return ((token)((uint32)literalType + literal));
}

// Convert a < xlength, xoffset > pair into a match token.
internal static token matchToken(uint32 xlength, uint32 xoffset) {
    return ((token)((uint32)matchType + (xlength << (int)(lengthShift)) + xoffset));
}

// Returns the literal of a literal token.
internal static uint32 literal(this token t) {
    return (uint32)(t - (uint32)literalType);
}

// Returns the extra offset of a match token.
internal static uint32 offset(this token t) {
    return (uint32)((uint32)t & (uint32)offsetMask);
}

internal static uint32 length(this token t) {
    return (uint32)(((t - (uint32)matchType) >> (int)(lengthShift)));
}

internal static uint32 lengthCode(uint32 len) {
    return lengthCodes[len];
}

// Returns the offset code corresponding to a specific offset.
internal static uint32 offsetCode(uint32 off) {
    if (off < (uint32)len(offsetCodes)) {
        return offsetCodes[off];
    }
    if ((off >> (int)(7)) < (uint32)len(offsetCodes)) {
        return offsetCodes[(off >> (int)(7))] + 14;
    }
    return offsetCodes[(off >> (int)(14))] + 28;
}

} // end flate_package
