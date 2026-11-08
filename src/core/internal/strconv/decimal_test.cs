// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.strconv_package;
using testing = testing_package;
using go.@internal;
using static go.@internal.strconv_internal_test_package;
using strconv = go.@internal.strconv_package;

partial class strconv_test_package {

partial struct shiftTest {
    internal uint64 i;
    internal nint shift;
    internal @string @out;
}

internal static readonly ж<slice<shiftTest>> Ꮡshifttests = new StaticBox<slice<shiftTest>>([
    new(0, -100, "0"),
    new(0, 100, "0"),
    new(1, 100, "1267650600228229401496703205376"),
    new(1, -100,
        "0.00000000000000000000000000000078886090522101180541"u8 + "17285652827862296732064351090230047702789306640625"u8
    ),
    new(12345678, 8, "3160493568"),
    new(12345678, -8, "48225.3046875"),
    new(195312, 9, "99999744"),
    new(1953125, 9, "1000000000")
]);
internal static ref slice<shiftTest> shifttests => ref ((StaticBox<slice<shiftTest>>)Ꮡshifttests).ValueSlot;

public static void TestDecimalShift(ж<testing.T> t) {
    for (nint i = 0; i < len(shifttests); i++) {
        var test = Ꮡ(shifttests, i);
        var d = strconv_internal_test_package.NewDecimal(test.i);
        d.Shift(test.shift);
        @string s = d.String();
        if (s != test.@out) {
            t.Errorf("Decimal %v << %v = %v, want %v",
                test.i, test.shift, s, test.@out);
        }
    }
}

partial struct roundTest {
    internal uint64 i;
    internal nint nd;
    internal @string down, round, up;
    internal uint64 @int;
}

internal static readonly ж<slice<roundTest>> Ꮡroundtests = new StaticBox<slice<roundTest>>([
    new(0, 4, "0", "0", "0", 0),
    new(12344999, 4, "12340000", "12340000", "12350000", 12340000),
    new(12345000, 4, "12340000", "12340000", "12350000", 12340000),
    new(12345001, 4, "12340000", "12350000", "12350000", 12350000),
    new(23454999, 4, "23450000", "23450000", "23460000", 23450000),
    new(23455000, 4, "23450000", "23460000", "23460000", 23460000),
    new(23455001, 4, "23450000", "23460000", "23460000", 23460000),

    new(99994999, 4, "99990000", "99990000", "100000000", 99990000),
    new(99995000, 4, "99990000", "100000000", "100000000", 100000000),
    new(99999999, 4, "99990000", "100000000", "100000000", 100000000),

    new(12994999, 4, "12990000", "12990000", "13000000", 12990000),
    new(12995000, 4, "12990000", "13000000", "13000000", 13000000),
    new(12999999, 4, "12990000", "13000000", "13000000", 13000000)
]);
internal static ref slice<roundTest> roundtests => ref ((StaticBox<slice<roundTest>>)Ꮡroundtests).ValueSlot;

public static void TestDecimalRound(ж<testing.T> t) {
    for (nint i = 0; i < len(roundtests); i++) {
        var test = Ꮡ(roundtests, i);
        var d = strconv_internal_test_package.NewDecimal(test.i);
        d.RoundDown(test.nd);
        @string s = d.String();
        if (s != test.down) {
            t.Errorf("Decimal %v RoundDown %d = %v, want %v",
                test.i, test.nd, s, test.down);
        }
        d = strconv_internal_test_package.NewDecimal(test.i);
        d.Round(test.nd);
        s = d.String();
        if (s != test.round) {
            t.Errorf("Decimal %v Round %d = %v, want %v",
                test.i, test.nd, s, test.down);
        }
        d = strconv_internal_test_package.NewDecimal(test.i);
        d.RoundUp(test.nd);
        s = d.String();
        if (s != test.up) {
            t.Errorf("Decimal %v RoundUp %d = %v, want %v",
                test.i, test.nd, s, test.up);
        }
    }
}

partial struct roundIntTest {
    internal uint64 i;
    internal nint shift;
    internal uint64 @int;
}

internal static slice<roundIntTest> roundinttests = [
    new(0, 100, 0),
    new(512, -8, 2),
    new(513, -8, 2),
    new(640, -8, 2),
    new(641, -8, 3),
    new(384, -8, 2),
    new(385, -8, 2),
    new(383, -8, 1),
    new(1, 100, 18446744073709551615UL),
    new(1000, 0, 1000)
];

public static void TestDecimalRoundedInteger(ж<testing.T> t) {
    for (nint i = 0; i < len(roundinttests); i++) {
        var test = roundinttests[i];
        var d = strconv_internal_test_package.NewDecimal(test.i);
        d.Shift(test.shift);
        var @int = d.RoundedInteger();
        if (@int != test.@int) {
            t.Errorf("Decimal %v >> %v RoundedInteger = %v, want %v",
                test.i, test.shift, @int, test.@int);
        }
    }
}

} // end strconv_test_package
