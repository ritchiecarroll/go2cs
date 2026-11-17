// Guards the open slice of an UNNAMED array through a pointer conversion: `(*[N]T)(p)[lo:]`, p an
// unsafe.Pointer, read, written, ranged, passed and copied. The array pointer doors took this form for a named
// array only (docs/phase4/DESIGN-array-pointer-doors.md, decision D4). An unnamed array kept the box of the whole
// array, read as a value: over memory that is no Go array of that type it answered length 0 where Go slices, and
// as copy's destination over a number's bytes (reflect's nameOff copy, arm C) it ended the process.
//
// Each arm runs under a recover that prints a panic, so a wrong answer is a line that differs from Go's and the
// arms after it still run. Arm E, over a Go array of the same type, was right before and must stay right.

package main

import (
	"fmt"
	"unsafe"
)

func arm(name string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "panic:", r)
		}
	}()
	f()
}

func takes(b []byte) int {
	n := 0
	for _, x := range b {
		n += int(x)
	}
	return n + 100*len(b)
}

func main() {
	// A: an anonymous struct element, the open slice from 1 (W17's program wrote it closed).
	arm("A", func() {
		var _ [3]struct {
			lo uint32
			hi uint32
		}
		var buf [3]uint64
		bp := unsafe.Pointer(&buf)
		tail := (*[3]struct {
			lo uint32
			hi uint32
		})(bp)[1:]
		tail[0].hi = 7
		fmt.Println("A", len(tail), cap(tail), buf[1]>>32)
	})

	// B: an unnamed array over a Go array of another element type (the 11-14 row's Q).
	arm("B", func() {
		var arr [4]uint32
		w := (*[16]byte)(unsafe.Pointer(&arr))[:]
		w[4] = 1
		fmt.Println("B", len(w), arr)
	})

	// C: reflect's nameOff shape, copy into the bytes of a number from the bytes of another value.
	arm("C", func() {
		var nameOff int32
		src := [8]byte{0, 1, 2, 3, 4, 5, 6, 7}
		off := unsafe.Pointer(&src[2])
		n := copy((*[4]byte)(unsafe.Pointer(&nameOff))[:], (*[4]byte)(off)[:])
		fmt.Println("C", n, nameOff)
	})

	// D: the open slice of a number's bytes as an argument (the registry shape).
	arm("D", func() {
		u := uint32(0x01020304)
		fmt.Println("D", takes((*[4]byte)(unsafe.Pointer(&u))[:]))
	})

	// E: over a Go array of the same type, from 1, bound and written (works on the old route).
	arm("E", func() {
		var a [4]byte
		s := (*[4]byte)(unsafe.Pointer(&a))[1:]
		s[0] = 9
		fmt.Println("E", len(s), cap(s), a)
	})

	// F: a range by value over the open slice of a number's bytes.
	arm("F", func() {
		u := uint64(0x0807060504030201)
		sum := 0
		for _, b := range (*[8]byte)(unsafe.Pointer(&u))[2:] {
			sum += int(b)
		}
		fmt.Println("F", sum)
	})

	// G: the open slice from lo of a Go array's bytes, stored through and read back through the array.
	arm("G", func() {
		var words [2]uint16
		b := (*[4]byte)(unsafe.Pointer(&words))[2:]
		b[0], b[1] = 0x34, 0x12
		fmt.Printf("G %d %#x\n", len(b), words[1])
	})
}
