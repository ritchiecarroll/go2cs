// Go's own layout for the shapes the [InlineArray] probe reproduces (GOARCH=amd64).
package main

import (
	"fmt"
	"unsafe"
)

type S22 struct {
	FforceAlignment [0]int16
	Fa              [201]int8
	F__ccgo_pad2    [1]byte
}

type Outer struct {
	B byte
	S S22
}

type TrailElem struct {
	A byte
	_ [0][2]int64
}

type MidZero struct {
	A byte
	Z [0]int32
	B byte
}

func main() {
	var o Outer
	fmt.Printf("S22 size %d align %d; Fa@%d pad@%d\n", unsafe.Sizeof(S22{}), unsafe.Alignof(S22{}), unsafe.Offsetof(S22{}.Fa), unsafe.Offsetof(S22{}.F__ccgo_pad2))
	fmt.Printf("Outer{B byte; S S22}: S@%d size %d\n", unsafe.Offsetof(o.S), unsafe.Sizeof(o))
	fmt.Printf("[2]S22 size %d\n", unsafe.Sizeof([2]S22{}))
	fmt.Printf("TrailElem{A byte; _ [0][2]int64}: size %d align %d\n", unsafe.Sizeof(TrailElem{}), unsafe.Alignof(TrailElem{}))
	var m MidZero
	fmt.Printf("MidZero{A byte; Z [0]int32; B byte}: Z@%d B@%d size %d align %d\n", unsafe.Offsetof(m.Z), unsafe.Offsetof(m.B), unsafe.Sizeof(m), unsafe.Alignof(m))
}
