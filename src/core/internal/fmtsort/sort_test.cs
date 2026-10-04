// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using cmp = cmp_package;
using fmt = fmt_package;
using fmtsort = go.@internal.fmtsort_package;
using math = math_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go.@internal;
using io = io_package;
using static go.@internal.fmtsort_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class fmtsort_test_package {

internal static slice<slice<reflectꓸValue>> compareTests;
internal static void initᴛcompareTests() { compareTests = new slice<reflectꓸValue>[]{
    ct(reflect.TypeOf((nint)0), (nint)(-1), (nint)(0), (nint)(1)),
    ct(reflect.TypeOf((int8)0), (nint)(-1), (nint)(0), (nint)(1)),
    ct(reflect.TypeOf((int16)0), (nint)(-1), (nint)(0), (nint)(1)),
    ct(reflect.TypeOf((int32)0), (nint)(-1), (nint)(0), (nint)(1)),
    ct(reflect.TypeOf((int64)0), (nint)(-1), (nint)(0), (nint)(1)),
    ct(reflect.TypeOf((nuint)0), (nint)(0), (nint)(1), (nint)(5)),
    ct(reflect.TypeOf((uint8)0), (nint)(0), (nint)(1), (nint)(5)),
    ct(reflect.TypeOf((uint16)0), (nint)(0), (nint)(1), (nint)(5)),
    ct(reflect.TypeOf((uint32)0), (nint)(0), (nint)(1), (nint)(5)),
    ct(reflect.TypeOf((uint64)0), (nint)(0), (nint)(1), (nint)(5)),
    ct(reflect.TypeOf((uintptr)0), (nint)(0), (nint)(1), (nint)(5)),
    ct(reflect.TypeOf(((@string)""u8)), (@string)""u8, (@string)"a"u8, (@string)"ab"u8),
    ct(reflect.TypeOf((float32)0F), math.NaN(), math.Inf(-1), -1e10D, (nint)(0), 1e10D, math.Inf(1)),
    ct(reflect.TypeOf((float64)0D), math.NaN(), math.Inf(-1), -1e10D, (nint)(0), 1e10D, math.Inf(1)),
    ct(reflect.TypeOf((complex64)(1F.i())), -1D + -1D.i(), -1D + 0D.i(), -1D + 1D.i(), -1D.i(), 0D.i(), 1D.i(), 1D + -1D.i(), 1D + 0D.i(), 1D + 1D.i()),
    ct(reflect.TypeOf((complex128)(1D.i())), -1D + -1D.i(), -1D + 0D.i(), -1D + 1D.i(), -1D.i(), 0D.i(), 1D.i(), 1D + -1D.i(), 1D + 0D.i(), 1D + 1D.i()),
    ct(reflect.TypeOf(false), false, true),
    ct(reflect.TypeOf(Ꮡints.at<nint>(0)), Ꮡints.at<nint>(0), Ꮡints.at<nint>(1), Ꮡints.at<nint>(2)),
    ct(reflect.TypeOf(@unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(0))), @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(0)), @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(1)), @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(2))),
    ct(reflect.TypeOf(chans[0]), chans[0], chans[1], chans[2]),
    ct(reflect.TypeOf(new toy(nil)), new toy(0, 1), new toy(0, 2), new toy(1, -1), new toy(1, 1)),
    ct(reflect.TypeOf(new nint[]{}.array(2)), new nint[]{1, 1}.array(), new nint[]{1, 2}.array(), new nint[]{2, 0}.array()),
    ct(reflect.TypeOf(((any)(nint)(0))), iFace, (nint)(1), (nint)(2), (nint)(3))
}.slice(); }

internal static any iFace;

internal static slice<reflectꓸValue> ct(reflectꓸType typ, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var value = new slice<reflectꓸValue>(len(args), () => new(nil));
    foreach (var (i, v) in args) {
        var x = reflect.ValueOf(v);
        if (!x.IsValid()){
            // Make it a typed nil.
            x = reflect.Zero(typ);
        } else {
            x = x.Convert(typ);
        }
        value[i] = x;
    }
    return value;
}

public static void TestCompare(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in compareTests) {
        foreach (var (i, v0) in test) {
            foreach (var (j, v1) in test) {
                nint c = fmtsort_internal_test_package.Compare(v0, v1);
                nint expect = default!;
                switch (ᐧ) {
                case {} when i == j: {
                    expect = 0;
                    break;
                }
                case {} when i < j: {
                    expect = -1;
                    break;
                }
                case {} when i > j: {
                    expect = 1;
                    break;
                }}

                if (c != expect) {
                    Ꮡt.Errorf("%s: compare(%v,%v)=%d; expect %d"u8, v0.Type(), v0, v1, c, expect);
                }
            }
        }
    }
}

