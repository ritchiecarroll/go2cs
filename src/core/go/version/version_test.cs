// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using reflect = reflect_package;
using testing = testing_package;
using static global::go.go.version_package;

partial class version_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string compareˢ => ref compareˢᵇ;
private static readonly @string compareˢᵇ = "Compare"u8;

public static void TestCompare(ж<testing.T> t) {
    test2<@string, @string, nint>(t, compareTests, compareˢ, Compare);
}

internal static slice<testCase2<@string, @string, nint>> compareTests = slice<testCase2<@string, @string, nint>>([
    new("", "", 0),
    new("x", "x", 0),
    new("", "x", 0),
    new("1", "1.1", 0),
    new("go1", "go1.1", -1),
    new("go1.5", "go1.6", -1),
    new("go1.5", "go1.10", -1),
    new("go1.6", "go1.6.1", -1),
    new("go1.19", "go1.19.0", 0),
    new("go1.19rc1", "go1.19", -1),
    new("go1.20", "go1.20.0", 0),
    new("go1.20", "go1.20.0-bigcorp", 0),
    new("go1.20rc1", "go1.20", -1),
    new("go1.21", "go1.21.0", -1),
    new("go1.21", "go1.21.0-bigcorp", -1),
    new("go1.21", "go1.21rc1", -1),
    new("go1.21rc1", "go1.21.0", -1),
    new("go1.6", "go1.19", -1),
    new("go1.19", "go1.19.1", -1),
    new("go1.19rc1", "go1.19", -1),
    new("go1.19rc1", "go1.19", -1),
    new("go1.19rc1", "go1.19.1", -1),
    new("go1.19rc1", "go1.19rc2", -1),
    new("go1.19.0", "go1.19.1", -1),
    new("go1.19rc1", "go1.19.0", -1),
    new("go1.19alpha3", "go1.19beta2", -1),
    new("go1.19beta2", "go1.19rc1", -1),
    new("go1.1", "go1.99999999999999998", -1),
    new("go1.99999999999999998", "go1.99999999999999999", -1)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string langˢ => ref langˢᵇ;
private static readonly @string langˢᵇ = "Lang"u8;

public static void TestLang(ж<testing.T> t) {
    test1<@string, @string>(t, langTests, langˢ, Lang);
}

internal static slice<testCase1<@string, @string>> langTests = slice<testCase1<@string, @string>>([
    new("bad", ""),
    new("go1.2rc3", "go1.2"),
    new("go1.2.3", "go1.2"),
    new("go1.2", "go1.2"),
    new("go1", "go1"),
    new("go222", "go222.0"),
    new("go1.999testmod", "go1.999")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string isValidˢ => ref isValidˢᵇ;
private static readonly @string isValidˢᵇ = "IsValid"u8;

public static void TestIsValid(ж<testing.T> t) {
    test1<@string, bool>(t, isValidTests, isValidˢ, IsValid);
}

internal static slice<testCase1<@string, bool>> isValidTests = slice<testCase1<@string, bool>>([
    new("", false),
    new("1.2.3", false),
    new("go1.2rc3", true),
    new("go1.2.3", true),
    new("go1.999testmod", true),
    new("go1.600+auto", false),
    new("go1.22", true),
    new("go1.21.0", true),
    new("go1.21rc2", true),
    new("go1.21", true),
    new("go1.20.0", true),
    new("go1.20", true),
    new("go1.19", true),
    new("go1.3", true),
    new("go1.2", true),
    new("go1", true)
]);

internal partial struct testCase1<In, Out> {
    internal In @in;
    internal Out @out;
}

internal partial struct testCase2<In1, In2, Out> {
    internal In1 in1;
    internal In2 in2;
    internal Out @out;
}

internal static void test1<In, Out>(ж<testing.T> t, slice<testCase1<In, Out>> tests, @string name, Func<In, Out> f) {
    t.Helper();
    foreach (var (_, tt) in tests) {
        {
            var @out = f(tt.@in); if (!reflect.DeepEqual((@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf())) {
                t.Errorf("%s(%v) = %v, want %v", name, (tt.@in).OrTypedNilOf(), (@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf());
            }
        }
    }
}

internal static void test2<In1, In2, Out>(ж<testing.T> t, slice<testCase2<In1, In2, Out>> tests, @string name, Func<In1, In2, Out> f) {
    t.Helper();
    foreach (var (_, tt) in tests) {
        {
            var @out = f(tt.in1, tt.in2); if (!reflect.DeepEqual((@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf())) {
                t.Errorf("%s(%+v, %+v) = %+v, want %+v", name, (tt.in1).OrTypedNilOf(), (tt.in2).OrTypedNilOf(), (@out).OrTypedNilOf(), (tt.@out).OrTypedNilOf());
            }
        }
    }
}

} // end version_internal_test_package
