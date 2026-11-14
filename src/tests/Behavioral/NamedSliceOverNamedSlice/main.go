package main

import "fmt"

// A defined type whose underlying type is another DEFINED slice type (`type fermat nat` over
// `type nat []uint64`) is a slice in Go: it is made, indexed, sliced, appended to, ranged over
// and len()'d like its base, and converts to and from it. The C# wrapper wrapped the base type
// and had no slice surface, so none of that compiled (CS1503, CS0021, CS0315).
// github.com/remyoudompheng/bigfft declares `type fermat nat` (sqlite-cs row S4).

type nat []uint64

type fermat nat

// A third hop down the same chain.
type limbs fermat

// Methods are not inherited through a defined type: each type has its own.
func (z nat) sum() (s uint64) {
	for _, w := range z {
		s += w
	}
	return
}

func (z fermat) norm() fermat {
	for i := range z {
		z[i] %= 10
	}
	return z
}

func main() {
	z := make(fermat, 3, 5)
	z[0] = 7
	z[2] = 19
	fmt.Println(len(z), cap(z), z[0], cap(z[:2]), z)

	n := nat(z[1:])
	f := fermat(n)
	fmt.Println(len(n), len(f), n.sum(), f.norm(), z)

	z = append(z, 4, 5)
	fmt.Println(len(z), cap(z), nat(z).sum())

	l := limbs(z[:2])
	l[1] = 8
	copy(z[3:], l)
	fmt.Println(len(l), z, fermat(l).norm())

	var nilF fermat
	fmt.Println(nilF == nil, len(nilF), fmt.Sprintf("%v %d", nat(nilF) == nil, len(append(nilF, 1))))

	for i, w := range z[:2] {
		fmt.Print(i, ":", w, " ")
	}
	fmt.Println()
}
