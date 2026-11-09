package main

import (
	"fmt"

	"KeyedLiteralNamedForeignStruct/guid"
)

// A keyed literal of a defined type written over a named struct, from OUTSIDE the package that
// declares it (go-winio internal/socket's `guid.GUID{Data1: ...}`).
var id = guid.GUID{
	Data1: 0x25a207b9,
	Data2: 0xddf3,
	Data3: 0x4660,
	Data4: [8]byte{0x8e, 0xe9, 0x76, 0xe5, 0x8c, 0x74, 0x06, 0x3e},
}

func main() {
	fmt.Println(id.Data1, id.Data2, id.Data3, id.Data4[1], id.First())

	// A partial keyed literal: omitted fields keep their zero value.
	partial := guid.GUID{Data2: 9}
	fmt.Println(partial.Data1, partial.Data2, partial.Data4[7])

	// Through a pointer, and as elided slice elements.
	p := &guid.GUID{Data1: 5}
	p.Data2 = 6
	fmt.Println(p.Data1, p.Data2, p.First())

	list := []guid.GUID{{Data1: 10}, {Data3: 11}}
	fmt.Println(len(list), list[0].Data1, list[1].Data3)

	ptrs := []*guid.GUID{{Data1: 12}}
	ptrs[0].Data2 = 13
	fmt.Println(len(ptrs), ptrs[0].Data1, ptrs[0].Data2, ptrs[0].First())

	// A literal is a fresh value: changing the copy leaves the original alone.
	copied := id
	copied.Data1 = 1
	fmt.Println(id.Data1 == 0x25a207b9, copied.Data1)

	// The same shape over the declaring package's UNEXPORTED struct.
	o := guid.Outer{A: 7, B: [2]byte{1, 2}}
	fmt.Println(o.A, o.B[1], o.Sum())

	vals, refs := guid.Pair()
	fmt.Println(len(vals), vals[0].A, vals[1].B[0], vals[1].Sum(), refs[0].A)

	// EMPTY literals need no fields, so they read the zero value from outside the declaring
	// package and inside it alike.
	empty := guid.GUID{}
	emptyPtr := &guid.GUID{}
	emptyList := []guid.GUID{{}, {Data1: 1}}
	emptyPtrs := []*guid.GUID{{}}
	fmt.Println(empty.Data1, emptyPtr.Data4[0], len(emptyList), emptyList[0].Data2, emptyList[1].Data1, emptyPtrs[0].Data3)

	zero, zeros, zeroPtrs := guid.Zeros()
	fmt.Println(zero.A, len(zeros), zeros[0].Sum(), zeroPtrs[0].B[1])
}
