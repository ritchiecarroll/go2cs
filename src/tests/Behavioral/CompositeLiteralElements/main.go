package main

import "fmt"

// S is an ordinary struct pointee for the elided-& element shape.
type S struct {
	A int
	B string
}

// withArray is a struct whose zero value carries a nested fixed-array field, so
// constructing one has to build the field's inner dimensions too.
type withArray struct {
	A [2][3]int
}

// nb is a NAMED array type: its pointee construction goes through the generated
// wrapper, not through the structural array projection.
type nb [4]byte

// nn is a NAMED type over a NESTED fixed array. Its EMPTY composite literal is the
// type's zero value, and building one has to construct each inner row: the wrapper's
// backing is an array<array<int>> whose element length lives only in the Go type.
type nn [2][3]int

// ns is a NAMED array whose ELEMENT is a struct needing construction — runtime's
// `semTable` shape (`[251]struct{root semaRoot; pad [40]byte}`), where the element's
// fixed-array field initializer runs only inside a declared constructor.
type ns [2]withArray

// ni/no are the THIRD arm of the element predicate, and the one neither `nb` nor `ns`
// reaches: a NAMED array element keeps its own zero-value handling (its wrapper
// allocates a backing of its own known size), so it must be left on the plain
// shortcut — the same exclusion `arrayElemFactory` documents.
type ni [3]int
type no [2]ni

// nsl and nmp are the NAMED SLICE and NAMED MAP flavours of the same shape. Each lowers to a
// different generated wrapper ctor, so each pins its own arm of the named-composite renderer.
type nsl []int
type nmp map[string]int

