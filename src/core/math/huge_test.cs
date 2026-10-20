// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static math_package;
using testing = testing_package;
using static go.math_internal_test_package;

partial class math_test_package {

// Inputs to test trig_reduce
internal static slice<float64> trigHuge = new float64[]{
    1 << 28,
    1 << 29,
    1 << 30,
    34359738368D,
    1329227995784915872903807060280344576D,
    1766847064778384329583297500742918515827483896875618958121606201292619776D,
    3121748550315992231381597229793166305748598142664971150859156959625371738819765620120306103063491971159826931121406622895447975679288285306290176D,
    1891969788213177603238151163393619335935685080427297832880571827617792D,
    2514859209672213815954130451185339640703892284324672462185344991442198389642370554817310158862574988296192D,
    MaxFloat64
}.slice();

// Results for trigHuge[i] calculated with https://github.com/robpike/ivy
// using 4096 bits of working precision.   Values requiring less than
// 102 decimal digits (1 << 120, 1 << 240, 1 << 480, 1234567891234567 << 180)
// were confirmed via https://keisan.casio.com/
internal static slice<float64> cosHuge = new float64[]{
    -0.16556897949057876D,
    -0.94517382606089662D,
    0.78670712294118812D,
    -0.76466301249635305D,
    -0.92587902285483787D,
    0.93601042593353793D,
    -0.28282777640193788D,
    -0.14616431394103619D,
    -0.79456058210671406D,
    -0.99998768942655994D
}.slice();

internal static slice<float64> sinHuge = new float64[]{
    -0.98619821183697566D,
    0.32656766301856334D,
    -0.61732641504604217D,
    -0.64443035102329113D,
    0.37782010936075202D,
    -0.35197227524865778D,
    0.95917070894368716D,
    0.98926032637023618D,
    -0.60718488235646949D,
    0.00496195478918406D
}.slice();

internal static slice<float64> tanHuge = new float64[]{
    5.95641897939639421D,
    -0.34551069233430392D,
    -0.78469661331920043D,
    0.84276385870875983D,
    -0.40806638884180424D,
    -0.37603456702698076D,
    -3.39135965054779932D,
    -6.76813854009065030D,
    0.76417695016604922D,
    -0.00496201587444489D
}.slice();

// Check that trig values of huge angles return accurate results.
// This confirms that argument reduction works for very large values
// up to MaxFloat64.
public static void TestHugeCos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(trigHuge); i++) {
        var f1 = cosHuge[i];
        var f2 = Cos(trigHuge[i]);
        if (!close(f1, f2)) {
            Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, trigHuge[i], f2, f1);
        }
        var f3 = Cos(-trigHuge[i]);
        if (!close(f1, f3)) {
            Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, -trigHuge[i], f3, f1);
        }
    }
}

public static void TestHugeSin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(trigHuge); i++) {
        var f1 = sinHuge[i];
        var f2 = Sin(trigHuge[i]);
        if (!close(f1, f2)) {
            Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, trigHuge[i], f2, f1);
        }
        var f3 = Sin(-trigHuge[i]);
        if (!close(-f1, f3)) {
            Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, -trigHuge[i], f3, -f1);
        }
    }
}

public static void TestHugeSinCos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(trigHuge); i++) {
        var (f1, g1) = (sinHuge[i], cosHuge[i]);
        var (f2, g2) = Sincos(trigHuge[i]);
        if (!close(f1, f2) || !close(g1, g2)) {
            Ꮡt.Errorf("Sincos(%g) = %g, %g, want %g, %g"u8, trigHuge[i], f2, g2, f1, g1);
        }
        var (f3, g3) = Sincos(-trigHuge[i]);
        if (!close(-f1, f3) || !close(g1, g3)) {
            Ꮡt.Errorf("Sincos(%g) = %g, %g, want %g, %g"u8, -trigHuge[i], f3, g3, -f1, g1);
        }
    }
}

public static void TestHugeTan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(trigHuge); i++) {
        var f1 = tanHuge[i];
        var f2 = Tan(trigHuge[i]);
        if (!close(f1, f2)) {
            Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, trigHuge[i], f2, f1);
        }
        var f3 = Tan(-trigHuge[i]);
        if (!close(-f1, f3)) {
            Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, -trigHuge[i], f3, -f1);
        }
    }
}

} // end math_test_package
