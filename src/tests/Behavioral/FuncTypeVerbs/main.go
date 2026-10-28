package main

import (
	"errors"
	"fmt"
)

// fmt's %T and %v over func values, and over the values of func literals that return a rune
// literal (seat #40). C# infers a lambda's type from its body, so a rune literal returned without
// its cast makes C# infer a char where Go has a rune: the delegate's %T then names a type Go never
// has, and before seat #40 fmt's %v of the char overflowed the stack.

const letter = 'c'

func pair() (rune, bool) { return 'p', true }

func main() {
	// Func values of a few signatures: %T prints the Go func type.
	noArgs := func() {}
	square := func(x int32) int32 { return x * x }
	parse := func(s string, n int) (bool, error) {
		if len(s) > n {
			return false, errors.New("too long")
		}
		return true, nil
	}
	sum := func(xs ...int) int {
		total := 0
		for _, x := range xs {
			total += x
		}
		return total
	}
	format := func(f string, args ...any) string { return fmt.Sprintf(f, args...) }

	fmt.Printf("%T\n", noArgs)
	fmt.Printf("%T\n", square)
	fmt.Printf("%T\n", parse)
	fmt.Printf("%T\n", sum)
	fmt.Printf("%T\n", format)
	ok, err := parse("go", 5)
	fmt.Println(square(7), ok, err, sum(1, 2, 3), format("%d-%s", 4, "x"))

	// A nil func: %T names its type and %v prints <nil>.
	var none func(int32) int32
	var boxed any = none
	fmt.Printf("%T %v %v\n", boxed, boxed, none == nil)

	// Func literals that return a rune literal, as #23 r2 met them. (A literal returned as uint16 or
	// byte is seat #23's to guard, in RuneLiteralContexts.)
	shrink := func(r rune) rune { return 'a' }
	both := func() (rune, bool) { return 'b', true }
	named := func() rune { return letter }
	fmt.Printf("%T %T %T\n", shrink, both, named)
	r, flag := both()
	fmt.Printf("%v %v %v %v\n", shrink('x'), r, flag, named())
	fmt.Printf("%T %T\n", shrink('x'), r)

	// Held in an interface, a slice and a map.
	var held any = shrink
	fns := []func() rune{func() rune { return 'f' }}
	byName := map[string]func() rune{"g": func() rune { return 'g' }}
	fmt.Printf("%T %T %T %v %v\n", held, fns, byName, fns[0](), byName["g"]())
	p, q := pair()
	fmt.Println(p, q, 'z', letter)
}
