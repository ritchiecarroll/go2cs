// TypeAssertionPanic guards that a FAILED single-value type assertion panics with Go's own value, a
// *runtime.TypeAssertionError, rather than a bare string.
//
// Go's compiler lowers x.(T) to a type-word comparison plus a call to runtime.panicdottypeE (or
// panicnildottype), which panics with a *TypeAssertionError. That value satisfies error and
// runtime.Error, its Error() text names the static interface, the dynamic type and the asserted type,
// and fmt prints its dynamic type as *runtime.TypeAssertionError. golib raised the sentence as a plain
// string, so `r.(runtime.Error)` and `r.(error)` missed on a recovered assertion failure. The
// recovered value's type, its runtime.Error assertion and its text are all compared.
//
// The source is an empty interface or error. A named non-empty interface as the source (io.Reader)
// prints its own name in Go, and the converted assertion does not yet carry the static interface type.
package main

import (
	"errors"
	"fmt"
	"runtime"
)

type celsius float64

type myErr struct{}

func (*myErr) Error() string { return "myErr" }

type shape interface {
	area() float64
	perimeter() float64
}

type square struct{ side float64 }

func (s square) perimeter() float64 { return 4 * s.side }

type circle struct{ r float64 }

func (c *circle) area() float64      { return 3 * c.r * c.r }
func (c *circle) perimeter() float64 { return 6 * c.r }

type point struct{ x, y int }

// catch runs f and reports what recover() saw: the value's dynamic type, whether it satisfies
// runtime.Error and error, and its text.
func catch(what string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			_, isRuntimeError := r.(runtime.Error)
			_, isError := r.(error)
			fmt.Printf("%s: %T runtime.Error: %v error: %v: %v\n", what, r, isRuntimeError, isError, r)
		} else {
			fmt.Printf("%s: NO PANIC\n", what)
		}
	}()
	f()
}

func main() {
	var boxed any = 7

	// 1. A concrete type that is not the asserted one, from an empty interface.
	catch("int-to-string", func() { _ = boxed.(string) })

	// 2. The same to a named type and to a struct pointer.
	catch("int-to-celsius", func() { _ = boxed.(celsius) })
	catch("int-to-*point", func() { _ = boxed.(*point) })

	// 3. A struct value asserted to its pointer type.
	var p any = point{1, 2}
	catch("point-to-*point", func() { _ = p.(*point) })

	// 4. A nil interface.
	var nothing any
	catch("nil-to-string", func() { _ = nothing.(string) })

	// 5. An assertion to an interface names the dynamic type and the first method it lacks, in Go's
	// order (area sorts before perimeter), and leaves the source type out; a nil source prints the bare word.
	catch("int-to-runtime.Error", func() { _ = boxed.(runtime.Error) })
	catch("nil-to-runtime.Error", func() { _ = nothing.(runtime.Error) })
	var sq any = square{2}
	catch("square-to-shape", func() { _ = sq.(shape) })
	// A value circle does not carry the pointer-receiver methods; a *circle carries both.
	var circ any = circle{1}
	catch("circle-to-shape", func() { _ = circ.(shape) })

	// 6. An error source: Go names the static interface, error.
	var err error = errors.New("boom")
	catch("error-to-*myErr", func() { _ = err.(*myErr) })

	// NEGATIVE CONTROLS - these must NOT panic.
	catch("int-to-int", func() { _ = boxed.(int) })
	var ptr any = &circle{1}
	catch("error-to-error", func() { _ = err.(error) })
	catch("*circle-to-shape", func() { _ = ptr.(shape) })
	catch("comma-ok", func() { _, ok := boxed.(string); fmt.Println("comma-ok:", ok) })

	fmt.Println("still running")
}
