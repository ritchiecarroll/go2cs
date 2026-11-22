// MnGlyphMethodFrames reads the frames of a library method in the suffix form (`Take⁔`) after it parks, which under
// generated twins is its async twin `Take⁔ꞏtwin`, and of a //go:noinline method reached through its forwarder by a
// method value and a method expression, where the forwarder is not inlined. It also prints, names only, the
// runtime.FuncForPC name of that method expression and method value. The local type's Take is the control: package
// main has no ⁔ (its methods stay extension methods in main_package), so a difference that shows in both is not the
// suffix's.
//
// Not in this row: a function literal inside a method. Neither its name (Go's carries the method's receiver,
// `pkg.(*T).M.func1`) nor an inlined literal call's line (Go reads its closing `}()`) equals Go's yet, in package main
// as much as in a library; the converter will emit both in their own seat.
package main

import (
	"fmt"
	"reflect"
	"runtime"

	"MnGlyphMethodFrames/glyphlib"
)

type local struct {
	ch    chan int
	trail []string
}

func (l *local) Take() int {
	v := <-l.ch
	l.trail = append(l.trail, glyphlib.Frames("local Take"))

	return v
}

func main() {
	done := make(chan int)

	b := glyphlib.New()
	go func() { done <- b.Take() }()
	b.Put(7)
	fmt.Println(<-done)

	for _, line := range b.Trail() {
		fmt.Println(line)
	}

	l := &local{ch: make(chan int)}
	go func() { done <- l.Take() }()
	l.ch <- 8
	fmt.Println(<-done)

	for _, line := range l.trail {
		fmt.Println(line)
	}

	pass := b.Pass
	fmt.Println(pass(1))

	expr := glyphlib.Box.Pass
	fmt.Println(expr(*b, 2))

	fmt.Println(b.Pass(3))

	fmt.Println("method expression:", runtime.FuncForPC(reflect.ValueOf(glyphlib.Box.Pass).Pointer()).Name())
	fmt.Println("method value:", runtime.FuncForPC(reflect.ValueOf(b.Pass).Pointer()).Name())
}
