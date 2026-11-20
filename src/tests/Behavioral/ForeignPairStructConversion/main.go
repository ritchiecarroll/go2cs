// ForeignPairStructConversion converts a struct value between two types that BOTH belong to other packages
// (sqlite-cs wall C).
//
// Go converts `lib.Pitch(o)`, with o an `other.Pitch`, because the two types have identical fields. In C# a
// conversion operator is declared by one of its two types, and both are another assembly's here, so there is
// no type of this package to declare it in; the cast the converter emits for a named target bound nothing
// (CS0030). The pair is recorded and go2cs-gen writes one function for it in this package's class, named as Go
// names the target; the site calls it: `libꓸPitch(o)`. A second source type overloads the same name.
//
// Every place the conversion can be written is here once: bound to a variable, as the base of a selector and of
// a method call, as an argument, a result, a field, an element, a map value and key, a package variable, a
// deferred and a go-statement argument, a send, a switch tag, an operand of ==, and inside a function literal.
// The operand is evaluated once. The conversion copies the value: a write to the copy does not reach the source,
// and a slice or pointer field still shares its target. A type written over another named type keeps its own
// conversion.
package main

import (
	"fmt"

	"ForeignPairStructConversion/lib"
	"ForeignPairStructConversion/other"
)

// P is this package's name for lib's type: the same pair, the same function.
type P = lib.Pitch

type holder struct {
	O other.Pitch
	L lib.Pitch
}

var calls int

func next() other.Pitch {
	calls++
	return other.Pitch{Hz: 100 + calls}
}

// A package variable's initializer converts before main runs.
var concert = lib.Pitch(other.Pitch{Hz: 440})

func double(p lib.Pitch) int { return p.Double() }

func octave(o other.Pitch) int { return o.Octave() }

func result(o other.Pitch) lib.Pitch { return lib.Pitch(o) }

func back(p lib.Pitch) other.Pitch { return other.Pitch(p) }

func (h holder) converted() lib.Pitch { return lib.Pitch(h.O) }

func report(label string, p lib.Pitch) { fmt.Println(label, p.Hz) }

