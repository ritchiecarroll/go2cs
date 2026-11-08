// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testenv = global::go.@internal.testenv_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

internal static ref readonly @string filename => ref filenameᵇ;
private static readonly @string filenameᵇ = "<src>"u8;

partial struct testEntry {
    internal @string src, str;
}

// dup returns a testEntry where both src and str are the same.
internal static testEntry dup(@string s) {
    return new testEntry(s, s);
}

// types that don't depend on any other type declarations
internal static slice<testEntry> independentTestTypes = slice<testEntry>([
    // basic types
    dup("int"),
    dup("float32"),
    dup("string"),

    // arrays
    dup("[10]int"),

    // slices
    dup("[]int"),
    dup("[][]int"),

    // structs
    dup("struct{}"),
    dup("struct{x int}"),
    new("""
struct {
		x, y int
		z float32 "foo"
	}
"""u8.Raw(), @"struct{x int; y int; z float32 ""foo""}"),
    new("""
struct {
		string
		elems []complex128
	}
"""u8.Raw(), @"struct{string; elems []complex128}"),

    // pointers
    dup("*int"),
    dup("***struct{}"),
    dup("*struct{a int; b float32}"),

    // functions
    dup("func()"),
    dup("func(x int)"),
    new("func(x, y int)", "func(x int, y int)"),
    new("func(x, y int, z string)", "func(x int, y int, z string)"),
    dup("func(int)"),
    new("func(int, string, byte)", "func(int, string, byte)"),

    dup("func() int"),
    new("func() (string)", "func() string"),
    dup("func() (u int)"),
    new("func() (u, v int, w string)", "func() (u int, v int, w string)"),

    dup("func(int) string"),
    dup("func(x int) string"),
    dup("func(x int) (u string)"),
    new("func(x, y int) (u string)", "func(x int, y int) (u string)"),

    dup("func(...int) string"),
    dup("func(x ...int) string"),
    dup("func(x ...int) (u string)"),
    new("func(x int, y ...int) (u string)", "func(x int, y ...int) (u string)"),

    // interfaces
    dup("interface{}"),
    dup("interface{m()}"),
    dup(@"interface{String() string; m(int) float32}"),
    dup("interface{int | float32 | complex128}"),
    dup("interface{int | ~float32 | ~complex128}"),
    dup("any"),
    dup("interface{comparable}"),
    // TODO(gri) adjust test for EvalCompositeTest
    // {"comparable", "interface{comparable}"},
    // {"error", "interface{Error() string}"},

    // maps
    dup("map[string]int"),
    new("map[struct{x, y int}][]byte", "map[struct{x int; y int}][]byte"),

    // channels
    dup("chan<- chan int"),
    dup("chan<- <-chan int"),
    dup("<-chan <-chan int"),
    dup("chan (<-chan int)"),
    dup("chan<- func()"),
    dup("<-chan []func() int")
]);

// types that depend on other type declarations (src in TestTypes)
internal static slice<testEntry> dependentTestTypes = slice<testEntry>([
    // interfaces
    dup(@"interface{io.Reader; io.Writer}"),
    dup(@"interface{m() int; io.Writer}"),
    new(@"interface{m() interface{T}}", @"interface{m() interface{p.T}}")
]);

public static void TestTypeString(ж<testing.T> t) {
    // The Go command is needed for the importer to determine the locations of stdlib .a files.
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    slice<testEntry> tests = default;
    tests = appendꓸꓸꓸ(tests, independentTestTypes);
    tests = appendꓸꓸꓸ(tests, dependentTestTypes);

    foreach (var (_, test) in range(tests)) {
        @string src = @"package p; import ""io""; type _ io.Writer; type T "u8 + test.src;
        var (pkg, err) = typecheck(src, nil, nil);
        if (err != nil) {
            t.Errorf("%s: %s", src, err);
            continue;
        }
        var obj = pkg.Scope().Lookup("T");
        if (obj == nil) {
            t.Errorf("%s: T not found", test.src);
            continue;
        }
        var typ = obj.Type().Underlying();
        @string got = typ.String(); if (got != test.str) {
            t.Errorf("%s: got %s, want %s", test.src, got, test.str);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePTypeTIntˢ => ref packagePTypeTIntˢᵇ;
private static readonly @string packagePTypeTIntˢᵇ = "package p; type T int"u8;
internal static ref readonly @string packageQˢ => ref packageQˢᵇ;
private static readonly @string packageQˢᵇ = "package q"u8;

internal partial struct TestQualifiedTypeString_type /*dyn*/ {
    internal typesꓸType typ;
    internal ж<types.Package> @this;
    internal @string want;
}

public static void TestQualifiedTypeString(ж<testing.T> t) {
    var p = mustTypecheck(packagePTypeTIntˢ, nil, nil);
    var q = mustTypecheck(packageQˢ, nil, nil);

    var pT = p.Scope().Lookup("T").Type();
    foreach (var (_, vᴛ1) in range(slice<TestQualifiedTypeString_type>([
        new(default, nil, "<nil>"),
        new(pT, nil, "p.T"),
        new(pT, p, "T"),
        new(pT, q, "p.T"),
        new(new types.PointerжΔType(NewPointer(pT)), p, "*T"),
        new(new types.PointerжΔType(NewPointer(pT)), q, "*p.T")
    ]))) {
        ref var test = ref heap(new TestQualifiedTypeString_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var qualifier = @string (ж<types.Package> pkg) => {
            if (pkg != testʗ1.@this) {
                return pkg.Name();
            }
            return ""u8;
        };
        @string got = TypeString(test.typ, new Func<ж<types.Package>, @string>(qualifier)); if (got != test.want) {
            t.Errorf("TypeString(%s, %s) = %s, want %s",
                test.@this.OrTypedNil(), test.typ, got, test.want);
        }
    }
}

} // end types_test_package
