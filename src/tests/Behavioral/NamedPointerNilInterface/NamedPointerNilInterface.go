package main

import "fmt"

// Guards the typed nil of a NAMED pointer type in interface space. Go's interface holds the pair
// (dynamic type, value), so a nil value of `type P *T` boxed into `any` is a NON-nil interface whose
// dynamic type is P: `x != nil` is true, `x.(P)` succeeds with a nil result, %T prints the name and a
// type switch takes P's case. That holds however the nil was produced -- a zero-valued variable, a
// struct field, a slice element, a map miss, a result -- and through every slot that boxes it: an
// assignment, a return, an argument, a map value, a composite element, and a type-parameter value
// boxed inside a generic body. The unnamed `*T` is the near miss beside each row.

type T struct{ n int }

type P *T

type IP *int

type holder struct {
	p  P
	ip IP
}

func boxP(p P) any { return p }

func retP() (r any) {
	var p P
	r = p
	return
}

func retPDirect() any {
	var p P
	return p
}

// a value of type-parameter type boxed inside a generic body: the type argument decides the typed nil
func boxE[E any](e E) any { return e }

func describe(x any) string {
	switch v := x.(type) {
	case nil:
		return "case nil"
	case P:
		return fmt.Sprint("case P, nil=", v == nil)
	case IP:
		return fmt.Sprint("case IP, nil=", v == nil)
	case *T:
		return fmt.Sprint("case *T, nil=", v == nil)
	default:
		return "default"
	}
}

func report(label string, x any) {
	_, okP := x.(P)
	_, okIP := x.(IP)
	_, okT := x.(*T)
	fmt.Printf("%s: nil=%v %%T=%T %%v=%v P=%v IP=%v *T=%v %s\n", label, x == nil, x, x, okP, okIP, okT, describe(x))
}

func main() {
	// assignment of a zero-valued variable
	var p P
	var x any = p
	report("assign P", x)

	var ip IP
	var y any = ip
	report("assign IP", y)

	var up *T
	var z any = up
	report("assign *T", z)

	// a typed nil conversion, the form that already holds its type
	report("conv P(nil)", any(P(nil)))

	// argument passing
	report("arg P", p)
	report("arg *T", up)

	// returns
	report("ret P param", boxP(p))
	report("ret P named", retP())
	report("ret P direct", retPDirect())

	// a map value, written and read back, and a map miss
	m := map[string]any{"p": p, "up": up}
	report("map P", m["p"])
	report("map *T", m["up"])

	mp := map[string]P{}
	report("map miss P", mp["absent"])

	// a struct field and a slice element
	var h holder
	report("field P", h.p)
	report("field IP", h.ip)

	s := make([]P, 1)
	report("elem P", s[0])

	// a composite element of []any
	xs := []any{p, ip, up}
	for i, e := range xs {
		report(fmt.Sprint("slice[", i, "]"), e)
	}

	// a type argument: the boxing happens inside the generic body
	report("generic P", boxE(p))
	report("generic IP", boxE(ip))
	report("generic *T", boxE(up))

	// the comma-ok assertion yields a nil P that is still nil
	if v, ok := x.(P); ok {
		fmt.Println("comma-ok P:", ok, v == nil)
	} else {
		fmt.Println("comma-ok P: failed")
	}

	// two typed nils of one named type compare equal; of different types, unequal
	var x2 any = P(nil)
	fmt.Println("x==x2", x == x2, "x==z", x == z, "x==y", x == y)

	// a non-nil named pointer still crosses as itself
	t := &T{n: 7}
	var q P = t
	var w any = q
	report("non-nil P", w)
	fmt.Println("non-nil P value:", w.(P).n)
}
