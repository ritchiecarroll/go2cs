// An unsafe.Pointer returned from a call keeps its referent alive, as any unsafe.Pointer does. Each round takes three
// pointers into a slice's storage, each from a call, makes the collector run, and then writes through them. A write
// that does not reach the slice is counted, and a count that is not zero fails the run.
//
// Each pointer is minted in its own function, so nothing in the checking frame holds the storage still by accident.
package main

import (
	"fmt"
	"os"
	"runtime"
	"unsafe"
)

var sink [][]byte

// added returns a pointer built with unsafe.Add in its return statement.
//
//go:noinline
func added(b []byte) unsafe.Pointer { return unsafe.Add(unsafe.Pointer(&b[0]), 4) }

// held builds the same kind of pointer into a variable first.
//
//go:noinline
func held(b []byte) unsafe.Pointer {
	p := unsafe.Add(unsafe.Pointer(&b[0]), 8)
	return p
}

// same returns the pointer it was handed.
//
//go:noinline
func same(p unsafe.Pointer) unsafe.Pointer { return p }

// own returns what a function returning its own pointer answered.
//
//go:noinline
func own(b []byte) unsafe.Pointer { return same(unsafe.Pointer(&b[0])) }

// collect allocates enough that the slice's neighbours die, and runs the collector twice.
func collect() {
	for i := 0; i < 200000; i++ {
		sink = append(sink, make([]byte, 128))
	}

	sink = nil
	runtime.GC()
	runtime.GC()
}

func main() {
	const rounds = 20
	var viaReturn, viaVariable, ownPointer int

	for round := 0; round < rounds; round++ {
		b := make([]byte, 64)
		p := added(b)
		q := held(b)
		r := own(b)

		collect()

		*(*byte)(p) = 1
		*(*byte)(q) = 2
		*(*byte)(r) = 3

		if b[4] != 1 {
			viaReturn++
		}

		if b[8] != 2 {
			viaVariable++
		}

		if b[0] != 3 {
			ownPointer++
		}
	}

	fmt.Println("unsafe.Add in a return: wrong", viaReturn, "of", rounds)
	fmt.Println("unsafe.Add into a variable: wrong", viaVariable, "of", rounds)
	fmt.Println("a function returning its own pointer: wrong", ownPointer, "of", rounds)

	if viaReturn+viaVariable+ownPointer != 0 {
		os.Exit(1)
	}
}
