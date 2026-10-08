// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package checktest defines some code and data for use in
// the crypto/internal/fips140/check test.
namespace go.crypto.@internal.fips140.check;

// blank import: go.crypto.@internal.fips140.check_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using runtime = runtime_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // go:linkname

partial class checktest_package {

public static ж<nint> ᏑNOPTRDATA = new StandardBox<nint>(1);
public static ref nint NOPTRDATA => ref ᏑNOPTRDATA.Value;

// The linkname here disables asan registration of this global,
// because asan gets mad about rodata globals.
//
//go:linkname RODATA crypto/internal/fips140/check/checktest.RODATA
public static int32 RODATA; // set to 2 in asm.s

// DATA needs to have both a pointer and an int so that _some_ of it gets
// initialized at link time, so it is treated as DATA and not BSS.
// The pointer is deferred to init time.

partial struct DATAᴛ1 /*dyn*/ {
    public ж<nint> P;
    public nint X;
}
public static DATAᴛ1 DATA = new DATAᴛ1(ᏑNOPTRDATA, 3);

public static nint NOPTRBSS;

public static ж<nint> BSS;

public static void TEXT() {
}

internal static array<byte> globl12 = new(12);
internal static array<byte> globl8 = new(8);

[GoInit] internal static void init() {
    globl8 = new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.array();
    globl12 = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}.array();
    runtime.Gosched();
    var sum = (byte)0;
    foreach (var (_, x) in globl12.ΔRangeSnapshot()) {
        sum += x;
    }
    if (sum != 78) {
        throw panic("globl12 did not sum properly");
    }
    sum = (byte)0;
    foreach (var (_, x) in globl8.ΔRangeSnapshot()) {
        sum += x;
    }
    if (sum != 36) {
        throw panic("globl8 did not sum properly");
    }
}

} // end checktest_package
