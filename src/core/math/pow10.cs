// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// pow10tab stores the pre-computed values 10**i for i < 32.
internal static array<float64> pow10tab = new float64[]{
    1e00D, 1e01D, 1e02D, 1e03D, 1e04D, 1e05D, 1e06D, 1e07D, 1e08D, 1e09D,
    1e10D, 1e11D, 1e12D, 1e13D, 1e14D, 1e15D, 1e16D, 1e17D, 1e18D, 1e19D,
    1e20D, 1e21D, 1e22D, 1e23D, 1e24D, 1e25D, 1e26D, 1e27D, 1e28D, 1e29D,
    1e30D, 1e31D
}.array();

// pow10postab32 stores the pre-computed value for 10**(i*32) at index i.
internal static array<float64> pow10postab32 = new float64[]{
    1e00D, 1e32D, 1e64D, 1e96D, 1e128D, 1e160D, 1e192D, 1e224D, 1e256D, 1e288D
}.array();

// pow10negtab32 stores the pre-computed value for 10**(-i*32) at index i.
internal static array<float64> pow10negtab32 = new float64[]{
    1e-00D, 1e-32D, 1e-64D, 1e-96D, 1e-128D, 1e-160D, 1e-192D, 1e-224D, 1e-256D, 1e-288D, 1e-320D
}.array();

// Pow10 returns 10**n, the base-10 exponential of n.
//
// Special cases are:
//
//	Pow10(n) =    0 for n < -323
//	Pow10(n) = +Inf for n > 308
public static float64 Pow10(nint n) {
    if (0 <= n && n <= 308) {
        return pow10postab32[(nuint)n / 32] * pow10tab[(nuint)n % 32];
    }

    if (-323 <= n && n <= 0) {
        return pow10negtab32[(nuint)(-n) / 32] / pow10tab[(nuint)(-n) % 32];
    }

    // n < -323 || 308 < n
    if (n > 0) {
        return Inf(1);
    }

    // n < -323
    return 0D;
}

} // end math_package
