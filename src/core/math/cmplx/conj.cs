// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

partial class cmplx_package {

// Conj returns the complex conjugate of x.
public static complex128 Conj(complex128 x) { return complex(real(x), -imag(x)); }

} // end cmplx_package
