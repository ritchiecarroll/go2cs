// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http.@internal;

using strings = strings_package;
using unicode = unicode_package;

partial class ascii_package {

// EqualFold is [strings.EqualFold], ASCII only. It reports whether s and t
// are equal, ASCII-case-insensitively.
public static bool EqualFold(@string s, @string t) {
    if (len(s) != len(t)) {
        return false;
    }
    for (nint i = 0; i < len(s); i++) {
        if (lower(s[i]) != lower(t[i])) {
            return false;
        }
    }
    return true;
}

// lower returns the ASCII lowercase version of b.
internal static byte lower(byte b) {
    if ('A' <= b && b <= 'Z') {
        return (byte)(b + ((rune)'a' - (rune)'A'));
    }
    return b;
}

// IsPrint returns whether s is ASCII and printable according to
// https://tools.ietf.org/html/rfc20#section-4.2.
public static bool IsPrint(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] < ' ' || s[i] > '~') {
            return false;
        }
    }
    return true;
}

// Is returns whether s is ASCII.
public static bool Is(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] > unicode.MaxASCII) {
            return false;
        }
    }
    return true;
}

// ToLower returns the lowercase version of s if s is ASCII and printable.
public static (@string lower, bool ok) ToLower(@string s) {
    if (!IsPrint(s)) {
        return ("", false);
    }
    return (strings.ToLower(s), true);
}

} // end ascii_package
