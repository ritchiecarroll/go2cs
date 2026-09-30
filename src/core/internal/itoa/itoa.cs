// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Simple conversions to avoid depending on strconv.
namespace go.@internal;

partial class itoa_package {

// Itoa converts val to a decimal string.
public static @string Itoa(nint val) {
    if (val < 0) {
        return "-"u8 + Uitoa((nuint)(-val));
    }
    return Uitoa((nuint)val);
}

// Uitoa converts val to a decimal string.
public static @string Uitoa(nuint val) {
    if (val == 0) {
        // avoid string allocation
        return "0"u8;
    }
    array<byte> buf = new(20);               // big enough for 64bit value base 10
    nint i = len(buf) - 1;
    while (val >= 10) {
        nuint q = val / 10;
        buf[i] = (byte)((rune)'0' + val - q * 10);
        i--;
        val = q;
    }
    // val < 10
    buf[i] = (byte)((rune)'0' + val);
    return ((@string)(buf.slice(i)));
}

internal static readonly @string hex = "0123456789abcdef"u8;

// Uitox converts val (a uint) to a hexadecimal string.
public static @string Uitox(nuint val) {
    if (val == 0) {
        // avoid string allocation
        return "0x0"u8;
    }
    array<byte> buf = new(20);               // big enough for 64bit value base 16 + 0x
    nint i = len(buf) - 1;
    while (val >= 16) {
        nuint q = val / 16;
        buf[i] = hex[val % 16];
        i--;
        val = q;
    }
    // val < 16
    buf[i] = hex[val % 16];
    i--;
    buf[i] = (rune)'x';
    i--;
    buf[i] = (rune)'0';
    return ((@string)(buf.slice(i)));
}

} // end itoa_package