func main() {
	o := other.Pitch{Hz: 220}
	p := lib.Pitch{Hz: 330}

	// Bound to a variable, each way.
	lp := lib.Pitch(o)
	op := other.Pitch(p)
	fmt.Println("bound:", lp.Hz, op.Hz)
	fmt.Printf("types: %T %T\n", lp, op)

	// The base of a selector and of a method call: the method is the target type's.
	fmt.Println("selector:", lib.Pitch(o).Hz, other.Pitch(p).Hz)
	fmt.Println("method:", lib.Pitch(o).Double(), other.Pitch(p).Octave())

	// An argument, a result, a method's result.
	fmt.Println("argument:", double(lib.Pitch(o)), octave(other.Pitch(p)))
	fmt.Println("result:", result(o).Hz, back(p).Hz)
	fmt.Println("receiver:", holder{O: o}.converted().Hz)

	// Two types of ONE other package.
	twin := lib.Twin(p)
	fmt.Println("same package:", twin.Hz, twin.Half(), lib.Pitch(twin).Double())

	// Three source types, one target: the function is overloaded.
	fmt.Println("overloads:", lib.Pitch(o).Hz, lib.Pitch(twin).Hz, lib.Pitch(other.Tagged{Hz: 55}).Hz)
	fmt.Println("tag ignored:", other.Tagged(o).Hz, other.Pitch(other.Tagged{Hz: 56}).Hz)

	// There and back.
	fmt.Println("round trip:", other.Pitch(lib.Pitch(o)) == o, lib.Pitch(other.Pitch(p)) == p)

	// The alias names lib's type.
	var aliased P = P(o)
	fmt.Println("alias:", aliased.Hz, P(other.Pitch{Hz: 5}).Double())

	// The package variable.
	fmt.Println("package variable:", concert.Hz)

	// A field, an element, a map value and a map key.
	h := holder{O: o, L: lib.Pitch(o)}
	s := []lib.Pitch{lib.Pitch(o), lib.Pitch(h.O)}
	a := [2]other.Pitch{other.Pitch(p), other.Pitch(s[0])}
	m := map[string]lib.Pitch{"a": lib.Pitch(o)}
	k := map[lib.Pitch]string{lib.Pitch(o): "key"}
	fmt.Println("field:", h.L.Hz)
	fmt.Println("elements:", s[0].Hz, s[1].Hz, a[0].Hz, a[1].Hz)
	fmt.Println("map:", m["a"].Hz, k[lib.Pitch{Hz: 220}], len(k))

	// Operands of every shape: a field, an element, a dereference, a call, a literal.
	po := &o
	fmt.Println("operands:", lib.Pitch(h.O).Hz, lib.Pitch(a[0]).Hz, lib.Pitch(*po).Hz, lib.Pitch(other.Low()).Hz, other.Pitch(lib.Middle()).Hz)

	// The operand is evaluated once.
	once := lib.Pitch(next())
	fmt.Println("once:", once.Hz, calls)
	fmt.Println("once more:", lib.Pitch(next()).Double(), calls)

	// The conversion copies: a write to the copy does not reach the source, nor the other way.
	copied := lib.Pitch(o)
	copied.Hz = 1
	o.Hz = 221
	fmt.Println("copy:", copied.Hz, o.Hz)
	o.Hz = 220

	// Several fields: a nested struct is copied, a slice and a pointer still share their target.
	ref := lib.Pitch{Hz: 7}
	on := other.Note{Name: "A", Pitch: lib.Pitch{Hz: 440}, Marks: []int{1, 2, 3}, Ref: &ref, Weight: 1.5}
	ln := lib.Note(on)
	ln.Name = "B"
	ln.Pitch.Hz = 441
	ln.Marks[0] = 9
	ln.Ref.Hz = 8
	fmt.Println("note:", ln.Name, ln.Pitch.Hz, ln.Marks, ln.Ref.Hz, ln.Weight)
	fmt.Println("source:", on.Name, on.Pitch.Hz, on.Marks, on.Ref.Hz, on.Weight)
	fmt.Println("note back:", other.Note(ln).Name, other.Note(ln).Ref == on.Ref)

	// No fields at all.
	fmt.Println("empty:", lib.Rest(other.Rest{}) == lib.Rest{}, other.Rest(lib.Rest{}) == other.Rest{})

	// A comparison and a switch.
	fmt.Println("equal:", lib.Pitch(o) == lib.Pitch{Hz: 220}, lib.Pitch(o) != p)

	switch lib.Pitch(o) {
	case p:
		fmt.Println("switch: p")
	case lib.Pitch{Hz: 220}:
		fmt.Println("switch: 220")
	default:
		fmt.Println("switch: none")
	}

	// A function literal, a deferred call, a goroutine and a channel.
	convert := func(v other.Pitch) lib.Pitch { return lib.Pitch(v) }
	fmt.Println("literal:", convert(o).Hz, convert(other.Pitch{Hz: 3}).Double())

	c := make(chan lib.Pitch, 2)
	c <- lib.Pitch(o)

	go func(v lib.Pitch) { c <- v }(lib.Pitch(other.Pitch{Hz: 12}))

	first, second := <-c, <-c
	fmt.Println("channel:", first.Hz, second.Hz)

	for i, v := range []other.Pitch{{Hz: 1}, {Hz: 2}} {
		fmt.Println("range:", i, lib.Pitch(v).Double())
	}

	// A type written over lib's type converts from its base as before, and to a third type through it.
	tag := other.Tag(p)
	fmt.Println("written over:", tag.Hz, tag.Triple(), lib.Pitch(tag).Hz)

	// The deferred call's argument is converted here, with the value o has now.
	defer report("deferred:", lib.Pitch(o))

	o.Hz = 0
	fmt.Println("last:", o.Hz)
}
