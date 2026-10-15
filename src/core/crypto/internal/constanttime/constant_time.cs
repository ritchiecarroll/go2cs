// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

partial class constanttime_package {

// The functions in this package are compiler intrinsics for constant-time
// operations. They are exposed by crypto/subtle and used directly by the
// FIPS 140-3 module.

// Select returns x if v == 1 and y if v == 0.
// Its behavior is undefined if v takes any other value.
public static nint Select(nint v, nint x, nint y) {
    // This is intrinsicified on arches with CMOV.
    // It implements the following superset behavior:
    // ConstantTimeSelect returns x if v != 0 and y if v == 0.
    // Do the same here to avoid non portable UB.
    v = (nint)boolToUint8(v != 0);
    return (nint)((nint)(~(v - 1) & x) | (nint)((v - 1) & y));
}

// ByteEq returns 1 if x == y and 0 otherwise.
public static nint ByteEq(uint8 x, uint8 y) {
    return (nint)boolToUint8(x == y);
}

// Eq returns 1 if x == y and 0 otherwise.
public static nint Eq(int32 x, int32 y) {
    return (nint)boolToUint8(x == y);
}

// LessOrEq returns 1 if x <= y and 0 otherwise.
// Its behavior is undefined if x or y are negative or > 2**31 - 1.
public static nint LessOrEq(nint x, nint y) {
    return (nint)boolToUint8(x <= y);
}

// boolToUint8 is a compiler intrinsic.
// It returns 1 for true and 0 for false.
internal static uint8 boolToUint8(bool b) {
    throw panic("unreachable; must be intrinsicified");
}

} // end constanttime_package
