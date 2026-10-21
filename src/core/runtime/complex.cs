// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// inf2one returns a signed 1 if f is an infinity and a signed 0 otherwise.
// The sign of the result is the sign of f.
internal static float64 inf2one(float64 f) {
    var g = 0.0D;
    if (isInf(f)) {
        g = 1.0D;
    }
    return copysign(g, f);
}

internal static complex128 complex128div(complex128 n, complex128 m) {
    float64 e = default!;                  // complex(e, f) = n/m
    float64 f = default!;

    // Algorithm for robust complex division as described in
    // Robert L. Smith: Algorithm 116: Complex division. Commun. ACM 5(8): 435 (1962).
    if (abs(real(m)) >= abs(imag(m))){
        var ratio = imag(m) / real(m);
        var denom = real(m) + ratio * imag(m);
        e = (real(n) + imag(n) * ratio) / denom;
        f = (imag(n) - real(n) * ratio) / denom;
    } else {
        var ratio = real(m) / imag(m);
        var denom = imag(m) + ratio * real(m);
        e = (real(n) * ratio + imag(n)) / denom;
        f = (imag(n) * ratio - real(n)) / denom;
    }

    if (isNaN(e) && isNaN(f)) {
        // Correct final result to infinities and zeros if applicable.
        // Matches C99: ISO/IEC 9899:1999 - G.5.1  Multiplicative operators.

        var (a, b) = (real(n), imag(n));
        var (c, d) = (real(m), imag(m));

        switch (ᐧ) {
        case {} when m == 0D && (!isNaN(a) || !isNaN(b)): {
            e = copysign(inf, c) * a;
            f = copysign(inf, c) * b;
            break;
        }
        case {} when (isInf(a) || isInf(b)) && isFinite(c) && isFinite(d): {
            a = inf2one(a);
            b = inf2one(b);
            e = inf * (a * c + b * d);
            f = inf * (b * c - a * d);
            break;
        }
        case {} when (isInf(c) || isInf(d)) && isFinite(a) && isFinite(b): {
            c = inf2one(c);
            d = inf2one(d);
            e = 0D * (a * c + b * d);
            f = 0D * (b * c - a * d);
            break;
        }}
    }

    return complex(e, f);
}

} // end runtime_package
