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
using sort = sort_package;
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
using iter = iter_package;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

// nopos indicates an unknown position
internal static tokenꓸPos nopos;

internal static types.Importer defaultImporter(ж<token.FileSet> fset) {
    return importer.ForCompiler(fset, runtime.Compiler, default);
}

internal static ж<ast.File> mustParse(ж<token.FileSet> fset, @string src) {
    var (f, err) = parser.ParseFile(fset, pkgName(src), src, parser.ParseComments);
    if (err != nil) {
        throw panic(err); // so we don't need to pass *testing.T
    }
    return f;
}

internal static (ж<types.Package>, error) typecheck(@string src, ж<types.Config> Ꮡconf, ж<typesꓸInfo> info) {
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
    return Ꮡconf.Check(f.Name.Name, fset, [f], info);
}

internal static ж<types.Package> mustTypecheck(@string src, ж<types.Config> conf, ж<typesꓸInfo> info) {
    var (pkg, err) = typecheck(src, conf, info);
    if (err != nil) {
        throw panic(err); // so we don't need to pass *testing.T
    }
    return pkg;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string kwᶜ => ref kwᶜᵇ;
private static readonly @string kwᶜᵇ = "package "u8;

// pkgName extracts the package name from src, which must contain a package header.
internal static @string pkgName(@string src) {
    @string kw = kwᶜ;
    nint i = strings.Index(src, kw); if (i >= 0) {
        @string after = src.slice(i + len(kw));
        nint n = len(after);
        nint iΔ1 = strings.IndexAny(after, "\n\t ;/"); if (iΔ1 >= 0) {
            n = iΔ1;
        }
        return after.slice(0, n);
    }
    throw panic("missing package header: " + src);
}

internal partial struct TestValuesInfo_type /*dyn*/ {
    internal @string src;
    internal @string expr; // constant expression
    internal @string typ;  // constant type
    internal @string val;  // constant value
}

public static void TestValuesInfo(ж<testing.T> t) {
    slice<TestValuesInfo_type> tests = slice<TestValuesInfo_type>([
        new(@"package a0; const _ = false", @"false", @"untyped bool", @"false"),
        new(@"package a1; const _ = 0", @"0", @"untyped int", @"0"),
        new(@"package a2; const _ = 'A'", @"'A'", @"untyped rune", @"65"),
        new(@"package a3; const _ = 0.", @"0.", @"untyped float", @"0"),
        new(@"package a4; const _ = 0i", @"0i", @"untyped complex", @"(0 + 0i)"),
        new(@"package a5; const _ = ""foo""", @"""foo""", @"untyped string", @"""foo"""),

        new(@"package b0; var _ = false", @"false", @"bool", @"false"),
        new(@"package b1; var _ = 0", @"0", @"int", @"0"),
        new(@"package b2; var _ = 'A'", @"'A'", @"rune", @"65"),
        new(@"package b3; var _ = 0.", @"0.", @"float64", @"0"),
        new(@"package b4; var _ = 0i", @"0i", @"complex128", @"(0 + 0i)"),
        new(@"package b5; var _ = ""foo""", @"""foo""", @"string", @"""foo"""),

        new(@"package c0a; var _ = bool(false)", @"false", @"bool", @"false"),
        new(@"package c0b; var _ = bool(false)", @"bool(false)", @"bool", @"false"),
        new(@"package c0c; type T bool; var _ = T(false)", @"T(false)", @"c0c.T", @"false"),

        new(@"package c1a; var _ = int(0)", @"0", @"int", @"0"),
        new(@"package c1b; var _ = int(0)", @"int(0)", @"int", @"0"),
        new(@"package c1c; type T int; var _ = T(0)", @"T(0)", @"c1c.T", @"0"),

        new(@"package c2a; var _ = rune('A')", @"'A'", @"rune", @"65"),
        new(@"package c2b; var _ = rune('A')", @"rune('A')", @"rune", @"65"),
        new(@"package c2c; type T rune; var _ = T('A')", @"T('A')", @"c2c.T", @"65"),

        new(@"package c3a; var _ = float32(0.)", @"0.", @"float32", @"0"),
        new(@"package c3b; var _ = float32(0.)", @"float32(0.)", @"float32", @"0"),
        new(@"package c3c; type T float32; var _ = T(0.)", @"T(0.)", @"c3c.T", @"0"),

        new(@"package c4a; var _ = complex64(0i)", @"0i", @"complex64", @"(0 + 0i)"),
        new(@"package c4b; var _ = complex64(0i)", @"complex64(0i)", @"complex64", @"(0 + 0i)"),
        new(@"package c4c; type T complex64; var _ = T(0i)", @"T(0i)", @"c4c.T", @"(0 + 0i)"),

        new(@"package c5a; var _ = string(""foo"")", @"""foo""", @"string", @"""foo"""),
        new(@"package c5b; var _ = string(""foo"")", @"string(""foo"")", @"string", @"""foo"""),
        new(@"package c5c; type T string; var _ = T(""foo"")", @"T(""foo"")", @"c5c.T", @"""foo"""),
        new(@"package c5d; var _ = string(65)", @"65", @"untyped int", @"65"),
        new(@"package c5e; var _ = string('A')", @"'A'", @"untyped rune", @"65"),
        new(@"package c5f; type T string; var _ = T('A')", @"'A'", @"untyped rune", @"65"),

        new(@"package d0; var _ = []byte(""foo"")", @"""foo""", @"string", @"""foo"""),
        new(@"package d1; var _ = []byte(string(""foo""))", @"""foo""", @"string", @"""foo"""),
        new(@"package d2; var _ = []byte(string(""foo""))", @"string(""foo"")", @"string", @"""foo"""),
        new(@"package d3; type T []byte; var _ = T(""foo"")", @"""foo""", @"string", @"""foo"""),

        new(@"package e0; const _ = float32( 1e-200)", @"float32(1e-200)", @"float32", @"0"),
        new(@"package e1; const _ = float32(-1e-200)", @"float32(-1e-200)", @"float32", @"0"),
        new(@"package e2; const _ = float64( 1e-2000)", @"float64(1e-2000)", @"float64", @"0"),
        new(@"package e3; const _ = float64(-1e-2000)", @"float64(-1e-2000)", @"float64", @"0"),
        new(@"package e4; const _ = complex64( 1e-200)", @"complex64(1e-200)", @"complex64", @"(0 + 0i)"),
        new(@"package e5; const _ = complex64(-1e-200)", @"complex64(-1e-200)", @"complex64", @"(0 + 0i)"),
        new(@"package e6; const _ = complex128( 1e-2000)", @"complex128(1e-2000)", @"complex128", @"(0 + 0i)"),
        new(@"package e7; const _ = complex128(-1e-2000)", @"complex128(-1e-2000)", @"complex128", @"(0 + 0i)"),

        new(@"package f0 ; var _ float32 =  1e-200", @"1e-200", @"float32", @"0"),
        new(@"package f1 ; var _ float32 = -1e-200", @"-1e-200", @"float32", @"0"),
        new(@"package f2a; var _ float64 =  1e-2000", @"1e-2000", @"float64", @"0"),
        new(@"package f3a; var _ float64 = -1e-2000", @"-1e-2000", @"float64", @"0"),
        new(@"package f2b; var _         =  1e-2000", @"1e-2000", @"float64", @"0"),
        new(@"package f3b; var _         = -1e-2000", @"-1e-2000", @"float64", @"0"),
        new(@"package f4 ; var _ complex64  =  1e-200 ", @"1e-200", @"complex64", @"(0 + 0i)"),
        new(@"package f5 ; var _ complex64  = -1e-200 ", @"-1e-200", @"complex64", @"(0 + 0i)"),
        new(@"package f6a; var _ complex128 =  1e-2000i", @"1e-2000i", @"complex128", @"(0 + 0i)"),
        new(@"package f7a; var _ complex128 = -1e-2000i", @"-1e-2000i", @"complex128", @"(0 + 0i)"),
        new(@"package f6b; var _            =  1e-2000i", @"1e-2000i", @"complex128", @"(0 + 0i)"),
        new(@"package f7b; var _            = -1e-2000i", @"-1e-2000i", @"complex128", @"(0 + 0i)"),

        new(@"package g0; const (a = len([iota]int{}); b; c); const _ = c", @"c", @"int", @"2"), // go.dev/issue/22341
        new(@"package g1; var(j int32; s int; n = 1.0<<s == j)", @"1.0", @"int32", @"1")         // go.dev/issue/48422
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
            Types: new map<ast.Expr, types.TypeAndValue>()
        ), out var Ꮡinfo);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();

        // look for expression
        ast.Expr expr = default;
        foreach (var (e, _) in info.Types) {
            if (ExprString(e) == test.expr) {
                expr = e;
                break;
            }
        }
        if (expr == nil) {
            t.Errorf("package %s: no expression found for %s", name, test.expr);
            continue;
        }
        var tv = info.Types[expr];

        // check that type is correct
        {
            @string got = tv.Type.String(); if (got != test.typ) {
                t.Errorf("package %s: got type %s; want %s", name, got, test.typ);
                continue;
            }
        }

        // if we have a constant, check that value is correct
        if (tv.Value != nil) {
            @string got = tv.Value.ExactString(); if (got != test.val) {
                t.Errorf("package %s: got value %s; want %s", name, got, test.val);
            }
        } else {
            if (test.val != ""u8) {
                t.Errorf("package %s: no constant found; want %s", name, test.val);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string brokenᶜ => ref brokenᶜᵇ;
private static readonly @string brokenᶜᵇ = "package broken_"u8;

internal partial struct TestTypesInfo_type /*dyn*/ {
    internal @string src;
    internal @string expr; // expression
    internal @string typ;  // value type
}

public static void TestTypesInfo(ж<testing.T> t) {
    // Test sources that are not expected to typecheck must start with the broken prefix.
    @string broken = brokenᶜ;

    slice<TestTypesInfo_type> tests = slice<TestTypesInfo_type>([
        // single-valued expressions of untyped constants
        new(@"package b0; var x interface{} = false", @"false", @"bool"),
        new(@"package b1; var x interface{} = 0", @"0", @"int"),
        new(@"package b2; var x interface{} = 0.", @"0.", @"float64"),
        new(@"package b3; var x interface{} = 0i", @"0i", @"complex128"),
        new(@"package b4; var x interface{} = ""foo""", @"""foo""", @"string"),

        // uses of nil
        new(@"package n0; var _ *int = nil", @"nil", @"untyped nil"),
        new(@"package n1; var _ func() = nil", @"nil", @"untyped nil"),
        new(@"package n2; var _ []byte = nil", @"nil", @"untyped nil"),
        new(@"package n3; var _ map[int]int = nil", @"nil", @"untyped nil"),
        new(@"package n4; var _ chan int = nil", @"nil", @"untyped nil"),
        new(@"package n5; var _ interface{} = nil", @"nil", @"untyped nil"),
        new(@"package n6; import ""unsafe""; var _ unsafe.Pointer = nil", @"nil", @"untyped nil"),

        new(@"package n10; var (x *int; _ = x == nil)", @"nil", @"untyped nil"),
        new(@"package n11; var (x func(); _ = x == nil)", @"nil", @"untyped nil"),
        new(@"package n12; var (x []byte; _ = x == nil)", @"nil", @"untyped nil"),
        new(@"package n13; var (x map[int]int; _ = x == nil)", @"nil", @"untyped nil"),
        new(@"package n14; var (x chan int; _ = x == nil)", @"nil", @"untyped nil"),
        new(@"package n15; var (x interface{}; _ = x == nil)", @"nil", @"untyped nil"),
        new(@"package n15; import ""unsafe""; var (x unsafe.Pointer; _ = x == nil)", @"nil", @"untyped nil"),

        new(@"package n20; var _ = (*int)(nil)", @"nil", @"untyped nil"),
        new(@"package n21; var _ = (func())(nil)", @"nil", @"untyped nil"),
        new(@"package n22; var _ = ([]byte)(nil)", @"nil", @"untyped nil"),
        new(@"package n23; var _ = (map[int]int)(nil)", @"nil", @"untyped nil"),
        new(@"package n24; var _ = (chan int)(nil)", @"nil", @"untyped nil"),
        new(@"package n25; var _ = (interface{})(nil)", @"nil", @"untyped nil"),
        new(@"package n26; import ""unsafe""; var _ = unsafe.Pointer(nil)", @"nil", @"untyped nil"),

        new(@"package n30; func f(*int) { f(nil) }", @"nil", @"untyped nil"),
        new(@"package n31; func f(func()) { f(nil) }", @"nil", @"untyped nil"),
        new(@"package n32; func f([]byte) { f(nil) }", @"nil", @"untyped nil"),
        new(@"package n33; func f(map[int]int) { f(nil) }", @"nil", @"untyped nil"),
        new(@"package n34; func f(chan int) { f(nil) }", @"nil", @"untyped nil"),
        new(@"package n35; func f(interface{}) { f(nil) }", @"nil", @"untyped nil"),
        new(@"package n35; import ""unsafe""; func f(unsafe.Pointer) { f(nil) }", @"nil", @"untyped nil"),

        // comma-ok expressions
        new(@"package p0; var x interface{}; var _, _ = x.(int)",
            @"x.(int)",
            @"(int, bool)"
        ),
        new(@"package p1; var x interface{}; func _() { _, _ = x.(int) }",
            @"x.(int)",
            @"(int, bool)"
        ),
        new(@"package p2a; type mybool bool; var m map[string]complex128; var b mybool; func _() { _, b = m[""foo""] }",
            @"m[""foo""]",
            @"(complex128, p2a.mybool)"
        ),
        new(@"package p2b; var m map[string]complex128; var b bool; func _() { _, b = m[""foo""] }",
            @"m[""foo""]",
            @"(complex128, bool)"
        ),
        new(@"package p3; var c chan string; var _, _ = <-c",
            @"<-c",
            @"(string, bool)"
        ),

        // go.dev/issue/6796
        new(@"package issue6796_a; var x interface{}; var _, _ = (x.(int))",
            @"x.(int)",
            @"(int, bool)"
        ),
        new(@"package issue6796_b; var c chan string; var _, _ = (<-c)",
            @"(<-c)",
            @"(string, bool)"
        ),
        new(@"package issue6796_c; var c chan string; var _, _ = (<-c)",
            @"<-c",
            @"(string, bool)"
        ),
        new(@"package issue6796_d; var c chan string; var _, _ = ((<-c))",
            @"(<-c)",
            @"(string, bool)"
        ),
        new(@"package issue6796_e; func f(c chan string) { _, _ = ((<-c)) }",
            @"(<-c)",
            @"(string, bool)"
        ),

        // go.dev/issue/7060
        new(@"package issue7060_a; var ( m map[int]string; x, ok = m[0] )",
            @"m[0]",
            @"(string, bool)"
        ),
        new(@"package issue7060_b; var ( m map[int]string; x, ok interface{} = m[0] )",
            @"m[0]",
            @"(string, bool)"
        ),
        new(@"package issue7060_c; func f(x interface{}, ok bool, m map[int]string) { x, ok = m[0] }",
            @"m[0]",
            @"(string, bool)"
        ),
        new(@"package issue7060_d; var ( ch chan string; x, ok = <-ch )",
            @"<-ch",
            @"(string, bool)"
        ),
        new(@"package issue7060_e; var ( ch chan string; x, ok interface{} = <-ch )",
            @"<-ch",
            @"(string, bool)"
        ),
        new(@"package issue7060_f; func f(x interface{}, ok bool, ch chan string) { x, ok = <-ch }",
            @"<-ch",
            @"(string, bool)"
        ),

        // go.dev/issue/28277
        new(@"package issue28277_a; func f(...int)",
            @"...int",
            @"[]int"
        ),
        new(@"package issue28277_b; func f(a, b int, c ...[]struct{})",
            @"...[]struct{}",
            @"[][]struct{}"
        ),

        // go.dev/issue/47243
        new(@"package issue47243_a; var x int32; var _ = x << 3", @"3", @"untyped int"),
        new(@"package issue47243_b; var x int32; var _ = x << 3.", @"3.", @"untyped float"),
        new(@"package issue47243_c; var x int32; var _ = 1 << x", @"1 << x", @"int"),
        new(@"package issue47243_d; var x int32; var _ = 1 << x", @"1", @"int"),
        new(@"package issue47243_e; var x int32; var _ = 1 << 2", @"1", @"untyped int"),
        new(@"package issue47243_f; var x int32; var _ = 1 << 2", @"2", @"untyped int"),
        new(@"package issue47243_g; var x int32; var _ = int(1) << 2", @"2", @"untyped int"),
        new(@"package issue47243_h; var x int32; var _ = 1 << (2 << x)", @"1", @"int"),
        new(@"package issue47243_i; var x int32; var _ = 1 << (2 << x)", @"(2 << x)", @"untyped int"),
        new(@"package issue47243_j; var x int32; var _ = 1 << (2 << x)", @"2", @"untyped int"),

        // tests for broken code that doesn't type-check
        new(broken + @"x0; func _() { var x struct {f string}; x.f := 0 }", @"x.f", @"string"),
        new(broken + @"x1; func _() { var z string; type x struct {f string}; y := &x{q: z}}", @"z", @"string"),
        new(broken + @"x2; func _() { var a, b string; type x struct {f string}; z := &x{f: a, f: b,}}", @"b", @"string"),
        new(broken + @"x3; var x = panic("""");", @"panic", @"func(interface{})"),
        new(@"package x4; func _() { panic("""") }", @"panic", @"func(interface{})"),
        new(broken + @"x5; func _() { var x map[string][...]int; x = map[string][...]int{"""": {1,2,3}} }", @"x", @"map[string]invalid type"),

        // parameterized functions
        new(@"package p0; func f[T any](T) {}; var _ = f[int]", @"f", @"func[T any](T)"),
        new(@"package p1; func f[T any](T) {}; var _ = f[int]", @"f[int]", @"func(int)"),
        new(@"package p2; func f[T any](T) {}; func _() { f(42) }", @"f", @"func(int)"),
        new(@"package p3; func f[T any](T) {}; func _() { f[int](42) }", @"f[int]", @"func(int)"),
        new(@"package p4; func f[T any](T) {}; func _() { f[int](42) }", @"f", @"func[T any](T)"),
        new(@"package p5; func f[T any](T) {}; func _() { f(42) }", @"f(42)", @"()"),

        // type parameters
        new(@"package t0; type t[] int; var _ t", @"t", @"t0.t"), // t[] is a syntax error that is ignored in this test in favor of t
        new(@"package t1; type t[P any] int; var _ t[int]", @"t", @"t1.t[P any]"),
        new(@"package t2; type t[P interface{}] int; var _ t[int]", @"t", @"t2.t[P interface{}]"),
        new(@"package t3; type t[P, Q interface{}] int; var _ t[int, int]", @"t", @"t3.t[P, Q interface{}]"),
        new(broken + @"t4; type t[P, Q interface{ m() }] int; var _ t[int, int]", @"t", @"broken_t4.t[P, Q interface{m()}]"),

        // instantiated types must be sanitized
        new(@"package g0; type t[P any] int; var x struct{ f t[int] }; var _ = x.f", @"x.f", @"g0.t[int]"),

        // go.dev/issue/45096
        new(@"package issue45096; func _[T interface{ ~int8 | ~int16 | ~int32  }](x T) { _ = x < 0 }", @"0", @"T"),

        // go.dev/issue/47895
        new(@"package p; import ""unsafe""; type S struct { f int }; var s S; var _ = unsafe.Offsetof(s.f)", @"s.f", @"int"),

        // go.dev/issue/74303. Note that interface field types are synthetic, so
        // even though `func()` doesn't appear in the source, it appears in the
        // syntax tree.
        new(@"package p; type T interface { M(int) }", @"func(int)", @"func(int)"),

        // go.dev/issue/50093
        new(@"package u0a; func _[_ interface{int}]() {}", @"int", @"int"),
        new(@"package u1a; func _[_ interface{~int}]() {}", @"~int", @"~int"),
        new(@"package u2a; func _[_ interface{int | string}]() {}", @"int | string", @"int | string"),
        new(@"package u3a; func _[_ interface{int | string | ~bool}]() {}", @"int | string | ~bool", @"int | string | ~bool"),
        new(@"package u3a; func _[_ interface{int | string | ~bool}]() {}", @"int | string", @"int | string"),
        new(@"package u3a; func _[_ interface{int | string | ~bool}]() {}", @"~bool", @"~bool"),
        new(@"package u3a; func _[_ interface{int | string | ~float64|~bool}]() {}", @"int | string | ~float64", @"int | string | ~float64"),

        new(@"package u0b; func _[_ int]() {}", @"int", @"int"),
        new(@"package u1b; func _[_ ~int]() {}", @"~int", @"~int"),
        new(@"package u2b; func _[_ int | string]() {}", @"int | string", @"int | string"),
        new(@"package u3b; func _[_ int | string | ~bool]() {}", @"int | string | ~bool", @"int | string | ~bool"),
        new(@"package u3b; func _[_ int | string | ~bool]() {}", @"int | string", @"int | string"),
        new(@"package u3b; func _[_ int | string | ~bool]() {}", @"~bool", @"~bool"),
        new(@"package u3b; func _[_ int | string | ~float64|~bool]() {}", @"int | string | ~float64", @"int | string | ~float64"),

        new(@"package u0c; type _ interface{int}", @"int", @"int"),
        new(@"package u1c; type _ interface{~int}", @"~int", @"~int"),
        new(@"package u2c; type _ interface{int | string}", @"int | string", @"int | string"),
        new(@"package u3c; type _ interface{int | string | ~bool}", @"int | string | ~bool", @"int | string | ~bool"),
        new(@"package u3c; type _ interface{int | string | ~bool}", @"int | string", @"int | string"),
        new(@"package u3c; type _ interface{int | string | ~bool}", @"~bool", @"~bool"),
        new(@"package u3c; type _ interface{int | string | ~float64|~bool}", @"int | string | ~float64", @"int | string | ~float64"),

        // reverse type inference
        new(@"package r1; var _ func(int) = g; func g[P any](P) {}", @"g", @"func(int)"),
        new(@"package r2; var _ func(int) = g[int]; func g[P any](P) {}", @"g", @"func[P any](P)"), // go.dev/issues/60212
        new(@"package r3; var _ func(int) = g[int]; func g[P any](P) {}", @"g[int]", @"func(int)"),
        new(@"package r4; var _ func(int, string) = g; func g[P, Q any](P, Q) {}", @"g", @"func(int, string)"),
        new(@"package r5; var _ func(int, string) = g[int]; func g[P, Q any](P, Q) {}", @"g", @"func[P, Q any](P, Q)"), // go.dev/issues/60212
        new(@"package r6; var _ func(int, string) = g[int]; func g[P, Q any](P, Q) {}", @"g[int]", @"func(int, string)"),

        new(@"package s1; func _() { f(g) }; func f(func(int)) {}; func g[P any](P) {}", @"g", @"func(int)"),
        new(@"package s2; func _() { f(g[int]) }; func f(func(int)) {}; func g[P any](P) {}", @"g", @"func[P any](P)"), // go.dev/issues/60212
        new(@"package s3; func _() { f(g[int]) }; func f(func(int)) {}; func g[P any](P) {}", @"g[int]", @"func(int)"),
        new(@"package s4; func _() { f(g) }; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}", @"g", @"func(int, string)"),
        new(@"package s5; func _() { f(g[int]) }; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}", @"g", @"func[P, Q any](P, Q)"), // go.dev/issues/60212
        new(@"package s6; func _() { f(g[int]) }; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}", @"g[int]", @"func(int, string)"),

        new(@"package s7; func _() { f(g, h) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}", @"g", @"func(int, int)"),
        new(@"package s8; func _() { f(g, h) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}", @"h", @"func(int, string)"),
        new(@"package s9; func _() { f(g, h[int]) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}", @"h", @"func[P, Q any](P, Q)"), // go.dev/issues/60212
        new(@"package s10; func _() { f(g, h[int]) }; func f[P any](func(int, P), func(P, string)) {}; func g[P any](P, P) {}; func h[P, Q any](P, Q) {}", @"h[int]", @"func(int, string)"),

        // go.dev/issue/68639
        // parenthesized and pointer type expressions in various positions
        // - as variable type, not generic
        new(@"package qa1; type T int; var x T", @"T", @"qa1.T"),
        new(@"package qa2; type T int; var x (T)", @"T", @"qa2.T"),
        new(@"package qa3; type T int; var x (T)", @"(T)", @"qa3.T"),
        new(@"package qa4; type T int; var x ((T))", @"T", @"qa4.T"),
        new(@"package qa5; type T int; var x ((T))", @"(T)", @"qa5.T"),
        new(@"package qa6; type T int; var x ((T))", @"((T))", @"qa6.T"),
        new(@"package qa7; type T int; var x *T", @"T", @"qa7.T"),
        new(@"package qa8; type T int; var x *T", @"*T", @"*qa8.T"),
        new(@"package qa9; type T int; var x (*T)", @"T", @"qa9.T"),
        new(@"package qa10; type T int; var x (*T)", @"*T", @"*qa10.T"),
        new(@"package qa11; type T int; var x *(T)", @"T", @"qa11.T"),
        new(@"package qa12; type T int; var x *(T)", @"(T)", @"qa12.T"),
        new(@"package qa13; type T int; var x *(T)", @"*(T)", @"*qa13.T"),
        new(@"package qa14; type T int; var x (*(T))", @"(T)", @"qa14.T"),
        new(@"package qa15; type T int; var x (*(T))", @"*(T)", @"*qa15.T"),
        new(@"package qa16; type T int; var x (*(T))", @"(*(T))", @"*qa16.T"),

        // - as ordinary function parameter, not generic
        new(@"package qb1; type T int; func _(T)", @"T", @"qb1.T"),
        new(@"package qb2; type T int; func _((T))", @"T", @"qb2.T"),
        new(@"package qb3; type T int; func _((T))", @"(T)", @"qb3.T"),
        new(@"package qb4; type T int; func _(((T)))", @"T", @"qb4.T"),
        new(@"package qb5; type T int; func _(((T)))", @"(T)", @"qb5.T"),
        new(@"package qb6; type T int; func _(((T)))", @"((T))", @"qb6.T"),
        new(@"package qb7; type T int; func _(*T)", @"T", @"qb7.T"),
        new(@"package qb8; type T int; func _(*T)", @"*T", @"*qb8.T"),
        new(@"package qb9; type T int; func _((*T))", @"T", @"qb9.T"),
        new(@"package qb10; type T int; func _((*T))", @"*T", @"*qb10.T"),
        new(@"package qb11; type T int; func _(*(T))", @"T", @"qb11.T"),
        new(@"package qb12; type T int; func _(*(T))", @"(T)", @"qb12.T"),
        new(@"package qb13; type T int; func _(*(T))", @"*(T)", @"*qb13.T"),
        new(@"package qb14; type T int; func _((*(T)))", @"(T)", @"qb14.T"),
        new(@"package qb15; type T int; func _((*(T)))", @"*(T)", @"*qb15.T"),
        new(@"package qb16; type T int; func _((*(T)))", @"(*(T))", @"*qb16.T"),

        // - as method receiver, not generic
        new(@"package qc1; type T int; func (T) _() {}", @"T", @"qc1.T"),
        new(@"package qc2; type T int; func ((T)) _() {}", @"T", @"qc2.T"),
        new(@"package qc3; type T int; func ((T)) _() {}", @"(T)", @"qc3.T"),
        new(@"package qc4; type T int; func (((T))) _() {}", @"T", @"qc4.T"),
        new(@"package qc5; type T int; func (((T))) _() {}", @"(T)", @"qc5.T"),
        new(@"package qc6; type T int; func (((T))) _() {}", @"((T))", @"qc6.T"),
        new(@"package qc7; type T int; func (*T) _() {}", @"T", @"qc7.T"),
        new(@"package qc8; type T int; func (*T) _() {}", @"*T", @"*qc8.T"),
        new(@"package qc9; type T int; func ((*T)) _() {}", @"T", @"qc9.T"),
        new(@"package qc10; type T int; func ((*T)) _() {}", @"*T", @"*qc10.T"),
        new(@"package qc11; type T int; func (*(T)) _() {}", @"T", @"qc11.T"),
        new(@"package qc12; type T int; func (*(T)) _() {}", @"(T)", @"qc12.T"),
        new(@"package qc13; type T int; func (*(T)) _() {}", @"*(T)", @"*qc13.T"),
        new(@"package qc14; type T int; func ((*(T))) _() {}", @"(T)", @"qc14.T"),
        new(@"package qc15; type T int; func ((*(T))) _() {}", @"*(T)", @"*qc15.T"),
        new(@"package qc16; type T int; func ((*(T))) _() {}", @"(*(T))", @"*qc16.T"),

        // - as variable type, generic
        new(@"package qd1; type T[_ any] int; var x T[int]", @"T", @"qd1.T[_ any]"),
        new(@"package qd2; type T[_ any] int; var x (T[int])", @"T[int]", @"qd2.T[int]"),
        new(@"package qd3; type T[_ any] int; var x (T[int])", @"(T[int])", @"qd3.T[int]"),
        new(@"package qd4; type T[_ any] int; var x ((T[int]))", @"T", @"qd4.T[_ any]"),
        new(@"package qd5; type T[_ any] int; var x ((T[int]))", @"(T[int])", @"qd5.T[int]"),
        new(@"package qd6; type T[_ any] int; var x ((T[int]))", @"((T[int]))", @"qd6.T[int]"),
        new(@"package qd7; type T[_ any] int; var x *T[int]", @"T", @"qd7.T[_ any]"),
        new(@"package qd8; type T[_ any] int; var x *T[int]", @"*T[int]", @"*qd8.T[int]"),
        new(@"package qd9; type T[_ any] int; var x (*T[int])", @"T", @"qd9.T[_ any]"),
        new(@"package qd10; type T[_ any] int; var x (*T[int])", @"*T[int]", @"*qd10.T[int]"),
        new(@"package qd11; type T[_ any] int; var x *(T[int])", @"T[int]", @"qd11.T[int]"),
        new(@"package qd12; type T[_ any] int; var x *(T[int])", @"(T[int])", @"qd12.T[int]"),
        new(@"package qd13; type T[_ any] int; var x *(T[int])", @"*(T[int])", @"*qd13.T[int]"),
        new(@"package qd14; type T[_ any] int; var x (*(T[int]))", @"(T[int])", @"qd14.T[int]"),
        new(@"package qd15; type T[_ any] int; var x (*(T[int]))", @"*(T[int])", @"*qd15.T[int]"),
        new(@"package qd16; type T[_ any] int; var x (*(T[int]))", @"(*(T[int]))", @"*qd16.T[int]"),

        // - as ordinary function parameter, generic
        new(@"package qe1; type T[_ any] int; func _(T[int])", @"T", @"qe1.T[_ any]"),
        new(@"package qe2; type T[_ any] int; func _((T[int]))", @"T[int]", @"qe2.T[int]"),
        new(@"package qe3; type T[_ any] int; func _((T[int]))", @"(T[int])", @"qe3.T[int]"),
        new(@"package qe4; type T[_ any] int; func _(((T[int])))", @"T", @"qe4.T[_ any]"),
        new(@"package qe5; type T[_ any] int; func _(((T[int])))", @"(T[int])", @"qe5.T[int]"),
        new(@"package qe6; type T[_ any] int; func _(((T[int])))", @"((T[int]))", @"qe6.T[int]"),
        new(@"package qe7; type T[_ any] int; func _(*T[int])", @"T", @"qe7.T[_ any]"),
        new(@"package qe8; type T[_ any] int; func _(*T[int])", @"*T[int]", @"*qe8.T[int]"),
        new(@"package qe9; type T[_ any] int; func _((*T[int]))", @"T", @"qe9.T[_ any]"),
        new(@"package qe10; type T[_ any] int; func _((*T[int]))", @"*T[int]", @"*qe10.T[int]"),
        new(@"package qe11; type T[_ any] int; func _(*(T[int]))", @"T[int]", @"qe11.T[int]"),
        new(@"package qe12; type T[_ any] int; func _(*(T[int]))", @"(T[int])", @"qe12.T[int]"),
        new(@"package qe13; type T[_ any] int; func _(*(T[int]))", @"*(T[int])", @"*qe13.T[int]"),
        new(@"package qe14; type T[_ any] int; func _((*(T[int])))", @"(T[int])", @"qe14.T[int]"),
        new(@"package qe15; type T[_ any] int; func _((*(T[int])))", @"*(T[int])", @"*qe15.T[int]"),
        new(@"package qe16; type T[_ any] int; func _((*(T[int])))", @"(*(T[int]))", @"*qe16.T[int]"),

        // - as method receiver, generic
        new(@"package qf1; type T[_ any] int; func (T[_]) _() {}", @"T", @"qf1.T[_ any]"),
        new(@"package qf2; type T[_ any] int; func ((T[_])) _() {}", @"T[_]", @"qf2.T[_]"),
        new(@"package qf3; type T[_ any] int; func ((T[_])) _() {}", @"(T[_])", @"qf3.T[_]"),
        new(@"package qf4; type T[_ any] int; func (((T[_]))) _() {}", @"T", @"qf4.T[_ any]"),
        new(@"package qf5; type T[_ any] int; func (((T[_]))) _() {}", @"(T[_])", @"qf5.T[_]"),
        new(@"package qf6; type T[_ any] int; func (((T[_]))) _() {}", @"((T[_]))", @"qf6.T[_]"),
        new(@"package qf7; type T[_ any] int; func (*T[_]) _() {}", @"T", @"qf7.T[_ any]"),
        new(@"package qf8; type T[_ any] int; func (*T[_]) _() {}", @"*T[_]", @"*qf8.T[_]"),
        new(@"package qf9; type T[_ any] int; func ((*T[_])) _() {}", @"T", @"qf9.T[_ any]"),
        new(@"package qf10; type T[_ any] int; func ((*T[_])) _() {}", @"*T[_]", @"*qf10.T[_]"),
        new(@"package qf11; type T[_ any] int; func (*(T[_])) _() {}", @"T[_]", @"qf11.T[_]"),
        new(@"package qf12; type T[_ any] int; func (*(T[_])) _() {}", @"(T[_])", @"qf12.T[_]"),
        new(@"package qf13; type T[_ any] int; func (*(T[_])) _() {}", @"*(T[_])", @"*qf13.T[_]"),
        new(@"package qf14; type T[_ any] int; func ((*(T[_]))) _() {}", @"(T[_])", @"qf14.T[_]"),
        new(@"package qf15; type T[_ any] int; func ((*(T[_]))) _() {}", @"*(T[_])", @"*qf15.T[_]"),
        new(@"package qf16; type T[_ any] int; func ((*(T[_]))) _() {}", @"(*(T[_]))", @"*qf16.T[_]"),

        // For historic reasons, type parameters in receiver type expressions
        // are considered both definitions and uses and thus also show up in
        // the Info.Types map (see go.dev/issue/68670).
        new(@"package t1; type T[_ any] int; func (T[P]) _() {}", @"P", @"P"),
        new(@"package t2; type T[_, _ any] int; func (T[P, Q]) _() {}", @"P", @"P"),
        new(@"package t3; type T[_, _ any] int; func (T[P, Q]) _() {}", @"Q", @"Q")
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()), out var Ꮡinfo);
        @string name = default;
        if (strings.HasPrefix(test.src, broken)) {
            var (pkg, err) = typecheck(test.src, nil, Ꮡinfo);
            if (err == nil) {
                t.Errorf("package %s: expected to fail but passed", pkg.Name());
                continue;
            }
            if (pkg != nil) {
                name = pkg.Name();
            }
        } else {
            name = mustTypecheck(test.src, nil, Ꮡinfo).Name();
        }

        // look for expression type
        typesꓸType typ = default;
        foreach (var (e, tv) in info.Types) {
            if (ExprString(e) == test.expr) {
                typ = tv.Type;
                break;
            }
        }
        if (typ == nil) {
            t.Errorf("package %s: no type found for %s", name, test.expr);
            continue;
        }

        // check that type is correct
        @string got = typ.String(); if (got != test.typ) {
            t.Errorf("package %s: expr = %s: got %s; want %s", name, test.expr, got, test.typ);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string libᶜ => ref libᶜᵇ;
private static readonly @string libᶜᵇ = """
package lib

func F[P any](P) {}

type T[P any] []P

"""u8.Raw();

internal partial struct TestInstanceInfo_testInst /*dyn*/ {
    internal @string name;
    internal slice<@string> targs;
    internal @string typ;
}

internal partial struct TestInstanceInfo_type /*dyn*/ {
    internal @string src;
    internal slice<TestInstanceInfo_testInst> instances; // recorded instances in source order
}

internal partial interface TestInstanceInfo_typeᴛ1 /*dyn*/ {
    ж<types.TypeParamList> TypeParams();
}

public static void TestInstanceInfo(ж<testing.T> t) {
    @string lib = libᶜ;

    slice<TestInstanceInfo_type> tests = slice<TestInstanceInfo_type>([
        new(@"package p0; func f[T any](T) {}; func _() { f(42) }",
            [new(@"f", [@"int"], @"func(int)")]
        ),
        new(@"package p1; func f[T any](T) T { panic(0) }; func _() { f('@') }",
            [new(@"f", [@"rune"], @"func(rune) rune")]
        ),
        new(@"package p2; func f[T any](...T) T { panic(0) }; func _() { f(0i) }",
            [new(@"f", [@"complex128"], @"func(...complex128) complex128")]
        ),
        new(@"package p3; func f[A, B, C any](A, *B, []C) {}; func _() { f(1.2, new(string), []byte{}) }",
            [new(@"f", [@"float64", @"string", @"byte"], @"func(float64, *string, []byte)")]
        ),
        new(@"package p4; func f[A, B any](A, *B, ...[]B) {}; func _() { f(1.2, new(byte)) }",
            [new(@"f", [@"float64", @"byte"], @"func(float64, *byte, ...[]byte)")]
        ),

        new(@"package s1; func f[T any, P interface{*T}](x T) {}; func _(x string) { f(x) }",
            [new(@"f", [@"string", @"*string"], @"func(x string)")]
        ),
        new(@"package s2; func f[T any, P interface{*T}](x []T) {}; func _(x []int) { f(x) }",
            [new(@"f", [@"int", @"*int"], @"func(x []int)")]
        ),
        new(@"package s3; type C[T any] interface{chan<- T}; func f[T any, P C[T]](x []T) {}; func _(x []int) { f(x) }",
            [
                new(@"C", [@"T"], @"interface{chan<- T}"),
                new(@"f", [@"int", @"chan<- int"], @"func(x []int)")
            ]
        ),
        new(@"package s4; type C[T any] interface{chan<- T}; func f[T any, P C[T], Q C[[]*P]](x []T) {}; func _(x []int) { f(x) }",
            [
                new(@"C", [@"T"], @"interface{chan<- T}"),
                new(@"C", [@"[]*P"], @"interface{chan<- []*P}"),
                new(@"f", [@"int", @"chan<- int", @"chan<- []*chan<- int"], @"func(x []int)")
            ]
        ),

        new(@"package t1; func f[T any, P interface{*T}]() T { panic(0) }; func _() { _ = f[string] }",
            [new(@"f", [@"string", @"*string"], @"func() string")]
        ),
        new(@"package t2; func f[T any, P interface{*T}]() T { panic(0) }; func _() { _ = (f[string]) }",
            [new(@"f", [@"string", @"*string"], @"func() string")]
        ),
        new(@"package t3; type C[T any] interface{chan<- T}; func f[T any, P C[T], Q C[[]*P]]() []T { return nil }; func _() { _ = f[int] }",
            [
                new(@"C", [@"T"], @"interface{chan<- T}"),
                new(@"C", [@"[]*P"], @"interface{chan<- []*P}"),
                new(@"f", [@"int", @"chan<- int", @"chan<- []*chan<- int"], @"func() []int")
            ]
        ),
        new(@"package t4; type C[T any] interface{chan<- T}; func f[T any, P C[T], Q C[[]*P]]() []T { return nil }; func _() { _ = (f[int]) }",
            [
                new(@"C", [@"T"], @"interface{chan<- T}"),
                new(@"C", [@"[]*P"], @"interface{chan<- []*P}"),
                new(@"f", [@"int", @"chan<- int", @"chan<- []*chan<- int"], @"func() []int")
            ]
        ),
        new(@"package i0; import ""lib""; func _() { lib.F(42) }",
            [new(@"F", [@"int"], @"func(int)")]
        ),

        new(@"package duplfunc0; func f[T any](T) {}; func _() { f(42); f(""foo""); f[int](3) }",
            [
                new(@"f", [@"int"], @"func(int)"),
                new(@"f", [@"string"], @"func(string)"),
                new(@"f", [@"int"], @"func(int)")
            ]
        ),
        new(@"package duplfunc1; import ""lib""; func _() { lib.F(42); lib.F(""foo""); lib.F(3) }",
            [
                new(@"F", [@"int"], @"func(int)"),
                new(@"F", [@"string"], @"func(string)"),
                new(@"F", [@"int"], @"func(int)")
            ]
        ),

        new(@"package type0; type T[P interface{~int}] struct{ x P }; var _ T[int]",
            [new(@"T", [@"int"], @"struct{x int}")]
        ),
        new(@"package type1; type T[P interface{~int}] struct{ x P }; var _ (T[int])",
            [new(@"T", [@"int"], @"struct{x int}")]
        ),
        new(@"package type2; type T[P interface{~int}] struct{ x P }; var _ T[(int)]",
            [new(@"T", [@"int"], @"struct{x int}")]
        ),
        new(@"package type3; type T[P1 interface{~[]P2}, P2 any] struct{ x P1; y P2 }; var _ T[[]int, int]",
            [new(@"T", [@"[]int", @"int"], @"struct{x []int; y int}")]
        ),
        new(@"package type4; import ""lib""; var _ lib.T[int]",
            [new(@"T", [@"int"], @"[]int")]
        ),

        new(@"package dupltype0; type T[P interface{~int}] struct{ x P }; var x T[int]; var y T[int]",
            [
                new(@"T", [@"int"], @"struct{x int}"),
                new(@"T", [@"int"], @"struct{x int}")
            ]
        ),
        new(@"package dupltype1; type T[P ~int] struct{ x P }; func (r *T[Q]) add(z T[Q]) { r.x += z.x }",
            [
                new(@"T", [@"Q"], @"struct{x Q}"),
                new(@"T", [@"Q"], @"struct{x Q}")
            ]
        ),
        new(@"package dupltype1; import ""lib""; var x lib.T[int]; var y lib.T[int]; var z lib.T[string]",
            [
                new(@"T", [@"int"], @"[]int"),
                new(@"T", [@"int"], @"[]int"),
                new(@"T", [@"string"], @"[]string")
            ]
        ),
        new(@"package issue51803; func foo[T any](T) {}; func _() { foo[int]( /* leave arg away on purpose */ ) }",
            [new(@"foo", [@"int"], @"func(int)")]
        ),

        // reverse type inference
        new(@"package reverse1a; var f func(int) = g; func g[P any](P) {}",
            [new(@"g", [@"int"], @"func(int)")]
        ),
        new(@"package reverse1b; func f(func(int)) {}; func g[P any](P) {}; func _() { f(g) }",
            [new(@"g", [@"int"], @"func(int)")]
        ),
        new(@"package reverse2a; var f func(int, string) = g; func g[P, Q any](P, Q) {}",
            [new(@"g", [@"int", @"string"], @"func(int, string)")]
        ),
        new(@"package reverse2b; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}; func _() { f(g) }",
            [new(@"g", [@"int", @"string"], @"func(int, string)")]
        ),
        new(@"package reverse2c; func f(func(int, string)) {}; func g[P, Q any](P, Q) {}; func _() { f(g[int]) }",
            [new(@"g", [@"int", @"string"], @"func(int, string)")]
        ),
        // reverse3a not possible (cannot assign to generic function outside of argument passing)
        new(@"package reverse3b; func f[R any](func(int) R) {}; func g[P any](P) string { return """" }; func _() { f(g) }",
            [
                new(@"f", [@"string"], @"func(func(int) string)"),
                new(@"g", [@"int"], @"func(int) string")
            ]
        ),
        new(@"package reverse4a; var _, _ func([]int, *float32) = g, h; func g[P, Q any]([]P, *Q) {}; func h[R any]([]R, *float32) {}",
            [
                new(@"g", [@"int", @"float32"], @"func([]int, *float32)"),
                new(@"h", [@"int"], @"func([]int, *float32)")
            ]
        ),
        new(@"package reverse4b; func f(_, _ func([]int, *float32)) {}; func g[P, Q any]([]P, *Q) {}; func h[R any]([]R, *float32) {}; func _() { f(g, h) }",
            [
                new(@"g", [@"int", @"float32"], @"func([]int, *float32)"),
                new(@"h", [@"int"], @"func([]int, *float32)")
            ]
        ),
        new(@"package issue59956; func f(func(int), func(string), func(bool)) {}; func g[P any](P) {}; func _() { f(g, g, g) }",
            [
                new(@"g", [@"int"], @"func(int)"),
                new(@"g", [@"string"], @"func(string)"),
                new(@"g", [@"bool"], @"func(bool)")
            ]
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestInstanceInfo_type(), out var Ꮡtest);
        test = vᴛ1;

        var imports = new testImporter(0);
        ref var conf = ref heap<types.Config>(new Config(Importer: imports), out var Ꮡconf);
        var instMap = new map<ж<ast.Ident>, types.Instance>();
        var useMap = new map<ж<ast.Ident>, types.Object>();
        var importsʗ1 = imports;
        var instMapʗ1 = instMap;
        var useMapʗ1 = useMap;
        ж<types.Package> makePkg(@string src) {
            var (pkgΔ1, err) = typecheck(src, Ꮡconf, Ꮡ(new typesꓸInfo(Instances: instMapʗ1, Uses: useMapʗ1)));
            // allow error for issue51803
            if (err != nil && (pkgΔ1 == nil || pkgΔ1.Name() != "issue51803"u8)) {
                t.Fatal(err);
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
        t.Run(pkg.Name(), (ж<testing.T> tΔ1) => {
            // Sort instances in source order for stability.
            var instances = sortedInstances(instMapʗ2);
            {
                nint got = len(instances);
                nint want = len(testʗ1.instances); if (got != want) {
                    tΔ1.Fatalf("got %d instances, want %d", got, want);
                }
            }

            // Pairwise compare with the expected instances.
            foreach (var (ii, inst) in range(instances)) {
                slice<typesꓸType> targs = default;
                for (nint i = 0; i < inst.Inst.TypeArgs.Len(); i++) {
                    targs = append(targs, inst.Inst.TypeArgs.At(i));
                }
                var typ = inst.Inst.Type;

                var testInst = testʗ1.instances[ii];
                {
                    @string got = inst.Ident.Name; if (got != testInst.name) {
                        tΔ1.Fatalf("got name %s, want %s", got, testInst.name);
                    }
                }
                if (len(targs) != len(testInst.targs)) {
                    tΔ1.Fatalf("got %d type arguments; want %d", len(targs), len(testInst.targs));
                }
                foreach (var (i, targ) in range(targs)) {
                    @string got = targ.String(); if (got != testInst.targs[i]) {
                        tΔ1.Errorf("type argument %d: got %s; want %s", i, got, testInst.targs[i]);
                    }
                }
                {
                    @string got = typ.Underlying().String(); if (got != testInst.typ) {
                        tΔ1.Errorf("package %s: got %s; want %s", pkgʗ1.Name(), got, testInst.typ);
                    }
                }

                // Verify the invariant that re-instantiating the corresponding generic
                // type with TypeArgs results in an identical instance.
                var ptype = useMapʗ2[inst.Ident].Type();
                var (lister, _) = ptype._<TestInstanceInfo_typeᴛ1>(ᐧ);
                if (lister == nil || lister.TypeParams().Len() == 0) {
                    tΔ1.Fatalf("info.Types[%v] = %v, want parameterized type", inst.Ident.OrTypedNil(), ptype);
                }
                var (inst2, err) = Instantiate(nil, ptype, targs, true);
                if (err != nil) {
                    tΔ1.Errorf("Instantiate(%v, %v) failed: %v", ptype, targs, err);
                }
                if (!Identical(inst.Inst.Type, inst2)) {
                    tΔ1.Errorf("%v and %v are not identical", inst.Inst.Type, inst2);
                }
            }
        });
    }
}

partial struct recordedInstance {
    public ж<ast.Ident> Ident;
    public types.Instance Inst;
}

internal static slice<recordedInstance> /*instances*/ sortedInstances(map<ж<ast.Ident>, types.Instance> m) {
    slice<recordedInstance> instances = default;

    foreach (var (id, inst) in m) {
        instances = append(instances, new recordedInstance(id, inst));
    }
    slices.SortFunc(instances, (recordedInstance a, recordedInstance b) => types_internal_test_package.CmpPos(a.Ident.Pos(), b.Ident.Pos()));
    return instances;
}

internal partial struct TestDefsInfo_type /*dyn*/ {
    internal @string src;
    internal @string obj;
    internal @string want;
}

public static void TestDefsInfo(ж<testing.T> t) {
    slice<TestDefsInfo_type> tests = slice<TestDefsInfo_type>([
        new(@"package p0; const x = 42", @"x", @"const p0.x untyped int"),
        new(@"package p1; const x int = 42", @"x", @"const p1.x int"),
        new(@"package p2; var x int", @"x", @"var p2.x int"),
        new(@"package p3; type x int", @"x", @"type p3.x int"),
        new(@"package p4; func f()", @"f", @"func p4.f()"),
        new(@"package p5; func f() int { x, _ := 1, 2; return x }", @"_", @"var _ int"),

        // Tests using generics.
        new(@"package g0; type x[T any] int", @"x", @"type g0.x[T any] int"),
        new(@"package g1; func f[T any]() {}", @"f", @"func g1.f[T any]()"),
        new(@"package g2; type x[T any] int; func (*x[_]) m() {}", @"m", @"func (*g2.x[_]).m()"),

        // Type parameters in receiver type expressions are definitions.
        new(@"package r0; type T[_ any] int; func (T[P]) _() {}", @"P", @"type parameter P any"),
        new(@"package r1; type T[_, _ any] int; func (T[P, Q]) _() {}", @"P", @"type parameter P any"),
        new(@"package r2; type T[_, _ any] int; func (T[P, Q]) _() {}", @"Q", @"type parameter Q any")
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
            Defs: new map<ж<ast.Ident>, types.Object>()
        ), out var Ꮡinfo);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();

        // find object
        types.Object def = default;
        foreach (var (id, obj) in info.Defs) {
            if (id.Name == test.obj) {
                def = obj;
                break;
            }
        }
        if (def == nil) {
            t.Errorf("package %s: %s not found", name, test.obj);
            continue;
        }

        @string got = def.String(); if (got != test.want) {
            t.Errorf("package %s: got %s; want %s", name, got, test.want);
        }
    }
}

internal partial struct TestUsesInfo_type /*dyn*/ {
    internal @string src;
    internal @string obj;
    internal @string want;
}

public static void TestUsesInfo(ж<testing.T> t) {
    slice<TestUsesInfo_type> tests = slice<TestUsesInfo_type>([
        new(@"package p0; func _() { _ = x }; const x = 42", @"x", @"const p0.x untyped int"),
        new(@"package p1; func _() { _ = x }; const x int = 42", @"x", @"const p1.x int"),
        new(@"package p2; func _() { _ = x }; var x int", @"x", @"var p2.x int"),
        new(@"package p3; func _() { type _ x }; type x int", @"x", @"type p3.x int"),
        new(@"package p4; func _() { _ = f }; func f()", @"f", @"func p4.f()"),

        // Tests using generics.
        new(@"package g0; func _[T any]() { _ = x }; const x = 42", @"x", @"const g0.x untyped int"),
        new(@"package g1; func _[T any](x T) { }", @"T", @"type parameter T any"),
        new(@"package g2; type N[A any] int; var _ N[int]", @"N", @"type g2.N[A any] int"),
        new(@"package g3; type N[A any] int; func (N[_]) m() {}", @"N", @"type g3.N[A any] int"),

        // Uses of fields are instantiated.
        new(@"package s1; type N[A any] struct{ a A }; var f = N[int]{}.a", @"a", @"field a int"),
        new(@"package s2; type N[A any] struct{ a A }; func (r N[B]) m(b B) { r.a = b }", @"a", @"field a B"),

        // Uses of methods are uses of the instantiated method.
        new(@"package m0; type N[A any] int; func (r N[B]) m() { r.n() }; func (N[C]) n() {}", @"n", @"func (m0.N[B]).n()"),
        new(@"package m1; type N[A any] int; func (r N[B]) m() { }; var f = N[int].m", @"m", @"func (m1.N[int]).m()"),
        new(@"package m2; func _[A any](v interface{ m() A }) { v.m() }", @"m", @"func (interface).m() A"),
        new(@"package m3; func f[A any]() interface{ m() A } { return nil }; var _ = f[int]().m()", @"m", @"func (interface).m() int"),
        new(@"package m4; type T[A any] func() interface{ m() A }; var x T[int]; var y = x().m", @"m", @"func (interface).m() int"),
        new(@"package m5; type T[A any] interface{ m() A }; func _[B any](t T[B]) { t.m() }", @"m", @"func (m5.T[B]).m() B"),
        new(@"package m6; type T[A any] interface{ m() }; func _[B any](t T[B]) { t.m() }", @"m", @"func (m6.T[B]).m()"),
        new(@"package m7; type T[A any] interface{ m() A }; func _(t T[int]) { t.m() }", @"m", @"func (m7.T[int]).m() int"),
        new(@"package m8; type T[A any] interface{ m() }; func _(t T[int]) { t.m() }", @"m", @"func (m8.T[int]).m()"),
        new(@"package m9; type T[A any] interface{ m() }; func _(t T[int]) { _ = t.m }", @"m", @"func (m9.T[int]).m()"),
        new(
            @"package m10; type E[A any] interface{ m() }; type T[B any] interface{ E[B]; n() }; func _(t T[int]) { t.m() }",
            @"m",
            @"func (m10.E[int]).m()"
        ),
        new(@"package m11; type T[A any] interface{ m(); n() }; func _(t1 T[int], t2 T[string]) { t1.m(); t2.n() }", @"m", @"func (m11.T[int]).m()"),
        new(@"package m12; type T[A any] interface{ m(); n() }; func _(t1 T[int], t2 T[string]) { t1.m(); t2.n() }", @"n", @"func (m12.T[string]).n()"),

        // For historic reasons, type parameters in receiver type expressions
        // are considered both definitions and uses (see go.dev/issue/68670).
        new(@"package r0; type T[_ any] int; func (T[P]) _() {}", @"P", @"type parameter P any"),
        new(@"package r1; type T[_, _ any] int; func (T[P, Q]) _() {}", @"P", @"type parameter P any"),
        new(@"package r2; type T[_, _ any] int; func (T[P, Q]) _() {}", @"Q", @"type parameter Q any")
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
            Uses: new map<ж<ast.Ident>, types.Object>()
        ), out var Ꮡinfo);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();

        // find object
        types.Object use = default;
        foreach (var (id, obj) in info.Uses) {
            if (id.Name == test.obj) {
                if (use != nil) {
                    throw panic(fmt.Sprintf("multiple uses of %q"u8, id.Name));
                }
                use = obj;
            }
        }
        if (use == nil) {
            t.Errorf("package %s: %s not found", name, test.obj);
            continue;
        }

        @string got = use.String(); if (got != test.want) {
            t.Errorf("package %s: got %s; want %s", name, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePTypeNAAnyIntFuncˢ => ref packagePTypeNAAnyIntFuncˢᵇ;
private static readonly @string packagePTypeNAAnyIntFuncˢᵇ = """
package p

type N[A any] int

func (r N[B]) m() { r.m(); r.n() }

func (r *N[C]) n() {  }

"""u8.Raw();

public static void TestGenericMethodInfo(ж<testing.T> t) {
    @string src = packagePTypeNAAnyIntFuncˢ;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Uses: new map<ж<ast.Ident>, types.Object>(),
        Selections: new map<ж<ast.SelectorExpr>, ж<types.Selection>>()
    ), out var Ꮡinfo);
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check("p", fset, [f], Ꮡinfo);
    if (err != nil) {
        t.Fatal(err);
    }

    var N = pkg.Scope().Lookup("N").Type()._<ж<types.Named>>();

    // Find the generic methods stored on N.
    var (gm, gn) = (N.Method(0), N.Method(1));
    if (gm.Name() == "n"u8) {
        (gm, gn) = (gn, gm);
    }

    // Collect objects from info.
    ж<types.Func> dm = default;                          // the declared methods
    ж<types.Func> dn = default;
    ref var dmm = ref heap<ж<types.Func>>(out var Ꮡdmm); // the methods used in the body of m
    ref var dmn = ref heap<ж<types.Func>>(out var Ꮡdmn);
    foreach (var (_, decl) in range(f.Decls)) {
        var (fdecl, ok) = decl._<ж<ast.FuncDecl>>(ᐧ);
        if (!ok) {
            continue;
        }
        var def = info.Defs[fdecl.Name]._<ж<types.Func>>();
        var exprᴛ1 = fdecl.Name.Name;
        switch (ᐧ) {
        case {} when exprᴛ1 == "m"u8: {
            dm = def;
            ast.Inspect(new ast.BlockStmtжNode(fdecl.Body), (ast.Node n) => {
                var (call, okΔ2) = n._<ж<ast.CallExpr>>(ᐧ); if (okΔ2) {
                    var sel = call.Fun._<ж<ast.SelectorExpr>>();
                    var use = Ꮡinfo.Value.Uses[sel.Sel]._<ж<types.Func>>();
                    var selection = Ꮡinfo.Value.Selections[sel];
                    if (selection.Kind() != MethodVal) {
                        t.Errorf("Selection kind = %v, want %v", selection.Kind(), MethodVal);
                    }
                    if (!AreEqual(selection.Obj(), use)) {
                        t.Errorf("info.Selections contains %v, want %v", selection.Obj(), use.OrTypedNil());
                    }
                    var exprᴛ2 = sel.Sel.Name;
                    switch (ᐧ) {
                    case {} when exprᴛ2 == "m"u8: {
                        Ꮡdmm.ValueSlot = use;
                        break;
                    }
                    case {} when exprᴛ2 == "n"u8: {
                        Ꮡdmn.ValueSlot = use;
                        break;
                    }}
                }
                return true;
            });
            break;
        }
        case {} when exprᴛ1 == "n"u8: {
            dn = def;
            break;
        }}
    }

    if (gm != dm) {
        t.Errorf(@"N.Method(...) returns %v for ""m"", but Info.Defs has %v", gm.OrTypedNil(), dm.OrTypedNil());
    }
    if (gn != dn) {
        t.Errorf(@"N.Method(...) returns %v for ""m"", but Info.Defs has %v", gm.OrTypedNil(), dm.OrTypedNil());
    }
    if (dmm != dm) {
        t.Errorf(@"Inside ""m"", r.m uses %v, want the defined func %v", dmm.OrTypedNil(), dm.OrTypedNil());
    }
    if (dmn == dn) {
        t.Errorf(@"Inside ""m"", r.n uses %v, want a func distinct from %v", dmm.OrTypedNil(), dm.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string importSpecˢ => ref importSpecˢᵇ;
private static readonly @string importSpecˢᵇ = "importSpec"u8;
internal static ref readonly @string caseClauseˢ => ref caseClauseˢᵇ;
private static readonly @string caseClauseˢᵇ = "caseClause"u8;
internal static ref readonly @string fieldˢ => ref fieldˢᵇ;
private static readonly @string fieldˢᵇ = "field"u8;

internal partial struct TestImplicitsInfo_type /*dyn*/ {
    internal @string src;
    internal @string want;
}

public static void TestImplicitsInfo(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    slice<TestImplicitsInfo_type> tests = slice<TestImplicitsInfo_type>([
        new(@"package p2; import . ""fmt""; var _ = Println", ""),           // no Implicits entry
        new(@"package p0; import local ""fmt""; var _ = local.Println", ""), // no Implicits entry
        new(@"package p1; import ""fmt""; var _ = fmt.Println", "importSpec: package fmt"),

        new(@"package p3; func f(x interface{}) { switch x.(type) { case int: } }", ""), // no Implicits entry
        new(@"package p4; func f(x interface{}) { switch t := x.(type) { case int: _ = t } }", "caseClause: var t int"),
        new(@"package p5; func f(x interface{}) { switch t := x.(type) { case int, uint: _ = t } }", "caseClause: var t interface{}"),
        new(@"package p6; func f(x interface{}) { switch t := x.(type) { default: _ = t } }", "caseClause: var t interface{}"),

        new(@"package p7; func f(x int) {}", ""), // no Implicits entry
        new(@"package p8; func f(int) {}", "field: var  int"),
        new(@"package p9; func f() (complex64) { return 0 }", "field: var  complex64"),
        new(@"package p10; type T struct{}; func (*T) f() {}", "field: var  *p10.T"),

        // Tests using generics.
        new(@"package f0; func f[T any](x int) {}", ""), // no Implicits entry
        new(@"package f1; func f[T any](int) {}", "field: var  int"),
        new(@"package f2; func f[T any](T) {}", "field: var  T"),
        new(@"package f3; func f[T any]() (complex64) { return 0 }", "field: var  complex64"),
        new(@"package f4; func f[T any](t T) (T) { return t }", "field: var  T"),
        new(@"package t0; type T[A any] struct{}; func (*T[_]) f() {}", "field: var  *t0.T[_]"),
        new(@"package t1; type T[A any] struct{}; func _(x interface{}) { switch t := x.(type) { case T[int]: _ = t } }", "caseClause: var t t1.T[int]"),
        new(@"package t2; type T[A any] struct{}; func _[P any](x interface{}) { switch t := x.(type) { case T[P]: _ = t } }", "caseClause: var t t2.T[P]"),
        new(@"package t3; func _[P any](x interface{}) { switch t := x.(type) { case P: _ = t } }", "caseClause: var t P")
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
            Implicits: new map<ast.Node, types.Object>()
        ), out var Ꮡinfo);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();

        // the test cases expect at most one Implicits entry
        if (len(info.Implicits) > 1) {
            t.Errorf("package %s: %d Implicits entries found", name, len(info.Implicits));
            continue;
        }

        // extract Implicits entry, if any
        @string got = default;
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
                t.Fatalf("package %s: unexpected %T", name, x);
                break;
            }}
            got += ": "u8 + obj.String();
        }

        // verify entry
        if (got != test.want) {
            t.Errorf("package %s: got %q; want %q", name, got, test.want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = """

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

"""u8.Raw();

internal partial struct TestPkgNameOf_type /*dyn*/ {
    internal @string path; // path string enclosed in "'s
    internal @string want;
}

public static void TestPkgNameOf(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    @string src = srcᶜ;

    slice<TestPkgNameOf_type> tests = slice<TestPkgNameOf_type>([
        new(@"""os""", "."),
        new(@"""io""", "_"),
        new(@"""math""", "math"),
        new(@"""path/filepath""", "filepath"),
        new(@"""sort""", "snort")
    ]);

    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Implicits: new map<ast.Node, types.Object>()
    ), out var Ꮡinfo);
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    conf.Importer = defaultImporter(fset);
    var (_, err) = Ꮡconf.Check("p", fset, [f], Ꮡinfo);
    if (err != nil) {
        t.Fatal(err);
    }

    // map import paths to importDecl
    var imports = new map<@string, ж<ast.ImportSpec>>();
    foreach (var (_, s) in range(f.Decls[0]._<ж<ast.GenDecl>>().Specs)) {
        var (imp, _) = s._<ж<ast.ImportSpec>>(ᐧ); if (imp != nil) {
            imports[(~imp.Path).Value] = imp;
        }
    }

    foreach (var (_, test) in range(tests)) {
        var imp = imports[test.path];
        if (imp == nil) {
            t.Fatalf("invalid test case: import path %s not found", test.path);
        }
        var got = info.PkgNameOf(imp);
        if (got == nil) {
            t.Fatalf("import %s: package name not found", test.path);
        }
        if (got.Name() != test.want) {
            t.Errorf("import %s: got %s; want %s", test.path, got.Name(), test.want);
        }
    }

    // test non-existing importDecl
    {
        var got = info.PkgNameOf(@new<ast.ImportSpec>()); if (got != nil) {
            t.Errorf("got %s for non-existing import declaration", got.Name());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string voidˢ => ref voidˢᵇ;
private static readonly @string voidˢᵇ = "void"u8;
internal static ref readonly @string typeˢ => ref typeˢᵇ;
private static readonly @string typeˢᵇ = "type"u8;
internal static ref readonly @string builtinˢ => ref builtinˢᵇ;
private static readonly @string builtinˢᵇ = "builtin"u8;
internal static ref readonly @string constˢ => ref constˢᵇ;
private static readonly @string constˢᵇ = "const"u8;
internal static ref readonly @string valueˢ => ref valueˢᵇ;
private static readonly @string valueˢᵇ = "value"u8;
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "nil"u8;
internal static ref readonly @string addressableˢ => ref addressableˢᵇ;
private static readonly @string addressableˢᵇ = "addressable"u8;
internal static ref readonly @string assignableˢ => ref assignableˢᵇ;
private static readonly @string assignableˢᵇ = "assignable"u8;
internal static ref readonly @string hasOkˢ => ref hasOkˢᵇ;
private static readonly @string hasOkˢᵇ = "hasOk"u8;
internal static ref readonly @string invalidˢ => ref invalidˢᵇ;
private static readonly @string invalidˢᵇ = "invalid"u8;

internal static @string predString(types.TypeAndValue tv) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    void pred(bool b, @string s) {
        if (b) {
            if (Ꮡbuf.Value.Len() > 0) {
                Ꮡbuf.WriteString(", ");
            }
            Ꮡbuf.WriteString(s);
        }
    }

    pred(tv.IsVoid(), voidˢ);
    pred(tv.IsType(), typeˢ);
    pred(tv.IsBuiltin(), builtinˢ);
    pred(tv.IsValue() && tv.Value != nil, constˢ);
    pred(tv.IsValue() && tv.Value == nil, valueˢ);
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
internal static ref readonly @string missingˢ => ref missingˢᵇ;
private static readonly @string missingˢᵇ = "<missing>"u8;

internal partial struct TestPredicatesInfo_type /*dyn*/ {
    internal @string src;
    internal @string expr;
    internal @string pred;
}

public static void TestPredicatesInfo(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    slice<TestPredicatesInfo_type> tests = slice<TestPredicatesInfo_type>([
        // void
        new(@"package n0; func f() { f() }", @"f()", @"void"),

        // types
        new(@"package t0; type _ int", @"int", @"type"),
        new(@"package t1; type _ []int", @"[]int", @"type"),
        new(@"package t2; type _ func()", @"func()", @"type"),
        new(@"package t3; type _ func(int)", @"int", @"type"),
        new(@"package t3; type _ func(...int)", @"...int", @"type"),

        // built-ins
        new(@"package b0; var _ = len("""")", @"len", @"builtin"),
        new(@"package b1; var _ = (len)("""")", @"(len)", @"builtin"),

        // constants
        new(@"package c0; var _ = 42", @"42", @"const"),
        new(@"package c1; var _ = ""foo"" + ""bar""", @"""foo"" + ""bar""", @"const"),
        new(@"package c2; const (i = 1i; _ = i)", @"i", @"const"),

        // values
        new(@"package v0; var (a, b int; _ = a + b)", @"a + b", @"value"),
        new(@"package v1; var _ = &[]int{1}", @"[]int{…}", @"value"),
        new(@"package v2; var _ = func(){}", @"(func() literal)", @"value"),
        new(@"package v4; func f() { _ = f }", @"f", @"value"),
        new(@"package v3; var _ *int = nil", @"nil", @"value, nil"),
        new(@"package v3; var _ *int = (nil)", @"(nil)", @"value, nil"),

        // addressable (and thus assignable) operands
        new(@"package a0; var (x int; _ = x)", @"x", @"value, addressable, assignable"),
        new(@"package a1; var (p *int; _ = *p)", @"*p", @"value, addressable, assignable"),
        new(@"package a2; var (s []int; _ = s[0])", @"s[0]", @"value, addressable, assignable"),
        new(@"package a3; var (s struct{f int}; _ = s.f)", @"s.f", @"value, addressable, assignable"),
        new(@"package a4; var (a [10]int; _ = a[0])", @"a[0]", @"value, addressable, assignable"),
        new(@"package a5; func _(x int) { _ = x }", @"x", @"value, addressable, assignable"),
        new(@"package a6; func _()(x int) { _ = x; return }", @"x", @"value, addressable, assignable"),
        new(@"package a7; type T int; func (x T) _() { _ = x }", @"x", @"value, addressable, assignable"),
        // composite literals are not addressable

        // assignable but not addressable values
        new(@"package s0; var (m map[int]int; _ = m[0])", @"m[0]", @"value, assignable, hasOk"),
        new(@"package s1; var (m map[int]int; _, _ = m[0])", @"m[0]", @"value, assignable, hasOk"),

        // hasOk expressions
        new(@"package k0; var (ch chan int; _ = <-ch)", @"<-ch", @"value, hasOk"),
        new(@"package k1; var (ch chan int; _, _ = <-ch)", @"<-ch", @"value, hasOk"),

        // missing entries
        // - package names are collected in the Uses map
        // - identifiers being declared are collected in the Defs map
        new(@"package m0; import ""os""; func _() { _ = os.Stdout }", @"os", @"<missing>"),
        new(@"package m1; import p ""os""; func _() { _ = p.Stdout }", @"p", @"<missing>"),
        new(@"package m2; const c = 0", @"c", @"<missing>"),
        new(@"package m3; type T int", @"T", @"<missing>"),
        new(@"package m4; var v int", @"v", @"<missing>"),
        new(@"package m5; func f() {}", @"f", @"<missing>"),
        new(@"package m6; func _(x int) {}", @"x", @"<missing>"),
        new(@"package m6; func _()(x int) { return }", @"x", @"<missing>"),
        new(@"package m6; type T int; func (x T) _() {}", @"x", @"<missing>")
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()), out var Ꮡinfo);
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
            t.Errorf("package %s: got %s; want %s", name, got, test.pred);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownNodeKindˢ => ref unknownNodeKindˢᵇ;
private static readonly @string unknownNodeKindˢᵇ = "<unknown node kind>"u8;
internal static ref readonly @string fileˢ => ref fileˢᵇ;
private static readonly @string fileˢᵇ = "file"u8;
internal static ref readonly @string funcˢ => ref funcˢᵇ;
private static readonly @string funcˢᵇ = "func"u8;
internal static ref readonly @string blockˢ => ref blockˢᵇ;
private static readonly @string blockˢᵇ = "block"u8;
internal static ref readonly @string switchˢ => ref switchˢᵇ;
private static readonly @string switchˢᵇ = "switch"u8;
internal static ref readonly @string typeSwitchˢ => ref typeSwitchˢᵇ;
private static readonly @string typeSwitchˢᵇ = "type switch"u8;
internal static ref readonly @string caseˢ => ref caseˢᵇ;
private static readonly @string caseˢᵇ = "case"u8;
internal static ref readonly @string commˢ => ref commˢᵇ;
private static readonly @string commˢᵇ = "comm"u8;
internal static ref readonly @string forˢ => ref forˢᵇ;
private static readonly @string forˢᵇ = "for"u8;
internal static ref readonly @string rangeˢ => ref rangeˢᵇ;
private static readonly @string rangeˢᵇ = "range"u8;

internal partial struct TestScopesInfo_type /*dyn*/ {
    internal @string src;
    internal slice<@string> scopes; // list of scope descriptors of the form kind:varlist
}

public static void TestScopesInfo(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    slice<TestScopesInfo_type> tests = slice<TestScopesInfo_type>([
        new(@"package p0", [
            "file:"
        ]),
        new(@"package p1; import ( ""fmt""; m ""math""; _ ""os"" ); var ( _ = fmt.Println; _ = m.Pi )", [
            "file:fmt m"
        ]),
        new(@"package p2; func _() {}", [
            "file:", "func:"
        ]),
        new(@"package p3; func _(x, y int) {}", [
            "file:", "func:x y"
        ]),
        new(@"package p4; func _(x, y int) { x, z := 1, 2; _ = z }", [
            "file:", "func:x y z" // redeclaration of x
        ]),
        new(@"package p5; func _(x, y int) (u, _ int) { return }", [
            "file:", "func:u x y"
        ]),
        new(@"package p6; func _() { { var x int; _ = x } }", [
            "file:", "func:", "block:x"
        ]),
        new(@"package p7; func _() { if true {} }", [
            "file:", "func:", "if:", "block:"
        ]),
        new(@"package p8; func _() { if x := 0; x < 0 { y := x; _ = y } }", [
            "file:", "func:", "if:x", "block:y"
        ]),
        new(@"package p9; func _() { switch x := 0; x {} }", [
            "file:", "func:", "switch:x"
        ]),
        new(@"package p10; func _() { switch x := 0; x { case 1: y := x; _ = y; default: }}", [
            "file:", "func:", "switch:x", "case:y", "case:"
        ]),
        new(@"package p11; func _(t interface{}) { switch t.(type) {} }", [
            "file:", "func:t", "type switch:"
        ]),
        new(@"package p12; func _(t interface{}) { switch t := t; t.(type) {} }", [
            "file:", "func:t", "type switch:t"
        ]),
        new(@"package p13; func _(t interface{}) { switch x := t.(type) { case int: _ = x } }", [
            "file:", "func:t", "type switch:", "case:x" // x implicitly declared
        ]),
        new(@"package p14; func _() { select{} }", [
            "file:", "func:"
        ]),
        new(@"package p15; func _(c chan int) { select{ case <-c: } }", [
            "file:", "func:c", "comm:"
        ]),
        new(@"package p16; func _(c chan int) { select{ case i := <-c: x := i; _ = x} }", [
            "file:", "func:c", "comm:i x"
        ]),
        new(@"package p17; func _() { for{} }", [
            "file:", "func:", "for:", "block:"
        ]),
        new(@"package p18; func _(n int) { for i := 0; i < n; i++ { _ = i } }", [
            "file:", "func:n", "for:i", "block:"
        ]),
        new(@"package p19; func _(a []int) { for i := range a { _ = i} }", [
            "file:", "func:a", "range:i", "block:"
        ]),
        new(@"package p20; var s int; func _(a []int) { for i, x := range a { s += x; _ = i } }", [
            "file:", "func:a", "range:i x", "block:"
        ])
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(Scopes: new map<ast.Node, ж<typesꓸScope>>()), out var Ꮡinfo);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();

        // number of scopes must match
        if (len(info.Scopes) != len(test.scopes)) {
            t.Errorf("package %s: got %d scopes; want %d", name, len(info.Scopes), len(test.scopes));
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
                kind = "if";
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
            @string desc = kind + ":"u8 + strings.Join(scope.Names(), " ");
            if (!slices.Contains(test.scopes, desc)) {
                t.Errorf("package %s: no matching scope found for %s", name, desc);
            }
        }
    }
}

internal partial struct TestInitOrderInfo_type /*dyn*/ {
    internal @string src;
    internal slice<@string> inits;
}

public static void TestInitOrderInfo(ж<testing.T> t) {
    slice<TestInitOrderInfo_type> tests = slice<TestInitOrderInfo_type>([
        new(@"package p0; var (x = 1; y = x)", [
            "x = 1", "y = x"
        ]),
        new(@"package p1; var (a = 1; b = 2; c = 3)", [
            "a = 1", "b = 2", "c = 3"
        ]),
        new(@"package p2; var (a, b, c = 1, 2, 3)", [
            "a = 1", "b = 2", "c = 3"
        ]),
        new(@"package p3; var _ = f(); func f() int { return 1 }", [
            "_ = f()" // blank var
        ]),
        new(@"package p4; var (a = 0; x = y; y = z; z = 0)", [
            "a = 0", "z = 0", "y = z", "x = y"
        ]),
        new(@"package p5; var (a, _ = m[0]; m map[int]string)", [
            "a, _ = m[0]" // blank var
        ]),
        new(@"package p6; var a, b = f(); func f() (_, _ int) { return z, z }; var z = 0", [
            "z = 0", "a, b = f()"
        ]),
        new(@"package p7; var (a = func() int { return b }(); b = 1)", [
            "b = 1", "a = (func() int literal)()"
        ]),
        new(@"package p8; var (a, b = func() (_, _ int) { return c, c }(); c = 1)", [
            "c = 1", "a, b = (func() (_, _ int) literal)()"
        ]),
        new(@"package p9; type T struct{}; func (T) m() int { _ = y; return 0 }; var x, y = T.m, 1", [
            "y = 1", "x = T.m"
        ]),
        new(@"package p10; var (d = c + b; a = 0; b = 0; c = 0)", [
            "a = 0", "b = 0", "c = 0", "d = c + b"
        ]),
        new(@"package p11; var (a = e + c; b = d + c; c = 0; d = 0; e = 0)", [
            "c = 0", "d = 0", "b = d + c", "e = 0", "a = e + c"
        ]),
        // emit an initializer for n:1 initializations only once (not for each node
        // on the lhs which may appear in different order in the dependency graph)
        new(@"package p12; var (a = x; b = 0; x, y = m[0]; m map[int]int)", [
            "b = 0", "x, y = m[0]", "a = x"
        ]),
        // test case from spec section on package initialization
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
"""u8.Raw(), [
            "d = 3", "b = f()", "c = f()", "a = c + b"
        ]),
        // test case for go.dev/issue/7131
        new("""
package main

		var counter int
		func next() int { counter++; return counter }

		var _ = makeOrder()
		func makeOrder() []int { return []int{f, b, d, e, c, a} }

		var a       = next()
		var b, c    = next(), next()
		var d, e, f = next(), next(), next()
		
"""u8.Raw(), [
            "a = next()", "b = next()", "c = next()", "d = next()", "e = next()", "f = next()", "_ = makeOrder()"
        ]),
        // test case for go.dev/issue/10709
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
"""u8.Raw(), [
            "t = makeT(0)", "v = t.m()"
        ]),
        // test case for go.dev/issue/10709: same as test before, but variable decls swapped
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
"""u8.Raw(), [
            "t = makeT(0)", "v = t.m()"
        ]),
        // another candidate possibly causing problems with go.dev/issue/10709
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
"""u8.Raw(), [
            "x1 = 0", "y1 = f1()", "x2 = 0", "y2 = f2()"
        ])
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(), out var Ꮡinfo);
        @string name = mustTypecheck(test.src, nil, Ꮡinfo).Name();

        // number of initializers must match
        if (len(info.InitOrder) != len(test.inits)) {
            t.Errorf("package %s: got %d initializers; want %d", name, len(info.InitOrder), len(test.inits));
            continue;
        }

        // initializers must match
        foreach (var (i, want) in range(test.inits)) {
            @string got = info.InitOrder[i].String();
            if (got != want) {
                t.Errorf("package %s, init %d: got %s; want %s", name, i, got, want);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packageMainVarA1ˢ => ref packageMainVarA1ˢᵇ;
private static readonly @string packageMainVarA1ˢᵇ = @"package main; var a = 1"u8;
internal static ref readonly @string packageMainVarB2ˢ => ref packageMainVarB2ˢᵇ;
private static readonly @string packageMainVarB2ˢᵇ = @"package main; var b = 2"u8;
internal static ref readonly @string mainˢ => ref mainˢᵇ;
private static readonly @string mainˢᵇ = "main"u8;

internal partial struct TestMultiFileInitOrder_type /*dyn*/ {
    internal slice<ж<ast.File>> files;
    internal @string want;
}

public static void TestMultiFileInitOrder(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var fileA = mustParse(fset, packageMainVarA1ˢ);
    var fileB = mustParse(fset, packageMainVarB2ˢ);

    // The initialization order must not depend on the parse
    // order of the files, only on the presentation order to
    // the type-checker.
    foreach (var (_, test) in range(slice<TestMultiFileInitOrder_type>([
        new([fileA, fileB], "[a = 1 b = 2]"),
        new([fileB, fileA], "[b = 2 a = 1]")
    ]))) {
        ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
        var (_, err) = @new<types.Config>().Check(mainˢ, fset, test.files, Ꮡinfo); if (err != nil) {
            t.Fatal(err);
        }
        @string got = fmt.Sprint(info.InitOrder); if (got != test.want) {
            t.Fatalf("got %s; want %s", got, test.want);
        }
    }
}

public static void TestFiles(ж<testing.T> t) {
    slice<@string> sources = slice<@string>([
        "package p; type T struct{}; func (T) m1() {}",
        "package p; func (T) m2() {}; var x interface{ m1(); m2() } = T{}",
        "package p; func (T) m3() {}; var y interface{ m1(); m2(); m3() } = T{}",
        "package p"
    ]);

    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var fset = token.NewFileSet();
    var pkg = NewPackage("p", "p");
    ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
    var check = NewChecker(Ꮡconf, fset, pkg, Ꮡinfo);

    foreach (var (_, src) in range(sources)) {
        var err = check.Files(slice<ж<ast.File>>([mustParse(fset, src)])); if (err != nil) {
            t.Error(err);
        }
    }

    // check InitOrder is [x y]
    slice<@string> vars = default;
    foreach (var (_, init) in range(info.InitOrder)) {
        foreach (var (_, v) in range(init.Lhs)) {
            vars = append(vars, v.Name());
        }
    }
    @string got = fmt.Sprint(vars);
    @string want = "[x y]"; if (got != want) {
        t.Errorf("InitOrder == %s, want %s", got, want);
    }
}

partial struct testImporter /*map[@string, ж<types.Package>]*/;

internal static (ж<types.Package>, error) Import(this testImporter m, @string path) {
    var pkg = m[path]; if (pkg != nil) {
        return (pkg, default);
    }
    return (default, fmt.Errorf("package %q not found"u8, path));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string libˢ => ref libˢᵇ;
private static readonly @string libˢᵇ = "lib"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string libSrcᶜ => ref libSrcᶜᵇ;
private static readonly @string libSrcᶜᵇ = """

package lib
type T float64
const C T = 3
var V T
func F() {}
func (T) M() {}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string mainSrcᶜ => ref mainSrcᶜᵇ;
private static readonly @string mainSrcᶜᵇ = """

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
"""u8.Raw();

public static void TestSelection(ж<testing.T> t) {
    var selections = new map<ж<ast.SelectorExpr>, ж<types.Selection>>();

    // We need a specific fileset in this test below for positions.
    // Cannot use typecheck helper.
    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(new Config(Importer: imports), out var Ꮡconf);
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    var selectionsʗ1 = selections;
    void makePkg(@string path, @string src) {
        var (pkg, err) = Ꮡconf.Check(path, fsetʗ1, slice<ж<ast.File>>([mustParse(fsetʗ1, src)]), Ꮡ(new typesꓸInfo(Selections: selectionsʗ1)));
        if (err != nil) {
            t.Fatal(err);
        }
        importsʗ1[path] = pkg;
    }

    @string libSrc = libSrcᶜ;
    @string mainSrc = mainSrcᶜ;

    var wantOut = new map<@string, array<@string>>{
        ["lib.T.M"] = new @string[]{"method expr (lib.T) M(lib.T)", ".[0]"}.array(),

        ["A{}.B"] = new @string[]{"field (main.A) B *main.B", ".[0]"}.array(),
        ["new(A).B"] = new @string[]{"field (*main.A) B *main.B", "->[0]"}.array(),
        ["A{}.C"] = new @string[]{"field (main.A) C main.C", ".[1]"}.array(),
        ["new(A).C"] = new @string[]{"field (*main.A) C main.C", "->[1]"}.array(),
        ["A{}.b"] = new @string[]{"field (main.A) b int", "->[0 0]"}.array(),
        ["new(A).b"] = new @string[]{"field (*main.A) b int", "->[0 0]"}.array(),
        ["A{}.c"] = new @string[]{"field (main.A) c int", ".[1 0]"}.array(),
        ["new(A).c"] = new @string[]{"field (*main.A) c int", "->[1 0]"}.array(),
        ["Inst.p"] = new @string[]{"field (main.G[int]) p int", ".[0]"}.array(),

        ["A{}.f"] = new @string[]{"method (main.A) f(int)", "->[0 0]"}.array(),
        ["new(A).f"] = new @string[]{"method (*main.A) f(int)", "->[0 0]"}.array(),
        ["A{}.g"] = new @string[]{"method (main.A) g()", ".[1 0]"}.array(),
        ["new(A).g"] = new @string[]{"method (*main.A) g()", "->[1 0]"}.array(),
        ["new(A).h"] = new @string[]{"method (*main.A) h()", "->[1 1]"}.array(), // TODO(gri) should this report .[1 1] ?
        ["B{}.f"] = new @string[]{"method (main.B) f(int)", ".[0]"}.array(),
        ["new(B).f"] = new @string[]{"method (*main.B) f(int)", "->[0]"}.array(),
        ["C{}.g"] = new @string[]{"method (main.C) g()", ".[0]"}.array(),
        ["new(C).g"] = new @string[]{"method (*main.C) g()", "->[0]"}.array(),
        ["new(C).h"] = new @string[]{"method (*main.C) h()", "->[1]"}.array(), // TODO(gri) should this report .[1] ?
        ["Inst.m"] = new @string[]{"method (main.G[int]) m(int)", ".[0]"}.array(),

        ["A.f"] = new @string[]{"method expr (main.A) f(main.A, int)", "->[0 0]"}.array(),
        ["(*A).f"] = new @string[]{"method expr (*main.A) f(*main.A, int)", "->[0 0]"}.array(),
        ["B.f"] = new @string[]{"method expr (main.B) f(main.B, int)", ".[0]"}.array(),
        ["(*B).f"] = new @string[]{"method expr (*main.B) f(*main.B, int)", "->[0]"}.array(),
        ["G[string].m"] = new @string[]{"method expr (main.G[string]) m(main.G[string], string)", ".[0]"}.array(),
        ["G[string]{}.p"] = new @string[]{"field (main.G[string]) p string", ".[0]"}.array()
    };

    makePkg(libˢ, libSrc);
    makePkg(mainˢ, mainSrc);

    foreach (var (e, sel) in selections) {
        _ = sel.String(); // assertion: must not panic

        nint start = fset.Position(e.Pos()).Offset;
        nint end = fset.Position(e.End()).Offset;
        @string syntax = mainSrc.slice(start, end); // (all SelectorExprs are in main, not lib)

        @string direct = ".";
        if (sel.Indirect()) {
            direct = "->";
        }
        var got = new @string[]{
            sel.String(),
            fmt.Sprintf("%s%v"u8, direct, sel.Index())
        }.array();
        var want = wantOut[syntax, () => new array<@string>(2)].Clone();
        if (want != got) {
            t.Errorf("%s: got %q; want %q", syntax, got, want);
        }
        delete(wantOut, syntax);

        // We must explicitly assert properties of the
        // Signature's receiver since it doesn't participate
        // in Identical() or String().
        var (sig, _) = sel.Type()._<ж<typesꓸSignature>>(ᐧ);
        if (sel.Kind() == MethodVal) {
            var gotΔ1 = sig.Recv().Type();
            var wantΔ1 = sel.Recv();
            if (!Identical(gotΔ1, wantΔ1)) {
                t.Errorf("%s: Recv() = %s, want %s", syntax, gotΔ1, wantΔ1);
            }
        } else if (sig != nil && sig.Recv() != nil) {
            t.Errorf("%s: signature has receiver %s", sig.OrTypedNil(), sig.Recv().Type());
        }
    }
    // Assert that all wantOut entries were used exactly once.
    foreach (var (syntax, _) in wantOut) {
        t.Errorf("no ast.Selection found with syntax %q", syntax);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string libSrcᶜ1 => ref libSrcᶜ1ᵇ;
private static readonly @string libSrcᶜ1ᵇ = """

package a
import "missing"
const C1 = foo
const C2 = missing.C

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string mainSrcᶜ1 => ref mainSrcᶜ1ᵇ;
private static readonly @string mainSrcᶜ1ᵇ = """

package main
import "a"
var _ = a.C1
var _ = a.C2

"""u8.Raw();

public static void TestIssue8518(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(new Config(
        Error: (error err) => {
            t.Log(err);
        }, // don't exit after first error
        Importer: imports
    ), out var Ꮡconf);
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    void makePkg(@string path, @string src) {
        (importsʗ1[path], _) = Ꮡconf.Check(path, fsetʗ1, slice<ж<ast.File>>([mustParse(fsetʗ1, src)]), nil); // errors logged via conf.Error
    }

    @string libSrc = libSrcᶜ1;

    @string mainSrc = mainSrcᶜ1;

    makePkg("a", libSrc);
    makePkg(mainˢ, mainSrc); // don't crash when type-checking this package
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string libSrcᶜ2 => ref libSrcᶜ2ᵇ;
private static readonly @string libSrcᶜ2ᵇ = """

package a
const C = foo

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string mainSrcᶜ2 => ref mainSrcᶜ2ᵇ;
private static readonly @string mainSrcᶜ2ᵇ = """

package main
import "a"
const _ = a.C

"""u8.Raw();

public static void TestIssue59603(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(new Config(
        Error: (error err) => {
            t.Log(err);
        }, // don't exit after first error
        Importer: imports
    ), out var Ꮡconf);
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    void makePkg(@string path, @string src) {
        (importsʗ1[path], _) = Ꮡconf.Check(path, fsetʗ1, slice<ж<ast.File>>([mustParse(fsetʗ1, src)]), nil); // errors logged via conf.Error
    }

    @string libSrc = libSrcᶜ2;

    @string mainSrc = mainSrcᶜ2;

    makePkg("a", libSrc);
    makePkg(mainˢ, mainSrc); // don't crash when type-checking this package
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "LookupFieldOrMethod on nil type"u8;

public static void TestLookupFieldOrMethodOnNil(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // LookupFieldOrMethod on a nil type is expected to produce a run-time panic.
        defer(() => {
            @string want = wantᶜ;
            var p = recover();
            var (s, ok) = p._<@string>(ᐧ); if (!ok || s != want) {
                t.Fatalf("got %v, want %s", p, want);
            }
        }, ref ᒐ);
        LookupFieldOrMethod(default, false, nil, ""u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestLookupFieldOrMethod_type /*dyn*/ {
    internal @string src;
    internal bool found;
    internal slice<nint> index;
    internal bool indirect;
}

public static void TestLookupFieldOrMethod(ж<testing.T> t) {
    // Test cases assume a lookup of the form a.f or x.f, where a stands for an
    // addressable value, and x for a non-addressable value (even though a variable
    // for ease of test case writing).
    //
    // Should be kept in sync with TestMethodSet.
    slice<TestLookupFieldOrMethod_type> tests = slice<TestLookupFieldOrMethod_type>([
        // field lookups
        new("var x T; type T struct{}", false, default, false),
        new("var x T; type T struct{ f int }", true, [0], false),
        new("var x T; type T struct{ a, b, f, c int }", true, [2], false),

        // field lookups on a generic type
        new("var x T[int]; type T[P any] struct{}", false, default, false),
        new("var x T[int]; type T[P any] struct{ f P }", true, [0], false),
        new("var x T[int]; type T[P any] struct{ a, b, f, c P }", true, [2], false),

        // method lookups
        new("var a T; type T struct{}; func (T) f() {}", true, [0], false),
        new("var a *T; type T struct{}; func (T) f() {}", true, [0], true),
        new("var a T; type T struct{}; func (*T) f() {}", true, [0], false),
        new("var a *T; type T struct{}; func (*T) f() {}", true, [0], true), // TODO(gri) should this report indirect = false?

        // method lookups on a generic type
        new("var a T[int]; type T[P any] struct{}; func (T[P]) f() {}", true, [0], false),
        new("var a *T[int]; type T[P any] struct{}; func (T[P]) f() {}", true, [0], true),
        new("var a T[int]; type T[P any] struct{}; func (*T[P]) f() {}", true, [0], false),
        new("var a *T[int]; type T[P any] struct{}; func (*T[P]) f() {}", true, [0], true), // TODO(gri) should this report indirect = false?

        // collisions
        new("type ( E1 struct{ f int }; E2 struct{ f int }; x struct{ E1; *E2 })", false, [1, 0], false),
        new("type ( E1 struct{ f int }; E2 struct{}; x struct{ E1; *E2 }); func (E2) f() {}", false, [1, 0], false),

        // collisions on a generic type
        new("type ( E1[P any] struct{ f P }; E2[P any] struct{ f P }; x struct{ E1[int]; *E2[int] })", false, [1, 0], false),
        new("type ( E1[P any] struct{ f P }; E2[P any] struct{}; x struct{ E1[int]; *E2[int] }); func (E2[P]) f() {}", false, [1, 0], false),

        // outside methodset
        // (*T).f method exists, but value of type T is not addressable
        new("var x T; type T struct{}; func (*T) f() {}", false, default, true),

        // outside method set of a generic type
        new("var x T[int]; type T[P any] struct{}; func (*T[P]) f() {}", false, default, true),

        // recursive generic types; see go.dev/issue/52715
        new("var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (N[P]) f() {}", true, [0, 0], true),
        new("var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (T[P]) f() {}", true, [0], false)
    ]);

    foreach (var (_, test) in range(tests)) {
        var pkg = mustTypecheck("package p;"u8 + test.src, nil, nil);

        var obj = pkg.Scope().Lookup("a");
        if (obj == nil) {
            obj = pkg.Scope().Lookup("x"); if (obj == nil) {
                t.Errorf("%s: incorrect test case - no object a or x", test.src);
                continue;
            }
        }

        var (f, index, indirect) = LookupFieldOrMethod(obj.Type(), obj.Name() == "a"u8, pkg, "f");
        if ((f != nil) != test.found) {
            if (f == nil) {
                t.Errorf("%s: got no object; want one", test.src);
            } else {
                t.Errorf("%s: got object = %v; want none", test.src, f);
            }
        }
        if (!slices.Equal<slice<nint>, nint>(index, test.index)) {
            t.Errorf("%s: got index = %v; want %v", test.src, index, test.index);
        }
        if (indirect != test.indirect) {
            t.Errorf("%s: got indirect = %v; want %v", test.src, indirect, test.indirect);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pkgˢ => ref pkgˢᵇ;
private static readonly @string pkgˢᵇ = "pkg"u8;
internal static ref readonly @string instanceˢ => ref instanceˢᵇ;
private static readonly @string instanceˢᵇ = "Instance"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = """

package pkg

type Tree[T any] struct {
	*Node[T]
}

func (*Tree[R]) N(r R) R { return r }

type Node[T any] struct {
	*Tree[T]
}

type Instance = *Tree[int]

"""u8.Raw();

// Test for go.dev/issue/52715
public static void TestLookupFieldOrMethod_RecursiveGeneric(ж<testing.T> t) {
    @string src = srcᶜ1;

    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var pkg = NewPackage(pkgˢ, f.Name.Name);
    var err = NewChecker(nil, fset, pkg, nil).Files([f]); if (err != nil) {
        throw panic(err);
    }

    var T = pkg.Scope().Lookup(instanceˢ).Type();
    (_, _, _) = LookupFieldOrMethod(T, false, pkg, "M"); // verify that LookupFieldOrMethod terminates
}

// newDefined creates a new defined type named T with the given underlying type.
// Helper function for use with TestIncompleteInterfaces only.
internal static ж<types.Named> newDefined(typesꓸType underlying) {
    var tname = NewTypeName(nopos, nil, "T", default);
    return NewNamed(tname, underlying, default);
}

internal partial struct TestConvertibleTo_type /*dyn*/ {
    internal typesꓸType v, t;
    internal bool want;
}

public static void TestConvertibleTo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in range(slice<TestConvertibleTo_type>([
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
        new(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))), new types.PointerжΔType(NewPointer(new types.ArrayжΔType(NewArray(new types.BasicжΔType(Typ[Uint]), 10)))), false),
        // Untyped string values are not permitted by the spec, so the behavior below is undefined.
        new(new types.BasicжΔType(Typ[UntypedString]), new types.BasicжΔType(Typ[ΔString]), true)
    ]))) {
        var got = ConvertibleTo(test.v, test.t); if (got != test.want) {
            Ꮡt.Errorf("ConvertibleTo(%v, %v) = %t, want %t", test.v, test.t, got, test.want);
        }
    }
}

internal partial struct TestAssignableTo_type /*dyn*/ {
    internal typesꓸType v, t;
    internal bool want;
}

public static void TestAssignableTo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in range(slice<TestAssignableTo_type>([
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int]), true),
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Float32]), false),
        new(new types.NamedжΔType(newDefined(new types.BasicжΔType(Typ[Int]))), new types.BasicжΔType(Typ[Int]), false),
        new(new types.NamedжΔType(newDefined(new types.StructжΔType(@new<types.Struct>()))), new types.StructжΔType(@new<types.Struct>()), true),
        new(new types.BasicжΔType(Typ[UntypedBool]), new types.BasicжΔType(Typ[Bool]), true),
        new(new types.BasicжΔType(Typ[UntypedString]), new types.BasicжΔType(Typ[Bool]), false),
        // Neither untyped string nor untyped numeric assignments arise during
        // normal type checking, so the below behavior is technically undefined by
        // the spec.
        new(new types.BasicжΔType(Typ[UntypedString]), new types.BasicжΔType(Typ[ΔString]), true),
        new(new types.BasicжΔType(Typ[ΔUntypedInt]), new types.BasicжΔType(Typ[Int]), true)
    ]))) {
        var got = AssignableTo(test.v, test.t); if (got != test.want) {
            Ꮡt.Errorf("AssignableTo(%v, %v) = %t, want %t", test.v, test.t, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testMustDeclareBothXAndYˢ = (@string)"test must declare both X and Y"u8;

internal partial struct TestIdentical_tests /*dyn*/ {
    internal @string src;
    internal bool want;
}

public static void TestIdentical(ж<testing.T> t) {
    // For each test, we compare the types of objects X and Y in the source.
    var tests = slice<TestIdentical_tests>([
        // Basic types.
        new("var X int; var Y int", true),
        new("var X int; var Y string", false),

        // TODO: add more tests for complex types.

        // Named types.
        new("type X int; type Y int", false),

        // Aliases.
        new("type X = int; type Y = int", true),

        // Functions.
        new(@"func X(int) string { return """" }; func Y(int) string { return """" }", true),
        new(@"func X() string { return """" }; func Y(int) string { return """" }", false),
        new(@"func X(int) string { return """" }; func Y(int) {}", false),

        // Generic functions. Type parameters should be considered identical modulo
        // renaming. See also go.dev/issue/49722.
        new(@"func X[P ~int](){}; func Y[Q ~int]() {}", true),
        new(@"func X[P1 any, P2 ~*P1](){}; func Y[Q1 any, Q2 ~*Q1]() {}", true),
        new(@"func X[P1 any, P2 ~[]P1](){}; func Y[Q1 any, Q2 ~*Q1]() {}", false),
        new(@"func X[P ~int](P){}; func Y[Q ~int](Q) {}", true),
        new(@"func X[P ~string](P){}; func Y[Q ~int](Q) {}", false),
        new(@"func X[P ~int]([]P){}; func Y[Q ~int]([]Q) {}", true)
    ]);

    foreach (var (_, test) in range(tests)) {
        var pkg = mustTypecheck("package p;"u8 + test.src, nil, nil);
        var X = pkg.Scope().Lookup("X");
        var Y = pkg.Scope().Lookup("Y");
        if (X == nil || Y == nil) {
            t.Fatal(testMustDeclareBothXAndYˢ);
        }
        var got = Identical(X.Type(), Y.Type()); if (got != test.want) {
            t.Errorf("Identical(%s, %s) = %t, want %t", X.Type(), Y.Type(), got, test.want);
        }
    }
}

internal partial struct TestIdentical_issue15173_type /*dyn*/ {
    internal typesꓸType x, y;
    internal bool want;
}

public static void TestIdentical_issue15173(ж<testing.T> t) {
    // Identical should allow nil arguments and be symmetric.
    foreach (var (_, test) in range(slice<TestIdentical_issue15173_type>([
        new(new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int]), true),
        new(new types.BasicжΔType(Typ[Int]), default, false),
        new(default, new types.BasicжΔType(Typ[Int]), false),
        new(default, default, true)
    ]))) {
        var got = Identical(test.x, test.y); if (got != test.want) {
            t.Errorf("Identical(%v, %v) = %t", test.x, test.y, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myIntˢ => ref myIntˢᵇ;
private static readonly @string myIntˢᵇ = "myInt"u8;

internal partial struct TestIdenticalUnions_type /*dyn*/ {
    internal @string x, y;
    internal bool want;
}

public static void TestIdenticalUnions(ж<testing.T> t) {
    var tname = NewTypeName(nopos, nil, myIntˢ, default);
    var myInt = NewNamed(tname, new types.BasicжΔType(Typ[Int]), default);
    var tmap = new map<@string, ж<typesꓸTerm>>{
        ["int"] = NewTerm(false, new types.BasicжΔType(Typ[Int])),
        ["~int"] = NewTerm(true, new types.BasicжΔType(Typ[Int])),
        ["string"] = NewTerm(false, new types.BasicжΔType(Typ[ΔString])),
        ["~string"] = NewTerm(true, new types.BasicжΔType(Typ[ΔString])),
        ["myInt"] = NewTerm(false, new types.NamedжΔType(myInt))
    };
    var tmapʗ1 = tmap;
    ж<types.Union> makeUnion(@string s) {
        var parts = strings.Split(s, "|");
        slice<ж<typesꓸTerm>> terms = default;
        foreach (var (_, p) in range(parts)) {
            var term = tmapʗ1[p];
            if (term == nil) {
                t.Fatalf("missing term %q", p);
            }
            terms = append(terms, term);
        }
        return NewUnion(terms);
    }
    foreach (var (_, test) in range(slice<TestIdenticalUnions_type>([
        // These tests are just sanity checks. The tests for type sets and
        // interfaces provide much more test coverage.
        new("int|~int", "~int", true),
        new("myInt|~int", "~int", true),
        new("int|string", "string|int", true),
        new("int|int|string", "string|int", true),
        new("myInt|string", "int|string", false)
    ]))) {
        var x = makeUnion(test.x);
        var y = makeUnion(test.y);
        var got = Identical(new types.UnionжΔType(x), new types.UnionжΔType(y)); if (got != test.want) {
            t.Errorf("Identical(%v, %v) = %t", test.x, test.y, got);
        }
    }
}

public static void TestIssue61737(ж<testing.T> t) {
    // This test verifies that it is possible to construct invalid interfaces
    // containing duplicate methods using the go/types API.
    //
    // It must be possible for importers to construct such invalid interfaces.
    // Previously, this panicked.

    var sig1 = NewSignatureType(nil, default, default, NewTuple(NewParam(nopos, nil, ""u8, new types.BasicжΔType(Typ[Int]))), nil, false);
    var sig2 = NewSignatureType(nil, default, default, NewTuple(NewParam(nopos, nil, ""u8, new types.BasicжΔType(Typ[ΔString]))), nil, false);

    var methods = slice<ж<types.Func>>([
        NewFunc(nopos, nil, "M", sig1),
        NewFunc(nopos, nil, "M", sig2)
    ]);

    var embeddedMethods = slice<ж<types.Func>>([
        NewFunc(nopos, nil, "M", sig2)
    ]);
    var embedded = NewInterfaceType(embeddedMethods, default);
    var iface = NewInterfaceType(methods, [new types.InterfaceжΔType(embedded)]);
    iface.Complete();
}

public static void TestNewAlias_Issue65455(ж<testing.T> t) {
    var obj = NewTypeName(nopos, nil, "A", default);
    var alias = NewAlias(obj, new types.BasicжΔType(Typ[Int]));
    alias.Underlying(); // must not panic
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ2 => ref srcᶜ2ᵇ;
private static readonly @string srcᶜ2ᵇ = "package p; func f() int16; var _ = f(undef)"u8;

public static void TestIssue15305(ж<testing.T> t) {
    @string src = srcᶜ2;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(new Config(
        Error: (error err) => {
        } // allow errors
    ), out var Ꮡconf);
    var info = Ꮡ(new typesꓸInfo(
        Types: new map<ast.Expr, types.TypeAndValue>()
    ));
    Ꮡconf.Check("p", fset, [f], info); // ignore result
    foreach (var (e, tv) in info.Types) {
        var (_, ok) = e._<ж<ast.CallExpr>>(ᐧ); if (ok) {
            if (!AreEqual(tv.Type, Typ[Int16])) {
                t.Errorf("CallExpr has type %v, want int16", tv.Type);
            }
            return;
        }
    }
    t.Errorf("CallExpr has no type");
}

internal partial struct TestCompositeLitTypes_type /*dyn*/ {
    internal @string lit, typ;
}

// TestCompositeLitTypes verifies that Info.Types registers the correct
// types for composite literal expressions and composite literal type
// expressions.
public static void TestCompositeLitTypes(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(slice<TestCompositeLitTypes_type>([
        new(@"[16]byte{}", @"[16]byte"),
        new(@"[...]byte{}", @"[0]byte"),                // test for go.dev/issue/14092
        new(@"[...]int{1, 2, 3}", @"[3]int"),           // test for go.dev/issue/14092
        new(@"[...]int{90: 0, 98: 1, 2}", @"[100]int"), // test for go.dev/issue/14092
        new(@"[]int{}", @"[]int"),
        new(@"map[string]bool{""foo"": true}", @"map[string]bool"),
        new(@"struct{}{}", @"struct{}"),
        new(@"struct{x, y int; z complex128}{}", @"struct{x int; y int; z complex128}")
    ]))) {
        ref var test = ref heap(new TestCompositeLitTypes_type(), out var Ꮡtest);
        test = vᴛ1;

        var fset = token.NewFileSet();
        var f = mustParse(fset, fmt.Sprintf("package p%d; var _ = %s"u8, i, test.lit));
        var types = new map<ast.Expr, types.TypeAndValue>();
        var (_, err) = @new<types.Config>().Check("p", fset, [f], Ꮡ(new typesꓸInfo(Types: types))); if (err != nil) {
            t.Fatalf("%s: %v", test.lit, err);
        }

        var testʗ1 = test;
        var typesʗ1 = types;
        void cmptype(ast.Expr x, @string want) {
            var (tv, ok) = typesʗ1[x, ꟷ];
            if (!ok) {
                t.Errorf("%s: no Types entry found", testʗ1.lit);
                return;
            }
            if (tv.Type == nil) {
                t.Errorf("%s: type is nil", testʗ1.lit);
                return;
            }
            @string got = tv.Type.String(); if (got != want) {
                t.Errorf("%s: got %v, want %s", testʗ1.lit, got, want);
            }
        }

        // test type of composite literal expression
        var rhs = f.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>().Values[0];
        cmptype(rhs, test.typ);

        // test type of composite literal type expression
        cmptype(rhs._<ж<ast.CompositeLit>>().Type, test.typ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ3 => ref srcᶜ3ᵇ;
private static readonly @string srcᶜ3ᵇ = """

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

"""u8.Raw();

// TestObjectParents verifies that objects have parent scopes or not
// as specified by the Object interface.
public static void TestObjectParents(ж<testing.T> t) {
    @string src = srcᶜ3;

    var fset = token.NewFileSet();
    var f = mustParse(fset, src);

    var info = Ꮡ(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>()
    ));
    var (_, err) = @new<types.Config>().Check("p", fset, [f], info); if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (ident, obj) in info.Defs) {
        if (obj == nil) {
            // only package names and implicit vars have a nil object
            // (in this test we only need to handle the package name)
            if (ident.Name != "p"u8) {
                t.Errorf("%v has nil object", ident.OrTypedNil());
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
            if (objΔ1.Signature().Recv() != nil) { // method
                wantParent = false;
            }
            break;
        }}

        var gotParent = obj.Parent() != nil;
        switch (ᐧ) {
        case {} when gotParent && !wantParent: {
            t.Errorf("%v: want no parent, got %s", ident.OrTypedNil(), obj.Parent().OrTypedNil());
            break;
        }
        case {} when !gotParent && wantParent: {
            t.Errorf("%v: no parent found", ident.OrTypedNil());
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string couldNotImportˢ => ref couldNotImportˢᵇ;
private static readonly @string couldNotImportˢᵇ = "could not import"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ4 => ref srcᶜ4ᵇ;
private static readonly @string srcᶜ4ᵇ = """

package p

import foo "go/types/thisdirectorymustnotexistotherwisethistestmayfail/foo" // should only see an error here

const c = foo.C
type T = foo.T
var v T = c
func f(x T) T { return foo.F(x) }

"""u8.Raw();

// TestFailedImport tests that we don't get follow-on errors
// elsewhere in a package due to failing to import a package.
public static void TestFailedImport(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    @string src = srcᶜ4;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var files = slice<ж<ast.File>>([f]);

    // type-check using all possible importers
    foreach (var (_, compiler) in range(slice<@string>(["gc", "gccgo", "source"]))) {
        nint errcount = 0;
        ref var conf = ref heap<types.Config>(new Config(
            Error: (error err) => {
                // we should only see the import error
                if (errcount > 0 || !strings.Contains(err.Error(), couldNotImportˢ)) {
                    t.Errorf("for %s importer, got unexpected error: %v", compiler, err);
                }
                errcount++;
            },
            Importer: importer.For(compiler, default)
        ), out var Ꮡconf);

        var info = Ꮡ(new typesꓸInfo(
            Uses: new map<ж<ast.Ident>, types.Object>()
        ));
        var (pkg, _) = Ꮡconf.Check("p", fset, files, info);
        if (pkg == nil) {
            t.Errorf("for %s importer, type-checking failed to return a package", compiler);
            continue;
        }

        var imports = pkg.Imports();
        if (len(imports) != 1) {
            t.Errorf("for %s importer, got %d imports, want 1", compiler, len(imports));
            continue;
        }
        var imp = imports[0];
        if (imp.Name() != "foo"u8) {
            t.Errorf(@"for %s importer, got %q, want ""foo""", compiler, imp.Name());
            continue;
        }

        // verify that all uses of foo refer to the imported package foo (imp)
        foreach (var (ident, obj) in info.Uses) {
            if (ident.Name == "foo"u8) {
                var (objΔ1, ok) = obj._<ж<types.PkgName>>(ᐧ); if (ok) {
                    if (objΔ1.Imported() != imp) {
                        t.Errorf("%s resolved to %v; want %v", ident.OrTypedNil(), objΔ1.Imported().OrTypedNil(), imp.OrTypedNil());
                    }
                } else {
                    t.Errorf("%s resolved to %v; want package name", ident.OrTypedNil(), objΔ1.OrTypedNil());
                }
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ5 => ref srcᶜ5ᵇ;
private static readonly @string srcᶜ5ᵇ = "package p; type T[P any] *T[P]"u8;

public static void TestInstantiate(ж<testing.T> t) {
    // eventually we like more tests but this is a start
    @string src = srcᶜ5;
    var pkg = mustTypecheck(src, nil, nil);

    // type T should have one type parameter
    var T = pkg.Scope().Lookup("T").Type()._<ж<types.Named>>();
    nint n = T.TypeParams().Len(); if (n != 1) {
        t.Fatalf("expected 1 type parameter; found %d", n);
    }

    // instantiation should succeed (no endless recursion)
    // even with a nil *Checker
    var (res, err) = Instantiate(nil, new types.NamedжΔType(T), [new types.BasicжΔType(Typ[Int])], false);
    if (err != nil) {
        t.Fatal(err);
    }

    // instantiated type should point to itself
    var p = res.Underlying()._<ж<types.Pointer>>().Elem(); if (!AreEqual(p, res)) {
        t.Fatalf("unexpected result type: %s points to %s", res, p);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ6 => ref srcᶜ6ᵇ;
private static readonly @string srcᶜ6ᵇ = """
package p

type I[P any] interface {
	m(P)
	n() P
}

type J = I[int]

type Nested[P any] *interface{b(P)}

type K = Nested[string]

"""u8.Raw();

public static partial void TestInstantiateConcurrent(ж<testing.T> t) {
    @string src = srcᶜ6;
    var pkg = mustTypecheck(src, nil, nil);

    var insts = slice<ж<types.Interface>>([
        pkg.Scope().Lookup("J").Type().Underlying()._<ж<types.Interface>>(),
        pkg.Scope().Lookup("K").Type().Underlying()._<ж<types.Pointer>>().Elem()._<ж<types.Interface>>()
    ]);

    // Use the interface instances concurrently.
    foreach (var (_, inst) in range(insts)) {
        ref var counts = ref heap(new array<nint>(2), out var Ꮡcounts);             // method counts
        ref var methods = ref heap(new array<slice<@string>>(2), out var Ꮡmethods); // method strings
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint iᴛ1 = 0; iᴛ1 < 2; iᴛ1++) {
            var i = iᴛ1;
            Ꮡwg.Add(1);
            var instʗ1 = inst;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);

                    Ꮡcounts.Value[i] = instʗ1.NumMethods();
                    for (nint mi = 0; mi < Ꮡcounts.Value[i]; mi++) {
                        Ꮡmethods.Value[i] = append(Ꮡmethods.Value[i], instʗ1.Method(mi).String());
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();

        if (counts[0] != counts[1]) {
            t.Errorf("mismatching method counts for %s: %d vs %d", inst.OrTypedNil(), counts[0], counts[1]);
            continue;
        }
        for (nint i = 0; i < counts[0]; i++) {
            @string m0 = methods[0][i];
            @string m1 = methods[1][i]; if (m0 != m1) {
                t.Errorf("mismatching methods for %s: %s vs %s", inst.OrTypedNil(), m0, m1);
            }
        }
    }
}

internal partial struct TestInstantiateErrors_tests /*dyn*/ {
    internal @string src; // by convention, T must be the type being instantiated
    internal slice<typesꓸType> targs;
    internal nint wantAt; // -1 indicates no error
}

public static void TestInstantiateErrors(ж<testing.T> t) {
    var tests = slice<TestInstantiateErrors_tests>([
        new("type T[P interface{~string}] int", [new types.BasicжΔType(Typ[Int])], 0),
        new("type T[P1 interface{int}, P2 interface{~string}] int", [new types.BasicжΔType(Typ[Int]), new types.BasicжΔType(Typ[Int])], 1),
        new("type T[P1 any, P2 interface{~[]P1}] int", slice<typesꓸType>([new types.BasicжΔType(Typ[Int]), new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[ΔString])))]), 1),
        new("type T[P1 interface{~[]P2}, P2 any] int", slice<typesꓸType>([new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[ΔString]))), new types.BasicжΔType(Typ[Int])]), 0)
    ]);

    foreach (var (_, test) in range(tests)) {
        @string src = "package p; "u8 + test.src;
        var pkg = mustTypecheck(src, nil, nil);

        var T = pkg.Scope().Lookup("T").Type()._<ж<types.Named>>();

        var (_, err) = Instantiate(nil, new types.NamedжΔType(T), test.targs, true);
        if (err == nil) {
            t.Fatalf("Instantiate(%v, %v) returned nil error, want non-nil", T.OrTypedNil(), test.targs);
        }

        var (argErr, ok) = errors.AsType<ж<types.ArgumentError>>(err);
        if (!ok) {
            t.Fatalf("Instantiate(%v, %v): error is not an *ArgumentError", T.OrTypedNil(), test.targs);
        }

        if (argErr.Index != test.wantAt) {
            t.Errorf("Instantiate(%v, %v): error at index %d, want index %d", T.OrTypedNil(), test.targs, argErr.Index, test.wantAt);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;

public static void TestArgumentErrorUnwrapping(ж<testing.T> t) {
    error err = new types.ArgumentErrorжerror(Ꮡ(new ArgumentError(
        Index: 1,
        Err: new typesꓸError(Msg: "test")
    )));
    var (e, ok) = errors.AsType<typesꓸError>(err);
    if (!ok) {
        t.Fatalf("error %v does not wrap types.Error", err);
    }
    if (e.Msg != "test"u8) {
        t.Errorf("e.Msg = %q, want %q", e.Msg, testˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packageLibTypeTPAnyˢ => ref packageLibTypeTPAnyˢᵇ;
private static readonly @string packageLibTypeTPAnyˢᵇ = @"package lib; type T[P any] struct{}"u8;
internal static ref readonly @string packageAImportLibVarALibˢ => ref packageAImportLibVarALibˢᵇ;
private static readonly @string packageAImportLibVarALibˢᵇ = @"package a; import ""lib""; var A lib.T[int]"u8;
internal static ref readonly @string packageBImportLibVarBLibˢ => ref packageBImportLibVarBLibˢᵇ;
private static readonly @string packageBImportLibVarBLibˢᵇ = @"package b; import ""lib""; var B lib.T[int]"u8;

public static void TestInstanceIdentity(ж<testing.T> t) {
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(new Config(Importer: imports), out var Ꮡconf);
    var importsʗ1 = imports;
    void makePkg(@string src) {
        var fset = token.NewFileSet();
        var f = mustParse(fset, src);
        @string name = f.Name.Name;
        var (pkg, err) = Ꮡconf.Check(name, fset, [f], nil);
        if (err != nil) {
            t.Fatal(err);
        }
        importsʗ1[name] = pkg;
    }
    makePkg(packageLibTypeTPAnyˢ);
    makePkg(packageAImportLibVarALibˢ);
    makePkg(packageBImportLibVarBLibˢ);
    var a = imports["a"].Scope().Lookup("A");
    var b = imports["b"].Scope().Lookup("B");
    if (!Identical(a.Type(), b.Type())) {
        t.Errorf("mismatching types: a.A: %s, b.B: %s", a.Type(), b.Type());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ7 => ref srcᶜ7ᵇ;
private static readonly @string srcᶜ7ᵇ = """

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

"""u8.Raw();

internal partial struct TestInstantiatedObjects_tests /*dyn*/ {
    internal @string name;
    internal types.Object obj;
}

// TestInstantiatedObjects verifies properties of instantiated objects.
public static void TestInstantiatedObjects(ж<testing.T> t) {
    @string src = srcᶜ7;
    var info = Ꮡ(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>()
    ));
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(new Config(), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check(f.Name.Name, fset, [f], info);
    if (err != nil) {
        t.Fatal(err);
    }

    var pkgʗ1 = pkg;
    typesꓸType lookup(@string name) => pkgʗ1.Scope().Lookup(name).Type();
    var fnScope = pkg.Scope().Lookup("fn")._<ж<types.Func>>().Scope();

    var tests = slice<TestInstantiatedObjects_tests>([
        // Struct fields
        new("field", new types.VarжObject(lookup("t").Underlying()._<ж<types.Struct>>().Field(0))),
        new("field", new types.VarжObject(fnScope.Lookup("r").Type().Underlying()._<ж<types.Struct>>().Field(0))),

        // Methods and method fields
        new("concreteMethod", new types.FuncжObject(lookup("t")._<ж<types.Named>>().Method(0))),
        new("recv", new types.VarжObject(lookup("t")._<ж<types.Named>>().Method(0).Signature().Recv())),
        new("mParam", new types.VarжObject(lookup("t")._<ж<types.Named>>().Method(0).Signature().Params().At(0))),
        new("mResult", new types.VarжObject(lookup("t")._<ж<types.Named>>().Method(0).Signature().Results().At(0))),

        // Interface methods
        new("interfaceMethod", new types.FuncжObject(lookup("i").Underlying()._<ж<types.Interface>>().Method(0))),

        // Function type fields
        new("ftParam", new types.VarжObject(lookup("ft").Underlying()._<ж<typesꓸSignature>>().Params().At(0))),
        new("ftResult", new types.VarжObject(lookup("ft").Underlying()._<ж<typesꓸSignature>>().Results().At(0))),

        // Function fields
        new("fParam", new types.VarжObject(lookup("f")._<ж<typesꓸSignature>>().Params().At(0))),
        new("fResult", new types.VarжObject(lookup("f")._<ж<typesꓸSignature>>().Results().At(0)))
    ]);

    // Collect all identifiers by name.
    var idents = new map<@string, slice<ж<ast.Ident>>>();
    var identsʗ1 = idents;
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        var (id, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
            identsʗ1[id.Name] = append(identsʗ1[id.Name], id);
        }
        return true;
    });

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestInstantiatedObjects_tests(), out var Ꮡtest);
        test = vᴛ1;

        var identsʗ2 = idents;
        var infoʗ1 = info;
        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            nint got = len(identsʗ2[testʗ1.name]); if (got != 1) {
                tΔ1.Fatalf("found %d identifiers named %s, want 1", got, testʗ1.name);
            }
            var ident = identsʗ2[testʗ1.name][0];
            var def = infoʗ1.Defs[ident];
            if (AreEqual(def, testʗ1.obj)) {
                tΔ1.Fatalf("info.Defs[%s] contains the test object", testʗ1.name);
            }
            var orig = originObject(testʗ1.obj); if (!AreEqual(def, orig)) {
                tΔ1.Errorf("info.Defs[%s] does not match obj.Origin()", testʗ1.name);
            }
            if (def.Pkg() != testʗ1.obj.Pkg()) {
                tΔ1.Errorf("Pkg() = %v, want %v", def.Pkg().OrTypedNil(), testʗ1.obj.Pkg().OrTypedNil());
            }
            if (def.Name() != testʗ1.obj.Name()) {
                tΔ1.Errorf("Name() = %v, want %v", def.Name(), testʗ1.obj.Name());
            }
            if (def.Pos() != testʗ1.obj.Pos()) {
                tΔ1.Errorf("Pos() = %v, want %v", def.Pos(), testʗ1.obj.Pos());
            }
            if (def.Parent() != testʗ1.obj.Parent()) {
                tΔ1.Fatalf("Parent() = %v, want %v", def.Parent().OrTypedNil(), testʗ1.obj.Parent().OrTypedNil());
            }
            if (def.Exported() != testʗ1.obj.Exported()) {
                tΔ1.Fatalf("Exported() = %v, want %v", def.Exported(), testʗ1.obj.Exported());
            }
            if (def.Id() != testʗ1.obj.Id()) {
                tΔ1.Fatalf("Id() = %v, want %v", def.Id(), testʗ1.obj.Id());
            }
            // String and Type are expected to differ.
        });
    }
}

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
internal static ref readonly @string emptyIfaceˢ => ref emptyIfaceˢᵇ;
private static readonly @string emptyIfaceˢᵇ = "EmptyIface"u8;
internal static ref readonly @string integerˢ => ref integerˢᵇ;
private static readonly @string integerˢᵇ = "Integer"u8;
internal static ref readonly @string emptyTypeSetˢ => ref emptyTypeSetˢᵇ;
private static readonly @string emptyTypeSetˢᵇ = "EmptyTypeSet"u8;
internal static ref readonly @string badˢ => ref badˢᵇ;
private static readonly @string badˢᵇ = "Bad"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ8 => ref srcᶜ8ᵇ;
private static readonly @string srcᶜ8ᵇ = """

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

"""u8.Raw();

internal partial struct TestImplements_tests /*dyn*/ {
    public typesꓸType V;
    public ж<types.Interface> T;
    internal bool want;
}

public static void TestImplements(ж<testing.T> t) {
    @string src = srcᶜ8;

    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(new Config(Error: (error _Δp0) => {
    }), out var Ꮡconf);
    var (pkg, _) = Ꮡconf.Check(f.Name.Name, fset, [f], nil);

    var pkgʗ1 = pkg;
    typesꓸType lookup(@string tname) => pkgʗ1.Scope().Lookup(tname).Type();
    ж<types.Interface> EmptyIface = lookup(emptyIfaceˢ).Underlying()._<ж<types.Interface>>();
    ж<types.Named> I = lookup("I")._<ж<types.Named>>();
    ж<types.Interface> II = I.Underlying()._<ж<types.Interface>>();
    ж<types.Named> C = lookup("C")._<ж<types.Named>>();
    ж<types.Interface> CI = C.Underlying()._<ж<types.Interface>>();
    ж<types.Interface> Integer = lookup(integerˢ).Underlying()._<ж<types.Interface>>();
    ж<types.Interface> EmptyTypeSet = lookup(emptyTypeSetˢ).Underlying()._<ж<types.Interface>>();
    typesꓸType N1 = lookup("N1");
    ж<types.Pointer> N1p = NewPointer(N1);
    typesꓸType N2 = lookup("N2");
    ж<types.Pointer> N2p = NewPointer(N2);
    typesꓸType N3 = lookup("N3");
    typesꓸType N4 = lookup("N4");
    typesꓸType Bad = lookup(badˢ);

    var tests = slice<TestImplements_tests>([
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
    ]);

    foreach (var (_, test) in range(tests)) {
        {
            var got = Implements(test.V, test.T); if (got != test.want) {
                t.Errorf("Implements(%s, %s) = %t, want %t", test.V, test.T.OrTypedNil(), got, test.want);
            }
        }

        // The type assertion x.(T) is valid if T is an interface or if T implements the type of x.
        // The assertion is never valid if T is a bad type.
        var V = test.T;
        var T = test.V;
        var want = false;
        var (_, ok) = T.Underlying()._<ж<types.Interface>>(ᐧ); if ((ok || Implements(T, V)) && !AreEqual(T, Bad)) {
            want = true;
        }
        {
            var got = AssertableTo(V, T); if (got != want) {
                t.Errorf("AssertableTo(%s, %s) = %t, want %t", V.OrTypedNil(), T, got, want);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ9 => ref srcᶜ9ᵇ;
private static readonly @string srcᶜ9ᵇ = """

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

"""u8.Raw();

public static void TestMissingMethodAlternative(ж<testing.T> t) {
    @string src = srcᶜ9;

    var pkg = mustTypecheck(src, nil, nil);

    var T = pkg.Scope().Lookup("T").Type().Underlying()._<ж<types.Interface>>();
    var Tʗ1 = T;
    var pkgʗ1 = pkg;
    (ж<types.Func>, bool) lookup(@string name) => MissingMethod(pkgʗ1.Scope().Lookup(name).Type(), Tʗ1, true);

    // V0 has method m with correct signature. Should not report wrongType.
    var (method, wrongType) = lookup("V0");
    if (method != nil || wrongType) {
        t.Fatalf("V0: got method = %v, wrongType = %v", method.OrTypedNil(), wrongType);
    }

    var lookupʗ1 = lookup;
    void checkMissingMethod(@string tname, bool reportWrongType) {
        var (methodΔ1, wrongTypeΔ1) = lookupʗ1(tname);
        if (methodΔ1 == nil || methodΔ1.Name() != "m"u8 || wrongTypeΔ1 != reportWrongType) {
            t.Fatalf("%s: got method = %v, wrongType = %v", tname, methodΔ1.OrTypedNil(), wrongTypeΔ1);
        }
    }

    // V1 has no method m. Should not report wrongType.
    checkMissingMethod("V1", false);

    // V2 has method m with wrong signature type (ignoring receiver). Should report wrongType.
    checkMissingMethod("V2", true);

    // V3 has no method m but it exists on *V3. Should report wrongType.
    checkMissingMethod("V3", true);

    // V4 has no method m but has M. Should not report wrongType.
    checkMissingMethod("V4", false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorURLˢ => ref errorURLˢᵇ;
private static readonly @string errorURLˢᵇ = "_ErrorURL"u8;
internal static ref readonly @string goDevESˢ => ref goDevESˢᵇ;
private static readonly @string goDevESˢᵇ = " [go.dev/e/%s]"u8;
internal static ref readonly @string goDevEUndeclaredNameˢ => ref goDevEUndeclaredNameˢᵇ;
private static readonly @string goDevEUndeclaredNameˢᵇ = " [go.dev/e/UndeclaredName]"u8;
internal static ref readonly @string goDevEWrongArgCountˢ => ref goDevEWrongArgCountˢᵇ;
private static readonly @string goDevEWrongArgCountˢᵇ = " [go.dev/e/WrongArgCount]\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string src1ᶜ => ref src1ᶜᵇ;
private static readonly @string src1ᶜᵇ = """

package p
var _ T

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string src2ᶜ => ref src2ᶜᵇ;
private static readonly @string src2ᶜᵇ = """

package p
func f() int { return 0 }
var _ = f(1, 2)

"""u8.Raw();

public static void TestErrorURL(ж<testing.T> t) {
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    stringFieldAddr(Ꮡconf, errorURLˢ).Value = goDevESˢ;

    // test case for a one-line error
    @string src1 = src1ᶜ;
    var (_, err) = typecheck(src1, Ꮡconf, nil);
    if (err == nil || !strings.HasSuffix(err.Error(), goDevEUndeclaredNameˢ)) {
        t.Errorf("src1: unexpected error: got %v", err);
    }

    // test case for a multi-line error
    @string src2 = src2ᶜ;
    (_, err) = typecheck(src2, Ꮡconf, nil);
    if (err == nil || !strings.Contains(err.Error(), goDevEWrongArgCountˢ)) {
        t.Errorf("src1: unexpected error: got %v", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePˢ => ref packagePˢᵇ;
private static readonly @string packagePˢᵇ = "package p"u8;

public static void TestModuleVersion(ж<testing.T> t) {
    // version go1.dd must be able to typecheck go1.dd.0, go1.dd.1, etc.
    @string goversionΔ1 = fmt.Sprintf("go1.%d"u8, (nint)(goversion.Version));
    foreach (var (_, v) in range(slice<@string>([
        goversionΔ1,
        goversionΔ1 + ".0"u8,
        goversionΔ1 + ".1"u8,
        goversionΔ1 + ".rc"u8
    ]))) {
        ref var conf = ref heap<types.Config>(new Config(GoVersion: v), out var Ꮡconf);
        var pkg = mustTypecheck(packagePˢ, Ꮡconf, nil);
        if (pkg.GoVersion() != conf.GoVersion) {
            t.Errorf("got %s; want %s", pkg.GoVersion(), conf.GoVersion);
        }
    }
}

internal partial struct TestFileVersions_type /*dyn*/ {
    internal @string goVersion;
    internal @string fileVersion;
    internal @string wantVersion;
}

public static void TestFileVersions(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestFileVersions_type>([
        new("", "", ""),                    // no versions specified
        new("go1.19", "", "go1.19"),        // module version specified
        new("", "go1.20", "go1.21"),        // file version specified below minimum of 1.21
        new("go1", "", "go1"),              // no file version specified
        new("go1", "goo1.22", "go1"),       // invalid file version specified
        new("go1", "go1.19", "go1.21"),     // file version specified below minimum of 1.21
        new("go1", "go1.20", "go1.21"),     // file version specified below minimum of 1.21
        new("go1", "go1.21", "go1.21"),     // file version specified at 1.21
        new("go1", "go1.22", "go1.22"),     // file version specified above 1.21
        new("go1.19", "", "go1.19"),        // no file version specified
        new("go1.19", "goo1.22", "go1.19"), // invalid file version specified
        new("go1.19", "go1.20", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.19", "go1.21", "go1.21"),  // file version specified at 1.21
        new("go1.19", "go1.22", "go1.22"),  // file version specified above 1.21
        new("go1.20", "", "go1.20"),        // no file version specified
        new("go1.20", "goo1.22", "go1.20"), // invalid file version specified
        new("go1.20", "go1.19", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.20", "go1.20", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.20", "go1.21", "go1.21"),  // file version specified at 1.21
        new("go1.20", "go1.22", "go1.22"),  // file version specified above 1.21
        new("go1.21", "", "go1.21"),        // no file version specified
        new("go1.21", "goo1.22", "go1.21"), // invalid file version specified
        new("go1.21", "go1.19", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.21", "go1.20", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.21", "go1.21", "go1.21"),  // file version specified at 1.21
        new("go1.21", "go1.22", "go1.22"),  // file version specified above 1.21
        new("go1.22", "", "go1.22"),        // no file version specified
        new("go1.22", "goo1.22", "go1.22"), // invalid file version specified
        new("go1.22", "go1.19", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.22", "go1.20", "go1.21"),  // file version specified below minimum of 1.21
        new("go1.22", "go1.21", "go1.21"),  // file version specified at 1.21
        new("go1.22", "go1.22", "go1.22"),  // file version specified above 1.21

        // versions containing release numbers
        // (file versions containing release numbers are considered invalid)
        new("go1.19.0", "", "go1.19.0"),         // no file version specified
        new("go1.20.1", "go1.19.1", "go1.20.1"), // invalid file version
        new("go1.20.1", "go1.21.1", "go1.20.1"), // invalid file version
        new("go1.21.1", "go1.19.1", "go1.21.1"), // invalid file version
        new("go1.21.1", "go1.21.1", "go1.21.1"), // invalid file version
        new("go1.22.1", "go1.19.1", "go1.22.1"), // invalid file version
        new("go1.22.1", "go1.21.1", "go1.22.1")  // invalid file version
    ]))) {
        @string src = default;
        if (test.fileVersion != ""u8) {
            src = "//go:build "u8 + test.fileVersion + "\n"u8;
        }
        src += "package p"u8;

        ref var conf = ref heap<types.Config>(new Config(GoVersion: test.goVersion), out var Ꮡconf);
        var versions = new map<ж<ast.File>, @string>();
        ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
        info.FileVersions = versions;
        mustTypecheck(src, Ꮡconf, Ꮡinfo);

        nint n = 0;
        foreach (var (_, v) in versions) {
            @string want = test.wantVersion;
            if (v != want) {
                t.Errorf("%q: unexpected file version: got %q, want %q", src, v, want);
            }
            n++;
        }
        if (n != 1) {
            t.Errorf("%q: incorrect number of map entries: got %d", src, n);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string types2ˢ => ref types2ˢᵇ;
private static readonly @string types2ˢᵇ = "types2"u8;
internal static ref readonly @string typesˢ => ref typesˢᵇ;
private static readonly @string typesˢᵇ = "types"u8;
internal static ref readonly @string fTypesFuncˢ => ref fTypesFuncˢᵇ;
private static readonly @string fTypesFuncˢᵇ = "f:*types.Func"u8;

internal partial struct TestTooNew_type /*dyn*/ {
    internal @string goVersion;   // package's Go version (as if derived from go.mod file)
    internal @string fileVersion; // file's Go version (becomes a build tag)
    internal @string wantErr;     // expected substring of concatenation of all errors
}

// TestTooNew ensures that "too new" errors are emitted when the file
// or module is tagged with a newer version of Go than this go/types.
public static void TestTooNew(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestTooNew_type>([
        new("go1.98", "", "package requires newer Go version go1.98"),
        new("", "go1.99", "p:2:9: file requires newer Go version go1.99"),
        new("go1.98", "go1.99", "package requires newer Go version go1.98"), // (two
        new("go1.98", "go1.99", "file requires newer Go version go1.99")     // errors)
    ]))) {
        @string src = default;
        if (test.fileVersion != ""u8) {
            src = "//go:build "u8 + test.fileVersion + "\n"u8;
        }
        src += "package p; func f()"u8;

        ref var errs = ref heap<slice<error>>(out var Ꮡerrs);
        ref var conf = ref heap<types.Config>(new Config(
            GoVersion: test.goVersion,
            Error: (error err) => {
                Ꮡerrs.ValueSlot = append(Ꮡerrs.ValueSlot, err);
            }
        ), out var Ꮡconf);
        var info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
        typecheck(src, Ꮡconf, info);
        @string got = fmt.Sprint(errs);
        if (!strings.Contains(got, test.wantErr)) {
            t.Errorf("%q: unexpected error: got %q, want substring %q",
                src, got, test.wantErr);
        }

        // Assert that declarations were type checked nonetheless.
        slice<@string> gotObjs = default;
        foreach (var (id, obj) in info.Defs) {
            if (obj != nil) {
                @string objStr = strings.ReplaceAll(fmt.Sprintf("%s:%T"u8, id.Name, obj), types2ˢ, typesˢ);
                gotObjs = append(gotObjs, objStr);
            }
        }
        @string wantObjs = fTypesFuncˢ;
        if (!strings.Contains(fmt.Sprint(gotObjs), wantObjs)) {
            t.Errorf("%q: got %s, want substring %q",
                src, gotObjs, wantObjs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aTAAˢ => ref aTAAˢᵇ;
private static readonly @string aTAAˢᵇ = "a.T[a.A]"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ10 => ref srcᶜ10ᵇ;
private static readonly @string srcᶜ10ᵇ = """
package a

var x T[B] // this appears to cause Unalias to be called on B while still Invalid

type T[_ any] struct{}
type A T[B]
type B = T[A]

"""u8.Raw();

// This is a regression test for #66704.
public static void TestUnaliasTooSoonInCycle(ж<testing.T> t) {
    setGotypesalias(t, true);
    @string src = srcᶜ10;
    var pkg = mustTypecheck(src, nil, nil);
    var B = pkg.Scope().Lookup("B");

    @string got = Unalias(B.Type()).String();
    @string want = aTAAˢ;
    if (got != want) {
        t.Errorf("Unalias(type B = T[A]) = %q, want %q", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ11 => ref srcᶜ11ᵇ;
private static readonly @string srcᶜ11ᵇ = """
package p

type A = B
type B = C
type C = int

"""u8.Raw();

public static void TestAlias_Rhs(ж<testing.T> t) {
    setGotypesalias(t, true);
    @string src = srcᶜ11;

    var pkg = mustTypecheck(src, nil, nil);
    var A = pkg.Scope().Lookup("A");

    @string got = A.Type()._<ж<types.Alias>>().Rhs().String();
    @string want = "p.B";
    if (got != want) {
        t.Errorf("A.Rhs = %s, want %s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePVarXAnyˢ => ref packagePVarXAnyˢᵇ;
private static readonly @string packagePVarXAnyˢᵇ = "package p; var x any"u8;

// Test the hijacking described of "any" described in golang/go#66921, for type
// checking.
public static void TestAnyHijacking_Check(ж<testing.T> t) {
    foreach (var (_, enableAlias) in range(slice<bool>([false, true]))) {
        t.Run(fmt.Sprintf("EnableAlias=%t"u8, enableAlias), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            setGotypesalias(tΔ1, enableAlias);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < 10; i++) {
                Ꮡwg.Add(1);
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        var pkg = mustTypecheck(packagePVarXAnyˢ, nil, nil);
                        var x = pkg.Scope().Lookup("x");
                        var (_, gotAlias) = x.Type()._<ж<types.Alias>>(ᐧ); if (gotAlias != enableAlias) {
                            tΔ1.Errorf(@"Lookup(""x"").Type() is %T: got Alias: %t, want %t", x.Type(), gotAlias, enableAlias);
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
internal static ref readonly @string anyˢ => ref anyˢᵇ;
private static readonly @string anyˢᵇ = "any"u8;

// Test the hijacking described of "any" described in golang/go#66921, for
// Scope.Lookup outside of type checking.
public static void TestAnyHijacking_Lookup(ж<testing.T> t) {
    foreach (var (_, enableAlias) in range(slice<bool>([false, true]))) {
        t.Run(fmt.Sprintf("EnableAlias=%t"u8, enableAlias), (ж<testing.T> tΔ1) => {
            setGotypesalias(tΔ1, enableAlias);
            var a = Universe.Lookup(anyˢ);
            var (_, gotAlias) = a.Type()._<ж<types.Alias>>(ᐧ); if (gotAlias != enableAlias) {
                tΔ1.Errorf(@"Lookup(""x"").Type() is %T: got Alias: %t, want %t", a.Type(), gotAlias, enableAlias);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gotypesalias0ˢ => ref gotypesalias0ˢᵇ;
private static readonly @string gotypesalias0ˢᵇ = "gotypesalias=0"u8;

internal static void setGotypesalias(ж<testing.T> t, bool enable) {
    if (enable) {
        t.Setenv(godebugˢ, gotypesalias1ˢ);
    } else {
        t.Setenv(godebugˢ, gotypesalias0ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aGoˢ => ref aGoˢᵇ;
private static readonly @string aGoˢᵇ = "a.go"u8;
internal static readonly object packagePConstK123ˢ = (@string)"package p; const k = 123"u8;

// TestVersionIssue69477 is a regression test for issue #69477,
// in which the type checker would panic while attempting
// to compute which file it is "in" based on syntax position.
public static void TestVersionIssue69477(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, packagePConstK123ˢ, 0);

    // Set an invalid Pos on the BasicLit.
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        var (lit, ok) = n._<ж<ast.BasicLit>>(ᐧ); if (ok) {
            lit.ValuePos = 99999;
        }
        return true;
    });

    // Type check. The checker will consult the effective
    // version for the BasicLit 123. This used to panic.
    var pkg = NewPackage("p", "p");
    var check = NewChecker(Ꮡ(new Config()), fset, pkg, nil);
    var err = check.Files([f]); if (err != nil) {
        t.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goBuildGo122PackagePVarˢ = (@string)"//go:build go1.22\n\npackage p; var _ int"u8;
internal static readonly object packageQFuncSFuncFuncˢ = (@string)"package q; func _(s func(func() bool)) { for range s {} }"u8;
internal static ref readonly @string rangeOverSVariableOfTypeˢ => ref rangeOverSVariableOfTypeˢᵇ;
private static readonly @string rangeOverSVariableOfTypeˢᵇ = "range over s (variable of type func(func() bool)): requires go1.23"u8;

// TestVersionWithoutPos is a regression test for issue #69477,
// in which the type checker would use position information
// to compute which file it is "in" based on syntax position.
//
// As a rule the type checker should not depend on position
// information for correctness, only for error messages and
// Object.Pos. (Scope.LookupParent was a mistake.)
//
// The Checker now holds the effective version in a state variable.
public static void TestVersionWithoutPos(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, goBuildGo122PackagePVarˢ, 0);

    // Splice in a decl from another file. Its pos will be wrong.
    var (f2, _) = parser.ParseFile(fset, aGoˢ, packageQFuncSFuncFuncˢ, 0);
    f.Decls[0] = f2.Decls[0];

    // Type check. The checker will consult the effective
    // version (1.22) for the for-range stmt to know whether
    // range-over-func are permitted: they are not.
    // (Previously, no error was reported.)
    var pkg = NewPackage("p", "p");
    var check = NewChecker(Ꮡ(new Config()), fset, pkg, nil);
    var err = check.Files([f]);
    @string got = fmt.Sprint(err);
    @string want = rangeOverSVariableOfTypeˢ;
    if (!strings.Contains(got, want)) {
        t.Errorf("check error was %q, want substring %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePVarGlobalIntTypeˢ = (@string)"""
package p

var global int

type T struct { field int }

func (recv T) f(param int) (result int) {
	var local int
	local2 := 0
	switch local3 := any(local).(type) {
	default:
		_ = local3
	}
	return local2
}

"""u8.Raw();

public static void TestVarKind(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, packagePVarGlobalIntTypeˢ, 0);

    var pkg = NewPackage("p", "p");
    var info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
    var check = NewChecker(Ꮡ(new Config()), fset, pkg, info);
    var err = check.Files([f]); if (err != nil) {
        t.Fatal(err);
    }
    slice<@string> got = default;
    foreach (var (_, obj) in info.Defs) {
        var (v, ok) = obj._<ж<types.Var>>(ᐧ); if (ok) {
            got = append(got, fmt.Sprintf("%s: %v"u8, v.Name(), v.Kind()));
        }
    }
    sort.Strings(got);
    var want = slice<@string>([
        "field: FieldVar",
        "global: PackageVar",
        "local2: LocalVar",
        "local: LocalVar",
        "param: ParamVar",
        "recv: RecvVar",
        "result: ResultVar"
    ]);
    if (!slices.Equal<slice<@string>, @string>(got, want)) {
        t.Errorf("got:\n%s\nwant:\n%s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string funcByteByteByteˢ => ref funcByteByteByteˢᵇ;
private static readonly @string funcByteByteByteˢᵇ = "func([]byte, ...byte) []byte"u8;
internal static ref readonly @string funcBytePBByteˢ => ref funcBytePBByteˢᵇ;
private static readonly @string funcBytePBByteˢᵇ = "func([]byte, p.B...) []byte"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ12 => ref srcᶜ12ᵇ;
private static readonly @string srcᶜ12ᵇ = """
package p

func f[T ~[]byte](y T) []byte { return append([]byte(nil), y...) }

type B []byte
var _ = f[B]

"""u8.Raw();

public static void TestIssue73871(ж<testing.T> t) {
    @string src = srcᶜ12;
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, pGoˢ, src, 0);

    var pkg = NewPackage("p", "p");
    var info = Ꮡ(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()));
    var check = NewChecker(Ꮡ(new Config()), fset, pkg, info);
    var err = check.Files([f]); if (err != nil) {
        t.Fatal(err);
    }

    // Check type inferred for 'append'.
    //
    // Before CL 688815, the inferred type of append's y parameter
    // was T. When a client such as x/tools/go/ssa instantiated T=B,
    // it would result in the Signature "func([]byte, B)" with the
    // variadic flag set, an invalid combination that caused
    // NewSignatureType to panic.
    var append = f.Decls[0]._<ж<ast.FuncDecl>>().Body.List[0]._<ж<ast.ReturnStmt>>().Results[0]._<ж<ast.CallExpr>>().Fun;
    var tAppend = info.TypeOf(append)._<ж<typesꓸSignature>>();
    {
        @string got = fmt.Sprint(tAppend.OrTypedNil());
        @string want = funcByteByteByteˢ; if (got != want) {
            // Before CL 688815, tAppend was func([]byte, T) []byte,
            // where T prints as "<expected string type>".
            t.Errorf("append: got type %s, want %s", got, want);
        }
    }

    // Instantiate the type inferred for append(...) at T=B.
    // Before the fix in CL 689277, NewSignatureType would panic.
    var @params = slices.Collect(tAppend.Params().Variables());
    @params[1] = NewVar(token.NoPos, pkg, ""u8, pkg.Scope().Lookup("B").Type());
    var sig = NewSignatureType(nil, default, default, NewTuple(@params.ꓸꓸꓸ), tAppend.Results(), true);
    {
        @string got = fmt.Sprint(sig.OrTypedNil());
        @string want = funcBytePBByteˢ; if (got != want) {
            t.Errorf("instantiated: got type %s, want %s", got, want);
        }
    }
}

} // end types_test_package
