// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using reflect = reflect_package;
using testing = testing_package;
using static go.@internal.gover_package;

partial class gover_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string compareˢ => ref compareˢᵇ;
private static readonly @string compareˢᵇ = "Compare"u8;

public static void TestCompare(ж<testing.T> t) { test2<@string, @string, nint>(t, compareTests, compareˢ, Compare); }

internal static slice<testCase2<@string, @string, nint>> compareTests = [
    new("", "", 0),
    new("x", "x", 0),
    new("", "x", 0),
    new("1", "1.1", -1),
    new("1.5", "1.6", -1),
    new("1.5", "1.10", -1),
    new("1.6", "1.6.1", -1),
    new("1.19", "1.19.0", 0),
    new("1.19rc1", "1.19", -1),
    new("1.20", "1.20.0", 0),
    new("1.20rc1", "1.20", -1),
    new("1.21", "1.21.0", -1),
    new("1.21", "1.21rc1", -1),
    new("1.21rc1", "1.21.0", -1),
    new("1.6", "1.19", -1),
    new("1.19", "1.19.1", -1),
    new("1.19rc1", "1.19", -1),
    new("1.19rc1", "1.19.1", -1),
    new("1.19rc1", "1.19rc2", -1),
    new("1.19.0", "1.19.1", -1),
    new("1.19rc1", "1.19.0", -1),
    new("1.19alpha3", "1.19beta2", -1),
    new("1.19beta2", "1.19rc1", -1),
    new("1.1", "1.99999999999999998", -1),
    new("1.99999999999999998", "1.99999999999999999", -1)
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string parseˢ => ref parseˢᵇ;
private static readonly @string parseˢᵇ = "Parse"u8;

public static void TestParse(ж<testing.T> t) { test1<@string, global::go.@internal.gover_package.Version>(t, parseTests, parseˢ, Parse); }

internal static slice<testCase1<@string, global::go.@internal.gover_package.Version>> parseTests = [
    new("1", new Version("1", "0", "0", "", "")),
    new("1.2", new Version("1", "2", "0", "", "")),
    new("1.2.3", new Version("1", "2", "3", "", "")),
    new("1.2rc3", new Version("1", "2", "", "rc", "3")),
    new("1.20", new Version("1", "20", "0", "", "")),
    new("1.21", new Version("1", "21", "", "", "")),
    new("1.21rc3", new Version("1", "21", "", "rc", "3")),
    new("1.21.0", new Version("1", "21", "0", "", "")),
    new("1.24", new Version("1", "24", "", "", "")),
    new("1.24rc3", new Version("1", "24", "", "rc", "3")),
    new("1.24.0", new Version("1", "24", "0", "", "")),
    new("1.999testmod", new Version("1", "999", "", "testmod", "")),
    new("1.99999999999999999", new Version("1", "99999999999999999", "", "", ""))
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string langˢ => ref langˢᵇ;
private static readonly @string langˢᵇ = "Lang"u8;

public static void TestLang(ж<testing.T> t) { test1<@string, @string>(t, langTests, langˢ, Lang); }

internal static slice<testCase1<@string, @string>> langTests = [
    new("1.2rc3", "1.2"),
    new("1.2.3", "1.2"),
    new("1.2", "1.2"),
    new("1", "1"),
    new("1.999testmod", "1.999")
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string isLangˢ => ref isLangˢᵇ;
private static readonly @string isLangˢᵇ = "IsLang"u8;

public static void TestIsLang(ж<testing.T> t) { test1<@string, bool>(t, isLangTests, isLangˢ, IsLang); }

internal static slice<testCase1<@string, bool>> isLangTests = [
    new("1.2rc3", false),
    new("1.2.3", false),
    new("1.999testmod", false),
    new("1.22", true),
    new("1.21", true),
    new("1.20", false), // == 1.20.0
    new("1.19", false), // == 1.20.0
    new("1.3", false),  // == 1.3.0
    new("1.2", false),  // == 1.2.0
    new("1", false)     // == 1.0.0
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string isValidˢ => ref isValidˢᵇ;
private static readonly @string isValidˢᵇ = "IsValid"u8;

public static void TestIsValid(ж<testing.T> t) { test1<@string, bool>(t, isValidTests, isValidˢ, IsValid); }

internal static slice<testCase1<@string, bool>> isValidTests = [
    new("1.2rc3", true),
    new("1.2.3", true),
    new("1.999testmod", true),
    new("1.600+auto", false),
    new("1.22", true),
    new("1.21.0", true),
    new("1.21rc2", true),
    new("1.21", true),
    new("1.20.0", true),
    new("1.20", true),
    new("1.19", true),
    new("1.3", true),
    new("1.2", true),
    new("1", true)
];

internal partial struct testCase1<In, Out> {
    internal In @in;
    internal Out @out;
}

internal partial struct testCase2<In1, In2, Out> {
    internal In1 in1;
    internal In2 in2;
    internal Out @out;
}

internal partial struct testCase3<In1, In2, In3, Out> {
    internal In1 in1;
    internal In2 in2;
    internal In3 in3;
    internal Out @out;
}

internal static void test1<In, Out>(ж<testing.T> t, slice<testCase1<In, Out>> tests, @string name, Func<In, Out> f) {
    t.Helper();
    foreach (var (_, tt) in range(tests)) {
        var @out = f(tt.@in); if (!reflect.DeepEqual((@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf())) {
            t.Errorf("%s(%v) = %v, want %v", name, (tt.@in).OrTypedNilOf(), (@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf());
        }
    }
}

internal static void test2<In1, In2, Out>(ж<testing.T> t, slice<testCase2<In1, In2, Out>> tests, @string name, Func<In1, In2, Out> f) {
    t.Helper();
    foreach (var (_, tt) in range(tests)) {
        var @out = f(tt.in1, tt.in2); if (!reflect.DeepEqual((@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf())) {
            t.Errorf("%s(%+v, %+v) = %+v, want %+v", name, (tt.in1).OrTypedNilOf(), (tt.in2).OrTypedNilOf(), (@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf());
        }
    }
}

} // end gover_internal_test_package
