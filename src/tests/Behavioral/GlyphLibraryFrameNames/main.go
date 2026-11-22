// GlyphLibraryFrameNames prints the Go names the runtime gives a library method's frames and function values: a
// closure inside a value-receiver and a pointer-receiver method, a value-receiver method value and method expression,
// and a pointer-receiver method value and method expression. Library methods are converted in the suffix form
// (`Name⁔` behind a forwarder). Package main has no ⁔: its methods stay extension methods in main_package, so its
// local type is the control. Two more package-main shapes: a closure inside `func init()` while a type in main also
// has a METHOD named init, and a closure inside that method. Names only: no line is printed.
package main

import (
	"fmt"
	"reflect"
	"runtime"

	"GlyphLibraryFrameNames/framelib"
)

type local struct{}

func (l local) Closure() string {
	f := func() string {
		return framelib.Here()
	}

	return f()
}

// ring has a method named init beside the package's func init: both are `init` in main_package.
type ring struct{}

func (r *ring) init() string {
	f := func() string {
		return framelib.Here()
	}

	return f()
}

var initClosure string

func init() {
	f := func() {
		initClosure = framelib.Here()
	}

	f()
}

func funcName(f any) string {
	return runtime.FuncForPC(reflect.ValueOf(f).Pointer()).Name()
}

func main() {
	var c framelib.Counter
	p := &c

	fmt.Println("method:", c.Name())
	fmt.Println("closure in value method:", c.Closure())
	fmt.Println("closure in pointer method:", p.Bump())
	fmt.Println("local closure:", local{}.Closure())
	fmt.Println("closure in func init:", initClosure)
	fmt.Println("closure in method init:", (&ring{}).init())

	fmt.Println("value method value:", funcName(c.Name))
	fmt.Println("value method expression:", funcName(framelib.Counter.Name))
	fmt.Println("pointer method value:", funcName(p.Bump))
	fmt.Println("pointer method expression:", funcName((*framelib.Counter).Bump))
	fmt.Println("local method value:", funcName(local{}.Closure))
}
