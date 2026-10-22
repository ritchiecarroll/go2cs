package main

import "fmt"

// A local variable named after a colliding package identifier (a type `p` plus a method `p` on
// another type forces `p` into the type-vs-method collision set) is renamed `Δp`, but when its
// address is taken it is heap-boxed and the box keeps the RAW Go name: `ref var Δp = ref
// heap<box>(out var Ꮡp)`. Taking its address `&p` — e.g. passing it to a function — must reference
// the box by the raw name `Ꮡp`, not `Ꮡ`+the renamed alias (`ᏑΔp`, which is not in scope → CS0103).
// This is the escaping-local analogue of the renamed-receiver/parameter box (RenamedReceiverBox);
// runtime hit it in `gopanic` (`var p _panic; … preprintpanics(&p)` with `p` colliding the type `p`).
// Contrast: a nested-scope SHADOW rename (`i` → `iΔ1`) keeps the SHADOW box name (`ᏑiΔ1`), so only
// the collision form is rewritten to the raw name.

type p struct{ id int }

type tagger struct{}

func (tagger) p() {}

type box struct{ n int }

// Both callees RETAIN the pointer they are given, so neither pointer parameter can be ref-lowered
// (a ref cannot outlive the call): the caller's &p, &p.n and &counter.n must stay real boxes, which is
// the only way the box-naming rules below are reached at all. A callee that only writes through its
// pointer is lowered to a `ref` parameter, and its caller then passes `ref Δp` with no box.
var lastBox *box
var lastInt *int

func setN(b *box, v int) { lastBox = b; b.n = v }

func bump(np *int) { lastInt = np; *np += 100 }

// counter (the GLOBAL var) collides with the method counter below - the var is Δ-renamed
// AND, being addressed, its static box companion is declared with the RENAMED identifier
// (the opposite of a local box, which keeps the raw name): &counter.n must emit
// the renamed box, not the raw one (CS0103 x151, runtime `var sweep sweepdata`).
var counter box

func (tagger) counter() int { return 1 }

// References the colliding type `p` and method `p` in a scope with no local `p` shadow.
func usesTypeP() int {
	var pv p
	pv.id = 1
	var t tagger
	t.p()
	return pv.id
}

func main() {
	p := box{n: 0} // local `p` collides with type `p` -> alias Δp, heap box Ꮡp
	setN(&p, 7)    // &p -> Ꮡp (raw box), NOT ᏑΔp
	setN(&p, p.n+3)
	// census F11: the address of a FIELD of the renamed heap-boxed local must also route
	// through the RAW box - `Ꮡp.of(box.Ꮡn)`, not `ᏑΔp.of(...)` (CS0103 x2, reflect
	// SliceOf/ArrayOf `&slice.Type` on the collision-renamed local `slice`).
	bump(&p.n)
	bump(&counter.n)
	fmt.Println(counter.n)
	fmt.Println(p.n, usesTypeP())    // 110 1
	fmt.Println(lastBox.n, *lastInt) // 110 100: the retained pointers alias p and counter.n
}
