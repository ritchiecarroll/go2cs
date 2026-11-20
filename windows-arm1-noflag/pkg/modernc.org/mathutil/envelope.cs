// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;

partial class mathutil_package {

partial struct Approximation /*num:nint*/;

internal static Approximation _ᴛ1ʗ => /* iota */ 0;
public static Approximation Linear => 1;
public static Approximation Sinusoidal => 2;

public static float64 Envelope(float64 x, slice<float64> points, Approximation approximation) {
    var step = 1D / (float64)(len(points) - 1);
    var fslot = math.Floor(x / step);
    var mod = x - fslot * step;
    nint slot = (nint)fslot;
    var (l, r) = (points[slot], points[slot + 1]);
    var rmod = mod / step;
    var matchᴛ1 = false;
    if (approximation == Sinusoidal) { matchᴛ1 = true;
        var k = (math.Sin((float64)math.Pi * (rmod - 0.5D)) + 1D) / 2D;
        return l + (r - l) * k;
    }
    if (approximation == Linear) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return l + (r - l) * rmod;
    }
    return default;

}

} // end mathutil_package
