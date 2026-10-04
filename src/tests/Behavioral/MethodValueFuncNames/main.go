// MethodValueFuncNames pins runtime.FuncForPC(...).Name() for METHOD VALUES, METHOD EXPRESSIONS and
// function literals -- the names go-cmp's cmp/internal/function.NameOf reads, whose TestNameOf agreed
// on 5 of 17 rows converted. Go's spelling, from the gc toolchain:
//
//   - a method expression names the method by its receiver type: `main.T.valueMethod`,
//     `main.(*T).pointerMethod`; `(*T).valueMethod` is the compiler's pointer wrapper, also
//     `main.(*T).valueMethod`;
//   - a method VALUE (bound receiver) is a distinct `-fm` wrapper: for a pointer method
//     `main.(*T).pointerMethod-fm`, whichever spelling of the receiver bound it;
//   - a method value taken through an INTERFACE is named by the interface: `fmt.Stringer.String-fm`;
//   - a function literal is `Outer.funcN`, a per-enclosing-function counter starting at 1, and a
//     literal directly inside `main` is `main.main.funcN`.
//
// Every named function that holds a literal is //go:noinline, for the reason FuncLiteralCallerNames
// records: gc renames a literal whose enclosing function was inlined, and go2cs performs no inlining.
//
// A VALUE-receiver method value (`t.valueMethod`, Go: `main.T.valueMethod-fm`) is emitted as a lambda
// over a copy of the receiver, so the runtime names it from the GoPositionMap record's method-value
// map (its Go line plus its callee) rather than from the emission. Its own guard,
// MethodValueFmRecord, pins that name, the -fm frame Go hides from runtime.Callers, and the
// same-method literal the record must not capture.
package main

import (
	"fmt"
	"reflect"
	"runtime"
)

type T struct{ n int }

func (t T) valueMethod() int { return t.n }

func (t *T) pointerMethod() int { return t.n }

func nameOf(fn any) string {
	f := runtime.FuncForPC(reflect.ValueOf(fn).Pointer())
	if f == nil {
		return "<nil Func>"
	}
	if name := f.Name(); name != "" {
		return name
	}
	return "<empty name>"
}

//go:noinline
func makeLiteral() func() int {
	return func() int { return 1 }
}

//go:noinline
func twoLiterals() (func() int, func() int) {
	first := func() int { return 1 }
	second := func() int { return 2 }
	return first, second
}

//go:noinline
func boundInsideLiteral(t *T) string {
	// The method value is taken INSIDE a literal: Go still names the -fm wrapper, never the literal.
	return func() string { return nameOf(t.pointerMethod) }()
}

func main() {
	t := T{n: 1}
	p := &t

	fmt.Println("method expression, value receiver:   ", nameOf(T.valueMethod))
	fmt.Println("method expression, pointer receiver: ", nameOf((*T).pointerMethod))
	fmt.Println("method expression, promoted to *T:   ", nameOf((*T).valueMethod))

	fmt.Println("method value, pointer receiver:      ", nameOf(p.pointerMethod))
	fmt.Println("method value, &t pointer receiver:   ", nameOf((&t).pointerMethod))
	fmt.Println("method value, addressable t:         ", nameOf(t.pointerMethod))
	fmt.Println("method value inside a literal:       ", boundInsideLiteral(p))

	fmt.Println("literal returned by a function:      ", nameOf(makeLiteral()))
	first, second := twoLiterals()
	fmt.Println("first literal:                       ", nameOf(first))
	fmt.Println("second literal:                      ", nameOf(second))

	fmt.Println("package function:                    ", nameOf(nameOf))

	inMain := func() int { return 3 }
	fmt.Println("literal inside main:                 ", nameOf(inMain))

	pc, _, _, _ := runtime.Caller(0)
	fmt.Println("main's own frame:                    ", runtime.FuncForPC(pc).Name())

	var s fmt.Stringer = named(1)
	fmt.Println("interface method value:              ", nameOf(s.String))
}

type named int

func (n named) String() string { return "named" }
