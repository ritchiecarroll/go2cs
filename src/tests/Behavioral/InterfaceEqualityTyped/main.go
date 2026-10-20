// Go's == between two interface values of one dynamic type, for each kind of type go2cs emits: a named integer
// (like syscall.Errno), a named float with NaN, a struct with an interface-typed field (whose == recurses through the
// interface comparison), a generic struct instance, a pointer, a string, a boxed float and complex, and an
// uncomparable dynamic type, which panics.
package main

import (
	"errors"
	"fmt"
	"math"
)

type errno uintptr

func (e errno) Error() string { return fmt.Sprintf("errno %d", uintptr(e)) }

const (
	eagain errno = 11
	eintr  errno = 4
)

type celsius float64

type pathError struct {
	Op   string
	Path string
	Err  error
}

func (e pathError) Error() string { return e.Op + " " + e.Path + ": " + e.Err.Error() }

type pair[K comparable, V comparable] struct {
	Key K
	Val V
}

type node struct{ n int }

func eq(a, b any) bool { return a == b }

func panicText(a, b any) (text string) {
	defer func() { text = fmt.Sprint(recover()) }()
	_ = a == b
	return "no panic"
}

func main() {
	var err error = eagain
	fmt.Println("errno:", err == eagain, err == eintr, err == error(eagain))

	nan := celsius(math.NaN())
	fmt.Println("named float:", eq(celsius(1.5), celsius(1.5)), eq(nan, nan), eq(celsius(0), celsius(math.Copysign(0, -1))))

	boom := errors.New("boom")
	a := pathError{"read", "p", boom}
	b := pathError{"read", "p", boom}
	c := pathError{"read", "p", errors.New("boom")}
	d := pathError{"read", "p", eagain}
	e := pathError{"read", "p", eagain}
	fmt.Println("struct with interface field:", eq(a, b), eq(a, c), eq(d, e), eq(a, d))
	var pe error = a
	fmt.Println("struct through error:", pe == error(b), pe == error(c))

	fmt.Println("generic struct:", eq(pair[string, int]{"k", 1}, pair[string, int]{"k", 1}), eq(pair[string, int]{"k", 1}, pair[string, int]{"k", 2}),
		eq(pair[string, float64]{"k", math.NaN()}, pair[string, float64]{"k", math.NaN()}))

	p, q := &node{1}, &node{1}
	fmt.Println("pointer:", eq(p, p), eq(p, q))

	fmt.Println("string:", eq("go", "go"), eq("go", "c#"))
	fmt.Println("float and complex:", eq(math.NaN(), math.NaN()), eq(1.0, 1.0), eq(complex(math.NaN(), 0), complex(math.NaN(), 0)), eq(complex64(1i), complex64(1i)))

	fmt.Println("uncomparable:", panicText([]int{1}, []int{1}))
	fmt.Println("uncomparable field:", panicText(pathError{"x", "y", nil}, struct{ s []int }{}))
	fmt.Println("uncomparable in struct:", panicText(struct{ v any }{[]int{1}}, struct{ v any }{[]int{1}}))
}
