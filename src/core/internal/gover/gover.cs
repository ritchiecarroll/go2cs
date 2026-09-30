// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gover implements support for Go toolchain versions like 1.21.0 and 1.21rc1.
// (For historical reasons, Go does not use semver for its toolchains.)
// This package provides the same basic analysis that golang.org/x/mod/semver does for semver.
//
// The go/version package should be imported instead of this one when possible.
// Note that this package works on "1.21" while go/version works on "go1.21".
namespace go.@internal;

using cmp = cmp_package;

partial class gover_package {

// A Version is a parsed Go version: major[.Minor[.Patch]][kind[pre]]
// The numbers are the original decimal strings to avoid integer overflows
// and since there is very little actual math. (Probably overflow doesn't matter in practice,
// but at the time this code was written, there was an existing test that used
// go1.99999999999, which does not fit in an int on 32-bit platforms.
// The "big decimal" representation avoids the problem entirely.)
[GoType] partial struct Version {
    public @string Major; // decimal
    public @string Minor; // decimal or ""
    public @string Patch; // decimal or ""
    public @string Kind; // "", "alpha", "beta", "rc"
    public @string Pre; // decimal or ""
}

// Compare returns -1, 0, or +1 depending on whether
// x < y, x == y, or x > y, interpreted as toolchain versions.
// The versions x and y must not begin with a "go" prefix: just "1.21" not "go1.21".
// Malformed versions compare less than well-formed versions and equal to each other.
// The language version "1.21" compares less than the release candidate and eventual releases "1.21rc1" and "1.21.0".
public static nint Compare(@string x, @string y) {
    var vx = Parse(x);
    var vy = Parse(y);
    {
        nint c = CmpInt(vx.Major, vy.Major); if (c != 0) {
            return c;
        }
    }
    {
        nint c = CmpInt(vx.Minor, vy.Minor); if (c != 0) {
            return c;
        }
    }
    {
        nint c = CmpInt(vx.Patch, vy.Patch); if (c != 0) {
            return c;
        }
    }
    {
        nint c = cmp.Compare(vx.Kind, vy.Kind); if (c != 0) {
            // "" < alpha < beta < rc
            return c;
        }
    }
    {
        nint c = CmpInt(vx.Pre, vy.Pre); if (c != 0) {
            return c;
        }
    }
    return 0;
}

// Max returns the maximum of x and y interpreted as toolchain versions,
// compared using Compare.
// If x and y compare equal, Max returns x.
public static @string Max(@string x, @string y) {
    if (Compare(x, y) < 0) {
        return y;
    }
    return x;
}

// IsLang reports whether v denotes the overall Go language version
// and not a specific release. Starting with the Go 1.21 release, "1.x" denotes
// the overall language version; the first release is "1.x.0".
// The distinction is important because the relative ordering is
//
//	1.21 < 1.21rc1 < 1.21.0
//
// meaning that Go 1.21rc1 and Go 1.21.0 will both handle go.mod files that
// say "go 1.21", but Go 1.21rc1 will not handle files that say "go 1.21.0".
public static bool IsLang(@string x) {
    var v = Parse(x);
    return v != new Version(nil) && v.Patch == ""u8 && v.Kind == ""u8 && v.Pre == ""u8;
}

// Lang returns the Go language version. For example, Lang("1.2.3") == "1.2".
public static @string Lang(@string x) {
    var v = Parse(x);
    if (v.Minor == ""u8 || v.Major == "1"u8 && v.Minor == "0"u8) {
        return v.Major;
    }
    return v.Major + "."u8 + v.Minor;
}

// IsValid reports whether the version x is valid.
public static bool IsValid(@string x) {
    return Parse(x) != new Version(nil);
}

// Parse parses the Go version string x into a version.
// It returns the zero version if x is malformed.
public static Version Parse(@string x) {
    Version v = default!;
    // Parse major version.
    bool ok = default!;
    (v.Major, x, ok) = cutInt(x);
    if (!ok) {
        return new Version(nil);
    }
    if (x == ""u8) {
        // Interpret "1" as "1.0.0".
        v.Minor = "0"u8;
        v.Patch = "0"u8;
        return v;
    }
    // Parse . before minor version.
    if (x[0] != (rune)'.') {
        return new Version(nil);
    }
    // Parse minor version.
    (v.Minor, x, ok) = cutInt(x[1..]);
    if (!ok) {
        return new Version(nil);
    }
    if (x == ""u8) {
        // Patch missing is same as "0" for older versions.
        // Starting in Go 1.21, patch missing is different from explicit .0.
        if (CmpInt(v.Minor, "21"u8) < 0) {
            v.Patch = "0"u8;
        }
        return v;
    }
    // Parse patch if present.
    if (x[0] == (rune)'.') {
        (v.Patch, x, ok) = cutInt(x[1..]);
        if (!ok || x != ""u8) {
            // Note that we are disallowing prereleases (alpha, beta, rc) for patch releases here (x != "").
            // Allowing them would be a bit confusing because we already have:
            //	1.21 < 1.21rc1
            // But a prerelease of a patch would have the opposite effect:
            //	1.21.3rc1 < 1.21.3
            // We've never needed them before, so let's not start now.
            return new Version(nil);
        }
        return v;
    }
    // Parse prerelease.
    nint i = 0;
    while (i < len(x) && (x[i] < (rune)'0' || (rune)'9' < x[i])) {
        if (x[i] < (rune)'a' || (rune)'z' < x[i]) {
            return new Version(nil);
        }
        i++;
    }
    if (i == 0) {
        return new Version(nil);
    }
    (v.Kind, x) = (x.slice(0, i), x.slice(i));
    if (x == ""u8) {
        return v;
    }
    (v.Pre, x, ok) = cutInt(x);
    if (!ok || x != ""u8) {
        return new Version(nil);
    }
    return v;
}

// cutInt scans the leading decimal number at the start of x to an integer
// and returns that value and the rest of the string.
internal static (@string n, @string rest, bool ok) cutInt(@string x) {
    nint i = 0;
    while (i < len(x) && (rune)'0' <= x[i] && x[i] <= (rune)'9') {
        i++;
    }
    if (i == 0 || x[0] == (rune)'0' && i != 1) {
        // no digits or unnecessary leading zero
        return ("", "", false);
    }
    return (x.slice(0, i), x.slice(i), true);
}

// CmpInt returns cmp.Compare(x, y) interpreting x and y as decimal numbers.
// (Copied from golang.org/x/mod/semver's compareInt.)
public static nint CmpInt(@string x, @string y) {
    if (x == y) {
        return 0;
    }
    if (len(x) < len(y)) {
        return -1;
    }
    if (len(x) > len(y)) {
        return +1;
    }
    if (x < y){
        return -1;
    } else {
        return +1;
    }
}

// DecInt returns the decimal string decremented by 1, or the empty string
// if the decimal is all zeroes.
// (Copied from golang.org/x/mod/module's decDecimal.)
public static @string DecInt(@string @decimal) {
    // Scan right to left turning 0s to 9s until you find a digit to decrement.
    var digits = slice<byte>(@decimal);
    nint i = len(digits) - 1;
    for (; i >= 0 && digits[i] == (rune)'0'; i--) {
        digits[i] = (rune)'9';
    }
    if (i < 0) {
        // decimal is all zeros
        return ""u8;
    }
    if (i == 0 && digits[i] == (rune)'1' && len(digits) > 1){
        digits = digits[1..];
    } else {
        digits[i]--;
    }
    return ((@string)digits);
}

} // end gover_package
