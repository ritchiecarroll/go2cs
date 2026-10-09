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

internal static readonly @string filename = "<src>"u8;

partial struct testEntry {
    internal @string src, str;
}

// dup returns a testEntry where both src and str are the same.
internal static testEntry dup(@string s) {
    return new testEntry(s, s);
}

// basic types
// arrays
// slices
// structs
// pointers
// functions
// interfaces
// TODO(gri) adjust test for EvalCompositeTest
// {"comparable", "interface{comparable}"},
// {"error", "interface{Error() string}"},
// maps
// channels
// types that don't depend on any other type declarations
internal static slice<testEntry> independentTestTypes = new testEntry[]{
    dup("int"u8),
    dup("float32"u8),
    dup("string"u8),
    dup("[10]int"u8),
    dup("[]int"u8),
    dup("[][]int"u8),
    dup("struct{}"u8),
    dup("struct{x int}"u8),
    new("""
struct {
		x, y int
		z float32 "foo"
	}
"""u8, @"struct{x int; y int; z float32 ""foo""}"u8),
    new("""
struct {
		string
		elems []complex128
	}
"""u8, @"struct{string; elems []complex128}"u8),
    dup("*int"u8),
    dup("***struct{}"u8),
    dup("*struct{a int; b float32}"u8),
    dup("func()"u8),
    dup("func(x int)"u8),
    new("func(x, y int)"u8, "func(x int, y int)"u8),
    new("func(x, y int, z string)"u8, "func(x int, y int, z string)"u8),
    dup("func(int)"u8),
    new("func(int, string, byte)"u8, "func(int, string, byte)"u8),
    dup("func() int"u8),
    new("func() (string)"u8, "func() string"u8),
    dup("func() (u int)"u8),
    new("func() (u, v int, w string)"u8, "func() (u int, v int, w string)"u8),
    dup("func(int) string"u8),
    dup("func(x int) string"u8),
    dup("func(x int) (u string)"u8),
    new("func(x, y int) (u string)"u8, "func(x int, y int) (u string)"u8),
    dup("func(...int) string"u8),
    dup("func(x ...int) string"u8),
    dup("func(x ...int) (u string)"u8),
    new("func(x int, y ...int) (u string)"u8, "func(x int, y ...int) (u string)"u8),
    dup("interface{}"u8),
    dup("interface{m()}"u8),
    dup(@"interface{String() string; m(int) float32}"u8),
    dup("interface{int | float32 | complex128}"u8),
    dup("interface{int | ~float32 | ~complex128}"u8),
    dup("any"u8),
    dup("interface{comparable}"u8),
    dup("map[string]int"u8),
    new("map[struct{x, y int}][]byte"u8, "map[struct{x int; y int}][]byte"u8),
    dup("chan<- chan int"u8),
    dup("chan<- <-chan int"u8),
    dup("<-chan <-chan int"u8),
    dup("chan (<-chan int)"u8),
    dup("chan<- func()"u8),
    dup("<-chan []func() int"u8)
}.slice();

// interfaces
// types that depend on other type declarations (src in TestTypes)
internal static slice<testEntry> dependentTestTypes = new testEntry[]{
    dup(@"interface{io.Reader; io.Writer}"u8),
    dup(@"interface{m() int; io.Writer}"u8),
    new(@"interface{m() interface{T}}"u8, @"interface{m() interface{p.T}}"u8)
}.slice();

public static void TestTypeString(ж<testing.T> Ꮡt) {
    // The Go command is needed for the importer to determine the locations of stdlib .a files.
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    slice<testEntry> tests = default!;
    tests = appendꓸꓸꓸ(tests, independentTestTypes);
    tests = appendꓸꓸꓸ(tests, dependentTestTypes);
    foreach (var (_, test) in tests) {
        @string src = @"package p; import ""io""; type _ io.Writer; type T "u8 + test.src;
        var (pkg, err) = typecheck(src, nil, nil);
        if (err != default!) {
            Ꮡt.Errorf("%s: %s"u8, src, err);
            continue;
        }
        var obj = pkg.Scope().Lookup("T"u8);
        if (obj == default!) {
            Ꮡt.Errorf("%s: T not found"u8, test.src);
            continue;
        }
        var typ = obj.Type().Underlying();
        {
            @string got = typ.String(); if (got != test.str) {
                Ꮡt.Errorf("%s: got %s, want %s"u8, test.src, got, test.str);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeTIntˢ = "package p; type T int"u8;
internal static readonly @string packageQˢ = "package q"u8;

internal partial struct TestQualifiedTypeString_type /*dyn*/ {
    internal typesꓸType typ;
    internal ж<types.Package> @this;
    internal @string want;
}

public static void TestQualifiedTypeString(ж<testing.T> Ꮡt) {
    var p = mustTypecheck(packagePTypeTIntˢ, nil, nil);
    var q = mustTypecheck(packageQˢ, nil, nil);
    var pT = p.Scope().Lookup("T"u8).Type();
    foreach (var (_, vᴛ1) in new TestQualifiedTypeString_type[]{
        new(default!, nil, "<nil>"u8),
        new(pT, nil, "p.T"u8),
        new(pT, p, "T"u8),
        new(pT, q, "p.T"u8),
        new(new types.PointerжΔType(NewPointer(pT)), p, "*T"u8),
        new(new types.PointerжΔType(NewPointer(pT)), q, "*p.T"u8)
    }.slice()) {
        ref var test = ref heap(new TestQualifiedTypeString_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var qualifier = @string (ж<types.Package> pkg) => {
            if (pkg != testʗ1.@this) {
                return pkg.Name();
            }
            return ""u8;
        };
        {
            @string got = TypeString(test.typ, new Func<ж<types.Package>, @string>(qualifier)); if (got != test.want) {
                Ꮡt.Errorf("TypeString(%s, %s) = %s, want %s"u8,
                    test.@this.OrTypedNil(), test.typ, got, test.want);
            }
        }
    }
}

} // end types_test_package
