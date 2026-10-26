// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class mime_package {

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ = GoBigConst.Parse("17331160586600743265244282880");

// isTSpecial reports whether c is in 'tspecials' as defined by RFC
// 1521 and RFC 2045.
internal static bool isTSpecial(byte c) {
    // tspecials :=  "(" / ")" / "<" / ">" / "@" /
    //               "," / ";" / ":" / "\" / <">
    //               "/" / "[" / "]" / "?" / "="
    //
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c >= 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	1<<'(' |
	1<<')' |
	1<<'<' |
	1<<'>' |
	1<<'@' |
	1<<',' |
	1<<';' |
	1<<':' |
	1<<'\\' |
	1<<'"' |
	1<<'/' |
	1<<'[' |
	1<<']' |
	1<<'?' |
	1<<'=' */
            maskᶜ;
    return ((uint64)((((uint64)1).Lsh((uint64)(c))) & (18158675342946992128UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (939524097UL))) != 0;
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ1 = GoBigConst.Parse("170141183443138071145086560442049888256");

// isTokenChar reports whether c is in 'token' as defined by RFC
// 1521 and RFC 2045.
internal static bool isTokenChar(byte c) {
    // token := 1*<any (US-ASCII) CHAR except SPACE, CTLs,
    //             or tspecials>
    //
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c >= 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	(1<<(10)-1)<<'0' |
	(1<<(26)-1)<<'a' |
	(1<<(26)-1)<<'A' |
	1<<'!' |
	1<<'#' |
	1<<'$' |
	1<<'%' |
	1<<'&' |
	1<<'\'' |
	1<<'*' |
	1<<'+' |
	1<<'-' |
	1<<'.' |
	1<<'^' |
	1<<'_' |
	1<<'`' |
	1<<'{' |
	1<<'|' |
	1<<'}' |
	1<<'~' */
            maskᶜ1;
    return ((uint64)((((uint64)1).Lsh((uint64)(c))) & (288068722172624896UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (9223372035915251710UL))) != 0;
}

// isToken reports whether s is a 'token' as defined by RFC 1521
// and RFC 2045.
internal static bool isToken(@string s) {
    if (s == ""u8) {
        return false;
    }
    foreach (var (_, c) in slice<byte>(s)) {
        if (!isTokenChar(c)) {
            return false;
        }
    }
    return true;
}

} // end mime_package
