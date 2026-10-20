// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using constanttime = go.crypto.@internal.constanttime_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using bits = math.bits_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140deps;
using math;

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

    return constanttime.ByteEq(v, 0);
}

// ConstantTimeLessOrEqBytes returns 1 if x <= y and 0 otherwise. The comparison
// is lexigraphical, or big-endian. The time taken is a function of the length of
// the slices and is independent of the contents. If the lengths of x and y do not
// match it returns 0 immediately.
public static nint ConstantTimeLessOrEqBytes(slice<byte> x, slice<byte> y) {
    if (len(x) != len(y)) {
        return 0;
    }

    // Do a constant time subtraction chain y - x.
    // If there is no borrow at the end, then x <= y.
    uint64 b = default!;
    while (len(x) > 8) {
        var x0 = byteorder.BEUint64(x.slice(len(x) - 8));
        var y0 = byteorder.BEUint64(y.slice(len(y) - 8));
        (_, b) = bits.Sub64(y0, x0, b);
        x = x.slice(0, len(x) - 8);
        y = y.slice(0, len(y) - 8);
    }
    if (len(x) > 0) {
        var xb = new slice<byte>(8);
        var yb = new slice<byte>(8);
        copy(xb.slice(8 - len(x)), x);
        copy(yb.slice(8 - len(y)), y);
        var x0 = byteorder.BEUint64(xb);
        var y0 = byteorder.BEUint64(yb);
        (_, b) = bits.Sub64(y0, x0, b);
    }
    return (nint)(b ^ 1);
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

} // end subtle_package
