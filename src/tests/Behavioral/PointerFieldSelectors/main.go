// PointerFieldSelectors guards Go's field selector through a pointer, `p.f`, which the converter emits as Go
// spells it (parity seat #31). The site binds a ref property go2cs-gen emits on the box for each type the file's
// [GoFieldAccess] record lists; it used to read `(~p).f`, a copy of the whole struct, and write `p.Value.f`.
//
// Each arm is a shape the property has to serve or has to leave alone, and each prints values Go computes, so a
// read of a stale copy, a write that landed nowhere or a property bound to the wrong member cannot print the
// expected lines:
//   - a read, a store, a compound assignment and an increment through a local pointer, and an aliasing pointer;
//   - a field promoted through a value embed (`o.Z`): it keeps the box form, since the site would have to name the
//     embed; and a struct-typed field read as a copy (`in := o.inner`);
//   - the chain `c.prev.prev.next.x = …` and a parallel swap of two fields;
//   - a field named like a member of ж<T> (`Value`): it keeps the explicit box spelling;
//   - abi.ArrayType's shape: a field `Len` that shadows the `Len` method its embedded type promotes;
//   - a generic struct read and written inside a generic function;
//   - pointers held in a map, a slice and a call result, and the address of a field beside the property;
//   - a NATIVE-backed box: a struct with an array field, whose CLR layout is not Go's, laid over a byte buffer and
//     reached through another struct's field. The property answers each field at its Go offset; the box form's
//     whole-value read refused it;
//   - a nil pointer, read and written: Go's own panic text.
//
// The nil read also prints the names of the Go frames between the panic and main (runtime frames omitted): a getter
// frame the JIT keeps (a Debug build) must not appear among them. Names only: a recovered panic's frame LINES are a
// separate matter, the same with or without the property.
package main

import (
	"fmt"
	"runtime"
	"strings"
	"unsafe"
)

type point struct {
	X, Y int
}

type node struct {
	x          int
	prev, next *node
}

// A field named like a member of ж<T>.
type boxed struct {
	Value int
	label string
}

type base struct {
	n int
}

func (b *base) Len() int { return b.n }

// The field Len shadows the Len method base promotes.
type sized struct {
	base
	Len int
}

type pair[T any] struct {
	First T
	Count int
}

func bump[T any](p *pair[T], v T) T {
	p.First = v
	p.Count++
	return p.First
}

type inner struct {
	Z int
}

type outer struct {
	inner
	W int
}

// Go offsets: Tag 0, Size 2, Body 4, Flag 8. The array is a managed reference in the CLR's layout.
type wire struct {
	Tag  uint16
	Size uint16
	Body [4]byte
	Flag uint32
}

type holder struct {
	w *wire
}

func mk(x int) *point { return &point{x, x + 1} }

func goFrames() string {
	pcs := make([]uintptr, 32)
	frames := runtime.CallersFrames(pcs[:runtime.Callers(2, pcs)])
	var names []string

	for {
		frame, more := frames.Next()

		if !strings.HasPrefix(frame.Function, "runtime.") {
			names = append(names, frame.Function)
		}

		if !more || frame.Function == "main.main" {
			break
		}
	}

	return strings.Join(names, " ")
}

func main() {
	p := &point{1, 2}
	p.X = 10
	p.Y += 5
	p.X++
	q := p
	q.Y = p.X * 2
	fmt.Println("point:", p.X, p.Y, *p)

	o := &outer{inner{3}, 4}
	o.Z = 7
	in := o.inner
	in.Z = 99
	fmt.Println("outer:", o.Z, o.W, in.Z, o.inner.Z)

	a, b, c := &node{x: 1}, &node{x: 2}, &node{x: 3}
	a.next, b.prev, b.next, c.prev = b, a, c, b
	c.prev.prev.next.x = 20
	fmt.Println("chain:", a.next.x, b.x, c.prev.x)

	p.X, p.Y = p.Y, p.X
	fmt.Println("swap:", p.X, p.Y)

	a.next, a.prev = a.prev, a.next
	fmt.Println("swap pointers:", a.next == nil, a.prev.x)

	v := &boxed{Value: 5, label: "x"}
	v.Value += 1
	v.label += "y"
	fmt.Println("collision:", v.Value, v.label)

	s := &sized{base{3}, 9}
	s.Len++
	fmt.Println("shadow:", s.Len, s.base.Len())

	g := &pair[string]{First: "a"}
	fmt.Println("generic:", bump(g, "b"), g.First, g.Count)

	m := map[string]*point{"k": {1, 1}}
	m["k"].X = 4
	ps := []*point{{2, 2}}
	ps[0].Y *= 3
	fmt.Println("containers:", m["k"].X, ps[0].Y, mk(5).Y)

	px := &p.X
	*px = 42
	fmt.Println("address:", p.X)

	buf := make([]byte, 16)
	h := holder{w: (*wire)(unsafe.Pointer(&buf[0]))}
	h.w.Tag = 0x0102
	h.w.Size = 3
	h.w.Flag = 0x0a0b0c0d
	h.w.Flag++
	fmt.Println("native bytes:", buf[:12])
	fmt.Println("native fields:", h.w.Tag, h.w.Size, h.w.Flag)

	func() {
		defer func() { fmt.Println("nil read:", recover(), goFrames()) }()

		var np *point
		fmt.Println(np.X)
	}()

	func() {
		defer func() { fmt.Println("nil write:", recover()) }()

		var nn *node
		nn.x = 1
	}()
}
