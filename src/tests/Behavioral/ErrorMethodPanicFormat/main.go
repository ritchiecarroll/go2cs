// Guards a Go panic raised INSIDE an Error method that the runtime reaches without a compile-time
// adapter.
//
// Each value below reaches fmt as `any`, so fmt's own `case error` asserts it at run time and golib's
// AdapterBinder builds the error value: for a value-typed error that is error's object shell, which
// forwards Error through a reflective invoker; under Native AOT it is every error the binder builds.
// fmt calls Error inside its catchPanic, which must see the panic as a Go panic and print Go's
// %!v(PANIC=Error method: ...) text. A reflection wrapper around the panic would escape catchPanic
// and end the program. The nil-pointer rows hold the receiver rules: a pointer-receiver Error
// answers a nil receiver itself, and a value-receiver Error reached through a nil pointer panics,
// which fmt prints as <nil>.
package main

import "fmt"

// E's Error panics.
type E struct{ m map[string]int }

func (e E) Error() string { panic("boom") }

// V has a value-receiver Error.
type V struct{ n int }

func (v V) Error() string { return fmt.Sprint("V(", v.n, ")") }

// P has a pointer-receiver Error that answers a nil receiver.
type P struct{ n int }

func (p *P) Error() string {
	if p == nil {
		return "nil *P"
	}
	return fmt.Sprint("P(", p.n, ")")
}

func main() {
	var e any = E{}
	fmt.Println(fmt.Sprintf("%v", e))
	fmt.Println(fmt.Sprintf("%s", e))

	func() {
		defer func() { fmt.Println("recovered:", recover()) }()
		if err, ok := e.(error); ok {
			fmt.Println("unreached:", err.Error())
		}
	}()

	var v any = V{7}
	fmt.Println(fmt.Sprintf("%v", v))
	if err, ok := v.(error); ok {
		w, isV := err.(V)
		fmt.Println(isV, w.n, err.Error())
	}

	var nv *V
	var a any = nv
	fmt.Println(fmt.Sprintf("%v", a))

	var np *P
	var b any = np
	fmt.Println(fmt.Sprintf("%v", b))

	var c any = &P{3}
	fmt.Println(fmt.Sprintf("%v", c))
}
