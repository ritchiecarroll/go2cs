// A NAMED fixed-size array type whose ELEMENT's zero value must be CONSTRUCTED, reached at its
// ZERO VALUE rather than through a composite literal.
//
// The named-array wrapper allocates its backing LAZILY, so every route to the zero value —
// `var x T`, `new(T)`, a struct field, a named result, an element of an array/slice of T, a map
// read — converges on that one allocation. When it was `new array<E>(N)` with no element factory,
// `default(E)` was the fill, and that is not the Go zero value for two element shapes:
//
//   - a nested UNNAMED array (nn below): the inner length lives only in the Go type — the emitted
//     descriptor is `[2]array<nint>` — so every element kept a LENGTH-ZERO backing. Measured at
//     master: `2 0 [[] []]` against Go's `2 3 [[0 0 0] [0 0 0]]`;
//   - a STRUCT whose own zero value needs construction (ns below, runtime's `semTable` shape):
//     `default` skips the generated constructor that runs the fixed-array field's initializer, and
//     the first inner index PANICKED.
//
// The two CONTROLS are as load-bearing as the two subjects, because they pin the boundary from the
// other side: a plain element (nb) and a NAMED element (no) must keep the bare-length backing — a
// named element's own wrapper allocates its own backing by this very route, so constructing it here
// would be both redundant and unspellable. Both were already correct at master and must stay so.
//
// Sibling: CompositeLiteralElements guards the same element predicate reached through a composite
// LITERAL. This one guards the zero value, which is where all five of the standard library's needy
// named arrays are actually built.
package main

import "fmt"

// SUBJECT 1 — element is a nested unnamed array. The inner 3 reaches C# only as converter cargo.
type nn [2][3]int

// SUBJECT 2 — element is a struct whose zero value needs construction (a fixed-array field).
type wa struct {
	a [3]int
	n int
}
type ns [2]wa

// CONTROL 1 — a plain element: default(byte) is already its Go zero value.
type nb [4]byte

// CONTROL 2 — a NAMED element: its own wrapper allocates its own backing lazily.
type ni [3]int
type no [2]ni

// A struct FIELD of each subject type: C# zeroes it to default(T), which reaches the same backing.
type holder struct {
	f nn
	g ns
}

// Named results take the zero-value prologue rather than a declaration statement.
func namedResult() (r nn, s ns) { return }

func main() {
	// SPELLING 1 — `var`.
	var d nn
	var sv ns
	var b nb
	var o no
	fmt.Println("varNN:", len(d), len(d[0]), d)
	fmt.Println("varNS:", len(sv), len(sv[0].a), sv)
	fmt.Println("varNB:", len(b), b)
	fmt.Println("varNO:", len(o), len(o[0]), o)

	// SPELLING 2 — `new(T)`. This is how runtime's GC test and bcache's cacheTable are built.
	pd := new(nn)
	ps := new(ns)
	pb := new(nb)
	po := new(no)
	fmt.Println("newNN:", len(*pd), len((*pd)[0]), *pd)
	fmt.Println("newNS:", len(*ps), len((*ps)[0].a), *ps)
	fmt.Println("newNB:", len(*pb), *pb)
	fmt.Println("newNO:", len(*po), len((*po)[0]), *po)

	// A struct FIELD's implicit zero value.
	var h holder
	fmt.Println("fieldNN:", len(h.f), len(h.f[0]), h.f)
	fmt.Println("fieldNS:", len(h.g), len(h.g[0].a), h.g)

	// A named RESULT's zero-value prologue.
	rr, ss := namedResult()
	fmt.Println("resNN:", len(rr), len(rr[0]), rr)
	fmt.Println("resNS:", len(ss), len(ss[0].a), ss)

	// An ELEMENT of an array of the named type: the outer two dimensions were already correct at
	// master — the wrapper heals its own length — and only the innermost was wrong. That is the
	// measurement that says all of these funnel through ONE allocation.
	var aa [2]nn
	fmt.Println("arrNN:", len(aa), len(aa[0]), len(aa[0][0]), aa)

	// A slice element and a MISSING map key: two more zero values with no declaration site.
	sl := make([]nn, 2)
	fmt.Println("sliceNN:", len(sl), len(sl[0]), len(sl[0][0]), sl)
	mp := make(map[int]nn)
	mv := mp[7]
	fmt.Println("mapNN:", len(mv), len(mv[0]), mv)

	// The WRITE arm: at master the first indexed write into a zero-valued needy named array
	// panicked with "index out of range [2] with length 0".
	d[1][2] = 9
	sv[1].a[2] = 8
	(*pd)[0][1] = 5
	fmt.Println("writeNN:", d)
	fmt.Println("writeNS:", sv)
	fmt.Println("writePtrNN:", *pd)
}
