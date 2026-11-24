package main

import (
	"fmt"
	"reflect"

	"DefinedFuncTypeIdentity/hooks"
)

// F is a methodless defined func type: it has an identity of its own.
type F func(string) error

// Alias is an alias, not a defined type: it IS func(string) error.
type Alias = func(string) error

// G has a method; it keeps its own type, as it always has.
type G func(int) int

func (g G) Twice(x int) int { return g(g(x)) }

type holder struct {
	f  F
	fs []F
}

func show(label string, v any) {
	t := reflect.TypeOf(v)
	fmt.Printf("%s: %%T=%T name=%q string=%q kind=%v\n", label, v, t.Name(), t.String(), t.Kind())
}

func takeF(f F) string                      { return fmt.Sprint(f("x") == nil) }
func takePlain(f func(string) error) string { return fmt.Sprint(f("y") == nil) }
func retF(plain func(string) error) F       { return plain }
func retPlain(f F) func(string) error       { return f }
func variadicF(fs ...F) int                 { return len(fs) }

func generic[T any](v T) string { return fmt.Sprintf("%T", v) }

type stringer struct{ s string }

// A tuple result deconstructed into existing variables, as context.WithCancel's is into `var cancel func()`.
func pairF() (int, F) { return 1, func(string) error { return nil } }

// A deferred call through a value of a defined func type, as database/sql's `defer release(err)`.
func deferF(f F) (out string) {
	defer f("deferred")
	return "deferred call"
}

// Keyed elements given method expressions, as internal/trace's dispatch table.
type node struct{ v int }

func (n *node) add(x int) int { return n.v + x }
func (n *node) mul(x int) int { return n.v * x }

type handler func(*node, int) int

func (s stringer) check(string) error { return nil }

func main() {
	var f F = func(string) error { return nil }
	plain := func(string) error { return nil }

	// The four facets.
	show("F", f)
	var nilF F
	show("nil F", nilF)
	show("hooks.Handler", hooks.Handler(func(x int) int { return x }))
	show("G", G(func(x int) int { return x }))
	show("Alias", Alias(plain))
	show("unnamed", plain)

	var i any = f
	_, isF := i.(F)
	_, isPlain := i.(func(string) error)
	fmt.Println("assert F", isF, "assert unnamed", isPlain)

	switch i.(type) {
	case func(string) error:
		fmt.Println("switch: unnamed")
	case F:
		fmt.Println("switch: F")
	}

	// Assignability: Go converts between F and its unnamed type implicitly.
	var a F = plain              // assignment, unnamed -> F
	var b func(string) error = f // assignment, F -> unnamed
	fmt.Println("assign", a("") == nil, b("") == nil)
	fmt.Println("argument", takeF(plain), takePlain(f))
	fmt.Println("return", retF(plain)("") == nil, retPlain(f)("") == nil)
	h := holder{f: plain, fs: []F{plain, f}}
	fmt.Println("composite", h.f("") == nil, len(h.fs))
	ch := make(chan F, 1)
	ch <- plain
	fmt.Println("channel", (<-ch)("") == nil)
	fs := append([]F{}, plain, f)
	fmt.Println("append", len(fs), variadicF(plain, f), variadicF(fs...))
	fmt.Println("generic", generic(f), generic(plain), generic[F](plain))
	s := stringer{"m"}
	var mv F = s.check // method value -> F
	fmt.Println("method value", mv("") == nil, F(s.check)("") == nil)
	var lit F = func(string) error { return fmt.Errorf("lit") }
	fmt.Println("func literal", lit("").Error())
	var none F
	fmt.Println("nil", none == nil, f != nil)
	fmt.Println("conversion", F(plain)("") == nil, (func(string) error)(f)("") == nil)

	// A tuple deconstructed into existing variables of the unnamed type.
	var n int
	var cancel func(string) error
	n, cancel = pairF()
	fmt.Println("tuple", n, cancel("") == nil)
	fmt.Println("defer", deferF(f))
	table := [3]handler{1: (*node).add, 2: (*node).mul}
	byName := map[string]F{"plain": plain, "f": f}
	fmt.Println("keyed", table[1](&node{2}, 3), table[2](&node{2}, 3), table[0] == nil, byName["plain"]("") == nil, len(byName))

	// Another package's named type, both ways.
	fmt.Println("hooks", hooks.Apply(func(x int) int { return x * 3 }, 2), hooks.Raw(hooks.Make(5), 1), hooks.Apply(hooks.Make(1), 1))

	// The near misses.
	var al Alias = f // an alias is the unnamed type
	fmt.Println("alias", al("") == nil)
	var g G = func(x int) int { return x + 1 }
	fmt.Println("method type", g.Twice(1))
}
