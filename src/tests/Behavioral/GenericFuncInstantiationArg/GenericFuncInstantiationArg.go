// Guards an EXPLICITLY INSTANTIATED generic function passed directly as an ARGUMENT. The Go value
// `pick[string]` is a func value like any other, and passing it where an interface or a func type
// is expected used to leave the converter wrapping a bare C# method group (`(pick<@string>)`) in
// the typed-nil accessor, which is CS0119: a method group where a value is needed. The same
// function bound to a variable first (`pf := pick[string]`) always converted, and is the control.
package main

import (
	"fmt"
	"reflect"
)

func pick[T any](a []T) T { return a[0] }

func pair[K comparable, V any](k K, v V) map[K]V { return map[K]V{k: v} }

func show(f any) {
	fmt.Println("show:", reflect.TypeOf(f).String())
}

func apply(f func(any) string, v any) string {
	return f(v)
}

func describe[T any](v T) string {
	return fmt.Sprintf("%v/%T", v, v)
}

func returned() any {
	return pick[string]
}

func main() {
	// Control: bound to a variable first.
	pf := pick[string]
	fmt.Println("control:", reflect.TypeOf(pf).String(), pf([]string{"a", "b", "c"}))

	// To reflect.TypeOf, one type argument and two.
	fmt.Println(reflect.TypeOf(pick[string]).String())
	fmt.Println(reflect.TypeOf(pair[string, int]).String())

	// To an `any` parameter.
	show(pick[int])
	show(pair[int, bool])

	// To fmt.Printf's variadic ...any.
	fmt.Printf("%T\n", pick[float64])
	fmt.Printf("%T\n", pair[string, string])

	// Returned through an `any` result.
	fmt.Println(reflect.TypeOf(returned()).String())

	// To a func-typed parameter.
	fmt.Println(apply(describe[any], 42))
	fmt.Println(apply(describe[any], "go"))
}
