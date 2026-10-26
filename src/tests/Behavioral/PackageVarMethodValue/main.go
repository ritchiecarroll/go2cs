// A pointer-receiver METHOD VALUE on a package-level var binds the VAR's address when it is evaluated:
// `f := g.inc` is `(&g).inc`, so every call through f mutates g itself. go2cs boxes a package var whose
// address is taken (`&g`, or a pointer-receiver call on it), and the method value used to be missing
// from that set: a var nothing else addressed kept a plain static field, `g.inc` emitted `Ꮡ(g).inc` —
// a box of a COPY — and every write through f was lost (Go printed 1, the conversion printed 0). An
// element of a package array failed to compile instead (CS0103, a box name nothing declared).
//
// Each cell owns its var, so no other site in the file boxes it by accident. The controls pin the
// boundary: a value receiver must still COPY, and a pointer-typed var, a slice element and a method
// promoted through an embedded pointer address storage that is already shared.
package main

import "fmt"

type T struct{ n int }

func (p *T) inc() { p.n++ }

func (p *T) add(d int) int {
	p.n += d
	return p.n
}

func (t T) peek() int { return t.n }

type H struct{ t T }

type V struct{ T }

type E struct{ *T }

type Tab struct {
	n    int
	vals [8]int
}

func (p *Tab) inc() { p.n++ }

func mk() T { return T{n: 10} }

func apply(f func()) { f() }

var (
	g   T              // a plain package var
	gh  H              // a value field of a package var
	ga  [2]T           // an element of a package array
	gp  T              // the method value passed as an argument
	gf  T              // the method value taken inside a func literal
	gl  = mk()         // a code-running initializer
	gvv V              // a method promoted through a VALUE embed
	gr  T              // a method value with a parameter and a result
	gw  T              // the var reassigned after the method value is taken
	gv  T              // control: a value receiver copies
	gpp = &T{}         // control: a pointer-typed var
	gs  = make([]T, 1) // control: a slice element
	ge  = E{&T{}}      // control: promoted through an embedded POINTER
)

// lz is lazy-eligible (constant data, at least 20 nodes); the method value boxes it, so it is built on
// first read WITH its box, and the method value must still reach the one storage every read sees.
var lz = Tab{vals: [8]int{1, 2, 3, 4, 5, 6, 7, 8 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1 * 1}}

func main() {
	f := g.inc
	f()
	f()
	fmt.Println("pkgvar  ", g.n)

	fh := gh.t.inc
	fh()
	fmt.Println("field   ", gh.t.n)

	fa := ga[1].inc
	fa()
	fmt.Println("array   ", ga[0].n, ga[1].n)

	apply(gp.inc)
	fmt.Println("argument", gp.n)

	func() {
		ff := gf.inc
		ff()
	}()
	fmt.Println("funclit ", gf.n)

	fl := gl.inc
	fl()
	fmt.Println("initcall", gl.n)

	fz := lz.inc
	fz()
	fmt.Println("lazy    ", lz.n, lz.vals[7])

	fv := gvv.inc
	fv()
	fmt.Println("embedval", gvv.n)

	fr := gr.add
	fmt.Println("result  ", fr(5), fr(2), gr.n)

	fw := gw.inc
	gw = T{n: 100}
	fw()
	fmt.Println("reassign", gw.n)

	// Controls.
	peek := gv.peek
	gv.n = 7
	fmt.Println("valrecv ", peek(), gv.n)

	fp := gpp.inc
	held := gpp
	gpp = &T{n: 50}
	fp()
	fmt.Println("pointer ", held.n, gpp.n)

	fs := gs[0].inc
	fs()
	fmt.Println("slice   ", gs[0].n)

	fe := ge.inc
	fe()
	fmt.Println("embedptr", ge.n)
}
