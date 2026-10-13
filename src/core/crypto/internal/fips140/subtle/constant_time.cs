// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

partial class subtle_package {

// ConstantTimeCompare returns 1 if the two slices, x and y, have equal contents
// and 0 otherwise. The time taken is a function of the length of the slices and
// is independent of the contents. If the lengths of x and y do not match it
// returns 0 immediately.
public static nint ConstantTimeCompare(slice<byte> x, slice<byte> y) {
    if (len(x) != len(y)) {
        return 0;
    }

    byte v = default!;

    for (nint i = 0; i < len(x); i++) {
        v |= (byte)((byte)(x[i] ^ y[i]));
    }

    return ConstantTimeByteEq(v, 0);
}

// ConstantTimeSelect returns x if v == 1 and y if v == 0.
// Its behavior is undefined if v takes any other value.
public static nint ConstantTimeSelect(nint v, nint x, nint y) {
    return (nint)((nint)(~(v - 1) & x) | (nint)((v - 1) & y));
}

// ConstantTimeByteEq returns 1 if x == y and 0 otherwise.
public static nint ConstantTimeByteEq(uint8 x, uint8 y) {
    return (nint)((((uint32)((uint8)(x ^ y)) - 1) >> (int)(31)));
}

// ConstantTimeEq returns 1 if x == y and 0 otherwise.
public static nint ConstantTimeEq(int32 x, int32 y) {
    return (nint)((((uint64)(uint32)((int32)(x ^ y)) - 1) >> (int)(63)));
}

// ConstantTimeCopy copies the contents of y into x (a slice of equal length)
// if v == 1. If v == 0, x is left unchanged. Its behavior is undefined if v
// takes any other value.
public static void ConstantTimeCopy(nint v, slice<byte> x, slice<byte> y) {
    if (len(x) != len(y)) {
        throw panic("subtle: slices have different lengths");
    }

    var xmask = (byte)(v - 1);
    var ymask = (byte)(~(v - 1));
    for (nint i = 0; i < len(x); i++) {
        x[i] = (byte)((byte)(x[i] & xmask) | (byte)(y[i] & ymask));
    }
}

// ConstantTimeLessOrEq returns 1 if x <= y and 0 otherwise.
// Its behavior is undefined if x or y are negative or > 2**31 - 1.
public static nint ConstantTimeLessOrEq(nint x, nint y) {
    var x32 = (int32)x;
    var y32 = (int32)y;
    return (nint)((int32)((((x32 - y32 - 1) >> (int)(31))) & 1));
}

} // end subtle_package
