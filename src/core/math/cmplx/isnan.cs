// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;

partial class cmplx_package {

// IsNaN reports whether either real(x) or imag(x) is NaN
// and neither is an infinity.
public static bool IsNaN(complex128 x) {
    switch (ᐧ) {
    case {} when math.IsInf(real(x), 0) || math.IsInf(imag(x), 0): {
        return false;
    }
    case {} when math.IsNaN(real(x)) || math.IsNaN(imag(x)): {
        return true;
    }}
    return false;
}

// NaN returns a complex “not-a-number” value.
public static complex128 NaN() {
    var nan = math.NaN();
    return complex(nan, nan);
}

} // end cmplx_package
