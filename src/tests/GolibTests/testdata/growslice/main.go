// growslice probe: the capacities Go's append and reflect.Value.Grow produce on the HEAP path.
//
// Run with optimizations off, so no append takes the compiler's stack backing store and every grow is
// runtime.growslice (nextslicecap, then roundupsize over the size classes):
//
//	go run -gcflags='-N -l' main.go > expected.txt
//
// GolibTests' GoGrowSliceTests replays every case through golib's append and compares; repoguard's
// TestGoGrowSliceExpectationsMatchTheToolchain re-runs this probe and fails when expected.txt drifts.
// Each line is a case name, then the capacities it observed, space-separated.
package main

import (
	"fmt"
	"reflect"
	"strings"
)

type s12 struct{ a, b, c int32 }
type s24 struct{ a, b, c int64 }
type p24 struct {
	s string
	n int64
}
type s40 struct{ a, b, c, d, e int64 }

var sink any

func emit(name string, caps []int) {
	parts := make([]string, len(caps))
	for i, c := range caps {
		parts[i] = fmt.Sprint(c)
	}
	fmt.Println(name, strings.Join(parts, " "))
}

// grows appends one element n times and records each capacity the slice takes.
func grows[T any](name string, n int) {
	var x []T
	var zero T
	var caps []int
	for range n {
		old := cap(x)
		x = append(x, zero)
		if cap(x) != old {
			caps = append(caps, cap(x))
		}
	}
	sink = x
	emit(name, caps)
}

// oneShot appends n elements at once to a nil slice, for each n, and records the capacity.
func oneShot[T any](name string, ns []int) {
	var caps []int
	for _, n := range ns {
		x := append([]T(nil), make([]T, n)...)
		sink = x
		caps = append(caps, cap(x))
	}
	emit(name, caps)
}

// reflectGrow grows through reflect.Value.Grow one element past the capacity each time.
func reflectGrow[T any](name string, n int) {
	var s []T
	v := reflect.ValueOf(&s).Elem()
	var caps []int
	for range n {
		v.SetLen(v.Cap())
		v.Grow(1)
		caps = append(caps, v.Cap())
	}
	sink = s
	emit(name, caps)
}

func main() {
	grows[byte]("grow byte", 70000)
	grows[int32]("grow int32", 20000)
	grows[int64]("grow int64", 10000)
	grows[string]("grow string", 3000)
	grows[*int]("grow ptr", 5000)
	grows[s12]("grow s12", 3000)
	grows[s24]("grow s24", 3000)
	grows[p24]("grow p24", 3000)
	grows[s40]("grow s40", 2000)
	grows[struct{}]("grow empty", 20)

	// Unnamed arrays: one golib type serves every length ([2]uint64 and [3]uint64 are both array<ulong>), so
	// golib sizes each growth from the appended value; a zero-length array is zero-size, so cap = len.
	grows[[3]byte]("grow arr3byte", 20)
	grows[[2]uint64]("grow arr2u64", 600)
	grows[[2][3]byte]("grow arr2x3byte", 2000)
	grows[[3]*int]("grow arr3ptr", 3000)
	grows[[0]int]("grow arr0int", 20)
	grows[[0][3]int]("grow arr0x3int", 10)

	var ns []int
	for n := 1; n <= 64; n++ {
		ns = append(ns, n)
	}
	ns = append(ns, 100, 255, 256, 257, 511, 512, 513, 1000, 1023, 1024, 1025, 4000, 4097, 8191, 8192, 8193,
		32760, 32761, 32768, 32769, 40000, 65536, 100000)
	oneShot[byte]("oneshot byte", ns)
	oneShot[int64]("oneshot int64", ns)
	oneShot[string]("oneshot string", ns)
	oneShot[s24]("oneshot s24", ns)
	oneShot[p24]("oneshot p24", ns)

	// Several elements per append: append(x, 1, 2, 3), and a five-element spread.
	var m []int64
	var mcaps []int
	for range 400 {
		old := cap(m)
		m = append(m, 1, 2, 3)
		if cap(m) != old {
			mcaps = append(mcaps, cap(m))
		}
	}
	sink = m
	emit("multi int64x3", mcaps)

	var b []byte
	var bcaps []int
	five := []byte{1, 2, 3, 4, 5}
	for range 800 {
		old := cap(b)
		b = append(b, five...)
		if cap(b) != old {
			bcaps = append(bcaps, cap(b))
		}
	}
	sink = b
	emit("spread byte+5", bcaps)

	// A grow past double the capacity takes the needed length, then rounds: 5 of 5, plus 20.
	full := make([]int64, 5, 5)
	full = append(full, make([]int64, 20)...)
	sink = full
	emit("pastdouble int64", []int{cap(full)})

	reflectGrow[byte]("reflect byte", 24)
	reflectGrow[int64]("reflect int64", 24)
}
