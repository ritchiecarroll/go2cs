// UnnamedArrayTypeArgDescriptor: unique.Make over an UNNAMED array type
// argument. C# spells every Go [N]E as one array<E>, so the length lives in
// the value and a descriptor built from the type alone has none; Make reads
// the type's size first (a zero size takes its zero-size arm, which would give
// every pair one handle). The array-dims companion carries the length from
// the call site to the type-only descriptor, across assemblies.
package main

import (
	"fmt"
	"unique"

	"UnnamedArrayTypeArgDescriptor/wrap"
)

// pair is a DEFINED array type: its own C# type carries the length.
type pair [2]string

// wrapMake is an unexported generic forwarder in the calling package.
func wrapMake[T comparable](v T) unique.Handle[T] {
	return unique.Make(v)
}

func main() {
	// The cure: unnamed [2]string and [3]string type arguments.
	ab := unique.Make([2]string{"a", "b"})
	ac := unique.Make([2]string{"a", "c"})
	ab2 := unique.Make([2]string{"a", "b"})
	abc := unique.Make([3]string{"a", "b", "c"})
	fmt.Println("pair ab == ab:", ab == ab2)
	fmt.Println("pair ab == ac:", ab == ac)
	fmt.Println("pair values:", ab.Value(), ac.Value(), ab2.Value())
	fmt.Println("triple value:", abc.Value(), len(abc.Value()))

	// A pointer to an array: the descriptor is a pointer's, no dims needed.
	p := &[2]string{"x", "y"}
	q := &[2]string{"x", "y"}
	fmt.Println("pointer p == p:", unique.Make(p) == unique.Make(p))
	fmt.Println("pointer p == q:", unique.Make(p) == unique.Make(q))

	// A struct holding an array: the struct's size comes from its field.
	s1 := unique.Make(struct{ a [2]string }{[2]string{"s", "t"}})
	s2 := unique.Make(struct{ a [2]string }{[2]string{"s", "t"}})
	s3 := unique.Make(struct{ a [2]string }{[2]string{"s", "u"}})
	fmt.Println("struct same:", s1 == s2, "different:", s1 == s3, "value:", s1.Value().a)

	// A defined array type.
	d1 := unique.Make(pair{"d", "e"})
	d2 := unique.Make(pair{"d", "e"})
	d3 := unique.Make(pair{"d", "f"})
	fmt.Println("defined same:", d1 == d2, "different:", d1 == d3, "value:", d1.Value())

	// A method value of the instantiated generic.
	f := unique.Make[[2]string]
	fmt.Println("method value same:", f([2]string{"m", "n"}) == f([2]string{"m", "n"}),
		"different:", f([2]string{"m", "n"}) == f([2]string{"m", "o"}))
	fmt.Println("method value meets direct:", f([2]string{"a", "b"}) == ab)

	// An unexported forwarder in this package.
	w1 := wrapMake([2]string{"w", "x"})
	w2 := wrapMake([2]string{"w", "x"})
	w3 := wrapMake([2]string{"w", "y"})
	fmt.Println("local wrapper same:", w1 == w2, "different:", w1 == w3, "meets direct:", wrapMake([2]string{"a", "b"}) == ab)

	// An exported forwarder in another package.
	e1 := wrap.Wrap([2]string{"e", "f"})
	e2 := wrap.Wrap([2]string{"e", "f"})
	e3 := wrap.Wrap([3]string{"e", "f", "g"})
	fmt.Println("foreign wrapper same:", e1 == e2, "meets direct:", wrap.Wrap([2]string{"a", "b"}) == ab, "triple:", e3.Value())
	fmt.Println("foreign Same:", wrap.Same([2]string{"k", "l"}, [2]string{"k", "l"}), wrap.Same([2]string{"k", "l"}, [2]string{"k", "m"}))
	fmt.Println("foreign non-array:", wrap.Same("str", "str"), wrap.Same(1, 2))
}
