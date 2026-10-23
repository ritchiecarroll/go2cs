// Code generated from gen_encoding_table.go using 'go generate'; DO NOT EDIT.
// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class url_package {

partial struct encoding /*num:uint8*/;

internal static encoding encodePath => /* 1 << iota */ 1;
internal static encoding encodePathSegment => 2;
internal static encoding encodeHost => 4;
internal static encoding encodeZone => 8;
internal static encoding encodeUserPassword => 16;
internal static encoding encodeQueryComponent => 32;
internal static encoding encodeFragment => 64;

// hexChar is actually NOT an encoding mode, but there are only seven
// encoding modes. We might as well abuse the otherwise unused most
// significant bit in uint8 to indicate whether a character is
// hexadecimal.
internal static encoding hexChar => 128;

[GoLazy] partial class lazyᴛtable { internal static array<encoding> table = new array<encoding>(256){
    [(rune)'!'] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    [(rune)'"'] = (encoding)(encodeZone | encodeHost),
    [(rune)'$'] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'&'] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'\''] = (encoding)(encodeZone | encodeHost),
    [(rune)'('] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    [(rune)')'] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    [(rune)'*'] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    [(rune)'+'] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)','] = (encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePath),
    [(rune)'-'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'.'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'/'] = (encoding)(encodeFragment | encodePath),
    [(rune)'0'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'1'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'2'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'3'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'4'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'5'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'6'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'7'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'8'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'9'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)':'] = (encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)';'] = (encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePath),
    [(rune)'<'] = (encoding)(encodeZone | encodeHost),
    [(rune)'='] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'>'] = (encoding)(encodeZone | encodeHost),
    [(rune)'?'] = encodeFragment,
    [(rune)'@'] = (encoding)((encoding)(encodeFragment | encodePathSegment) | encodePath),
    [(rune)'A'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'B'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'C'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'D'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'E'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'F'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'G'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'H'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'I'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'J'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'K'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'L'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'M'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'N'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'O'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'P'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'Q'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'R'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'S'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'T'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'U'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'V'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'W'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'X'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'Y'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'Z'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'['] = (encoding)(encodeZone | encodeHost),
    [(rune)']'] = (encoding)(encodeZone | encodeHost),
    [(rune)'_'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'a'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'b'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'c'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'d'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'e'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'f'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'g'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'h'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'i'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'j'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'k'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'l'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'m'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'n'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'o'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'p'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'q'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'r'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'s'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'t'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'u'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'v'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'w'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'x'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'y'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'z'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [(rune)'~'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath)
}; }

} // end url_package
