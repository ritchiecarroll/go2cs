// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static strconv_package;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

internal static Func<@string, @string, nint, ж<strconv.NumError>> baseError = strconv_internal_test_package.BaseError;
internal static Func<@string, @string, nint, ж<strconv.NumError>> bitSizeError = strconv_internal_test_package.BitSizeError;

} // end strconv_test_package
