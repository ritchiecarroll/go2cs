package main

import (
	"fmt"
	"math/bits"
	"sort"
)

// A type whose methods are named like a built-in, like an imported package and like two functions of
// the package. Go reads each bare name as the built-in, the import or the function; a method is
// reached only through a value.
type words []uint64

func (w words) Len() int           { return len(w) }
func (w words) Less(i, j int) bool { return w[i] < w[j] }
func (w words) Swap(i, j int)      { w[i], w[j] = w[j], w[i] }

func (w words) cap() int  { return cap(w) + 1 }
func (w words) bits() int { return bits.Len64(w[0]) }

// Sort sorts w through the package's function of the same name.
func (w words) Sort() { Sort(w) }

func (w words) Run() int { return len(w) }

type gauge struct{ n int }

// size reads the built-in and the import from a type that declares neither name.
func (g gauge) size(w words) int { return cap(w) + bits.Len64(uint64(g.n)) }

func Sort(data sort.Interface) { sort.Sort(data) }

func Run() int { return 4 }

func others() {
	w := words{3, 1, 2}
	w.Sort()

	run := Run
	var boxed any = Run
	_, isFunc := boxed.(func() int)

	fmt.Println("others", w, w.cap(), cap(w), w.bits(), bits.Len64(w[0]), gauge{n: 5}.size(w), run(), isFunc, w.Run())
}