[GoType] partial struct sortTest {
    internal any data;    // Always a map.
    internal @string print; // Printed result using our custom printer.
}

internal static slice<sortTest> sortTests;
internal static void initᴛsortTests() { sortTests = new sortTest[]{
    new(
        new map<nint, @string>{[7] = "bar"u8, [-3] = "foo"u8},
        "-3:foo 7:bar"u8
    ),
    new(
        new map<uint8, @string>{[7] = "bar"u8, [3] = "foo"u8},
        "3:foo 7:bar"u8
    ),
    new(
        new map<@string, @string>{["7"u8] = "bar"u8, ["3"u8] = "foo"u8},
        "3:foo 7:bar"u8
    ),
    new(
        new map<float64, @string>{[7D] = "bar"u8, [-3D] = "foo"u8, [math.NaN()] = "nan"u8, [math.Inf(0)] = "inf"u8},
        "NaN:nan -3:foo 7:bar +Inf:inf"u8
    ),
    new(
        new map<complex128, @string>{[7D + 2D.i()] = "bar2"u8, [7D + 1D.i()] = "bar"u8, [-3D] = "foo"u8, [complex(math.NaN(), 0D)] = "nan"u8, [complex(math.Inf(0), 0D)] = "inf"u8},
        "(NaN+0i):nan (-3+0i):foo (7+1i):bar (7+2i):bar2 (+Inf+0i):inf"u8
    ),
    new(
        new map<bool, @string>{[true] = "true"u8, [false] = "false"u8},
        "false:false true:true"u8
    ),
    new(
        chanMap(),
        "CHAN0:0 CHAN1:1 CHAN2:2"u8
    ),
    new(
        pointerMap(),
        "PTR0:0 PTR1:1 PTR2:2"u8
    ),
    new(
        unsafePointerMap(),
        "UNSAFEPTR0:0 UNSAFEPTR1:1 UNSAFEPTR2:2"u8
    ),
    new(
        new map<toy, @string>{[new(7, 2)] = "72"u8, [new(7, 1)] = "71"u8, [new(3, 4)] = "34"u8},
        "{3 4}:34 {7 1}:71 {7 2}:72"u8
    ),
    new(
        new map<array<nint>, @string>{[new nint[]{7, 2}.array()] = "72"u8, [new nint[]{7, 1}.array()] = "71"u8, [new nint[]{3, 4}.array()] = "34"u8},
        "[3 4]:34 [7 1]:71 [7 2]:72"u8
    )
}.slice(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "nil"u8;

internal static @string sprint(any data) {
    var om = fmtsort.Sort(reflect.ValueOf(data));
    if (om == default!) {
        return nilˢ;
    }
    var b = @new<strings.Builder>();
    foreach (var (i, m) in om) {
        if (i > 0) {
            b.WriteRune((rune)' ');
        }
        b.WriteString(sprintKey(m.Key));
        b.WriteRune((rune)':');
        fmt.Fprint(new fmtsort_test_package.strings_BuilderжWriter(b), m.Value);
    }
    return b.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ptrˢ = "PTR???"u8;
internal static readonly @string unsafeptrˢ = "UNSAFEPTR???"u8;
internal static readonly @string chanˢ = "CHAN???"u8;

// sprintKey formats a reflect.Value but gives reproducible values for some
// problematic types such as pointers. Note that it only does special handling
// for the troublesome types used in the test cases; it is not a general
// printer.
internal static @string sprintKey(reflectꓸValue key) {
    {
        @string str = key.Type().String();
        var exprᴛ1 = str;
        if (exprᴛ1 == "*int"u8) {
            var ptr = key.Interface()._<ж<nint>>();
            foreach (var (i, _) in ints) {
                if (ptr == Ꮡints.at<nint>(i)) {
                    return fmt.Sprintf("PTR%d"u8, i);
                }
            }
            return ptrˢ;
        }
        if (exprᴛ1 == "unsafe.Pointer"u8) {
            @unsafe.Pointer ptr = key.Interface()._<@unsafe.Pointer>();
            foreach (var (i, _) in ints) {
                if (ptr == @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(i))) {
                    return fmt.Sprintf("UNSAFEPTR%d"u8, i);
                }
            }
            return unsafeptrˢ;
        }
        if (exprᴛ1 == "chan int"u8) {
            var c = key.Interface()._<channel<nint>>();
            foreach (var (i, _) in chans) {
                if (c == chans[i]) {
                    return fmt.Sprintf("CHAN%d"u8, i);
                }
            }
            return chanˢ;
        }
        { /* default: */
            return fmt.Sprint(key);
        }
    }

}

internal static ж<array<nint>> Ꮡints = new StandardBox<array<nint>>(new array<nint>(3));
internal static ref array<nint> ints => ref Ꮡints.Value;
internal static slice<channel<nint>> chans;
internal static void initᴛchans() { chans = makeChans(); }
internal static ж<Δruntime.Pinner> Ꮡpin = new StandardBox<Δruntime.Pinner>(new Δruntime.Pinner(nil));
internal static ref Δruntime.Pinner pin => ref Ꮡpin.Value;

internal static slice<channel<nint>> makeChans() {
    var cs = new channel<nint>[]{new channel<nint>(0), new channel<nint>(0), new channel<nint>(0)}.slice();
    // Order channels by address. See issue #49431.
    foreach (var (i, _) in cs) {
        pin.Pin(@unsafe.Pointer.OrTypedNil((uintptr)reflect.ValueOf(cs[i]).UnsafePointer()));
    }
    slices.SortFunc(cs, (channel<nint> a, channel<nint> b) => cmp.Compare(reflect.ValueOf(a).Pointer(), reflect.ValueOf(b).Pointer()));
    return cs;
}

internal static map<ж<nint>, @string> pointerMap() {
    var m = new map<ж<nint>, @string>();
    for (nint i = 2; i >= 0; i--) {
        m[Ꮡints.at<nint>(i)] = fmt.Sprint(i);
    }
    return m;
}

internal static map<@unsafe.Pointer, @string> unsafePointerMap() {
    var m = new map<@unsafe.Pointer, @string>();
    for (nint i = 2; i >= 0; i--) {
        m[@unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(i))] = fmt.Sprint(i);
    }
    return m;
}

internal static map<channel<nint>, @string> chanMap() {
    var m = new map<channel<nint>, @string>();
    for (nint i = 2; i >= 0; i--) {
        m[chans[i]] = fmt.Sprint(i);
    }
    return m;
}

[GoType] partial struct toy {
    public nint A; // Exported.
    internal nint b; // Unexported.
}

public static void TestOrder(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in sortTests) {
        @string got = sprint(test.data);
        if (got != test.print) {
            Ꮡt.Errorf("%s: got %q, want %q"u8, reflect.TypeOf(test.data), got, test.print);
        }
    }
}

