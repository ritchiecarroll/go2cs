package main

import (
	"fmt"
	"unsafe"
)

// `mark` is BOTH a package-level type and a method name → the converter records a
// type-vs-method name collision and Δ-renames the package-level identifier (`Δmark`).
// A struct FIELD named `mark` is struct-scoped and is NOT renamed (declared `mark`), so
// its box accessor must be `holder.Ꮡmark` — matching the generated static — NOT the
// collision-renamed `holder.ᏑΔmark` (which would be CS0117). Guards structFieldBoxName.

type mark struct{ id int }

type tagger struct{}

func (tagger) mark() {} // method named `mark` (collides with type `mark`)

type holder struct {
	mark  int
	extra int
}

var h holder

// `gm` is a global whose TYPE is the collision type `mark` itself. Taking the address of one of
// its fields routes through a box accessor whose OWNING type is `mark` — which must use the
// Δ-renamed name (`Ꮡgm.of(Δmark.Ꮡid)`); a raw `mark.Ꮡid` binds to the `mark` method group (CS0119).
var gm mark

// localShadowsCollisionType reproduces runtime malloc `persistentalloc1`'s shape: a box accessor whose
// OWNING type is the collision type `mark` (`&m.id` → `m.of(Δmark.Ꮡid)`), followed by a LOCAL variable
// named after that type (`mark := 7` → renamed `Δmark`). In C# the local is function-scoped, so a bare
// `Δmark.Ꮡid` in the earlier accessor binds to the not-yet-declared local (CS0841). The accessor must
// qualify the owning type with its package static class (`main_package.Δmark.Ꮡid`), which a local can
// never shadow. Guards boxAccessorType's collision-qualification.
//
//go:noinline
func localShadowsCollisionType() int {
	m := &mark{id: 10}
	pid := &m.id // box accessor referencing the collision type `mark`
	*pid = 55
	mark := 7 // local named after the collision type, declared AFTER the accessor
	return *pid + mark // 55 + 7 = 62
}

// `w` is a struct type; locals named after it (`w := &w{…}`) reproduce the runtime's pervasive
// local-named-after-its-own-type idiom (`m := getg().m`).
type w struct {
	park  int
	other int
}

//go:noinline
func run(f func()) { f() }

// capturedLocalNamedAfterType reproduces runtime rwmutex `lockSlow`'s shape: the field-address of a
// type-named local taken INSIDE a closure that captures it. The capture renames the receiver (`wʗ1`),
// but the ENCLOSING local `w` stays visible to the lambda, so a bare owning-type reference `w.Ꮡpark`
// in the accessor binds to that `ж<w>` local (CS1061). The owning type must be package-qualified when
// the receiver is the type-named variable OR its lambda capture (`wʗ1.of(main_package.w.Ꮡpark)`).
// Guards boxAccessorType's capture-suffix collision arm.
//
//go:noinline
func capturedLocalNamedAfterType() int {
	w := &w{park: 30, other: 4}
	got := 0
	run(func() {
		p := &w.park // &captured.field inside the closure — the rwmutex notesleep(&m.park) shape
		*p = *p + 3
		got = *p
	})
	return got + w.other // 33 + 4 = 37
}

// boxRefCapturedValueNamedAfterType: the VALUE-local variant of the shape above. The closure takes
// &w.park, so the local is routed by-box (no snapshot — the heap box `Ꮡw` is the `.of()` receiver),
// and the body writes the variable after the closure runs, so every reference shares the ONE box.
// The owning-type reference in the accessor is package-qualified (`Ꮡw.of(main_package.w.Ꮡpark)`):
// for THIS local (declared type == type name) a bare `w.Ꮡpark` would also compile via C#'s
// identical-simple-name rule, but a box-ref'd local whose declared type DIFFERS from the type name
// (a pointer-typed `w := &w{…}`) has no such fallback — the bare name binds the variable (CS1061) —
// so the box-receiver render qualifies uniformly. Pins boxAccessorType's box-receiver arm via the
// golden; the shared-box semantics (got 105, then 106 after the body write) are the output check.
//
//go:noinline
func boxRefCapturedValueNamedAfterType() int {
	w := w{park: 100, other: 9}
	got := 0
	run(func() {
		p := &w.park
		*p = *p + 5
		got = *p
	})
	w.park = w.park + 1           // a write after the closure — the shared storage must show 106
	return got + w.park + w.other // 105 + 106 + 9 = 220
}

func main() {
	// &global.field routes through the box-field accessor: Ꮡh.of(holder.Ꮡmark).
	p := &h.mark
	*p = 42

	q := &h.extra
	*q = 7

	fmt.Println(h.mark, h.extra) // 42 7

	// Keep the colliding type/method referenced so they are emitted.
	var m mark
	m.id = 3
	var t tagger
	t.mark()
	fmt.Println(m.id) // 3

	// Address of a field of a global whose type IS the collision type `mark`.
	pid := &gm.id
	*pid = 99
	fmt.Println(gm.id) // 99

	// A keyed composite literal whose field name is the collision identifier `mark`: the field
	// name must be emitted unrenamed (`mark:`), matching the struct's declared field — a Δ-rename
	// (`Δmark:`) is not a parameter of the generated constructor (CS1739).
	h2 := holder{mark: 5, extra: 6}
	fmt.Println(h2.mark, h2.extra) // 5 6

	fmt.Println(localShadowsCollisionType()) // 62

	fmt.Println(capturedLocalNamedAfterType()) // 37

	fmt.Println(boxRefCapturedValueNamedAfterType()) // 220

	bm := boxMembers{ReinterpretUnaliased: 4, IsOrderTokenAt: 5}
	got := bumpBoxMembers(&bm)
	fmt.Println(got, bm.ReinterpretUnaliased, bm.IsOrderTokenAt) // 1415 14 15
}

// boxMembers' fields are named like members of golib's pointer box ж<T>: ReinterpretUnaliased (an extension
// method over ж<T>) and IsOrderTokenAt (an instance method). Through a pointer, Go's p.f names the field.
type boxMembers struct {
	ReinterpretUnaliased int
	IsOrderTokenAt       int
	label                string // a managed field: the route serves only a struct the CLR tracks
}

// q is a reinterpret of p, the local shape whose p.f the converter renders through the box (fieldAccessSelector).
//
//go:noinline
func bumpBoxMembers(p *boxMembers) int {
	q := (*boxMembers)(unsafe.Pointer(p))
	q.ReinterpretUnaliased += 10
	q.IsOrderTokenAt *= 3
	return q.ReinterpretUnaliased*100 + q.IsOrderTokenAt
}
