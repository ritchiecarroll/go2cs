// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using color = image.color_package;
using testing = testing_package;
using image;
using static go.image_package;

partial class image_internal_test_package {

public static void TestYCbCr(ж<testing.T> Ꮡt) {
    var rects = new global::go.image_package.Rectangle[]{
        Rect(0, 0, 16, 16),
        Rect(1, 0, 16, 16),
        Rect(0, 1, 16, 16),
        Rect(1, 1, 16, 16),
        Rect(1, 1, 15, 16),
        Rect(1, 1, 16, 15),
        Rect(1, 1, 15, 15),
        Rect(2, 3, 14, 15),
        Rect(7, 0, 7, 16),
        Rect(0, 8, 16, 8),
        Rect(0, 0, 10, 11),
        Rect(5, 6, 16, 16),
        Rect(7, 7, 8, 8),
        Rect(7, 8, 8, 9),
        Rect(8, 7, 9, 8),
        Rect(8, 8, 9, 9),
        Rect(7, 7, 17, 17),
        Rect(8, 8, 17, 17),
        Rect(9, 9, 17, 17),
        Rect(10, 10, 17, 17)
    }.slice();
    var subsampleRatios = new global::go.image_package.YCbCrSubsampleRatio[]{
        YCbCrSubsampleRatio444,
        YCbCrSubsampleRatio422,
        YCbCrSubsampleRatio420,
        YCbCrSubsampleRatio440,
        YCbCrSubsampleRatio411,
        YCbCrSubsampleRatio410
    }.slice();
    var deltas = new global::go.image_package.Point[]{
        Pt(0, 0),
        Pt(1000, 1001),
        Pt(5001, -400),
        Pt(-701, -801)
    }.slice();
    foreach (var (_, r) in rects) {
        foreach (var (_, subsampleRatio) in subsampleRatios) {
            foreach (var (_, delta) in deltas) {
                testYCbCr(Ꮡt, r, subsampleRatio, delta);
            }
        }
        if (testing.Short()) {
            break;
        }
    }
}

internal static void testYCbCr(ж<testing.T> Ꮡt, global::go.image_package.Rectangle r, global::go.image_package.YCbCrSubsampleRatio subsampleRatio, global::go.image_package.Point delta) {
    // Create a YCbCr image m, whose bounds are r translated by (delta.X, delta.Y).
    var r1 = r.Add(delta);
    var m = NewYCbCr(r1, subsampleRatio);
    // Test that the image buffer is reasonably small even if (delta.X, delta.Y) is far from the origin.
    if (len((~m).Y) > 100 * 100) {
        Ꮡt.Errorf("r=%v, subsampleRatio=%v, delta=%v: image buffer is too large"u8,
            r, subsampleRatio, delta);
        return;
    }
    // Initialize m's pixels. For 422 and 420 subsampling, some of the Cb and Cr elements
    // will be set multiple times. That's OK. We just want to avoid a uniform image.
    for (nint y = r1.Min.Y; y < r1.Max.Y; y++) {
        for (nint x = r1.Min.X; x < r1.Max.X; x++) {
            nint yi = m.YOffset(x, y);
            nint ci = m.COffset(x, y);
            m.Value.Y[yi] = (uint8)(16 * y + x);
            m.Value.Cb[ci] = (uint8)(y + 16 * x);
            m.Value.Cr[ci] = (uint8)(y + 16 * x);
        }
    }
    // Make various sub-images of m.
    for (nint y0 = delta.Y + 3; y0 < delta.Y + 7; y0++) {
        for (nint y1 = delta.Y + 8; y1 < delta.Y + 13; y1++) {
            for (nint x0 = delta.X + 3; x0 < delta.X + 7; x0++) {
                for (nint x1 = delta.X + 8; x1 < delta.X + 13; x1++) {
                    var subRect = Rect(x0, y0, x1, y1);
                    var sub = m.SubImage(subRect)._<ж<global::go.image_package.YCbCr>>();
                    // For each point in the sub-image's bounds, check that m.At(x, y) equals sub.At(x, y).
                    for (nint y = sub.Value.Rect.Min.Y; y < (~sub).Rect.Max.Y; y++) {
                        for (nint x = sub.Value.Rect.Min.X; x < (~sub).Rect.Max.X; x++) {
                            var color0 = m.At(x, y)._<color.YCbCr>();
                            var color1 = sub.At(x, y)._<color.YCbCr>();
                            if (color0 != color1) {
                                Ꮡt.Errorf("r=%v, subsampleRatio=%v, delta=%v, x=%d, y=%d, color0=%v, color1=%v"u8,
                                    r, subsampleRatio, delta, x, y, color0, color1);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}

public static void TestYCbCrSlicesDontOverlap(ж<testing.T> Ꮡt) {
    var m = NewYCbCr(Rect(0, 0, 8, 8), YCbCrSubsampleRatio420);
    var names = new @string[]{"Y"u8, "Cb"u8, "Cr"u8}.slice();
    var slices = new slice<byte>[]{
        (~m).Y.slice(0, cap((~m).Y)),
        (~m).Cb.slice(0, cap((~m).Cb)),
        (~m).Cr.slice(0, cap((~m).Cr))
    }.slice();
    foreach (var (i, Δslice) in slices) {
        var want = (uint8)(10 + i);
        foreach (var (j, _) in Δslice) {
            Δslice[j] = want;
        }
    }
    foreach (var (i, Δslice) in slices) {
        var want = (uint8)(10 + i);
        foreach (var (j, got) in Δslice) {
            if (got != want) {
                Ꮡt.Fatalf("m.%s[%d]: got %d, want %d"u8, names[i], j, got, want);
            }
        }
    }
}

} // end image_internal_test_package