[GoType("dyn")] internal partial struct TestInterface_type {
    internal nint x, y;
}

public static void TestInterface(ж<testing.T> Ꮡt) {
    // A map containing multiple concrete types should be sorted by type,
    // then value. However, the relative ordering of types is unspecified,
    // so test this by checking the presence of sorted subgroups.
    var m = new map<any, @string>{
        [new nint[]{1, 0}.array()] = ""u8,
        [new nint[]{0, 1}.array()] = ""u8,
        [true] = ""u8,
        [false] = ""u8,
        [3.1D] = ""u8,
        [2.1D] = ""u8,
        [1.1D] = ""u8,
        [math.NaN()] = ""u8,
        [(nint)(3)] = ""u8,
        [(nint)(2)] = ""u8,
        [(nint)(1)] = ""u8,
        [(@string)"c"u8] = ""u8,
        [(@string)"b"u8] = ""u8,
        [(@string)"a"u8] = ""u8,
        [new TestInterface_type(1, 0)] = ""u8,
        [new TestInterface_type(0, 1)] = ""u8
    };
    @string got = sprint(m);
    var typeGroups = new @string[]{
        "NaN: 1.1: 2.1: 3.1:"u8, // float64

        "false: true:"u8, // bool

        "1: 2: 3:"u8, // int

        "a: b: c:"u8, // string

        "[0 1]: [1 0]:"u8, // [2]int

        "{0 1}: {1 0}:"u8
    }.slice();
    // struct{ x int; y int }
    foreach (var (_, g) in typeGroups) {
        if (!strings.Contains(got, g)) {
            Ꮡt.Errorf("sorted map should contain %q"u8, g);
        }
    }
}

} // end fmtsort_test_package
