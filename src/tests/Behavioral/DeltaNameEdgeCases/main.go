package main

// Every Δ name the marker-glyph refusal (GO2CS2015) lets through, each printing a value that shows it bound to its own
// declaration and not to a name the converter generates. Struct fields and types whose names start with Δ, and methods
// whose names start with Δ and go on, are refused today (golib drops a leading Δ when it projects their Go names), so
// they are not here; a method named exactly Δ converts and is here.

import (
	"fmt"
	"reflect"
	"runtime"
	"strings"
	"unicode"
	"unicode/utf8"
)

// Δ-led package variables, a bare Δ, and a Δ-led constant convert as written.
var Δx = 2.5
var Δ = 7

const ΔK = 3

// A Δ-led function, named back through runtime.Caller (the name only: the call line is not printed).
func Δf() string {
	pc, _, _, ok := runtime.Caller(0)

	if !ok {
		return "no caller"
	}

	return runtime.FuncForPC(pc).Name()
}

// A Δ-led parameter.
func scale(Δv float64) float64 {
	return Δv * 2
}

// Δt beside a local t: two names, two values.
func deltaBesideLocal() {
	t := 1.5
	Δt := 0.25
	fmt.Println("t:", t, "Δt:", Δt, "t+Δt:", t+Δt)
}

// Locals named like words go2cs reserves are renamed by the converter (slice -> Δslice, array -> Δarray); each reads back
// its own value, and a package-level slice2 one name away keeps its own.
var slice2 = []int{9, 8}

func reservedLocals() {
	slice := []int{1, 2, 3}
	array := [2]string{"a", "b"}
	slice = append(slice, len(array))
	fmt.Println("slice:", slice, "array:", array, "slice2:", slice2)
}

// A variable shadowed in nested scopes: the converter renames the inner ones (xΔ1, xΔ2); every level prints its own.
func shadowedLevels() {
	x := 1
	fmt.Println("level 0:", x)
	{
		x := 2
		fmt.Println("level 1:", x)
		{
			x := 3
			fmt.Println("level 2:", x)
		}
		fmt.Println("level 1 again:", x)
	}
	fmt.Println("level 0 again:", x)
}

// A type switch binding: at an interface case label the converter binds it through a temporary named Δw.
func describe(v any) string {
	switch w := v.(type) {
	case int:
		return fmt.Sprint("int ", w+1)
	case string:
		return "string " + strings.ToUpper(w)
	case []int:
		return fmt.Sprint("slice of ", len(w))
	case Texter:
		return "texter " + w.Text()
	default:
		return fmt.Sprintf("other %v", w)
	}
}

// unicode is also the parent namespace of unicode/utf8, so the converter renames its import alias (Δunicode).
func importAlias() {
	fmt.Println("IsUpper(Δ):", unicode.IsUpper('Δ'), "RuneLen(Δ):", utf8.RuneLen('Δ'))
}

// A generic function under a method-set constraint whose result carries T in a generic type, instantiated with pointers,
// takes a shape witness: a type parameter the converter names TΔTexter. It is called with two instantiations.
type Texter interface{ Text() string }

type word struct{ s string }

func (w word) Text() string { return "word " + w.s }

type count int

func (c count) Text() string { return fmt.Sprint("count ", int(c)) }

type pair[T Texter] struct{ first, second T }

func keep[T Texter](v T) (pair[T], string) { return pair[T]{v, v}, "[" + v.Text() + "]" }

// A method named exactly Δ: golib's method projections leave a bare Δ alone.
type Q struct{}

func (Q) Δ() string {
	pc, _, _, _ := runtime.Caller(0)
	return "Q.Δ in " + runtime.FuncForPC(pc).Name()
}

func bareDeltaMethod() {
	fmt.Println(Q{}.Δ())
	var i any = Q{}
	d, ok := i.(interface{ Δ() string })
	fmt.Println("assert:", ok, d.Δ())
	fmt.Println("reflect method:", reflect.TypeOf(Q{}).Method(0).Name)
}

// Package-level names one spelling away from a generated one: no y is declared, bytes is not imported, Q is a type
// and not a type parameter, no type Other exists, and Δp, Δpx and Δe miss the generated parameter shapes ΔpN and ΔeN.
var yΔ1, yΔp = 11, 12
var Δbytes = "not an import"
var QΔTexter, TΔOther = 21, 22
var Δp, Δpx, Δe = 31, 32, 33

// A field and a method with a non-leading Δ: golib drops only a leading one.
type R struct{ tΔ int }

func (r R) UΔ() int { return r.tΔ * 2 }

func nonLeadingDelta() {
	r := R{tΔ: 5}
	fmt.Println(fmt.Sprintf("%+v", r), r.UΔ(), reflect.TypeOf(r).Field(0).Name, reflect.TypeOf(r).Method(0).Name)
}

// A function's type parameter named ΔT: the interface type parameter go2cs names ΔT lives only in a converted interface.
func twice[ΔT any](v ΔT) string { return fmt.Sprint(v, v) }

// A generic constrained by an imported interface. Stage 1 of the shape witness serves only a same-package constraint (a
// pointer instantiation here is GO2CS2005), so no witness type parameter is minted for it; a value instantiation converts.
type label struct{ s string }

func (l label) String() string { return "label " + l.s }

func strs[S fmt.Stringer](v S) ([]S, string) { return []S{v}, v.String() }

func main() {
	fmt.Println("Δx:", Δx, "Δ:", Δ, "ΔK:", ΔK, "Δx*ΔK:", Δx*ΔK)
	fmt.Println("scale(Δx):", scale(Δx))
	fmt.Println("Δf:", Δf())
	deltaBesideLocal()
	reservedLocals()
	shadowedLevels()
	fmt.Println(describe(41), "|", describe("go"), "|", describe([]int{1, 2}), "|", describe(word{"w"}), "|", describe(1.5))
	importAlias()

	n := count(3)
	ws, wText := keep(&word{"Δ"})
	cs, cText := keep(&n)
	fmt.Println(wText, cText, ws.second.s, int(*cs.first))
	bareDeltaMethod()
	fmt.Println("names:", yΔ1, yΔp, Δbytes, QΔTexter, TΔOther, Δp, Δpx, Δe)
	nonLeadingDelta()
	fmt.Println("twice:", twice(4), twice("ab"))
	ls, lText := strs(label{"x"})
	fmt.Println("strs:", len(ls), lText)
}
