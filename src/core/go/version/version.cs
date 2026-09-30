// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package version provides operations on [Go versions]
// in [Go toolchain name syntax]: strings like
// "go1.20", "go1.21.0", "go1.22rc2", and "go1.23.4-bigcorp".
//
// [Go versions]: https://go.dev/doc/toolchain#version
// [Go toolchain name syntax]: https://go.dev/doc/toolchain#name
namespace go.go;

// import "go/version"
using gover = @internal.gover_package;
using strings = strings_package;
using @internal;

partial class version_package {

// stripGo converts from a "go1.21-bigcorp" version to a "1.21" version.
// If v does not start with "go", stripGo returns the empty string (a known invalid version).
internal static @string stripGo(@string v) {
    (v, _, _) = strings.Cut(v, "-"u8); // strip -bigcorp suffix.
    if (len(v) < 2 || v[..2] != "go") {
        return ""u8;
    }
    return v[2..];
}

// Lang returns the Go language version for version x.
// If x is not a valid version, Lang returns the empty string.
// For example:
//
//	Lang("go1.21rc2") = "go1.21"
//	Lang("go1.21.2") = "go1.21"
//	Lang("go1.21") = "go1.21"
//	Lang("go1") = "go1"
//	Lang("bad") = ""
//	Lang("1.21") = ""
public static @string Lang(@string x) {
    @string v = gover.Lang(stripGo(x));
    if (v == ""u8) {
        return ""u8;
    }
    if (strings.HasPrefix(x[2..], v)){
        return x.slice(0, 2 + len(v)); // "go"+v without allocation
    } else {
        return "go"u8 + v;
    }
}

// Compare returns -1, 0, or +1 depending on whether
// x < y, x == y, or x > y, interpreted as Go versions.
// The versions x and y must begin with a "go" prefix: "go1.21" not "1.21".
// Invalid versions, including the empty string, compare less than
// valid versions and equal to each other.
// The language version "go1.21" compares less than the
// release candidate and eventual releases "go1.21rc1" and "go1.21.0".
public static nint Compare(@string x, @string y) {
    return gover.Compare(stripGo(x), stripGo(y));
}

// IsValid reports whether the version x is valid.
public static bool IsValid(@string x) {
    return gover.IsValid(stripGo(x));
}

} // end version_package
