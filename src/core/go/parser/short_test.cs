// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains test cases for short valid and invalid programs.
namespace go.go;

using testing = testing_package;
using static global::go.go.parser_package;

partial class parser_internal_test_package {

// go.dev/issue/9639
// generic code
internal static slice<@string> valids = new @string[]{
    "package p\n"u8,
    @"package p;"u8,
    @"package p; import ""fmt""; func f() { fmt.Println(""Hello, World!"") };"u8,
    @"package p; func f() { if f(T{}) {} };"u8,
    @"package p; func f() { _ = <-chan int(nil) };"u8,
    @"package p; func f() { _ = (<-chan int)(nil) };"u8,
    @"package p; func f() { _ = (<-chan <-chan int)(nil) };"u8,
    @"package p; func f() { _ = <-chan <-chan <-chan <-chan <-int(nil) };"u8,
    @"package p; func f(func() func() func());"u8,
    @"package p; func f(...T);"u8,
    @"package p; func f(float, ...int);"u8,
    @"package p; func f(x int, a ...int) { f(0, a...); f(1, a...,) };"u8,
    @"package p; func f(int,) {};"u8,
    @"package p; func f(...int,) {};"u8,
    @"package p; func f(x ...int,) {};"u8,
    @"package p; type T []int; var a []bool; func f() { if a[T{42}[0]] {} };"u8,
    @"package p; type T []int; func g(int) bool { return true }; func f() { if g(T{42}[0]) {} };"u8,
    @"package p; type T []int; func f() { for _ = range []int{T{42}[0]} {} };"u8,
    @"package p; var a = T{{1, 2}, {3, 4}}"u8,
    @"package p; func f() { select { case <- c: case c <- d: case c <- <- d: case <-c <- d: } };"u8,
    @"package p; func f() { select { case x := (<-c): } };"u8,
    @"package p; func f() { if ; true {} };"u8,
    @"package p; func f() { switch ; {} };"u8,
    @"package p; func f() { for _ = range ""foo"" + ""bar"" {} };"u8,
    @"package p; func f() { var s []int; g(s[:], s[i:], s[:j], s[i:j], s[i:j:k], s[:j:k]) };"u8,
    @"package p; var ( _ = (struct {*T}).m; _ = (interface {T}).m )"u8,
    @"package p; func ((T),) m() {}"u8,
    @"package p; func ((*T),) m() {}"u8,
    @"package p; func (*(T),) m() {}"u8,
    @"package p; func _(x []int) { for range x {} }"u8,
    @"package p; func _() { if [T{}.n]int{} {} }"u8,
    @"package p; func _() { map[int]int{}[0]++; map[int]int{}[0] += 1 }"u8,
    @"package p; func _(x interface{f()}) { interface{f()}(x).f() }"u8,
    @"package p; func _(x chan int) { chan int(x) <- 0 }"u8,
    @"package p; const (x = 0; y; z)"u8,
    @"package p; var _ = map[P]int{P{}:0, {}:1}"u8,
    @"package p; var _ = map[*P]int{&P{}:0, {}:1}"u8,
    @"package p; type T = int"u8,
    @"package p; type (T = p.T; _ = struct{}; x = *T)"u8,
    @"package p; type T (*int)"u8,
    @"package p; type _ struct{ int }"u8,
    @"package p; type _ struct{ pkg.T }"u8,
    @"package p; type _ struct{ *pkg.T }"u8,
    @"package p; var _ = func()T(nil)"u8,
    @"package p; func _(T (P))"u8,
    @"package p; func _(T []E)"u8,
    @"package p; func _(T [P]E)"u8,
    @"package p; type _ [A+B]struct{}"u8,
    @"package p; func (R) _()"u8,
    @"package p; type _ struct{ f [n]E }"u8,
    @"package p; type _ struct{ f [a+b+c+d]E }"u8,
    @"package p; type I1 interface{}; type I2 interface{ I1 }"u8,
    @"package p; type _ []T[int]"u8,
    @"package p; type T[P any] struct { P }"u8,
    @"package p; type T[P comparable] struct { P }"u8,
    @"package p; type T[P comparable[P]] struct { P }"u8,
    @"package p; type T[P1, P2 any] struct { P1; f []P2 }"u8,
    @"package p; func _[T any]()()"u8,
    @"package p; func _(T (P))"u8,
    @"package p; func f[A, B any](); func _() { _ = f[int, int] }"u8,
    @"package p; func _(x T[P1, P2, P3])"u8,
    @"package p; func _(x p.T[Q])"u8,
    @"package p; func _(p.T[Q])"u8,
    @"package p; type _[A interface{},] struct{}"u8,
    @"package p; type _[A interface{}] struct{}"u8,
    @"package p; type _[A,  B any,] struct{}"u8,
    @"package p; type _[A, B any] struct{}"u8,
    @"package p; type _[A any,] struct{}"u8,
    @"package p; type _[A any]struct{}"u8,
    @"package p; type _[A any] struct{ A }"u8,
    @"package p; func _[T any]()"u8,
    @"package p; func _[T any](x T)"u8,
    @"package p; func _[T1, T2 any](x T)"u8,
    @"package p; func _[A, B any](a A) B"u8,
    @"package p; func _[A, B C](a A) B"u8,
    @"package p; func _[A, B C[A, B]](a A) B"u8,
    @"package p; type _[A, B any] interface { _(a A) B }"u8,
    @"package p; type _[A, B C[A, B]] interface { _(a A) B }"u8,
    @"package p; func _[T1, T2 interface{}](x T1) T2"u8,
    @"package p; func _[T1 interface{ m() }, T2, T3 interface{}](x T1, y T3) T2"u8,
    @"package p; var _ = []T[int]{}"u8,
    @"package p; var _ = [10]T[int]{}"u8,
    @"package p; var _ = func()T[int]{}"u8,
    @"package p; var _ = map[T[int]]T[int]{}"u8,
    @"package p; var _ = chan T[int](x)"u8,
    @"package p; func _(_ T[P], T P) T[P]"u8,
    @"package p; var _ T[chan int]"u8,
    @"package p; func (_ R[P]) _(x T)"u8,
    @"package p; func (_ R[ P, Q]) _(x T)"u8,
    @"package p; func (R[P]) _()"u8,
    @"package p; func _(T[P])"u8,
    @"package p; func _(T[P1, P2, P3 ])"u8,
    @"package p; func _(T[P]) T[P]"u8,
    @"package p; type _ struct{ T[P]}"u8,
    @"package p; type _ struct{ T[struct{a, b, c int}] }"u8,
    @"package p; type _ interface{int|float32; bool; m(); string;}"u8,
    @"package p; type I1[T any] interface{}; type I2 interface{ I1[int] }"u8,
    @"package p; type I1[T any] interface{}; type I2[T any] interface{ I1[T] }"u8,
    @"package p; type _ interface { N[T] }"u8,
    @"package p; type T[P any] = T0"u8
}.slice();

public static void TestValid(ж<testing.T> Ꮡt) {
    foreach (var (_, src) in valids) {
        checkErrors(Ꮡt, src, src, (global::go.go.parser_package.Mode)(DeclarationErrors | AllErrors), false);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ4 = @"package p; var _ = T{}"u8;

// TestSingle is useful to track down a problem with a single short test program.
public static void TestSingle(ж<testing.T> Ꮡt) {
    @string src = srcᶜ4;
    checkErrors(Ꮡt, src, src, (global::go.go.parser_package.Mode)(DeclarationErrors | AllErrors), true);
}

// go.dev/issue/8656
// go.dev/issue/9639
// go.dev/issue/12437
// go.dev/issue/11611
// go.dev/issue/13475
// generic code
internal static slice<@string> invalids = new @string[]{
    @"foo /* ERROR ""expected 'package'"" */ !"u8,
    @"package p; func f() { if { /* ERROR ""missing condition"" */ } };"u8,
    @"package p; func f() { if ; /* ERROR ""missing condition"" */ {} };"u8,
    @"package p; func f() { if f(); /* ERROR ""missing condition"" */ {} };"u8,
    @"package p; func f() { if _ = range /* ERROR ""expected operand"" */ x; true {} };"u8,
    @"package p; func f() { switch _ /* ERROR ""expected switch expression"" */ = range x; true {} };"u8,
    @"package p; func f() { for _ = range x ; /* ERROR ""expected '{'"" */ ; {} };"u8,
    @"package p; func f() { for ; ; _ = range /* ERROR ""expected operand"" */ x {} };"u8,
    @"package p; func f() { for ; _ /* ERROR ""expected boolean or range expression"" */ = range x ; {} };"u8,
    @"package p; func f() { switch t = /* ERROR ""expected ':=', found '='"" */ t.(type) {} };"u8,
    @"package p; func f() { switch t /* ERROR ""expected switch expression"" */ , t = t.(type) {} };"u8,
    @"package p; func f() { switch t /* ERROR ""expected switch expression"" */ = t.(type), t {} };"u8,
    @"package p; func f() { _ = (<-<- /* ERROR ""expected 'chan'"" */ chan int)(nil) };"u8,
    @"package p; func f() { _ = (<-chan<-chan<-chan<-chan<-chan<- /* ERROR ""expected channel type"" */ int)(nil) };"u8,
    @"package p; func f() { if x := g(); x /* ERROR ""expected boolean expression"" */ = 0 {}};"u8,
    @"package p; func f() { _ = x = /* ERROR ""expected '=='"" */ 0 {}};"u8,
    @"package p; func f() { _ = 1 == func()int { var x bool; x = x = /* ERROR ""expected '=='"" */ true; return x }() };"u8,
    @"package p; func f() { var s []int; _ = s[] /* ERROR ""expected operand"" */ };"u8,
    @"package p; func f() { var s []int; _ = s[i:j: /* ERROR ""final index required"" */ ] };"u8,
    @"package p; func f() { var s []int; _ = s[i: /* ERROR ""middle index required"" */ :k] };"u8,
    @"package p; func f() { var s []int; _ = s[i: /* ERROR ""middle index required"" */ :] };"u8,
    @"package p; func f() { var s []int; _ = s[: /* ERROR ""middle index required"" */ :] };"u8,
    @"package p; func f() { var s []int; _ = s[: /* ERROR ""middle index required"" */ ::] };"u8,
    @"package p; func f() { var s []int; _ = s[i:j:k: /* ERROR ""expected ']'"" */ l] };"u8,
    @"package p; func f() { for x /* ERROR ""boolean or range expression"" */ = []string {} }"u8,
    @"package p; func f() { for x /* ERROR ""boolean or range expression"" */ := []string {} }"u8,
    @"package p; func f() { for i /* ERROR ""boolean or range expression"" */ , x = []string {} }"u8,
    @"package p; func f() { for i /* ERROR ""boolean or range expression"" */ , x := []string {} }"u8,
    @"package p; func f() { go f /* ERROR HERE ""must be function call"" */ }"u8,
    @"package p; func f() { go ( /* ERROR ""must not be parenthesized"" */ f()) }"u8,
    @"package p; func f() { defer func() {} /* ERROR HERE ""must be function call"" */ }"u8,
    @"package p; func f() { defer ( /* ERROR ""must not be parenthesized"" */ f()) }"u8,
    @"package p; func f() { go func() { func() { f(x func /* ERROR ""missing ','"" */ (){}) } } }"u8,
    @"package p; func _() (type /* ERROR ""found 'type'"" */ T)(T)"u8,
    @"package p; func (type /* ERROR ""found 'type'"" */ T)(T) _()"u8,
    @"package p; type _[A+B, /* ERROR ""unexpected comma"" */ ] int"u8,
    @"package p; type _ struct{ [ /* ERROR ""expected '}', found '\['"" */ ]byte }"u8,
    @"package p; type _ struct{ ( /* ERROR ""cannot parenthesize embedded type"" */ int) }"u8,
    @"package p; type _ struct{ ( /* ERROR ""cannot parenthesize embedded type"" */ []byte) }"u8,
    @"package p; type _ struct{ *( /* ERROR ""cannot parenthesize embedded type"" */ int) }"u8,
    @"package p; type _ struct{ *( /* ERROR ""cannot parenthesize embedded type"" */ []byte) }"u8,
    @"package p; func f() (a b string /* ERROR ""missing ','"" */ , ok bool)"u8,
    @"package p; var x, y, z; /* ERROR ""expected type"" */"u8,
    @"package p; var _ = struct { x int, /* ERROR ""expected ';', found ','"" */ }{};"u8,
    @"package p; var _ = struct { x int, /* ERROR ""expected ';', found ','"" */ y float }{};"u8,
    @"package p; type _ struct { int, } /* ERROR ""expected 'IDENT', found '}'"" */ ;"u8,
    @"package p; type _ struct { int, float } /* ERROR ""expected type, found '}'"" */ ;"u8,
    @"package p; func f() { if true {} else ; /* ERROR ""expected if statement or block"" */ }"u8,
    @"package p; func f() { if true {} else defer /* ERROR ""expected if statement or block"" */ f() }"u8,
    @"package p; type _[_ any] int; var _ = T[] /* ERROR ""expected operand"" */ {}"u8,
    @"package p; var _ func[ /* ERROR ""must have no type parameters"" */ T any](T)"u8,
    @"package p; func _[]/* ERROR ""empty type parameter list"" */()"u8,
    @"package p; type _[A,] /* ERROR ""missing type constraint"" */ struct{ A }"u8,
    @"package p; func _[type /* ERROR ""found 'type'"" */ P, *Q interface{}]()"u8,
    @"package p; func (T) _[ /* ERROR ""must have no type parameters"" */ A, B any](a A) B"u8,
    @"package p; func (T) _[ /* ERROR ""must have no type parameters"" */ A, B C](a A) B"u8,
    @"package p; func (T) _[ /* ERROR ""must have no type parameters"" */ A, B C[A, B]](a A) B"u8,
    @"package p; func(*T[e, e /* ERROR ""e redeclared"" */ ]) _()"u8
}.slice();

public static void TestInvalid(ж<testing.T> Ꮡt) {
    foreach (var (_, src) in invalids) {
        checkErrors(Ꮡt, src, src, (global::go.go.parser_package.Mode)(DeclarationErrors | AllErrors), true);
    }
}

} // end parser_internal_test_package
