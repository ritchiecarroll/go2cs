// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using errors = errors_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using importer = global::go.go.importer_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using goversion = global::go.@internal.goversion_package;
using testenv = global::go.@internal.testenv_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using static global::go.go.types_package;
using runtime = runtime_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.@internal;
using global::go.go;
using io = io_package;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

// nopos indicates an unknown position
internal static tokenꓸPos nopos;

internal static types.Importer defaultImporter(ж<token.FileSet> Ꮡfset) {
    return importer.ForCompiler(Ꮡfset, runtime.Compiler, default!);
}

internal static ж<ast.File> mustParse(ж<token.FileSet> Ꮡfset, @string src) {
    var (f, err) = parser.ParseFile(Ꮡfset, pkgName(src), src, parser.ParseComments);
    if (err != default!) {
        throw panic(err); // so we don't need to pass *testing.T
    }
    return f;
}

internal static (ж<types.Package>, error) typecheck(@string src, ж<types.Config> Ꮡconf, ж<typesꓸInfo> Ꮡinfo) {
    ref var conf = ref Ꮡconf.DerefOrNull();

    // TODO(adonovan): plumb this from caller.
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    if (Ꮡconf == nil) {
        Ꮡconf = Ꮡ(new Config(
            Error: (error err) => {
            }, // collect all errors

            Importer: defaultImporter(fset)
        )); conf = ref Ꮡconf.DerefOrNull();
    }
    return Ꮡconf.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), Ꮡinfo);
}

