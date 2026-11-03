// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testing = testing_package;
using static go.image_package;

partial class image_internal_test_package {

public static void TestRectangle(ж<testing.T> t) {
    // in checks that every point in f is in g.
    error @in(global::go.image_package.Rectangle f, global::go.image_package.Rectangle g) {
        if (!f.In(g)) {
            return fmt.Errorf("f=%s, f.In(%s): got false, want true"u8, f, g);
        }
        for (nint y = f.Min.Y; y < f.Max.Y; y++) {
            for (nint x = f.Min.X; x < f.Max.X; x++) {
                var p = new Point(x, y);
                if (!p.In(g)) {
                    return fmt.Errorf("p=%s, p.In(%s): got false, want true"u8, p, g);
                }
            }
        }
        return default;
    }

    var rects = slice<global::go.image_package.Rectangle>([
        Rect(0, 0, 10, 10),
        Rect(10, 0, 20, 10),
        Rect(1, 2, 3, 4),
        Rect(4, 6, 10, 10),
        Rect(2, 3, 12, 5),
        Rect(-1, -2, 0, 0),
        Rect(-1, -2, 4, 6),
        Rect(-10, -20, 30, 40),
        Rect(8, 8, 8, 8),
        Rect(88, 88, 88, 88),
        Rect(6, 5, 4, 3)
    ]);

    // r.Eq(s) should be equivalent to every point in r being in s, and every
    // point in s being in r.
    foreach (var (_, r) in rects) {
        foreach (var (_, s) in rects) {
            var got = r.Eq(s);
            var want = @in(r, s) == nil && @in(s, r) == nil;
            if (got != want) {
                t.Errorf("Eq: r=%s, s=%s: got %t, want %t", r, s, got, want);
            }
        }
    }

    // The intersection should be the largest rectangle a such that every point
    // in a is both in r and in s.
    foreach (var (_, r) in rects) {
        foreach (var (_, s) in rects) {
            var a = r.Intersect(s);
            {
                var err = @in(a, r); if (err != nil) {
                    t.Errorf("Intersect: r=%s, s=%s, a=%s, a not in r: %v", r, s, a, err);
                }
            }
            {
                var err = @in(a, s); if (err != nil) {
                    t.Errorf("Intersect: r=%s, s=%s, a=%s, a not in s: %v", r, s, a, err);
                }
            }
            {
                var (isZero, overlaps) = (a == (new Rectangle()), r.Overlaps(s)); if (isZero == overlaps) {
                    t.Errorf("Intersect: r=%s, s=%s, a=%s: isZero=%t same as overlaps=%t",
                        r, s, a, isZero, overlaps);
                }
            }
            var largerThanA = new global::go.image_package.Rectangle[]{a, a, a, a}.array();
            largerThanA[0].Min.X--;
            largerThanA[1].Min.Y--;
            largerThanA[2].Max.X++;
            largerThanA[3].Max.Y++;
            foreach (var (i, b) in largerThanA.ΔRangeSnapshot()) {
                if (b.Empty()) {
                    // b isn't actually larger than a.
                    continue;
                }
                if (@in(b, r) == nil && @in(b, s) == nil) {
                    t.Errorf("Intersect: r=%s, s=%s, a=%s, b=%s, i=%d: intersection could be larger",
                        r, s, a, b, i);
                }
            }
        }
    }

    // The union should be the smallest rectangle a such that every point in r
    // is in a and every point in s is in a.
    foreach (var (_, r) in rects) {
        foreach (var (_, s) in rects) {
            var a = r.Union(s);
            {
                var err = @in(r, a); if (err != nil) {
                    t.Errorf("Union: r=%s, s=%s, a=%s, r not in a: %v", r, s, a, err);
                }
            }
            {
                var err = @in(s, a); if (err != nil) {
                    t.Errorf("Union: r=%s, s=%s, a=%s, s not in a: %v", r, s, a, err);
                }
            }
            if (a.Empty()) {
                // You can't get any smaller than a.
                continue;
            }
            var smallerThanA = new global::go.image_package.Rectangle[]{a, a, a, a}.array();
            smallerThanA[0].Min.X++;
            smallerThanA[1].Min.Y++;
            smallerThanA[2].Max.X--;
            smallerThanA[3].Max.Y--;
            foreach (var (i, b) in smallerThanA.ΔRangeSnapshot()) {
                if (@in(r, b) == nil && @in(s, b) == nil) {
                    t.Errorf("Union: r=%s, s=%s, a=%s, b=%s, i=%d: union could be smaller",
                        r, s, a, b, i);
                }
            }
        }
    }
}

} // end image_internal_test_package
