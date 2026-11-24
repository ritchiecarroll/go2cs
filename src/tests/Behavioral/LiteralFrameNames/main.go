// LiteralFrameNames: a function literal's frame carries the name Go gives it. Go names a literal for its enclosing
// function with a per-function counter: (*ring).init.func1 for a literal in a method named init, init.0.func1 and
// init.1.func1 for literals in the first and second func init, init.func1 for one in a package-level var
// initializer, and outer.func1.1 for a literal nested in another. A library's two types with same-named methods
// keep their receivers apart. Go's name for a literal follows its inliner (a literal in a method inlined into main
// is renamed main.main.(*T).M.func1), so every enclosing function here is //go:noinline and the names are the
// declared ones.
//
// The last line is the line half: an immediately invoked literal's caller line is the line of its closing }(),
// where the call is.
package main

import (
	"fmt"
	"runtime"

	"LiteralFrameNames/framelib"
)

type ring struct{ n int }

// A method named init, beside the package's func init below.
//
//go:noinline
func (r *ring) init() string {
	f := func() string {
		r.n++
		return here()
	}
	return f()
}

var fromVar = func() string { return here() }

var initNames []string

func init() {
	f := func() {
		// A long body on purpose: the literal's lines sit far from its function's first statement.
		a := 1
		b := a + 1
		c := b + 1
		d := c + 1
		e := d + 1
		if e > 100 {
			fmt.Println("unreachable")
		}
		initNames = append(initNames, here())
	}
	f()
}

func init() {
	g := func() {
		initNames = append(initNames, here())
	}
	g()
}

// A literal handed to call is not invoked in place, so Go cannot inline it into its parent and rename the literal
// nested in it.
//
//go:noinline
func outer() string {
	var name string
	call(func() {
		call(func() {
			name = here()
		})
	})
	return name
}

//go:noinline
func call(f func()) { f() }

//go:noinline
func callLine() int {
	var line int
	func() {
		_, _, line, _ = runtime.Caller(1)
	}()
	return line
}

func here() string {
	pc, _, _, _ := runtime.Caller(1)
	return runtime.FuncForPC(pc).Name()
}

func main() {
	for _, name := range initNames {
		fmt.Println("init literal:", name)
	}

	fmt.Println("method init literal:", (&ring{}).init())
	fmt.Println("var literal:", fromVar())
	fmt.Println("nested literal:", outer())
	fmt.Println("counter literal:", framelib.CounterName())
	fmt.Println("gauge literal:", framelib.GaugeName())
	fmt.Println("caller line of an invoked literal:", callLine())
}
