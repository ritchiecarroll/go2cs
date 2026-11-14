package main

import "fmt"

// A type the Go source declares is named as declared, even when its name is a spelling the
// converter otherwise maps to a builtin (`float` to float64): `float{n: n}` emitted
// `new float64(n: n)` (CS1739). The converter now decides from the go/types object, not the
// spelling. modernc.org/mathutil declares `type float struct` (sqlite-cs row S7).

type float struct {
	n, fracBits int
}

func (f *float) normalize() { f.fracBits++ }

func (f float) String() string { return fmt.Sprintf("float(%d,%d)", f.n, f.fracBits) }

func newFloat(n, fracBits int) float {
	f := float{n: n, fracBits: fracBits}
	f.normalize()
	return f
}

// A function-local type with a builtin's spelling keeps its own name too.
func half(v float64) any {
	type float float32
	return float(v) / 2
}

func main() {
	f := newFloat(3, 4)
	fmt.Println(f.n, f.fracBits)

	p := &float{1, 2}
	p.normalize()
	fs := []float{{5, 6}, {n: 7}}
	m := map[string]float{"a": {8, 9}}

	var x any = f
	g, ok := x.(float)
	var z float
	fmt.Println(*p, fs, m["a"], g, ok, z)
	fmt.Printf("%v %T\n", half(5), half(5))
}
