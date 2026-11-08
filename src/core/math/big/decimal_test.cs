// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using testing = testing_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal partial struct TestDecimalString_type /*dyn*/ {
    internal global::go.math.big_package.@decimal x;
    internal @string want;
}

public static void TestDecimalString(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestDecimalString_type>([
        new(want: "0"),
        new(new @decimal(default, 1000), "0"), // exponent of 0 is ignored
        new(new @decimal(slice<byte>("12345"u8), 0), "0.12345"),
        new(new @decimal(slice<byte>("12345"u8), -3), "0.00012345"),
        new(new @decimal(slice<byte>("12345"u8), +3), "123.45"),
        new(new @decimal(slice<byte>("12345"u8), +10), "1234500000")
    ]))) {
        var test = vᴛ1;

        @string got = test.x.String(); if (got != test.want) {
            t.Errorf("%v == %s; want %s", test.x, got, test.want);
        }
    }
}

internal partial struct TestDecimalInit_type /*dyn*/ {
    internal global::go.math.big_package.Word x;
    internal nint shift;
    internal @string want;
}

public static void TestDecimalInit(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestDecimalInit_type>([
        new(0, 0, "0"),
        new(0, -100, "0"),
        new(0, 100, "0"),
        new(1, 0, "1"),
        new(1, 10, "1024"),
        new(1, 100, "1267650600228229401496703205376"),
        new(1, -100, "0.0000000000000000000000000000007888609052210118054117285652827862296732064351090230047702789306640625"),
        new(12345678, 8, "3160493568"),
        new(12345678, -8, "48225.3046875"),
        new(195312, 9, "99999744"),
        new(1953125, 9, "1000000000")
    ]))) {
        ref var d = ref heap(new global::go.math.big_package.@decimal(), out var Ꮡd);
        Ꮡd.init(new Δnat(new global::go.math.big_package.Word[]{test.x}.slice()).norm(), test.shift);
        @string got = d.String(); if (got != test.want) {
            t.Errorf("%d << %d == %s; want %s", test.x, test.shift, got, test.want);
        }
    }
}

internal partial struct TestDecimalRounding_type /*dyn*/ {
    internal uint64 x;
    internal nint n;
    internal @string down, even, up;
}

public static void TestDecimalRounding(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestDecimalRounding_type>([
        new(0, 0, "0", "0", "0"),
        new(0, 1, "0", "0", "0"),

        new(1, 0, "0", "0", "10"),
        new(5, 0, "0", "0", "10"),
        new(9, 0, "0", "10", "10"),

        new(15, 1, "10", "20", "20"),
        new(45, 1, "40", "40", "50"),
        new(95, 1, "90", "100", "100"),

        new(12344999, 4, "12340000", "12340000", "12350000"),
        new(12345000, 4, "12340000", "12340000", "12350000"),
        new(12345001, 4, "12340000", "12350000", "12350000"),
        new(23454999, 4, "23450000", "23450000", "23460000"),
        new(23455000, 4, "23450000", "23460000", "23460000"),
        new(23455001, 4, "23450000", "23460000", "23460000"),

        new(99994999, 4, "99990000", "99990000", "100000000"),
        new(99995000, 4, "99990000", "100000000", "100000000"),
        new(99999999, 4, "99990000", "100000000", "100000000"),

        new(12994999, 4, "12990000", "12990000", "13000000"),
        new(12995000, 4, "12990000", "13000000", "13000000"),
        new(12999999, 4, "12990000", "13000000", "13000000")
    ]))) {
        var x = ((global::go.math.big_package.Δnat)default).setUint64(test.x);

        ref var d = ref heap(new global::go.math.big_package.@decimal(), out var Ꮡd);
        Ꮡd.init(x, 0);
        Ꮡd.roundDown(test.n);
        {
            @string got = d.String(); if (got != test.down) {
                t.Errorf("roundDown(%d, %d) = %s; want %s", test.x, test.n, got, test.down);
            }
        }

        Ꮡd.init(x, 0);
        Ꮡd.round(test.n);
        {
            @string got = d.String(); if (got != test.even) {
                t.Errorf("round(%d, %d) = %s; want %s", test.x, test.n, got, test.even);
            }
        }

        Ꮡd.init(x, 0);
        d.roundUp(test.n);
        {
            @string got = d.String(); if (got != test.up) {
                t.Errorf("roundUp(%d, %d) = %s; want %s", test.x, test.n, got, test.up);
            }
        }
    }
}

internal static @string sink;

public static void BenchmarkDecimalConversion(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        for (nint shift = -100; shift <= +100; shift++) {
            ref var d = ref heap(new global::go.math.big_package.@decimal(), out var Ꮡd);
            Ꮡd.init(natOne, shift);
            sink = d.String();
        }
    }
}

public static void BenchmarkFloatString(ж<testing.B> b) {
    var x = @new<global::go.math.big_package.Float>();
    foreach (var (_, prec) in range(slice<nuint>([100, 1000, 10000, 100000]))) {
        x.SetPrec(prec).SetRat(NewRat(1, 3));
        var xʗ1 = x;
        b.Run(fmt.Sprintf("%v"u8, prec), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sink = xʗ1.String();
            }
        });
    }
}

} // end big_internal_test_package
