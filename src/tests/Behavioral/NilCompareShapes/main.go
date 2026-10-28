package main

import "fmt"

// Untyped nil compared with an interface or a func value. An interface that holds a typed
// nil pointer, or a nil map, is itself non-nil; a func variable is nil until assigned.

type myErr struct{ msg string }

func (e *myErr) Error() string { return e.msg }

func typedNilErr() error {
	var p *myErr
	return p
}

func nilErr() error {
	return nil
}

type handler func(int) int

type holder struct {
	err error
	fn  func() string
}

func tryRecover(doPanic bool) (recovered bool) {
	defer func() {
		recovered = recover() != nil
	}()

	if doPanic {
		panic("boom")
	}

	return false
}

func describe(v any) string {
	switch v {
	case nil:
		return "nil interface"
	default:
		return fmt.Sprintf("non-nil %T", v)
	}
}

func main() {
	err := typedNilErr()
	fmt.Println("typed nil pointer in error != nil:", err != nil)
	fmt.Println("nil == typed nil pointer in error:", nil == err)

	err = nilErr()
	fmt.Println("nil error == nil:", err == nil)

	var fn func()
	fmt.Println("nil func == nil:", fn == nil)
	fn = func() {}
	fmt.Println("assigned func != nil:", fn != nil)

	var h handler
	fmt.Println("nil named func == nil:", h == nil)
	h = func(x int) int { return x * 2 }
	fmt.Println("assigned named func != nil:", nil != h, h(21))

	fmt.Println("recover() != nil after panic:", tryRecover(true))
	fmt.Println("recover() != nil without panic:", tryRecover(false))

	var m map[string]int
	var a any = m
	fmt.Println("any holding nil map != nil:", a != nil)

	var sl []int
	var as any = sl
	fmt.Println("any holding nil slice != nil:", as != nil, nil == as)

	var empty any
	fmt.Println("empty any == nil:", empty == nil)

	var hd holder
	fmt.Println("zero struct fields are nil:", hd.err == nil, hd.fn == nil)
	hd.err = fmt.Errorf("wrapped")
	hd.fn = func() string { return "set" }
	fmt.Println("assigned fields are non-nil:", hd.err != nil, hd.fn != nil, hd.fn())

	fmt.Println(describe(nil))
	fmt.Println(describe(m))
	fmt.Println(describe(typedNilErr()))
}
