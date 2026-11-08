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
internal static void initᴛcompareTests() { compareTests = slice<slice<reflectꓸValue>>([
    ct(reflect.TypeOf((nint)0), (nint)(-1), (nint)0, (nint)1),
    ct(reflect.TypeOf((int8)0), (nint)(-1), (nint)0, (nint)1),
    ct(reflect.TypeOf((int16)0), (nint)(-1), (nint)0, (nint)1),
    ct(reflect.TypeOf((int32)0), (nint)(-1), (nint)0, (nint)1),
    ct(reflect.TypeOf((int64)0), (nint)(-1), (nint)0, (nint)1),
    ct(reflect.TypeOf((nuint)0), (nint)0, (nint)1, (nint)5),
    ct(reflect.TypeOf((uint8)0), (nint)0, (nint)1, (nint)5),
    ct(reflect.TypeOf((uint16)0), (nint)0, (nint)1, (nint)5),
    ct(reflect.TypeOf((uint32)0), (nint)0, (nint)1, (nint)5),
    ct(reflect.TypeOf((uint64)0), (nint)0, (nint)1, (nint)5),
    ct(reflect.TypeOf((uintptr)0), (nint)0, (nint)1, (nint)5),
    ct(reflect.TypeOf((@string)""u8), "", "a", "ab"),
    ct(reflect.TypeOf((float32)0F), math.NaN(), math.Inf(-1), -1e10D, (nint)0, 1e10D, math.Inf(1)),
    ct(reflect.TypeOf((float64)0D), math.NaN(), math.Inf(-1), -1e10D, (nint)0, 1e10D, math.Inf(1)),
    ct(reflect.TypeOf((complex64)(1F.i())), -1D + -1D.i(), -1D + 0D.i(), -1D + 1D.i(), -1D.i(), 0D.i(), 1D.i(), 1D + -1D.i(), 1D + 0D.i(), 1D + 1D.i()),
    ct(reflect.TypeOf((complex128)(1D.i())), -1D + -1D.i(), -1D + 0D.i(), -1D + 1D.i(), -1D.i(), 0D.i(), 1D.i(), 1D + -1D.i(), 1D + 0D.i(), 1D + 1D.i()),
    ct(reflect.TypeOf(false), false, true),
    ct(reflect.TypeOf(Ꮡints.at<nint>(0)), Ꮡints.at<nint>(0), Ꮡints.at<nint>(1), Ꮡints.at<nint>(2)),
    ct(reflect.TypeOf(@unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(0))), @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(0)), @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(1)), @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(2))),
    ct(reflect.TypeOf(chans[0]), chans[0], chans[1], chans[2]),
    ct(reflect.TypeOf(new toy()), new toy(0, 1), new toy(0, 2), new toy(1, -1), new toy(1, 1)),
    ct(reflect.TypeOf(new nint[]{}.array(2)), new nint[]{1, 1}.array(), new nint[]{1, 2}.array(), new nint[]{2, 0}.array()),
    ct(reflect.TypeOf((any)(nint)0), iFace, (nint)1, (nint)2, (nint)3)
]); }

internal static any iFace;

internal static slice<reflectꓸValue> ct(reflectꓸType typ, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var value = new slice<reflectꓸValue>(len(args), () => new(nil));
    foreach (var (i, v) in range(args)) {
        var x = reflect.ValueOf(v);
        if (!x.IsValid()) { // Make it a typed nil.
            x = reflect.Zero(typ);
        } else {
            x = x.Convert(typ);
        }
        value[i] = x;
    }
    return value;
}

public static void TestCompare(ж<testing.T> t) {
    foreach (var (_, test) in range(compareTests)) {
        foreach (var (i, v0) in range(test)) {
            foreach (var (j, v1) in range(test)) {
                nint c = fmtsort_internal_test_package.Compare(v0, v1);
                nint expect = default;
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
                    t.Errorf("%s: compare(%v,%v)=%d; expect %d", v0.Type(), v0, v1, c, expect);
                }
            }
        }
    }
}

partial struct sortTest {
    internal any data;      // Always a map.
    internal @string print; // Printed result using our custom printer.
}

