// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using math = math_package;
using rand = go.math.rand_package;
using testing = testing_package;
using go.math;
using static go.@internal.trace_package;

partial class trace_internal_test_package {

public static void TestMUD(ж<testing.T> Ꮡt) {
    // Insert random uniforms and check histogram mass and
    // cumulative sum approximations.
    var rnd = rand.New(rand.NewSource(42));
    var mass = 0.0D;
    ref var mud = ref heap(new global::go.@internal.trace_package.mud(), out var Ꮡmud);
    for (nint i = 0; i < 100; i++) {
        var (area, l, r) = (rnd.Float64(), rnd.Float64(), rnd.Float64());
        if (rnd.Intn(10) == 0) {
            r = l;
        }
        Ꮡt.Log(l, r, area);
        Ꮡmud.add(l, r, area);
        mass += area;

        // Check total histogram weight.
        var hmass = 0.0D;
        foreach (var (_, val) in mud.hist.ΔRangeSnapshot()) {
            hmass += val;
        }
        if (!aeq(mass, hmass)) {
            Ꮡt.Fatalf("want mass %g, got %g"u8, mass, hmass);
        }

        // Check inverse cumulative sum approximations.
        for (var j = 0.0D; j < mass; j += mass * 0.099D) {
            mud.setTrackMass(j);
            var (lΔ1, u, ok) = mud.approxInvCumulativeSum();
            var (inv, ok2) = mud.invCumulativeSum(j);
            if (!ok || !ok2) {
                Ꮡt.Fatalf("inverse cumulative sum failed: approx %v, exact %v"u8, ok, ok2);
            }
            if (!(lΔ1 <= inv && inv < u)) {
                Ꮡt.Fatalf("inverse(%g) = %g, not ∈ [%g, %g)"u8, j, inv, lΔ1, u);
            }
        }
    }
}

public static void TestMUDTracking(ж<testing.T> Ꮡt) {
    // Test that the tracked mass is tracked correctly across
    // updates.
    var rnd = rand.New(rand.NewSource(42));
    UntypedInt uniforms = 100;
    for (var trackMass = 0.0D; trackMass < uniforms; trackMass += /* uniforms / 50 */ 2D) {
        ref var mud = ref heap(new global::go.@internal.trace_package.mud(), out var Ꮡmud);
        var mass = 0.0D;
        mud.setTrackMass(trackMass);
        for (nint i = 0; i < uniforms; i++) {
            var (area, l, r) = (rnd.Float64(), rnd.Float64(), rnd.Float64());
            Ꮡmud.add(l, r, area);
            mass += area;
            (l, var u, var ok) = mud.approxInvCumulativeSum();
            var (inv, ok2) = mud.invCumulativeSum(trackMass);

            if (mass < trackMass){
                if (ok) {
                    Ꮡt.Errorf("approx(%g) = [%g, %g), but mass = %g"u8, trackMass, l, u, mass);
                }
                if (ok2) {
                    Ꮡt.Errorf("exact(%g) = %g, but mass = %g"u8, trackMass, inv, mass);
                }
            } else {
                if (!ok) {
                    Ꮡt.Errorf("approx(%g) failed, but mass = %g"u8, trackMass, mass);
                }
                if (!ok2) {
                    Ꮡt.Errorf("exact(%g) failed, but mass = %g"u8, trackMass, mass);
                }
                if (ok && ok2 && !(l <= inv && inv < u)) {
                    Ꮡt.Errorf("inverse(%g) = %g, not ∈ [%g, %g)"u8, trackMass, inv, l, u);
                }
            }
        }
    }
}

// aeq returns true if x and y are equal up to 8 digits (1 part in 100
// million).
// TODO(amedee) dup of gc_test.go
internal static bool aeq(float64 x, float64 y) {
    if (x < 0D && y < 0D) {
        (x, y) = (-x, -y);
    }
    UntypedInt digits = 8;
    var factor = 1D - math.Pow(10D, /* -digits + 1 */ -7D);
    return x * factor <= y && y * factor <= x;
}

} // end trace_internal_test_package