internal static ж<types.Package> mustTypecheck(@string src, ж<types.Config> Ꮡconf, ж<typesꓸInfo> Ꮡinfo) {
    var (pkg, err) = typecheck(src, Ꮡconf, Ꮡinfo);
    if (err != default!) {
        throw panic(err); // so we don't need to pass *testing.T
    }
    return pkg;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string kwᶜ = "package "u8;

// pkgName extracts the package name from src, which must contain a package header.
internal static @string pkgName(@string src) {
    @string kw = kwᶜ;
    {
        nint i = strings.Index(src, kw); if (i >= 0) {
            @string after = src.slice(i + len(kw));
            nint n = len(after);
            {
                nint iΔ1 = strings.IndexAny(after, "\n\t ;/"u8); if (iΔ1 >= 0) {
                    n = iΔ1;
                }
            }
            return after.slice(0, n);
        }
    }
    throw panic("missing package header: " + src);
}

[GoType("dyn")] internal partial struct TestValuesInfo_type {
    internal @string src;
    internal @string expr; // constant expression
    internal @string typ; // constant type
    internal @string val; // constant value
}

public static void TestValuesInfo(ж<testing.T> Ꮡt) {
// go.dev/issue/22341
// go.dev/issue/48422
    slice<TestValuesInfo_type> tests = new TestValuesInfo_type[]{
        new(@"package a0; const _ = false"u8, @"false"u8, @"untyped bool"u8, @"false"u8),
        new(@"package a1; const _ = 0"u8, @"0"u8, @"untyped int"u8, @"0"u8),
        new(@"package a2; const _ = 'A'"u8, @"'A'"u8, @"untyped rune"u8, @"65"u8),
        new(@"package a3; const _ = 0."u8, @"0."u8, @"untyped float"u8, @"0"u8),
        new(@"package a4; const _ = 0i"u8, @"0i"u8, @"untyped complex"u8, @"(0 + 0i)"u8),
        new(@"package a5; const _ = ""foo"""u8, @"""foo"""u8, @"untyped string"u8, @"""foo"""u8),
        new(@"package b0; var _ = false"u8, @"false"u8, @"bool"u8, @"false"u8),
        new(@"package b1; var _ = 0"u8, @"0"u8, @"int"u8, @"0"u8),
        new(@"package b2; var _ = 'A'"u8, @"'A'"u8, @"rune"u8, @"65"u8),
        new(@"package b3; var _ = 0."u8, @"0."u8, @"float64"u8, @"0"u8),
        new(@"package b4; var _ = 0i"u8, @"0i"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package b5; var _ = ""foo"""u8, @"""foo"""u8, @"string"u8, @"""foo"""u8),
        new(@"package c0a; var _ = bool(false)"u8, @"false"u8, @"bool"u8, @"false"u8),
        new(@"package c0b; var _ = bool(false)"u8, @"bool(false)"u8, @"bool"u8, @"false"u8),
        new(@"package c0c; type T bool; var _ = T(false)"u8, @"T(false)"u8, @"c0c.T"u8, @"false"u8),
        new(@"package c1a; var _ = int(0)"u8, @"0"u8, @"int"u8, @"0"u8),
        new(@"package c1b; var _ = int(0)"u8, @"int(0)"u8, @"int"u8, @"0"u8),
        new(@"package c1c; type T int; var _ = T(0)"u8, @"T(0)"u8, @"c1c.T"u8, @"0"u8),
        new(@"package c2a; var _ = rune('A')"u8, @"'A'"u8, @"rune"u8, @"65"u8),
        new(@"package c2b; var _ = rune('A')"u8, @"rune('A')"u8, @"rune"u8, @"65"u8),
        new(@"package c2c; type T rune; var _ = T('A')"u8, @"T('A')"u8, @"c2c.T"u8, @"65"u8),
        new(@"package c3a; var _ = float32(0.)"u8, @"0."u8, @"float32"u8, @"0"u8),
        new(@"package c3b; var _ = float32(0.)"u8, @"float32(0.)"u8, @"float32"u8, @"0"u8),
        new(@"package c3c; type T float32; var _ = T(0.)"u8, @"T(0.)"u8, @"c3c.T"u8, @"0"u8),
        new(@"package c4a; var _ = complex64(0i)"u8, @"0i"u8, @"complex64"u8, @"(0 + 0i)"u8),
        new(@"package c4b; var _ = complex64(0i)"u8, @"complex64(0i)"u8, @"complex64"u8, @"(0 + 0i)"u8),
        new(@"package c4c; type T complex64; var _ = T(0i)"u8, @"T(0i)"u8, @"c4c.T"u8, @"(0 + 0i)"u8),
        new(@"package c5a; var _ = string(""foo"")"u8, @"""foo"""u8, @"string"u8, @"""foo"""u8),
        new(@"package c5b; var _ = string(""foo"")"u8, @"string(""foo"")"u8, @"string"u8, @"""foo"""u8),
        new(@"package c5c; type T string; var _ = T(""foo"")"u8, @"T(""foo"")"u8, @"c5c.T"u8, @"""foo"""u8),
        new(@"package c5d; var _ = string(65)"u8, @"65"u8, @"untyped int"u8, @"65"u8),
        new(@"package c5e; var _ = string('A')"u8, @"'A'"u8, @"untyped rune"u8, @"65"u8),
        new(@"package c5f; type T string; var _ = T('A')"u8, @"'A'"u8, @"untyped rune"u8, @"65"u8),
        new(@"package d0; var _ = []byte(""foo"")"u8, @"""foo"""u8, @"string"u8, @"""foo"""u8),
        new(@"package d1; var _ = []byte(string(""foo""))"u8, @"""foo"""u8, @"string"u8, @"""foo"""u8),
        new(@"package d2; var _ = []byte(string(""foo""))"u8, @"string(""foo"")"u8, @"string"u8, @"""foo"""u8),
        new(@"package d3; type T []byte; var _ = T(""foo"")"u8, @"""foo"""u8, @"string"u8, @"""foo"""u8),
        new(@"package e0; const _ = float32( 1e-200)"u8, @"float32(1e-200)"u8, @"float32"u8, @"0"u8),
        new(@"package e1; const _ = float32(-1e-200)"u8, @"float32(-1e-200)"u8, @"float32"u8, @"0"u8),
        new(@"package e2; const _ = float64( 1e-2000)"u8, @"float64(1e-2000)"u8, @"float64"u8, @"0"u8),
        new(@"package e3; const _ = float64(-1e-2000)"u8, @"float64(-1e-2000)"u8, @"float64"u8, @"0"u8),
        new(@"package e4; const _ = complex64( 1e-200)"u8, @"complex64(1e-200)"u8, @"complex64"u8, @"(0 + 0i)"u8),
        new(@"package e5; const _ = complex64(-1e-200)"u8, @"complex64(-1e-200)"u8, @"complex64"u8, @"(0 + 0i)"u8),
        new(@"package e6; const _ = complex128( 1e-2000)"u8, @"complex128(1e-2000)"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package e7; const _ = complex128(-1e-2000)"u8, @"complex128(-1e-2000)"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package f0 ; var _ float32 =  1e-200"u8, @"1e-200"u8, @"float32"u8, @"0"u8),
        new(@"package f1 ; var _ float32 = -1e-200"u8, @"-1e-200"u8, @"float32"u8, @"0"u8),
        new(@"package f2a; var _ float64 =  1e-2000"u8, @"1e-2000"u8, @"float64"u8, @"0"u8),
        new(@"package f3a; var _ float64 = -1e-2000"u8, @"-1e-2000"u8, @"float64"u8, @"0"u8),
        new(@"package f2b; var _         =  1e-2000"u8, @"1e-2000"u8, @"float64"u8, @"0"u8),
        new(@"package f3b; var _         = -1e-2000"u8, @"-1e-2000"u8, @"float64"u8, @"0"u8),
        new(@"package f4 ; var _ complex64  =  1e-200 "u8, @"1e-200"u8, @"complex64"u8, @"(0 + 0i)"u8),
        new(@"package f5 ; var _ complex64  = -1e-200 "u8, @"-1e-200"u8, @"complex64"u8, @"(0 + 0i)"u8),
        new(@"package f6a; var _ complex128 =  1e-2000i"u8, @"1e-2000i"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package f7a; var _ complex128 = -1e-2000i"u8, @"-1e-2000i"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package f6b; var _            =  1e-2000i"u8, @"1e-2000i"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package f7b; var _            = -1e-2000i"u8, @"-1e-2000i"u8, @"complex128"u8, @"(0 + 0i)"u8),
        new(@"package g0; const (a = len([iota]int{}); b; c); const _ = c"u8, @"c"u8, @"int"u8, @"2"u8),
        new(@"package g1; var(j int32; s int; n = 1.0<<s == j)"u8, @"1.0"u8, @"int32"u8, @"1"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(
            Types: new map<ast.Expr, types.TypeAndValue>()
        );
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // look for expression
        ast.Expr expr = default!;
        foreach (var (e, _) in info.Types) {
            if (ExprString(e) == test.expr) {
                expr = e;
                break;
            }
        }
        if (expr == default!) {
            Ꮡt.Errorf("package %s: no expression found for %s"u8, name, test.expr);
            continue;
        }
        var tv = info.Types[expr];
        // check that type is correct
        {
            @string got = tv.Type.String(); if (got != test.typ) {
                Ꮡt.Errorf("package %s: got type %s; want %s"u8, name, got, test.typ);
                continue;
            }
        }
        // if we have a constant, check that value is correct
        if (tv.Value != default!){
            {
                @string got = tv.Value.ExactString(); if (got != test.val) {
                    Ꮡt.Errorf("package %s: got value %s; want %s"u8, name, got, test.val);
                }
            }
        } else {
            if (test.val != ""u8) {
                Ꮡt.Errorf("package %s: no constant found; want %s"u8, name, test.val);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string brokenᶜ = "package broken_"u8;

[GoType("dyn")] internal partial struct TestTypesInfo_type {
    internal @string src;
    internal @string expr; // expression
    internal @string typ; // value type
}

public static void TestTypesInfo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test sources that are not expected to typecheck must start with the broken prefix.
    @string broken = brokenᶜ;
// single-valued expressions of untyped constants
// uses of nil
// comma-ok expressions
// go.dev/issue/6796
// go.dev/issue/7060
// go.dev/issue/28277
// go.dev/issue/47243
// tests for broken code that doesn't type-check
// parameterized functions
// type parameters
// t[] is a syntax error that is ignored in this test in favor of t
// instantiated types must be sanitized
// go.dev/issue/45096
// go.dev/issue/47895
// go.dev/issue/50093
// reverse type inference
// go.dev/issues/60212
// go.dev/issues/60212
// go.dev/issues/60212
// go.dev/issues/60212
// go.dev/issues/60212
// go.dev/issue/68639
// parenthesized and pointer type expressions in various positions
// - as variable type, not generic
// - as ordinary function parameter, not generic
// - as method receiver, not generic
// - as variable type, generic
// - as ordinary function parameter, generic
// - as method receiver, generic
// For historic reasons, type parameters in receiver type expressions
// are considered both definitions and uses and thus also show up in
// the Info.Types map (see go.dev/issue/68670).
    slice<TestTypesInfo_type> tests = new TestTypesInfo_type[]{
        new(@"package b0; var x interface{} = false"u8, @"false"u8, @"bool"u8),
        new(@"package b1; var x interface{} = 0"u8, @"0"u8, @"int"u8),
        new(@"package b2; var x interface{} = 0."u8, @"0."u8, @"float64"u8),
        new(@"package b3; var x interface{} = 0i"u8, @"0i"u8, @"complex128"u8),
        new(@"package b4; var x interface{} = ""foo"""u8, @"""foo"""u8, @"string"u8),
        new(@"package n0; var _ *int = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n1; var _ func() = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n2; var _ []byte = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n3; var _ map[int]int = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n4; var _ chan int = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n5; var _ interface{} = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n6; import ""unsafe""; var _ unsafe.Pointer = nil"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n10; var (x *int; _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n11; var (x func(); _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n12; var (x []byte; _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n13; var (x map[int]int; _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n14; var (x chan int; _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n15; var (x interface{}; _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n15; import ""unsafe""; var (x unsafe.Pointer; _ = x == nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n20; var _ = (*int)(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n21; var _ = (func())(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n22; var _ = ([]byte)(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n23; var _ = (map[int]int)(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n24; var _ = (chan int)(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n25; var _ = (interface{})(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n26; import ""unsafe""; var _ = unsafe.Pointer(nil)"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n30; func f(*int) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n31; func f(func()) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n32; func f([]byte) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n33; func f(map[int]int) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n34; func f(chan int) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n35; func f(interface{}) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package n35; import ""unsafe""; func f(unsafe.Pointer) { f(nil) }"u8, @"nil"u8, @"untyped nil"u8),
        new(@"package p0; var x interface{}; var _, _ = x.(int)"u8,
            @"x.(int)"u8,
            @"(int, bool)"u8
        ),
        new(@"package p1; var x interface{}; func _() { _, _ = x.(int) }"u8,
            @"x.(int)"u8,
            @"(int, bool)"u8
        ),
        new(@"package p2a; type mybool bool; var m map[string]complex128; var b mybool; func _() { _, b = m[""foo""] }"u8,
            @"m[""foo""]"u8,
            @"(complex128, p2a.mybool)"u8
        ),
        new(@"package p2b; var m map[string]complex128; var b bool; func _() { _, b = m[""foo""] }"u8,
            @"m[""foo""]"u8,
            @"(complex128, bool)"u8
        ),
        new(@"package p3; var c chan string; var _, _ = <-c"u8,
            @"<-c"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue6796_a; var x interface{}; var _, _ = (x.(int))"u8,
            @"x.(int)"u8,
            @"(int, bool)"u8
        ),
        new(@"package issue6796_b; var c chan string; var _, _ = (<-c)"u8,
            @"(<-c)"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue6796_c; var c chan string; var _, _ = (<-c)"u8,
            @"<-c"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue6796_d; var c chan string; var _, _ = ((<-c))"u8,
            @"(<-c)"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue6796_e; func f(c chan string) { _, _ = ((<-c)) }"u8,
            @"(<-c)"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue7060_a; var ( m map[int]string; x, ok = m[0] )"u8,
            @"m[0]"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue7060_b; var ( m map[int]string; x, ok interface{} = m[0] )"u8,
            @"m[0]"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue7060_c; func f(x interface{}, ok bool, m map[int]string) { x, ok = m[0] }"u8,
            @"m[0]"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue7060_d; var ( ch chan string; x, ok = <-ch )"u8,
            @"<-ch"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue7060_e; var ( ch chan string; x, ok interface{} = <-ch )"u8,
            @"<-ch"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue7060_f; func f(x interface{}, ok bool, ch chan string) { x, ok = <-ch }"u8,
            @"<-ch"u8,
            @"(string, bool)"u8
        ),
        new(@"package issue28277_a; func f(...int)"u8,
            @"...int"u8,
            @"[]int"u8
        ),
        new(@"package issue28277_b; func f(a, b int, c ...[]struct{})"u8,
            @"...[]struct{}"u8,
            @"[][]struct{}"u8
        ),
        new(@"package issue47243_a; var x int32; var _ = x << 3"u8, @"3"u8, @"untyped int"u8),
        new(@"package issue47243_b; var x int32; var _ = x << 3."u8, @"3."u8, @"untyped float"u8),
        new(@"package issue47243_c; var x int32; var _ = 1 << x"u8, @"1 << x"u8, @"int"u8),
        new(@"package issue47243_d; var x int32; var _ = 1 << x"u8, @"1"u8, @"int"u8),
        new(@"package issue47243_e; var x int32; var _ = 1 << 2"u8, @"1"u8, @"untyped int"u8),
        new(@"package issue47243_f; var x int32; var _ = 1 << 2"u8, @"2"u8, @"untyped int"u8),
        new(@"package issue47243_g; var x int32; var _ = int(1) << 2"u8, @"2"u8, @"untyped int"u8),
        new(@"package issue47243_h; var x int32; var _ = 1 << (2 << x)"u8, @"1"u8, @"int"u8),
        new(@"package issue47243_i; var x int32; var _ = 1 << (2 << x)"u8, @"(2 << x)"u8, @"untyped int"u8),
        new(@"package issue47243_j; var x int32; var _ = 1 << (2 << x)"u8, @"2"u8, @"untyped int"u8),
        new(broken + @"x0; func _() { var x struct {f string}; x.f := 0 }", @"x.f"u8, @"string"u8),
        new(broken + @"x1; func _() { var z string; type x struct {f string}; y := &x{q: z}}", @"z"u8, @"string"u8),
        new(broken + @"x2; func _() { var a, b string; type x struct {f string}; z := &x{f: a, f: b,}}", @"b"u8, @"string"u8),
        new(broken + @"x3; var x = panic("""");", @"panic"u8, @"func(interface{})"u8),
        new(@"package x4; func _() { panic("""") }"u8, @"panic"u8, @"func(interface{})"u8),
        new(broken + @"x5; func _() { var x map[string][...]int; x = map[string][...]int{"""": {1,2,3}} }", @"x"u8, @"map[string]invalid type"u8),
        new(@"package p0; func f[T any](T) {}; var _ = f[int]"u8, @"f"u8, @"func[T any](T)"u8),
        new(@"package p1; func f[T any](T) {}; var _ = f[int]"u8, @"f[int]"u8, @"func(int)"u8),
        new(@"package p2; func f[T any](T) {}; func _() { f(42) }"u8, @"f"u8, @"func(int)"u8),
        new(@"package p3; func f[T any](T) {}; func _() { f[int](42) }"u8, @"f[int]"u8, @"func(int)"u8),
        new(@"package p4; func f[T any](T) {}; func _() { f[int](42) }"u8, @"f"u8, @"func[T any](T)"u8),
        new(@"package p5; func f[T any](T) {}; func _() { f(42) }"u8, @"f(42)"u8, @"()"u8),
        new(@"package t0; type t[] int; var _ t"u8, @"t"u8, @"t0.t"u8),
        new(@"package t1; type t[P any] int; var _ t[int]"u8, @"t"u8, @"t1.t[P any]"u8),
        new(@"package t2; type t[P interface{}] int; var _ t[int]"u8, @"t"u8, @"t2.t[P interface{}]"u8),
        new(@"package t3; type t[P, Q interface{}] int; var _ t[int, int]"u8, @"t"u8, @"t3.t[P, Q interface{}]"u8),
        new(broken + @"t4; type t[P, Q interface{ m() }] int; var _ t[int, int]", @"t"u8, @"broken_t4.t[P, Q interface{m()}]"u8),
        new(@"package g0; type t[P any] int; var x struct{ f t[int] }; var _ = x.f"u8, @"x.f"u8, @"g0.t[int]"u8),
        new(@"package issue45096; func _[T interface{ ~int8 | ~int16 | ~int32  }](x T) { _ = x < 0 }"u8, @"0"u8, @"T"u8),
        new(@"package p; import ""unsafe""; type S struct { f int }; var s S; var _ = unsafe.Offsetof(s.f)"u8, @"s.f"u8, @"int"u8),
        new(@"package u0a; func _[_ interface{int}]() {}"u8, @"int"u8, @"int"u8),
        new(@"package u1a; func _[_ interface{~int}]() {}"u8, @"~int"u8, @"~int"u8),
        new(@"package u2a; func _[_ interface{int | string}]() {}"u8, @"int | string"u8, @"int | string"u8),
        new(@"package u3a; func _[_ interface{int | string | ~bool}]() {}"u8, @"int | string | ~bool"u8, @"int | string | ~bool"u8),
        new(@"package u3a; func _[_ interface{int | string | ~bool}]() {}"u8, @"int | string"u8, @"int | string"u8),
        new(@"package u3a; func _[_ interface{int | string | ~bool}]() {}"u8, @"~bool"u8, @"~bool"u8),
        new(@"package u3a; func _[_ interface{int | string | ~float64|~bool}]() {}"u8, @"int | string | ~float64"u8, @"int | string | ~float64"u8),
        new(@"package u0b; func _[_ int]() {}"u8, @"int"u8, @"int"u8),
        new(@"package u1b; func _[_ ~int]() {}"u8, @"~int"u8, @"~int"u8),
        new(@"package u2b; func _[_ int | string]() {}"u8, @"int | string"u8, @"int | string"u8),
        new(@"package u3b; func _[_ int | string | ~bool]() {}"u8, @"int | string | ~bool"u8, @"int | string | ~bool"u8),
        new(@"package u3b; func _[_ int | string | ~bool]() {}"u8, @"int | string"u8, @"int | string"u8),
        new(@"package u3b; func _[_ int | string | ~bool]() {}"u8, @"~bool"u8, @"~bool"u8),
        new(@"package u3b; func _[_ int | string | ~float64|~bool]() {}"u8, @"int | string | ~float64"u8, @"int | string | ~float64"u8),
        new(@"package u0c; type _ interface{int}"u8, @"int"u8, @"int"u8),
        new(@"package u1c; type _ interface{~int}"u8, @"~int"u8, @"~int"u8),
        new(@"package u2c; type _ interface{int | string}"u8, @"int | string"u8, @"int | string"u8),
        new(@"package u3c; type _ interface{int | string | ~bool}"u8, @"int | string | ~bool"u8, @"int | string | ~bool"u8),
        new(@"package u3c; type _ interface{int | string | ~bool}"u8, @"int | string"u8, @"int | string"u8),
        new(@"package u3c; type _ interface{int | string | ~bool}"u8, @"~bool"u8, @"~bool"u8),
        new(@"package u3c; type _ interface{int | string | ~float64|~bool}"u8, @"int | string | ~float64"u8, @"int | string | ~float64"u8),
        new(@"package r1; var _ func(int) = g; func g[P any](P) {}"u8, @"g"u8, @"func(int)"u8),
        new(@"package r2; var _ func(int) = g[int]; func g[P any](P) {}"u8, @"g"u8, @"func[P any](P)"u8),
        new(@"package r3; var _ func(int) = g[int]; func g[P any](P) {}"u8, @"g[int]"u8, @"func(int)"u8),
        new(@"package r4; var _ func(int, string) = g; func g[P, Q any](P, Q) {}"u8, @"g"u8, @"func(int, string)"u8),
        new(@"package r5; var _ func(int, string) = g[int]; func g[P, Q any](P, Q) {}"u8, @"g"u8, @"func[P, Q any](P, Q)"u8),
        new(@"package r6; var _ func(int, string) = g[int]; func g[P, Q any](P, Q) {}"u8, @"g[int]"u8, @"func(int, string)"u8),
        new(@"package s1; func _() { f(g) }; func f(func(int)) {}; func g[P any](P) {}"u8, @"g"u8, @"func(int)"u8),
        new(@"package s2; func _() { f(g[int]) }; func f(func(int)) {}; func g[P any](P) {}"u8, @"g"u8, @"func[P any](P)"u8),
        new(@"package s3; func _() { f(g[int]) }; func f(func(int)) {}; func g[P any](P) {}"u8, @"g[int]"u8, @"func(int)"u8),
        new(@"package s4; func _() { f(g) }; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}"u8, @"g"u8, @"func(int, string)"u8),
        new(@"package s5; func _() { f(g[int]) }; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}"u8, @"g"u8, @"func[P, Q any](P, Q)"u8),
        new(@"package s6; func _() { f(g[int]) }; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}"u8, @"g[int]"u8, @"func(int, string)"u8),
        new(@"package s7; func _() { f(g, h) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}"u8, @"g"u8, @"func(int, int)"u8),
        new(@"package s8; func _() { f(g, h) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}"u8, @"h"u8, @"func(int, string)"u8),
        new(@"package s9; func _() { f(g, h[int]) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}"u8, @"h"u8, @"func[P, Q any](P, Q)"u8),
        new(@"package s10; func _() { f(g, h[int]) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}"u8, @"h[int]"u8, @"func(int, string)"u8),
        new(@"package qa1; type T int; var x T"u8, @"T"u8, @"qa1.T"u8),
        new(@"package qa2; type T int; var x (T)"u8, @"T"u8, @"qa2.T"u8),
        new(@"package qa3; type T int; var x (T)"u8, @"(T)"u8, @"qa3.T"u8),
        new(@"package qa4; type T int; var x ((T))"u8, @"T"u8, @"qa4.T"u8),
        new(@"package qa5; type T int; var x ((T))"u8, @"(T)"u8, @"qa5.T"u8),
        new(@"package qa6; type T int; var x ((T))"u8, @"((T))"u8, @"qa6.T"u8),
        new(@"package qa7; type T int; var x *T"u8, @"T"u8, @"qa7.T"u8),
        new(@"package qa8; type T int; var x *T"u8, @"*T"u8, @"*qa8.T"u8),
        new(@"package qa9; type T int; var x (*T)"u8, @"T"u8, @"qa9.T"u8),
        new(@"package qa10; type T int; var x (*T)"u8, @"*T"u8, @"*qa10.T"u8),
        new(@"package qa11; type T int; var x *(T)"u8, @"T"u8, @"qa11.T"u8),
        new(@"package qa12; type T int; var x *(T)"u8, @"(T)"u8, @"qa12.T"u8),
        new(@"package qa13; type T int; var x *(T)"u8, @"*(T)"u8, @"*qa13.T"u8),
        new(@"package qa14; type T int; var x (*(T))"u8, @"(T)"u8, @"qa14.T"u8),
        new(@"package qa15; type T int; var x (*(T))"u8, @"*(T)"u8, @"*qa15.T"u8),
        new(@"package qa16; type T int; var x (*(T))"u8, @"(*(T))"u8, @"*qa16.T"u8),
        new(@"package qb1; type T int; func _(T)"u8, @"T"u8, @"qb1.T"u8),
        new(@"package qb2; type T int; func _((T))"u8, @"T"u8, @"qb2.T"u8),
        new(@"package qb3; type T int; func _((T))"u8, @"(T)"u8, @"qb3.T"u8),
        new(@"package qb4; type T int; func _(((T)))"u8, @"T"u8, @"qb4.T"u8),
        new(@"package qb5; type T int; func _(((T)))"u8, @"(T)"u8, @"qb5.T"u8),
        new(@"package qb6; type T int; func _(((T)))"u8, @"((T))"u8, @"qb6.T"u8),
        new(@"package qb7; type T int; func _(*T)"u8, @"T"u8, @"qb7.T"u8),
        new(@"package qb8; type T int; func _(*T)"u8, @"*T"u8, @"*qb8.T"u8),
        new(@"package qb9; type T int; func _((*T))"u8, @"T"u8, @"qb9.T"u8),
        new(@"package qb10; type T int; func _((*T))"u8, @"*T"u8, @"*qb10.T"u8),
        new(@"package qb11; type T int; func _(*(T))"u8, @"T"u8, @"qb11.T"u8),
        new(@"package qb12; type T int; func _(*(T))"u8, @"(T)"u8, @"qb12.T"u8),
        new(@"package qb13; type T int; func _(*(T))"u8, @"*(T)"u8, @"*qb13.T"u8),
        new(@"package qb14; type T int; func _((*(T)))"u8, @"(T)"u8, @"qb14.T"u8),
        new(@"package qb15; type T int; func _((*(T)))"u8, @"*(T)"u8, @"*qb15.T"u8),
        new(@"package qb16; type T int; func _((*(T)))"u8, @"(*(T))"u8, @"*qb16.T"u8),
        new(@"package qc1; type T int; func (T) _() {}"u8, @"T"u8, @"qc1.T"u8),
        new(@"package qc2; type T int; func ((T)) _() {}"u8, @"T"u8, @"qc2.T"u8),
        new(@"package qc3; type T int; func ((T)) _() {}"u8, @"(T)"u8, @"qc3.T"u8),
        new(@"package qc4; type T int; func (((T))) _() {}"u8, @"T"u8, @"qc4.T"u8),
        new(@"package qc5; type T int; func (((T))) _() {}"u8, @"(T)"u8, @"qc5.T"u8),
        new(@"package qc6; type T int; func (((T))) _() {}"u8, @"((T))"u8, @"qc6.T"u8),
        new(@"package qc7; type T int; func (*T) _() {}"u8, @"T"u8, @"qc7.T"u8),
        new(@"package qc8; type T int; func (*T) _() {}"u8, @"*T"u8, @"*qc8.T"u8),
        new(@"package qc9; type T int; func ((*T)) _() {}"u8, @"T"u8, @"qc9.T"u8),
        new(@"package qc10; type T int; func ((*T)) _() {}"u8, @"*T"u8, @"*qc10.T"u8),
        new(@"package qc11; type T int; func (*(T)) _() {}"u8, @"T"u8, @"qc11.T"u8),
        new(@"package qc12; type T int; func (*(T)) _() {}"u8, @"(T)"u8, @"qc12.T"u8),
        new(@"package qc13; type T int; func (*(T)) _() {}"u8, @"*(T)"u8, @"*qc13.T"u8),
        new(@"package qc14; type T int; func ((*(T))) _() {}"u8, @"(T)"u8, @"qc14.T"u8),
        new(@"package qc15; type T int; func ((*(T))) _() {}"u8, @"*(T)"u8, @"*qc15.T"u8),
        new(@"package qc16; type T int; func ((*(T))) _() {}"u8, @"(*(T))"u8, @"*qc16.T"u8),
        new(@"package qd1; type T[_ any] int; var x T[int]"u8, @"T"u8, @"qd1.T[_ any]"u8),
        new(@"package qd2; type T[_ any] int; var x (T[int])"u8, @"T[int]"u8, @"qd2.T[int]"u8),
        new(@"package qd3; type T[_ any] int; var x (T[int])"u8, @"(T[int])"u8, @"qd3.T[int]"u8),
        new(@"package qd4; type T[_ any] int; var x ((T[int]))"u8, @"T"u8, @"qd4.T[_ any]"u8),
        new(@"package qd5; type T[_ any] int; var x ((T[int]))"u8, @"(T[int])"u8, @"qd5.T[int]"u8),
        new(@"package qd6; type T[_ any] int; var x ((T[int]))"u8, @"((T[int]))"u8, @"qd6.T[int]"u8),
        new(@"package qd7; type T[_ any] int; var x *T[int]"u8, @"T"u8, @"qd7.T[_ any]"u8),
        new(@"package qd8; type T[_ any] int; var x *T[int]"u8, @"*T[int]"u8, @"*qd8.T[int]"u8),
        new(@"package qd9; type T[_ any] int; var x (*T[int])"u8, @"T"u8, @"qd9.T[_ any]"u8),
        new(@"package qd10; type T[_ any] int; var x (*T[int])"u8, @"*T[int]"u8, @"*qd10.T[int]"u8),
        new(@"package qd11; type T[_ any] int; var x *(T[int])"u8, @"T[int]"u8, @"qd11.T[int]"u8),
        new(@"package qd12; type T[_ any] int; var x *(T[int])"u8, @"(T[int])"u8, @"qd12.T[int]"u8),
        new(@"package qd13; type T[_ any] int; var x *(T[int])"u8, @"*(T[int])"u8, @"*qd13.T[int]"u8),
        new(@"package qd14; type T[_ any] int; var x (*(T[int]))"u8, @"(T[int])"u8, @"qd14.T[int]"u8),
        new(@"package qd15; type T[_ any] int; var x (*(T[int]))"u8, @"*(T[int])"u8, @"*qd15.T[int]"u8),
        new(@"package qd16; type T[_ any] int; var x (*(T[int]))"u8, @"(*(T[int]))"u8, @"*qd16.T[int]"u8),
        new(@"package qe1; type T[_ any] int; func _(T[int])"u8, @"T"u8, @"qe1.T[_ any]"u8),
        new(@"package qe2; type T[_ any] int; func _((T[int]))"u8, @"T[int]"u8, @"qe2.T[int]"u8),
        new(@"package qe3; type T[_ any] int; func _((T[int]))"u8, @"(T[int])"u8, @"qe3.T[int]"u8),
        new(@"package qe4; type T[_ any] int; func _(((T[int])))"u8, @"T"u8, @"qe4.T[_ any]"u8),
        new(@"package qe5; type T[_ any] int; func _(((T[int])))"u8, @"(T[int])"u8, @"qe5.T[int]"u8),
        new(@"package qe6; type T[_ any] int; func _(((T[int])))"u8, @"((T[int]))"u8, @"qe6.T[int]"u8),
        new(@"package qe7; type T[_ any] int; func _(*T[int])"u8, @"T"u8, @"qe7.T[_ any]"u8),
        new(@"package qe8; type T[_ any] int; func _(*T[int])"u8, @"*T[int]"u8, @"*qe8.T[int]"u8),
        new(@"package qe9; type T[_ any] int; func _((*T[int]))"u8, @"T"u8, @"qe9.T[_ any]"u8),
        new(@"package qe10; type T[_ any] int; func _((*T[int]))"u8, @"*T[int]"u8, @"*qe10.T[int]"u8),
        new(@"package qe11; type T[_ any] int; func _(*(T[int]))"u8, @"T[int]"u8, @"qe11.T[int]"u8),
        new(@"package qe12; type T[_ any] int; func _(*(T[int]))"u8, @"(T[int])"u8, @"qe12.T[int]"u8),
        new(@"package qe13; type T[_ any] int; func _(*(T[int]))"u8, @"*(T[int])"u8, @"*qe13.T[int]"u8),
        new(@"package qe14; type T[_ any] int; func _((*(T[int])))"u8, @"(T[int])"u8, @"qe14.T[int]"u8),
        new(@"package qe15; type T[_ any] int; func _((*(T[int])))"u8, @"*(T[int])"u8, @"*qe15.T[int]"u8),
        new(@"package qe16; type T[_ any] int; func _((*(T[int])))"u8, @"(*(T[int]))"u8, @"*qe16.T[int]"u8),
        new(@"package qf1; type T[_ any] int; func (T[_]) _() {}"u8, @"T"u8, @"qf1.T[_ any]"u8),
        new(@"package qf2; type T[_ any] int; func ((T[_])) _() {}"u8, @"T[_]"u8, @"qf2.T[_]"u8),
        new(@"package qf3; type T[_ any] int; func ((T[_])) _() {}"u8, @"(T[_])"u8, @"qf3.T[_]"u8),
        new(@"package qf4; type T[_ any] int; func (((T[_]))) _() {}"u8, @"T"u8, @"qf4.T[_ any]"u8),
        new(@"package qf5; type T[_ any] int; func (((T[_]))) _() {}"u8, @"(T[_])"u8, @"qf5.T[_]"u8),
        new(@"package qf6; type T[_ any] int; func (((T[_]))) _() {}"u8, @"((T[_]))"u8, @"qf6.T[_]"u8),
        new(@"package qf7; type T[_ any] int; func (*T[_]) _() {}"u8, @"T"u8, @"qf7.T[_ any]"u8),
        new(@"package qf8; type T[_ any] int; func (*T[_]) _() {}"u8, @"*T[_]"u8, @"*qf8.T[_]"u8),
        new(@"package qf9; type T[_ any] int; func ((*T[_])) _() {}"u8, @"T"u8, @"qf9.T[_ any]"u8),
        new(@"package qf10; type T[_ any] int; func ((*T[_])) _() {}"u8, @"*T[_]"u8, @"*qf10.T[_]"u8),
        new(@"package qf11; type T[_ any] int; func (*(T[_])) _() {}"u8, @"T[_]"u8, @"qf11.T[_]"u8),
        new(@"package qf12; type T[_ any] int; func (*(T[_])) _() {}"u8, @"(T[_])"u8, @"qf12.T[_]"u8),
        new(@"package qf13; type T[_ any] int; func (*(T[_])) _() {}"u8, @"*(T[_])"u8, @"*qf13.T[_]"u8),
        new(@"package qf14; type T[_ any] int; func ((*(T[_]))) _() {}"u8, @"(T[_])"u8, @"qf14.T[_]"u8),
        new(@"package qf15; type T[_ any] int; func ((*(T[_]))) _() {}"u8, @"*(T[_])"u8, @"*qf15.T[_]"u8),
        new(@"package qf16; type T[_ any] int; func ((*(T[_]))) _() {}"u8, @"(*(T[_]))"u8, @"*qf16.T[_]"u8),
        new(@"package t1; type T[_ any] int; func (T[P]) _() {}"u8, @"P"u8, @"P"u8),
        new(@"package t2; type T[_, _ any] int; func (T[P, Q]) _() {}"u8, @"P"u8, @"P"u8),
        new(@"package t3; type T[_, _ any] int; func (T[P, Q]) _() {}"u8, @"Q"u8, @"Q"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>());
        @string name = default!;
        if (strings.HasPrefix(test.src, broken)){
            var (pkg, err) = typecheck(test.src, nil, Ꮡinfo);
            if (err == default!) {
                Ꮡt.Errorf("package %s: expected to fail but passed"u8, pkg.Name());
                continue;
            }
            if (pkg != nil) {
                name = pkg.Name();
            }
        } else {
            name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        }
        // look for expression type
        typesꓸType typ = default!;
        foreach (var (e, tv) in info.Types) {
            if (ExprString(e) == test.expr) {
                typ = tv.Type;
                break;
            }
        }
        if (typ == default!) {
            Ꮡt.Errorf("package %s: no type found for %s"u8, name, test.expr);
            continue;
        }
        // check that type is correct
        {
            @string got = typ.String(); if (got != test.typ) {
                Ꮡt.Errorf("package %s: expr = %s: got %s; want %s"u8, name, test.expr, got, test.typ);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string libᶜ = """
package lib

func F[P any](P) {}

type T[P any] []P

"""u8;

[GoType("dyn")] internal partial struct TestInstanceInfo_testInst {
    internal @string name;
    internal slice<@string> targs;
    internal @string typ;
}

[GoType("dyn")] internal partial struct TestInstanceInfo_type {
    internal @string src;
    internal slice<TestInstanceInfo_testInst> instances; // recorded instances in source order
}

[GoType("dyn")] internal partial interface TestInstanceInfo_typeᴛ1 {
    ж<types.TypeParamList> TypeParams();
}

public static void TestInstanceInfo(ж<testing.T> Ꮡt) {
    @string lib = libᶜ;
// reverse type inference
// reverse3a not possible (cannot assign to generic function outside of argument passing)
    slice<TestInstanceInfo_type> tests = new TestInstanceInfo_type[]{
        new(@"package p0; func f[T any](T) {}; func _() { f(42) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)}.slice()
        ),
        new(@"package p1; func f[T any](T) T { panic(0) }; func _() { f('@') }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"rune"u8}.slice(), @"func(rune) rune"u8)}.slice()
        ),
        new(@"package p2; func f[T any](...T) T { panic(0) }; func _() { f(0i) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"complex128"u8}.slice(), @"func(...complex128) complex128"u8)}.slice()
        ),
        new(@"package p3; func f[A, B, C any](A, *B, []C) {}; func _() { f(1.2, new(string), []byte{}) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"float64"u8, @"string"u8, @"byte"u8}.slice(), @"func(float64, *string, []byte)"u8)}.slice()
        ),
        new(@"package p4; func f[A, B any](A, *B, ...[]B) {}; func _() { f(1.2, new(byte)) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"float64"u8, @"byte"u8}.slice(), @"func(float64, *byte, ...[]byte)"u8)}.slice()
        ),
        new(@"package s1; func f[T any, P interface{*T}](x T) {}; func _(x string) { f(x) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"string"u8, @"*string"u8}.slice(), @"func(x string)"u8)}.slice()
        ),
        new(@"package s2; func f[T any, P interface{*T}](x []T) {}; func _(x []int) { f(x) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"int"u8, @"*int"u8}.slice(), @"func(x []int)"u8)}.slice()
        ),
        new(@"package s3; type C[T any] interface{chan<- T}; func f[T any, P C[T]](x []T) {}; func _(x []int) { f(x) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"C"u8, new @string[]{@"T"u8}.slice(), @"interface{chan<- T}"u8),
                new(@"f"u8, new @string[]{@"int"u8, @"chan<- int"u8}.slice(), @"func(x []int)"u8)
            }.slice()
        ),
        new(@"package s4; type C[T any] interface{chan<- T}; func f[T any, P C[T], Q C[[]*P]](x []T) {}; func _(x []int) { f(x) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"C"u8, new @string[]{@"T"u8}.slice(), @"interface{chan<- T}"u8),
                new(@"C"u8, new @string[]{@"[]*P"u8}.slice(), @"interface{chan<- []*P}"u8),
                new(@"f"u8, new @string[]{@"int"u8, @"chan<- int"u8, @"chan<- []*chan<- int"u8}.slice(), @"func(x []int)"u8)
            }.slice()
        ),
        new(@"package t1; func f[T any, P interface{*T}]() T { panic(0) }; func _() { _ = f[string] }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"string"u8, @"*string"u8}.slice(), @"func() string"u8)}.slice()
        ),
        new(@"package t2; func f[T any, P interface{*T}]() T { panic(0) }; func _() { _ = (f[string]) }"u8,
            new TestInstanceInfo_testInst[]{new(@"f"u8, new @string[]{@"string"u8, @"*string"u8}.slice(), @"func() string"u8)}.slice()
        ),
        new(@"package t3; type C[T any] interface{chan<- T}; func f[T any, P C[T], Q C[[]*P]]() []T { return nil }; func _() { _ = f[int] }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"C"u8, new @string[]{@"T"u8}.slice(), @"interface{chan<- T}"u8),
                new(@"C"u8, new @string[]{@"[]*P"u8}.slice(), @"interface{chan<- []*P}"u8),
                new(@"f"u8, new @string[]{@"int"u8, @"chan<- int"u8, @"chan<- []*chan<- int"u8}.slice(), @"func() []int"u8)
            }.slice()
        ),
        new(@"package t4; type C[T any] interface{chan<- T}; func f[T any, P C[T], Q C[[]*P]]() []T { return nil }; func _() { _ = (f[int]) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"C"u8, new @string[]{@"T"u8}.slice(), @"interface{chan<- T}"u8),
                new(@"C"u8, new @string[]{@"[]*P"u8}.slice(), @"interface{chan<- []*P}"u8),
                new(@"f"u8, new @string[]{@"int"u8, @"chan<- int"u8, @"chan<- []*chan<- int"u8}.slice(), @"func() []int"u8)
            }.slice()
        ),
        new(@"package i0; import ""lib""; func _() { lib.F(42) }"u8,
            new TestInstanceInfo_testInst[]{new(@"F"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)}.slice()
        ),
        new(@"package duplfunc0; func f[T any](T) {}; func _() { f(42); f(""foo""); f[int](3) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"f"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8),
                new(@"f"u8, new @string[]{@"string"u8}.slice(), @"func(string)"u8),
                new(@"f"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)
            }.slice()
        ),
        new(@"package duplfunc1; import ""lib""; func _() { lib.F(42); lib.F(""foo""); lib.F(3) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"F"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8),
                new(@"F"u8, new @string[]{@"string"u8}.slice(), @"func(string)"u8),
                new(@"F"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)
            }.slice()
        ),
        new(@"package type0; type T[P interface{~int}] struct{ x P }; var _ T[int]"u8,
            new TestInstanceInfo_testInst[]{new(@"T"u8, new @string[]{@"int"u8}.slice(), @"struct{x int}"u8)}.slice()
        ),
        new(@"package type1; type T[P interface{~int}] struct{ x P }; var _ (T[int])"u8,
            new TestInstanceInfo_testInst[]{new(@"T"u8, new @string[]{@"int"u8}.slice(), @"struct{x int}"u8)}.slice()
        ),
        new(@"package type2; type T[P interface{~int}] struct{ x P }; var _ T[(int)]"u8,
            new TestInstanceInfo_testInst[]{new(@"T"u8, new @string[]{@"int"u8}.slice(), @"struct{x int}"u8)}.slice()
        ),
        new(@"package type3; type T[P1 interface{~[]P2}, P2 any] struct{ x P1; y P2 }; var _ T[[]int, int]"u8,
            new TestInstanceInfo_testInst[]{new(@"T"u8, new @string[]{@"[]int"u8, @"int"u8}.slice(), @"struct{x []int; y int}"u8)}.slice()
        ),
        new(@"package type4; import ""lib""; var _ lib.T[int]"u8,
            new TestInstanceInfo_testInst[]{new(@"T"u8, new @string[]{@"int"u8}.slice(), @"[]int"u8)}.slice()
        ),
        new(@"package dupltype0; type T[P interface{~int}] struct{ x P }; var x T[int]; var y T[int]"u8,
            new TestInstanceInfo_testInst[]{
                new(@"T"u8, new @string[]{@"int"u8}.slice(), @"struct{x int}"u8),
                new(@"T"u8, new @string[]{@"int"u8}.slice(), @"struct{x int}"u8)
            }.slice()
        ),
        new(@"package dupltype1; type T[P ~int] struct{ x P }; func (r *T[Q]) add(z T[Q]) { r.x += z.x }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"T"u8, new @string[]{@"Q"u8}.slice(), @"struct{x Q}"u8),
                new(@"T"u8, new @string[]{@"Q"u8}.slice(), @"struct{x Q}"u8)
            }.slice()
        ),
        new(@"package dupltype1; import ""lib""; var x lib.T[int]; var y lib.T[int]; var z lib.T[string]"u8,
            new TestInstanceInfo_testInst[]{
                new(@"T"u8, new @string[]{@"int"u8}.slice(), @"[]int"u8),
                new(@"T"u8, new @string[]{@"int"u8}.slice(), @"[]int"u8),
                new(@"T"u8, new @string[]{@"string"u8}.slice(), @"[]string"u8)
            }.slice()
        ),
        new(@"package issue51803; func foo[T any](T) {}; func _() { foo[int]( /* leave arg away on purpose */ ) }"u8,
            new TestInstanceInfo_testInst[]{new(@"foo"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)}.slice()
        ),
        new(@"package reverse1a; var f func(int) = g; func g[P any](P) {}"u8,
            new TestInstanceInfo_testInst[]{new(@"g"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)}.slice()
        ),
        new(@"package reverse1b; func f(func(int)) {}; func g[P any](P) {}; func _() { f(g) }"u8,
            new TestInstanceInfo_testInst[]{new(@"g"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8)}.slice()
        ),
        new(@"package reverse2a; var f func(int, string) = g; func g[P, Q any](P, Q) {}"u8,
            new TestInstanceInfo_testInst[]{new(@"g"u8, new @string[]{@"int"u8, @"string"u8}.slice(), @"func(int, string)"u8)}.slice()
        ),
        new(@"package reverse2b; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}; func _() { f(g) }"u8,
            new TestInstanceInfo_testInst[]{new(@"g"u8, new @string[]{@"int"u8, @"string"u8}.slice(), @"func(int, string)"u8)}.slice()
        ),
        new(@"package reverse2c; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}; func _() { f(g[int]) }"u8,
            new TestInstanceInfo_testInst[]{new(@"g"u8, new @string[]{@"int"u8, @"string"u8}.slice(), @"func(int, string)"u8)}.slice()
        ),
        new(@"package reverse3b; func f[R any](func(int) R) {}; func g[P any](P) string { return """" }; func _() { f(g) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"f"u8, new @string[]{@"string"u8}.slice(), @"func(func(int) string)"u8),
                new(@"g"u8, new @string[]{@"int"u8}.slice(), @"func(int) string"u8)
            }.slice()
        ),
        new(@"package reverse4a; var _, _ func([]int, *float32) = g, h; func g[P, Q any]([]P, *Q) {}; func h[R any]([]R, *float32) {}"u8,
            new TestInstanceInfo_testInst[]{
                new(@"g"u8, new @string[]{@"int"u8, @"float32"u8}.slice(), @"func([]int, *float32)"u8),
                new(@"h"u8, new @string[]{@"int"u8}.slice(), @"func([]int, *float32)"u8)
            }.slice()
        ),
        new(@"package reverse4b; func f(_, _ func([]int, *float32)) {}; func g[P, Q any]([]P, *Q) {}; func h[R any]([]R, *float32) {}; func _() { f(g, h) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"g"u8, new @string[]{@"int"u8, @"float32"u8}.slice(), @"func([]int, *float32)"u8),
                new(@"h"u8, new @string[]{@"int"u8}.slice(), @"func([]int, *float32)"u8)
            }.slice()
        ),
        new(@"package issue59956; func f(func(int), func(string), func(bool)) {}; func g[P any](P) {}; func _() { f(g, g, g) }"u8,
            new TestInstanceInfo_testInst[]{
                new(@"g"u8, new @string[]{@"int"u8}.slice(), @"func(int)"u8),
                new(@"g"u8, new @string[]{@"string"u8}.slice(), @"func(string)"u8),
                new(@"g"u8, new @string[]{@"bool"u8}.slice(), @"func(bool)"u8)
            }.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestInstanceInfo_type(), out var Ꮡtest);
        test = vᴛ1;

        var imports = new testImporter(0);
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(Importer: imports);
        var instMap = new map<ж<ast.Ident>, types.Instance>();
        var useMap = new map<ж<ast.Ident>, types.Object>();
        var importsʗ1 = imports;
        var instMapʗ1 = instMap;
        var useMapʗ1 = useMap;
        ж<types.Package> makePkg(@string src) {
            var (pkgΔ1, err) = typecheck(src, Ꮡconf, Ꮡ(new typesꓸInfo(Instances: instMapʗ1, Uses: useMapʗ1)));
            // allow error for issue51803
            if (err != default! && (pkgΔ1 == nil || pkgΔ1.Name() != "issue51803"u8)) {
                Ꮡt.Fatal(err);
            }
            importsʗ1[pkgΔ1.Name()] = pkgΔ1;
            return pkgΔ1;
        }
        makePkg(lib);
        var pkg = makePkg(test.src);
        var instMapʗ2 = instMap;
        var pkgʗ1 = pkg;
        var testʗ1 = test;
        var useMapʗ2 = useMap;
        Ꮡt.Run(pkg.Name(), (ж<testing.T> tΔ1) => {
            // Sort instances in source order for stability.
            var instances = sortedInstances(instMapʗ2);
            {
                nint got = len(instances);
                nint want = len(testʗ1.instances); if (got != want) {
                    tΔ1.Fatalf("got %d instances, want %d"u8, got, want);
                }
            }
            // Pairwise compare with the expected instances.
            foreach (var (ii, inst) in instances) {
                slice<typesꓸType> targs = default!;
                for (nint i = 0; i < inst.Inst.TypeArgs.Len(); i++) {
                    targs = append(targs, inst.Inst.TypeArgs.At(i));
                }
                var typ = inst.Inst.Type;
                var testInst = testʗ1.instances[ii];
                {
                    @string got = inst.Ident.Value.Name; if (got != testInst.name) {
                        tΔ1.Fatalf("got name %s, want %s"u8, got, testInst.name);
                    }
                }
                if (len(targs) != len(testInst.targs)) {
                    tΔ1.Fatalf("got %d type arguments; want %d"u8, len(targs), len(testInst.targs));
                }
                foreach (var (i, targ) in targs) {
                    {
                        @string got = targ.String(); if (got != testInst.targs[i]) {
                            tΔ1.Errorf("type argument %d: got %s; want %s"u8, i, got, testInst.targs[i]);
                        }
                    }
                }
                {
                    @string got = typ.Underlying().String(); if (got != testInst.typ) {
                        tΔ1.Errorf("package %s: got %s; want %s"u8, pkgʗ1.Name(), got, testInst.typ);
                    }
                }
                // Verify the invariant that re-instantiating the corresponding generic
                // type with TypeArgs results in an identical instance.
                var ptype = useMapʗ2[inst.Ident].Type();
                var (lister, _) = ptype._<TestInstanceInfo_typeᴛ1>(ᐧ);
                if (lister == default! || lister.TypeParams().Len() == 0) {
                    tΔ1.Fatalf("info.Types[%v] = %v, want parameterized type"u8, inst.Ident.OrTypedNil(), ptype);
                }
                var (inst2, err) = Instantiate(nil, ptype, targs, true);
                if (err != default!) {
                    tΔ1.Errorf("Instantiate(%v, %v) failed: %v"u8, ptype, targs, err);
                }
                if (!Identical(inst.Inst.Type, inst2)) {
                    tΔ1.Errorf("%v and %v are not identical"u8, inst.Inst.Type, inst2);
                }
            }
        });
    }
}

[GoType] partial struct recordedInstance {
    public ж<ast.Ident> Ident;
    public types.Instance Inst;
}

internal static slice<recordedInstance> /*instances*/ sortedInstances(map<ж<ast.Ident>, types.Instance> m) {
    slice<recordedInstance> instances = default!;

    foreach (var (id, inst) in m) {
        instances = append(instances, new recordedInstance(id, inst));
    }
    slices.SortFunc(instances, (recordedInstance a, recordedInstance b) => types_internal_test_package.CmpPos(a.Ident.Pos(), b.Ident.Pos()));
    return instances;
}

[GoType("dyn")] internal partial struct TestDefsInfo_type {
    internal @string src;
    internal @string obj;
    internal @string want;
}

public static void TestDefsInfo(ж<testing.T> Ꮡt) {
// Tests using generics.
// Type parameters in receiver type expressions are definitions.
    slice<TestDefsInfo_type> tests = new TestDefsInfo_type[]{
        new(@"package p0; const x = 42"u8, @"x"u8, @"const p0.x untyped int"u8),
        new(@"package p1; const x int = 42"u8, @"x"u8, @"const p1.x int"u8),
        new(@"package p2; var x int"u8, @"x"u8, @"var p2.x int"u8),
        new(@"package p3; type x int"u8, @"x"u8, @"type p3.x int"u8),
        new(@"package p4; func f()"u8, @"f"u8, @"func p4.f()"u8),
        new(@"package p5; func f() int { x, _ := 1, 2; return x }"u8, @"_"u8, @"var _ int"u8),
        new(@"package g0; type x[T any] int"u8, @"x"u8, @"type g0.x[T any] int"u8),
        new(@"package g1; func f[T any]() {}"u8, @"f"u8, @"func g1.f[T any]()"u8),
        new(@"package g2; type x[T any] int; func (*x[_]) m() {}"u8, @"m"u8, @"func (*g2.x[_]).m()"u8),
        new(@"package r0; type T[_ any] int; func (T[P]) _() {}"u8, @"P"u8, @"type parameter P any"u8),
        new(@"package r1; type T[_, _ any] int; func (T[P, Q]) _() {}"u8, @"P"u8, @"type parameter P any"u8),
        new(@"package r2; type T[_, _ any] int; func (T[P, Q]) _() {}"u8, @"Q"u8, @"type parameter Q any"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(
            Defs: new map<ж<ast.Ident>, types.Object>()
        );
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // find object
        types.Object def = default!;
        foreach (var (id, obj) in info.Defs) {
            if ((~id).Name == test.obj) {
                def = obj;
                break;
            }
        }
        if (def == default!) {
            Ꮡt.Errorf("package %s: %s not found"u8, name, test.obj);
            continue;
        }
        {
            @string got = def.String(); if (got != test.want) {
                Ꮡt.Errorf("package %s: got %s; want %s"u8, name, got, test.want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestUsesInfo_type {
    internal @string src;
    internal @string obj;
    internal @string want;
}

public static void TestUsesInfo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// Tests using generics.
// Uses of fields are instantiated.
// Uses of methods are uses of the instantiated method.
// For historic reasons, type parameters in receiver type expressions
// are considered both definitions and uses (see go.dev/issue/68670).
    slice<TestUsesInfo_type> tests = new TestUsesInfo_type[]{
        new(@"package p0; func _() { _ = x }; const x = 42"u8, @"x"u8, @"const p0.x untyped int"u8),
        new(@"package p1; func _() { _ = x }; const x int = 42"u8, @"x"u8, @"const p1.x int"u8),
        new(@"package p2; func _() { _ = x }; var x int"u8, @"x"u8, @"var p2.x int"u8),
        new(@"package p3; func _() { type _ x }; type x int"u8, @"x"u8, @"type p3.x int"u8),
        new(@"package p4; func _() { _ = f }; func f()"u8, @"f"u8, @"func p4.f()"u8),
        new(@"package g0; func _[T any]() { _ = x }; const x = 42"u8, @"x"u8, @"const g0.x untyped int"u8),
        new(@"package g1; func _[T any](x T) { }"u8, @"T"u8, @"type parameter T any"u8),
        new(@"package g2; type N[A any] int; var _ N[int]"u8, @"N"u8, @"type g2.N[A any] int"u8),
        new(@"package g3; type N[A any] int; func (N[_]) m() {}"u8, @"N"u8, @"type g3.N[A any] int"u8),
        new(@"package s1; type N[A any] struct{ a A }; var f = N[int]{}.a"u8, @"a"u8, @"field a int"u8),
        new(@"package s2; type N[A any] struct{ a A }; func (r N[B]) m(b B) { r.a = b }"u8, @"a"u8, @"field a B"u8),
        new(@"package m0; type N[A any] int; func (r N[B]) m() { r.n() }; func (N[C]) n() {}"u8, @"n"u8, @"func (m0.N[B]).n()"u8),
        new(@"package m1; type N[A any] int; func (r N[B]) m() { }; var f = N[int].m"u8, @"m"u8, @"func (m1.N[int]).m()"u8),
        new(@"package m2; func _[A any](v interface{ m() A }) { v.m() }"u8, @"m"u8, @"func (interface).m() A"u8),
        new(@"package m3; func f[A any]() interface{ m() A } { return nil }; var _ = f[int]().m()"u8, @"m"u8, @"func (interface).m() int"u8),
        new(@"package m4; type T[A any] func() interface{ m() A }; var x T[int]; var y = x().m"u8, @"m"u8, @"func (interface).m() int"u8),
        new(@"package m5; type T[A any] interface{ m() A }; func _[B any](t T[B]) { t.m() }"u8, @"m"u8, @"func (m5.T[B]).m() B"u8),
        new(@"package m6; type T[A any] interface{ m() }; func _[B any](t T[B]) { t.m() }"u8, @"m"u8, @"func (m6.T[B]).m()"u8),
        new(@"package m7; type T[A any] interface{ m() A }; func _(t T[int]) { t.m() }"u8, @"m"u8, @"func (m7.T[int]).m() int"u8),
        new(@"package m8; type T[A any] interface{ m() }; func _(t T[int]) { t.m() }"u8, @"m"u8, @"func (m8.T[int]).m()"u8),
        new(@"package m9; type T[A any] interface{ m() }; func _(t T[int]) { _ = t.m }"u8, @"m"u8, @"func (m9.T[int]).m()"u8),
        new(
            @"package m10; type E[A any] interface{ m() }; type T[B any] interface{ E[B]; n() }; func _(t T[int]) { t.m() }"u8,
            @"m"u8,
            @"func (m10.E[int]).m()"u8
        ),
        new(@"package m11; type T[A any] interface{ m(); n() }; func _(t1 T[int], t2 T[string]) { t1.m(); t2.n() }"u8, @"m"u8, @"func (m11.T[int]).m()"u8),
        new(@"package m12; type T[A any] interface{ m(); n() }; func _(t1 T[int], t2 T[string]) { t1.m(); t2.n() }"u8, @"n"u8, @"func (m12.T[string]).n()"u8),
        new(@"package r0; type T[_ any] int; func (T[P]) _() {}"u8, @"P"u8, @"type parameter P any"u8),
        new(@"package r1; type T[_, _ any] int; func (T[P, Q]) _() {}"u8, @"P"u8, @"type parameter P any"u8),
        new(@"package r2; type T[_, _ any] int; func (T[P, Q]) _() {}"u8, @"Q"u8, @"type parameter Q any"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(
            Uses: new map<ж<ast.Ident>, types.Object>()
        );
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // find object
        types.Object use = default!;
        foreach (var (id, obj) in info.Uses) {
            if ((~id).Name == test.obj) {
                if (use != default!) {
                    throw panic(fmt.Sprintf("multiple uses of %q"u8, (~id).Name));
                }
                use = obj;
            }
        }
        if (use == default!) {
            Ꮡt.Errorf("package %s: %s not found"u8, name, test.obj);
            continue;
        }
        {
            @string got = use.String(); if (got != test.want) {
                Ꮡt.Errorf("package %s: got %s; want %s"u8, name, got, test.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeNAAnyIntFuncˢ = """
package p

type N[A any] int

func (r N[B]) m() { r.m(); r.n() }

func (r *N[C]) n() {  }

"""u8;

public static void TestGenericMethodInfo(ж<testing.T> Ꮡt) {
    @string src = packagePTypeNAAnyIntFuncˢ;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
    info = new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Uses: new map<ж<ast.Ident>, types.Object>(),
        Selections: new map<ж<ast.SelectorExpr>, ж<types.Selection>>()
    );
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check("p"u8, fset, new ж<ast.File>[]{f}.slice(), Ꮡinfo);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var N = pkg.Scope().Lookup("N"u8).Type()._<ж<types.Named>>();
    // Find the generic methods stored on N.
    var (gm, gn) = (N.Method(0), N.Method(1));
    if (gm.Name() == "n"u8) {
        (gm, gn) = (gn, gm);
    }
    // Collect objects from info.
    ж<types.Func> dm = default!;                      // the declared methods
    ж<types.Func> dn = default!;
    ref var dmm = ref heap<ж<types.Func>>(out var Ꮡdmm);                    // the methods used in the body of m
    ref var dmn = ref heap<ж<types.Func>>(out var Ꮡdmn);
    foreach (var (_, decl) in (~f).Decls) {
        var (fdecl, ok) = decl._<ж<ast.FuncDecl>>(ᐧ);
        if (!ok) {
            continue;
        }
        var def = info.Defs[(~fdecl).Name]._<ж<types.Func>>();
        var exprᴛ1 = (~(~fdecl).Name).Name;
        if (exprᴛ1 == "m"u8) {
            dm = def;
            ast.Inspect(new ast.BlockStmtжNode((~fdecl).Body), (ast.Node n) => {
                {
                    var (call, okΔ2) = n._<ж<ast.CallExpr>>(ᐧ); if (okΔ2) {
                        var sel = (~call).Fun._<ж<ast.SelectorExpr>>();
                        var use = Ꮡinfo.Value.Uses[(~sel).Sel]._<ж<types.Func>>();
                        var selection = Ꮡinfo.Value.Selections[sel];
                        if (selection.Kind() != MethodVal) {
                            Ꮡt.Errorf("Selection kind = %v, want %v"u8, selection.Kind(), MethodVal);
                        }
                        if (!AreEqual(selection.Obj(), use)) {
                            Ꮡt.Errorf("info.Selections contains %v, want %v"u8, selection.Obj(), use.OrTypedNil());
                        }
                        var exprᴛ2 = (~(~sel).Sel).Name;
                        if (exprᴛ2 == "m"u8) {
                            Ꮡdmm.ValueSlot = use;
                        }
                        else if (exprᴛ2 == "n"u8) {
                            Ꮡdmn.ValueSlot = use;
                        }

                    }
                }
                return true;
            });
        }
        else if (exprᴛ1 == "n"u8) {
            dn = def;
        }

    }
    if (gm != dm) {
        Ꮡt.Errorf(@"N.Method(...) returns %v for ""m"", but Info.Defs has %v"u8, gm.OrTypedNil(), dm.OrTypedNil());
    }
    if (gn != dn) {
        Ꮡt.Errorf(@"N.Method(...) returns %v for ""m"", but Info.Defs has %v"u8, gm.OrTypedNil(), dm.OrTypedNil());
    }
    if (dmm != dm) {
        Ꮡt.Errorf(@"Inside ""m"", r.m uses %v, want the defined func %v"u8, dmm.OrTypedNil(), dm.OrTypedNil());
    }
    if (dmn == dn) {
        Ꮡt.Errorf(@"Inside ""m"", r.n uses %v, want a func distinct from %v"u8, dmm.OrTypedNil(), dm.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string importSpecˢ = "importSpec"u8;
internal static readonly @string caseClauseˢ = "caseClause"u8;
internal static readonly @string fieldˢ = "field"u8;

[GoType("dyn")] internal partial struct TestImplicitsInfo_type {
    internal @string src;
    internal @string want;
}

public static void TestImplicitsInfo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
// no Implicits entry
// no Implicits entry
// no Implicits entry
// no Implicits entry
// Tests using generics.
// no Implicits entry
    slice<TestImplicitsInfo_type> tests = new TestImplicitsInfo_type[]{
        new(@"package p2; import . ""fmt""; var _ = Println"u8, ""u8),
        new(@"package p0; import local ""fmt""; var _ = local.Println"u8, ""u8),
        new(@"package p1; import ""fmt""; var _ = fmt.Println"u8, "importSpec: package fmt"u8),
        new(@"package p3; func f(x interface{}) { switch x.(type) { case int: } }"u8, ""u8),
        new(@"package p4; func f(x interface{}) { switch t := x.(type) { case int: _ = t } }"u8, "caseClause: var t int"u8),
        new(@"package p5; func f(x interface{}) { switch t := x.(type) { case int, uint: _ = t } }"u8, "caseClause: var t interface{}"u8),
        new(@"package p6; func f(x interface{}) { switch t := x.(type) { default: _ = t } }"u8, "caseClause: var t interface{}"u8),
        new(@"package p7; func f(x int) {}"u8, ""u8),
        new(@"package p8; func f(int) {}"u8, "field: var  int"u8),
        new(@"package p9; func f() (complex64) { return 0 }"u8, "field: var  complex64"u8),
        new(@"package p10; type T struct{}; func (*T) f() {}"u8, "field: var  *p10.T"u8),
        new(@"package f0; func f[T any](x int) {}"u8, ""u8),
        new(@"package f1; func f[T any](int) {}"u8, "field: var  int"u8),
        new(@"package f2; func f[T any](T) {}"u8, "field: var  T"u8),
        new(@"package f3; func f[T any]() (complex64) { return 0 }"u8, "field: var  complex64"u8),
        new(@"package f4; func f[T any](t T) (T) { return t }"u8, "field: var  T"u8),
        new(@"package t0; type T[A any] struct{}; func (*T[_]) f() {}"u8, "field: var  *t0.T[_]"u8),
        new(@"package t1; type T[A any] struct{}; func _(x interface{}) { switch t := x.(type) { case T[int]: _ = t } }"u8, "caseClause: var t t1.T[int]"u8),
        new(@"package t2; type T[A any] struct{}; func _[P any](x interface{}) { switch t := x.(type) { case T[P]: _ = t } }"u8, "caseClause: var t t2.T[P]"u8),
        new(@"package t3; func _[P any](x interface{}) { switch t := x.(type) { case P: _ = t } }"u8, "caseClause: var t P"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(
            Implicits: new map<ast.Node, types.Object>()
        );
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // the test cases expect at most one Implicits entry
        if (len(info.Implicits) > 1) {
            Ꮡt.Errorf("package %s: %d Implicits entries found"u8, name, len(info.Implicits));
            continue;
        }
        // extract Implicits entry, if any
        @string got = default!;
        foreach (var (n, obj) in info.Implicits) {
            switch (n.type()) {
            case ж<ast.ImportSpec> x: {
                got = importSpecˢ;
                break;
            }
            case ж<ast.CaseClause> x: {
                got = caseClauseˢ;
                break;
            }
            case ж<ast.Field> x: {
                got = fieldˢ;
                break;
            }
            default: {
                var x = n;
                Ꮡt.Fatalf("package %s: unexpected %T"u8, name, x);
                break;
            }}
            got += ": "u8 + obj.String();
        }
        // verify entry
        if (got != test.want) {
            Ꮡt.Errorf("package %s: got %q; want %q"u8, name, got, test.want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = """

package p

import (
	. "os"
	_ "io"
	"math"
	"path/filepath"
	snort "sort"
)

// avoid imported and not used errors
var (
	_ = Open // os.Open
	_ = math.Sin
	_ = filepath.Abs
	_ = snort.Ints
)

"""u8;

[GoType("dyn")] internal partial struct TestPkgNameOf_type {
    internal @string path; // path string enclosed in "'s
    internal @string want;
}

public static void TestPkgNameOf(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    @string src = srcᶜ;
    slice<TestPkgNameOf_type> tests = new TestPkgNameOf_type[]{
        new(@"""os"""u8, "."u8),
        new(@"""io"""u8, "_"u8),
        new(@"""math"""u8, "math"u8),
        new(@"""path/filepath"""u8, "filepath"u8),
        new(@"""sort"""u8, "snort"u8)
    }.slice();
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
    info = new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Implicits: new map<ast.Node, types.Object>()
    );
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    conf.Importer = defaultImporter(fset);
    var (_, err) = Ꮡconf.Check("p"u8, fset, new ж<ast.File>[]{f}.slice(), Ꮡinfo);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // map import paths to importDecl
    var imports = new map<@string, ж<ast.ImportSpec>>();
    foreach (var (_, s) in (~(~f).Decls[0]._<ж<ast.GenDecl>>()).Specs) {
        {
            var (imp, _) = s._<ж<ast.ImportSpec>>(ᐧ); if (imp != nil) {
                imports[(~(~imp).Path).Value] = imp;
            }
        }
    }
    foreach (var (_, test) in tests) {
        var imp = imports[test.path];
        if (imp == nil) {
            Ꮡt.Fatalf("invalid test case: import path %s not found"u8, test.path);
        }
        var got = info.PkgNameOf(imp);
        if (got == nil) {
            Ꮡt.Fatalf("import %s: package name not found"u8, test.path);
        }
        if (got.Name() != test.want) {
            Ꮡt.Errorf("import %s: got %s; want %s"u8, test.path, got.Name(), test.want);
        }
    }
    // test non-existing importDecl
    {
        var got = info.PkgNameOf(@new<ast.ImportSpec>()); if (got != nil) {
            Ꮡt.Errorf("got %s for non-existing import declaration"u8, got.Name());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string voidˢ = "void"u8;
internal static readonly @string typeˢ = "type"u8;
internal static readonly @string builtinˢ = "builtin"u8;
internal static readonly @string constˢ = "const"u8;
internal static readonly @string valueˢ = "value"u8;
internal static readonly @string nilˢ = "nil"u8;
internal static readonly @string addressableˢ = "addressable"u8;
internal static readonly @string assignableˢ = "assignable"u8;
internal static readonly @string hasOkˢ = "hasOk"u8;
internal static readonly @string invalidˢ = "invalid"u8;

internal static @string predString(types.TypeAndValue tv) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    void pred(bool b, @string s) {
        if (b) {
            if (Ꮡbuf.Value.Len() > 0) {
                Ꮡbuf.WriteString(", "u8);
            }
            Ꮡbuf.WriteString(s);
        }
    }
    pred(tv.IsVoid(), voidˢ);
    pred(tv.IsType(), typeˢ);
    pred(tv.IsBuiltin(), builtinˢ);
    pred(tv.IsValue() && tv.Value != default!, constˢ);
    pred(tv.IsValue() && tv.Value == default!, valueˢ);
    pred(tv.IsNil(), nilˢ);
    pred(tv.Addressable(), addressableˢ);
    pred(tv.Assignable(), assignableˢ);
    pred(tv.HasOk(), hasOkˢ);
    if (buf.Len() == 0) {
        return invalidˢ;
    }
    return buf.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingˢ = "<missing>"u8;

[GoType("dyn")] internal partial struct TestPredicatesInfo_type {
    internal @string src;
    internal @string expr;
    internal @string pred;
}

public static void TestPredicatesInfo(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
// void
// types
// built-ins
// constants
// values
// addressable (and thus assignable) operands
// composite literals are not addressable
// assignable but not addressable values
// hasOk expressions
// missing entries
// - package names are collected in the Uses map
// - identifiers being declared are collected in the Defs map
    slice<TestPredicatesInfo_type> tests = new TestPredicatesInfo_type[]{
        new(@"package n0; func f() { f() }"u8, @"f()"u8, @"void"u8),
        new(@"package t0; type _ int"u8, @"int"u8, @"type"u8),
        new(@"package t1; type _ []int"u8, @"[]int"u8, @"type"u8),
        new(@"package t2; type _ func()"u8, @"func()"u8, @"type"u8),
        new(@"package t3; type _ func(int)"u8, @"int"u8, @"type"u8),
        new(@"package t3; type _ func(...int)"u8, @"...int"u8, @"type"u8),
        new(@"package b0; var _ = len("""")"u8, @"len"u8, @"builtin"u8),
        new(@"package b1; var _ = (len)("""")"u8, @"(len)"u8, @"builtin"u8),
        new(@"package c0; var _ = 42"u8, @"42"u8, @"const"u8),
        new(@"package c1; var _ = ""foo"" + ""bar"""u8, @"""foo"" + ""bar"""u8, @"const"u8),
        new(@"package c2; const (i = 1i; _ = i)"u8, @"i"u8, @"const"u8),
        new(@"package v0; var (a, b int; _ = a + b)"u8, @"a + b"u8, @"value"u8),
        new(@"package v1; var _ = &[]int{1}"u8, @"[]int{…}"u8, @"value"u8),
        new(@"package v2; var _ = func(){}"u8, @"(func() literal)"u8, @"value"u8),
        new(@"package v4; func f() { _ = f }"u8, @"f"u8, @"value"u8),
        new(@"package v3; var _ *int = nil"u8, @"nil"u8, @"value, nil"u8),
        new(@"package v3; var _ *int = (nil)"u8, @"(nil)"u8, @"value, nil"u8),
        new(@"package a0; var (x int; _ = x)"u8, @"x"u8, @"value, addressable, assignable"u8),
        new(@"package a1; var (p *int; _ = *p)"u8, @"*p"u8, @"value, addressable, assignable"u8),
        new(@"package a2; var (s []int; _ = s[0])"u8, @"s[0]"u8, @"value, addressable, assignable"u8),
        new(@"package a3; var (s struct{f int}; _ = s.f)"u8, @"s.f"u8, @"value, addressable, assignable"u8),
        new(@"package a4; var (a [10]int; _ = a[0])"u8, @"a[0]"u8, @"value, addressable, assignable"u8),
        new(@"package a5; func _(x int) { _ = x }"u8, @"x"u8, @"value, addressable, assignable"u8),
        new(@"package a6; func _()(x int) { _ = x; return }"u8, @"x"u8, @"value, addressable, assignable"u8),
        new(@"package a7; type T int; func (x T) _() { _ = x }"u8, @"x"u8, @"value, addressable, assignable"u8),
        new(@"package s0; var (m map[int]int; _ = m[0])"u8, @"m[0]"u8, @"value, assignable, hasOk"u8),
        new(@"package s1; var (m map[int]int; _, _ = m[0])"u8, @"m[0]"u8, @"value, assignable, hasOk"u8),
        new(@"package k0; var (ch chan int; _ = <-ch)"u8, @"<-ch"u8, @"value, hasOk"u8),
        new(@"package k1; var (ch chan int; _, _ = <-ch)"u8, @"<-ch"u8, @"value, hasOk"u8),
        new(@"package m0; import ""os""; func _() { _ = os.Stdout }"u8, @"os"u8, @"<missing>"u8),
        new(@"package m1; import p ""os""; func _() { _ = p.Stdout }"u8, @"p"u8, @"<missing>"u8),
        new(@"package m2; const c = 0"u8, @"c"u8, @"<missing>"u8),
        new(@"package m3; type T int"u8, @"T"u8, @"<missing>"u8),
        new(@"package m4; var v int"u8, @"v"u8, @"<missing>"u8),
        new(@"package m5; func f() {}"u8, @"f"u8, @"<missing>"u8),
        new(@"package m6; func _(x int) {}"u8, @"x"u8, @"<missing>"u8),
        new(@"package m6; func _()(x int) { return }"u8, @"x"u8, @"<missing>"u8),
        new(@"package m6; type T int; func (x T) _() {}"u8, @"x"u8, @"<missing>"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>());
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // look for expression predicates
        @string got = missingˢ;
        foreach (var (e, tv) in info.Types) {
            //println(name, ExprString(e))
            if (ExprString(e) == test.expr) {
                got = predString(tv);
                break;
            }
        }
        if (got != test.pred) {
            Ꮡt.Errorf("package %s: got %s; want %s"u8, name, got, test.pred);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownNodeKindˢ = "<unknown node kind>"u8;
internal static readonly @string fileˢ = "file"u8;
internal static readonly @string funcˢ = "func"u8;
internal static readonly @string blockˢ = "block"u8;
internal static readonly @string switchˢ = "switch"u8;
internal static readonly @string typeSwitchˢ = "type switch"u8;
internal static readonly @string caseˢ = "case"u8;
internal static readonly @string commˢ = "comm"u8;
internal static readonly @string forˢ = "for"u8;
internal static readonly @string rangeˢ = "range"u8;

[GoType("dyn")] internal partial struct TestScopesInfo_type {
    internal @string src;
    internal slice<@string> scopes; // list of scope descriptors of the form kind:varlist
}

public static void TestScopesInfo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
// redeclaration of x
// x implicitly declared
    slice<TestScopesInfo_type> tests = new TestScopesInfo_type[]{
        new(@"package p0"u8, new @string[]{
            "file:"u8
        }.slice()),
        new(@"package p1; import ( ""fmt""; m ""math""; _ ""os"" ); var ( _ = fmt.Println; _ = m.Pi )"u8, new @string[]{
            "file:fmt m"u8
        }.slice()),
        new(@"package p2; func _() {}"u8, new @string[]{
            "file:"u8, "func:"u8
        }.slice()),
        new(@"package p3; func _(x, y int) {}"u8, new @string[]{
            "file:"u8, "func:x y"u8
        }.slice()),
        new(@"package p4; func _(x, y int) { x, z := 1, 2; _ = z }"u8, new @string[]{
            "file:"u8, "func:x y z"u8
        }.slice()),
        new(@"package p5; func _(x, y int) (u, _ int) { return }"u8, new @string[]{
            "file:"u8, "func:u x y"u8
        }.slice()),
        new(@"package p6; func _() { { var x int; _ = x } }"u8, new @string[]{
            "file:"u8, "func:"u8, "block:x"u8
        }.slice()),
        new(@"package p7; func _() { if true {} }"u8, new @string[]{
            "file:"u8, "func:"u8, "if:"u8, "block:"u8
        }.slice()),
        new(@"package p8; func _() { if x := 0; x < 0 { y := x; _ = y } }"u8, new @string[]{
            "file:"u8, "func:"u8, "if:x"u8, "block:y"u8
        }.slice()),
        new(@"package p9; func _() { switch x := 0; x {} }"u8, new @string[]{
            "file:"u8, "func:"u8, "switch:x"u8
        }.slice()),
        new(@"package p10; func _() { switch x := 0; x { case 1: y := x; _ = y; default: }}"u8, new @string[]{
            "file:"u8, "func:"u8, "switch:x"u8, "case:y"u8, "case:"u8
        }.slice()),
        new(@"package p11; func _(t interface{}) { switch t.(type) {} }"u8, new @string[]{
            "file:"u8, "func:t"u8, "type switch:"u8
        }.slice()),
        new(@"package p12; func _(t interface{}) { switch t := t; t.(type) {} }"u8, new @string[]{
            "file:"u8, "func:t"u8, "type switch:t"u8
        }.slice()),
        new(@"package p13; func _(t interface{}) { switch x := t.(type) { case int: _ = x } }"u8, new @string[]{
            "file:"u8, "func:t"u8, "type switch:"u8, "case:x"u8
        }.slice()),
        new(@"package p14; func _() { select{} }"u8, new @string[]{
            "file:"u8, "func:"u8
        }.slice()),
        new(@"package p15; func _(c chan int) { select{ case <-c: } }"u8, new @string[]{
            "file:"u8, "func:c"u8, "comm:"u8
        }.slice()),
        new(@"package p16; func _(c chan int) { select{ case i := <-c: x := i; _ = x} }"u8, new @string[]{
            "file:"u8, "func:c"u8, "comm:i x"u8
        }.slice()),
        new(@"package p17; func _() { for{} }"u8, new @string[]{
            "file:"u8, "func:"u8, "for:"u8, "block:"u8
        }.slice()),
        new(@"package p18; func _(n int) { for i := 0; i < n; i++ { _ = i } }"u8, new @string[]{
            "file:"u8, "func:n"u8, "for:i"u8, "block:"u8
        }.slice()),
        new(@"package p19; func _(a []int) { for i := range a { _ = i} }"u8, new @string[]{
            "file:"u8, "func:a"u8, "range:i"u8, "block:"u8
        }.slice()),
        new(@"package p20; var s int; func _(a []int) { for i, x := range a { s += x; _ = i } }"u8, new @string[]{
            "file:"u8, "func:a"u8, "range:i x"u8, "block:"u8
        }.slice())
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(Scopes: new map<ast.Node, ж<typesꓸScope>>());
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // number of scopes must match
        if (len(info.Scopes) != len(test.scopes)) {
            Ꮡt.Errorf("package %s: got %d scopes; want %d"u8, name, len(info.Scopes), len(test.scopes));
        }
        // scope descriptions must match
        foreach (var (node, scope) in info.Scopes) {
            @string kind = unknownNodeKindˢ;
            switch (node.type()) {
            case ж<ast.File>: {
                kind = fileˢ;
                break;
            }
            case ж<ast.FuncType>: {
                kind = funcˢ;
                break;
            }
            case ж<ast.BlockStmt>: {
                kind = blockˢ;
                break;
            }
            case ж<ast.IfStmt>: {
                kind = "if"u8;
                break;
            }
            case ж<ast.SwitchStmt>: {
                kind = switchˢ;
                break;
            }
            case ж<ast.TypeSwitchStmt>: {
                kind = typeSwitchˢ;
                break;
            }
            case ж<ast.CaseClause>: {
                kind = caseˢ;
                break;
            }
            case ж<ast.CommClause>: {
                kind = commˢ;
                break;
            }
            case ж<ast.ForStmt>: {
                kind = forˢ;
                break;
            }
            case ж<ast.RangeStmt>: {
                kind = rangeˢ;
                break;
            }}

            // look for matching scope description
            @string desc = kind + ":"u8 + strings.Join(scope.Names(), " "u8);
            if (!slices.Contains(test.scopes, desc)) {
                Ꮡt.Errorf("package %s: no matching scope found for %s"u8, name, desc);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestInitOrderInfo_type {
    internal @string src;
    internal slice<@string> inits;
}

public static void TestInitOrderInfo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// blank var
// blank var
// emit an initializer for n:1 initializations only once (not for each node
// on the lhs which may appear in different order in the dependency graph)
// test case from spec section on package initialization
// test case for go.dev/issue/7131
// test case for go.dev/issue/10709
// test case for go.dev/issue/10709: same as test before, but variable decls swapped
// another candidate possibly causing problems with go.dev/issue/10709
    slice<TestInitOrderInfo_type> tests = new TestInitOrderInfo_type[]{
        new(@"package p0; var (x = 1; y = x)"u8, new @string[]{
            "x = 1"u8, "y = x"u8
        }.slice()),
        new(@"package p1; var (a = 1; b = 2; c = 3)"u8, new @string[]{
            "a = 1"u8, "b = 2"u8, "c = 3"u8
        }.slice()),
        new(@"package p2; var (a, b, c = 1, 2, 3)"u8, new @string[]{
            "a = 1"u8, "b = 2"u8, "c = 3"u8
        }.slice()),
        new(@"package p3; var _ = f(); func f() int { return 1 }"u8, new @string[]{
            "_ = f()"u8
        }.slice()),
        new(@"package p4; var (a = 0; x = y; y = z; z = 0)"u8, new @string[]{
            "a = 0"u8, "z = 0"u8, "y = z"u8, "x = y"u8
        }.slice()),
        new(@"package p5; var (a, _ = m[0]; m map[int]string)"u8, new @string[]{
            "a, _ = m[0]"u8
        }.slice()),
        new(@"package p6; var a, b = f(); func f() (_, _ int) { return z, z }; var z = 0"u8, new @string[]{
            "z = 0"u8, "a, b = f()"u8
        }.slice()),
        new(@"package p7; var (a = func() int { return b }(); b = 1)"u8, new @string[]{
            "b = 1"u8, "a = (func() int literal)()"u8
        }.slice()),
        new(@"package p8; var (a, b = func() (_, _ int) { return c, c }(); c = 1)"u8, new @string[]{
            "c = 1"u8, "a, b = (func() (_, _ int) literal)()"u8
        }.slice()),
        new(@"package p9; type T struct{}; func (T) m() int { _ = y; return 0 }; var x, y = T.m, 1"u8, new @string[]{
            "y = 1"u8, "x = T.m"u8
        }.slice()),
        new(@"package p10; var (d = c + b; a = 0; b = 0; c = 0)"u8, new @string[]{
            "a = 0"u8, "b = 0"u8, "c = 0"u8, "d = c + b"u8
        }.slice()),
        new(@"package p11; var (a = e + c; b = d + c; c = 0; d = 0; e = 0)"u8, new @string[]{
            "c = 0"u8, "d = 0"u8, "b = d + c"u8, "e = 0"u8, "a = e + c"u8
        }.slice()),
        new(@"package p12; var (a = x; b = 0; x, y = m[0]; m map[int]int)"u8, new @string[]{
            "b = 0"u8, "x, y = m[0]"u8, "a = x"u8
        }.slice()),
        new("""
package p12

		var (
			a = c + b
			b = f()
			c = f()
			d = 3
		)

		func f() int {
			d++
			return d
		}
"""u8, new @string[]{
            "d = 3"u8, "b = f()"u8, "c = f()"u8, "a = c + b"u8
        }.slice()),
        new("""
package main

		var counter int
		func next() int { counter++; return counter }

		var _ = makeOrder()
		func makeOrder() []int { return []int{f, b, d, e, c, a} }

		var a       = next()
		var b, c    = next(), next()
		var d, e, f = next(), next(), next()
		
"""u8, new @string[]{
            "a = next()"u8, "b = next()"u8, "c = next()"u8, "d = next()"u8, "e = next()"u8, "f = next()"u8, "_ = makeOrder()"u8
        }.slice()),
        new("""
package p13

		var (
		    v = t.m()
		    t = makeT(0)
		)

		type T struct{}

		func (T) m() int { return 0 }

		func makeT(n int) T {
		    if n > 0 {
		        return makeT(n-1)
		    }
		    return T{}
		}
"""u8, new @string[]{
            "t = makeT(0)"u8, "v = t.m()"u8
        }.slice()),
        new("""
package p14

		var (
		    t = makeT(0)
		    v = t.m()
		)

		type T struct{}

		func (T) m() int { return 0 }

		func makeT(n int) T {
		    if n > 0 {
		        return makeT(n-1)
		    }
		    return T{}
		}
"""u8, new @string[]{
            "t = makeT(0)"u8, "v = t.m()"u8
        }.slice()),
        new("""
package p15

		var y1 = f1()

		func f1() int { return g1() }
		func g1() int { f1(); return x1 }

		var x1 = 0

		var y2 = f2()

		func f2() int { return g2() }
		func g2() int { return x2 }

		var x2 = 0
"""u8, new @string[]{
            "x1 = 0"u8, "y1 = f1()"u8, "x2 = 0"u8, "y2 = f2()"u8
        }.slice())
    }.slice();
    foreach (var (_, test) in tests) {
        ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
        info = new typesꓸInfo(nil);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        // number of initializers must match
        if (len(info.InitOrder) != len(test.inits)) {
            Ꮡt.Errorf("package %s: got %d initializers; want %d"u8, name, len(info.InitOrder), len(test.inits));
            continue;
        }
        // initializers must match
        foreach (var (i, want) in test.inits) {
            @string got = info.InitOrder[i].String();
            if (got != want) {
                Ꮡt.Errorf("package %s, init %d: got %s; want %s"u8, name, i, got, want);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageMainVarA1ˢ = @"package main; var a = 1"u8;
internal static readonly @string packageMainVarB2ˢ = @"package main; var b = 2"u8;
internal static readonly @string mainˢ = "main"u8;

[GoType("dyn")] internal partial struct TestMultiFileInitOrder_type {
    internal slice<ж<ast.File>> files;
    internal @string want;
}

public static void TestMultiFileInitOrder(ж<testing.T> Ꮡt) {
    var fset = token.NewFileSet();
    var fileA = mustParse(fset, packageMainVarA1ˢ);
    var fileB = mustParse(fset, packageMainVarB2ˢ);
    // The initialization order must not depend on the parse
    // order of the files, only on the presentation order to
    // the type-checker.
    foreach (var (_, test) in new TestMultiFileInitOrder_type[]{
        new(new ж<ast.File>[]{fileA, fileB}.slice(), "[a = 1 b = 2]"u8),
        new(new ж<ast.File>[]{fileB, fileA}.slice(), "[b = 2 a = 1]"u8)
    }.slice()) {
        ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
        {
            var (_, err) = @new<types.Config>().Check(mainˢ, fset, test.files, Ꮡinfo); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        {
            @string got = fmt.Sprint(info.InitOrder); if (got != test.want) {
                Ꮡt.Fatalf("got %s; want %s"u8, got, test.want);
            }
        }
    }
}

public static void TestFiles(ж<testing.T> Ꮡt) {
    slice<@string> sources = new @string[]{
        "package p; type T struct{}; func (T) m1() {}"u8,
        "package p; func (T) m2() {}; var x interface{ m1(); m2() } = T{}"u8,
        "package p; func (T) m3() {}; var y interface{ m1(); m2(); m3() } = T{}"u8,
        "package p"u8
    }.slice();
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var fset = token.NewFileSet();
    var pkg = NewPackage("p"u8, "p"u8);
    ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
    var check = NewChecker(Ꮡconf, fset, pkg, Ꮡinfo);
    foreach (var (_, src) in sources) {
        {
            var err = check.Files(new ж<ast.File>[]{mustParse(fset, src)}.slice()); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
    }
    // check InitOrder is [x y]
    slice<@string> vars = default!;
    foreach (var (_, init) in info.InitOrder) {
        foreach (var (_, v) in (~init).Lhs) {
            vars = append(vars, v.Name());
        }
    }
    {
        @string got = fmt.Sprint(vars);
        @string want = "[x y]"u8; if (got != want) {
            Ꮡt.Errorf("InitOrder == %s, want %s"u8, got, want);
        }
    }
}

[GoType("map[@string, ж<types.Package>]")] partial struct testImporter;

internal static (ж<types.Package>, error) Import(this testImporter m, @string path) {
    {
        var pkg = m[path]; if (pkg != nil) {
            return (pkg, default!);
        }
    }
    return (default!, fmt.Errorf("package %q not found"u8, path));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string libˢ = "lib"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string libSrcᶜ = """

package lib
type T float64
const C T = 3
var V T
func F() {}
func (T) M() {}

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string mainSrcᶜ = """

package main
import "lib"

type A struct {
	*B
	C
}

type B struct {
	b int
}

func (B) f(int)

type C struct {
	c int
}

type G[P any] struct {
	p P
}

func (G[P]) m(P) {}

var Inst G[int]

func (C) g()
func (*C) h()

func main() {
	// qualified identifiers
	var _ lib.T
	_ = lib.C
	_ = lib.F
	_ = lib.V
	_ = lib.T.M

	// fields
	_ = A{}.B
	_ = new(A).B

	_ = A{}.C
	_ = new(A).C

	_ = A{}.b
	_ = new(A).b

	_ = A{}.c
	_ = new(A).c

	_ = Inst.p
	_ = G[string]{}.p

	// methods
	_ = A{}.f
	_ = new(A).f
	_ = A{}.g
	_ = new(A).g
	_ = new(A).h

	_ = B{}.f
	_ = new(B).f

	_ = C{}.g
	_ = new(C).g
	_ = new(C).h
	_ = Inst.m

	// method expressions
	_ = A.f
	_ = (*A).f
	_ = B.f
	_ = (*B).f
	_ = G[string].m
}
"""u8;

public static void TestSelection(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var selections = new map<ж<ast.SelectorExpr>, ж<types.Selection>>();
    // We need a specific fileset in this test below for positions.
    // Cannot use typecheck helper.
    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: imports);
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    var selectionsʗ1 = selections;
    void makePkg(@string path, @string src) {
        var (pkg, err) = Ꮡconf.Check(path, fsetʗ1, new ж<ast.File>[]{mustParse(fsetʗ1, src)}.slice(), Ꮡ(new typesꓸInfo(Selections: selectionsʗ1)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        importsʗ1[path] = pkg;
    }
    @string libSrc = libSrcᶜ;
    @string mainSrc = mainSrcᶜ;
    var wantOut = new map<@string, array<@string>>{
        ["lib.T.M"u8] = new @string[]{"method expr (lib.T) M(lib.T)"u8, ".[0]"u8}.array(),
        ["A{}.B"u8] = new @string[]{"field (main.A) B *main.B"u8, ".[0]"u8}.array(),
        ["new(A).B"u8] = new @string[]{"field (*main.A) B *main.B"u8, "->[0]"u8}.array(),
        ["A{}.C"u8] = new @string[]{"field (main.A) C main.C"u8, ".[1]"u8}.array(),
        ["new(A).C"u8] = new @string[]{"field (*main.A) C main.C"u8, "->[1]"u8}.array(),
        ["A{}.b"u8] = new @string[]{"field (main.A) b int"u8, "->[0 0]"u8}.array(),
        ["new(A).b"u8] = new @string[]{"field (*main.A) b int"u8, "->[0 0]"u8}.array(),
        ["A{}.c"u8] = new @string[]{"field (main.A) c int"u8, ".[1 0]"u8}.array(),
        ["new(A).c"u8] = new @string[]{"field (*main.A) c int"u8, "->[1 0]"u8}.array(),
        ["Inst.p"u8] = new @string[]{"field (main.G[int]) p int"u8, ".[0]"u8}.array(),
        ["A{}.f"u8] = new @string[]{"method (main.A) f(int)"u8, "->[0 0]"u8}.array(),
        ["new(A).f"u8] = new @string[]{"method (*main.A) f(int)"u8, "->[0 0]"u8}.array(),
        ["A{}.g"u8] = new @string[]{"method (main.A) g()"u8, ".[1 0]"u8}.array(),
        ["new(A).g"u8] = new @string[]{"method (*main.A) g()"u8, "->[1 0]"u8}.array(),
        ["new(A).h"u8] = new @string[]{"method (*main.A) h()"u8, "->[1 1]"u8}.array(), // TODO(gri) should this report .[1 1] ?

        ["B{}.f"u8] = new @string[]{"method (main.B) f(int)"u8, ".[0]"u8}.array(),
        ["new(B).f"u8] = new @string[]{"method (*main.B) f(int)"u8, "->[0]"u8}.array(),
        ["C{}.g"u8] = new @string[]{"method (main.C) g()"u8, ".[0]"u8}.array(),
        ["new(C).g"u8] = new @string[]{"method (*main.C) g()"u8, "->[0]"u8}.array(),
        ["new(C).h"u8] = new @string[]{"method (*main.C) h()"u8, "->[1]"u8}.array(), // TODO(gri) should this report .[1] ?

        ["Inst.m"u8] = new @string[]{"method (main.G[int]) m(int)"u8, ".[0]"u8}.array(),
        ["A.f"u8] = new @string[]{"method expr (main.A) f(main.A, int)"u8, "->[0 0]"u8}.array(),
        ["(*A).f"u8] = new @string[]{"method expr (*main.A) f(*main.A, int)"u8, "->[0 0]"u8}.array(),
        ["B.f"u8] = new @string[]{"method expr (main.B) f(main.B, int)"u8, ".[0]"u8}.array(),
        ["(*B).f"u8] = new @string[]{"method expr (*main.B) f(*main.B, int)"u8, "->[0]"u8}.array(),
        ["G[string].m"u8] = new @string[]{"method expr (main.G[string]) m(main.G[string], string)"u8, ".[0]"u8}.array(),
        ["G[string]{}.p"u8] = new @string[]{"field (main.G[string]) p string"u8, ".[0]"u8}.array()
    };
    makePkg(libˢ, libSrc);
    makePkg(mainˢ, mainSrc);
    foreach (var (e, sel) in selections) {
        _ = sel.String(); // assertion: must not panic
        nint start = fset.Position(e.Pos()).Offset;
        nint end = fset.Position(e.End()).Offset;
        @string syntax = mainSrc.slice(start, end); // (all SelectorExprs are in main, not lib)
        @string direct = "."u8;
        if (sel.Indirect()) {
            direct = "->"u8;
        }
        var got = new @string[]{
            sel.String(),
            fmt.Sprintf("%s%v"u8, direct, sel.Index())
        }.array();
        var want = wantOut[syntax, () => new array<@string>(2)].Clone();
        if (want != got) {
            Ꮡt.Errorf("%s: got %q; want %q"u8, syntax, got, want);
        }
        delete(wantOut, syntax);
        // We must explicitly assert properties of the
        // Signature's receiver since it doesn't participate
        // in Identical() or String().
        var (sig, _) = sel.Type()._<ж<typesꓸSignature>>(ᐧ);
        if (sel.Kind() == MethodVal){
            var gotΔ1 = sig.Recv().Type();
            var wantΔ1 = sel.Recv();
            if (!Identical(gotΔ1, wantΔ1)) {
                Ꮡt.Errorf("%s: Recv() = %s, want %s"u8, syntax, gotΔ1, wantΔ1);
            }
        } else 
        if (sig != nil && sig.Recv() != nil) {
            Ꮡt.Errorf("%s: signature has receiver %s"u8, sig.OrTypedNil(), sig.Recv().Type());
        }
    }
    // Assert that all wantOut entries were used exactly once.
    foreach (var (syntax, _) in wantOut) {
        Ꮡt.Errorf("no ast.Selection found with syntax %q"u8, syntax);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string libSrcᶜ1 = """

package a
import "missing"
const C1 = foo
const C2 = missing.C

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string mainSrcᶜ1 = """

package main
import "a"
var _ = a.C1
var _ = a.C2

"""u8;

public static void TestIssue8518(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(
        Error: (error err) => {
            Ꮡt.Log(err); // don't exit after first error
        },
        Importer: imports
    );
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    void makePkg(@string path, @string src) {
        (importsʗ1[path], _) = Ꮡconf.Check(path, fsetʗ1, new ж<ast.File>[]{mustParse(fsetʗ1, src)}.slice(), nil); // errors logged via conf.Error
    }
    @string libSrc = libSrcᶜ1;
    @string mainSrc = mainSrcᶜ1;
    makePkg("a"u8, libSrc);
    makePkg(mainˢ, mainSrc); // don't crash when type-checking this package
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string libSrcᶜ2 = """

package a
const C = foo

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string mainSrcᶜ2 = """

package main
import "a"
const _ = a.C

"""u8;

public static void TestIssue59603(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(
        Error: (error err) => {
            Ꮡt.Log(err); // don't exit after first error
        },
        Importer: imports
    );
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    void makePkg(@string path, @string src) {
        (importsʗ1[path], _) = Ꮡconf.Check(path, fsetʗ1, new ж<ast.File>[]{mustParse(fsetʗ1, src)}.slice(), nil); // errors logged via conf.Error
    }
    @string libSrc = libSrcᶜ2;
    @string mainSrc = mainSrcᶜ2;
    makePkg("a"u8, libSrc);
    makePkg(mainˢ, mainSrc); // don't crash when type-checking this package
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "LookupFieldOrMethod on nil type"u8;

public static void TestLookupFieldOrMethodOnNil(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // LookupFieldOrMethod on a nil type is expected to produce a run-time panic.
        defer(() => {
            @string want = wantᶜ;
            var p = recover();
            {
                var (s, ok) = p._<@string>(ᐧ); if (!ok || s != want) {
                    Ꮡt.Fatalf("got %v, want %s"u8, p, want);
                }
            }
        }, ref ᒐ);
        LookupFieldOrMethod(default!, false, nil, ""u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestLookupFieldOrMethod_type {
    internal @string src;
    internal bool found;
    internal slice<nint> index;
    internal bool indirect;
}

public static void TestLookupFieldOrMethod(ж<testing.T> Ꮡt) {
// field lookups
// field lookups on a generic type
// method lookups
// TODO(gri) should this report indirect = false?
// method lookups on a generic type
// TODO(gri) should this report indirect = false?
// collisions
// collisions on a generic type
// outside methodset
// (*T).f method exists, but value of type T is not addressable
// outside method set of a generic type
// recursive generic types; see go.dev/issue/52715
    // Test cases assume a lookup of the form a.f or x.f, where a stands for an
    // addressable value, and x for a non-addressable value (even though a variable
    // for ease of test case writing).
    //
    // Should be kept in sync with TestMethodSet.
    slice<TestLookupFieldOrMethod_type> tests = new TestLookupFieldOrMethod_type[]{
        new("var x T; type T struct{}"u8, false, default!, false),
        new("var x T; type T struct{ f int }"u8, true, new nint[]{0}.slice(), false),
        new("var x T; type T struct{ a, b, f, c int }"u8, true, new nint[]{2}.slice(), false),
        new("var x T[int]; type T[P any] struct{}"u8, false, default!, false),
        new("var x T[int]; type T[P any] struct{ f P }"u8, true, new nint[]{0}.slice(), false),
        new("var x T[int]; type T[P any] struct{ a, b, f, c P }"u8, true, new nint[]{2}.slice(), false),
        new("var a T; type T struct{}; func (T) f() {}"u8, true, new nint[]{0}.slice(), false),
        new("var a *T; type T struct{}; func (T) f() {}"u8, true, new nint[]{0}.slice(), true),
        new("var a T; type T struct{}; func (*T) f() {}"u8, true, new nint[]{0}.slice(), false),
        new("var a *T; type T struct{}; func (*T) f() {}"u8, true, new nint[]{0}.slice(), true),
        new("var a T[int]; type T[P any] struct{}; func (T[P]) f() {}"u8, true, new nint[]{0}.slice(), false),
        new("var a *T[int]; type T[P any] struct{}; func (T[P]) f() {}"u8, true, new nint[]{0}.slice(), true),
        new("var a T[int]; type T[P any] struct{}; func (*T[P]) f() {}"u8, true, new nint[]{0}.slice(), false),
        new("var a *T[int]; type T[P any] struct{}; func (*T[P]) f() {}"u8, true, new nint[]{0}.slice(), true),
        new("type ( E1 struct{ f int }; E2 struct{ f int }; x struct{ E1; *E2 })"u8, false, new nint[]{1, 0}.slice(), false),
        new("type ( E1 struct{ f int }; E2 struct{}; x struct{ E1; *E2 }); func (E2) f() {}"u8, false, new nint[]{1, 0}.slice(), false),
        new("type ( E1[P any] struct{ f P }; E2[P any] struct{ f P }; x struct{ E1[int]; *E2[int] })"u8, false, new nint[]{1, 0}.slice(), false),
        new("type ( E1[P any] struct{ f P }; E2[P any] struct{}; x struct{ E1[int]; *E2[int] }); func (E2[P]) f() {}"u8, false, new nint[]{1, 0}.slice(), false),
        new("var x T; type T struct{}; func (*T) f() {}"u8, false, default!, true),
        new("var x T[int]; type T[P any] struct{}; func (*T[P]) f() {}"u8, false, default!, true),
        new("var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (N[P]) f() {}"u8, true, new nint[]{0, 0}.slice(), true),
        new("var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (T[P]) f() {}"u8, true, new nint[]{0}.slice(), false)
    }.slice();
    foreach (var (_, test) in tests) {
        var pkg = mustTypecheck("package p;"u8 + test.src, nil, nil);
        var obj = pkg.Scope().Lookup("a"u8);
        if (obj == default!) {
            {
                obj = pkg.Scope().Lookup("x"u8); if (obj == default!) {
                    Ꮡt.Errorf("%s: incorrect test case - no object a or x"u8, test.src);
                    continue;
                }
            }
        }
        var (f, index, indirect) = LookupFieldOrMethod(obj.Type(), obj.Name() == "a"u8, pkg, "f"u8);
        if ((f != default!) != test.found) {
            if (f == default!){
                Ꮡt.Errorf("%s: got no object; want one"u8, test.src);
            } else {
                Ꮡt.Errorf("%s: got object = %v; want none"u8, test.src, f);
            }
        }
        if (!slices.Equal<slice<nint>, nint>(index, test.index)) {
            Ꮡt.Errorf("%s: got index = %v; want %v"u8, test.src, index, test.index);
        }
        if (indirect != test.indirect) {
            Ꮡt.Errorf("%s: got indirect = %v; want %v"u8, test.src, indirect, test.indirect);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pkgˢ = "pkg"u8;
internal static readonly @string instanceˢ = "Instance"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ1 = """

package pkg

type Tree[T any] struct {
	*Node[T]
}

func (*Tree[R]) N(r R) R { return r }

type Node[T any] struct {
	*Tree[T]
}

type Instance = *Tree[int]

"""u8;

// Test for go.dev/issue/52715
public static void TestLookupFieldOrMethod_RecursiveGeneric(ж<testing.T> Ꮡt) {
    @string src = srcᶜ1;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var pkg = NewPackage(pkgˢ, (~(~f).Name).Name);
    {
        var err = NewChecker(nil, fset, pkg, nil).Files(new ж<ast.File>[]{f}.slice()); if (err != default!) {
            throw panic(err);
        }
    }
    var T = pkg.Scope().Lookup(instanceˢ).Type();
    (_, _, _) = LookupFieldOrMethod(T, false, pkg, "M"u8); // verify that LookupFieldOrMethod terminates
}

// newDefined creates a new defined type named T with the given underlying type.
// Helper function for use with TestIncompleteInterfaces only.
internal static ж<types.Named> newDefined(typesꓸType underlying) {
    var tname = NewTypeName(nopos, nil, "T"u8, default!);
    return NewNamed(tname, underlying, default!);
}

[GoType("dyn")] internal partial struct TestConvertibleTo_type {
    internal typesꓸType v, t;
    internal bool want;
}

public static void TestConvertibleTo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestConvertibleTo_type[]{
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int]), true),
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Float32]), true),
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[ΔString]), true),
        new(new types.NamedжΔType(newDefined(new types.BasicжΔType(Typ[Int]))), new types.BasicжΔType(Typ[Int]), true),
        new(new types.NamedжΔType(newDefined(new types.StructжΔType(@new<types.Struct>()))), new types.StructжΔType(@new<types.Struct>()), true),
        new(new types.NamedжΔType(newDefined(new types.BasicжΔType(Typ[Int]))), new types.StructжΔType(@new<types.Struct>()), false),
        new(new types.BasicжΔType(Typ[ΔUntypedInt]), new types.BasicжΔType(Typ[Int]), true),
        new(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))), new types.ArrayжΔType(NewArray(new types.BasicжΔType(Typ[Int]), 10)), true),
        new(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))), new types.ArrayжΔType(NewArray(new types.BasicжΔType(Typ[Uint]), 10)), false),
        new(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))), new types.PointerжΔType(NewPointer(new types.ArrayжΔType(NewArray(new types.BasicжΔType(Typ[Int]), 10)))), true),
        new(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))), new types.PointerжΔType(NewPointer(new types.ArrayжΔType(NewArray(new types.BasicжΔType(Typ[Uint]), 10)))), false), // Untyped string values are not permitted by the spec, so the behavior below is undefined.

        new(new types.BasicжΔType(Typ[UntypedString]), new types.BasicжΔType(Typ[ΔString]), true)
    }.slice()) {
        {
            var got = ConvertibleTo(test.v, test.t); if (got != test.want) {
                Ꮡt.Errorf("ConvertibleTo(%v, %v) = %t, want %t"u8, test.v, test.t, got, test.want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestAssignableTo_type {
    internal typesꓸType v, t;
    internal bool want;
}

public static void TestAssignableTo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestAssignableTo_type[]{
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int]), true),
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Float32]), false),
        new(new types.NamedжΔType(newDefined(new types.BasicжΔType(Typ[Int]))), new types.BasicжΔType(Typ[Int]), false),
        new(new types.NamedжΔType(newDefined(new types.StructжΔType(@new<types.Struct>()))), new types.StructжΔType(@new<types.Struct>()), true),
        new(new types.BasicжΔType(Typ[UntypedBool]), new types.BasicжΔType(Typ[Bool]), true),
        new(new types.BasicжΔType(Typ[UntypedString]), new types.BasicжΔType(Typ[Bool]), false), // Neither untyped string nor untyped numeric assignments arise during
 // normal type checking, so the below behavior is technically undefined by
 // the spec.

        new(new types.BasicжΔType(Typ[UntypedString]), new types.BasicжΔType(Typ[ΔString]), true),
        new(new types.BasicжΔType(Typ[ΔUntypedInt]), new types.BasicжΔType(Typ[Int]), true)
    }.slice()) {
        {
            var got = AssignableTo(test.v, test.t); if (got != test.want) {
                Ꮡt.Errorf("AssignableTo(%v, %v) = %t, want %t"u8, test.v, test.t, got, test.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testMustDeclareBothXAndYˢ = (@string)"test must declare both X and Y"u8;

[GoType("dyn")] internal partial struct TestIdentical_tests {
    internal @string src;
    internal bool want;
}

public static void TestIdentical(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // For each test, we compare the types of objects X and Y in the source.
    var tests = new TestIdentical_tests[]{ // Basic types.

        new("var X int; var Y int"u8, true),
        new("var X int; var Y string"u8, false), // TODO: add more tests for complex types.
 // Named types.

        new("type X int; type Y int"u8, false), // Aliases.

        new("type X = int; type Y = int"u8, true), // Functions.

        new(@"func X(int) string { return """" }; func Y(int) string { return """" }"u8, true),
        new(@"func X() string { return """" }; func Y(int) string { return """" }"u8, false),
        new(@"func X(int) string { return """" }; func Y(int) {}"u8, false), // Generic functions. Type parameters should be considered identical modulo
 // renaming. See also go.dev/issue/49722.

        new(@"func X[P ~int](){}; func Y[Q ~int]() {}"u8, true),
        new(@"func X[P1 any, P2 ~*P1](){}; func Y[Q1 any, Q2 ~*Q1]() {}"u8, true),
        new(@"func X[P1 any, P2 ~[]P1](){}; func Y[Q1 any, Q2 ~*Q1]() {}"u8, false),
        new(@"func X[P ~int](P){}; func Y[Q ~int](Q) {}"u8, true),
        new(@"func X[P ~string](P){}; func Y[Q ~int](Q) {}"u8, false),
        new(@"func X[P ~int]([]P){}; func Y[Q ~int]([]Q) {}"u8, true)
    }.slice();
    foreach (var (_, test) in tests) {
        var pkg = mustTypecheck("package p;"u8 + test.src, nil, nil);
        var X = pkg.Scope().Lookup("X"u8);
        var Y = pkg.Scope().Lookup("Y"u8);
        if (X == default! || Y == default!) {
            Ꮡt.Fatal(testMustDeclareBothXAndYˢ);
        }
        {
            var got = Identical(X.Type(), Y.Type()); if (got != test.want) {
                Ꮡt.Errorf("Identical(%s, %s) = %t, want %t"u8, X.Type(), Y.Type(), got, test.want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestIdentical_issue15173_type {
    internal typesꓸType x, y;
    internal bool want;
}

public static void TestIdentical_issue15173(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Identical should allow nil arguments and be symmetric.
    foreach (var (_, test) in new TestIdentical_issue15173_type[]{
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int]), true),
        new(new types.BasicжΔType(Typ[Int]), default!, false),
        new(default!, new types.BasicжΔType(Typ[Int]), false),
        new(default!, default!, true)
    }.slice()) {
        {
            var got = Identical(test.x, test.y); if (got != test.want) {
                Ꮡt.Errorf("Identical(%v, %v) = %t"u8, test.x, test.y, got);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myIntˢ = "myInt"u8;

[GoType("dyn")] internal partial struct TestIdenticalUnions_type {
    internal @string x, y;
    internal bool want;
}

public static void TestIdenticalUnions(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tname = NewTypeName(nopos, nil, myIntˢ, default!);
    var myInt = NewNamed(tname, new types.BasicжΔType(Typ[Int]), default!);
    var tmap = new map<@string, ж<typesꓸTerm>>{
        ["int"u8] = NewTerm(false, new types.BasicжΔType(Typ[Int])),
        ["~int"u8] = NewTerm(true, new types.BasicжΔType(Typ[Int])),
        ["string"u8] = NewTerm(false, new types.BasicжΔType(Typ[ΔString])),
        ["~string"u8] = NewTerm(true, new types.BasicжΔType(Typ[ΔString])),
        ["myInt"u8] = NewTerm(false, new types.NamedжΔType(myInt))
    };
    var tmapʗ1 = tmap;
    ж<types.Union> makeUnion(@string s) {
        var parts = strings.Split(s, "|"u8);
        slice<ж<typesꓸTerm>> terms = default!;
        foreach (var (_, p) in parts) {
            var term = tmapʗ1[p];
            if (term == nil) {
                Ꮡt.Fatalf("missing term %q"u8, p);
            }
            terms = append(terms, term);
        }
        return NewUnion(terms);
    }
    foreach (var (_, test) in new TestIdenticalUnions_type[]{ // These tests are just sanity checks. The tests for type sets and
 // interfaces provide much more test coverage.

        new("int|~int"u8, "~int"u8, true),
        new("myInt|~int"u8, "~int"u8, true),
        new("int|string"u8, "string|int"u8, true),
        new("int|int|string"u8, "string|int"u8, true),
        new("myInt|string"u8, "int|string"u8, false)
    }.slice()) {
        var x = makeUnion(test.x);
        var y = makeUnion(test.y);
        {
            var got = Identical(new types.UnionжΔType(x), new types.UnionжΔType(y)); if (got != test.want) {
                Ꮡt.Errorf("Identical(%v, %v) = %t"u8, test.x, test.y, got);
            }
        }
    }
}

public static void TestIssue61737(ж<testing.T> Ꮡt) {
    // This test verifies that it is possible to construct invalid interfaces
    // containing duplicate methods using the go/types API.
    //
    // It must be possible for importers to construct such invalid interfaces.
    // Previously, this panicked.
    var sig1 = NewSignatureType(nil, default!, default!, NewTuple(NewParam(nopos, nil, ""u8, new types.BasicжΔType(Typ[Int]))), nil, false);
    var sig2 = NewSignatureType(nil, default!, default!, NewTuple(NewParam(nopos, nil, ""u8, new types.BasicжΔType(Typ[ΔString]))), nil, false);
    var methods = new ж<types.Func>[]{
        NewFunc(nopos, nil, "M"u8, sig1),
        NewFunc(nopos, nil, "M"u8, sig2)
    }.slice();
    var embeddedMethods = new ж<types.Func>[]{
        NewFunc(nopos, nil, "M"u8, sig2)
    }.slice();
    var embedded = NewInterfaceType(embeddedMethods, default!);
    var iface = NewInterfaceType(methods, new typesꓸType[]{new types.InterfaceжΔType(embedded)}.slice());
    iface.Complete();
}

public static void TestNewAlias_Issue65455(ж<testing.T> Ꮡt) {
    var obj = NewTypeName(nopos, nil, "A"u8, default!);
    var alias = NewAlias(obj, new types.BasicжΔType(Typ[Int]));
    alias.Underlying(); // must not panic
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ2 = "package p; func f() int16; var _ = f(undef)"u8;

public static void TestIssue15305(ж<testing.T> Ꮡt) {
    @string src = srcᶜ2;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(
        Error: (error err) => {
        }
    );
    // allow errors
    var info = Ꮡ(new typesꓸInfo(
        Types: new map<ast.Expr, types.TypeAndValue>()
    ));
    Ꮡconf.Check("p"u8, fset, new ж<ast.File>[]{f}.slice(), info); // ignore result
    foreach (var (e, tv) in (~info).Types) {
        {
            var (_, ok) = e._<ж<ast.CallExpr>>(ᐧ); if (ok) {
                if (!AreEqual(tv.Type, Typ[Int16])) {
                    Ꮡt.Errorf("CallExpr has type %v, want int16"u8, tv.Type);
                }
                return;
            }
        }
    }
    Ꮡt.Errorf("CallExpr has no type"u8);
}

[GoType("dyn")] internal partial struct TestCompositeLitTypes_type {
    internal @string lit, typ;
}

// TestCompositeLitTypes verifies that Info.Types registers the correct
// types for composite literal expressions and composite literal type
// expressions.
public static void TestCompositeLitTypes(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in new TestCompositeLitTypes_type[]{
        new(@"[16]byte{}"u8, @"[16]byte"u8),
        new(@"[...]byte{}"u8, @"[0]byte"u8), // test for go.dev/issue/14092

        new(@"[...]int{1, 2, 3}"u8, @"[3]int"u8), // test for go.dev/issue/14092

        new(@"[...]int{90: 0, 98: 1, 2}"u8, @"[100]int"u8), // test for go.dev/issue/14092

        new(@"[]int{}"u8, @"[]int"u8),
        new(@"map[string]bool{""foo"": true}"u8, @"map[string]bool"u8),
        new(@"struct{}{}"u8, @"struct{}"u8),
        new(@"struct{x, y int; z complex128}{}"u8, @"struct{x int; y int; z complex128}"u8)
    }.slice()) {
        ref var test = ref heap(new TestCompositeLitTypes_type(), out var Ꮡtest);
        test = vᴛ1;

        var fset = token.NewFileSet();
        var f = mustParse(fset, fmt.Sprintf("package p%d; var _ = %s"u8, i, test.lit));
        var types = new map<ast.Expr, types.TypeAndValue>();
        {
            var (_, err) = @new<types.Config>().Check("p"u8, fset, new ж<ast.File>[]{f}.slice(), Ꮡ(new typesꓸInfo(Types: types))); if (err != default!) {
                Ꮡt.Fatalf("%s: %v"u8, test.lit, err);
            }
        }
        var testʗ1 = test;
        var typesʗ1 = types;
        void cmptype(ast.Expr x, @string want) {
            var (tv, ok) = typesʗ1[x, ꟷ];
            if (!ok) {
                Ꮡt.Errorf("%s: no Types entry found"u8, testʗ1.lit);
                return;
            }
            if (tv.Type == default!) {
                Ꮡt.Errorf("%s: type is nil"u8, testʗ1.lit);
                return;
            }
            {
                @string got = tv.Type.String(); if (got != want) {
                    Ꮡt.Errorf("%s: got %v, want %s"u8, testʗ1.lit, got, want);
                }
            }
        }
        // test type of composite literal expression
        var rhs = (~(~(~f).Decls[0]._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.ValueSpec>>()).Values[0];
        cmptype(rhs, test.typ);
        // test type of composite literal type expression
        cmptype((~rhs._<ж<ast.CompositeLit>>()).Type, test.typ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ3 = """

package p

const C = 0

type T1 struct {
	a, b int
	T2
}

type T2 interface {
	im1()
	im2()
}

func (T1) m1() {}
func (*T1) m2() {}

func f(x int) { y := x; print(y) }

"""u8;

// TestObjectParents verifies that objects have parent scopes or not
// as specified by the Object interface.
public static void TestObjectParents(ж<testing.T> Ꮡt) {
    @string src = srcᶜ3;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var info = Ꮡ(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>()
    ));
    {
        var (_, err) = @new<types.Config>().Check("p"u8, fset, new ж<ast.File>[]{f}.slice(), info); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    foreach (var (ident, obj) in (~info).Defs) {
        if (obj == default!) {
            // only package names and implicit vars have a nil object
            // (in this test we only need to handle the package name)
            if ((~ident).Name != "p"u8) {
                Ꮡt.Errorf("%v has nil object"u8, ident.OrTypedNil());
            }
            continue;
        }
        // struct fields, type-associated and interface methods
        // have no parent scope
        var wantParent = true;
        switch (obj.type()) {
        case ж<types.Var> objΔ1: {
            if (objΔ1.IsField()) {
                wantParent = false;
            }
            break;
        }
        case ж<types.Func> objΔ1: {
            if (objΔ1.Signature().Recv() != nil) {
                // method
                wantParent = false;
            }
            break;
        }}
        var gotParent = obj.Parent() != nil;
        switch (ᐧ) {
        case {} when gotParent && !wantParent: {
            Ꮡt.Errorf("%v: want no parent, got %s"u8, ident.OrTypedNil(), obj.Parent().OrTypedNil());
            break;
        }
        case {} when !gotParent && wantParent: {
            Ꮡt.Errorf("%v: no parent found"u8, ident.OrTypedNil());
            break;
        }}

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string couldNotImportˢ = "could not import"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ4 = """

package p

import foo "go/types/thisdirectorymustnotexistotherwisethistestmayfail/foo" // should only see an error here

const c = foo.C
type T = foo.T
var v T = c
func f(x T) T { return foo.F(x) }

"""u8;

// TestFailedImport tests that we don't get follow-on errors
// elsewhere in a package due to failing to import a package.
public static void TestFailedImport(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    @string src = srcᶜ4;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var files = new ж<ast.File>[]{f}.slice();
    // type-check using all possible importers
    foreach (var (_, compiler) in new @string[]{"gc"u8, "gccgo"u8, "source"u8}.slice()) {
        nint errcount = 0;
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(
            Error: (error err) => {
                // we should only see the import error
                if (errcount > 0 || !strings.Contains(err.Error(), couldNotImportˢ)) {
                    Ꮡt.Errorf("for %s importer, got unexpected error: %v"u8, compiler, err);
                }
                errcount++;
            },
            Importer: importer.For(compiler, default!)
        );
        var info = Ꮡ(new typesꓸInfo(
            Uses: new map<ж<ast.Ident>, types.Object>()
        ));
        var (pkg, _) = Ꮡconf.Check("p"u8, fset, files, info);
        if (pkg == nil) {
            Ꮡt.Errorf("for %s importer, type-checking failed to return a package"u8, compiler);
            continue;
        }
        var imports = pkg.Imports();
        if (len(imports) != 1) {
            Ꮡt.Errorf("for %s importer, got %d imports, want 1"u8, compiler, len(imports));
            continue;
        }
        var imp = imports[0];
        if (imp.Name() != "foo"u8) {
            Ꮡt.Errorf(@"for %s importer, got %q, want ""foo"""u8, compiler, imp.Name());
            continue;
        }
        // verify that all uses of foo refer to the imported package foo (imp)
        foreach (var (ident, obj) in (~info).Uses) {
            if ((~ident).Name == "foo"u8) {
                {
                    var (objΔ1, ok) = obj._<ж<types.PkgName>>(ᐧ); if (ok){
                        if (objΔ1.Imported() != imp) {
                            Ꮡt.Errorf("%s resolved to %v; want %v"u8, ident.OrTypedNil(), objΔ1.Imported().OrTypedNil(), imp.OrTypedNil());
                        }
                    } else {
                        Ꮡt.Errorf("%s resolved to %v; want package name"u8, ident.OrTypedNil(), objΔ1.OrTypedNil());
                    }
                }
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ5 = "package p; type T[P any] *T[P]"u8;

public static void TestInstantiate(ж<testing.T> Ꮡt) {
    // eventually we like more tests but this is a start
    @string src = srcᶜ5;
    var pkg = mustTypecheck(src, nil, nil);
    // type T should have one type parameter
    var T = pkg.Scope().Lookup("T"u8).Type()._<ж<types.Named>>();
    {
        nint n = T.TypeParams().Len(); if (n != 1) {
            Ꮡt.Fatalf("expected 1 type parameter; found %d"u8, n);
        }
    }
    // instantiation should succeed (no endless recursion)
    // even with a nil *Checker
    var (res, err) = Instantiate(nil, new types.NamedжΔType(T), new typesꓸType[]{new types.BasicжΔType(Typ[Int])}.slice(), false);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // instantiated type should point to itself
    {
        var p = res.Underlying()._<ж<types.Pointer>>().Elem(); if (!AreEqual(p, res)) {
            Ꮡt.Fatalf("unexpected result type: %s points to %s"u8, res, p);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ6 = """
package p

type I[P any] interface {
	m(P)
	n() P
}

type J = I[int]

type Nested[P any] *interface{b(P)}

type K = Nested[string]

"""u8;

public static partial void TestInstantiateConcurrent(ж<testing.T> Ꮡt) {
    @string src = srcᶜ6;
    var pkg = mustTypecheck(src, nil, nil);
    var insts = new ж<types.Interface>[]{
        pkg.Scope().Lookup("J"u8).Type().Underlying()._<ж<types.Interface>>(),
        pkg.Scope().Lookup("K"u8).Type().Underlying()._<ж<types.Pointer>>().Elem()._<ж<types.Interface>>()
    }.slice();
    // Use the interface instances concurrently.
    foreach (var (_, inst) in insts) {
        ref var counts = ref heap(new array<nint>(2), out var Ꮡcounts);              // method counts
        ref var methods = ref heap(new array<slice<@string>>(2), out var Ꮡmethods);                 // method strings
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint i = 0; i < 2; i++) {
            nint iΔ1 = i;
            Ꮡwg.Add(1);
            var instʗ1 = inst;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    Ꮡcounts.Value[iΔ1] = instʗ1.NumMethods();
                    for (nint mi = 0; mi < Ꮡcounts.Value[iΔ1]; mi++) {
                        Ꮡmethods.Value[iΔ1] = append(Ꮡmethods.Value[iΔ1], instʗ1.Method(mi).String());
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
        if (counts[0] != counts[1]) {
            Ꮡt.Errorf("mismatching method counts for %s: %d vs %d"u8, inst.OrTypedNil(), counts[0], counts[1]);
            continue;
        }
        for (nint i = 0; i < counts[0]; i++) {
            {
                @string m0 = methods[0][i];
                @string m1 = methods[1][i]; if (m0 != m1) {
                    Ꮡt.Errorf("mismatching methods for %s: %s vs %s"u8, inst.OrTypedNil(), m0, m1);
                }
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestInstantiateErrors_tests {
    internal @string src; // by convention, T must be the type being instantiated
    internal slice<typesꓸType> targs;
    internal nint wantAt; // -1 indicates no error
}

public static void TestInstantiateErrors(ж<testing.T> Ꮡt) {
    var tests = new TestInstantiateErrors_tests[]{
        new("type T[P interface{~string}] int"u8, new typesꓸType[]{new types.BasicжΔType(Typ[Int])}.slice(), 0),
        new("type T[P1 interface{int}, P2 interface{~string}] int"u8, new typesꓸType[]{new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int])}.slice(), 1),
        new("type T[P1 any, P2 interface{~[]P1}] int"u8, new typesꓸType[]{new types.BasicжΔType(Typ[Int]), new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[ΔString])))}.slice(), 1),
        new("type T[P1 interface{~[]P2}, P2 any] int"u8, new typesꓸType[]{new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[ΔString]))), new types.BasicжΔType(Typ[Int])}.slice(), 0)
    }.slice();
    foreach (var (_, test) in tests) {
        @string src = "package p; "u8 + test.src;
        var pkg = mustTypecheck(src, nil, nil);
        var T = pkg.Scope().Lookup("T"u8).Type()._<ж<types.Named>>();
        var (_, err) = Instantiate(nil, new types.NamedжΔType(T), test.targs, true);
        if (err == default!) {
            Ꮡt.Fatalf("Instantiate(%v, %v) returned nil error, want non-nil"u8, T.OrTypedNil(), test.targs);
        }
        ref var argErr = ref heap<ж<types.ArgumentError>>(out var ᏑargErr);
        if (!errors.As(err, ᏑargErr)) {
            Ꮡt.Fatalf("Instantiate(%v, %v): error is not an *ArgumentError"u8, T.OrTypedNil(), test.targs);
        }
        if ((~argErr).Index != test.wantAt) {
            Ꮡt.Errorf("Instantiate(%v, %v): error at index %d, want index %d"u8, T.OrTypedNil(), test.targs, (~argErr).Index, test.wantAt);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;

public static void TestArgumentErrorUnwrapping(ж<testing.T> Ꮡt) {
    error err = new types.ArgumentErrorжerror(Ꮡ(new ArgumentError(
        Index: 1,
        Err: new typesꓸError(Msg: "test"u8)
    )));
    ref var e = ref heap(new typesꓸError(), out var Ꮡe);
    if (!errors.As(err, Ꮡe)) {
        Ꮡt.Fatalf("error %v does not wrap types.Error"u8, err);
    }
    if (e.Msg != "test"u8) {
        Ꮡt.Errorf("e.Msg = %q, want %q"u8, e.Msg, testˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageLibTypeTPAnyˢ = @"package lib; type T[P any] struct{}"u8;
internal static readonly @string packageAImportLibVarALibˢ = @"package a; import ""lib""; var A lib.T[int]"u8;
internal static readonly @string packageBImportLibVarBLibˢ = @"package b; import ""lib""; var B lib.T[int]"u8;

public static void TestInstanceIdentity(ж<testing.T> Ꮡt) {
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: imports);
    var importsʗ1 = imports;
    void makePkg(@string src) {
        var fset = token.NewFileSet();
        var f = mustParse(fset, src);
        @string name = f.Value.Name.Value.Name;
        var (pkg, err) = Ꮡconf.Check(name, fset, new ж<ast.File>[]{f}.slice(), nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        importsʗ1[name] = pkg;
    }
    makePkg(packageLibTypeTPAnyˢ);
    makePkg(packageAImportLibVarALibˢ);
    makePkg(packageBImportLibVarBLibˢ);
    var a = imports["a"u8].Scope().Lookup("A"u8);
    var b = imports["b"u8].Scope().Lookup("B"u8);
    if (!Identical(a.Type(), b.Type())) {
        Ꮡt.Errorf("mismatching types: a.A: %s, b.B: %s"u8, a.Type(), b.Type());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ7 = """

package p

type T[P any] struct {
	field P
}

func (recv *T[Q]) concreteMethod(mParam Q) (mResult Q) { return }

type FT[P any] func(ftParam P) (ftResult P)

func F[P any](fParam P) (fResult P){ return }

type I[P any] interface {
	interfaceMethod(P)
}

type R[P any] T[P]

func (R[P]) m() {} // having a method triggers expansion of R

var (
	t T[int]
	ft FT[int]
	f = F[int]
	i I[int]
)

func fn() {
	var r R[int]
	_ = r
}

"""u8;

[GoType("dyn")] internal partial struct TestInstantiatedObjects_tests {
    internal @string name;
    internal types.Object obj;
}

// TestInstantiatedObjects verifies properties of instantiated objects.
public static void TestInstantiatedObjects(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string src = srcᶜ7;
    var info = Ꮡ(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>()
    ));
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(nil);
    var (pkg, err) = Ꮡconf.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), info);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var pkgʗ1 = pkg;
    typesꓸType lookup(@string name) => pkgʗ1.Scope().Lookup(name).Type();
    var fnScope = pkg.Scope().Lookup("fn"u8)._<ж<types.Func>>().Scope();
    var tests = new TestInstantiatedObjects_tests[]{ // Struct fields

        new("field"u8, new types.VarжObject(lookup("t"u8).Underlying()._<ж<types.Struct>>().Field(0))),
        new("field"u8, new types.VarжObject(fnScope.Lookup("r"u8).Type().Underlying()._<ж<types.Struct>>().Field(0))), // Methods and method fields

        new("concreteMethod"u8, new types.FuncжObject(lookup("t"u8)._<ж<types.Named>>().Method(0))),
        new("recv"u8, new types.VarжObject(lookup("t"u8)._<ж<types.Named>>().Method(0).Signature().Recv())),
        new("mParam"u8, new types.VarжObject(lookup("t"u8)._<ж<types.Named>>().Method(0).Signature().Params().At(0))),
        new("mResult"u8, new types.VarжObject(lookup("t"u8)._<ж<types.Named>>().Method(0).Signature().Results().At(0))), // Interface methods

        new("interfaceMethod"u8, new types.FuncжObject(lookup("i"u8).Underlying()._<ж<types.Interface>>().Method(0))), // Function type fields

        new("ftParam"u8, new types.VarжObject(lookup("ft"u8).Underlying()._<ж<typesꓸSignature>>().Params().At(0))),
        new("ftResult"u8, new types.VarжObject(lookup("ft"u8).Underlying()._<ж<typesꓸSignature>>().Results().At(0))), // Function fields

        new("fParam"u8, new types.VarжObject(lookup("f"u8)._<ж<typesꓸSignature>>().Params().At(0))),
        new("fResult"u8, new types.VarжObject(lookup("f"u8)._<ж<typesꓸSignature>>().Results().At(0)))
    }.slice();
    // Collect all identifiers by name.
    var idents = new map<@string, slice<ж<ast.Ident>>>();
    var identsʗ1 = idents;
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        {
            var (id, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
                identsʗ1[(~id).Name] = append(identsʗ1[(~id).Name], id);
            }
        }
        return true;
    });
    foreach (var (_, test) in tests) {
        ref var testΔ1 = ref heap<TestInstantiatedObjects_tests>(out var ᏑtestΔ1);
        testΔ1 = test;
        var identsʗ2 = idents;
        var infoʗ1 = info;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.name, (ж<testing.T> tΔ1) => {
            {
                nint got = len(identsʗ2[testʗ1.name]); if (got != 1) {
                    tΔ1.Fatalf("found %d identifiers named %s, want 1"u8, got, testʗ1.name);
                }
            }
            var ident = identsʗ2[testʗ1.name][0];
            var def = (~infoʗ1).Defs[ident];
            if (AreEqual(def, testʗ1.obj)) {
                tΔ1.Fatalf("info.Defs[%s] contains the test object"u8, testʗ1.name);
            }
            {
                var orig = originObject(testʗ1.obj); if (!AreEqual(def, orig)) {
                    tΔ1.Errorf("info.Defs[%s] does not match obj.Origin()"u8, testʗ1.name);
                }
            }
            if (def.Pkg() != testʗ1.obj.Pkg()) {
                tΔ1.Errorf("Pkg() = %v, want %v"u8, def.Pkg().OrTypedNil(), testʗ1.obj.Pkg().OrTypedNil());
            }
            if (def.Name() != testʗ1.obj.Name()) {
                tΔ1.Errorf("Name() = %v, want %v"u8, def.Name(), testʗ1.obj.Name());
            }
            if (def.Pos() != testʗ1.obj.Pos()) {
                tΔ1.Errorf("Pos() = %v, want %v"u8, def.Pos(), testʗ1.obj.Pos());
            }
            if (def.Parent() != testʗ1.obj.Parent()) {
                tΔ1.Fatalf("Parent() = %v, want %v"u8, def.Parent().OrTypedNil(), testʗ1.obj.Parent().OrTypedNil());
            }
            if (def.Exported() != testʗ1.obj.Exported()) {
                tΔ1.Fatalf("Exported() = %v, want %v"u8, def.Exported(), testʗ1.obj.Exported());
            }
            if (def.Id() != testʗ1.obj.Id()) {
                tΔ1.Fatalf("Id() = %v, want %v"u8, def.Id(), testʗ1.obj.Id());
            }
        });
    }
}

// String and Type are expected to differ.
internal static types.Object originObject(types.Object obj) {
    switch (obj.type()) {
    case ж<types.Var> objΔ1: {
        return new types.VarжObject(objΔ1.Origin());
    }
    case ж<types.Func> objΔ1: {
        return new types.FuncжObject(objΔ1.Origin());
    }}
    return obj;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyIfaceˢ = "EmptyIface"u8;
internal static readonly @string integerˢ = "Integer"u8;
internal static readonly @string emptyTypeSetˢ = "EmptyTypeSet"u8;
internal static readonly @string badˢ = "Bad"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ8 = """

package p

type EmptyIface interface{}

type I interface {
	m()
}

type C interface {
	m()
	~int
}

type Integer interface{
	int8 | int16 | int32 | int64
}

type EmptyTypeSet interface{
	Integer
	~string
}

type N1 int
func (N1) m() {}

type N2 int
func (*N2) m() {}

type N3 int
func (N3) m(int) {}

type N4 string
func (N4) m()

type Bad Bad // invalid type

"""u8;

[GoType("dyn")] internal partial struct TestImplements_tests {
    public typesꓸType V;
    public ж<types.Interface> T;
    internal bool want;
}

public static void TestImplements(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string src = srcᶜ8;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Error: (error _Δp0) => {
    });
    var (pkg, _) = Ꮡconf.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), nil);
    var pkgʗ1 = pkg;
    typesꓸType lookup(@string tname) => pkgʗ1.Scope().Lookup(tname).Type();
    ж<types.Interface> EmptyIface = lookup(emptyIfaceˢ).Underlying()._<ж<types.Interface>>();
    ж<types.Named> I = lookup("I"u8)._<ж<types.Named>>();
    ж<types.Interface> II = I.Underlying()._<ж<types.Interface>>();
    ж<types.Named> C = lookup("C"u8)._<ж<types.Named>>();
    ж<types.Interface> CI = C.Underlying()._<ж<types.Interface>>();
    ж<types.Interface> Integer = lookup(integerˢ).Underlying()._<ж<types.Interface>>();
    ж<types.Interface> EmptyTypeSet = lookup(emptyTypeSetˢ).Underlying()._<ж<types.Interface>>();
    typesꓸType N1 = lookup("N1"u8);
    ж<types.Pointer> N1p = NewPointer(N1);
    typesꓸType N2 = lookup("N2"u8);
    ж<types.Pointer> N2p = NewPointer(N2);
    typesꓸType N3 = lookup("N3"u8);
    typesꓸType N4 = lookup("N4"u8);
    typesꓸType Bad = lookup(badˢ);
    var tests = new TestImplements_tests[]{
        new(new types.NamedжΔType(I), II, true),
        new(new types.NamedжΔType(I), CI, false),
        new(new types.NamedжΔType(C), II, true),
        new(new types.NamedжΔType(C), CI, true),
        new(new types.BasicжΔType(Typ[Int8]), Integer, true),
        new(new types.BasicжΔType(Typ[Int64]), Integer, true),
        new(new types.BasicжΔType(Typ[ΔString]), Integer, false),
        new(new types.InterfaceжΔType(EmptyTypeSet), II, true),
        new(new types.InterfaceжΔType(EmptyTypeSet), EmptyTypeSet, true),
        new(new types.BasicжΔType(Typ[Int]), EmptyTypeSet, false),
        new(N1, II, true),
        new(N1, CI, true),
        new(new types.PointerжΔType(N1p), II, true),
        new(new types.PointerжΔType(N1p), CI, false),
        new(N2, II, false),
        new(N2, CI, false),
        new(new types.PointerжΔType(N2p), II, true),
        new(new types.PointerжΔType(N2p), CI, false),
        new(N3, II, false),
        new(N3, CI, false),
        new(N4, II, true),
        new(N4, CI, false),
        new(Bad, II, false),
        new(Bad, CI, false),
        new(Bad, EmptyIface, true)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            var got = Implements(test.V, test.T); if (got != test.want) {
                Ꮡt.Errorf("Implements(%s, %s) = %t, want %t"u8, test.V, test.T.OrTypedNil(), got, test.want);
            }
        }
        // The type assertion x.(T) is valid if T is an interface or if T implements the type of x.
        // The assertion is never valid if T is a bad type.
        var V = test.T;
        var T = test.V;
        var want = false;
        {
            var (_, ok) = T.Underlying()._<ж<types.Interface>>(ᐧ); if ((ok || Implements(T, V)) && !AreEqual(T, Bad)) {
                want = true;
            }
        }
        {
            var got = AssertableTo(V, T); if (got != want) {
                Ꮡt.Errorf("AssertableTo(%s, %s) = %t, want %t"u8, V.OrTypedNil(), T, got, want);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ9 = """

package p
type T interface {
	m()
}

type V0 struct{}
func (V0) m() {}

type V1 struct{}

type V2 struct{}
func (V2) m() int

type V3 struct{}
func (*V3) m()

type V4 struct{}
func (V4) M()

"""u8;

public static void TestMissingMethodAlternative(ж<testing.T> Ꮡt) {
    @string src = srcᶜ9;
    var pkg = mustTypecheck(src, nil, nil);
    var T = pkg.Scope().Lookup("T"u8).Type().Underlying()._<ж<types.Interface>>();
    var Tʗ1 = T;
    var pkgʗ1 = pkg;
    (ж<types.Func>, bool) lookup(@string name) => MissingMethod(pkgʗ1.Scope().Lookup(name).Type(), Tʗ1, true);
    // V0 has method m with correct signature. Should not report wrongType.
    var (method, wrongType) = lookup("V0"u8);
    if (method != nil || wrongType) {
        Ꮡt.Fatalf("V0: got method = %v, wrongType = %v"u8, method.OrTypedNil(), wrongType);
    }
    var lookupʗ1 = lookup;
    void checkMissingMethod(@string tname, bool reportWrongType) {
        var (methodΔ1, wrongTypeΔ1) = lookupʗ1(tname);
        if (methodΔ1 == nil || methodΔ1.Name() != "m"u8 || wrongTypeΔ1 != reportWrongType) {
            Ꮡt.Fatalf("%s: got method = %v, wrongType = %v"u8, tname, methodΔ1.OrTypedNil(), wrongTypeΔ1);
        }
    }
    // V1 has no method m. Should not report wrongType.
    checkMissingMethod("V1"u8, false);
    // V2 has method m with wrong signature type (ignoring receiver). Should report wrongType.
    checkMissingMethod("V2"u8, true);
    // V3 has no method m but it exists on *V3. Should report wrongType.
    checkMissingMethod("V3"u8, true);
    // V4 has no method m but has M. Should not report wrongType.
    checkMissingMethod("V4"u8, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorURLˢ = "_ErrorURL"u8;
internal static readonly @string goDevESˢ = " [go.dev/e/%s]"u8;
internal static readonly @string goDevEUndeclaredNameˢ = " [go.dev/e/UndeclaredName]"u8;
internal static readonly @string goDevEWrongArgCountˢ = " [go.dev/e/WrongArgCount]\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string src1ᶜ = """

package p
var _ T

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string src2ᶜ = """

package p
func f() int { return 0 }
var _ = f(1, 2)

"""u8;

public static void TestErrorURL(ж<testing.T> Ꮡt) {
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    stringFieldAddr(Ꮡconf, errorURLˢ).Value = goDevESˢ;
    // test case for a one-line error
    @string src1 = src1ᶜ;
    var (_, err) = typecheck(src1, Ꮡconf, nil);
    if (err == default! || !strings.HasSuffix(err.Error(), goDevEUndeclaredNameˢ)) {
        Ꮡt.Errorf("src1: unexpected error: got %v"u8, err);
    }
    // test case for a multi-line error
    @string src2 = src2ᶜ;
    (_, err) = typecheck(src2, Ꮡconf, nil);
    if (err == default! || !strings.Contains(err.Error(), goDevEWrongArgCountˢ)) {
        Ꮡt.Errorf("src1: unexpected error: got %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePˢ = "package p"u8;

public static void TestModuleVersion(ж<testing.T> Ꮡt) {
    // version go1.dd must be able to typecheck go1.dd.0, go1.dd.1, etc.
    @string goversionΔ1 = fmt.Sprintf("go1.%d"u8, (nint)(goversion.Version));
    foreach (var (_, v) in new @string[]{
        goversionΔ1,
        goversionΔ1 + ".0"u8,
        goversionΔ1 + ".1"u8,
        goversionΔ1 + ".rc"u8
    }.slice()) {
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(GoVersion: v);
        var pkg = mustTypecheck(packagePˢ, Ꮡconf, nil);
        if (pkg.GoVersion() != conf.GoVersion) {
            Ꮡt.Errorf("got %s; want %s"u8, pkg.GoVersion(), conf.GoVersion);
        }
    }
}

[GoType("dyn")] internal partial struct TestFileVersions_type {
    internal @string goVersion;
    internal @string fileVersion;
    internal @string wantVersion;
}

public static void TestFileVersions(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFileVersions_type[]{
        new(""u8, ""u8, ""u8), // no versions specified

        new("go1.19"u8, ""u8, "go1.19"u8), // module version specified

        new(""u8, "go1.20"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1"u8, ""u8, "go1"u8), // no file version specified

        new("go1"u8, "goo1.22"u8, "go1"u8), // invalid file version specified

        new("go1"u8, "go1.19"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1"u8, "go1.20"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1"u8, "go1.21"u8, "go1.21"u8), // file version specified at 1.21

        new("go1"u8, "go1.22"u8, "go1.22"u8), // file version specified above 1.21

        new("go1.19"u8, ""u8, "go1.19"u8), // no file version specified

        new("go1.19"u8, "goo1.22"u8, "go1.19"u8), // invalid file version specified

        new("go1.19"u8, "go1.20"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.19"u8, "go1.21"u8, "go1.21"u8), // file version specified at 1.21

        new("go1.19"u8, "go1.22"u8, "go1.22"u8), // file version specified above 1.21

        new("go1.20"u8, ""u8, "go1.20"u8), // no file version specified

        new("go1.20"u8, "goo1.22"u8, "go1.20"u8), // invalid file version specified

        new("go1.20"u8, "go1.19"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.20"u8, "go1.20"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.20"u8, "go1.21"u8, "go1.21"u8), // file version specified at 1.21

        new("go1.20"u8, "go1.22"u8, "go1.22"u8), // file version specified above 1.21

        new("go1.21"u8, ""u8, "go1.21"u8), // no file version specified

        new("go1.21"u8, "goo1.22"u8, "go1.21"u8), // invalid file version specified

        new("go1.21"u8, "go1.19"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.21"u8, "go1.20"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.21"u8, "go1.21"u8, "go1.21"u8), // file version specified at 1.21

        new("go1.21"u8, "go1.22"u8, "go1.22"u8), // file version specified above 1.21

        new("go1.22"u8, ""u8, "go1.22"u8), // no file version specified

        new("go1.22"u8, "goo1.22"u8, "go1.22"u8), // invalid file version specified

        new("go1.22"u8, "go1.19"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.22"u8, "go1.20"u8, "go1.21"u8), // file version specified below minimum of 1.21

        new("go1.22"u8, "go1.21"u8, "go1.21"u8), // file version specified at 1.21

        new("go1.22"u8, "go1.22"u8, "go1.22"u8), // file version specified above 1.21
 // versions containing release numbers
 // (file versions containing release numbers are considered invalid)

        new("go1.19.0"u8, ""u8, "go1.19.0"u8), // no file version specified

        new("go1.20.1"u8, "go1.19.1"u8, "go1.20.1"u8), // invalid file version

        new("go1.20.1"u8, "go1.21.1"u8, "go1.20.1"u8), // invalid file version

        new("go1.21.1"u8, "go1.19.1"u8, "go1.21.1"u8), // invalid file version

        new("go1.21.1"u8, "go1.21.1"u8, "go1.21.1"u8), // invalid file version

        new("go1.22.1"u8, "go1.19.1"u8, "go1.22.1"u8), // invalid file version

        new("go1.22.1"u8, "go1.21.1"u8, "go1.22.1"u8)
    }.slice()) {
        // invalid file version
        @string src = default!;
        if (test.fileVersion != ""u8) {
            src = "//go:build "u8 + test.fileVersion + "\n"u8;
        }
        src += "package p"u8;
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(GoVersion: test.goVersion);
        var versions = new map<ж<ast.File>, @string>();
        ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
        info.FileVersions = versions;
        mustTypecheck(src, Ꮡconf, Ꮡinfo);
        nint n = 0;
        foreach (var (_, v) in versions) {
            @string want = test.wantVersion;
            if (v != want) {
                Ꮡt.Errorf("%q: unexpected file version: got %q, want %q"u8, src, v, want);
            }
            n++;
        }
        if (n != 1) {
            Ꮡt.Errorf("%q: incorrect number of map entries: got %d"u8, src, n);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string types2ˢ = "types2"u8;
internal static readonly @string typesˢ = "types"u8;
internal static readonly @string fTypesFuncˢ = "f:*types.Func"u8;

[GoType("dyn")] internal partial struct TestTooNew_type {
    internal @string goVersion; // package's Go version (as if derived from go.mod file)
    internal @string fileVersion; // file's Go version (becomes a build tag)
    internal @string wantErr; // expected substring of concatenation of all errors
}

// TestTooNew ensures that "too new" errors are emitted when the file
// or module is tagged with a newer version of Go than this go/types.
public static void TestTooNew(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestTooNew_type[]{
        new("go1.98"u8, ""u8, "package requires newer Go version go1.98"u8),
        new(""u8, "go1.99"u8, "p:2:9: file requires newer Go version go1.99"u8),
        new("go1.98"u8, "go1.99"u8, "package requires newer Go version go1.98"u8), // (two

        new("go1.98"u8, "go1.99"u8, "file requires newer Go version go1.99"u8)
    }.slice()) {
        // errors)
        @string src = default!;
        if (test.fileVersion != ""u8) {
            src = "//go:build "u8 + test.fileVersion + "\n"u8;
        }
        src += "package p; func f()"u8;
        ref var errs = ref heap<slice<error>>(out var Ꮡerrs);
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(
            GoVersion: test.goVersion,
            Error: (error err) => {
                Ꮡerrs.ValueSlot = append(Ꮡerrs.ValueSlot, err);
            }
        );
        var info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
        typecheck(src, Ꮡconf, info);
        @string got = fmt.Sprint(errs);
        if (!strings.Contains(got, test.wantErr)) {
            Ꮡt.Errorf("%q: unexpected error: got %q, want substring %q"u8,
                src, got, test.wantErr);
        }
        // Assert that declarations were type checked nonetheless.
        slice<@string> gotObjs = default!;
        foreach (var (id, obj) in (~info).Defs) {
            if (obj != default!) {
                @string objStr = strings.ReplaceAll(fmt.Sprintf("%s:%T"u8, (~id).Name, obj), types2ˢ, typesˢ);
                gotObjs = append(gotObjs, objStr);
            }
        }
        @string wantObjs = fTypesFuncˢ;
        if (!strings.Contains(fmt.Sprint(gotObjs), wantObjs)) {
            Ꮡt.Errorf("%q: got %s, want substring %q"u8,
                src, gotObjs, wantObjs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aTAAˢ = "a.T[a.A]"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ10 = """
package a

var x T[B] // this appears to cause Unalias to be called on B while still Invalid

type T[_ any] struct{}
type A T[B]
type B = T[A]

"""u8;

// This is a regression test for #66704.
public static void TestUnaliasTooSoonInCycle(ж<testing.T> Ꮡt) {
    setGotypesalias(Ꮡt, true);
    @string src = srcᶜ10;
    var pkg = mustTypecheck(src, nil, nil);
    var B = pkg.Scope().Lookup("B"u8);
    @string got = Unalias(B.Type()).String();
    @string want = aTAAˢ;
    if (got != want) {
        Ꮡt.Errorf("Unalias(type B = T[A]) = %q, want %q"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ11 = """
package p

type A = B
type B = C
type C = int

"""u8;

public static void TestAlias_Rhs(ж<testing.T> Ꮡt) {
    setGotypesalias(Ꮡt, true);
    @string src = srcᶜ11;
    var pkg = mustTypecheck(src, nil, nil);
    var A = pkg.Scope().Lookup("A"u8);
    @string got = A.Type()._<ж<types.Alias>>().Rhs().String();
    @string want = "p.B"u8;
    if (got != want) {
        Ꮡt.Errorf("A.Rhs = %s, want %s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePVarXAnyˢ = "package p; var x any"u8;

// Test the hijacking described of "any" described in golang/go#66921, for type
// checking.
public static void TestAnyHijacking_Check(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, enableAlias) in new bool[]{false, true}.slice()) {
        Ꮡt.Run(fmt.Sprintf("EnableAlias=%t"u8, enableAlias), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            setGotypesalias(tΔ1, enableAlias);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < 10; i++) {
                Ꮡwg.Add(1);
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        var pkg = mustTypecheck(packagePVarXAnyˢ, nil, nil);
                        var x = pkg.Scope().Lookup("x"u8);
                        {
                            var (_, gotAlias) = x.Type()._<ж<types.Alias>>(ᐧ); if (gotAlias != enableAlias) {
                                tΔ1.Errorf(@"Lookup(""x"").Type() is %T: got Alias: %t, want %t"u8, x.Type(), gotAlias, enableAlias);
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            Ꮡwg.Wait();
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string anyˢ = "any"u8;

// Test the hijacking described of "any" described in golang/go#66921, for
// Scope.Lookup outside of type checking.
public static void TestAnyHijacking_Lookup(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, enableAlias) in new bool[]{false, true}.slice()) {
        Ꮡt.Run(fmt.Sprintf("EnableAlias=%t"u8, enableAlias), (ж<testing.T> tΔ1) => {
            setGotypesalias(tΔ1, enableAlias);
            var a = Universe.Lookup(anyˢ);
            {
                var (_, gotAlias) = a.Type()._<ж<types.Alias>>(ᐧ); if (gotAlias != enableAlias) {
                    tΔ1.Errorf(@"Lookup(""x"").Type() is %T: got Alias: %t, want %t"u8, a.Type(), gotAlias, enableAlias);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string gotypesalias1ˢ = "gotypesalias=1"u8;
internal static readonly @string gotypesalias0ˢ = "gotypesalias=0"u8;

internal static void setGotypesalias(ж<testing.T> Ꮡt, bool enable) {
    if (enable){
        Ꮡt.Setenv(godebugˢ, gotypesalias1ˢ);
    } else {
        Ꮡt.Setenv(godebugˢ, gotypesalias0ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aGoˢ = "a.go"u8;
internal static readonly object packagePConstK123ˢ = (@string)"package p; const k = 123"u8;

// TestVersionIssue69477 is a regression test for issue #69477,
// in which the type checker would panic while attempting
// to compute which file it is "in" based on syntax position.
public static void TestVersionIssue69477(ж<testing.T> Ꮡt) {
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, packagePConstK123ˢ, 0);
    // Set an invalid Pos on the BasicLit.
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        {
            var (lit, ok) = n._<ж<ast.BasicLit>>(ᐧ); if (ok) {
                lit.Value.ValuePos = 99999;
            }
        }
        return true;
    });
    // Type check. The checker will consult the effective
    // version for the BasicLit 123. This used to panic.
    var pkg = NewPackage("p"u8, "p"u8);
    var check = NewChecker(Ꮡ(new Config(nil)), fset, pkg, nil);
    {
        var err = check.Files(new ж<ast.File>[]{f}.slice()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goBuildGo122PackagePVarˢ = (@string)"//go:build go1.22\n\npackage p; var _ int"u8;
internal static readonly object packageQFuncSFuncFuncˢ = (@string)"package q; func _(s func(func() bool)) { for range s {} }"u8;
internal static readonly @string rangeOverSVariableOfTypeˢ = "range over s (variable of type func(func() bool)): requires go1.23"u8;

// TestVersionWithoutPos is a regression test for issue #69477,
// in which the type checker would use position information
// to compute which file it is "in" based on syntax position.
//
// As a rule the type checker should not depend on position
// information for correctness, only for error messages and
// Object.Pos. (Scope.LookupParent was a mistake.)
//
// The Checker now holds the effective version in a state variable.
public static void TestVersionWithoutPos(ж<testing.T> Ꮡt) {
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, goBuildGo122PackagePVarˢ, 0);
    // Splice in a decl from another file. Its pos will be wrong.
    var (f2, _) = parser.ParseFile(fset, aGoˢ, packageQFuncSFuncFuncˢ, 0);
    f.Value.Decls[0] = (~f2).Decls[0];
    // Type check. The checker will consult the effective
    // version (1.22) for the for-range stmt to know whether
    // range-over-func are permitted: they are not.
    // (Previously, no error was reported.)
    var pkg = NewPackage("p"u8, "p"u8);
    var check = NewChecker(Ꮡ(new Config(nil)), fset, pkg, nil);
    var err = check.Files(new ж<ast.File>[]{f}.slice());
    @string got = fmt.Sprint(err);
    @string want = rangeOverSVariableOfTypeˢ;
    if (!strings.Contains(got, want)) {
        Ꮡt.Errorf("check error was %q, want substring %q"u8, got, want);
    }
}

} // end types_test_package
