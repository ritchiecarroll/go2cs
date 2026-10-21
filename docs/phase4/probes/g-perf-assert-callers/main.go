// perfrev: the two G-area items of the adversarial perf review, as plain loops (same program in Go and converted C#).
//  1. runtime.Callers capturing 8 frames.
//  2. type assertion of an `any` holding a pointer (*T, a raw ж box in C#) or a value, to its concrete type and to an interface.
package main

import (
	"fmt"
	"os"
	"runtime"
	"strconv"
	"time"
)

type T struct{ n int }

func (t *T) Len() int { return t.n }

type V struct{ n int }

func (v V) Len() int { return v.n }

type lener interface{ Len() int }

var sinkInt int
var xp, xv any

//go:noinline
func assertPT(x any) int {
	if p, ok := x.(*T); ok {
		return p.n
	}
	return 0
}

//go:noinline
func assertPI(x any) int {
	if l, ok := x.(lener); ok {
		return l.Len()
	}
	return 0
}

//go:noinline
func assertVT(x any) int {
	if v, ok := x.(V); ok {
		return v.n
	}
	return 0
}

var pcs [8]uintptr

//go:noinline
func deep(d int) int {
	if d == 0 {
		return runtime.Callers(1, pcs[:])
	}
	return deep(d-1) + 0
}

func measure(name string, n int, f func(int) int) {
	f(n / 10) // warm
	best := time.Duration(1 << 62)
	for r := 0; r < 5; r++ {
		start := time.Now()
		sinkInt += f(n)
		if el := time.Since(start); el < best {
			best = el
		}
	}
	fmt.Printf("%-40s %8.1f ns/op\n", name, float64(best.Nanoseconds())/float64(n))
}

func main() {
	n := 2_000_000
	if len(os.Args) > 1 {
		n, _ = strconv.Atoi(os.Args[1])
	}
	xp, xv = &T{n: 1}, V{n: 2}

	measure("Callers depth 8 (frames captured)", n/20, func(k int) int {
		s := 0
		for i := 0; i < k; i++ {
			s += deep(8)
		}
		return s
	})
	measure("any(*T).(*T)", n, func(k int) int {
		s := 0
		for i := 0; i < k; i++ {
			s += assertPT(xp)
		}
		return s
	})
	measure("any(*T).(lener)", n, func(k int) int {
		s := 0
		for i := 0; i < k; i++ {
			s += assertPI(xp)
		}
		return s
	})
	measure("any(V).(V)", n, func(k int) int {
		s := 0
		for i := 0; i < k; i++ {
			s += assertVT(xv)
		}
		return s
	})
	measure("any(V).(lener)", n, func(k int) int {
		s := 0
		for i := 0; i < k; i++ {
			s += assertPI(xv)
		}
		return s
	})
	fmt.Println("frames:", deep(8), "sink", sinkInt != 0)
}
