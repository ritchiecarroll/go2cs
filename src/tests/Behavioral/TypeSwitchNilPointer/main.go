package main

import "fmt"

// An interface holding a nil *T is NOT a nil interface in Go: it has a dynamic type, so a type switch
// matches `case *T:` and binds a nil pointer, and `case nil:` does not match. The converter's pointer
// adapter wraps the canonical typed-nil box rather than a null, so `.type()` unwraps it and the C#
// type pattern binds it. Covered: a non-empty interface and `any`, a single-type case that binds,
// a multi-type case (whose binding keeps the interface type), and a pointer-receiver method that Go
// calls on the nil pointee.

type Namer interface{ Name() string }

type T struct{ n int }

func (t *T) Name() string {
	if t == nil {
		return "nil T"
	}
	return fmt.Sprint("T", t.n)
}

type U struct{}

func (*U) Name() string { return "U" }

func classify(i Namer) string {
	switch v := i.(type) {
	case nil:
		return "nil interface"
	case *T:
		return fmt.Sprint("*T, nil pointee: ", v == nil, ", Name: ", v.Name())
	default:
		return "default"
	}
}

func classifyAny(x any) string {
	switch v := x.(type) {
	case nil:
		return "nil any"
	case *T:
		return fmt.Sprint("*T in any, nil pointee: ", v == nil)
	default:
		return "default"
	}
}

func multi(i Namer) string {
	switch v := i.(type) {
	case *T, *U:
		return fmt.Sprint("*T or *U, interface nil: ", v == nil, ", Name: ", v.Name())
	case nil:
		return "nil interface"
	}
	return "default"
}

func main() {
	var p *T
	fmt.Println(classify(p))
	fmt.Println(classify(&T{1}))
	fmt.Println(classify(nil))

	fmt.Println(classifyAny(p))
	fmt.Println(classifyAny(nil))

	var u *U
	fmt.Println(multi(p))
	fmt.Println(multi(u))
	fmt.Println(multi(nil))
}
