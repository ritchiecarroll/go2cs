package main

// Every Δ name the marker-glyph refusal (GO2CS2015) lets through, each printing a value that shows it bound to its own
// declaration and not to a name the converter generates. Struct fields, methods and types whose names start with Δ are
// refused today (golib drops a leading Δ when it projects their Go names), so they are not here.

import (
	"fmt"
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
}
