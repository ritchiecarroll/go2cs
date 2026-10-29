// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Inverse of the floating-point error function.
*/
// This implementation is based on the rational approximation
// of percentage points of normal distribution available from
// https://www.jstor.org/stable/2347330.

// Coefficients for approximation to erf in |x| <= 0.85
internal static UntypedFloat a0 => 1.1975323115670912564578e0;
internal static UntypedFloat a1 => 4.7072688112383978012285e1;
internal static UntypedFloat a2 => 6.9706266534389598238465e2;
internal static UntypedFloat a3 => 4.8548868893843886794648e3;
internal static UntypedFloat a4 => 1.6235862515167575384252e4;
internal static UntypedFloat a5 => 2.3782041382114385731252e4;
internal static UntypedFloat a6 => 1.1819493347062294404278e4;
internal static UntypedFloat a7 => 8.8709406962545514830200e2;
internal static UntypedFloat b0 => 1.0000000000000000000e0;
internal static UntypedFloat b1 => 4.2313330701600911252e1;
internal static UntypedFloat b2 => 6.8718700749205790830e2;
internal static UntypedFloat b3 => 5.3941960214247511077e3;
internal static UntypedFloat b4 => 2.1213794301586595867e4;
internal static UntypedFloat b5 => 3.9307895800092710610e4;
internal static UntypedFloat b6 => 2.8729085735721942674e4;
internal static UntypedFloat b7 => 5.2264952788528545610e3;
// Coefficients for approximation to erf in 0.85 < |x| <= 1-2*exp(-25)
internal static UntypedFloat c0 => 1.42343711074968357734e0;
internal static UntypedFloat c1 => 4.63033784615654529590e0;
internal static UntypedFloat c2 => 5.76949722146069140550e0;
internal static UntypedFloat c3 => 3.64784832476320460504e0;
internal static UntypedFloat c4 => 1.27045825245236838258e0;
internal static UntypedFloat c5 => 2.41780725177450611770e-1;
internal static UntypedFloat c6 => 2.27238449892691845833e-2;
internal static UntypedFloat c7 => 7.74545014278341407640e-4;
internal static UntypedFloat d0 => 1.4142135623730950488016887e0;
internal static UntypedFloat d1 => 2.9036514445419946173133295e0;
internal static UntypedFloat d2 => 2.3707661626024532365971225e0;
internal static UntypedFloat d3 => 9.7547832001787427186894837e-1;
internal static UntypedFloat d4 => 2.0945065210512749128288442e-1;
internal static UntypedFloat d5 => 2.1494160384252876777097297e-2;
internal static UntypedFloat d6 => 7.7441459065157709165577218e-4;
internal static UntypedFloat d7 => 1.4859850019840355905497876e-9;
// Coefficients for approximation to erf in 1-2*exp(-25) < |x| < 1
internal static UntypedFloat e0 => 6.65790464350110377720e0;
internal static UntypedFloat e1 => 5.46378491116411436990e0;
internal static UntypedFloat e2 => 1.78482653991729133580e0;
internal static UntypedFloat e3 => 2.96560571828504891230e-1;
internal static UntypedFloat e4 => 2.65321895265761230930e-2;
internal static UntypedFloat e5 => 1.24266094738807843860e-3;
internal static UntypedFloat e6 => 2.71155556874348757815e-5;
internal static UntypedFloat e7 => 2.01033439929228813265e-7;
internal static UntypedFloat f0 => 1.414213562373095048801689e0;
internal static UntypedFloat f1 => 8.482908416595164588112026e-1;
internal static UntypedFloat f2 => 1.936480946950659106176712e-1;
internal static UntypedFloat f3 => 2.103693768272068968719679e-2;
internal static UntypedFloat f4 => 1.112800997078859844711555e-3;
internal static UntypedFloat f5 => 2.611088405080593625138020e-5;
internal static UntypedFloat f6 => 2.010321207683943062279931e-7;
internal static UntypedFloat f7 => 2.891024605872965461538222e-15;

// Erfinv returns the inverse error function of x.
//
// Special cases are:
//
//	Erfinv(1) = +Inf
//	Erfinv(-1) = -Inf
//	Erfinv(x) = NaN if x < -1 or x > 1
//	Erfinv(NaN) = NaN
public static float64 Erfinv(float64 x) {
    // special cases
    if (IsNaN(x) || x <= -1D || x >= 1D) {
        if (x == -1D || x == 1D) {
            return Inf((nint)x);
        }
        return NaN();
    }

    var sign = false;
    if (x < 0D) {
        x = -x;
        sign = true;
    }

    float64 ans = default;
    if (x <= 0.85D) { // |x| <= 0.85
        var r = 0.180625D - 0.25D * x * x;
        var z1 = (((((((float64)a7 * r + (float64)a6) * r + (float64)a5) * r + (float64)a4) * r + (float64)a3) * r + (float64)a2) * r + (float64)a1) * r + (float64)a0;
        var z2 = (((((((float64)b7 * r + (float64)b6) * r + (float64)b5) * r + (float64)b4) * r + (float64)b3) * r + (float64)b2) * r + (float64)b1) * r + (float64)b0;
        ans = (x * z1) / z2;
    } else {
        float64 z1 = default;
        float64 z2 = default;
        var r = Sqrt((float64)Ln2 - Log(1.0D - x));
        if (r <= 5.0D) {
            r -= 1.6D;
            z1 = (((((((float64)c7 * r + (float64)c6) * r + (float64)c5) * r + (float64)c4) * r + (float64)c3) * r + (float64)c2) * r + (float64)c1) * r + (float64)c0;
            z2 = (((((((float64)d7 * r + (float64)d6) * r + (float64)d5) * r + (float64)d4) * r + (float64)d3) * r + (float64)d2) * r + (float64)d1) * r + (float64)d0;
        } else {
            r -= 5.0D;
            z1 = (((((((float64)e7 * r + (float64)e6) * r + (float64)e5) * r + (float64)e4) * r + (float64)e3) * r + (float64)e2) * r + (float64)e1) * r + (float64)e0;
            z2 = (((((((float64)f7 * r + (float64)f6) * r + (float64)f5) * r + (float64)f4) * r + (float64)f3) * r + (float64)f2) * r + (float64)f1) * r + (float64)f0;
        }
        ans = z1 / z2;
    }

    if (sign) {
        return -ans;
    }
    return ans;
}

// Erfcinv returns the inverse of [Erfc](x).
//
// Special cases are:
//
//	Erfcinv(0) = +Inf
//	Erfcinv(2) = -Inf
//	Erfcinv(x) = NaN if x < 0 or x > 2
//	Erfcinv(NaN) = NaN
public static float64 Erfcinv(float64 x) {
    return Erfinv(1D - x);
}

} // end math_package
