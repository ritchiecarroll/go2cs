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

internal static slice<testEntry> testExprs = new testEntry[]{
    // basic type literals
    dup("x"u8),
    dup("true"u8),
    dup("42"u8),
    dup("3.1415"u8),
    dup("2.71828i"u8),
    dup(@"'a'"u8),
    dup(@"""foo"""u8),
    dup("`bar`"u8),
    dup("any"u8),

    // func and composite literals
    new("func(){}"u8, "(func() literal)"u8),
    new("func(x int) complex128 {}"u8, "(func(x int) complex128 literal)"u8),
    new("[]int{1, 2, 3}"u8, "[]int{…}"u8),

    // type expressions
    dup("[1 << 10]byte"u8),
    dup("[]int"u8),
    dup("*int"u8),
    dup("struct{x int}"u8),
    dup("func()"u8),
    dup("func(int, float32) string"u8),
    dup("interface{m()}"u8),
    dup("interface{m() string; n(x int)}"u8),
    dup("interface{~int}"u8),
    dup("map[string]int"u8),
    dup("chan E"u8),
    dup("<-chan E"u8),
    dup("chan<- E"u8),

    // new interfaces
    dup("interface{int}"u8),
    dup("interface{~int}"u8),

    // generic constraints
    dup("interface{~a | ~b | ~c; ~int | ~string; float64; m()}"u8),
    dup("interface{int | string}"u8),
    dup("interface{~int | ~string; float64; m()}"u8),
    dup("interface{~T[int, string] | string}"u8),

    // generic types
    dup("x[T]"u8),
    dup("x[N | A | S]"u8),
    dup("x[N, A]"u8),

    // non-type expressions
    dup("(x)"u8),
    dup("x.f"u8),
    dup("a[i]"u8),
    dup("s[:]"u8),
    dup("s[i:]"u8),
    dup("s[:j]"u8),
    dup("s[i:j]"u8),
    dup("s[:j:k]"u8),
    dup("s[i:j:k]"u8),
    dup("x.(T)"u8),
    dup("x.([10]int)"u8),
    dup("x.([...]int)"u8),
    dup("x.(struct{})"u8),
    dup("x.(struct{x int; y, z float32; E})"u8),
    dup("x.(func())"u8),
    dup("x.(func(x int))"u8),
    dup("x.(func() int)"u8),
    dup("x.(func(x, y int, z float32) (r int))"u8),
    dup("x.(func(a, b, c int))"u8),
    dup("x.(func(x ...T))"u8),
    dup("x.(interface{})"u8),
    dup("x.(interface{m(); n(x int); E})"u8),
    dup("x.(interface{m(); n(x int) T; E; F})"u8),
    dup("x.(map[K]V)"u8),
    dup("x.(chan E)"u8),
    dup("x.(<-chan E)"u8),
    dup("x.(chan<- chan int)"u8),
    dup("x.(chan<- <-chan int)"u8),
    dup("x.(<-chan chan int)"u8),
    dup("x.(chan (<-chan int))"u8),
    dup("f()"u8),
    dup("f(x)"u8),
    dup("int(x)"u8),
    dup("f(x, x + y)"u8),
    dup("f(s...)"u8),
    dup("f(a, s...)"u8),

    // generic functions
    dup("f[T]()"u8),
    dup("f[T](T)"u8),
    dup("f[T, T1]()"u8),
    dup("f[T, T1](T, T1)"u8),
    dup("*x"u8),
    dup("&x"u8),
    dup("x + y"u8),
    dup("x + y << (2 * s)"u8)
}.slice();

public static void TestExprString(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in testExprs) {
        var (x, err) = parser.ParseExpr(test.src);
        if (err != default!) {
            Ꮡt.Errorf("%s: %s"u8, test.src, err);
            continue;
        }
        {
            @string got = ExprString(x); if (got != test.str) {
                Ꮡt.Errorf("%s: got %s, want %s"u8, test.src, got, test.str);
            }
        }
    }
}

} // end types_test_package
