// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using parser = global::go.go.parser_package;
using testing = testing_package;
using static global::go.go.types_package;
using ast = global::go.go.ast_package;
using global::go.go;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

internal static slice<testEntry> testExprs = slice<testEntry>([
    // basic type literals
    dup("x"),
    dup("true"),
    dup("42"),
    dup("3.1415"),
    dup("2.71828i"),
    dup(@"'a'"),
    dup(@"""foo"""),
    dup("`bar`"),
    dup("any"),

    // func and composite literals
    new("func(){}", "(func() literal)"),
    new("func(x int) complex128 {}", "(func(x int) complex128 literal)"),
    new("[]int{1, 2, 3}", "[]int{…}"),

    // type expressions
    dup("[1 << 10]byte"),
    dup("[]int"),
    dup("*int"),
    dup("struct{x int}"),
    dup("func()"),
    dup("func(int, float32) string"),
    dup("interface{m()}"),
    dup("interface{m() string; n(x int)}"),
    dup("interface{~int}"),

    dup("map[string]int"),
    dup("chan E"),
    dup("<-chan E"),
    dup("chan<- E"),

    // new interfaces
    dup("interface{int}"),
    dup("interface{~int}"),

    // generic constraints
    dup("interface{~a | ~b | ~c; ~int | ~string; float64; m()}"),
    dup("interface{int | string}"),
    dup("interface{~int | ~string; float64; m()}"),
    dup("interface{~T[int, string] | string}"),

    // generic types
    dup("x[T]"),
    dup("x[N | A | S]"),
    dup("x[N, A]"),

    // non-type expressions
    dup("(x)"),
    dup("x.f"),
    dup("a[i]"),

    dup("s[:]"),
    dup("s[i:]"),
    dup("s[:j]"),
    dup("s[i:j]"),
    dup("s[:j:k]"),
    dup("s[i:j:k]"),

    dup("x.(T)"),

    dup("x.([10]int)"),
    dup("x.([...]int)"),

    dup("x.(struct{})"),
    dup("x.(struct{x int; y, z float32; E})"),

    dup("x.(func())"),
    dup("x.(func(x int))"),
    dup("x.(func() int)"),
    dup("x.(func(x, y int, z float32) (r int))"),
    dup("x.(func(a, b, c int))"),
    dup("x.(func(x ...T))"),

    dup("x.(interface{})"),
    dup("x.(interface{m(); n(x int); E})"),
    dup("x.(interface{m(); n(x int) T; E; F})"),

    dup("x.(map[K]V)"),

    dup("x.(chan E)"),
    dup("x.(<-chan E)"),
    dup("x.(chan<- chan int)"),
    dup("x.(chan<- <-chan int)"),
    dup("x.(<-chan chan int)"),
    dup("x.(chan (<-chan int))"),

    dup("f()"),
    dup("f(x)"),
    dup("int(x)"),
    dup("f(x, x + y)"),
    dup("f(s...)"),
    dup("f(a, s...)"),

    // generic functions
    dup("f[T]()"),
    dup("f[T](T)"),
    dup("f[T, T1]()"),
    dup("f[T, T1](T, T1)"),

    dup("*x"),
    dup("&x"),
    dup("x + y"),
    dup("x + y << (2 * s)")
]);

public static void TestExprString(ж<testing.T> t) {
    foreach (var (_, test) in range(testExprs)) {
        var (x, err) = parser.ParseExpr(test.src);
        if (err != nil) {
            t.Errorf("%s: %s", test.src, err);
            continue;
        }
        @string got = ExprString(x); if (got != test.str) {
            t.Errorf("%s: got %s, want %s", test.src, got, test.str);
        }
    }
}

} // end types_test_package
