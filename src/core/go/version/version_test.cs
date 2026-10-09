// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using reflect = reflect_package;
using testing = testing_package;
using static global::go.go.version_package;

partial class version_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string compareˢ = "Compare"u8;

public static void TestCompare(ж<testing.T> Ꮡt) {
    test2<@string, @string, nint>(Ꮡt, compareTests, compareˢ, Compare);
}

internal static slice<testCase2<@string, @string, nint>> compareTests = new testCase2<@string, @string, nint>[]{
    new(""u8, ""u8, 0),
    new("x"u8, "x"u8, 0),
    new(""u8, "x"u8, 0),
    new("1"u8, "1.1"u8, 0),
    new("go1"u8, "go1.1"u8, -1),
    new("go1.5"u8, "go1.6"u8, -1),
    new("go1.5"u8, "go1.10"u8, -1),
    new("go1.6"u8, "go1.6.1"u8, -1),
    new("go1.19"u8, "go1.19.0"u8, 0),
    new("go1.19rc1"u8, "go1.19"u8, -1),
    new("go1.20"u8, "go1.20.0"u8, 0),
    new("go1.20"u8, "go1.20.0-bigcorp"u8, 0),
    new("go1.20rc1"u8, "go1.20"u8, -1),
    new("go1.21"u8, "go1.21.0"u8, -1),
    new("go1.21"u8, "go1.21.0-bigcorp"u8, -1),
    new("go1.21"u8, "go1.21rc1"u8, -1),
    new("go1.21rc1"u8, "go1.21.0"u8, -1),
    new("go1.6"u8, "go1.19"u8, -1),
    new("go1.19"u8, "go1.19.1"u8, -1),
    new("go1.19rc1"u8, "go1.19"u8, -1),
    new("go1.19rc1"u8, "go1.19"u8, -1),
    new("go1.19rc1"u8, "go1.19.1"u8, -1),
    new("go1.19rc1"u8, "go1.19rc2"u8, -1),
    new("go1.19.0"u8, "go1.19.1"u8, -1),
    new("go1.19rc1"u8, "go1.19.0"u8, -1),
    new("go1.19alpha3"u8, "go1.19beta2"u8, -1),
    new("go1.19beta2"u8, "go1.19rc1"u8, -1),
    new("go1.1"u8, "go1.99999999999999998"u8, -1),
    new("go1.99999999999999998"u8, "go1.99999999999999999"u8, -1)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string langˢ = "Lang"u8;

public static void TestLang(ж<testing.T> Ꮡt) {
    test1<@string, @string>(Ꮡt, langTests, langˢ, Lang);
}

internal static slice<testCase1<@string, @string>> langTests = new testCase1<@string, @string>[]{
    new("bad"u8, ""u8),
    new("go1.2rc3"u8, "go1.2"u8),
    new("go1.2.3"u8, "go1.2"u8),
    new("go1.2"u8, "go1.2"u8),
    new("go1"u8, "go1"u8),
    new("go222"u8, "go222.0"u8),
    new("go1.999testmod"u8, "go1.999"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string isValidˢ = "IsValid"u8;

public static void TestIsValid(ж<testing.T> Ꮡt) {
    test1<@string, bool>(Ꮡt, isValidTests, isValidˢ, IsValid);
}

internal static slice<testCase1<@string, bool>> isValidTests = new testCase1<@string, bool>[]{
    new(""u8, false),
    new("1.2.3"u8, false),
    new("go1.2rc3"u8, true),
    new("go1.2.3"u8, true),
    new("go1.999testmod"u8, true),
    new("go1.600+auto"u8, false),
    new("go1.22"u8, true),
    new("go1.21.0"u8, true),
    new("go1.21rc2"u8, true),
    new("go1.21"u8, true),
    new("go1.20.0"u8, true),
    new("go1.20"u8, true),
    new("go1.19"u8, true),
    new("go1.3"u8, true),
    new("go1.2"u8, true),
    new("go1"u8, true)
}.slice();

internal partial struct testCase1<In, Out> {
    internal In @in;
    internal Out @out;
}

internal partial struct testCase2<In1, In2, Out> {
    internal In1 in1;
    internal In2 in2;
    internal Out @out;
}

internal static void test1<In, Out>(ж<testing.T> Ꮡt, slice<testCase1<In, Out>> tests, @string name, Func<In, Out> f) {
    Ꮡt.Helper();
    foreach (var (_, tt) in tests) {
        {
            var @out = f(tt.@in); if (!reflect.DeepEqual(@out, tt.@out)) {
                Ꮡt.Errorf("%s(%v) = %v, want %v"u8, name, tt.@in, @out, tt.@out);
            }
        }
    }
}

internal static void test2<In1, In2, Out>(ж<testing.T> Ꮡt, slice<testCase2<In1, In2, Out>> tests, @string name, Func<In1, In2, Out> f) {
    Ꮡt.Helper();
    foreach (var (_, tt) in tests) {
        {
            var @out = f(tt.in1, tt.in2); if (!reflect.DeepEqual(@out, tt.@out)) {
                Ꮡt.Errorf("%s(%+v, %+v) = %+v, want %+v"u8, name, tt.in1, tt.in2, @out, tt.@out);
            }
        }
    }
}

} // end version_internal_test_package
