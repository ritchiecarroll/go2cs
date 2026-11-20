// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class strconv_package {

// ParseBool returns the boolean value represented by the string.
// It accepts 1, t, T, TRUE, true, True, 0, f, F, FALSE, false, False.
// Any other value returns an error.
public static (bool, error) ParseBool(@string str) {
    switch (ᐧ) {
    case {} when str == "1"u8 || str == "t"u8 || str == "T"u8 || str == "true"u8 || str == "TRUE"u8 || str == "True"u8: {
        return (true, default);
    }
    case {} when str == "0"u8 || str == "f"u8 || str == "F"u8 || str == "false"u8 || str == "FALSE"u8 || str == "False"u8: {
        return (false, default);
    }}
    return (false, ErrSyntax);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;
internal static ref readonly @string falseˢ => ref falseˢᵇ;
private static readonly @string falseˢᵇ = "false"u8;

// FormatBool returns "true" or "false" according to the value of b.
public static @string FormatBool(bool b) {
    if (b) {
        return trueˢ;
    }
    return falseˢ;
}

// AppendBool appends "true" or "false", according to the value of b,
// to dst and returns the extended buffer.
public static slice<byte> AppendBool(slice<byte> dst, bool b) {
    if (b) {
        return append(dst, ((@string)"true").ꓸꓸꓸ);
    }
    return append(dst, ((@string)"false").ꓸꓸꓸ);
}

} // end strconv_package
