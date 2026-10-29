// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.build;

using strconv = strconv_package;
using strings = strings_package;

partial class constraint_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string go1ˢ = "go1"u8;

// GoVersion returns the minimum Go version implied by a given build expression.
// If the expression can be satisfied without any Go version tags, GoVersion returns an empty string.
//
// For example:
//
//	GoVersion(linux && go1.22) = "go1.22"
//	GoVersion((linux && go1.22) || (windows && go1.20)) = "go1.20" => go1.20
//	GoVersion(linux) = ""
//	GoVersion(linux || (windows && go1.22)) = ""
//	GoVersion(!go1.22) = ""
//
// GoVersion assumes that any tag or negated tag may independently be true,
// so that its analysis can be purely structural, without SAT solving.
// “Impossible” subexpressions may therefore affect the result.
//
// For example:
//
//	GoVersion((linux && !linux && go1.20) || go1.21) = "go1.20"
public static @string GoVersion(Expr x) {
    nint v = minVersion(x, +1);
    if (v < 0) {
        return ""u8;
    }
    if (v == 0) {
        return go1ˢ;
    }
    return "go1."u8 + strconv.Itoa(v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string go1ˢ2 = "go1."u8;

// minVersion returns the minimum Go major version (9 for go1.9)
// implied by expression z, or if sign < 0, by expression !z.
internal static nint minVersion(Expr z, nint sign) {
    switch (z.type()) {
    default: {
        var zΔ1 = z;
        return -1;
    }
    case ж<AndExpr> zΔ1: {
        var op = andVersion;
        if (sign < 0) {
            op = orVersion;
        }
        return op(minVersion(zΔ1.X, sign), minVersion(zΔ1.Y, sign));
    }
    case ж<OrExpr> zΔ1: {
        var op = orVersion;
        if (sign < 0) {
            op = andVersion;
        }
        return op(minVersion(zΔ1.X, sign), minVersion(zΔ1.Y, sign));
    }
    case ж<NotExpr> zΔ1: {
        return minVersion(zΔ1.X, -sign);
    }
    case ж<TagExpr> zΔ1: {
        if (sign < 0) {
            // !foo implies nothing
            return -1;
        }
        if (zΔ1.Tag == "go1"u8) {
            return 0;
        }
        var (_, v, _) = strings.Cut(zΔ1.Tag, go1ˢ2);
        var (n, err) = strconv.Atoi(v);
        if (err != nil) {
            // not a go1.N tag
            return -1;
        }
        return n;
    }}
}

// andVersion returns the minimum Go version
// implied by the AND of two minimum Go versions,
// which is the max of the versions.
internal static nint andVersion(nint x, nint y) {
    if (x > y) {
        return x;
    }
    return y;
}

// orVersion returns the minimum Go version
// implied by the OR of two minimum Go versions,
// which is the min of the versions.
internal static nint orVersion(nint x, nint y) {
    if (x < y) {
        return x;
    }
    return y;
}

} // end constraint_package
