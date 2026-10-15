// Go 1.26's new(expr): new takes an EXPRESSION and allocates a variable initialized to a COPY of its
// value, typed as the expression (its default type when untyped). Before go2cs's H4 support every such
// call read as new(T) and emitted the zero value -- the program compiled and printed zeros.
//
// Each line below guards one emission: an untyped constant of each kind (the pointee is cast so C# cannot
// infer its own literal type), a typed conversion, a call result, struct and array values (Go copies them:
// the writes through the new pointer must not reach the originals), a named-array value, a new(expr)
// converted to a named-array pointer (a wrapper over the COPY, never a zero value), a package-level
// anonymous struct value (lifted under the var's name) and a typed constant, and new(expr) in return,
// argument and field positions.

package main

import "fmt"

type point struct{ x, y int }
type row [3]byte

func answer() int { return 42 }

// Package level: an anonymous struct VALUE through new (visitValueSpec's lift site) and a typed constant.
var cfg = new(struct {
	name string
	n    int
}{"cfg", 3})
var limit = new(int64(1) << 40)

func boxed(v int) *int { return new(v * 2) }

type holder struct{ p *string }

func main() {
	p := new(42)        // untyped constant: *int
	f := new(1.5)       // untyped float: *float64
	s := new("go")      // untyped string: *string
	q := new(int64(7))  // typed conversion
	r := new(answer())  // function result
	pt := point{1, 2}
	pp := new(pt) // struct value: copied
	arr := [3]int{1, 2, 3}
	pa := new(arr) // array value: copied
	rw := row{9, 8, 7}
	pr := new(rw) // named array value: copied
	raw := [3]byte{4, 5, 6}
	pc := (*row)(new(raw)) // conversion of new(expr) to a named-array pointer (convCallExpr's wrapper site)
	*p, pp.x, pa[0], pr[0], pc[0] = 43, 10, 100, 1, 40
	fmt.Println(*p, *f, *s, *q, *r)
	fmt.Println(pt, *pp, arr, *pa, rw, *pr, raw, *pc)
	fmt.Println(cfg.name, cfg.n, *limit, *boxed(21), *holder{p: new("field")}.p)
	fmt.Printf("%T %T %T %T %T\n", p, f, s, pa, pc)
}
