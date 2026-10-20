// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

internal static void checkDistribution(ж<testing.T> Ꮡt, slice<ж<global::go.net_package.SRV>> data, float64 margin) {
    nint sum = 0;
    foreach (var (_, srv) in data) {
        sum += (nint)(~srv).Weight;
    }

    var results = new map<@string, nint>();

    nint count = 10000;
    for (nint j = 0; j < count; j++) {
        var d = new slice<ж<global::go.net_package.SRV>>(len(data));
        copy(d, data);
        ((global::go.net_package.byPriorityWeight)d).shuffleByWeight();
        @string key = d[0].Value.Target;
        results[key] = results[key] + 1;
    }

    nint actual = results[(~data[0]).Target];
    var expected = (float64)count * (float64)(~data[0]).Weight / (float64)sum;
    var diff = (float64)actual - expected;
    Ꮡt.Logf("actual: %v diff: %v e: %v m: %v"u8, actual, diff, expected, margin);
    if (diff < 0D) {
        diff = -diff;
    }
    if (diff > (expected * margin)) {
        Ꮡt.Errorf("missed target weight: expected %v, %v"u8, expected, actual);
    }
}

internal static void testUniformity(ж<testing.T> Ꮡt, nint size, float64 margin) {
    var data = new slice<ж<global::go.net_package.SRV>>(size);
    for (nint i = 0; i < size; i++) {
        data[i] = Ꮡ(new SRV(Target: ((@string)((rune)'a' + (rune)i)), Weight: 1));
    }
    checkDistribution(Ꮡt, data, margin);
}

public static void TestDNSSRVUniformity(ж<testing.T> Ꮡt) {
    testUniformity(Ꮡt, 2, 0.05D);
    testUniformity(Ꮡt, 3, 0.10D);
    testUniformity(Ꮡt, 10, 0.20D);
    testWeighting(Ꮡt, 0.05D);
}

internal static void testWeighting(ж<testing.T> Ꮡt, float64 margin) {
    var data = new ж<global::go.net_package.SRV>[]{
        Ꮡ(new global::go.net_package.SRV(Target: "a"u8, Weight: 60)),
        Ꮡ(new global::go.net_package.SRV(Target: "b"u8, Weight: 30)),
        Ꮡ(new global::go.net_package.SRV(Target: "c"u8, Weight: 10))
    }.slice();
    checkDistribution(Ꮡt, data, margin);
}

public static void TestWeighting(ж<testing.T> Ꮡt) {
    testWeighting(Ꮡt, 0.05D);
}

} // end net_internal_test_package
