// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using image = image_package;
using testing = testing_package;
using static go.image.draw_package;

partial class draw_internal_test_package {

internal partial struct clipTest {
    internal @string desc;
    internal image.Rectangle r, dr, sr, mr;
    internal image.Point sp, mp;
    internal bool nilMask;
    internal image.Rectangle r0;
    internal image.Point sp0, mp0;
}

// The following tests all have a nil mask.
// The following tests all have a non-nil mask.
internal static slice<clipTest> clipTests = new clipTest[]{
    new(
        "basic"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 100, 100),
        new image.Rectangle(nil),
        new image.Point(nil),
        new image.Point(nil),
        true,
        image.Rect(0, 0, 100, 100),
        new image.Point(nil),
        new image.Point(nil)
    ),
    new(
        "clip dr"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(40, 40, 60, 60),
        image.Rect(0, 0, 100, 100),
        new image.Rectangle(nil),
        new image.Point(nil),
        new image.Point(nil),
        true,
        image.Rect(40, 40, 60, 60),
        image.Pt(40, 40),
        new image.Point(nil)
    ),
    new(
        "clip sr"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 100, 100),
        image.Rect(20, 20, 80, 80),
        new image.Rectangle(nil),
        new image.Point(nil),
        new image.Point(nil),
        true,
        image.Rect(20, 20, 80, 80),
        image.Pt(20, 20),
        new image.Point(nil)
    ),
    new(
        "clip dr and sr"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 50, 100),
        image.Rect(20, 20, 80, 80),
        new image.Rectangle(nil),
        new image.Point(nil),
        new image.Point(nil),
        true,
        image.Rect(20, 20, 50, 80),
        image.Pt(20, 20),
        new image.Point(nil)
    ),
    new(
        "clip dr and sr, sp outside sr (top-left)"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 50, 100),
        image.Rect(20, 20, 80, 80),
        new image.Rectangle(nil),
        image.Pt(15, 8),
        new image.Point(nil),
        true,
        image.Rect(5, 12, 50, 72),
        image.Pt(20, 20),
        new image.Point(nil)
    ),
    new(
        "clip dr and sr, sp outside sr (middle-left)"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 50, 100),
        image.Rect(20, 20, 80, 80),
        new image.Rectangle(nil),
        image.Pt(15, 66),
        new image.Point(nil),
        true,
        image.Rect(5, 0, 50, 14),
        image.Pt(20, 66),
        new image.Point(nil)
    ),
    new(
        "clip dr and sr, sp outside sr (bottom-left)"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 50, 100),
        image.Rect(20, 20, 80, 80),
        new image.Rectangle(nil),
        image.Pt(15, 91),
        new image.Point(nil),
        true,
        new image.Rectangle(nil),
        image.Pt(15, 91),
        new image.Point(nil)
    ),
    new(
        "clip dr and sr, sp inside sr"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 50, 100),
        image.Rect(20, 20, 80, 80),
        new image.Rectangle(nil),
        image.Pt(44, 33),
        new image.Point(nil),
        true,
        image.Rect(0, 0, 36, 47),
        image.Pt(44, 33),
        new image.Point(nil)
    ),
    new(
        "basic mask"u8,
        image.Rect(0, 0, 80, 80),
        image.Rect(20, 0, 100, 80),
        image.Rect(0, 0, 50, 49),
        image.Rect(0, 0, 46, 47),
        new image.Point(nil),
        new image.Point(nil),
        false,
        image.Rect(20, 0, 46, 47),
        image.Pt(20, 0),
        image.Pt(20, 0)
    ),
    new(
        "clip sr and mr"u8,
        image.Rect(0, 0, 100, 100),
        image.Rect(0, 0, 100, 100),
        image.Rect(23, 23, 55, 86),
        image.Rect(44, 44, 87, 58),
        image.Pt(10, 10),
        image.Pt(11, 11),
        false,
        image.Rect(33, 33, 45, 47),
        image.Pt(43, 43),
        image.Pt(44, 44)
    )
}.slice();

public static void TestClip(ж<testing.T> Ꮡt) {
    var dst0 = image.NewRGBA(image.Rect(0, 0, 100, 100));
    var src0 = image.NewRGBA(image.Rect(0, 0, 100, 100));
    var mask0 = image.NewRGBA(image.Rect(0, 0, 100, 100));
    foreach (var (_, c) in clipTests) {
        var dst = dst0.SubImage(c.dr)._<ж<imageꓸRGBA>>();
        var src = src0.SubImage(c.sr)._<ж<imageꓸRGBA>>();
        ref var r = ref heap<image.Rectangle>(out var Ꮡr);
        r = c.r;
        ref var sp = ref heap<image.Point>(out var Ꮡsp);
        sp = c.sp;
        ref var mp = ref heap<image.Point>(out var Ꮡmp);
        mp = c.mp;
        if (c.nilMask){
            clip(new draw_test_package.image_ΔRGBAжImage(dst), Ꮡr, new image.ΔRGBAжImage(src), ref sp, default!, nil);
        } else {
            clip(new draw_test_package.image_ΔRGBAжImage(dst), Ꮡr, new image.ΔRGBAжImage(src), ref sp, mask0.SubImage(c.mr), Ꮡmp);
        }
        // Check that the actual results equal the expected results.
        if (!c.r0.Eq(r)) {
            Ꮡt.Errorf("%s: clip rectangle want %v got %v"u8, c.desc, c.r0, r);
            continue;
        }
        if (!c.sp0.Eq(sp)) {
            Ꮡt.Errorf("%s: sp want %v got %v"u8, c.desc, c.sp0, sp);
            continue;
        }
        if (!c.nilMask) {
            if (!c.mp0.Eq(mp)) {
                Ꮡt.Errorf("%s: mp want %v got %v"u8, c.desc, c.mp0, mp);
                continue;
            }
        }
        // Check that the clipped rectangle is contained by the dst / src / mask
        // rectangles, in their respective coordinate spaces.
        if (!r.In(c.dr)) {
            Ꮡt.Errorf("%s: c.dr %v does not contain r %v"u8, c.desc, c.dr, r);
        }
        // sr is r translated into src's coordinate space.
        var sr = r.Add(c.sp.Sub(c.dr.Min));
        if (!sr.In(c.sr)) {
            Ꮡt.Errorf("%s: c.sr %v does not contain sr %v"u8, c.desc, c.sr, sr);
        }
        if (!c.nilMask) {
            // mr is r translated into mask's coordinate space.
            var mr = r.Add(c.mp.Sub(c.dr.Min));
            if (!mr.In(c.mr)) {
                Ꮡt.Errorf("%s: c.mr %v does not contain mr %v"u8, c.desc, c.mr, mr);
            }
        }
    }
}

} // end draw_internal_test_package
