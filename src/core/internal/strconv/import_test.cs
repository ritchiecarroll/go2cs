// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using uint128 = go.@internal.strconv_package.uint128;

namespace go.@internal;

using static go.@internal.strconv_package;
using static go.@internal.strconv_internal_test_package;
using strconv = go.@internal.strconv_package;

partial class strconv_test_package {

internal static UntypedInt pow10Min => /* Pow10Min */ -348;
internal static UntypedInt pow10Max => /* Pow10Max */ 347;

internal static Func<nint, nint> mulLog10_2 = strconv_internal_test_package.MulLog10_2;
internal static Func<nint, nint> mulLog2_10 = strconv_internal_test_package.MulLog2_10;
internal static Func<@string, nint, (float64, nint, error)> parseFloatPrefix = strconv_internal_test_package.ParseFloatPrefix;
internal static Func<nint, (strconv.uint128, nint, bool)> pow10 = strconv_internal_test_package.Pow10;
internal static Func<uint64, uint64, strconv.uint128> umul128 = strconv_internal_test_package.Umul128;
internal static Func<uint64, strconv.uint128, (uint64, uint64, uint64)> umul192 = strconv_internal_test_package.Umul192;
internal static array<array<uint64>> div5Tab = strconv_internal_test_package.Div5Tab.Clone();
internal static Func<uint64, nint, bool> divisiblePow5 = strconv_internal_test_package.DivisiblePow5;
internal static Func<uint64, (uint64, nint)> trimZeros = strconv_internal_test_package.TrimZeros;

} // end strconv_test_package