func main() {
	// --- Defect 1 family: an ELIDED `&T` element whose element type is a pointer.
	// Go permits `[]*T{{}}` as shorthand for `[]*T{&T{}}` for any composite T.

	// pointer-to-ARRAY pointee: the pointee's own length must be constructed.
	pa := []*[4]byte{{}}
	fmt.Println("pa:", len(pa), len(*pa[0]), *pa[0])
	pa[0][1] = 9
	fmt.Printf("pa written: %v\n", *pa[0])

	// the EXPLICIT spelling of the same value, as the control.
	paExplicit := []*[4]byte{&[4]byte{}}
	fmt.Println("paExplicit:", len(paExplicit), len(*paExplicit[0]), *paExplicit[0])

	// a POPULATED elided pointer-to-array element.
	paPop := []*[4]byte{{1, 2, 3, 4}}
	fmt.Println("paPop:", len(*paPop[0]), *paPop[0])

	// a SHORT elided pointer-to-array element.
	paShort := []*[4]byte{{1, 2}}
	fmt.Println("paShort:", len(*paShort[0]), *paShort[0])

	// A pointer-to-NAMED non-struct pointee. Its value is built by the generated WRAPPER ctor,
	// which lives in the TYPED path's named-composite machinery rather than in the structural
	// projection the elided arms emit — so the elided spelling is ROUTED to that renderer rather
	// than a second copy being grown beside it. Each elided row is paired with the explicit
	// spelling it elides and the pair must emit BYTE-IDENTICALLY: elision is surface syntax, so
	// one Go value may not have two emissions. Every elided row here was CS0144 before the
	// routing (`new()` against the abstract `ж<T>`), which is what makes this block the
	// compile-failure control as well as the behavioral one.
	pna := []*nb{{}}
	pnaExplicit := []*nb{&nb{}}
	fmt.Println("pna:", len(*pna[0]), *pna[0])
	fmt.Println("pnaExplicit:", len(*pnaExplicit[0]), *pnaExplicit[0])
	pna[0][2] = 7
	fmt.Printf("pna written: %v\n", *pna[0])

	// The NAMED SLICE and NAMED MAP flavours: a different wrapper ctor each, so each pins its own
	// arm of the renderer instead of re-pinning the array one.
	pnsl := []*nsl{{}}
	pnslExplicit := []*nsl{&nsl{}}
	fmt.Println("pnsl:", len(*pnsl[0]), *pnsl[0] == nil, *pnsl[0])
	fmt.Println("pnslExplicit:", len(*pnslExplicit[0]), *pnslExplicit[0])

	pnmp := []*nmp{{}}
	pnmpExplicit := []*nmp{&nmp{}}
	fmt.Println("pnmp:", len(*pnmp[0]), *pnmp[0] == nil, *pnmp[0])
	fmt.Println("pnmpExplicit:", len(*pnmpExplicit[0]), *pnmpExplicit[0])

	// A POPULATED elided named-array element, so the routing is pinned for a non-empty literal
	// too — the wrapper's element-list ctor rather than its empty shortcut.
	pnaPop := []*nb{{1, 2, 3, 4}}
	fmt.Println("pnaPop:", len(*pnaPop[0]), *pnaPop[0])

	// A named pointee over a NESTED fixed array. This row was deliberately NOT pinned when the
	// elided routing landed, because all three spellings then printed `2 0 [[] []]` against Go's
	// `2 3 [[0 0 0] [0 0 0]]` -- the named wrapper's empty-literal shortcut carried no element
	// factory -- and pinning it would have baked a known-wrong golden. That shortcut now carries
	// the factory, so the row is pinned here: it is the UNION's own property, correct only
	// because the elided spelling is routed to the typed renderer AND that renderer constructs
	// the element. Neither change alone makes this line right.
	pnn := []*nn{{}}
	fmt.Println("pnn:", len(*pnn[0]), len((*pnn[0])[0]), *pnn[0])
	pnnExplicit := []*nn{&nn{}}
	fmt.Println("pnnExplicit:", len(*pnnExplicit[0]), len((*pnnExplicit[0])[0]), *pnnExplicit[0])

	// The named pointee in the MAP-VALUE and fixed-ARRAY container slots, which reach the arm by
	// a different route than the slice-element rows above.
	mpn := map[string]*nb{"a": {}}
	fmt.Println("mpn:", len(mpn), len(*mpn["a"]), *mpn["a"])
	apn := [2]*nb{{}, {}}
	fmt.Println("apn:", len(apn), len(*apn[0]), *apn[0], *apn[1])

	// pointer-to-nested-ARRAY pointee: the inner length must survive too.
	pnest := []*[2][3]int{{}}
	fmt.Println("pnest:", len(*pnest[0]), len(pnest[0][0]), *pnest[0])

	// pointer-to-STRUCT pointee.
	ps := []*S{{}}
	fmt.Println("ps:", len(ps), *ps[0])
	ps2 := []*S{{A: 7, B: "x"}}
	fmt.Println("ps2:", *ps2[0])

	// pointer-to-SLICE pointee.
	psl := []*[]int{{}}
	fmt.Println("psl:", len(psl), len(*psl[0]), *psl[0] == nil, *psl[0])

	// pointer-to-MAP pointee.
	pm := []*map[string]int{{}}
	fmt.Println("pm:", len(pm), len(*pm[0]), *pm[0] == nil, *pm[0])

	// pointer element inside a MAP literal.
	mp := map[string]*[2]int{"a": {}}
	fmt.Println("mp:", len(mp), len(*mp["a"]), *mp["a"])

	// pointer element inside a fixed-size ARRAY literal.
	ap := [2]*[3]int{{}, {}}
	fmt.Println("ap:", len(ap), len(*ap[0]), *ap[0], *ap[1])

	// --- Defect 2 family: a ZERO nested-array element must keep its inner length.

	nested := [][2][3]int{{}}
	fmt.Println("nested:", len(nested), len(nested[0]), len(nested[0][0]), nested[0])
	nested[0][1][2] = 9
	fmt.Printf("nested written: %v\n", nested[0])

	// the populated element, as the control.
	nestedPop := [][2][3]int{{{1, 2, 3}, {4, 5, 6}}}
	fmt.Println("nestedPop:", len(nestedPop[0]), len(nestedPop[0][0]), nestedPop[0])

	// a SHORT nested-array element: the written inner row plus a padded one.
	nestedShort := [][2][3]int{{{1, 2, 3}}}
	fmt.Println("nestedShort:", len(nestedShort[0]), len(nestedShort[0][1]), nestedShort[0])

	// a KEYED (sparse) nested-array element: the unset row still needs its length.
	nestedKeyed := [][2][3]int{{1: {7, 8, 9}}}
	fmt.Println("nestedKeyed:", len(nestedKeyed[0]), len(nestedKeyed[0][0]), nestedKeyed[0])

	// an elided array element whose ELEMENT is a struct needing construction.
	structElem := [][2]withArray{{}}
	fmt.Println("structElem:", len(structElem[0]), len(structElem[0][1].A), len(structElem[0][1].A[0]), structElem[0])

	// a zero nested-array element inside a fixed-size ARRAY literal.
	arrNested := [2][2][3]int{{}}
	fmt.Println("arrNested:", len(arrNested), len(arrNested[0]), len(arrNested[0][0]), len(arrNested[1][0]), arrNested)

	// a zero nested-array element inside a MAP literal.
	mapNested := map[string][2][3]int{"k": {}}
	fmt.Println("mapNested:", len(mapNested["k"]), len(mapNested["k"][0]), mapNested["k"])

	// a struct element whose zero value carries a nested fixed-array field.
	sa := []struct{ A [2][3]int }{{}}
	fmt.Println("sa:", len(sa), len(sa[0].A), len(sa[0].A[0]), sa[0])

	san := []withArray{{}}
	fmt.Println("san:", len(san[0].A), len(san[0].A[0]), san[0])

	// --- Defect 2 family, one level up: a NAMED type over the same shapes. An EMPTY
	// composite of a named array is its ZERO VALUE, and the wrapper's shortcut for that
	// case emitted a plain `new T[N]` projection — sized, but N copies of `default(T)` —
	// so a needy element arrived as unusable storage exactly as the unnamed case did.

	nnLit := nn{}
	fmt.Println("nnLit:", len(nnLit), len(nnLit[0]), nnLit)

	// the same value through the EXPLICIT pointer spelling.
	nnPtr := &nn{}
	fmt.Println("nnPtr:", len(*nnPtr), len((*nnPtr)[0]), *nnPtr)

	// a write into the zero value's inner row must land, which is what "usable storage" means.
	nnWrite := nn{}
	nnWrite[1][2] = 9
	fmt.Printf("nnWrite: %v\n", nnWrite)

	// the NAMED array whose element is a struct needing construction (semTable's shape).
	nsLit := ns{}
	fmt.Println("nsLit:", len(nsLit), len(nsLit[0].A), len(nsLit[0].A[0]), nsLit)

	nsPtr := &ns{}
	fmt.Println("nsPtr:", len(*nsPtr), len((*nsPtr)[0].A), len((*nsPtr)[0].A[0]), *nsPtr)

	// an elided named-array element inside a slice literal — a DIFFERENT renderer that
	// already carried the factory, kept so the two spellings stay pinned together.
	nnElided := []nn{{}}
	fmt.Println("nnElided:", len(nnElided[0]), len(nnElided[0][0]), nnElided[0])

	// --- Named-array controls: the shapes that must NOT change.

	// a PLAIN element keeps the `new T[N]` shortcut — the negative control for the fix.
	nbLit := nb{}
	fmt.Println("nbLit:", len(nbLit), nbLit)

	// a NAMED array element also keeps the shortcut, and is still right: its own wrapper
	// allocates a backing of its own known size. The third arm of the predicate.
	noLit := no{}
	fmt.Println("noLit:", len(noLit), len(noLit[0]), noLit)
	noLit[1][2] = 7
	fmt.Printf("noWrite: %v\n", noLit)

	// a POPULATED named nested literal (the elements are written, nothing is padded).
	nnPop := nn{{1, 2, 3}, {4, 5, 6}}
	fmt.Println("nnPop:", len(nnPop), len(nnPop[0]), nnPop)

	// a SHORT named nested literal: the written row plus a constructed one.
	nnShort := nn{{1, 2, 3}}
	fmt.Println("nnShort:", len(nnShort), len(nnShort[1]), nnShort)

	// a KEYED (sparse) named nested literal: the unset row still needs its length.
	nnKeyed := nn{1: {7, 8, 9}}
	fmt.Println("nnKeyed:", len(nnKeyed), len(nnKeyed[0]), nnKeyed)

	// A NAMED array's ZERO VALUE reached WITHOUT a composite literal — `var x nn`, `new(nn)`,
	// `default(nn)` — is deliberately NOT in this guard. It is the same wrong value from a
	// DIFFERENT layer: the converter routes a named array's declaration to `default!` and the
	// generated wrapper allocates its backing lazily as `new array<E>(N)`, with no element
	// factory and no way to know an inner Go length. That is where every one of the five
	// standard-library named array types with a needy element is actually built
	// (runtime's `var semtable semTable`, `new(cacheTable)`, `new(T)` in runtime's own GC
	// test), so it is worth fixing — but it needs the factory to reach a position that has no
	// template and no type syntax, which is a go2cs-gen/golib change with its own gates.
	// Pinning it here would bake a known-wrong golden.

	// --- Controls: the DECLARED spellings of the same shapes were already correct.

	var ctrl [3][2][2]int
	fmt.Println("ctrl:", len(ctrl), len(ctrl[0]), len(ctrl[0][0]), ctrl)

	ctrlLit := [3][2][2]int{}
	fmt.Println("ctrlLit:", len(ctrlLit), len(ctrlLit[0]), len(ctrlLit[0][0]), ctrlLit)
}
