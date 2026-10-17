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
// Sources are empty interfaces. A named non-empty interface as the source (error, io.Reader) prints
// its own name in Go, and the converted assertion does not yet carry the static interface type.
package main

import (
	"fmt"
	"runtime"
)

type celsius float64

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

	// NEGATIVE CONTROLS - these must NOT panic.
	catch("int-to-int", func() { _ = boxed.(int) })
	catch("comma-ok", func() { _, ok := boxed.(string); fmt.Println("comma-ok:", ok) })

	fmt.Println("still running")
}
