// Guards the array pointer doors over an array whose ELEMENT is a ccgo union: a struct holding a [0]T carrier for
// each member but the one that has storage (sqlite's TExpr and TRtreeCoord, libc's Taddress and its sockaddr union;
// docs/phase4/DESIGN-array-pointer-doors.md, 16.11). The doors asked whether the element is managed with the plain
// reading, where every Go array is an array<T> reference, so a union read as managed and the door declined: the old
// box route read the whole array as a value and answered length 0 over memory that is no Go array of that type. A
// struct admitted to inline arrays holds a [0]T as an alignment carrier, so the doors now read admission.
//
// Each arm runs under a recover that prints a panic, so a wrong answer is a line that differs from Go's and the arms
// after it still run. Arm F, over a Go array of the same type, was right before and must stay right.

package main

import (
	"fmt"
	"unsafe"
)

type Tu32 = uint32

// ccgo's union shape: a [0]T carrier for each other member, then the one member that has storage.
type TRtreeCoord = struct {
	Fi [0]int32
	Fu [0]Tu32
	Ff float32
}

// A struct holding unions as fields (sqlite's TExpr shape, cut down).
type TExpr = struct {
	Fop uint8
	Fu  struct {
		FiValue [0]int32
		FzToken uintptr
	}
	Fw struct {
		FiOfst [0]int32
		FiJoin int32
	}
}

// A union whose storage member is itself an array (libc's sockaddr union shape).
type TAddr = struct {
	Fa [0]uint64
	Fb [16]byte
}

func arm(name string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "panic:", r)
		}
	}()
	f()
}

func readCoord(p unsafe.Pointer, i int) float32 {
	return (*[4]TRtreeCoord)(p)[i].Ff
}

func writeCoord(p unsafe.Pointer, i int, f float32) {
	(*[4]TRtreeCoord)(p)[i].Ff = f
}

func joinOf(p unsafe.Pointer, i int) int32 {
	return (*[3]TExpr)(p)[i].Fw.FiJoin
}

func setJoin(p unsafe.Pointer, i int, j int32) {
	(*[3]TExpr)(p)[i].Fw.FiJoin = j
}

func main() {
	// A: a store and a read of a union element, over words that are no Go array of unions (libc's heap is not).
	arm("A", func() {
		var raw [2]uint64
		p := unsafe.Pointer(&raw)
		writeCoord(p, 3, 2.25)
		fmt.Printf("A %v %#x\n", readCoord(p, 3), raw[1]>>32)
	})

	// B: a read of bits Go stored, at element 0.
	arm("B", func() {
		var raw [2]uint64
		raw[0] = 0x3fc00000 // 1.5 as float32 bits
		fmt.Println("B", readCoord(unsafe.Pointer(&raw), 0))
	})

	// C: a union nested in the element, stored and read through the view.
	arm("C", func() {
		var words [9]uint64 // three 24-byte TExpr
		q := unsafe.Pointer(&words)
		setJoin(q, 2, 9)
		fmt.Println("C", joinOf(q, 2), words[8]&0xffffffff)
	})

	// D: the nested union read after Go stored its word.
	arm("D", func() {
		var words [9]uint64
		words[5] = 7 // element 1's Fw.FiJoin, at byte 40
		fmt.Println("D", joinOf(unsafe.Pointer(&words), 1), unsafe.Sizeof(TExpr{}))
	})

	// E: a union whose storage is a nonzero array, written by index through the view.
	arm("E", func() {
		var words [4]uint64 // two 16-byte TAddr
		(*[2]TAddr)(unsafe.Pointer(&words))[1].Fb[3] = 0x5a
		fmt.Printf("E %#x %d\n", words[2], unsafe.Sizeof(TAddr{}))
	})

	// F: a Go array of the same type, right on the old route, must stay right.
	arm("F", func() {
		var arr [4]TRtreeCoord
		writeCoord(unsafe.Pointer(&arr), 1, 4.5)
		fmt.Println("F", arr[1].Ff, readCoord(unsafe.Pointer(&arr), 1))
	})

	// G: the open slice of a union array over words, written and read back.
	arm("G", func() {
		var raw [2]uint64
		s := (*[4]TRtreeCoord)(unsafe.Pointer(&raw))[1:]
		s[1].Ff = 3.5
		fmt.Printf("G %d %d %#x\n", len(s), cap(s), raw[1]&0xffffffff)
	})
}
