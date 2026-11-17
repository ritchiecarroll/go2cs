// Guards Go arrays held INLINE in a struct the converter admits (I0, docs/phase4/DESIGN-array-value-storage.md 7.7):
// each struct below is converted whole through unsafe.Pointer, so its arrays are go.arrays members laid out at Go's
// offsets, and a [0]T is an alignment carrier. A view over raw bytes reads and writes Go's own bytes; len, cap, both
// ranges, an array literal (keyed too) and fmt of a boxed array all read as Go's. An inline array boxed into an
// interface crosses as a copy in the one representation every Go [N]T has there, nested arrays deep: a type switch,
// interface ==, a map key and reflect see it, and a later write to the field does not reach the box.
package main

import (
	"fmt"
	"reflect"
	"unsafe"
)

type Plain struct{ N int32 }

type Holder struct {
	P   Plain
	Buf [8]byte
}

type S22 struct {
	FforceAlignment [0]int16
	Fa              [5]int8
	Fpad            [1]byte
}

type Named struct {
	FnName  uint8
	FzName  [7]int8
	FrLimit float32
}

type Grid struct {
	Cells [2][3]int16
	Off   [4]uint8
}

func view(p unsafe.Pointer) *Holder { return (*Holder)(p) }

func s22(p unsafe.Pointer) *S22 { return (*S22)(p) }

func named(p unsafe.Pointer) *Named { return (*Named)(p) }

func grid(p unsafe.Pointer) *Grid { return (*Grid)(p) }

func box(v any) any { return v }

func main() {
	var raw [16]byte
	for i := range raw {
		raw[i] = byte(i + 1)
	}
	h := view(unsafe.Pointer(&raw[0]))
	n := len(h.Buf) + cap(h.Buf)
	for i := range h.Buf {
		h.Buf[i] += byte(i)
	}
	for i, b := range h.Buf {
		n += i * int(b)
	}
	h.Buf[2] += 3
	fmt.Println(n, h.P.N, h.Buf[0], h.Buf[7], raw[6], raw[11])
	fmt.Println(h.Buf)
	fmt.Printf("%v %d\n", h.Buf, unsafe.Sizeof(*h))

	s := s22(unsafe.Pointer(&raw[0]))
	s.Fa[1] = -2
	fmt.Println(unsafe.Sizeof(*s), unsafe.Alignof(*s), s.Fa[0], s.Fa[1], raw[1])

	x := Named{FnName: 6, FzName: [7]int8{'s', 'e', 'c', 'o', 'n', 'd'}, FrLimit: 4.5}
	fmt.Println(x.FnName, x.FzName[0], x.FzName[5], x.FzName[6], x.FrLimit, unsafe.Sizeof(x))
	_ = named

	g := Grid{Cells: [2][3]int16{{1, 2, 3}, {4, 5, 6}}, Off: [4]uint8{1: 7, 9}}
	b := box(g.Off)
	g.Off[1] = 0
	fmt.Println(b, g.Off, box(g.Cells) == box([2][3]int16{{1, 2, 3}, {4, 5, 6}}))
	switch v := box(g.Cells[1]).(type) {
	case [3]int16:
		fmt.Println("[3]int16", v[0], v[2])
	default:
		fmt.Println("other", v)
	}
	m := map[any]int{box(g.Cells[0]): 1}
	fmt.Println(m[[3]int16{1, 2, 3}], reflect.ValueOf(g.Off).Cap(), reflect.ValueOf(g.Cells).Index(1).Len())
	fmt.Printf("%T %v\n", g.Cells, g.Cells[1])
	_ = grid
}
