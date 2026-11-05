package main

import (
	"fmt"
	"os"
	"runtime"
)

// A compound shift whose target is a map index follows Go's shift rule like any other target: a count at or past
// the element's width gives 0 (or the sign fill for a negative signed value), and a negative signed count panics
// with the runtime.Error "negative shift amount". C#'s native shift masks the count to the element's promoted
// width, so `m[1] >>= 40` on a uint32 element would read as `>> 8`. Each arm runs under recover and prints its
// class, so a divergence names it; an argument runs only the arm of that name.

var (
	wide  uint  = 40
	nine  uint  = 9
	neg   int   = -1
	big   int64 = 1 << 40
	minus int64 = -8
	calls int
)

func key() int {
	calls++
	return 1
}

func arm(class string, f func() string) {
	if len(os.Args) > 1 && os.Args[1] != class {
		return
	}

	defer func() {
		if r := recover(); r != nil {
			_, isRuntimeError := r.(runtime.Error)
			fmt.Println(class+": panic:", r, isRuntimeError)
		}
	}()

	fmt.Println(class+":", f())
}

func shl[T ~uint8 | ~uint32](x T, n uint) T {
	x <<= n
	return x
}

func shrSigned[T ~int8 | ~int64](x T, n int) T {
	return x >> n
}

func shrIndex[T ~uint8 | ~uint32](m map[int]T, n uint) T {
	m[1] >>= n
	return m[1]
}

func main() {
	arm("uint32 const right", func() string {
		m := map[int]uint32{1: 0xFFFFFFFF}
		m[1] >>= 40
		return fmt.Sprint(m[1])
	})
	arm("uint32 const left", func() string {
		m := map[int]uint32{1: 0xFF}
		m[1] <<= 40
		return fmt.Sprint(m[1])
	})
	arm("uint32 const in range", func() string {
		m := map[int]uint32{1: 0xFF00}
		m[1] >>= 8
		return fmt.Sprint(m[1])
	})
	arm("uint32 unbounded right", func() string {
		m := map[int]uint32{1: 0xFFFFFFFF}
		m[1] >>= wide
		return fmt.Sprint(m[1])
	})
	arm("uint8 const left", func() string {
		m := map[int]uint8{1: 0xFF}
		m[1] <<= 33
		return fmt.Sprint(m[1])
	})
	arm("uint8 unbounded right", func() string {
		m := map[int]uint8{1: 0xFF}
		m[1] >>= wide
		return fmt.Sprint(m[1])
	})
	arm("uint8 unbounded in range", func() string {
		m := map[int]uint8{1: 0xFF}
		m[1] >>= 4
		m[1] <<= nine - 6
		return fmt.Sprint(m[1])
	})
	arm("int64 unbounded right", func() string {
		m := map[string]int64{"a": big, "b": minus}
		m["a"] >>= wide + 30
		m["b"] >>= wide + 30
		return fmt.Sprint(m["a"], " ", m["b"])
	})
	arm("missing key", func() string {
		m := map[int]uint32{}
		m[7] <<= wide
		v, ok := m[7]
		return fmt.Sprint(v, " ", ok)
	})
	arm("key evaluated once", func() string {
		m := map[int]uint32{1: 0xFFFFFFFF}
		calls = 0
		m[key()] >>= wide
		return fmt.Sprint(m[1], " ", calls)
	})
	arm("negative count", func() string {
		m := map[int]uint32{1: 0xFF}
		m[1] <<= neg
		return fmt.Sprint(m[1])
	})
	arm("type parameter", func() string {
		return fmt.Sprint(shl(uint8(0xFF), nine), " ", shl(uint32(0xFF), wide), " ", shl(uint8(1), 3))
	})
	arm("type parameter signed", func() string {
		return fmt.Sprint(shrSigned(int8(-8), int(nine)), " ", shrSigned(int64(big), int(wide)+30), " ", shrSigned(int8(64), 3))
	})
	arm("type parameter negative count", func() string {
		return fmt.Sprint(shrSigned(int8(1), neg))
	})
	arm("type parameter map", func() string {
		return fmt.Sprint(shrIndex(map[int]uint8{1: 0xFF}, nine), " ", shrIndex(map[int]uint32{1: 0xFFFFFFFF}, wide))
	})
}
