// GlyphLibraryFrameNames prints the Go names the runtime gives a library method's frames and function values: a
// value-receiver and a pointer-receiver method's own frame, their method values and their method expressions. Library
// methods are converted in the suffix form (`Name⁔` behind a forwarder). Package main has no ⁔: its methods stay
// extension methods in main_package, so its local type is the control. Names only: no line is printed.
//
// Not in this row: a function literal inside a method. Its Go name carries the method's receiver
// (`pkg.(*T).M.func1`), which the runtime does not give it yet; the converter will emit it in its own seat.
package main

import (
	"fmt"
	"reflect"
	"runtime"

	"GlyphLibraryFrameNames/framelib"
)

type local struct{}

func (l local) Name() string {
	return framelib.Here()
}

func funcName(f any) string {
	return runtime.FuncForPC(reflect.ValueOf(f).Pointer()).Name()
}

func main() {
	var c framelib.Counter
	p := &c

	fmt.Println("value method:", c.Name())
	fmt.Println("pointer method:", p.Bump())
	fmt.Println("local method:", local{}.Name())

	fmt.Println("value method value:", funcName(c.Name))
	fmt.Println("value method expression:", funcName(framelib.Counter.Name))
	fmt.Println("pointer method value:", funcName(p.Bump))
	fmt.Println("pointer method expression:", funcName((*framelib.Counter).Bump))
	fmt.Println("local method value:", funcName(local{}.Name))
}
