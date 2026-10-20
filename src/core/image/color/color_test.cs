// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using testing = testing_package;
using quick = go.testing.quick_package;
using go.testing;
using static go.image.color_package;

partial class color_internal_test_package {

public static void TestSqDiff(ж<testing.T> Ꮡt) {
    // canonical sqDiff implementation
    var orig = (uint32 x, uint32 y) => {
        uint32 d = default!;
        if (x > y){
            d = (uint32)(x - y);
        } else {
            d = (uint32)(y - x);
        }
        return (d * d) >> 2;
    };
    var testCases = new uint32[]{
        0,
        1,
        2,
        0x0fffd,
        0x0fffe,
        0x0ffff,
        0x10000,
        0x10001,
        0x10002,
        0xfffffffdU,
        0xfffffffeU,
        0xffffffffU
    }.slice();
    foreach (var (_, x) in testCases) {
        foreach (var (_, y) in testCases) {
            {
                var (got, want) = (sqDiff(x, y), orig(x, y)); if (got != want) {
                    Ꮡt.Fatalf("sqDiff(%#x, %#x): got %d, want %d"u8, x, y, got, want);
                }
            }
        }
    }
    {
        var err = quick.CheckEqual((orig).OrTypedNilFunc(), sqDiff, Ꮡ(new quick.Config(MaxCountScale: 10D))); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

} // end color_internal_test_package
