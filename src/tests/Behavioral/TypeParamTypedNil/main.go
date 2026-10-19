// TypeParamTypedNil guards the typed-nil boundary for a value of TYPE-PARAMETER type: a nil pointer (or func) is a
// value with a dynamic type in Go, inside a generic body as anywhere else. `show[*P](nil)` prints `*main.P <nil>`,
// and `any(e) == nil` is false, through every shape a type-parameter value reaches an empty interface by: a variadic
// argument, a return, an explicit conversion, an assignment, a composite element and a channel send. An interface
// type argument stays Go's nil interface, and a type parameter whose type set holds no nil passes through.
package main

import "fmt"

type P struct{ x int }

func show[E any](e E) string { return fmt.Sprintf("%T %v", e, e) }

func box[E any](e E) any { return e }

func isNil[E any](e E) bool { return any(e) == nil }

func assign[E any](e E) any {
	var x any
	x = e
	return x
}

func slot[E any](e E) []any { return []any{e} }

func send[E any](e E) any {
	c := make(chan any, 1)
	c <- e
	return <-c
}

type compError interface {
	comparable
	error
}

func nameOf[E compError](want E) string { return fmt.Sprintf("%T", want) }

type myErr struct{}

func (*myErr) Error() string { return "myErr" }

func plain[T ~int | ~string](t T) any { return t }

func main() {
	var p *P
	var f func()
	var e error

	fmt.Println(show(p))
	fmt.Println(box(p) == nil, isNil(p), assign(p) == nil, slot(p)[0] == nil, send(p) == nil)
	fmt.Printf("%T %T\n", box(p), assign(p))
	fmt.Println(fmt.Sprintf("%T", box(f)), box(f) == nil, isNil(f))
	fmt.Println(nameOf[*myErr](nil))
	fmt.Println(box(e) == nil, isNil(e))
	fmt.Println(show(&P{1}), plain(3), plain("s"))
}
