// Guards the no-inline carrier: a converted function that must keep its own frame is emitted as a
// partial method's implementing part (`partial` where the [MethodImpl(NoInlining)] prefix stood), and
// go2cs-gen writes the declaring part carrying the attribute. Every shape below reaches runtime.Caller
// and prints the frame it names, so an implementation that loses the attribute on any shape lets the
// JIT inline that function into main and the printed name becomes main.main. The fixture runs with
// tiered compilation off (runtimeconfig.template.json), so the first call is already the optimizing
// JIT's code and the loop below is not needed to reach it.
//
// The func literal and the local function cannot be partial: they keep the attribute themselves, and
// are here so the fallback is exercised beside the carrier.
package main

import (
	"fmt"
	"runtime"
)

// here names the function that called it.
func here() string {
	pc, _, _, ok := runtime.Caller(1)
	if !ok {
		return "<no caller>"
	}
	return runtime.FuncForPC(pc).Name()
}

// plain is a thin forwarder to here: the closure marks it, and only the mark keeps its frame.
func plain() string { return here() }

// variadic takes a params array.
func variadic(xs ...int) string { return here() }

// pair returns a named tuple and asks for its own frame directly.
func pair() (name string, n int) {
	pc, _, _, _ := runtime.Caller(0)
	return runtime.FuncForPC(pc).Name(), 1
}

// generic is a type-parameterized function.
func generic[T any](x T) string { return here() }

type counter struct{ n int }

// ptr is a pointer-receiver method: a `this ref` extension plus go2cs-gen's ж<counter> overload.
func (c *counter) ptr() string { return here() }

// val is a value-receiver method.
func (c counter) val() string { return here() }

type box[T any] struct{ v T }

// get is a pointer-receiver method on a generic type.
func (b *box[T]) get() string { return here() }

var fromInit string

// init is a module initializer as well as a carrier: it asks for its own frame directly.
func init() {
	pc, _, _, _ := runtime.Caller(0)
	fromInit = runtime.FuncForPC(pc).Name()
}

// main takes the mark too, because the literal inside it calls runtime.Caller (the scan of a body
// enters its func literals), so the program's entry point is a carrier as well.
func main() {
	c := &counter{}
	b := &box[int]{v: 1}

	// A func literal bound to a variable passed on, so it stays a lambda.
	lit := func() string { return here() }
	call := func(f func() string) string { return f() }

	// A func literal that is only called becomes a C# local function.
	local := func() string {
		pc, _, _, _ := runtime.Caller(0)
		return runtime.FuncForPC(pc).Name()
	}

	name, n := pair()

	fmt.Println(plain())
	fmt.Println(variadic(1, 2))
	fmt.Println(name, n)
	fmt.Println(generic(1))
	fmt.Println(c.ptr())
	fmt.Println(c.val())
	fmt.Println(b.get())
	fmt.Println(fromInit)
	fmt.Println(call(lit))
	fmt.Println(local())
}
