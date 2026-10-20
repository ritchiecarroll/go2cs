// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package subtle implements functions that are often useful in cryptographic
// code but require careful thought to use correctly.
namespace go.crypto;

using constanttime = go.crypto.@internal.constanttime_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class subtle_package {

// These functions are forwarded to crypto/internal/constanttime for intrinsified
// operations, and to crypto/internal/fips140/subtle for byte slice operations.

// ConstantTimeCompare returns 1 if the two slices, x and y, have equal contents
// and 0 otherwise. The time taken is a function of the length of the slices and
// is independent of the contents. If the lengths of x and y do not match it
// returns 0 immediately.
public static nint ConstantTimeCompare(slice<byte> x, slice<byte> y) {
    return subtle.ConstantTimeCompare(x, y);
}

// ConstantTimeSelect returns x if v == 1 and y if v == 0.
// Its behavior is undefined if v takes any other value.
public static nint ConstantTimeSelect(nint v, nint x, nint y) {
    return constanttime.Select(v, x, y);
}

// ConstantTimeByteEq returns 1 if x == y and 0 otherwise.
public static nint ConstantTimeByteEq(uint8 x, uint8 y) {
    return constanttime.ByteEq(x, y);
}

// ConstantTimeEq returns 1 if x == y and 0 otherwise.
public static nint ConstantTimeEq(int32 x, int32 y) {
    return constanttime.Eq(x, y);
}

// ConstantTimeCopy copies the contents of y into x (a slice of equal length)
// if v == 1. If v == 0, x is left unchanged. Its behavior is undefined if v
// takes any other value.
public static void ConstantTimeCopy(nint v, slice<byte> x, slice<byte> y) {
    subtle.ConstantTimeCopy(v, x, y);
}

// ConstantTimeLessOrEq returns 1 if x <= y and 0 otherwise.
// Its behavior is undefined if x or y are negative or > 2**31 - 1.
public static nint ConstantTimeLessOrEq(nint x, nint y) {
    return constanttime.LessOrEq(x, y);
}

} // end subtle_package
