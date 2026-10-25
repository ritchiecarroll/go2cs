// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using Uint128 = global::go.@internal.strconv_package.uint128;

namespace go.@internal;

using static go.@internal.strconv_package;

partial class strconv_internal_test_package {

public static UntypedInt Pow10Min => /* pow10Min */ -348;
public static UntypedInt Pow10Max => /* pow10Max */ 347;

public static Func<nint, nint> MulLog10_2 = mulLog10_2;
public static Func<nint, nint> MulLog2_10 = mulLog2_10;
public static Func<@string, nint, (float64, nint, error)> ParseFloatPrefix;
internal static void initᴛParseFloatPrefix() { ParseFloatPrefix = parseFloatPrefix; }
internal static Func<nint, (global::go.@internal.strconv_package.uint128, nint, bool)> Pow10;
internal static void initᴛPow10() { Pow10 = pow10; }
internal static Func<uint64, uint64, global::go.@internal.strconv_package.uint128> Umul128 = umul128;
internal static Func<uint64, global::go.@internal.strconv_package.uint128, (uint64, uint64, uint64)> Umul192 = umul192;
public static array<array<uint64>> Div5Tab;
internal static void initᴛDiv5Tab() { Div5Tab = div5Tab.Clone(); }
public static Func<uint64, nint, bool> DivisiblePow5;
internal static void initᴛDivisiblePow5() { DivisiblePow5 = divisiblePow5; }
public static Func<uint64, (uint64, nint)> TrimZeros = trimZeros;

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
