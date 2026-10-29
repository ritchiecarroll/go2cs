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
    ['!'] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    ['"'] = (encoding)(encodeZone | encodeHost),
    ['$'] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['&'] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['\''] = (encoding)(encodeZone | encodeHost),
    ['('] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    [')'] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    ['*'] = (encoding)((encoding)(encodeFragment | encodeZone) | encodeHost),
    ['+'] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [','] = (encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePath),
    ['-'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['.'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['/'] = (encoding)(encodeFragment | encodePath),
    ['0'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['1'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['2'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['3'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['4'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['5'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['6'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['7'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['8'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['9'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [':'] = (encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    [';'] = (encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePath),
    ['<'] = (encoding)(encodeZone | encodeHost),
    ['='] = (encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['>'] = (encoding)(encodeZone | encodeHost),
    ['?'] = encodeFragment,
    ['@'] = (encoding)((encoding)(encodeFragment | encodePathSegment) | encodePath),
    ['A'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['B'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['C'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['D'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['E'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['F'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['G'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['H'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['I'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['J'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['K'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['L'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['M'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['N'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['O'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['P'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['Q'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['R'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['S'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['T'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['U'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['V'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['W'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['X'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['Y'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['Z'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['['] = (encoding)(encodeZone | encodeHost),
    [']'] = (encoding)(encodeZone | encodeHost),
    ['_'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['a'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['b'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['c'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['d'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['e'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['f'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(hexChar | encodeFragment) | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['g'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['h'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['i'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['j'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['k'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['l'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['m'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['n'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['o'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['p'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['q'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['r'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['s'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['t'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['u'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['v'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['w'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['x'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['y'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['z'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath),
    ['~'] = (encoding)((encoding)((encoding)((encoding)((encoding)((encoding)(encodeFragment | encodeQueryComponent) | encodeUserPassword) | encodeZone) | encodeHost) | encodePathSegment) | encodePath)
}; }

} // end url_package
