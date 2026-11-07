// Asserts a value to an interface with a variadic method at run time, where no compile-time adapter
// exists for the pair: the dynamic type is a struct, or a pointer to one. Go answers true for each.
// Native AOT once read false for every variadic case here (DATA-aot-variadic-shell.md): the
// runtime-built implementation needed a generic instantiation over the struct.
package main

import (
	"fmt"
	"strings"
	"testing"
)

// TT: testing.T's methods, promoted through a pointer embed from another assembly.
type TT struct{ *testing.T }

// L, LL: a local variadic method on a pointer receiver, promoted through a pointer embed.
type L struct{ tag string }

func (l *L) Error(args ...any) { fmt.Println("L.Error", l.tag, len(args), fmt.Sprint(args...)) }

type LL struct{ *L }

// V: a local variadic value method, declared directly.
type V struct{ n int }

func (v V) Error(args ...any) { fmt.Println("V.Error", v.n, len(args), fmt.Sprint(args...)) }

func (v V) Join(sep string, parts ...string) string { return fmt.Sprint(v.n) + ":" + strings.Join(parts, sep) }

// G: a generic type with a variadic value method.
type G[T any] struct{ v T }

func (g G[T]) Error(args ...any) { fmt.Println("G.Error", g.v, len(args)) }

// u: an unexported variadic method, asserted inside its own package.
type u struct{ id int }

func (x u) logf(format string, args ...any) { fmt.Printf("u.logf %d "+format+"\n", append([]any{x.id}, args...)...) }

// Errorer: a NAMED interface with the variadic method.
type Errorer interface{ Error(...any) }

func check(label string, ok bool) { fmt.Printf("%-28s: %v\n", label, ok) }

func main() {
	var tt any = TT{}
	_, ok := tt.(interface{ Error(...any) })
	check("TT    Error(...any)", ok)
	_, ok = tt.(interface{ Name() string })
	check("TT    Name() string", ok)
	_, ok = tt.(interface{ Logf(string, ...any) })
	check("TT    Logf(string, ...any)", ok)
	_, ok = tt.(interface{ Failed() bool })
	check("TT    Failed() bool", ok)
	_, ok = tt.(Errorer)
	check("TT    Errorer", ok)

	var pt any = &TT{}
	_, ok = pt.(interface{ Error(...any) })
	check("*TT   Error(...any)", ok)

	var t any = &testing.T{}
	_, ok = t.(interface{ Error(...any) })
	check("*T    Error(...any)", ok)
	_, ok = t.(interface{ Name() string })
	check("*T    Name() string", ok)

	// The asserted values are called through the interface: the variadic arguments arrive.
	var ll any = LL{&L{"embedded"}}
	if e, ok := ll.(interface{ Error(...any) }); ok {
		check("LL    Error(...any)", ok)
		e.Error("a", 1, true)
	} else {
		check("LL    Error(...any)", ok)
	}

	var v any = V{7}
	if e, ok := v.(interface{ Error(...any) }); ok {
		check("V     Error(...any)", ok)
		e.Error()
		e.Error("x", "y")
	} else {
		check("V     Error(...any)", ok)
	}
	if j, ok := v.(interface{ Join(string, ...string) string }); ok {
		check("V     Join(string, ...string)", ok)
		fmt.Println(j.Join("-", "a", "b", "c"))
	} else {
		check("V     Join(string, ...string)", ok)
	}
	var pv any = &V{8}
	if e, ok := pv.(Errorer); ok {
		check("*V    Errorer", ok)
		e.Error("through", "pointer")
	} else {
		check("*V    Errorer", ok)
	}

	var g any = G[int]{3}
	if e, ok := g.(interface{ Error(...any) }); ok {
		check("G     Error(...any)", ok)
		e.Error(1, 2)
	} else {
		check("G     Error(...any)", ok)
	}

	var x any = u{5}
	if l, ok := x.(interface{ logf(string, ...any) }); ok {
		check("u     logf(string, ...any)", ok)
		l.logf("%s=%d", "k", 9)
	} else {
		check("u     logf(string, ...any)", ok)
	}

	// A miss stays a miss: the signature differs.
	_, ok = v.(interface{ Error(...string) })
	check("V     Error(...string)", ok)
}
