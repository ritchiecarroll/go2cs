// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using Uint128 = global::go.@internal.strconv_package.uint128;

namespace go.@internal;

using static go.@internal.strconv_package;

partial class strconv_internal_test_package {

public static UntypedInt Pow10Min => /* pow10Min */ -348;
public static UntypedInt Pow10Max => /* pow10Max */ 347;

public static readonly ж<Func<nint, nint>> ᏑMulLog10_2 = new StaticBox<Func<nint, nint>>(mulLog10_2);
public static ref Func<nint, nint> MulLog10_2 => ref ((StaticBox<Func<nint, nint>>)ᏑMulLog10_2).ValueSlot;
public static readonly ж<Func<nint, nint>> ᏑMulLog2_10 = new StaticBox<Func<nint, nint>>(mulLog2_10);
public static ref Func<nint, nint> MulLog2_10 => ref ((StaticBox<Func<nint, nint>>)ᏑMulLog2_10).ValueSlot;
public static readonly ж<Func<@string, nint, (float64, nint, error)>> ᏑParseFloatPrefix = new StaticBox<Func<@string, nint, (float64, nint, error)>>(default(Func<@string, nint, (float64, nint, error)>));
public static ref Func<@string, nint, (float64, nint, error)> ParseFloatPrefix => ref ((StaticBox<Func<@string, nint, (float64, nint, error)>>)ᏑParseFloatPrefix).ValueSlot;
internal static void initᴛParseFloatPrefix() { ParseFloatPrefix = parseFloatPrefix; }
internal static readonly ж<Func<nint, (global::go.@internal.strconv_package.uint128, nint, bool)>> ᏑPow10 = new StaticBox<Func<nint, (global::go.@internal.strconv_package.uint128, nint, bool)>>(default(Func<nint, (global::go.@internal.strconv_package.uint128, nint, bool)>));
internal static ref Func<nint, (global::go.@internal.strconv_package.uint128, nint, bool)> Pow10 => ref ((StaticBox<Func<nint, (global::go.@internal.strconv_package.uint128, nint, bool)>>)ᏑPow10).ValueSlot;
internal static void initᴛPow10() { Pow10 = pow10; }
internal static readonly ж<Func<uint64, uint64, global::go.@internal.strconv_package.uint128>> ᏑUmul128 = new StaticBox<Func<uint64, uint64, global::go.@internal.strconv_package.uint128>>(umul128);
internal static ref Func<uint64, uint64, global::go.@internal.strconv_package.uint128> Umul128 => ref ((StaticBox<Func<uint64, uint64, global::go.@internal.strconv_package.uint128>>)ᏑUmul128).ValueSlot;
internal static readonly ж<Func<uint64, global::go.@internal.strconv_package.uint128, (uint64, uint64, uint64)>> ᏑUmul192 = new StaticBox<Func<uint64, global::go.@internal.strconv_package.uint128, (uint64, uint64, uint64)>>(umul192);
internal static ref Func<uint64, global::go.@internal.strconv_package.uint128, (uint64, uint64, uint64)> Umul192 => ref ((StaticBox<Func<uint64, global::go.@internal.strconv_package.uint128, (uint64, uint64, uint64)>>)ᏑUmul192).ValueSlot;
public static readonly ж<array<array<uint64>>> ᏑDiv5Tab = new StaticBox<array<array<uint64>>>(default(array<array<uint64>>));
public static ref array<array<uint64>> Div5Tab => ref ((StaticBox<array<array<uint64>>>)ᏑDiv5Tab).ValueSlot;
internal static void initᴛDiv5Tab() { Div5Tab = div5Tab.Clone(); }
public static readonly ж<Func<uint64, nint, bool>> ᏑDivisiblePow5 = new StaticBox<Func<uint64, nint, bool>>(default(Func<uint64, nint, bool>));
public static ref Func<uint64, nint, bool> DivisiblePow5 => ref ((StaticBox<Func<uint64, nint, bool>>)ᏑDivisiblePow5).ValueSlot;
internal static void initᴛDivisiblePow5() { DivisiblePow5 = divisiblePow5; }
public static readonly ж<Func<uint64, (uint64, nint)>> ᏑTrimZeros = new StaticBox<Func<uint64, (uint64, nint)>>(trimZeros);
public static ref Func<uint64, (uint64, nint)> TrimZeros => ref ((StaticBox<Func<uint64, (uint64, nint)>>)ᏑTrimZeros).ValueSlot;

internal static ж<global::go.@internal.strconv_package.@decimal> NewDecimal(uint64 i) {
    var d = @new<global::go.@internal.strconv_package.@decimal>();
    d.Assign(i);
    return d;
}

public static bool SetOptimize(bool b) {
    var old = optimize;
    optimize = b;
    return old;
}

} // end strconv_internal_test_package