internal static slice<sortTest> sortTests;
internal static void initᴛsortTests() { sortTests = slice<sortTest>([
    new(
        new map<nint, @string>{[7] = "bar", [-3] = "foo"},
        "-3:foo 7:bar"
    ),
    new(
        new map<uint8, @string>{[7] = "bar", [3] = "foo"},
        "3:foo 7:bar"
    ),
    new(
        new map<@string, @string>{["7"] = "bar", ["3"] = "foo"},
        "3:foo 7:bar"
    ),
    new(
        new map<float64, @string>{[7D] = "bar", [-3D] = "foo", [math.NaN()] = "nan", [math.Inf(0)] = "inf"},
        "NaN:nan -3:foo 7:bar +Inf:inf"
    ),
    new(
        new map<complex128, @string>{[7D + 2D.i()] = "bar2", [7D + 1D.i()] = "bar", [-3D] = "foo", [complex(math.NaN(), 0D)] = "nan", [complex(math.Inf(0), 0D)] = "inf"},
        "(NaN+0i):nan (-3+0i):foo (7+1i):bar (7+2i):bar2 (+Inf+0i):inf"
    ),
    new(
        new map<bool, @string>{[true] = "true", [false] = "false"},
        "false:false true:true"
    ),
    new(
        chanMap(),
        "CHAN0:0 CHAN1:1 CHAN2:2"
    ),
    new(
        pointerMap(),
        "PTR0:0 PTR1:1 PTR2:2"
    ),
    new(
        unsafePointerMap(),
        "UNSAFEPTR0:0 UNSAFEPTR1:1 UNSAFEPTR2:2"
    ),
    new(
        new map<toy, @string>{[new(7, 2)] = "72", [new(7, 1)] = "71", [new(3, 4)] = "34"},
        "{3 4}:34 {7 1}:71 {7 2}:72"
    ),
    new(
        new map<array<nint>, @string>{[new nint[]{7, 2}.array()] = "72", [new nint[]{7, 1}.array()] = "71", [new nint[]{3, 4}.array()] = "34"},
        "[3 4]:34 [7 1]:71 [7 2]:72"
    )
]); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "nil"u8;

internal static @string sprint(any data) {
    var om = fmtsort.Sort(reflect.ValueOf(data));
    if (om == nil) {
        return nilˢ;
    }
    var b = @new<strings.Builder>();
    foreach (var (i, m) in range<fmtsort.KeyValue>(om)) {
        if (i > 0) {
            b.WriteRune(' ');
        }
        b.WriteString(sprintKey(m.Key));
        b.WriteRune(':');
        fmt.Fprint(new fmtsort_test_package.strings_BuilderжWriter(b), m.Value);
    }
    return b.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ptrˢ => ref ptrˢᵇ;
private static readonly @string ptrˢᵇ = "PTR???"u8;
internal static ref readonly @string unsafeptrˢ => ref unsafeptrˢᵇ;
private static readonly @string unsafeptrˢᵇ = "UNSAFEPTR???"u8;
internal static ref readonly @string chanˢ => ref chanˢᵇ;
private static readonly @string chanˢᵇ = "CHAN???"u8;

// sprintKey formats a reflect.Value but gives reproducible values for some
// problematic types such as pointers. Note that it only does special handling
// for the troublesome types used in the test cases; it is not a general
// printer.
internal static @string sprintKey(reflectꓸValue key) {
    @string str = key.Type().String();
    var exprᴛ1 = str;
    switch (ᐧ) {
    case {} when exprᴛ1 == "*int"u8: {
        var ptr = key.Interface()._<ж<nint>>();
        foreach (var i in range(len(ints))) {
            if (ptr == Ꮡints.at<nint>(i)) {
                return fmt.Sprintf("PTR%d"u8, i);
            }
        }
        return ptrˢ;
    }
    case {} when exprᴛ1 == "unsafe.Pointer"u8: {
        @unsafe.Pointer ptr = key.Interface()._<@unsafe.Pointer>();
        foreach (var i in range(len(ints))) {
            if (ptr == @unsafe.Pointer.FromPinnedBox(Ꮡints.at<nint>(i))) {
                return fmt.Sprintf("UNSAFEPTR%d"u8, i);
            }
        }
        return unsafeptrˢ;
    }
    case {} when exprᴛ1 == "chan int"u8: {
        var c = key.Interface()._<channel<nint>>();
        foreach (var i in range(len(chans))) {
            if (c == chans[i]) {
                return fmt.Sprintf("CHAN%d"u8, i);
            }
        }
        return chanˢ;
    }
    default: {
        return fmt.Sprint(key);
    }}
}

internal static readonly ж<array<nint>> Ꮡints = new StaticBox<array<nint>>(new array<nint>(3));
internal static ref array<nint> ints => ref ((StaticBox<array<nint>>)Ꮡints).ValueSlot;
internal static slice<channel<nint>> chans;
internal static void initᴛchans() { chans = makeChans(); }
internal static readonly ж<Δruntime.Pinner> Ꮡpin = new StaticBox<Δruntime.Pinner>(new Δruntime.Pinner(nil));
internal static ref Δruntime.Pinner pin => ref ((StaticBox<Δruntime.Pinner>)Ꮡpin).ValueSlot;

internal static slice<channel<nint>> makeChans() {
    var cs = slice<channel<nint>>([new channel<nint>(0), new channel<nint>(0), new channel<nint>(0)]);
    // Order channels by address. See issue #49431.
    foreach (var i in range(len(cs))) {
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

partial struct toy {
    public nint A;   // Exported.
    internal nint b; // Unexported.
}

public static void TestOrder(ж<testing.T> t) {
    foreach (var (_, test) in range(sortTests)) {
        @string got = sprint(test.data);
        if (got != test.print) {
            t.Errorf("%s: got %q, want %q", reflect.TypeOf(test.data), got, test.print);
        }
    }
}

internal partial struct TestInterface_type /*dyn*/ {
    internal nint x, y;
}

public static void TestInterface(ж<testing.T> t) {
    // A map containing multiple concrete types should be sorted by type,
    // then value. However, the relative ordering of types is unspecified,
    // so test this by checking the presence of sorted subgroups.
    var m = new map<any, @string>{
        [new nint[]{1, 0}.array()] = "",
        [new nint[]{0, 1}.array()] = "",
        [true] = "",
        [false] = "",
        [3.1D] = "",
        [2.1D] = "",
        [1.1D] = "",
        [math.NaN()] = "",
        [(nint)3] = "",
        [(nint)2] = "",
        [(nint)1] = "",
        [(@string)"c"] = "",
        [(@string)"b"] = "",
        [(@string)"a"] = "",
        [new TestInterface_type(1, 0)] = "",
        [new TestInterface_type(0, 1)] = ""
    };
    @string got = sprint(m);
    var typeGroups = slice<@string>([
        "NaN: 1.1: 2.1: 3.1:", // float64
        "false: true:",        // bool
        "1: 2: 3:",            // int
        "a: b: c:",            // string
        "[0 1]: [1 0]:",       // [2]int
        "{0 1}: {1 0}:"        // struct{ x int; y int }
    ]);
    foreach (var (_, g) in range(typeGroups)) {
        if (!strings.Contains(got, g)) {
            t.Errorf("sorted map should contain %q", g);
        }
    }
}

} // end fmtsort_test_package
