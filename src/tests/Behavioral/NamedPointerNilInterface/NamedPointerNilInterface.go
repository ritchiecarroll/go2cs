package main

import (
	"fmt"
	"unsafe"
)

// Guards the typed nil of a NAMED pointer type in interface space. Go's interface holds the pair
// (dynamic type, value), so a nil value of `type P *T` boxed into `any` is a NON-nil interface whose
// dynamic type is P: `x != nil` is true, `x.(P)` succeeds with a nil result, %T prints the name and a
// type switch takes P's case. That holds however the nil was produced -- a zero-valued variable, a
// struct field, a slice element, a map miss, a result -- and through every slot that boxes it: an
// assignment, a return, an argument, a map value or key, a composite element, a channel send, a
// panic, a deferred or go call, a closure, and a type-parameter value boxed inside a generic body.
// The unnamed `*T` is the near miss beside each row.

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

// a package-level any initialized from a nil P
var pkgP P
var pkgAny any = pkgP

// a direct variadic ...any parameter
func boxVar(xs ...any) any { return xs[0] }

// a multi-value return whose first result is any
func retPair() (any, error) {
	var p P
	return p, nil
}

// a nil P panicked and recovered: a non-nil interface, so not Go's PanicNilError
func panicP(p P) (r any) {
	defer func() { r = recover() }()
	panic(p)
}

// a deferred call evaluates its any argument at the defer
func deferP(p P) {
	defer report("defer P", p)
}

// a go call boxes its any argument at the go statement
func reportDone(label string, x any, done chan bool) {
	report(label, x)
	done <- true
}

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

	// a conversion of a nil operand: from *T it builds the wrapper, from a P it carries the
	// operand's nil through, and either way the result is a typed nil of P
	report("conv P(*T nil)", P(up))
	report("conv P(P nil)", P(p))

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

	// conversions through unsafe.Pointer(nil): the cast carries the null, and the result is still a
	// typed nil of the target
	report("conv P(unsafe nil)", P(unsafe.Pointer(nil)))
	report("conv *T(unsafe nil)", (*T)(unsafe.Pointer(nil)))

	// a channel send and a select send into chan any
	ch := make(chan any, 1)
	ch <- p
	report("chan send P", <-ch)
	select {
	case ch <- p:
	}
	report("select send P", <-ch)

	// an any map KEY: store, index and literal; a typed nil P is not the nil key
	mk := map[any]int{}
	mk[p] = 1
	for k := range mk {
		report("map key store P", k)
	}
	v1, ok1 := mk[p]
	_, okNil := mk[nil]
	fmt.Println("map key index P:", v1, ok1, "nil key:", okNil)
	ml := map[any]int{p: 2}
	for k := range ml {
		report("map key literal P", k)
	}

	// a direct ...any call and append to []any
	report("variadic P", boxVar(p))
	as := append([]any{}, p)
	report("append P", as[0])

	// panic(p) then recover().(P)
	r := panicP(p)
	report("recover P", r)
	rp, okR := r.(P)
	fmt.Println("recover().(P):", okR, rp == nil)

	// a deferred call and a go call with a nil P argument
	deferP(p)
	done := make(chan bool)
	go reportDone("go P", p, done)
	<-done

	// a closure returning any, a multi-value (any, error) return, and a package-level any
	f := func() any { return p }
	report("closure P", f())
	a, err := retPair()
	report("ret pair P", a)
	fmt.Println("ret pair err nil:", err == nil)
	report("pkg var P", pkgAny)

	// the comma-ok assertion yields a nil P that is still nil
	if v, ok := x.(P); ok {
		fmt.Println("comma-ok P:", ok, v == nil)
	} else {
		fmt.Println("comma-ok P: failed")
	}

	// two typed nils of one named type compare equal; of different types, unequal
	var x2 any = P(nil)
	fmt.Println("x==x2", x == x2, "x==z", x == z, "x==y", x == y)

	// the mixed comparison of an any against a P converts the P to any
	fmt.Println("x==p", x == p, "z==up", z == up)

	// the three forms of a nil P -- a nil variable, P(nil) and P(nil *T) -- are one value: equal, and
	// one map key
	fmt.Println("P(nil)==P(up)", any(P(nil)) == any(P(up)), "x==P(up)", x == any(P(up)))
	keys := map[any]int{}
	keys[any(P(nil))] = 1
	keys[any(P(up))] = 2
	keys[x] = 3
	fmt.Println("nil P keys:", len(keys), keys[any(P(nil))])

	// a non-nil named pointer still crosses as itself
	t := &T{n: 7}
	var q P = t
	var w any = q
	report("non-nil P", w)
	fmt.Println("non-nil P value:", w.(P).n)
}
