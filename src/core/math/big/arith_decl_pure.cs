// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build math_big_pure_go
namespace go.math;

partial class big_package {

internal static Word /*c*/ addVV(slice<Word> z, slice<Word> x, slice<Word> y) {
    return addVV_g(z, x, y);
}

internal static Word /*c*/ subVV(slice<Word> z, slice<Word> x, slice<Word> y) {
    return subVV_g(z, x, y);
}

internal static Word /*c*/ lshVU(slice<Word> z, slice<Word> x, nuint s) {
    return lshVU_g(z, x, s);
}

internal static Word /*c*/ rshVU(slice<Word> z, slice<Word> x, nuint s) {
    return rshVU_g(z, x, s);
}

internal static Word /*c*/ mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r) {
    return mulAddVWW_g(z, x, y, r);
}

internal static Word /*c*/ addMulVVWW(slice<Word> z, slice<Word> x, slice<Word> y, Word m, Word a) {
    return addMulVVWW_g(z, x, y, m, a);
}

} // end big_package
